using AutoMindBackend.Data;
using AutoMindBackend.Models;

namespace AutoMindBackend.Services;

public class VehicleService
{
    private readonly AppDbContext _context;

    public VehicleService(AppDbContext context)
    {
        _context = context;
    }

    public List<Vehicle> GetAll()
    {
        return _context.Vehicles.ToList();
    }

    public Vehicle? GetById(int id)
    {
        return _context.Vehicles.Find(id);
    }

    public Vehicle Add(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();
        return vehicle;
    }

    public bool Delete(int id)
    {
        var vehicle = _context.Vehicles.Find(id);
        if (vehicle == null) return false;

        _context.Vehicles.Remove(vehicle);
        _context.SaveChanges();
        return true;
    }
}
