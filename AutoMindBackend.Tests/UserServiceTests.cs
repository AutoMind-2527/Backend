using AutoMindBackend.Models;
using AutoMindBackend.Services;
using FluentAssertions;
using Xunit;

namespace AutoMindBackend.Tests;

public class UserServiceTests : TestBase
{
    [Fact]
    public void Delete_ShouldRemoveUserAndRelations()
    {
        var ctx = CreateContext();
        var user = new User { Id = 1, Username = "andrej", Role = "User", PasswordHash = [1], PasswordSalt = [1] };
        ctx.Users.Add(user);
        ctx.Vehicles.Add(new Vehicle { Id = 1, UserId = 1 });
        ctx.Trips.Add(new Trip { Id = 1, UserId = 1 });
        ctx.SaveChanges();

        var service = new UserService(ctx);
        service.Delete(1).Should().BeTrue();

        ctx.Users.Should().BeEmpty();
        ctx.Vehicles.Should().BeEmpty();
        ctx.Trips.Should().BeEmpty();
    }

    [Fact]
    public void GetVehiclesByUser_ShouldReturnCorrectVehicles()
    {
        var ctx = CreateContext();
        ctx.Vehicles.AddRange(
            new Vehicle { Id = 1, UserId = 1 },
            new Vehicle { Id = 2, UserId = 2 }
        );
        ctx.SaveChanges();

        var service = new UserService(ctx);
        service.GetVehiclesByUser(1).Should().HaveCount(1);
    }
}
