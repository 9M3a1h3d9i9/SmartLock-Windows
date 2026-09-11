using System.Windows;
using System.Windows.Threading;
using SmartLock.Core.Services;

namespace SmartLock.Infrastructure.Windows;

public sealed class WindowsIncomingCallService : IIncomingCallService
{
    public Task<bool> TriggerAsync(string callerName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callerName);
        cancellationToken.ThrowIfCancellationRequested();

        var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        dispatcher.Invoke(() =>
        {
            var window = new IncomingCallWindow(callerName)
            {
                Owner = Application.Current?.MainWindow
            };
            window.Show();
            window.Activate();
        });

        return Task.FromResult(true);
    }

    private sealed class IncomingCallWindow : Window
    {
        public IncomingCallWindow(string callerName)
        {
            Title = "Incoming call";
            Width = 360;
            Height = 220;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.ToolWindow;
            Topmost = true;
            Content = new System.Windows.Controls.StackPanel
            {
                Margin = new Thickness(28),
                Children =
                {
                    new System.Windows.Controls.TextBlock
                    {
                        Text = "INCOMING CALL",
                        FontSize = 12,
                        FontWeight = FontWeights.Bold
                    },
                    new System.Windows.Controls.TextBlock
                    {
                        Text = callerName,
                        FontSize = 28,
                        Margin = new Thickness(0, 18, 0, 8)
                    },
                    new System.Windows.Controls.TextBlock
                    {
                        Text = "Security notification • demo/optional integration",
                        Opacity = 0.7
                    },
                    new System.Windows.Controls.Button
                    {
                        Content = "Dismiss",
                        Margin = new Thickness(0, 20, 0, 0),
                        Padding = new Thickness(16, 8, 16, 8)
                    }
                }
            };

            if (Content is System.Windows.Controls.StackPanel panel && panel.Children[^1] is System.Windows.Controls.Button button)
            {
                button.Click += (_, _) => Close();
            }
        }
    }
}
