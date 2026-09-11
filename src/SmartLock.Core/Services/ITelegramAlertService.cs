namespace SmartLock.Core.Services;

public interface ITelegramAlertService
{
    Task<bool> SendIncidentAsync(
        string message,
        string? photoPath = null,
        CancellationToken cancellationToken = default);
}
