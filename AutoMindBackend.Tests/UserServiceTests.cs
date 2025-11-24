using AutoMindBackend.Models;
using AutoMindBackend.Services;
using FluentAssertions;
using Xunit;

namespace AutoMindBackend.Tests;

public class UserServiceTests : TestBase
{
    [Fact]
    public void GetAll_ShouldReturnUserPublicDtos()
    {
        var ctx = CreateContext();
        ctx.Users.AddRange(
            new User { Id = 1, Username = "andrej", Role = "Admin" },
            new User { Id = 2, Username = "vanja", Role = "User" }
        );
        ctx.SaveChanges();

        var service = new UserService(ctx);

        var result = service.GetAll();

        result.Should().HaveCount(2);
        result.Select(u => u.Username).Should().BeEquivalentTo("andrej", "vanja");
        result.Any(u => u.Id == 1 && u.Role == "Admin").Should().BeTrue();
    }

    [Fact]
    public void GetById_ShouldReturnDto_WhenUserExists()
    {
        var ctx = CreateContext();
        ctx.Users.Add(new User { Id = 5, Username = "andrej", Role = "Admin" });
        ctx.SaveChanges();

        var service = new UserService(ctx);

        var result = service.GetById(5);

        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
        result.Username.Should().Be("andrej");
        result.Role.Should().Be("Admin");
    }

    [Fact]
    public void GetById_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var ctx = CreateContext();
        var service = new UserService(ctx);

        var result = service.GetById(999);

        result.Should().BeNull();
    }

    [Fact]
    public void Delete_ShouldRemoveUserAndRelatedVehiclesAndTrips()
    {
        var ctx = CreateContext();

        var user = new User { Id = 1, Username = "andrej", Role = "User" };
        ctx.Users.Add(user);
        ctx.Vehicles.Add(new Vehicle { Id = 10, UserId = 1, LicensePlate = "L-111AA" });
        ctx.Trips.Add(new Trip { Id = 20, UserId = 1, VehicleId = 10, DistanceKm = 50 });
        ctx.SaveChanges();

        var service = new UserService(ctx);

        var deleted = service.Delete(1);

        deleted.Should().BeTrue();
        ctx.Users.Should().BeEmpty();
        ctx.Vehicles.Should().BeEmpty();
        ctx.Trips.Should().BeEmpty();
    }

    [Fact]
    public void GetVehiclesAndTripsByUser_ShouldReturnOnlyUsersData()
    {
        var ctx = CreateContext();

        ctx.Users.AddRange(
            new User { Id = 1, Username = "andrej", Role = "User" },
            new User { Id = 2, Username = "vanja", Role = "User" }
        );

        ctx.Vehicles.AddRange(
            new Vehicle { Id = 10, UserId = 1, LicensePlate = "L-111AA" },
            new Vehicle { Id = 11, UserId = 2, LicensePlate = "L-222BB" }
        );

        ctx.Trips.AddRange(
            new Trip { Id = 20, UserId = 1, VehicleId = 10, DistanceKm = 100 },
            new Trip { Id = 21, UserId = 2, VehicleId = 11, DistanceKm = 200 }
        );

        ctx.SaveChanges();

        var service = new UserService(ctx);

        var vehiclesUser1 = service.GetVehiclesByUser(1);
        var tripsUser1 = service.GetTripsByUser(1);

        vehiclesUser1.Should().HaveCount(1);
        vehiclesUser1[0].Id.Should().Be(10);

        tripsUser1.Should().HaveCount(1);
        tripsUser1[0].Id.Should().Be(20);
    }
}
