namespace SmartLock.Core.Services;

public interface IAdminOtpService
{
    bool Validate(string code);
}
