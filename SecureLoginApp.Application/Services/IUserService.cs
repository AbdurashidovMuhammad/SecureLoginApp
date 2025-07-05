using SecureLoginApp.Application.Models;
using SecureLoginApp.Application.Models.Users;

namespace SecureLoginApp.Application.Services;

public interface IUserService
{
    Task<ApiResult<string>> RegisterAsync(string fullname, string email, string password, bool isAdminSite);
    Task<ApiResult<LoginResponseModel>> LoginAsync(LoginUserModel model);
    Task<ApiResult<string>> VerifyOtpAsync(OtpVerificationModel model);
    Task<ApiResult<UserAuthResponseModel>> GetUserAuth();
}
