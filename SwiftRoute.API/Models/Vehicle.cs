using System.ComponentModel.DataAnnotations;

public class Vehicle
{
    public int Id { get; set; }
    public string Model { get; set; } = "";
    public int CurrentOdometer { get; set; }
    public int LastServiceOdometer { get; set; }
    public string Status { get; set; } = "";

    public string RequiredClass { get; set; } = "";
    public string PlateNumber { get; set; } = "SR-0000-XX";
    public int Year { get; set; } = 2024;

    [Timestamp]
    public byte[] RowVersion { get; set; } // 🔥 IMPORTANT
}