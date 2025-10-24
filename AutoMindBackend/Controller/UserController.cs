using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMindBackend.Services;
using AutoMindBackend.Models;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] 
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult GetAllUsers()
    {
        var users = _userService.GetAll();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public IActionResult GetUserById(int id)
    {
        var user = _userService.GetById(id);
        if (user == null) return NotFound();
        return Ok(user);
    }


    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        bool deleted = _userService.Delete(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id}/vehicles")]
    public IActionResult GetUserVehicles(int id)
    {
        var vehicles = _userService.GetVehiclesByUser(id);
        return Ok(vehicles);
    }

    [HttpGet("{id}/trips")]
    public IActionResult GetUserTrips(int id)
    {
        var trips = _userService.GetTripsByUser(id);
        return Ok(trips);
    }
}
