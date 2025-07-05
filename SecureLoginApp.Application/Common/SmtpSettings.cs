namespace SecureLoginApp.Application.Common;

public class SmtpSettings
{
    public string SmtpServer { get; set; } = null!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string DefaultFromEmail { get; set; } = null!;
    public string DefaultFromName { get; set; } = null!;
}
