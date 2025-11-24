using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Models;
using AutoMindBackend.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

    [HttpGet]
    [Authorize(Roles = "Admin,User")] // Rollen von Keycloak
    public async Task<IActionResult> GetAll()
    {
        await _userSyncService.SyncUserFromKeycloak(User);
        var keycloakUserId = User.FindFirst("sub")?.Value;

        if (User.IsInRole("Admin"))
            return Ok(_service.GetAll());

        return Ok(_service.GetAllByKeycloakUser(keycloakUserId!));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetById(int id)
    {
        await _userSyncService.SyncUserFromKeycloak(User);
        var keycloakUserId = User.FindFirst("sub")?.Value;

        if (User.IsInRole("Admin"))
            return Ok(_service.GetById(id));

        var trip = _service.GetByIdAndKeycloakUser(id, keycloakUserId!);
        if (trip == null) return Unauthorized("Kein Zugriff auf diese Fahrt!");
        return Ok(trip);
    }

    [HttpGet("vehicle/{vehicleId}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetByVehicle(int vehicleId)
    {
        await _userSyncService.SyncUserFromKeycloak(User);
        var keycloakUserId = User.FindFirst("sub")?.Value;

        if (User.IsInRole("Admin"))
            return Ok(_service.GetByVehicleId(vehicleId));

        return Ok(_service.GetByVehicleIdAndKeycloakUser(vehicleId, keycloakUserId!));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Nur Admin darf löschen
    public IActionResult Delete(int id)
    {
        var deleted = _service.Delete(id);
        return deleted ? NoContent() : NotFound();
    }

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
                UserId = user.Id
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