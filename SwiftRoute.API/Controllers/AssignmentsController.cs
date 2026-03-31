using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SwiftRoute.API.Data;
using SwiftRoute.API.Models;
using SwiftRoute.API.Hubs;
using System;
using System.Linq;

namespace SwiftRoute.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hub;

        public AssignmentsController(AppDbContext context, IHubContext<NotificationHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        [HttpGet("active")]
        public IActionResult GetActive()
        {
            var res = _context.Assignments
                .Where(a => a.EndTime == null)
                .Join(_context.Drivers, a => a.DriverId, d => d.Id, (a, d) => new { a, d })
                .Join(_context.Vehicles, ad => ad.a.VehicleId, v => v.Id, (ad, v) => new
                {
                    Id = ad.a.Id,
                    DriverId = ad.d.Id,
                    DriverName = ad.d.Name,
                    VehicleId = v.Id,
                    VehicleModel = v.Model,
                    PlateNumber = v.PlateNumber,
                    StartTime = ad.a.StartTime,
                    Status = ad.a.Status
                })
                .ToList();

            return Ok(res);
        }

        [HttpPost]
public async Task<IActionResult> Assign(int driverId, int vehicleId)
{
    var driver = _context.Drivers.Find(driverId);
    var vehicle = _context.Vehicles.Find(vehicleId);

    if (driver == null || vehicle == null)
        return BadRequest("Invalid driver or vehicle");

    if (driver.LicenseClass != vehicle.RequiredClass)
    return BadRequest("Driver license does not match vehicle type");
    
    if (_context.Assignments.Any(a => a.DriverId == driverId && a.EndTime == null))
        return BadRequest("Driver already assigned");

    if (_context.Assignments.Any(a => a.VehicleId == vehicleId && a.EndTime == null))
        return BadRequest("Vehicle already assigned");

    if (driver.ExpiryDate < DateTime.Now)
        return BadRequest("License expired");

    if (vehicle.Status != "Active")
        return BadRequest("Vehicle not available");

        

    // 🔥 Maintenance Rule + SignalR + Concurrency
    if (vehicle.CurrentOdometer - vehicle.LastServiceOdometer >= 10000)
    {
        try
        {
            vehicle.Status = "Maintenance Required";
            _context.SaveChanges();

            await _hub.Clients.All.SendAsync("ReceiveAlert",
                $"🚨 Vehicle {vehicle.Model} needs maintenance!");

            return BadRequest("Vehicle needs maintenance");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("⚠️ Vehicle was updated by another user. Try again.");
        }
    }

    var assignment = new Assignment
    {
        DriverId = driverId,
        VehicleId = vehicleId,
        StartTime = DateTime.Now
    };

    try
    {
        driver.Status = "Assigned";
        vehicle.Status = "In-Transit";

        _context.Assignments.Add(assignment);
        _context.SaveChanges();
        
        await _hub.Clients.All.SendAsync("RefreshStats");
    }
    catch (DbUpdateConcurrencyException)
    {
        return Conflict("⚠️ Assignment conflict occurred. The driver or vehicle was just updated by another user.");
    }

    return Ok(assignment);
}

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var assignment = _context.Assignments.Find(id);
            if (assignment == null) return NotFound("Assignment not found");
            if (assignment.EndTime != null) return BadRequest("Assignment already completed");

            var driver = _context.Drivers.Find(assignment.DriverId);
            var vehicle = _context.Vehicles.Find(assignment.VehicleId);

            if (driver != null) driver.Status = "Available";
            if (vehicle != null) vehicle.Status = "Active";
            
            assignment.EndTime = DateTime.Now;
            assignment.Status = "Completed";

            _context.SaveChanges();
            await _hub.Clients.All.SendAsync("RefreshStats");

            return Ok(assignment);
        }
    }
}