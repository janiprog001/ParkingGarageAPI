using System.ComponentModel.DataAnnotations;

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

        // Kapcsolat a User entitu00e1ssal
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
