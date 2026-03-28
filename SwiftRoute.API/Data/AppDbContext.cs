using Microsoft.EntityFrameworkCore;
using SwiftRoute.API.Models;

namespace SwiftRoute.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Assignment> Assignments { get; set; }

        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
    }
}