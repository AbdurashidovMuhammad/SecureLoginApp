namespace SecureLoginApp.Core.Entities;

public class PermissionGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
