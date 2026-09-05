using System.Windows;

namespace SmartLock.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
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
        // V0.1 intentionally does not authenticate or handle Windows credentials.
        // Authentication integration will use supported Windows APIs in a later milestone.
        StatusText.Text = "Authentication is not configured in this development build.";
        PinBox.Clear();
        PinBox.Focus();
    }
}
