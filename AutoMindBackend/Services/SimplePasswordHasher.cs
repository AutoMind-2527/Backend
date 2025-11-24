using System.Security.Cryptography;
using System.Text;

namespace AutoMindBackend.Services;

public class SimplePasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash); // ✅ Gibt string zurück
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var hashOfProvided = HashPassword(providedPassword);
        return hashedPassword == hashOfProvided;
    }
}