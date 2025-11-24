namespace AutoMindBackend.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public double FuelConsumption { get; set; }
    
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