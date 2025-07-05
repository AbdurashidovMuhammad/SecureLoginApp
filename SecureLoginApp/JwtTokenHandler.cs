using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecureLoginApp.Core.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SecureLoginApp.Application.Helpers.GenerateJwt;

public class JwtTokenHandler : IJwtTokenHandler
{
    private readonly JwtOption _jwtOption;

    public JwtTokenHandler(IOptions<JwtOption> jwtOption)
    {
        _jwtOption = jwtOption.Value;
    }

    public string GenerateAccessToken(User user, string token)
    {
        var claims = new List<Claim>()
       {
           new Claim(CustomClaimNames.Id, user.Id.ToString()),
           new Claim(CustomClaimNames.Email, user.Email),
           new Claim(CustomClaimNames.Token, token)
       };

        // Fix for CS1061: 'User' does not contain a definition for 'Role'  
        // Assuming 'UserRole' is a collection in 'User' and contains role information  
        if (user.UserRoles != null && user.UserRoles.Any())
        {
            foreach (var userRole in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.RoleName)); // Assuming 'RoleName' exists in 'UserRole'  
            }
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOption.SecretKey));

        var jwtToken = new JwtSecurityToken(
            issuer: _jwtOption.Issuer,
            audience: _jwtOption.Audience,
            expires: DateTime.Now.AddSeconds(_jwtOption.ExpirationInSeconds),
            claims: claims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    public string GenerateRefreshToken()
    {
        byte[] bytes = new byte[64];

        using var randomGenerator = RandomNumberGenerator.Create();
        randomGenerator.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
