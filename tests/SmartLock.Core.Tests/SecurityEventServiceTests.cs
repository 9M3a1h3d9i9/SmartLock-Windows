using SmartLock.Core.Services;

namespace SmartLock.Core.Tests;

public sealed class SecurityEventServiceTests
{
    [Fact]
    public void Record_AddsEventWithIncidentIdAndTimestamp()
    {
        var service = new SecurityEventService();

        service.Record("AUTH_ATTEMPT", "REJECTED", "Authentication is not configured.");

        var securityEvent = Assert.Single(service.Events);
        Assert.Equal("AUTH_ATTEMPT", securityEvent.EventType);
        Assert.Equal("REJECTED", securityEvent.Status);
        Assert.Equal("Authentication is not configured.", securityEvent.Message);
        Assert.StartsWith("SL-", securityEvent.IncidentId);
        Assert.NotEqual(default, securityEvent.Timestamp);
    }

    [Theory]
    [InlineData("", "OK", "message")]
    [InlineData("EVENT", "", "message")]
    [InlineData("EVENT", "OK", "")]
    public void Record_RejectsBlankFields(string eventType, string status, string message)
    {
        var service = new SecurityEventService();

        Assert.Throws<ArgumentException>(() => service.Record(eventType, status, message));
        Assert.Empty(service.Events);
    }

    [Fact]
    public void Record_TrimsInput()
    {
        var service = new SecurityEventService();

        service.Record(" AUTH_ATTEMPT ", " REJECTED ", " message ");

        var securityEvent = Assert.Single(service.Events);
        Assert.Equal("AUTH_ATTEMPT", securityEvent.EventType);
        Assert.Equal("REJECTED", securityEvent.Status);
        Assert.Equal("message", securityEvent.Message);
    }
}
