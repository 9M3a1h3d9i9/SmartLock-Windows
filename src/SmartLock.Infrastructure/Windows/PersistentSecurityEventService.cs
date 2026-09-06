using SmartLock.Core.Models;
using SmartLock.Core.Services;

namespace SmartLock.Infrastructure.Windows;

/// <summary>
/// Adds durable local JSON persistence and best-effort Windows Event Log forwarding
/// to the in-memory security event service.
/// </summary>
public sealed class PersistentSecurityEventService : ISecurityEventService
{
    private readonly SecurityEventService _inner;
    private readonly ISecurityEventStore _store;
    private readonly WindowsEventLogSecuritySink _eventLogSink;
    private readonly object _sync = new();

    public PersistentSecurityEventService(
        ISecurityEventStore store,
        WindowsEventLogSecuritySink eventLogSink)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(eventLogSink);

        _store = store;
        _eventLogSink = eventLogSink;
        var initialEvents = _store.LoadAsync().GetAwaiter().GetResult();
        _inner = new SecurityEventService(initialEvents);
    }

    public IReadOnlyList<SecurityEvent> Events => _inner.Events;

    public SecurityEvent Record(
        SecurityEventType eventType,
        SecuritySeverity severity,
        SecurityEventStatus status,
        string message)
    {
        var securityEvent = _inner.Record(eventType, severity, status, message);

        lock (_sync)
        {
            // The local JSON store is the authoritative durable audit trail.
            _store.AppendAsync(securityEvent).GetAwaiter().GetResult();
            _eventLogSink.Write(securityEvent);
        }

        return securityEvent;
    }
}
