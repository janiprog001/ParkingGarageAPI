using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ParkingGarageAPI.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        
        // Kapcsolódó entitások id-jei
        public int UserId { get; set; }
        public int CarId { get; set; }
        public int ParkingSpotId { get; set; }
        
        // Foglalás időpontjai
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
        
        // Foglalás státusza (pl. "Függőben", "Jóváhagyott", "Lemondott", "Befejezett")
        [Required]
        public string Status { get; set; }
        
        // Díj
        [Required]
        public decimal TotalFee { get; set; }
        
        // Létrehozás időpontja
        public DateTime CreatedAt { get; set; }
        
        // Navigációs tulajdonságok
        [JsonIgnore]
        public User User { get; set; }
        
        [JsonIgnore]
        public Car Car { get; set; }
        
        [JsonIgnore]
        public ParkingSpot ParkingSpot { get; set; }
    }
} 