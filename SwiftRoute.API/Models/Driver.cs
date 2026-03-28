
using System.ComponentModel.DataAnnotations;
namespace SwiftRoute.API.Models
{
    public class Driver
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LicenseNumber { get; set; } = "TX-000000";
        public string LicenseClass { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}