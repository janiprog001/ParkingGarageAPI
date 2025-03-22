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
        public DbSet<ParkingSpot> ParkingSpots { get; set; }
        public DbSet<ParkingHistory> ParkingHistories { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        // Az entitások közötti kapcsolat konfigurálása
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Car>()
                .HasOne(c => c.User)
                .WithMany(u => u.Cars)
                .HasForeignKey(c => c.UserId);
            
            modelBuilder.Entity<ParkingSpot>()
                .HasOne(p => p.Car)
                .WithOne(c => c.ParkingSpot)
                .HasForeignKey<ParkingSpot>(p => p.CarId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
            
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.ParkingHistory)
                .WithMany()
                .HasForeignKey(i => i.ParkingHistoryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        
        // Seed metódus
        public void Seed()
        {
            // Ellenőrizzük, hogy az adatbázis létrejött-e
            Database.EnsureCreated();
            
            if (!Users.Any())
            {
                // Normál felhasználó
                var user = new User
                {
                    FirstName = "John",
                    LastName = "Doe",
                    PhoneNumber = "1234567890",
                    Email = "john.doe@example.com",
                    PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("Start123")),
                    IsAdmin = false
                };
                
                // Admin felhasználó
                var admin = new User
                {
                    FirstName = "Admin",
                    LastName = "User",
                    PhoneNumber = "0987654321",
                    Email = "admin@example.com",
                    PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("Admin123")),
                    IsAdmin = true
                };
                
                Users.Add(user);
                Users.Add(admin);
                SaveChanges();
                
                Cars.Add(
                    new Car
                    {
                        Brand = "Toyota",
                        Model = "Corolla",
                        Year = 2020,
                        LicensePlate = "ABC-123",
                        UserId = user.Id
                    }
                );
                SaveChanges();
            }
            
            // ParkingSpot-ok hozzáadása, ha még nincsenek
            if (!ParkingSpots.Any())
            {
                for (int floor = 1; floor <= 2; floor++)
                {
                    for (int spot = 1; spot <= 10; spot++)
                    {
                        ParkingSpots.Add(new ParkingSpot
                        {
                            FloorNumber = floor.ToString(),
                            SpotNumber = spot.ToString("D2"),
                            IsOccupied = false
                        });
                    }
                }
                SaveChanges();
            }
        }
    }
}
