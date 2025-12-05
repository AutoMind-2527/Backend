using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Models;
using AutoMindBackend.Services;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly TripService _service;
    private readonly UserSyncService _userSyncService;

    public TripsController(TripService service, UserSyncService userSyncService)
    {
        _service = service;
        _userSyncService = userSyncService;
    }

    // GET: api/trips
    // Admin: alle Trips, User: nur eigene
    [HttpGet]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetAll()
    {
        await _userSyncService.SyncUserFromKeycloak(User);
        var user = await _userSyncService.GetUserFromKeycloak(User);

        if (User.IsInRole("Admin"))
        {
            var trips = _service.GetAll();
            return Ok(trips);
        }

        var userTrips = _service.GetAllByUserId(user.Id);
        return Ok(userTrips);
    }

    // GET: api/trips/{id}
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetById(int id)
    {
        await _userSyncService.SyncUserFromKeycloak(User);
        var user = await _userSyncService.GetUserFromKeycloak(User);

        if (User.IsInRole("Admin"))
        {
            var trip = _service.GetById(id);
            if (trip == null) return NotFound();
            return Ok(trip);
        }
        else
        {
            var trip = _service.GetByIdAndUserId(id, user.Id);
            if (trip == null) return NotFound();
            return Ok(trip);
        }
    }

    // GET: api/trips/byVehicle/{vehicleId}
    [HttpGet("byVehicle/{vehicleId:int}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetByVehicle(int vehicleId)
    {
        await _userSyncService.SyncUserFromKeycloak(User);
        var user = await _userSyncService.GetUserFromKeycloak(User);

        if (User.IsInRole("Admin"))
        {
            var trips = _service.GetByVehicleId(vehicleId);
            return Ok(trips);
        }
        else
        {
            var trips = _service.GetByVehicleIdAndUserId(vehicleId, user.Id);
            return Ok(trips);
        }
    }

    // DELETE: api/trips/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userSyncService.SyncUserFromKeycloak(User);
        var user = await _userSyncService.GetUserFromKeycloak(User);

        if (!User.IsInRole("Admin"))
        {
            // Prüfen, ob der Trip dem aktuell eingeloggten User gehört
            var trip = _service.GetByIdAndUserId(id, user.Id);
            if (trip == null) return NotFound();
        }

        var success = _service.Delete(id);
        if (!success) return NotFound();

        return NoContent();
    }

    // POST: api/trips
    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> Create([FromBody] TripCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

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
        var createdDto = created.ToDto();

        return CreatedAtAction(nameof(GetById), new { id = createdDto.Id }, createdDto);
    }
}
