using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ParkingGarageAPI.Context;
using ParkingGarageAPI.Entities;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;

namespace ParkingGarageAPI.Controller;

[Route("api/admin/statistics")]
[ApiController]
[Authorize(Policy = "AdminOnly")]
public class AdminStatisticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public AdminStatisticsController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Összes parkolási előzmény
    [HttpGet("all-history")]
    public IActionResult GetAllParkingHistory()
    {
        try
        {
            var history = _context.ParkingHistories
                .OrderByDescending(h => h.EndTime)
                .ToList();
                
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Teljes bevételi statisztika
    [HttpGet("revenue")]
    public IActionResult GetRevenueStatistics([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        try
        {
            IQueryable<ParkingHistory> query = _context.ParkingHistories;
            
            // Időszak szűrése, ha meg van adva
            if (year.HasValue)
            {
                query = query.Where(h => h.EndTime.Year == year);
            }
            
            if (month.HasValue && year.HasValue)
            {
                query = query.Where(h => h.EndTime.Month == month);
            }
            
            var history = query.ToList();
            
            var totalParkings = history.Count;
            var totalRevenue = (int)history.Sum(h => h.Fee);
            var totalMinutes = history.Sum(h => (h.EndTime - h.StartTime).TotalMinutes);
            
            var hours = Math.Floor(totalMinutes / 60);
            var minutes = Math.Floor(totalMinutes % 60);
            
            return Ok(new {
                period = GetPeriodName(year, month),
                totalParkings = totalParkings,
                totalRevenue = totalRevenue,
                totalParkingDuration = $"{hours} óra {minutes} perc",
                averageParkingFee = totalParkings > 0 
                    ? Math.Round(totalRevenue / (double)totalParkings, 0)
                    : 0
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Parkolóház kihasználtsági statisztika
    [HttpGet("occupancy")]
    public IActionResult GetOccupancyStatistics()
    {
        try
        {
            var totalSpots = _context.ParkingSpots.Count();
            var occupiedSpots = _context.ParkingSpots.Count(p => p.IsOccupied);
            var availableSpots = totalSpots - occupiedSpots;
            
            var occupancyPercentage = totalSpots > 0 
                ? Math.Round((double)occupiedSpots / totalSpots * 100, 1)
                : 0;
                
            return Ok(new {
                totalSpots = totalSpots,
                occupiedSpots = occupiedSpots,
                availableSpots = availableSpots,
                occupancyPercentage = $"{occupancyPercentage}%"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Felhasználói aktivitás
    [HttpGet("user-activity")]
    public IActionResult GetUserActivityStatistics()
    {
        try
        {
            var userStats = _context.ParkingHistories
                .GroupBy(h => new { h.UserId, h.UserName, h.UserEmail })
                .ToList() // Adatok lekérése az adatbázisból
                .Select(g => new {
                    userId = g.Key.UserId,
                    userName = g.Key.UserName,
                    email = g.Key.UserEmail,
                    totalParkings = g.Count(),
                    totalFee = (int)g.Sum(h => h.Fee),
                    avgParkingDuration = g.Average(h => (h.EndTime - h.StartTime).TotalMinutes)
                })
                .OrderByDescending(u => u.totalParkings)
                .ToList();
                
            // Átlagos időtartam formázása
            var formattedStats = userStats.Select(u => new {
                userId = u.userId,
                userName = u.userName,
                email = u.email,
                totalParkings = u.totalParkings,
                totalFee = u.totalFee,
                avgParkingDuration = $"{Math.Floor(u.avgParkingDuration / 60)} óra {Math.Floor(u.avgParkingDuration % 60)} perc"
            });
                
            return Ok(formattedStats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Havi bevételi kimutatás
    [HttpGet("monthly-revenue")]
    public IActionResult GetMonthlyRevenueStatistics([FromQuery] int year = 0)
    {
        try
        {
            if (year == 0) year = DateTime.Now.Year;
            
            var parkingHistories = _context.ParkingHistories
                .Where(h => h.EndTime.Year == year)
                .ToList();
                
            var monthlyStats = new List<object>();
            
            for (int month = 1; month <= 12; month++)
            {
                var monthData = parkingHistories
                    .Where(h => h.EndTime.Month == month)
                    .ToList();
                    
                var totalParkings = monthData.Count;
                var totalRevenue = (int)monthData.Sum(h => h.Fee);
                
                monthlyStats.Add(new {
                    month = month,
                    monthName = new DateTime(year, month, 1).ToString("MMMM"),
                    totalParkings = totalParkings,
                    totalRevenue = totalRevenue
                });
            }
                
            return Ok(new {
                year = year,
                monthlyRevenue = monthlyStats
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Segédfüggvény az időszak nevének formázásához
    private string GetPeriodName(int? year, int? month)
    {
        if (year.HasValue && month.HasValue)
        {
            return $"{year}. {new DateTime(year.Value, month.Value, 1).ToString("MMMM")}";
        }
        if (year.HasValue)
        {
            return $"{year}. év";
        }
        
        return "Teljes időszak";
    }
} 