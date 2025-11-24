namespace AutoMindBackend.Models;

public class User
{
    public int Id { get; set; }
    public string KeycloakId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string PasswordHash { get; set; } = string.Empty;
    
    // ✅ Falls PasswordSalt benötigt wird:
    public string PasswordSalt { get; set; } = string.Empty;
    
    // Navigation Properties
    public List<Vehicle> Vehicles { get; set; } = new();
    public List<Trip> Trips { get; set; } = new();
}