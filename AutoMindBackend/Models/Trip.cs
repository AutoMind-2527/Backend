namespace AutoMindBackend.Models;

public class Trip
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double DistanceKm { get; set; }
    public string StartLocation { get; set; } = string.Empty;
    public string EndLocation { get; set; } = string.Empty;
    
    // Foreign Keys
    public int VehicleId { get; set; }
    public int UserId { get; set; } // ✅ User-ForeignKey hinzufügen
    
    // Navigation Properties
    public Vehicle Vehicle { get; set; } = null!;
    public User User { get; set; } = null!; // ✅ User-Navigation Property hinzufügen
}
