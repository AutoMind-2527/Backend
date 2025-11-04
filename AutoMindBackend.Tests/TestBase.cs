using AutoMindBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMindBackend.Tests;

public abstract class TestBase
{
    protected AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}