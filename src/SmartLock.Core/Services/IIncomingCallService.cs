namespace SmartLock.Core.Services;

public interface IIncomingCallService
{
    Task<bool> TriggerAsync(string callerName, CancellationToken cancellationToken = default);
}
