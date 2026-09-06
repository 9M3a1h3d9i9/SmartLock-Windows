using SmartLock.Core.Models;
using SmartLock.Core.Services;

namespace SmartLock.Core.Tests;

public sealed class AuthenticationIncidentEngineTests
{
    [Fact]
    public void FailedAttempts_TriggerLockoutAtConfiguredThreshold()
    {
        var events = new SecurityEventService();
        var engine = new AuthenticationIncidentEngine(events, 3, TimeSpan.FromMinutes(1));

        engine.RegisterFailedAttempt();
        engine.RegisterFailedAttempt();
        var result = engine.RegisterFailedAttempt();

        Assert.False(result.Accepted);
        Assert.True(result.LockedOut);
        Assert.True(engine.State.IsLocked);
        Assert.Equal(0, engine.State.RemainingAttempts);
        Assert.Contains(events.Events, e => e.EventType == SecurityEventType.Lockout);
    }

    [Fact]
    public void FailedAttempt_IsBlockedWhileLockedOut()
    {
        var events = new SecurityEventService();
        var engine = new AuthenticationIncidentEngine(events, 1, TimeSpan.FromMinutes(1));

        engine.RegisterFailedAttempt();
        var blocked = engine.RegisterFailedAttempt();

        Assert.True(blocked.LockedOut);
        Assert.Equal(1, engine.State.FailedAttempts);
        Assert.Contains(events.Events, e => e.Message.Contains("blocked", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AcceptedAttempt_ClearsFailureCounterAndLockout()
    {
        var events = new SecurityEventService();
        var engine = new AuthenticationIncidentEngine(events, 1, TimeSpan.FromMinutes(1));

        engine.RegisterFailedAttempt();
        engine.ResetLockout();
        engine.RegisterAcceptedAttempt();

        Assert.False(engine.State.IsLocked);
        Assert.Equal(0, engine.State.FailedAttempts);
        Assert.Contains(events.Events, e => e.Status == SecurityEventStatus.Accepted);
    }
}
