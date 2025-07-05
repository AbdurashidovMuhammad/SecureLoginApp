using SecureLoginApp.Core.Entities;

namespace SecureLoginApp.Application.Helpers.GenerateJwt;

public interface IJwtTokenHandler
{
    string GenerateAccessToken(User user, string sessionToken);
    string GenerateRefreshToken();
}
