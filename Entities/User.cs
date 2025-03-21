using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ParkingGarageAPI.Entities
{
    public class User
    {
        public User()
        {
            Cars = new List<Car>();
        }
        
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        
        // Admin jogosultság
        public bool IsAdmin { get; set; } = false;

        // Kapcsolat a Car entitással
        public ICollection<Car> Cars { get; set; }
    }
}
