using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlanMorph.Core.Entities;

namespace PlanMorph.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[] { "Admin", "Client", "Architect", "Contractor", "Engineer", "Student" };

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
        RoleManager<IdentityRole<Guid>> roleManager,
        IConfiguration configuration)
    {
        var adminEmail = configuration["SeedAdmin:Email"]?.Trim();
        var adminPassword = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword) ||
            string.Equals(adminEmail, "<set-in-env>", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(adminPassword, "<set-in-env>", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

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

            var result = await userManager.CreateAsync(admin, adminPassword);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }

    public static async Task SeedCommissionTiersAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.CommissionTiers.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var tiers = new List<CommissionTier>
        {
            new() { RevenueType = CommissionRevenueType.DesignSale, MinAmountKes = 0m, MaxAmountKes = 20000m, RatePercent = 3m, CreatedAt = now, UpdatedAt = now },
            new() { RevenueType = CommissionRevenueType.DesignSale, MinAmountKes = 20001m, MaxAmountKes = 50000m, RatePercent = 4m, CreatedAt = now, UpdatedAt = now },
            new() { RevenueType = CommissionRevenueType.DesignSale, MinAmountKes = 50001m, MaxAmountKes = 100000m, RatePercent = 5m, CreatedAt = now, UpdatedAt = now },
            new() { RevenueType = CommissionRevenueType.DesignSale, MinAmountKes = 100001m, MaxAmountKes = 200000m, RatePercent = 6m, CreatedAt = now, UpdatedAt = now },
            new() { RevenueType = CommissionRevenueType.DesignSale, MinAmountKes = 200001m, MaxAmountKes = null, RatePercent = 7m, CreatedAt = now, UpdatedAt = now },

            new() { RevenueType = CommissionRevenueType.ContractReferral, MinAmountKes = 0m, MaxAmountKes = 500000m, RatePercent = 1.5m, CreatedAt = now, UpdatedAt = now },
            new() { RevenueType = CommissionRevenueType.ContractReferral, MinAmountKes = 500001m, MaxAmountKes = 2000000m, RatePercent = 2m, CreatedAt = now, UpdatedAt = now },
            new() { RevenueType = CommissionRevenueType.ContractReferral, MinAmountKes = 2000001m, MaxAmountKes = 10000000m, RatePercent = 2.5m, CreatedAt = now, UpdatedAt = now },
            new() { RevenueType = CommissionRevenueType.ContractReferral, MinAmountKes = 10000001m, MaxAmountKes = null, RatePercent = 3m, CreatedAt = now, UpdatedAt = now }
        };

        await dbContext.CommissionTiers.AddRangeAsync(tiers);
        await dbContext.SaveChangesAsync();
    }
}
