using System.Windows;
using SmartLock.Core.Services;

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
    }
}
