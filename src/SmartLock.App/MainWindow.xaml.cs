using System.Windows;
using SmartLock.Core.Services;
using SmartLock.UI.ViewModels;

namespace SmartLock.App;

public partial class MainWindow : Window
{
    private readonly LockScreenViewModel _viewModel;

    public MainWindow(ISecurityEventService securityEvents)
    {
        ArgumentNullException.ThrowIfNull(securityEvents);

        _viewModel = new LockScreenViewModel(securityEvents);
        InitializeComponent();
        Loaded += (_, _) => PinBox.Focus();
        DataContext = _viewModel;
    }

    private void Unlock_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SubmitAuthentication();
        PinBox.Clear();
        PinBox.Focus();
    }

    protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.E &&
            (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0)
        {
            Close();
            e.Handled = true;
            return;
        }

        base.OnPreviewKeyDown(e);
    }
}
