using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingGarageAPI.Context;
using ParkingGarageAPI.DTOs;
using ParkingGarageAPI.Entities;
using System.Text.Json.Serialization;

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

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await context.Users
            .Include(u => u.Cars)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber,
                u.IsAdmin,
                Cars = u.Cars.Select(c => new
                {
                    c.Id,
                    c.Brand,
                    c.Model,
                    c.Year,
                    c.LicensePlate,
                    c.IsParked
                }).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(int id)
    {
        var currentUserEmail = User.FindFirstValue(ClaimTypes.Name);
        var currentUser = await context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);
        
        if (currentUser == null)
            return Unauthorized("User not found.");

        if (!currentUser.IsAdmin && currentUser.Id != id)
            return Forbid("You don't have permission to view this user.");

        var user = await context.Users
            .Include(u => u.Cars)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber,
                u.IsAdmin,
                Cars = u.Cars.Select(c => new
                {
                    c.Id,
                    c.Brand,
                    c.Model,
                    c.Year,
                    c.LicensePlate,
                    c.IsParked
                }).ToList()
            })
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound("User not found.");

        return Ok(user);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        var currentUserEmail = User.FindFirstValue(ClaimTypes.Name);
        var currentUser = await context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);
        
        if (currentUser == null)
            return Unauthorized("User not found.");

        var userToUpdate = await context.Users.FindAsync(id);
        if (userToUpdate == null)
            return NotFound("User not found.");

        if (!currentUser.IsAdmin && currentUser.Id != id)
            return Forbid("You don't have permission to modify this user.");

        if (updateUserDto.Email != userToUpdate.Email && 
            context.Users.Any(u => u.Email == updateUserDto.Email))
            return BadRequest("Email already in use.");

        userToUpdate.FirstName = updateUserDto.FirstName ?? userToUpdate.FirstName;
        userToUpdate.LastName = updateUserDto.LastName ?? userToUpdate.LastName;
        userToUpdate.Email = updateUserDto.Email ?? userToUpdate.Email;
        userToUpdate.PhoneNumber = updateUserDto.PhoneNumber ?? userToUpdate.PhoneNumber;

        if (!string.IsNullOrEmpty(updateUserDto.NewPassword))
        {
            userToUpdate.PasswordHash = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(updateUserDto.NewPassword));
        }

        if (updateUserDto.IsAdmin.HasValue && currentUser.IsAdmin)
        {
            userToUpdate.IsAdmin = updateUserDto.IsAdmin.Value;
        }

        await context.SaveChangesAsync();
        return Ok("User updated successfully.");
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            Console.WriteLine($"Starting user deletion for ID: {id}");
            
            var currentUserEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            Console.WriteLine($"Current user email: {currentUserEmail}");
            
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                Console.WriteLine("No current user email found");
                return Unauthorized(new { message = "Felhasználó nincs bejelentkezve" });
            }

            var currentUser = await context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);
            Console.WriteLine($"Current user found: {currentUser != null}, IsAdmin: {currentUser?.IsAdmin}");
            
            if (currentUser == null)
            {
                Console.WriteLine("Current user not found in database");
                return Unauthorized(new { message = "Felhasználó nem található" });
            }

            if (!currentUser.IsAdmin)
            {
                Console.WriteLine("Current user is not admin");
                return Forbid("Nincs jogosultsága a művelet végrehajtásához");
            }

            Console.WriteLine("Loading user to delete with related entities");
            var userToDelete = await context.Users
                .Include(u => u.Cars)
                .Include(u => u.Invoices)
                .Include(u => u.ParkingHistories)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (userToDelete == null)
            {
                Console.WriteLine($"User to delete not found with ID: {id}");
                return NotFound(new { message = "A törlendő felhasználó nem található" });
            }

            Console.WriteLine($"Found user to delete: {userToDelete.Email}");
            Console.WriteLine($"Related entities - Cars: {userToDelete.Cars?.Count ?? 0}, " +
                            $"Invoices: {userToDelete.Invoices?.Count ?? 0}, " +
                            $"ParkingHistories: {userToDelete.ParkingHistories?.Count ?? 0}");

            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    // Először töröljük a számlákat, mert azok hivatkoznak a parkolási előzményekre
                    if (userToDelete.Invoices != null && userToDelete.Invoices.Any())
                    {
                        Console.WriteLine($"Deleting {userToDelete.Invoices.Count} invoices");
                        context.Invoices.RemoveRange(userToDelete.Invoices);
                        await context.SaveChangesAsync();
                    }

                    // Ezután töröljük a parkolási előzményeket
                    if (userToDelete.ParkingHistories != null && userToDelete.ParkingHistories.Any())
                    {
                        Console.WriteLine($"Deleting {userToDelete.ParkingHistories.Count} parking histories");
                        context.ParkingHistories.RemoveRange(userToDelete.ParkingHistories);
                        await context.SaveChangesAsync();
                    }

                    // Végül töröljük az autókat
                    if (userToDelete.Cars != null && userToDelete.Cars.Any())
                    {
                        Console.WriteLine($"Deleting {userToDelete.Cars.Count} cars");
                        context.Cars.RemoveRange(userToDelete.Cars);
                        await context.SaveChangesAsync();
                    }

                    // Végül töröljük a felhasználót
                    Console.WriteLine("Deleting user");
                    context.Users.Remove(userToDelete);
                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    Console.WriteLine("User deletion completed successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during deletion: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        Console.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                    }
                    
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return Ok(new { message = "Felhasználó sikeresen törölve" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                Console.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
            }
            
            return StatusCode(500, new { 
                message = "Váratlan hiba történt", 
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var foundUser = context.Users.FirstOrDefault(u => u.Email == loginDto.Email);
        if (foundUser == null || foundUser.PasswordHash != Convert.ToBase64String(Encoding.UTF8.GetBytes(loginDto.Password)))
            return Unauthorized("Invalid email or password.");

        var loginTime = DateTime.UtcNow;
        var expiresAt = loginTime.AddMinutes(15); // 15 percig érvényes sütik

        var claims = new List<Claim> 
        { 
            new Claim(ClaimTypes.Name, foundUser.Email),
            new Claim(ClaimTypes.Role, foundUser.IsAdmin ? "Admin" : "User")
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties 
        { 
            IsPersistent = true, 
            IssuedUtc = loginTime,
            ExpiresUtc = expiresAt
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(claimsIdentity), 
            authProperties);

        return Ok(new LoginResponse 
        { 
            Message = "Login successful.", 
            User = foundUser.Email,
            UserId = foundUser.Id,
            IsAdmin = foundUser.IsAdmin,
            LoginTime = loginTime.ToString("HH:mm:ss"),
            ExpiresAt = expiresAt.ToString("HH:mm:ss")
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok("User logged out successfully.");
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetUserStatus()
    {
        var currentUserEmail = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrEmpty(currentUserEmail))
            return Unauthorized(new { authenticated = false, message = "Not authenticated" });
            
        var user = await context.Users
            .Include(u => u.Cars)
            .Select(u => new {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.IsAdmin,
                Cars = u.Cars.Select(c => new {
                    c.Id,
                    c.Brand,
                    c.Model,
                    c.LicensePlate,
                    c.IsParked
                }).ToList()
            })
            .FirstOrDefaultAsync(u => u.Email == currentUserEmail);
            
        if (user == null)
            return Unauthorized(new { authenticated = false, message = "User not found" });
            
        return Ok(new { 
            authenticated = true,
            user = user
        });
    }
}

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? NewPassword { get; set; }
    public bool? IsAdmin { get; set; }
}

public class LoginResponse
{
    public string Message { get; set; }
    public string User { get; set; }
    public int UserId { get; set; }
    public bool IsAdmin { get; set; }
    public string LoginTime { get; set; }
    public string ExpiresAt { get; set; }
}
