namespace SwiftRoute.API.Models
{
    public class VehicleDTO
    {
        public string Model { get; set; } = "";
        public string RequiredClass { get; set; } = "";
        public string PlateNumber { get; set; } = "";
        public int Year { get; set; }
        public int CurrentOdometer { get; set; }
    }

    public class DriverDTO
    {
        public string Name { get; set; } = "";
        public string LicenseNumber { get; set; } = "";
        public string LicenseClass { get; set; } = "";
        public DateTime ExpiryDate { get; set; }
    }
}
