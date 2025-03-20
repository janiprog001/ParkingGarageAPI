using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ParkingGarageAPI.Entities
{
    public class ParkingSpot
    {
        public int Id { get; set; }
        
        [Required]
        public string FloorNumber { get; set; }
        
        [Required]
        public string SpotNumber { get; set; }
        
        public bool IsOccupied { get; set; } = false;
        
        // Kapcsolat az autóval (ha van)
        public int? CarId { get; set; }
        
        [JsonIgnore]
        public Car? Car { get; set; }
        
        // Parkolás kezdete és vége
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
} 