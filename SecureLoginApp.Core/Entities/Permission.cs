namespace SecureLoginApp.Core.Entities;

public class Permission
{
    public int Id { get; set; }
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int PermissionGroupId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    public PermissionGroup PermissionGroup { get; set; } = null!;
    public ICollection<RolePermission> Roles { get; set; } = new List<RolePermission>();
}
