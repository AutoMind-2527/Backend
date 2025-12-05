using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMindBackend.Services;

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

    /// <summary>
    /// Gerät schickt alle ~30 Sekunden einen GPS-Punkt.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> PushGpsPoint([FromBody] GpsPointCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _gpsService.AddGpsPointAsync(
            dto.VehicleId,
            dto.Latitude,
            dto.Longitude,
            dto.SpeedKmh
        );

        return Ok(new { message = "GPS point stored" });
    }

    /// <summary>
    /// Alle GPS-Daten sehen (nur Admin, Debug).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var gpsData = await _gpsService.GetAllGpsDataAsync();
        return Ok(gpsData);
    }

    /// <summary>
    /// Trip nur simulieren (wird NICHT in DB gespeichert).
    /// </summary>
    [HttpGet("simulate-trip")]
    [Authorize(Roles = "Admin,User")]
    public IActionResult SimulateTrip(
        [FromQuery] int vehicleId,
        [FromQuery] double startLat,
        [FromQuery] double startLon,
        [FromQuery] double endLat,
        [FromQuery] double endLon)
    {
        var tripPreview = _gpsService.CreateTripFromGps(startLat, startLon, endLat, endLon, vehicleId);
        return Ok(tripPreview);
    }
}

/// <summary>
/// DTO, das dein Gerät an die API schickt.
/// </summary>
public class GpsPointCreateDto
{
    public int VehicleId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? SpeedKmh { get; set; }
}
