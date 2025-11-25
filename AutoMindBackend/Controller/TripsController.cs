using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Models;
using AutoMindBackend.Services;
using Microsoft.AspNetCore.Authorization;

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

    
    [HttpGet]
    [Authorize(Roles = "Admin,User")] 
    public async Task<IActionResult> GetAll()
    {
        await _userSyncService.SyncUserFromKeycloak(User);
        var user = await _userSyncService.GetUserFromKeycloak(User);

        if (User.IsInRole("Admin"))
        {
            return Ok(_service.GetAll());
        }

        var trips = _service.GetAllByUserId(user.Id);
        return Ok(trips);
    }

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

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] 
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
