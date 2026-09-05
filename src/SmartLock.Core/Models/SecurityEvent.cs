namespace SmartLock.Core.Models;

public sealed record SecurityEvent(
    DateTimeOffset Timestamp,
    SecurityEventType EventType,
    SecuritySeverity Severity,
    SecurityEventStatus Status,
    string Message,
    string IncidentId);
