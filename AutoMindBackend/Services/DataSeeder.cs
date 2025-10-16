using AutoMindBackend.Models;

namespace AutoMindBackend.Services;

public class DataSeeder
{
    private readonly VehicleService _vehicleService;
    private readonly TripService _tripService;

    public DataSeeder(VehicleService vehicleService, TripService tripService)
    {
        _vehicleService = vehicleService;
        _tripService = tripService;
    }

    public void SeedData()
    {
        if (!_vehicleService.GetAll().Any())
        {
            var car = _vehicleService.Add(new Vehicle
            {
                LicensePlate = "L-111AA",
                Brand = "BMW",
                Model = "i4",
                Mileage = 12000,
                FuelConsumption = 16.5
            });

            _tripService.Add(new Trip
            {
                StartTime = DateTime.Now.AddHours(-3),
                EndTime = DateTime.Now.AddHours(-2),
                DistanceKm = 80,
                StartLocation = "Linz",
                EndLocation = "Wien",
                VehicleId = car.Id
            });

            Console.WriteLine("Beispiel-Daten erfolgreich eingefügt!");
        }
    }
}
