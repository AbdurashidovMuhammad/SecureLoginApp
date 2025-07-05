namespace SecureLoginApp.Application.Security;

public interface IUser
{
    int Id { get; }
    public string FullName { get; }
    public IEnumerable<string> Permissions { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsVerified { get; set; }
}
