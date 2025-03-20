using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ParkingGarageAPI.Entities
{
    public class Car
    {
        public int Id { get; set; }
        [Required]
        public string Brand { get; set; }
        [Required]
        public string Model { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public string LicensePlate { get; set; }

        // Kapcsolat a User entitással
        public int UserId { get; set; }
        
        [JsonIgnore] // Megakadályozza a körkörös referenciát JSON szerializáláskor
        public User? User { get; set; }
    }
}
