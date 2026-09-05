using SmartLock.Core.Models;
using SmartLock.Core.Services;

namespace SmartLock.Core.Tests;

public sealed class SecurityEventServiceTests
{
    [Fact]
    public void Record_AddsTypedEventWithIncidentIdAndTimestamp()
    {
        var service = new SecurityEventService();

        var recorded = service.Record(
            SecurityEventType.AuthenticationAttempt,
            SecuritySeverity.Warning,
            SecurityEventStatus.Rejected,
            "Authentication is not configured.");

        var securityEvent = Assert.Single(service.Events);
        Assert.Equal(recorded, securityEvent);
        Assert.Equal(SecurityEventType.AuthenticationAttempt, securityEvent.EventType);
        Assert.Equal(SecuritySeverity.Warning, securityEvent.Severity);
        Assert.Equal(SecurityEventStatus.Rejected, securityEvent.Status);
        Assert.Equal("Authentication is not configured.", securityEvent.Message);
        Assert.StartsWith("SL-", securityEvent.IncidentId);
        Assert.NotEqual(default, securityEvent.Timestamp);
    }

    [Fact]
    public void Record_RejectsBlankMessage()
    {
        var service = new SecurityEventService();

        Assert.Throws<ArgumentException>(() => service.Record(
            SecurityEventType.PolicyViolation,
            SecuritySeverity.High,
            SecurityEventStatus.Observed,
            "   "));

        Assert.Empty(service.Events);
    }

    [Fact]
    public void Record_TrimsMessage()
    {
        var service = new SecurityEventService();

        service.Record(
            SecurityEventType.SessionStarted,
            SecuritySeverity.Info,
            SecurityEventStatus.Accepted,
            " message ");

        var securityEvent = Assert.Single(service.Events);
        Assert.Equal("message", securityEvent.Message);
    }

    [Fact]
    public void Events_ReturnsSnapshot()
    {
        var service = new SecurityEventService();
        service.Record(
            SecurityEventType.SessionStarted,
            SecuritySeverity.Info,
            SecurityEventStatus.Accepted,
            "session started");

        var snapshot = service.Events;

        service.Record(
            SecurityEventType.SessionEnded,
            SecuritySeverity.Info,
            SecurityEventStatus.Resolved,
            "session ended");

        Assert.Single(snapshot);
        Assert.Equal(2, service.Events.Count);
    }
}
