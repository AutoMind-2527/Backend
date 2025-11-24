using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Services;
using Microsoft.AspNetCore.Authorization;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Nur eingeloggte Benutzer
public class GpsController : ControllerBase
{
    private readonly GpsService _gpsService;

    public GpsController(GpsService gpsService)
    {
        _gpsService = gpsService;
    }

    [HttpPost("simulate-trip")]
    [Authorize(Roles = "Admin,User")]
    public IActionResult SimulateTrip([FromQuery] int vehicleId, [FromQuery] double startLat, [FromQuery] double startLon, [FromQuery] double endLat, [FromQuery] double endLon)
    {
        var trip = _gpsService.CreateTripFromGps(startLat, startLon, endLat, endLon, vehicleId);
        return Ok(trip);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")] // Nur Admin darf alle GPS-Daten sehen
    public IActionResult GetAll()
    {
        var gpsData = _gpsService.GetAllGpsData();
        return Ok(gpsData);
    }
}