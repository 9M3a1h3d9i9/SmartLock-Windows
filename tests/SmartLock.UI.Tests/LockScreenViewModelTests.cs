using SmartLock.Core.Services;
using SmartLock.UI.ViewModels;

namespace SmartLock.UI.Tests;

public sealed class LockScreenViewModelTests
{
    [Fact]
    public void SubmitAuthentication_RecordsRejectedAttemptAndClearsInput()
    {
        var events = new SecurityEventService();
        var viewModel = new LockScreenViewModel(events)
        {
            CredentialInput = "development-value"
        };

        viewModel.SubmitAuthentication();

        Assert.Empty(viewModel.CredentialInput);
        Assert.Equal("Authentication is not configured in this development build.", viewModel.StatusMessage);
        var securityEvent = Assert.Single(events.Events);
        Assert.Equal("AUTH_ATTEMPT", securityEvent.EventType);
        Assert.Equal("REJECTED", securityEvent.Status);
    }

    [Fact]
    public void CredentialInput_RaisesPropertyChanged()
    {
        var viewModel = new LockScreenViewModel(new SecurityEventService());
        string? changedProperty = null;
        viewModel.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

        viewModel.CredentialInput = "value";

        Assert.Equal(nameof(LockScreenViewModel.CredentialInput), changedProperty);
    }
}
