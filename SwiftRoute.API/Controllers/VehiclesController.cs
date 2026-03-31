using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SwiftRoute.API.Data;
using SwiftRoute.API.Hubs;
using SwiftRoute.API.Models;

namespace SwiftRoute.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hub;

        public VehiclesController(AppDbContext context, IHubContext<NotificationHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        // ✅ EXISTING GET
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.Vehicles.ToList());
        }

        // ✅ EXISTING POST UPGRADED
        [HttpPost]
        public async Task<IActionResult> Create(VehicleDTO vDto)
        {
            var v = new Vehicle
            {
                Model = vDto.Model,
                RequiredClass = string.IsNullOrEmpty(vDto.RequiredClass) ? "Class A" : vDto.RequiredClass,
                PlateNumber = vDto.PlateNumber,
                Year = vDto.Year,
                CurrentOdometer = vDto.CurrentOdometer,
                Status = "Active",
                LastServiceOdometer = vDto.CurrentOdometer
            };

            _context.Vehicles.Add(v);
            _context.SaveChanges();
            
            await _hub.Clients.All.SendAsync("RefreshStats");
            return Ok(v);
        }

        // 🔥 ADD ODOMETER UPDATE API
        [HttpPut("{id}/odometer")]
        public async Task<IActionResult> UpdateOdometer(int id, [FromBody] int newOdometer)
        {
            var vehicle = _context.Vehicles.Find(id);
            if (vehicle == null) return NotFound("Vehicle not found");

            if (newOdometer < vehicle.CurrentOdometer) 
                return BadRequest("Odometer cannot be decreased.");

            vehicle.CurrentOdometer = newOdometer;
            
            // 🚨 Check 10k Maintenance Threshold Live
            if (vehicle.Status == "Active" && (vehicle.CurrentOdometer - vehicle.LastServiceOdometer) >= 10000)
            {
                vehicle.Status = "Maintenance Required";
                await _hub.Clients.All.SendAsync("ReceiveAlert", $"🚨 Vehicle {vehicle.Model} hit 10k limit and is locked for maintenance!");
            }

            _context.SaveChanges();
            await _hub.Clients.All.SendAsync("RefreshStats");
            return Ok(vehicle);
        }

        // 🔥 EXISTING SERVICE VEHICLE
        [HttpPost("service")]
        public async Task<IActionResult> ServiceVehicle(int vehicleId)
        {
            var vehicle = _context.Vehicles.Find(vehicleId);
            if (vehicle == null)
                return NotFound();

            var log = new MaintenanceLog
            {
                VehicleId = vehicleId,
                ServiceDate = DateTime.Now,
                Odometer = vehicle.CurrentOdometer,
                ServiceType = "General Service",
                Status = "COMPLETED",
                Technician = "Auto-Assigned"
            };

            vehicle.LastServiceOdometer = vehicle.CurrentOdometer;
            vehicle.Status = "Active";

            _context.MaintenanceLogs.Add(log);
            _context.SaveChanges();

            await _hub.Clients.All.SendAsync("RefreshStats");
            return Ok("Vehicle serviced and activated ✅");
        }
    }
}