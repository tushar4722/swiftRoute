using Microsoft.EntityFrameworkCore;
using SwiftRoute.API.Data;
using SwiftRoute.API.Hubs; // ✅ IMPORTANT
using SwiftRoute.API.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Services
builder.Services.AddControllers();
builder.Services.AddSignalR(); // ✅ SignalR

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.WithOrigins("http://localhost:5173") // 👈 frontend URL
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials()); // 👈 IMPORTANT
});

// 🔹 Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

var app = builder.Build();

// 🔹 Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// ✅ SignalR endpoint
app.MapHub<NotificationHub>("/hub/notifications");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // 🔹 DRIVERS
    if (!db.Drivers.Any())
    {
        db.Drivers.AddRange(
            new Driver { Name = "Julian D'Amico", LicenseNumber = "TX-492019-Z", LicenseClass = "Class A (CDL)", ExpiryDate = DateTime.Parse("2026-11-12"), Status = "Available" },
            new Driver { Name = "Sarah Lorenz", LicenseNumber = "CA-882193-L", LicenseClass = "Class B", ExpiryDate = DateTime.Parse("2024-01-04"), Status = "Expired" },
            new Driver { Name = "Marcus Wu", LicenseNumber = "NY-220194-P", LicenseClass = "Class A (CDL)", ExpiryDate = DateTime.Parse("2025-08-28"), Status = "Available" },
            new Driver { Name = "Brenda Knight", LicenseNumber = "FL-334912-A", LicenseClass = "Class B", ExpiryDate = DateTime.Parse("2024-04-15"), Status = "Available" },
            new Driver { Name = "Marcus Thorne", LicenseNumber = "WA-992120-C", LicenseClass = "Class A", ExpiryDate = DateTime.Parse("2025-10-21"), Status = "Assigned" },
            new Driver { Name = "Elena Rodriguez", LicenseNumber = "NM-821034-L", LicenseClass = "Class A", ExpiryDate = DateTime.Parse("2026-02-14"), Status = "Assigned" }
        );
        db.SaveChanges();
    }

    // 🔹 VEHICLES
    if (!db.Vehicles.Any())
    {
        db.Vehicles.AddRange(
            new Vehicle { PlateNumber = "SR-7721-TX", Year = 2023, Model = "Freightliner Cascadia", CurrentOdometer = 142500, LastServiceOdometer = 130000, Status = "Maintenance Required", RequiredClass = "Class A" },
            new Vehicle { PlateNumber = "SR-4092-NY", Year = 2022, Model = "Volvo VNL 860", CurrentOdometer = 88120, LastServiceOdometer = 80000, Status = "Active", RequiredClass = "Class A" },
            new Vehicle { PlateNumber = "SR-9931-CA", Year = 2024, Model = "Peterbilt 579", CurrentOdometer = 12400, LastServiceOdometer = 12000, Status = "Assigned", RequiredClass = "Class B" },
            new Vehicle { PlateNumber = "SR-1120-LA", Year = 2021, Model = "Mack Anthem", CurrentOdometer = 50000, LastServiceOdometer = 50000, Status = "Active", RequiredClass = "Class A" }
        );
        db.SaveChanges();
    }

    if (!db.MaintenanceLogs.Any())
    {
        var firstVehicleId = db.Vehicles.First().Id;
        db.MaintenanceLogs.AddRange(
            new MaintenanceLog { VehicleId = firstVehicleId, ServiceDate = DateTime.Now.AddDays(-6), Odometer = 41000, ServiceType = "Brake Calibration", Status = "COMPLETED", Technician = "James Miller" },
            new MaintenanceLog { VehicleId = firstVehicleId, ServiceDate = DateTime.Now.AddDays(-5), Odometer = 41500, ServiceType = "Transmission Sync", Status = "IN PROGRESS", Technician = "Sarah Chen" },
            new MaintenanceLog { VehicleId = firstVehicleId, ServiceDate = DateTime.Now.AddDays(15), Odometer = 42000, ServiceType = "Tire Rotation", Status = "SCHEDULED", Technician = "Robert Ford" }
        );
        db.SaveChanges();
    }

    // 🔹 ASSIGNMENTS
    if (!db.Assignments.Any())
    {
        var d1 = db.Drivers.First(d => d.Name == "Marcus Thorne").Id;
        var v1 = db.Vehicles.First(v => v.Model.Contains("Volvo")).Id;

        var d2 = db.Drivers.First(d => d.Name == "Elena Rodriguez").Id;
        var v2 = db.Vehicles.First(v => v.Model.Contains("Mack")).Id;

        db.Assignments.AddRange(
            new Assignment { DriverId = d1, VehicleId = v1, StartTime = DateTime.Now.AddHours(-4).AddMinutes(-20), Status = "On Route" },
            new Assignment { DriverId = d2, VehicleId = v2, StartTime = DateTime.Now.AddHours(-0).AddMinutes(-15), Status = "Staging" }
        );
        db.SaveChanges();
    }
}
app.Run();