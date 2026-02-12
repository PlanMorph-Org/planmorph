using Microsoft.AspNetCore.Identity;
using PlanMorph.Core.Entities;

namespace PlanMorph.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[] { "Admin", "Client", "Architect", "Contractor", "Engineer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    public static async Task SeedAdminUserAsync(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        var adminEmail = "admin@planmorph.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                Role = UserRole.Admin,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}