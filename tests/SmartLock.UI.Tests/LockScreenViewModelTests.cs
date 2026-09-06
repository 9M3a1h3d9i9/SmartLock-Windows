using SmartLock.Core.Models;
using SmartLock.Core.Services;
using SmartLock.UI.ViewModels;

namespace SmartLock.UI.Tests;

public sealed class LockScreenViewModelTests
{
    [Fact]
    public async Task SubmitAuthentication_RecordsRejectedAttemptAndUpdatesStatus()
    {
        var events = new SecurityEventService();
        var viewModel = new LockScreenViewModel(events, new FakeCameraEvidenceService());

        await viewModel.SubmitAuthenticationAsync();

        Assert.Equal("Authentication failed. Camera evidence is disabled.", viewModel.StatusMessage);
        var securityEvent = Assert.Single(events.Events);
        Assert.Equal(SecurityEventType.AuthenticationAttempt, securityEvent.EventType);
        Assert.Equal(SecuritySeverity.Warning, securityEvent.Severity);
        Assert.Equal(SecurityEventStatus.Rejected, securityEvent.Status);
    }

    [Fact]
    public async Task SubmitAuthentication_WhenCameraEvidenceEnabled_CapturesAndRecordsEvidence()
    {
        var events = new SecurityEventService();
        var camera = new FakeCameraEvidenceService(new(true, "evidence.jpg", null));
        var viewModel = new LockScreenViewModel(events, camera)
        {
            CameraEvidenceEnabled = true
        };

        await viewModel.SubmitAuthenticationAsync();

        Assert.Contains("Security photo captured locally", viewModel.StatusMessage);
        Assert.Equal(2, events.Events.Count);
        Assert.Equal(SecurityEventType.PolicyViolation, events.Events[1].EventType);
        Assert.Equal(SecuritySeverity.High, events.Events[1].Severity);
    }

    [Fact]
    public async Task StatusMessage_RaisesPropertyChanged()
    {
        var viewModel = new LockScreenViewModel(new SecurityEventService(), new FakeCameraEvidenceService());
        string? changedProperty = null;
        viewModel.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

        await viewModel.SubmitAuthenticationAsync();

        Assert.Equal(nameof(LockScreenViewModel.StatusMessage), changedProperty);
    }

    private sealed class FakeCameraEvidenceService : ICameraEvidenceService
    {
        private readonly CameraCaptureResult _result;

        public FakeCameraEvidenceService(CameraCaptureResult? result = null)
        {
            _result = result ?? new(false, null, "disabled");
        }

        public Task<CameraCaptureResult> CaptureFailedAuthenticationAsync(
            string incidentId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_result);
        }
    }
}
