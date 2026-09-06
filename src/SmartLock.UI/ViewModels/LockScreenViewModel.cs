using System.ComponentModel;
using System.Runtime.CompilerServices;
using SmartLock.Core.Models;
using SmartLock.Core.Services;

namespace SmartLock.UI.ViewModels;

public sealed class LockScreenViewModel : INotifyPropertyChanged
{
    private readonly ISecurityEventService _securityEvents;
    private readonly ICameraEvidenceService _cameraEvidence;
    private string _statusMessage = "Ready";
    private bool _cameraEvidenceEnabled;
    private bool _isProcessing;

    public LockScreenViewModel(
        ISecurityEventService securityEvents,
        ICameraEvidenceService cameraEvidence)
    {
        ArgumentNullException.ThrowIfNull(securityEvents);
        ArgumentNullException.ThrowIfNull(cameraEvidence);

        _securityEvents = securityEvents;
        _cameraEvidence = cameraEvidence;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
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

    public async Task SubmitAuthenticationAsync()
    {
        if (IsProcessing)
        {
            return;
        }

        IsProcessing = true;
        try
        {
            var securityEvent = _securityEvents.Record(
                SecurityEventType.AuthenticationAttempt,
                SecuritySeverity.Warning,
                SecurityEventStatus.Rejected,
                "Authentication attempt rejected in development mode.");

            StatusMessage = "Authentication failed.";

            if (!CameraEvidenceEnabled)
            {
                StatusMessage += " Camera evidence is disabled.";
                return;
            }

            StatusMessage = "Authentication failed. Capturing security evidence...";
            var capture = await _cameraEvidence.CaptureFailedAuthenticationAsync(securityEvent.IncidentId);

            if (capture.Success)
            {
                _securityEvents.Record(
                    SecurityEventType.PolicyViolation,
                    SecuritySeverity.High,
                    SecurityEventStatus.Observed,
                    $"Camera evidence captured for failed authentication: {capture.FilePath}");

                StatusMessage = $"Authentication failed. Security photo captured locally.\n{capture.FilePath}";
            }
            else
            {
                _securityEvents.Record(
                    SecurityEventType.PolicyViolation,
                    SecuritySeverity.Warning,
                    SecurityEventStatus.Observed,
                    $"Camera evidence capture failed: {capture.ErrorMessage}");

                StatusMessage = $"Authentication failed. Camera capture failed.\n{capture.ErrorMessage}";
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
