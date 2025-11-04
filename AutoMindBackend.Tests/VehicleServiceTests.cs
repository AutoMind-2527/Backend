using AutoMindBackend.Models;
using AutoMindBackend.Services;
using FluentAssertions;
using Xunit;

namespace AutoMindBackend.Tests;

public class VehicleServiceTests : TestBase
{
    [Fact]
    public void Add_ShouldSaveVehicle()
    {
        var ctx = CreateContext();
        var service = new VehicleService(ctx);
        var vehicle = new Vehicle { LicensePlate = "L-123AA", Brand = "BMW", Model = "i4", Mileage = 1000, FuelConsumption = 6.5 };

        var result = service.Add(vehicle);

        result.Id.Should().BeGreaterThan(0);
        ctx.Vehicles.Count().Should().Be(1);
    }

    [Fact]
    public void AddMileage_ShouldIncreaseMileage()
    {
        var ctx = CreateContext();
        var v = new Vehicle { Id = 1, LicensePlate = "L-999AA", Mileage = 1000 };
        ctx.Vehicles.Add(v);
        ctx.SaveChanges();

        var service = new VehicleService(ctx);
        service.AddMileage(1, 250);

        ctx.Vehicles.Find(1)!.Mileage.Should().Be(1250);
    }

    [Theory]
    [InlineData(14900, false)]
    [InlineData(15000, true)]
    [InlineData(16000, true)]
    [InlineData(15100, true)]
    public void NeedsService_ShouldReturnExpected(double mileage, bool expected)
    {
        var ctx = CreateContext();
        ctx.Vehicles.Add(new Vehicle { Id = 1, Mileage = mileage });
        ctx.SaveChanges();

        var service = new VehicleService(ctx);
        var result = service.NeedsService(1);

        result.Should().Be(expected);
    }
}
