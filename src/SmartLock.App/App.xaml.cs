using System.Windows;
using SmartLock.App.Services;
using SmartLock.Core.Services;
using SmartLock.Infrastructure.Windows;

namespace SmartLock.App;

public partial class App : Application
{
    public ISecurityEventService SecurityEvents { get; } = new SecurityEventService();
    public AuthenticationIncidentEngine IncidentEngine { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IncidentEngine = new AuthenticationIncidentEngine(
            SecurityEvents,
            maxFailedAttempts: 5,
            lockoutDuration: TimeSpan.FromSeconds(30));

        var cameraEvidence = new WindowsCameraEvidenceService();
        var window = new MainWindow(SecurityEvents, cameraEvidence, IncidentEngine);
        MainWindow = window;
        window.Show();

        var monitor = new SessionContextMonitor(new WindowsSessionSignalProvider(), TimeSpan.FromSeconds(1));
        window.Closed += (_, _) => monitor.Dispose();
        monitor.ContextUpdated += (_, context) => window.UpdateSessionContext(context);
        monitor.Start();
    }
}
