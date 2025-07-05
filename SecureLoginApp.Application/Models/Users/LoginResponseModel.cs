namespace SecureLoginApp.Application.Models.Users;

public class LoginResponseModel
{
    public string Email { get; set; } = null!;
    public string Fullname { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;

    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}
