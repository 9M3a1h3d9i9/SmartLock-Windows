namespace SmartLock.Core.Models;

public enum SecurityEventType
{
    AuthenticationAttempt,
    Lockout,
    SessionStarted,
    SessionEnded,
    PolicyViolation
}
