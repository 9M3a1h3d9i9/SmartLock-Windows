using System.Runtime.InteropServices;
using SmartLock.Core.Models;
using SmartLock.Core.Services;

namespace SmartLock.Infrastructure.Windows;

public sealed class WindowsSessionSignalProvider : ISessionSignalProvider
{
    public SessionContext GetCurrentContext(DateTimeOffset observedAt)
    {
        var idleDuration = GetIdleDuration();
        return SessionContextEvaluator.Evaluate(observedAt, idleDuration, isLocked: false);
    }

    private static TimeSpan GetIdleDuration()
    {
        var lastInputInfo = new LASTINPUTINFO
        {
            cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>()
        };

        if (!GetLastInputInfo(ref lastInputInfo))
        {
            throw new InvalidOperationException("Windows could not provide the last user-input timestamp.");
        }

        var elapsedMilliseconds = unchecked((uint)Environment.TickCount64 - lastInputInfo.dwTime);
        return TimeSpan.FromMilliseconds(elapsedMilliseconds);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }
}
