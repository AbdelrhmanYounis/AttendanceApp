using AttendanceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AttendanceApp.Data;

    public class Seed
    {
        public static async Task SeedData(UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;

            var roles = new List<IdentityRole>
            {
                new IdentityRole{Name = "User"},
                new IdentityRole{Name = "Admin"},
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }
            
            var admin = new ApplicationUser
            {
                Email = "admin@contrato.com",
                UserName = "admin",
                FirstName="Admin",
                LastName="Admin",
                Position="Employee"
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRoleAsync(admin,"Admin");
        }
    }

