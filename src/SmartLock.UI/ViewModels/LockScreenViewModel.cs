using System.ComponentModel;
using System.Runtime.CompilerServices;
using SmartLock.Core.Services;

namespace SmartLock.UI.ViewModels;

public sealed class LockScreenViewModel : INotifyPropertyChanged
{
    private readonly ISecurityEventService _securityEvents;
    private string _credentialInput = string.Empty;
    private string _statusMessage = "Ready";

    public LockScreenViewModel(ISecurityEventService securityEvents)
    {
        ArgumentNullException.ThrowIfNull(securityEvents);
        _securityEvents = securityEvents;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CredentialInput
    {
        get => _credentialInput;
        set => SetField(ref _credentialInput, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
    }

    public void SubmitAuthentication()
    {
        _securityEvents.Record(
            "AUTH_ATTEMPT",
            "REJECTED",
            "Authentication is not configured in this development build.");

        StatusMessage = "Authentication is not configured in this development build.";
        CredentialInput = string.Empty;
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
