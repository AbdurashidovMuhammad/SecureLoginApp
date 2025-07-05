namespace SecureLoginApp.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Fullname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Salt { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public bool IsVerified { get; set; } = false;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
