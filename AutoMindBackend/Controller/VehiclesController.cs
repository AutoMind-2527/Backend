using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Data;
using AutoMindBackend.Models;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly AppDbContext _context;

    public VehiclesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_context.Vehicles.ToList());
    }

    [HttpPost]
    public IActionResult Create(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetAll), vehicle);
    }
}
