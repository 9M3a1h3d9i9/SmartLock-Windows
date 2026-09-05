using SmartLock.Core.Models;

namespace SmartLock.Core.Services;

public interface ISecurityEventService
{
    SecurityEvent Record(
        SecurityEventType eventType,
        SecuritySeverity severity,
        SecurityEventStatus status,
        string message);

    IReadOnlyList<SecurityEvent> Events { get; }
}
