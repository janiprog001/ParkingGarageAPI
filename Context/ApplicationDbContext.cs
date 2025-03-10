using System.Text;
using Microsoft.EntityFrameworkCore;
using ParkingGarageAPI.Entities;

namespace ParkingGarageAPI.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
    }
}
