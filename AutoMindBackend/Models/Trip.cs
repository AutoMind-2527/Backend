namespace AutoMindBackend.Models;

public class Trip
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double DistanceKm { get; set; }
    public string StartLocation { get; set; } = string.Empty;
    public string EndLocation { get; set; } = string.Empty;

    public int VehicleId { get; set; }
    
    public double FuelUsed { get; set; }        
    public double TripCost { get; set; }        
    public double AverageSpeed { get; set; } 

    public int UserId { get; set; }   
}
