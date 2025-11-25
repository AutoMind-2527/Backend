using System.Security.Claims;
using AutoMindBackend.Data;
using AutoMindBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoMindBackend.Services;

public class UserSyncService
{
    private readonly AppDbContext _context;
    private readonly AuthService _authService;

    public UserSyncService(AppDbContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    private static string GetKeycloakUserId(ClaimsPrincipal userPrincipal)
    {
        // Versuche mehrere Claim-Typen, weil .NET "sub" oft ummappt
        var sub =
            userPrincipal.FindFirst("sub")?.Value ??
            userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            userPrincipal.FindFirst("sid")?.Value;

        if (string.IsNullOrWhiteSpace(sub))
            throw new Exception("Keycloak User ID nicht gefunden");

        return sub;
    }

    private static string GetUsername(ClaimsPrincipal userPrincipal)
    {
        // Username aus Name oder preferred_username holen
        var username =
            userPrincipal.FindFirst(ClaimTypes.Name)?.Value ??
            userPrincipal.FindFirst("preferred_username")?.Value;

        if (string.IsNullOrWhiteSpace(username))
            throw new Exception("Username nicht gefunden");

        return username;
    }

    private static string GetRole(ClaimsPrincipal userPrincipal)
    {
        // Prüfen, ob ein Role-Claim "Admin" existiert
        var isAdmin = userPrincipal.Claims.Any(c =>
            (c.Type == ClaimTypes.Role || c.Type == "role") &&
            c.Value == "Admin");

        return isAdmin ? "Admin" : "User";
    }

    public async Task<User> SyncUserFromKeycloak(ClaimsPrincipal userPrincipal)
    {
        var keycloakUserId = GetKeycloakUserId(userPrincipal);
        var username = GetUsername(userPrincipal);
        var email = userPrincipal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var role = GetRole(userPrincipal);

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakUserId);

        if (existingUser != null)
        {
            existingUser.Username = username;
            existingUser.Email = email;
            existingUser.Role = role;

            await _context.SaveChangesAsync();
            return existingUser;
        }

        var newUser = new User
        {
            KeycloakId = keycloakUserId,
            Username = username,
            Email = email,
            Role = role
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return newUser;
    }

    public async Task<User> GetUserFromKeycloak(ClaimsPrincipal userPrincipal)
    {
        var keycloakUserId = GetKeycloakUserId(userPrincipal);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakUserId);

        if (user != null)
            return user;

        // Wenn User noch nicht existiert -> automatisch anlegen/syncen
        return await SyncUserFromKeycloak(userPrincipal);
    }
}
