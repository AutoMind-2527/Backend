using Microsoft.EntityFrameworkCore;
using AutoMindBackend.Data;

var builder = WebApplication.CreateBuilder(args);

// --- SERVICES registrieren ---
builder.Services.AddControllers();

// Swagger aktivieren
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB-Kontext registrieren (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=automind.db"));

var app = builder.Build();

// --- PIPELINE konfigurieren ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
