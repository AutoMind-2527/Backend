namespace AutoMindBackend.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public double FuelConsumption { get; set; }
    
    /// <summary>
    /// Unique code to claim/link this tracker to a user account
    /// </summary>
    public string? TrackerCode { get; set; }
    
    /// <summary>
    /// Whether this tracker has been claimed by a user
    /// </summary>
    public bool IsClaimed { get; set; } = false;
    
    // Foreign Key
    public int UserId { get; set; }
    
    // Navigation Properties
    public User User { get; set; } = null!;
    public List<Trip> Trips { get; set; } = new();

    // Berechnete Properties
    public double TotalFuelConsumed => Trips.Sum(t => t.DistanceKm * FuelConsumption / 100);
    public double TotalDistance => Trips.Sum(t => t.DistanceKm);
}

public class VehicleUpdateDto
{
    public string LicensePlate { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public double FuelConsumption { get; set; }
}

public class ClaimTrackerDto
{
    public string TrackerCode { get; set; } = string.Empty;
}