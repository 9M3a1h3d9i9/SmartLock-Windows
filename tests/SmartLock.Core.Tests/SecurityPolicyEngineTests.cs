using SmartLock.Core.Models;
using SmartLock.Core.Services;

namespace SmartLock.Core.Tests;

public sealed class SecurityPolicyEngineTests
{
    private readonly SecurityPolicyEngine _engine = new();

    [Fact]
    public void Evaluate_RejectedAuthenticationCreatesIncident()
    {
        var securityEvent = new SecurityEvent(
            DateTimeOffset.UtcNow,
            SecurityEventType.AuthenticationAttempt,
            SecuritySeverity.Warning,
            SecurityEventStatus.Rejected,
            "Rejected authentication.",
            "SL-test");

        var incident = _engine.Evaluate(securityEvent);

        Assert.NotNull(incident);
        Assert.Equal("SL-test", incident.IncidentId);
        Assert.Equal(SecuritySeverity.Warning, incident.Severity);
        Assert.False(incident.IsResolved);
    }

    [Fact]
    public void Evaluate_SessionEventDoesNotCreateIncident()
    {
        var securityEvent = new SecurityEvent(
            DateTimeOffset.UtcNow,
            SecurityEventType.SessionStarted,
            SecuritySeverity.Info,
            SecurityEventStatus.Accepted,
            "Session started.",
            "SL-session");

        Assert.Null(_engine.Evaluate(securityEvent));
    }

    [Fact]
    public void Evaluate_PolicyViolationCreatesIncident()
    {
        var securityEvent = new SecurityEvent(
            DateTimeOffset.UtcNow,
            SecurityEventType.PolicyViolation,
            SecuritySeverity.High,
            SecurityEventStatus.Observed,
            "Development policy violation.",
            "SL-policy");

        var incident = _engine.Evaluate(securityEvent);

        Assert.NotNull(incident);
        Assert.Equal("Development policy violation.", incident.Summary);
        Assert.Equal(SecurityEventType.PolicyViolation, incident.TriggerEvent);
    }
}
