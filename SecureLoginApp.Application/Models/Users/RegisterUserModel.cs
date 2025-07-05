namespace SecureLoginApp.Application.Models.Users
{
    public class RegisterUserModel
    {
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool isAdminSite { get; set; }
    }
}
