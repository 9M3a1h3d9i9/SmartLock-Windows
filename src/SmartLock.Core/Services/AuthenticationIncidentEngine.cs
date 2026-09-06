using SmartLock.Core.Models;

namespace SmartLock.Core.Services;

public sealed record AuthenticationIncidentResult(
    bool Accepted,
    bool LockedOut,
    SecurityEvent Event,
    LockoutState State);

public sealed class AuthenticationIncidentEngine
{
    private readonly ISecurityEventService _events;
    private readonly int _maxFailedAttempts;
    private readonly TimeSpan _lockoutDuration;
    private readonly object _sync = new();
    private int _failedAttempts;
    private DateTimeOffset? _lockedUntil;

    public AuthenticationIncidentEngine(
        ISecurityEventService events,
        int maxFailedAttempts = 5,
        TimeSpan? lockoutDuration = null)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxFailedAttempts, 1);
        if (lockoutDuration is { } duration && duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lockoutDuration));
        }

        _events = events;
        _maxFailedAttempts = maxFailedAttempts;
        _lockoutDuration = lockoutDuration ?? TimeSpan.FromSeconds(30);
    }

    public LockoutState State
    {
        get
        {
            lock (_sync)
            {
                RefreshLockout();
                return CreateState();
            }
        }
    }

    public AuthenticationIncidentResult RegisterFailedAttempt(string reason = "Authentication attempt rejected.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        lock (_sync)
        {
            RefreshLockout();
            if (_lockedUntil is { } until && until > DateTimeOffset.UtcNow)
            {
                var blocked = _events.Record(
                    SecurityEventType.Lockout,
                    SecuritySeverity.High,
                    SecurityEventStatus.Rejected,
                    "Authentication attempt blocked because the application is locked out.");
                return new(false, true, blocked, CreateState());
            }

            _failedAttempts++;
            var eventType = SecurityEventType.AuthenticationAttempt;
            var severity = _failedAttempts >= _maxFailedAttempts
                ? SecuritySeverity.High
                : SecuritySeverity.Warning;

            var securityEvent = _events.Record(
                eventType,
                severity,
                SecurityEventStatus.Rejected,
                $"{reason.Trim()} Failed attempts: {_failedAttempts}/{_maxFailedAttempts}.");

            if (_failedAttempts >= _maxFailedAttempts)
            {
                _lockedUntil = DateTimeOffset.UtcNow.Add(_lockoutDuration);
                _events.Record(
                    SecurityEventType.Lockout,
                    SecuritySeverity.High,
                    SecurityEventStatus.Observed,
                    $"Application lockout activated for {_lockoutDuration.TotalSeconds:0} seconds.");
            }

            return new(false, _lockedUntil is not null, securityEvent, CreateState());
        }
    }

    public void RegisterAcceptedAttempt()
    {
        lock (_sync)
        {
            RefreshLockout();
            _failedAttempts = 0;
            _lockedUntil = null;
            _events.Record(
                SecurityEventType.AuthenticationAttempt,
                SecuritySeverity.Info,
                SecurityEventStatus.Accepted,
                "Authentication accepted; failed-attempt counter reset.");
        }
    }

    public void ResetLockout()
    {
        lock (_sync)
        {
            _failedAttempts = 0;
            _lockedUntil = null;
            _events.Record(
                SecurityEventType.Lockout,
                SecuritySeverity.Info,
                SecurityEventStatus.Resolved,
                "Application lockout cleared by an authorized local action.");
        }
    }

    private void RefreshLockout()
    {
        if (_lockedUntil is { } until && until <= DateTimeOffset.UtcNow)
        {
            _lockedUntil = null;
            _failedAttempts = 0;
            _events.Record(
                SecurityEventType.Lockout,
                SecuritySeverity.Info,
                SecurityEventStatus.Resolved,
                "Application lockout expired; authentication attempts are available again.");
        }
    }

    private LockoutState CreateState() => new(_failedAttempts, _maxFailedAttempts, _lockedUntil);
}
