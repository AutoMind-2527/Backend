using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Models;
using AutoMindBackend.Services;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly VehicleService _service;

    public VehiclesController(VehicleService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_service.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var vehicle = _service.GetById(id);
        if (vehicle == null) return NotFound();
        return Ok(vehicle);
    }

    [HttpPost]
    public IActionResult Create(Vehicle vehicle)
    {
        var created = _service.Add(vehicle);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var deleted = _service.Delete(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
