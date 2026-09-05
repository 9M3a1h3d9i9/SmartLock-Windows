namespace SmartLock.Core.Models;

public sealed record SecurityEvent(
    DateTimeOffset Timestamp,
    string EventType,
    string Status,
    string Message,
    string IncidentId);
