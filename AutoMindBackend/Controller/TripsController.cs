using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Models;
using AutoMindBackend.Services;
using Microsoft.AspNetCore.Authorization;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly TripService _service;
    private readonly UserContextService _userContext;

    public TripsController(TripService service, UserContextService userContext)
    {
        _service = service;
        _userContext = userContext;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var username = _userContext.GetUsername();

        if (User.IsInRole("Admin"))
            return Ok(_service.GetAll());

        return Ok(_service.GetAllByUser(username!));
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var username = _userContext.GetUsername();

        if (User.IsInRole("Admin"))
            return Ok(_service.GetById(id));

        var trip = _service.GetByIdAndUser(id, username!);
        if (trip == null) return Unauthorized("Kein Zugriff auf diese Fahrt!");
        return Ok(trip);
    }

    [HttpGet("vehicle/{vehicleId}")]
    public IActionResult GetByVehicle(int vehicleId)
    {
        var username = _userContext.GetUsername();

        if (User.IsInRole("Admin"))
            return Ok(_service.GetByVehicleId(vehicleId));

        return Ok(_service.GetByVehicleIdAndUser(vehicleId, username!));
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var username = _userContext.GetUsername();

        if (User.IsInRole("Admin"))
        {
            var deleted = _service.Delete(id);
            return deleted ? NoContent() : NotFound();
        }

        bool success = _service.DeleteByUser(id, username!);
        return success ? NoContent() : Unauthorized("Kein Zugriff auf diese Fahrt!");
    }
    
    [HttpPost]
    public IActionResult Create(Trip trip)
    {
        try
        {
            var username = _userContext.GetUsername();
            var role = _userContext.GetRole();
            var created = _service.AddForUser(trip, username!, role!);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}
