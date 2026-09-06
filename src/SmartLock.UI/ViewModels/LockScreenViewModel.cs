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
    private string _statusMessage = "Ready";
    private bool _cameraEvidenceEnabled;
    private bool _isProcessing;
    private string _lockoutMessage = string.Empty;

    public LockScreenViewModel(
        ISecurityEventService securityEvents,
        ICameraEvidenceService cameraEvidence,
        AuthenticationIncidentEngine incidentEngine)
    {
        ArgumentNullException.ThrowIfNull(securityEvents);
        ArgumentNullException.ThrowIfNull(cameraEvidence);
        ArgumentNullException.ThrowIfNull(incidentEngine);

        _securityEvents = securityEvents;
        _cameraEvidence = cameraEvidence;
        _incidentEngine = incidentEngine;
        RefreshSecurityState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<SecurityEvent> IncidentTimeline { get; } = [];

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
    }

    public string LockoutMessage
    {
        get => _lockoutMessage;
        private set => SetField(ref _lockoutMessage, value);
    }

    public bool CameraEvidenceEnabled
    {
        get => _cameraEvidenceEnabled;
        set => SetField(ref _cameraEvidenceEnabled, value);
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        private set => SetField(ref _isProcessing, value);
    }

    public bool IsLockedOut => _incidentEngine.State.IsLocked;

    public int RemainingAttempts => _incidentEngine.State.RemainingAttempts;

    public async Task SubmitAuthenticationAsync()
    {
        if (IsProcessing || IsLockedOut)
        {
            RefreshSecurityState();
            return;
        }

        IsProcessing = true;
        try
        {
            // Development authentication deliberately rejects the attempt.
            // Windows credentials are never read, stored, or intercepted.
            var result = _incidentEngine.RegisterFailedAttempt("Authentication attempt rejected in development mode.");
            StatusMessage = $"Authentication failed. {result.State.FailedAttempts}/{result.State.MaxFailedAttempts} failed attempts.";

            if (result.LockedOut)
            {
                StatusMessage = "Security lockout activated. Try again after the lockout expires.";
            }

            if (CameraEvidenceEnabled)
            {
                StatusMessage = "Authentication failed. Capturing security evidence...";
                var capture = await _cameraEvidence.CaptureFailedAuthenticationAsync(result.Event.IncidentId);

                if (capture.Success)
                {
                    _securityEvents.Record(
                        SecurityEventType.PolicyViolation,
                        SecuritySeverity.High,
                        SecurityEventStatus.Observed,
                        $"Camera evidence captured for failed authentication: {capture.FilePath}");
                    StatusMessage = "Authentication failed. Security photo captured locally.";
                }
                else
                {
                    _securityEvents.Record(
                        SecurityEventType.PolicyViolation,
                        SecuritySeverity.Warning,
                        SecurityEventStatus.Observed,
                        $"Camera evidence capture failed: {capture.ErrorMessage}");
                    StatusMessage = $"Authentication failed. Camera capture failed: {capture.ErrorMessage}";
                }
            }
        }
        finally
        {
            IsProcessing = false;
            RefreshSecurityState();
        }
    }

    public void RefreshSecurityState()
    {
        var state = _incidentEngine.State;
        LockoutMessage = state.IsLocked
            ? $"LOCKED OUT • {state.RemainingLockout!.Value.TotalSeconds:0} seconds remaining"
            : $"Security policy • {state.RemainingAttempts} attempt(s) remaining";

        IncidentTimeline.Clear();
        foreach (var securityEvent in _securityEvents.Events.OrderByDescending(e => e.Timestamp).Take(20))
        {
            IncidentTimeline.Add(securityEvent);
        }

        OnPropertyChanged(nameof(IsLockedOut));
        OnPropertyChanged(nameof(RemainingAttempts));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
