using AutoMindBackend.Data;
using AutoMindBackend.Models;
using Microsoft.AspNetCore.Http;

namespace AutoMindBackend.Services;

public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    // ✅ Nur für lokale Benutzer (falls benötigt)
    public User CreateLocalUser(string username, string password)
    {
        if (_context.Users.Any(u => u.Username == username))
            throw new Exception("Benutzername bereits vergeben");

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), // Nur für lokale Benutzer
            Role = "User"
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }

    // ✅ Keycloak Benutzer synchronisieren
    public User SyncKeycloakUser(string keycloakUserId, string username, string email, string role)
    {
        var existingUser = _context.Users.FirstOrDefault(u => u.KeycloakId == keycloakUserId);
        
        if (existingUser != null)
        {
            // Update falls nötig
            existingUser.Username = username;
            existingUser.Email = email;
            existingUser.Role = role;
        }
        else
        {
            // Neuen Benutzer erstellen
            existingUser = new User
            {
                KeycloakId = keycloakUserId,
                Username = username,
                Email = email,
                Role = role
            };
            _context.Users.Add(existingUser);
        }

        _context.SaveChanges();
        return existingUser;
    }

    // ✅ Lokales Login (falls benötigt)
    public User ValidateLocalUser(string username, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null)
            throw new Exception("Benutzer nicht gefunden");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new Exception("Ungültiges Passwort");

        return user;
    }
}

