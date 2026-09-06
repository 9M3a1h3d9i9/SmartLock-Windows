namespace SmartLock.Core.Models;

public sealed record LockoutState(
    int FailedAttempts,
    int MaxFailedAttempts,
    DateTimeOffset? LockedUntil)
{
    public bool IsLocked => LockedUntil is { } until && until > DateTimeOffset.UtcNow;
    public int RemainingAttempts => Math.Max(0, MaxFailedAttempts - FailedAttempts);
    public TimeSpan? RemainingLockout => IsLocked ? LockedUntil - DateTimeOffset.UtcNow : null;
}
