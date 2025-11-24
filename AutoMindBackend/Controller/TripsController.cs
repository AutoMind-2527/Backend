using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Models;
using AutoMindBackend.Services;
using Microsoft.AspNetCore.Authorization;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Nur eingeloggte Benutzer
public class TripsController : ControllerBase
{
    private readonly TripService _service;
    private readonly UserSyncService _userSyncService;

    public TripsController(TripService service, UserSyncService userSyncService)
    {
        _service = service;
        _userSyncService = userSyncService;
    }

    // GET /api/Trips
    [HttpGet]
    [Authorize(Roles = "Admin,User")] // Rollen von Keycloak
    public async Task<IActionResult> GetAll()
    {
        // User in lokaler DB synchronisieren und laden
        await _userSyncService.SyncUserFromKeycloak(User);
        var user = await _userSyncService.GetUserFromKeycloak(User);

        if (User.IsInRole("Admin"))
        {
            // Admin sieht alle Trips
            return Ok(_service.GetAll());
        }

        // Normale User sehen nur eigene Trips (über UserId)
        var trips = _service.GetAllByUserId(user.Id);
        return Ok(trips);
    }

    // GET /api/Trips/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetById(int id)
    {
        await _userSyncService.SyncUserFromKeycloak(User);
        var user = await _userSyncService.GetUserFromKeycloak(User);

        if (User.IsInRole("Admin"))
        {
            var tripAdmin = _service.GetById(id);
            if (tripAdmin == null) return NotFound();
            return Ok(tripAdmin);
        }

        var trip = _service.GetByIdAndUserId(id, user.Id);
        if (trip == null) 
            return Unauthorized("Kein Zugriff auf diese Fahrt!");

        return Ok(trip);
    }

    // GET /api/Trips/vehicle/{vehicleId}
    [HttpGet("vehicle/{vehicleId}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetByVehicle(int vehicleId)
    {
        await _userSyncService.SyncUserFromKeycloak(User);
        var user = await _userSyncService.GetUserFromKeycloak(User);

        if (User.IsInRole("Admin"))
        {
            return Ok(_service.GetByVehicleId(vehicleId));
        }

        var trips = _service.GetByVehicleIdAndUserId(vehicleId, user.Id);
        return Ok(trips);
    }

    // DELETE /api/Trips/{id}  -> nur Admin
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Nur Admin darf löschen
    public IActionResult Delete(int id)
    {
        var deleted = _service.Delete(id);
        return deleted ? NoContent() : NotFound();
    }

    // POST /api/Trips
    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> Create(TripCreateDto dto)
    {
        try
        {
            await _userSyncService.SyncUserFromKeycloak(User);
            var user = await _userSyncService.GetUserFromKeycloak(User);

            var trip = new Trip
            {
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                DistanceKm = dto.DistanceKm,
                StartLocation = dto.StartLocation,
                EndLocation = dto.EndLocation,
                VehicleId = dto.VehicleId,
                UserId = user.Id  // WICHTIG: Trip gehört diesem DB-User
            };

            var created = _service.Add(trip);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
