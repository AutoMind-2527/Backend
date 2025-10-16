using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Services;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GpsController : ControllerBase
{
    private readonly GpsService _gpsService;

    public GpsController(GpsService gpsService)
    {
        _gpsService = gpsService;
    }

    [HttpPost("simulate-trip")]
    public IActionResult SimulateTrip([FromQuery] int vehicleId, [FromQuery] double startLat, [FromQuery] double startLon, [FromQuery] double endLat, [FromQuery] double endLon)
    {
        var trip = _gpsService.CreateTripFromGps(startLat, startLon, endLat, endLon, vehicleId);
        return Ok(trip);
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var gpsData = _gpsService.GetAllGpsData();
        return Ok(gpsData);
    }

}
