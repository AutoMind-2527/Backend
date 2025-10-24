using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Models;
using AutoMindBackend.Services;
using Microsoft.AspNetCore.Authorization;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class VehiclesController : ControllerBase
{
    private readonly VehicleService _service;
    private readonly UserContextService _userContext;

    public VehiclesController(VehicleService service, UserContextService userContext)
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

        var vehicle = _service.GetByIdAndUser(id, username!);
        if (vehicle == null) return Unauthorized("Kein Zugriff auf dieses Fahrzeug!");
        return Ok(vehicle);
    }

    [HttpGet("{id}/service-status")]
    public IActionResult CheckService(int id)
    {
        bool needsService = _service.NeedsService(id);
        return Ok(new { vehicleId = id, needsService });
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
        return success ? NoContent() : Unauthorized("Kein Zugriff auf dieses Fahrzeug!");
    }

    [HttpPost]
    public IActionResult Create(Vehicle vehicle)
    {
        var username = _userContext.GetUsername();
        var role = _userContext.GetRole();

        var created = _service.AddForUser(vehicle, username!, role!);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

}
