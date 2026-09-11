namespace SmartLock.Core.Services;

public interface IWindowsSecurityAlertService
{
    void Show(string title, string message);
}
