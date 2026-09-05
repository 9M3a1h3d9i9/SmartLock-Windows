using System.Windows.Threading;
using SmartLock.Core.Models;
using SmartLock.Core.Services;

namespace SmartLock.App.Services;

public sealed class SessionContextMonitor : IDisposable
{
    private readonly ISessionSignalProvider _provider;
    private readonly DispatcherTimer _timer;

    public SessionContextMonitor(ISessionSignalProvider provider, TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval));
        }

        _provider = provider;
        _timer = new DispatcherTimer { Interval = interval };
        _timer.Tick += OnTick;
    }

    public event EventHandler<SessionContext>? ContextUpdated;

    public void Start()
    {
        Publish();
        _timer.Start();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
    }

    private void OnTick(object? sender, EventArgs e) => Publish();

    private void Publish()
    {
        try
        {
            var context = _provider.GetCurrentContext(DateTimeOffset.UtcNow);
            ContextUpdated?.Invoke(this, context);
        }
        catch (Exception)
        {
            // The monitor must not terminate the UI loop if an OS signal is unavailable.
        }
    }
}
