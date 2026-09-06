using SmartLock.Core.Models;

namespace SmartLock.Core.Services;

public sealed class SecurityEventService : ISecurityEventService
{
    private readonly List<SecurityEvent> _events = [];
    private readonly object _sync = new();

    public SecurityEventService(IEnumerable<SecurityEvent>? initialEvents = null)
    {
        if (initialEvents is null)
        {
            return;
        }

        _events.AddRange(initialEvents);
    }

    public IReadOnlyList<SecurityEvent> Events
    {
        get
        {
            lock (_sync)
            {
                return _events.ToArray();
            }
        }
    }

    public SecurityEvent Record(
        SecurityEventType eventType,
        SecuritySeverity severity,
        SecurityEventStatus status,
        string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var securityEvent = new SecurityEvent(
            DateTimeOffset.UtcNow,
            eventType,
            severity,
            status,
            message.Trim(),
            $"SL-{Guid.NewGuid():N}");

        lock (_sync)
        {
            _events.Add(securityEvent);
        }

        return securityEvent;
    }
}
