using System.Windows;
using System.Windows.Threading;
using SmartLock.Core.Services;

namespace SmartLock.Infrastructure.Windows;

public sealed class WindowsSecurityAlertService : IWindowsSecurityAlertService
{
    public void Show(string title, string message)
    {
        var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        dispatcher.Invoke(() =>
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.ServiceNotification));
    }
}
