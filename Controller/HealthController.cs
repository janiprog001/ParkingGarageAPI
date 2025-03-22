using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingGarageAPI.Context;

namespace ParkingGarageAPI.Controller;

[Route("api/health")]
[ApiController]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<IActionResult> Check()
    {
        try
        {
            // Ellenőrizzük, hogy az adatbázis kapcsolat működik-e
            bool canConnectToDb = await _context.Database.CanConnectAsync();
            
            if (canConnectToDb)
            {
                return Ok(new { status = "healthy", message = "API is running and database connection is working" });
            }
            else
            {
                return StatusCode(500, new { status = "unhealthy", message = "Cannot connect to database" });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = "error", message = $"Health check failed: {ex.Message}" });
        }
    }
} 