using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingGarageAPI.Context;
using ParkingGarageAPI.DTOs;
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

        // Generálunk egy új egyedi ID-t
        int newId = context.Users.Any() ? context.Users.Max(u => u.Id) + 1 : 1;
        user.Id = newId;
        
        // Biztosítjuk, hogy minden autónak az IsParked értéke false legyen
        if (user.Cars != null && user.Cars.Any())
        {
            foreach (var car in user.Cars)
            {
                car.IsParked = false;
            }
        }
        
        // Explicit módon beállítjuk az IsAdmin értékét false-ra
        user.IsAdmin = false;
        
        user.PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.PasswordHash));
        context.Users.Add(user);
        context.SaveChanges();
        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var foundUser = context.Users.FirstOrDefault(u => u.Email == loginDto.Email);
        if (foundUser == null || foundUser.PasswordHash != Convert.ToBase64String(Encoding.UTF8.GetBytes(loginDto.Password)))
            return Unauthorized("Invalid email or password.");

        var loginTime = DateTime.UtcNow;
        var expiresAt = loginTime.AddMinutes(5); // 5 percig érvényes sütik

        var claims = new List<Claim> 
        { 
            new Claim(ClaimTypes.Name, foundUser.Email),
            new Claim(ClaimTypes.Role, foundUser.IsAdmin ? "Admin" : "User")
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties 
        { 
            IsPersistent = true, 
            IssuedUtc = loginTime, // Bejelentkezési idő
            ExpiresUtc = expiresAt  // Lejárati idő
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(claimsIdentity), 
            authProperties);

        return Ok(new 
        { 
            Message = "Login successful.", 
            User = foundUser.Email,
            IsAdmin = foundUser.IsAdmin,
            LoginTime = loginTime.ToString("HH:mm:ss"),
            ExpiresAt = expiresAt.ToString("HH:mm:ss")
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok("Logged out successfully.");
    }
}
