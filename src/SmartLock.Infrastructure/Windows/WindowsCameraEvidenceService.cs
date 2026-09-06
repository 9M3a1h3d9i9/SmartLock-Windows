using OpenCvSharp;
using SmartLock.Core.Services;

namespace SmartLock.Infrastructure.Windows;

public sealed class WindowsCameraEvidenceService : ICameraEvidenceService
{
    private readonly string _evidenceDirectory;
    private readonly int _cameraIndex;
    private readonly int _retentionLimit;

    public WindowsCameraEvidenceService(
        string? evidenceDirectory = null,
        int cameraIndex = 0,
        int retentionLimit = 100)
    {
        if (cameraIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cameraIndex));
        }

        if (retentionLimit < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(retentionLimit));
        }

        _cameraIndex = cameraIndex;
        _retentionLimit = retentionLimit;
        _evidenceDirectory = evidenceDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SmartLock",
            "SecurityEvidence");
    }

    public Task<CameraCaptureResult> CaptureFailedAuthenticationAsync(
        string incidentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(incidentId);

        return Task.Run(() => Capture(incidentId), cancellationToken);
    }

    private CameraCaptureResult Capture(string incidentId)
    {
        try
        {
            Directory.CreateDirectory(_evidenceDirectory);
            CleanupOldEvidence();

            using var camera = new VideoCapture(_cameraIndex, VideoCaptureAPIs.DSHOW);
            if (!camera.IsOpened())
            {
                return new(false, null, "The selected camera could not be opened. Check Windows camera privacy settings and that no other application is using it.");
            }

            camera.Set(VideoCaptureProperties.FrameWidth, 1280);
            camera.Set(VideoCaptureProperties.FrameHeight, 720);

            using var frame = new Mat();
            if (!camera.Read(frame) || frame.Empty())
            {
                return new(false, null, "The camera opened but did not return a frame.");
            }

            var safeIncidentId = string.Concat(incidentId.Where(char.IsLetterOrDigit));
            var fileName = $"failed-auth-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss-fff}-{safeIncidentId}.jpg";
            var filePath = Path.Combine(_evidenceDirectory, fileName);

            Cv2.ImWrite(filePath, frame, [new ImageEncodingParam(ImwriteFlags.JpegQuality, 90)]);
            return File.Exists(filePath)
                ? new(true, filePath, null)
                : new(false, null, "The camera frame was captured but could not be written to local storage.");
        }
        catch (Exception ex) when (ex is OpenCVException or IOException or UnauthorizedAccessException)
        {
            return new(false, null, $"Camera capture failed: {ex.Message}");
        }
    }

    private void CleanupOldEvidence()
    {
        var files = new DirectoryInfo(_evidenceDirectory)
            .EnumerateFiles("failed-auth-*.jpg")
            .OrderByDescending(file => file.CreationTimeUtc)
            .Skip(_retentionLimit);

        foreach (var file in files)
        {
            try
            {
                file.Delete();
            }
            catch (IOException)
            {
                // A stale evidence file must not prevent a new security event from being recorded.
            }
            catch (UnauthorizedAccessException)
            {
                // Same principle for files that are temporarily inaccessible.
            }
        }
    }
}
