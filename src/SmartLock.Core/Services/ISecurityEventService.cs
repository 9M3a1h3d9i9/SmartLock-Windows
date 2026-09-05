namespace SmartLock.Core.Services;

public interface ISecurityEventService
{
    void Record(string eventType, string status, string message);
}
