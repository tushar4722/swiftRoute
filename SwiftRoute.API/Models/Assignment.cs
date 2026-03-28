namespace SwiftRoute.API.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public int VehicleId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = "On Route";
    }
}