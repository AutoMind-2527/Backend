using AutoMindBackend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core mit SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=automind.db"));

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
