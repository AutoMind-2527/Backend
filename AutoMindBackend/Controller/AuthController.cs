using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Services;
using Microsoft.AspNetCore.Authorization;
using AutoMindBackend.Models;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] LoginDto dto)
    {
        try
        {
            var user = _authService.Register(dto.Username, dto.Password);
            return Ok(new { user.Id, user.Username });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        try
        {
            string token = _authService.Login(dto.Username, dto.Password);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/reset-password")]
    public IActionResult ResetPassword(int id, [FromBody] ResetPasswordDto dto)
    {
        try
        {
            _authService.ResetPassword(id, dto.NewPassword);
            return Ok(new { message = "Passwort erfolgreich gesetzt. Bitte Benutzer informieren." });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
