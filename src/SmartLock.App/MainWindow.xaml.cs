using System.Windows;
using SmartLock.Core.Services;

namespace SmartLock.App;

public partial class MainWindow : Window
{
    private readonly ISecurityEventService _securityEvents;

    public MainWindow(ISecurityEventService securityEvents)
    {
        ArgumentNullException.ThrowIfNull(securityEvents);
        _securityEvents = securityEvents;

        InitializeComponent();
        Loaded += (_, _) => PinBox.Focus();
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.E &&
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0)
            {
                Close();
            }
        };
    }

    private void Unlock_Click(object sender, RoutedEventArgs e)
    {
        _securityEvents.Record(
            "AUTH_ATTEMPT",
            "REJECTED",
            "Authentication is not configured in this development build.");

        StatusText.Text = "Authentication is not configured in this development build.";
        PinBox.Clear();
        PinBox.Focus();
    }
}
