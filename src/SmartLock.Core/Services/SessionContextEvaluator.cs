using SmartLock.Core.Models;

namespace SmartLock.Core.Services;

public sealed class SessionContextEvaluator
{
    public static SessionContext Evaluate(DateTimeOffset observedAt, TimeSpan idleDuration, bool isLocked)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(idleDuration, TimeSpan.Zero);

        var state = isLocked
            ? SessionState.Locked
            : idleDuration > TimeSpan.Zero
                ? SessionState.Idle
                : SessionState.Active;

        return new SessionContext(observedAt, idleDuration, state);
    }
}
