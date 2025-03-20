using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ParkingGarageAPI.Context;
using ParkingGarageAPI.Entities;
using System.Security.Claims;
using System.Collections.Generic;

namespace ParkingGarageAPI.Controller;

[Route("api/cars")]
[ApiController]
[Authorize]
public class CarController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public CarController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public IActionResult GetUserCars()
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized("User not authenticated.");
                
            var user = _context.Users
                .Include(u => u.Cars)
                .FirstOrDefault(u => u.Email == userEmail);
            
            if (user == null)
                return NotFound("User not found.");
                
            if (user.Cars == null)
            {
                user.Cars = new List<Car>();
            }
                
            return Ok(user.Cars);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPost]
    public IActionResult AddCar([FromBody] Car car)
    {
        try
        {
            if (car == null)
                return BadRequest("Car data is null");
                
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized("User not authenticated.");
                
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            
            if (user == null)
                return NotFound("User not found.");
                
            car.UserId = user.Id;
            _context.Cars.Add(car);
            _context.SaveChanges();
            
            return Ok("Car added successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpDelete("{id}")]
    public IActionResult DeleteCar(int id)
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized("User not authenticated.");
                
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            
            if (user == null)
                return NotFound("User not found.");
                
            var car = _context.Cars.FirstOrDefault(c => c.Id == id && c.UserId == user.Id);
            
            if (car == null)
                return NotFound("Car not found or you don't have permission to delete it.");
                
            _context.Cars.Remove(car);
            _context.SaveChanges();
            
            return Ok("Car deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
} 