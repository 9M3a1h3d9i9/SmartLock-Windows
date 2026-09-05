namespace SmartLock.Core.Models;

public sealed record SessionContext(
    DateTimeOffset ObservedAt,
    TimeSpan IdleDuration,
    SessionState State);
