public class MaintenanceLog
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime ServiceDate { get; set; }
    public int Odometer { get; set; }
    
    public string ServiceType { get; set; } = "General Inspection";
    public string Status { get; set; } = "COMPLETED";
    public string Technician { get; set; } = "Pending";
}