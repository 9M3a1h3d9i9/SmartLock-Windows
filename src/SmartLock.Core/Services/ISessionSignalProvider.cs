using SmartLock.Core.Models;

namespace SmartLock.Core.Services;

public interface ISessionSignalProvider
{
    SessionContext GetCurrentContext(DateTimeOffset observedAt);
}
