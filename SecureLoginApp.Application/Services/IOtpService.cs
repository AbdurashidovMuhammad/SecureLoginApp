using SecureLoginApp.Core.Entities;

namespace SecureLoginApp.Application.Services;

public interface IOtpService
{
    Task<string> GenerateAndSaveOtpAsync(int userId);
    Task<UserOTPs?> GetLatestOtpAsync(int userId, string code);
}
