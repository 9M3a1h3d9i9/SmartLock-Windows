using System.Runtime.InteropServices;
using SmartLock.Core.Services;

namespace SmartLock.Infrastructure.Windows;

public sealed class WindowsWorkstationLockService : IWorkstationLockService
{
    public bool TryLock() => LockWorkStation();

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool LockWorkStation();
}
