using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ParkingGarageAPI.Context;
using ParkingGarageAPI.Entities;
using System.Security.Claims;

namespace ParkingGarageAPI.Controller;

[Route("api/reservations")]
[ApiController]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public ReservationController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Foglalás létrehozása
    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
    {
        try
        {
            if (request == null || request.CarId <= 0 || request.ParkingSpotId <= 0)
                return BadRequest("Érvénytelen kérés");
                
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users
                .Include(u => u.Cars)
                .FirstOrDefaultAsync(u => u.Email == userEmail);
                
            if (user == null)
                return NotFound("Felhasználó nem található");
                
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == request.CarId && c.UserId == user.Id);
            if (car == null)
                return NotFound("Az autó nem található vagy nem a tiéd");
                
            var parkingSpot = await _context.ParkingSpots.FirstOrDefaultAsync(p => p.Id == request.ParkingSpotId);
            if (parkingSpot == null)
                return NotFound("A parkolóhely nem található");
                
            if (parkingSpot.IsOccupied)
                return BadRequest("A parkolóhely már foglalt");
                
            if (car.IsParked)
                return BadRequest("Az autó már le van parkolva");
                
            // Ellenőrizzük, hogy az időpontok érvényesek-e
            if (request.StartTime >= request.EndTime)
                return BadRequest("A kezdő időpont nem lehet későbbi, mint a befejező időpont");
                
            if (request.StartTime < DateTime.Now)
                return BadRequest("A kezdő időpont nem lehet korábbi, mint a jelenlegi idő");
                
            // Új foglalás létrehozása
            var reservation = new Reservation
            {
                UserId = user.Id,
                CarId = car.Id,
                ParkingSpotId = parkingSpot.Id,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = "Jóváhagyott", // vagy "Függőben", ha admin jóváhagyás kell
                TotalFee = CalculateFee(request.StartTime, request.EndTime), // Díj kiszámítása
                CreatedAt = DateTime.Now
            };
            
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            
            return Ok(new {
                message = "Foglalás sikeresen létrehozva",
                reservationId = reservation.Id,
                startTime = reservation.StartTime,
                endTime = reservation.EndTime,
                totalFee = reservation.TotalFee,
                status = reservation.Status
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Felhasználó saját foglalásainak lekérdezése
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReservations()
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                
            if (user == null)
                return NotFound("Felhasználó nem található");
                
            var reservations = await _context.Reservations
                .Where(r => r.UserId == user.Id)
                .Include(r => r.ParkingSpot)
                .OrderByDescending(r => r.StartTime)
                .Select(r => new
                {
                    id = r.Id,
                    startTime = r.StartTime,
                    endTime = r.EndTime,
                    status = r.Status,
                    totalFee = r.TotalFee,
                    floorNumber = r.ParkingSpot.FloorNumber,
                    spotNumber = r.ParkingSpot.SpotNumber
                })
                .ToListAsync();
                
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Specifikus foglalás lekérdezése
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReservationById(int id)
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                
            if (user == null)
                return NotFound("Felhasználó nem található");
            
            Reservation reservation;
            
            // Admin láthatja bárki foglalását, normál felhasználó csak a sajátját
            if (user.IsAdmin)
            {
                reservation = await _context.Reservations
                    .Include(r => r.ParkingSpot)
                    .Include(r => r.Car)
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            else
            {
                reservation = await _context.Reservations
                    .Include(r => r.ParkingSpot)
                    .Include(r => r.Car)
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);
            }
            
            if (reservation == null)
                return NotFound("A foglalás nem található vagy nincs jogosultságod a megtekintéséhez");
                
            return Ok(new
            {
                id = reservation.Id,
                startTime = reservation.StartTime,
                endTime = reservation.EndTime,
                status = reservation.Status,
                totalFee = reservation.TotalFee,
                createdAt = reservation.CreatedAt,
                floorNumber = reservation.ParkingSpot.FloorNumber,
                spotNumber = reservation.ParkingSpot.SpotNumber,
                car = new
                {
                    id = reservation.Car.Id,
                    brand = reservation.Car.Brand,
                    model = reservation.Car.Model,
                    licensePlate = reservation.Car.LicensePlate
                },
                user = user.IsAdmin ? new
                {
                    id = reservation.User.Id,
                    name = $"{reservation.User.FirstName} {reservation.User.LastName}",
                    email = reservation.User.Email
                } : null
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Foglalás lemondása/törlése
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelReservation(int id)
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                
            if (user == null)
                return NotFound("Felhasználó nem található");
            
            Reservation reservation;
            
            // Admin törölheti bárki foglalását, normál felhasználó csak a sajátját
            if (user.IsAdmin)
            {
                reservation = await _context.Reservations.FindAsync(id);
            }
            else
            {
                reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);
            }
            
            if (reservation == null)
                return NotFound("A foglalás nem található vagy nincs jogosultságod a törléséhez");
                
            // Ellenőrizzük, hogy a foglalás már megkezdődött-e
            if (reservation.StartTime <= DateTime.Now)
                return BadRequest("A már megkezdett foglalást nem lehet lemondani");
                
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Foglalás sikeresen törölve" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Adminok számára: összes foglalás lekérdezése
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAllReservations()
    {
        try
        {
            var reservations = await _context.Reservations
                .Include(r => r.ParkingSpot)
                .Include(r => r.User)
                .Include(r => r.Car)
                .OrderByDescending(r => r.StartTime)
                .Select(r => new
                {
                    id = r.Id,
                    startTime = r.StartTime,
                    endTime = r.EndTime,
                    status = r.Status,
                    totalFee = r.TotalFee,
                    floorNumber = r.ParkingSpot.FloorNumber,
                    spotNumber = r.ParkingSpot.SpotNumber,
                    user = new
                    {
                        id = r.User.Id,
                        name = $"{r.User.FirstName} {r.User.LastName}",
                        email = r.User.Email
                    },
                    car = new
                    {
                        id = r.Car.Id,
                        brand = r.Car.Brand,
                        model = r.Car.Model,
                        licensePlate = r.Car.LicensePlate
                    }
                })
                .ToListAsync();
                
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Adott felhasználó foglalásainak lekérdezése (admin jogosultság)
    [HttpGet("user/{userId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetUserReservations(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Felhasználó nem található");
                
            var reservations = await _context.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.ParkingSpot)
                .Include(r => r.Car)
                .OrderByDescending(r => r.StartTime)
                .Select(r => new
                {
                    id = r.Id,
                    startTime = r.StartTime,
                    endTime = r.EndTime,
                    status = r.Status,
                    totalFee = r.TotalFee,
                    floorNumber = r.ParkingSpot.FloorNumber,
                    spotNumber = r.ParkingSpot.SpotNumber,
                    car = new
                    {
                        id = r.Car.Id,
                        brand = r.Car.Brand,
                        model = r.Car.Model,
                        licensePlate = r.Car.LicensePlate
                    }
                })
                .ToListAsync();
                
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Felhasználó autóihoz tartozó foglalások lekérdezése
    [HttpGet("mycars")]
    public async Task<IActionResult> GetMyCarReservations()
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users
                .Include(u => u.Cars)
                .FirstOrDefaultAsync(u => u.Email == userEmail);
                
            if (user == null)
                return NotFound("Felhasználó nem található");
                
            var carIds = user.Cars.Select(c => c.Id).ToList();
            
            var reservations = await _context.Reservations
                .Where(r => carIds.Contains(r.CarId))
                .Include(r => r.ParkingSpot)
                .Include(r => r.Car)
                .OrderByDescending(r => r.StartTime)
                .Select(r => new
                {
                    id = r.Id,
                    startTime = r.StartTime,
                    endTime = r.EndTime,
                    status = r.Status,
                    totalFee = r.TotalFee,
                    floorNumber = r.ParkingSpot.FloorNumber,
                    spotNumber = r.ParkingSpot.SpotNumber,
                    car = new
                    {
                        id = r.Car.Id,
                        brand = r.Car.Brand,
                        model = r.Car.Model,
                        licensePlate = r.Car.LicensePlate
                    }
                })
                .ToListAsync();
                
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Parkolási díj kiszámítása
    private decimal CalculateFee(DateTime startTime, DateTime endTime)
    {
        // Példa díjszámítás: 500 Ft/óra
        decimal hourlyRate = 500m;
        decimal hours = (decimal)(endTime - startTime).TotalHours;
        
        return Math.Ceiling(hours) * hourlyRate;
    }
}

// Request modellek
public class CreateReservationRequest
{
    public int CarId { get; set; }
    public int ParkingSpotId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
} 