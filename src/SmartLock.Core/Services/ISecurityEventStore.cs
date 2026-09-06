using SmartLock.Core.Models;

namespace SmartLock.Core.Services;

public interface ISecurityEventStore
{
    Task AppendAsync(SecurityEvent securityEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SecurityEvent>> LoadAsync(CancellationToken cancellationToken = default);
}
