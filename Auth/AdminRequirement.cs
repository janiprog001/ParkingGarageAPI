using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using ParkingGarageAPI.Context;
using Microsoft.EntityFrameworkCore;

namespace ParkingGarageAPI.Auth
{
    // Admin jogosultság követelmény
    public class AdminRequirement : IAuthorizationRequirement
    {
    }

    // Admin jogosultság ellenőrző
    public class AdminAuthorizationHandler : AuthorizationHandler<AdminRequirement>
    {
        private readonly ApplicationDbContext _context;

        public AdminAuthorizationHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            AdminRequirement requirement)
        {
            // Felhasználó email cím lekérése
            var userEmail = context.User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userEmail))
            {
                return; // Nem bejelentkezett felhasználó
            }

            // Felhasználó lekérése az adatbázisból
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            // Admin jogosultság ellenőrzése
            if (user != null && user.IsAdmin)
            {
                context.Succeed(requirement);
            }
        }
    }
} 