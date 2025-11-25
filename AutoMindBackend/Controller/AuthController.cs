using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly UserSyncService _userSyncService;

    public AuthController(UserSyncService userSyncService)
    {
        _userSyncService = userSyncService;
    }

    [HttpGet("sync-user")]
    public async Task<IActionResult> SyncUser()
    {
        try
        {
            // Benutzer aus Keycloak-Token synchronisieren
            var user = await _userSyncService.SyncUserFromKeycloak(User);
            return Ok(new
            {
                user.Id,
                user.Username,
                user.Role,
                user.Email,
                Message = "Benutzer erfolgreich synchronisiert"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value; // Keycloak User ID

        var username =
            User.FindFirst(ClaimTypes.Name)?.Value ??
            User.FindFirst("preferred_username")?.Value;

        var email =
            User.FindFirst(ClaimTypes.Email)?.Value ??
            User.FindFirst("email")?.Value;

        // Admin-Rolle prüfen: gibt es einen Role-Claim mit Wert "Admin"?
        var isAdmin = User.Claims.Any(c =>
            (c.Type == ClaimTypes.Role || c.Type == "role") &&
            c.Value == "Admin");

        var role = isAdmin ? "Admin" : "User";

        return Ok(new
        {
            KeycloakId = userId,
            Username = username,
            Role = role,
            Email = email
        });
    }
}
