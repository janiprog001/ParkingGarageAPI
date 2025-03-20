using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ParkingGarageAPI.Context;
using ParkingGarageAPI.Entities;
using System.Security.Claims;

namespace ParkingGarageAPI.Controller;

[Route("api/parking")]
[ApiController]
[Authorize]
public class ParkingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public ParkingController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Szabad parkolóhelyek lekérdezése
    [HttpGet("spots/available")]
    public IActionResult GetAvailableSpots()
    {
        try
        {
            var availableSpots = _context.ParkingSpots
                .Where(p => !p.IsOccupied)
                .ToList();
                
            return Ok(availableSpots);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Összes parkolóhely lekérdezése
    [HttpGet("spots")]
    public IActionResult GetAllSpots()
    {
        try
        {
            var spots = _context.ParkingSpots.ToList();
            return Ok(spots);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Parkolás kezdése
    [HttpPost("start")]
    public IActionResult StartParking([FromBody] StartParkingRequest request)
    {
        try
        {
            if (request == null || request.CarId <= 0 || request.ParkingSpotId <= 0)
                return BadRequest("Érvénytelen kérés");
                
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var user = _context.Users
                .Include(u => u.Cars)
                .FirstOrDefault(u => u.Email == userEmail);
                
            if (user == null)
                return NotFound("Felhasználó nem található");
                
            var car = _context.Cars.FirstOrDefault(c => c.Id == request.CarId && c.UserId == user.Id);
            if (car == null)
                return NotFound("Az autó nem található vagy nem a tiéd");
                
            var parkingSpot = _context.ParkingSpots.FirstOrDefault(p => p.Id == request.ParkingSpotId);
            if (parkingSpot == null)
                return NotFound("A parkolóhely nem található");
                
            if (parkingSpot.IsOccupied)
                return BadRequest("A parkolóhely már foglalt");
                
            if (car.IsParked)
                return BadRequest("Az autó már le van parkolva");
                
            // Parkolás kezdése
            parkingSpot.IsOccupied = true;
            parkingSpot.CarId = car.Id;
            parkingSpot.StartTime = DateTime.Now;
            parkingSpot.EndTime = null;
            
            car.IsParked = true;
            
            _context.SaveChanges();
            
            return Ok(new {
                message = "Parkolás elkezdve",
                startTime = parkingSpot.StartTime,
                floor = parkingSpot.FloorNumber,
                spot = parkingSpot.SpotNumber
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Parkolás befejezése
    [HttpPost("end")]
    public IActionResult EndParking([FromBody] EndParkingRequest request)
    {
        try
        {
            if (request == null || request.CarId <= 0)
                return BadRequest("Érvénytelen kérés");
                
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var user = _context.Users
                .Include(u => u.Cars)
                .FirstOrDefault(u => u.Email == userEmail);
                
            if (user == null)
                return NotFound("Felhasználó nem található");
                
            var car = _context.Cars.FirstOrDefault(c => c.Id == request.CarId && c.UserId == user.Id);
            if (car == null)
                return NotFound("Az autó nem található vagy nem a tiéd");
                
            if (!car.IsParked)
                return BadRequest("Az autó nincs leparkolva");
                
            var parkingSpot = _context.ParkingSpots.FirstOrDefault(p => p.CarId == car.Id);
            if (parkingSpot == null)
                return NotFound("A parkolóhely nem található");
                
            // Parkolás befejezése
            parkingSpot.IsOccupied = false;
            parkingSpot.EndTime = DateTime.Now;
            
            // Parkolási díj számítása
            TimeSpan parkingDuration = parkingSpot.EndTime.Value - parkingSpot.StartTime.Value;
            
            // Percenkénti díjszámítás (600 Ft/óra = 10 Ft/perc)
            decimal minuteRate = 600m / 60m; // 10 Ft/perc
            decimal parkingFee = (decimal)parkingDuration.TotalMinutes * minuteRate;
            
            // Kerekítés a legközelebbi 10 forintra
            parkingFee = Math.Ceiling(parkingFee / 10) * 10;
            
            car.IsParked = false;
            parkingSpot.CarId = null;
            
            _context.SaveChanges();
            
            return Ok(new {
                message = "Parkolás befejezve",
                startTime = parkingSpot.StartTime,
                endTime = parkingSpot.EndTime,
                duration = $"{parkingDuration.Hours} óra {parkingDuration.Minutes} perc",
                fee = $"{parkingFee} Ft",
                rate = "600 Ft/óra"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Az aktuális felhasználó parkoló autóinak lekérdezése
    [HttpGet("my")]
    public IActionResult GetMyParkedCars()
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var user = _context.Users
                .Include(u => u.Cars)
                .FirstOrDefault(u => u.Email == userEmail);
                
            if (user == null)
                return NotFound("Felhasználó nem található");
                
            var parkedCars = _context.Cars
                .Where(c => c.UserId == user.Id && c.IsParked)
                .Join(_context.ParkingSpots,
                    car => car.Id,
                    spot => spot.CarId,
                    (car, spot) => new {
                        Car = car,
                        ParkingSpot = spot,
                        StartTime = spot.StartTime
                    })
                .ToList();
                
            return Ok(parkedCars);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
}

// Request osztályok
public class StartParkingRequest
{
    public int CarId { get; set; }
    public int ParkingSpotId { get; set; }
}

public class EndParkingRequest
{
    public int CarId { get; set; }
} 