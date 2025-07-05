using System.Security.Cryptography;
using System.Text;

namespace SecureLoginApp.Application.Helpers.PasswordHashers;

public class PasswordHasher : IPasswordHasher
{
    private const int KeySize = 32;
    private const int IterationsCount = 10000;

    public string GenerateSalt()
    {
        return Guid.NewGuid().ToString();
    }

    public string Encrypt(string password, string salt)
    {
        using var algorithm = new Rfc2898DeriveBytes(
            password,
            Encoding.UTF8.GetBytes(salt),
            IterationsCount,
            HashAlgorithmName.SHA256);

        var key = algorithm.GetBytes(KeySize);
        return Convert.ToBase64String(key);
    }

    public bool Verify(string hash, string password, string salt)
    {
        var encrypted = Encrypt(password, salt);
        return hash == encrypted;
    }
}
