namespace SecureLoginApp.Application.Models.Roles
{
    public class RoleListResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool HasAssignedUsers { get; set; }
    }
}
