using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingGarageAPI.Context;
using System.Security.Claims;

namespace ParkingGarageAPI.Controller
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("userdata")]
        [Authorize]
        public async Task<IActionResult> GetUserData()
        {
            // Az aktuu00e1lisan bejelentkezett felhasznu00e1lu00f3 email cu00edmu00e9nek leku00e9ru00e9se a Claims-bu0151l
            var userEmail = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("Ku00e9rem jelentkezzen be");
            }

            // Az aktuu00e1lis felhasznu00e1lu00f3 leku00e9ru00e9se az email cu00edm alapju00e1n
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                return Unauthorized("Ku00e9rem jelentkezzen be");
            }

            // Az autu00f3k leku00e9ru00e9se a bejelentkezett felhasznu00e1lu00f3hoz
            var cars = await _context.Cars.Where(c => c.UserId == user.Id).ToListAsync();

            // Felhasznu00e1lu00f3i adatok u00e9s autu00f3k visszaadu00e1sa
            var userData = new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                Cars = cars.Select(c => new
                {
                    c.Brand,
                    c.Model,
                    c.Year,
                    c.LicensePlate
                }).ToList()
            };

            return Ok(userData);
        }
    }
}
