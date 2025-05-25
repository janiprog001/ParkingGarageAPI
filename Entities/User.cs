using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ParkingGarageAPI.Entities
{
    public class User
    {
        public User()
        {
            Cars = new List<Car>();
        }
        
        [JsonIgnore]
        public int Id { get; set; }
        
        [Required]
        [JsonPropertyName("firstName")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string FirstName { get; set; }
        
        [Required]
        [JsonPropertyName("lastName")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string LastName { get; set; }
        
        [Required]
        [JsonPropertyName("phoneNumber")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PhoneNumber { get; set; }
        
        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Email { get; set; }
        
        [Required]
        [JsonPropertyName("passwordHash")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PasswordHash { get; set; }
        
        // Admin jogosultság
        [JsonIgnore]
        public bool IsAdmin { get; set; } = false;

        // Kapcsolat a Car entitással
        [JsonPropertyName("cars")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<Car> Cars { get; set; }

        // Kapcsolat az Invoice entitással
        [JsonIgnore]
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        // Kapcsolat a ParkingHistory entitással
        [JsonIgnore]
        public ICollection<ParkingHistory> ParkingHistories { get; set; } = new List<ParkingHistory>();

        // Kapcsolat a Reservation entitással
        [JsonIgnore]
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
