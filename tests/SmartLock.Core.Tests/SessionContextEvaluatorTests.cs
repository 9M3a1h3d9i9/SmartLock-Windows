using SmartLock.Core.Models;
using SmartLock.Core.Services;

namespace SmartLock.Core.Tests;

public sealed class SessionContextEvaluatorTests
{
    private readonly SessionContextEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_WithNoIdleTime_ReturnsActive()
    {
        var observedAt = DateTimeOffset.UtcNow;

        var context = _evaluator.Evaluate(observedAt, TimeSpan.Zero, false);

        Assert.Equal(SessionState.Active, context.State);
        Assert.Equal(observedAt, context.ObservedAt);
        Assert.Equal(TimeSpan.Zero, context.IdleDuration);
    }

    [Fact]
    public void Evaluate_WithIdleTime_ReturnsIdle()
    {
        var context = _evaluator.Evaluate(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5), false);

        Assert.Equal(SessionState.Idle, context.State);
    }

    [Fact]
    public void Evaluate_WhenLocked_ReturnsLocked()
    {
        var context = _evaluator.Evaluate(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5), true);

        Assert.Equal(SessionState.Locked, context.State);
    }

    [Fact]
    public void Evaluate_WithNegativeIdleTime_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _evaluator.Evaluate(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(-1), false));
    }
}
