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
            
            if (!ParkingSpots.Any())
            {
                // Töröljük a meglévő parkolóhelyeket
                ParkingSpots.RemoveRange(ParkingSpots);
                SaveChanges();

                // Reseteljük az ID számlálót MySQL-ben
                Database.ExecuteSqlRaw("ALTER TABLE ParkingSpots AUTO_INCREMENT = 1");
                
                // Létrehozzuk a parkolóhelyeket batch-ekben
                var spots = new List<ParkingSpot>();
                for (int floor = 1; floor <= 3; floor++)
                {
                    for (int row = 0; row < 4; row++)
                    {
                        for (int col = 1; col <= 5; col++)
                        {
                            string spotLetter = ((char)('A' + row)).ToString();
                            string spotNumber = $"{spotLetter}{col:D2}";
                            
                            spots.Add(new ParkingSpot
                            {
                                FloorNumber = floor.ToString(),
                                SpotNumber = spotNumber,
                                IsOccupied = false
                            });
                        }
                    }
                    
                    // Minden emeletenként mentjük a batch-et
                    ParkingSpots.AddRange(spots);
                    SaveChanges();
                    spots.Clear();
                }
            }
        }
    }
}
