using System.ComponentModel;
using System.Runtime.CompilerServices;
using SmartLock.Core.Models;
using SmartLock.Core.Services;

namespace SmartLock.UI.ViewModels;

public sealed class LockScreenViewModel : INotifyPropertyChanged
{
    private readonly ISecurityEventService _securityEvents;
    private string _statusMessage = "Ready";

    public LockScreenViewModel(ISecurityEventService securityEvents)
    {
        ArgumentNullException.ThrowIfNull(securityEvents);
        _securityEvents = securityEvents;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
    }

    public void SubmitAuthentication()
    {
        _securityEvents.Record(
            SecurityEventType.AuthenticationAttempt,
            SecuritySeverity.Warning,
            SecurityEventStatus.Rejected,
            "Authentication is not configured in this development build.");

        StatusMessage = "Authentication is not configured in this development build.";
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
