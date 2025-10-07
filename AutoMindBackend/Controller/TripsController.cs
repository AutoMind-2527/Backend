using Microsoft.AspNetCore.Mvc;
using AutoMindBackend.Data;
using AutoMindBackend.Models;

namespace AutoMindBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TripsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var trips = _context.Trips.ToList();
        return Ok(trips);
    }

    [HttpPost]
    public IActionResult Create(Trip trip)
    {
        _context.Trips.Add(trip);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetAll), trip);
    }
}
