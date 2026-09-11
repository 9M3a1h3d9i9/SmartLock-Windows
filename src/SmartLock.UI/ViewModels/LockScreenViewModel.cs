using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SmartLock.Core.Models;
using SmartLock.Core.Services;

namespace SmartLock.UI.ViewModels;

public sealed class LockScreenViewModel : INotifyPropertyChanged
{
    private readonly ISecurityEventService _securityEvents;
    private readonly ICameraEvidenceService _cameraEvidence;
    private readonly AuthenticationIncidentEngine _incidentEngine;
    private readonly IWorkstationLockService? _workstationLock;
    private readonly ITelegramAlertService? _telegram;
    private readonly IAdminOtpService? _adminOtp;
    private readonly IWindowsSecurityAlertService? _windowsAlert;
    private readonly IIncomingCallService? _incomingCall;
    private string _statusMessage = "Ready";
    private bool _cameraEvidenceEnabled;
    private bool _lockWindowsSessionOnPolicyLockout;
    private bool _telegramAlertsEnabled;
    private bool _windowsSecurityAlertsEnabled;
    private bool _incomingCallEnabled;
    private bool _isProcessing;
    private string _adminOtpCode = string.Empty;
    private string _lockoutMessage = string.Empty;

    public LockScreenViewModel(
        ISecurityEventService securityEvents,
        ICameraEvidenceService cameraEvidence,
        AuthenticationIncidentEngine incidentEngine,
        IWorkstationLockService? workstationLock = null,
        ITelegramAlertService? telegram = null,
        IAdminOtpService? adminOtp = null,
        IWindowsSecurityAlertService? windowsAlert = null,
        IIncomingCallService? incomingCall = null)
    {
        ArgumentNullException.ThrowIfNull(securityEvents);
        ArgumentNullException.ThrowIfNull(cameraEvidence);
        ArgumentNullException.ThrowIfNull(incidentEngine);

        _securityEvents = securityEvents;
        _cameraEvidence = cameraEvidence;
        _incidentEngine = incidentEngine;
        _workstationLock = workstationLock;
        _telegram = telegram;
        _adminOtp = adminOtp;
        _windowsAlert = windowsAlert;
        _incomingCall = incomingCall;
        RefreshSecurityState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<SecurityEvent> IncidentTimeline { get; } = [];

    public string StatusMessage { get => _statusMessage; private set => SetField(ref _statusMessage, value); }
    public string LockoutMessage { get => _lockoutMessage; private set => SetField(ref _lockoutMessage, value); }

    public bool CameraEvidenceEnabled { get => _cameraEvidenceEnabled; set => SetField(ref _cameraEvidenceEnabled, value); }
    public bool LockWindowsSessionOnPolicyLockout { get => _lockWindowsSessionOnPolicyLockout; set => SetField(ref _lockWindowsSessionOnPolicyLockout, value); }
    public bool TelegramAlertsEnabled { get => _telegramAlertsEnabled; set => SetField(ref _telegramAlertsEnabled, value); }
    public bool WindowsSecurityAlertsEnabled { get => _windowsSecurityAlertsEnabled; set => SetField(ref _windowsSecurityAlertsEnabled, value); }
    public bool IncomingCallEnabled { get => _incomingCallEnabled; set => SetField(ref _incomingCallEnabled, value); }
    public string AdminOtpCode { get => _adminOtpCode; set => SetField(ref _adminOtpCode, value); }

    public bool IsProcessing
    {
        get => _isProcessing;
        private set
        {
            if (!SetField(ref _isProcessing, value)) return;
            OnPropertyChanged(nameof(CanSubmit));
        }
    }

    public bool IsLockedOut => _incidentEngine.State.IsLocked;
    public bool CanSubmit => !IsProcessing && !IsLockedOut;
    public int RemainingAttempts => _incidentEngine.State.RemainingAttempts;

    public async Task SubmitAuthenticationAsync()
    {
        if (!CanSubmit)
        {
            RefreshSecurityState();
            return;
        }

        IsProcessing = true;
        try
        {
            var result = _incidentEngine.RegisterFailedAttempt("Authentication attempt rejected in development mode.");
            StatusMessage = $"Authentication failed. {result.State.FailedAttempts}/{result.State.MaxFailedAttempts} failed attempts.";
            string? photoPath = null;

            if (CameraEvidenceEnabled)
            {
                StatusMessage = "Authentication failed. Capturing security evidence...";
                var capture = await _cameraEvidence.CaptureFailedAuthenticationAsync(result.Event.IncidentId);
                photoPath = capture.Success ? capture.FilePath : null;
                _securityEvents.Record(
                    SecurityEventType.PolicyViolation,
                    capture.Success ? SecuritySeverity.High : SecuritySeverity.Warning,
                    SecurityEventStatus.Observed,
                    capture.Success
                        ? $"Camera evidence captured for failed authentication: {capture.FilePath}"
                        : $"Camera evidence capture failed: {capture.ErrorMessage}");
            }

            if (result.LockedOut)
            {
                StatusMessage = "Security lockout activated.";

                if (LockWindowsSessionOnPolicyLockout && _workstationLock is not null)
                {
                    var locked = _workstationLock.TryLock();
                    _securityEvents.Record(
                        SecurityEventType.PolicyViolation,
                        locked ? SecuritySeverity.High : SecuritySeverity.Warning,
                        SecurityEventStatus.Observed,
                        locked ? "Windows workstation locked after application policy lockout." : "Windows workstation lock request failed after application policy lockout.");
                }

                if (WindowsSecurityAlertsEnabled && _windowsAlert is not null)
                {
                    _windowsAlert.Show("SmartLock Security Alert", "Multiple failed authentication attempts triggered an application security lockout.");
                }

                if (IncomingCallEnabled && _incomingCall is not null)
                {
                    await _incomingCall.TriggerAsync("SmartLock Security Admin");
                }
            }

            if (TelegramAlertsEnabled && _telegram is not null)
            {
                var telegramMessage = result.LockedOut
                    ? $"SmartLock security incident\nIncident: {result.Event.IncidentId}\nStatus: application lockout activated\nFailed attempts: {result.State.FailedAttempts}/{result.State.MaxFailedAttempts}"
                    : $"SmartLock failed authentication\nIncident: {result.Event.IncidentId}\nFailed attempts: {result.State.FailedAttempts}/{result.State.MaxFailedAttempts}";
                var sent = await _telegram.SendIncidentAsync(telegramMessage, photoPath);
                _securityEvents.Record(
                    SecurityEventType.PolicyViolation,
                    sent ? SecuritySeverity.Info : SecuritySeverity.Warning,
                    SecurityEventStatus.Observed,
                    sent ? "Telegram security notification sent." : "Telegram security notification could not be sent.");
            }

            if (photoPath is not null)
            {
                StatusMessage = TelegramAlertsEnabled ? "Security photo captured locally and notification processed." : "Security photo captured locally.";
            }
        }
        finally
        {
            IsProcessing = false;
            RefreshSecurityState();
        }
    }

    public bool TryAdminOverride()
    {
        if (_adminOtp is null || !_adminOtp.Validate(AdminOtpCode))
        {
            StatusMessage = "Admin OTP rejected.";
            _securityEvents.Record(SecurityEventType.PolicyViolation, SecuritySeverity.Warning, SecurityEventStatus.Rejected, "Administrator OTP validation failed.");
            return false;
        }

        _incidentEngine.ResetLockout();
        AdminOtpCode = string.Empty;
        StatusMessage = "Administrator OTP accepted. Security lockout cleared.";
        RefreshSecurityState();
        return true;
    }

    public void RefreshSecurityState()
    {
        var state = _incidentEngine.State;
        LockoutMessage = state.IsLocked
            ? $"LOCKED OUT • {state.RemainingLockout!.Value.TotalSeconds:0} seconds remaining"
            : $"Security policy • {state.RemainingAttempts} attempt(s) remaining";

        IncidentTimeline.Clear();
        foreach (var securityEvent in _securityEvents.Events.OrderByDescending(e => e.Timestamp).Take(20))
            IncidentTimeline.Add(securityEvent);

        OnPropertyChanged(nameof(IsLockedOut));
        OnPropertyChanged(nameof(RemainingAttempts));
        OnPropertyChanged(nameof(CanSubmit));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
