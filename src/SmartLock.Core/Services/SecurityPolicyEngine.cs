using SmartLock.Core.Models;

namespace SmartLock.Core.Services;

public sealed class SecurityPolicyEngine
{
    public Incident? Evaluate(SecurityEvent securityEvent)
    {
        ArgumentNullException.ThrowIfNull(securityEvent);

        return securityEvent.EventType switch
        {
            SecurityEventType.AuthenticationAttempt when securityEvent.Status == SecurityEventStatus.Rejected
                => new Incident(
                    securityEvent.IncidentId,
                    securityEvent.Timestamp,
                    securityEvent.Severity,
                    securityEvent.EventType,
                    "Rejected authentication attempt detected.",
                    false),
            SecurityEventType.PolicyViolation => new Incident(
                securityEvent.IncidentId,
                securityEvent.Timestamp,
                securityEvent.Severity,
                securityEvent.EventType,
                securityEvent.Message,
                false),
            _ => null
        };
    }
}
