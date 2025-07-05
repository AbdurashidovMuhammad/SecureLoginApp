namespace SecureLoginApp.Core.Entities;

public class UserOTPs
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ExpiredAt { get; set; }

    public User User { get; set; } = null!;

    public ICollection<UserOTPs> OtpCodes { get; set; } = new List<UserOTPs>();
}
