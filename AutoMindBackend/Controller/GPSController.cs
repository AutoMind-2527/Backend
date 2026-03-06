using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMindBackend.Services;
using AutoMindBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Nur eingeloggte Benutzer
public class GpsController : ControllerBase
{
    private readonly GpsService _gpsService;
    private readonly AppDbContext _context;

    public GpsController(GpsService gpsService, AppDbContext context)
    {
        _gpsService = gpsService;
        _context = context;
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
    /// Raspberry Pi/IoT device sends GPS data using tracker code (no user auth required).
    /// TODO: Consider adding API key authentication for production security.
    /// </summary>
    [HttpPost("byTracker")]
    [AllowAnonymous] // No user authentication required - Pi sends with tracker code
    public async Task<IActionResult> PushGpsPointByTracker([FromBody] GpsPointByTrackerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Look up vehicle by tracker code
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.TrackerCode == dto.TrackerCode);

        if (vehicle == null)
            return NotFound(new { error = "Tracker code not found" });

        if (!vehicle.IsClaimed)
            return BadRequest(new { error = "Tracker not yet claimed by a user" });

        // Send GPS data to the vehicle
        await _gpsService.AddGpsPointAsync(
            vehicle.Id,
            dto.Latitude,
            dto.Longitude,
            dto.SpeedKmh
        );

        return Ok(new { message = "GPS point stored", vehicleId = vehicle.Id });
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

/// <summary>
/// DTO for Raspberry Pi/IoT devices that send GPS data using tracker code.
/// </summary>
public class GpsPointByTrackerDto
{
    public string TrackerCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? SpeedKmh { get; set; }
}
