using System.Diagnostics;
using SmartLock.Core.Models;

namespace SmartLock.Infrastructure.Windows;

public sealed class WindowsEventLogSecuritySink
{
    private const string SourceName = "SmartLock-Windows";
    private const string LogName = "Application";

    public void Write(SecurityEvent securityEvent)
    {
        ArgumentNullException.ThrowIfNull(securityEvent);

        try
        {
            using var source = new EventLog(LogName) { Source = SourceName };
            source.WriteEntry(
                $"[{securityEvent.EventType}] {securityEvent.Message} IncidentId={securityEvent.IncidentId}",
                ToEntryType(securityEvent.Severity),
                GetEventId(securityEvent.EventType));
        }
        catch (Exception ex) when (ex is SecurityException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            // Event Log registration may require administrator privileges. Local persistence remains authoritative.
        }
    }

    private static EventLogEntryType ToEntryType(SecuritySeverity severity) => severity switch
    {
        SecuritySeverity.Critical => EventLogEntryType.Error,
        SecuritySeverity.High => EventLogEntryType.Error,
        SecuritySeverity.Warning => EventLogEntryType.Warning,
        _ => EventLogEntryType.Information
    };

    private static int GetEventId(SecurityEventType eventType) => eventType switch
    {
        SecurityEventType.AuthenticationAttempt => 1001,
        SecurityEventType.Lockout => 1002,
        SecurityEventType.SessionStarted => 1003,
        SecurityEventType.SessionEnded => 1004,
        SecurityEventType.PolicyViolation => 1005,
        _ => 1099
    };
}
