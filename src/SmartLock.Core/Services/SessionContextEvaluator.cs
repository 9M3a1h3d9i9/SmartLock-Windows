using SmartLock.Core.Models;

namespace SmartLock.Core.Services;

public sealed class SessionContextEvaluator
{
    public SessionContext Evaluate(DateTimeOffset observedAt, TimeSpan idleDuration, bool isLocked)
    {
        if (idleDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(idleDuration));
        }

        var state = isLocked
            ? SessionState.Locked
            : idleDuration > TimeSpan.Zero
                ? SessionState.Idle
                : SessionState.Active;

        return new SessionContext(observedAt, idleDuration, state);
    }
}
