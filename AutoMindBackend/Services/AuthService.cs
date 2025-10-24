using System.Security.Cryptography;
using System.Text;
using AutoMindBackend.Data;
using AutoMindBackend.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AutoMindBackend.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public User Register(string username, string password)
    {
        if (_context.Users.Any(u => u.Username == username))
            throw new Exception("Benutzername existiert bereits.");

        CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        var user = new User
        {
            Username = username,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = username.ToLower() == "admin" ? "Admin" : "User"
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }


    public string Login(string username, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null) throw new Exception("Benutzer nicht gefunden.");

        if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            throw new Exception("Falsches Passwort.");

        return CreateToken(user);
    }

    private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
    {
        using var hmac = new HMACSHA512();
        salt = hmac.Key;
        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        using var hmac = new HMACSHA512(salt);
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computed.SequenceEqual(hash);
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(6),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public void ResetPassword(int userId, string newPassword)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) throw new Exception("Benutzer nicht gefunden.");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            throw new Exception("Neues Passwort muss mindestens 6 Zeichen haben.");

        CreatePasswordHash(newPassword, out byte[] hash, out byte[] salt);

        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        _context.SaveChanges();

        Console.WriteLine($"[AuthService] Admin hat Passwort für UserId {userId} zurückgesetzt.");
    }
}
