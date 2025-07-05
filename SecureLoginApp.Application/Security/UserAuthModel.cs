namespace SecureLoginApp.Application.Security;

public class UserAuthModel : IUser
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public IEnumerable<string> Permissions { get; set; } = [];
    public bool IsAdmin { get; set; }
    public bool IsVerified { get; set; }
}
