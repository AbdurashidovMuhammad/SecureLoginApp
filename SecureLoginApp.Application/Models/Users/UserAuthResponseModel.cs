namespace SecureLoginApp.Application.Models.Users;

public class UserAuthResponseModel
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public IEnumerable<string> Permissions { get; set; } = new List<string>();
}
