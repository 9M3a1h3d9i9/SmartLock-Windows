using System.Windows;
using SmartLock.Core.Models;
using SmartLock.Core.Services;
using SmartLock.UI.ViewModels;

namespace SmartLock.App;

public partial class MainWindow : Window
{
    private readonly LockScreenViewModel _viewModel;

    public MainWindow(
        ISecurityEventService securityEvents,
        ICameraEvidenceService cameraEvidence,
        AuthenticationIncidentEngine incidentEngine)
    {
        ArgumentNullException.ThrowIfNull(securityEvents);
        ArgumentNullException.ThrowIfNull(cameraEvidence);
        ArgumentNullException.ThrowIfNull(incidentEngine);

        _viewModel = new LockScreenViewModel(securityEvents, cameraEvidence, incidentEngine);
        InitializeComponent();
        Loaded += (_, _) => PinBox.Focus();
        DataContext = _viewModel;
    }

    public void UpdateSessionContext(SessionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        StatusText.Text = $"Session: {context.State} • Idle: {context.IdleDuration:hh\\:mm\\:ss}";
    }

    private async void Unlock_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SubmitAuthenticationAsync();
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
