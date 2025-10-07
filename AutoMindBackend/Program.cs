using Microsoft.EntityFrameworkCore;
using AutoMindBackend.Data;
using AutoMindBackend.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<VehicleService>();

builder.Services.AddScoped<TripService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=automind.db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
