using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwiftRoute.API.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SwiftRoute.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalDrivers = await _context.Drivers.CountAsync();
            var totalVehicles = await _context.Vehicles.CountAsync();
            
            // Count assignments where EndTime is null (active)
            var activeAssignments = await _context.Assignments.CountAsync(a => a.EndTime == null);
            
            // Maintenance required count
            var maintenanceRequired = await _context.Vehicles.CountAsync(v => v.Status == "Maintenance Required");

            return Ok(new
            {
                TotalDrivers = totalDrivers,
                TotalVehicles = totalVehicles,
                ActiveAssignments = activeAssignments,
                MaintenanceRequired = maintenanceRequired
            });
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetRecentLogs()
        {
            // Join MaintenanceLogs with Vehicles to get model details
            var logs = await _context.MaintenanceLogs
                .OrderByDescending(m => m.ServiceDate)
                .Take(5)
                .Join(_context.Vehicles,
                    log => log.VehicleId,
                    vehicle => vehicle.Id,
                    (log, vehicle) => new
                    {
                        AssetId = $"VH-{vehicle.Id:D4}",
                        ServiceType = log.ServiceType,
                        Status = log.Status,
                        Technician = log.Technician,
                        Date = log.ServiceDate.ToString("MMM dd, HH:mm")
                    })
                .ToListAsync();

            return Ok(logs);
        }
    }
}
