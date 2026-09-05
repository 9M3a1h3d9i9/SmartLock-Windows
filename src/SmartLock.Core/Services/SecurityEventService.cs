namespace SmartLock.Core.Services;

public sealed class SecurityEventService : ISecurityEventService
{
    private readonly List<Models.SecurityEvent> _events = [];

    public IReadOnlyList<Models.SecurityEvent> Events => _events.AsReadOnly();

    public void Record(string eventType, string status, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        _events.Add(new Models.SecurityEvent(
            DateTimeOffset.UtcNow,
            eventType.Trim(),
            status.Trim(),
            message.Trim(),
            $"SL-{Guid.NewGuid():N}"));
    }
}
