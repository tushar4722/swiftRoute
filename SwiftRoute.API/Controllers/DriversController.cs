using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SwiftRoute.API.Data;
using SwiftRoute.API.Hubs;
using SwiftRoute.API.Models;

namespace SwiftRoute.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriversController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hub;

        public DriversController(AppDbContext context, IHubContext<NotificationHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        // ✅ GET all drivers
        [HttpGet]
        public IActionResult GetAll()
        {
            var drivers = _context.Drivers.ToList();
            return Ok(drivers);
        }

        // ✅ POST create driver
        [HttpPost]
        public async Task<IActionResult> Create(Driver driver)
        {
            if (string.IsNullOrEmpty(driver.Status)) driver.Status = "Available";

            _context.Drivers.Add(driver);
            _context.SaveChanges();

            await _hub.Clients.All.SendAsync("RefreshStats");
            return Ok(driver);
        }

        // ✅ UPDATE driver (ADD THIS)
        [HttpPut("{id}")]
        public IActionResult Update(int id, Driver driver)
        {
            var existing = _context.Drivers.Find(id);
            if (existing == null) return NotFound();

            existing.Name = driver.Name;
            existing.LicenseClass = driver.LicenseClass;
            existing.ExpiryDate = driver.ExpiryDate;
            existing.Status = driver.Status;

            _context.SaveChanges();

            return Ok(existing);
        }


        [HttpDelete("{id}")]
public IActionResult Delete(int id)
{
    var driver = _context.Drivers.Find(id);
    if (driver == null) return NotFound();

    _context.Drivers.Remove(driver);
    _context.SaveChanges();

    return Ok("Driver deleted ✅");
}
    }
}