using Microsoft.AspNetCore.Mvc;
using ParkingGarageAPI.Context;
using ParkingGarageAPI.Entities;

namespace ParkingGarageAPI.Controller;

[Route("api/users")]
[ApiController]
public class UsersController(ApplicationDbContext context) : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] User user)
    {
        if (context.Users.Any(u => u.Email == user.Email))
            return BadRequest("Email already registered.");

        context.Users.Add(user);
        context.SaveChanges();
        return Ok("User registered successfully.");
    }
}
