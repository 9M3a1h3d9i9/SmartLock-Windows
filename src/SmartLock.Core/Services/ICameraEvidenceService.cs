namespace SmartLock.Core.Services;

public interface ICameraEvidenceService
{
    Task<CameraCaptureResult> CaptureFailedAuthenticationAsync(
        string incidentId,
        CancellationToken cancellationToken = default);
}

public sealed record CameraCaptureResult(
    bool Success,
    string? FilePath,
    string? ErrorMessage);
