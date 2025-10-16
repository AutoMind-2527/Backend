using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Models;
using AutoMindBackend.Services;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly TripService _service;

    public TripsController(TripService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var trips = _service.GetAll();
        return Ok(trips);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var trip = _service.GetById(id);
        if (trip == null) return NotFound();
        return Ok(trip);
    }

    [HttpGet("vehicle/{vehicleId}")]
    public IActionResult GetByVehicle(int vehicleId)
    {
        var trips = _service.GetByVehicleId(vehicleId);
        return Ok(trips);
    }

    [HttpGet("stats/{vehicleId}")]
    public IActionResult GetStats(int vehicleId)
    {
        var stats = new
        {
            TotalKm = _service.GetTotalDistance(vehicleId),
            TotalFuel = _service.GetTotalFuelUsed(vehicleId),
            TotalCost = _service.GetTotalCost(vehicleId),
            AvgConsumption = _service.GetAverageConsumption(vehicleId)
        };
        return Ok(stats);
    }


    [HttpPost]
    public IActionResult Create(Trip trip)
    {
        try
        {
            var created = _service.Add(trip);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var deleted = _service.Delete(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
