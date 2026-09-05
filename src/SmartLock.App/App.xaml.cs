using System.Windows;
using SmartLock.App.Services;
using SmartLock.Core.Services;
using SmartLock.Infrastructure.Windows;

namespace SmartLock.App;

public partial class App : Application
{
    public ISecurityEventService SecurityEvents { get; } = new SecurityEventService();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var window = new MainWindow(SecurityEvents);
        MainWindow = window;
        window.Show();

        var monitor = new SessionContextMonitor(new WindowsSessionSignalProvider(), TimeSpan.FromSeconds(1));
        window.Closed += (_, _) => monitor.Dispose();
        monitor.ContextUpdated += (_, context) => window.UpdateSessionContext(context);
        monitor.Start();
    }
}
