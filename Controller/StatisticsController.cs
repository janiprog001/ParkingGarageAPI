using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ParkingGarageAPI.Context;
using ParkingGarageAPI.Entities;
using System.Security.Claims;

namespace ParkingGarageAPI.Controller;

[Route("api/statistics")]
[ApiController]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public StatisticsController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Felhasználó saját parkolási előzményei
    [HttpGet("history")]
    public IActionResult GetUserParkingHistory()
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var history = _context.ParkingHistories
                .Where(h => h.UserEmail == userEmail)
                .OrderByDescending(h => h.EndTime)
                .ToList();
                
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Felhasználó parkolási összesítője
    [HttpGet("summary")]
    public IActionResult GetUserParkingSummary()
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var history = _context.ParkingHistories
                .Where(h => h.UserEmail == userEmail)
                .ToList();
            
            if (!history.Any())
                return Ok(new {
                    message = "Nincs még parkolási előzmény",
                    totalParkings = 0,
                    totalFee = 0,
                    averageDuration = "0 óra 0 perc"
                });
                
            var totalParkings = history.Count;
            var totalFee = history.Sum(h => h.Fee);
            var totalMinutes = history.Sum(h => (h.EndTime - h.StartTime).TotalMinutes);
            var averageMinutes = totalMinutes / totalParkings;
            
            var hours = Math.Floor(averageMinutes / 60);
            var minutes = Math.Floor(averageMinutes % 60);
            
            return Ok(new {
                message = "Parkolási összesítő",
                totalParkings = totalParkings,
                totalFee = $"{totalFee:F0}",
                averageDuration = $"{hours} óra {minutes} perc"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Parkolási statisztika autónként
    [HttpGet("by-car")]
    public IActionResult GetParkingStatisticsByCar()
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var carStats = _context.ParkingHistories
                .Where(h => h.UserEmail == userEmail)
                .GroupBy(h => new { h.CarId, h.CarBrand, h.CarModel, h.LicensePlate })
                .ToList()
                .Select(g => new
                {
                    carId = g.Key.CarId,
                    brand = g.Key.CarBrand,
                    model = g.Key.CarModel,
                    licensePlate = g.Key.LicensePlate,
                    totalParkings = g.Count(),
                    totalFee = $"{g.Sum(h => h.Fee):F0}",
                    totalDuration = $"{Math.Floor(g.Sum(h => (h.EndTime - h.StartTime).TotalHours))} óra {Math.Floor(g.Sum(h => (h.EndTime - h.StartTime).TotalMinutes) % 60)} perc"
                })
                .OrderByDescending(c => c.totalParkings);
                //.ToList();
                
            return Ok(carStats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Havi parkolási összesítő
    [HttpGet("monthly")]
    public IActionResult GetMonthlyParkingStatistics([FromQuery] int year = 0, [FromQuery] int month = 0)
    {
        try
        {
            // Ha nincs megadva év és hónap, vegyük az aktuálisat
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var monthlyStats = _context.ParkingHistories
                .Where(h => h.UserEmail == userEmail && 
                            h.EndTime.Year == year && 
                            h.EndTime.Month == month)
                .ToList();
                
            var totalParkings = monthlyStats.Count;
            var totalFee = monthlyStats.Sum(h => h.Fee);
            var totalMinutes = monthlyStats.Sum(h => (h.EndTime - h.StartTime).TotalMinutes);
            
            var hours = Math.Floor(totalMinutes / 60);
            var minutes = Math.Floor(totalMinutes % 60);
            
            return Ok(new {
                year = year,
                month = month,
                monthName = new DateTime(year, month, 1).ToString("MMMM"),
                totalParkings = totalParkings,
                totalFee = $"{totalFee:F0}",
                totalDuration = $"{hours} óra {minutes} perc"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
} 