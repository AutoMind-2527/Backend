using AutoMindBackend.Data;
using AutoMindBackend.Models;

namespace AutoMindBackend.Services;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public List<UserPublicDto> GetAll()
    {
        return _context.Users
            .Select(u => new UserPublicDto
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role
            })
            .ToList();
    }


    public UserPublicDto? GetById(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user == null) return null;

        return new UserPublicDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }


    public bool Delete(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return false;

        var vehicles = _context.Vehicles.Where(v => v.UserId == id).ToList();
        var trips = _context.Trips.Where(t => t.UserId == id).ToList();

        _context.Trips.RemoveRange(trips);
        _context.Vehicles.RemoveRange(vehicles);
        _context.Users.Remove(user);
        _context.SaveChanges();

        return true;
    }
    
    public List<Vehicle> GetVehiclesByUser(int userId)
    {
        return _context.Vehicles
            .Where(v => v.UserId == userId)
            .ToList();
    }

    public List<Trip> GetTripsByUser(int userId)
    {
        return _context.Trips
            .Where(t => t.UserId == userId)
            .ToList();
    }
}
