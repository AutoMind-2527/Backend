using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Models;
using AutoMindBackend.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly VehicleService _service;
    private readonly UserSyncService _userSyncService;

    public VehiclesController(VehicleService service, UserSyncService userSyncService)
    {
        _service = service;
        _userSyncService = userSyncService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,User")]
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

        var vehicle = _service.GetByIdAndKeycloakUser(id, keycloakUserId!);
        if (vehicle == null) 
            return Unauthorized("Kein Zugriff auf dieses Fahrzeug!");
        
        return Ok(vehicle);
    }

    [HttpGet("{id}/service-status")]
    [Authorize(Roles = "Admin,User")]
    public IActionResult CheckService(int id)
    {
        var serviceStatus = _service.GetServiceStatus(id);
        return Ok(serviceStatus);
    }

    [HttpGet("{id}/stats")]
    [Authorize(Roles = "Admin,User")]
    public IActionResult GetVehicleStats(int id)
    {
        var stats = _service.GetVehicleStats(id);
        if (stats == null)
            return NotFound();
        
        return Ok(stats);
    }

    [HttpGet("needing-service")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetVehiclesNeedingService()
    {
        var vehicles = _service.GetVehiclesNeedingService();
        return Ok(vehicles);
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
    public async Task<IActionResult> Create(VehicleCreateDto dto)
    {
        await _userSyncService.SyncUserFromKeycloak(User);
        var user = await _userSyncService.GetUserFromKeycloak(User);

        var vehicle = new Vehicle
        {
            LicensePlate = dto.LicensePlate,
            Brand = dto.Brand,
            Model = dto.Model,
            Mileage = dto.Mileage,
            FuelConsumption = dto.FuelConsumption,
            UserId = user.Id
        };

        var created = _service.Add(vehicle);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> Update(int id, VehicleUpdateDto dto)
    {
        await _userSyncService.SyncUserFromKeycloak(User);
        var keycloakUserId = User.FindFirst("sub")?.Value;

        Vehicle updatedVehicle;

        if (User.IsInRole("Admin"))
        {
            updatedVehicle = _service.Update(id, dto);
        }
        else
        {
            // Prüfen ob User Zugriff auf das Fahrzeug hat
            var existingVehicle = _service.GetByIdAndKeycloakUser(id, keycloakUserId!);
            if (existingVehicle == null)
                return Unauthorized("Kein Zugriff auf dieses Fahrzeug!");

            updatedVehicle = _service.Update(id, dto);
        }

        if (updatedVehicle == null)
            return NotFound();

        return Ok(updatedVehicle);
    }
}