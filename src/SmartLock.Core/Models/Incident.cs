namespace SmartLock.Core.Models;

public sealed record Incident(
    string IncidentId,
    DateTimeOffset OpenedAt,
    SecuritySeverity Severity,
    SecurityEventType TriggerEvent,
    string Summary,
    bool IsResolved);
