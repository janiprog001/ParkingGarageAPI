using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ParkingGarageAPI.Entities
{
    public class User
    {
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

        // Kapcsolat a Car entitással
        public ICollection<Car> Cars { get; set; }
    }
}
