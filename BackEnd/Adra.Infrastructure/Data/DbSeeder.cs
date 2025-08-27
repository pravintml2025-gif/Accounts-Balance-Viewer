using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Adra.Core.Entities;
using Adra.Infrastructure.Identity;

namespace Adra.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbSeeder));

        try
        {
            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed roles first
            await SeedRolesAsync(roleManager, logger);

            // Seed users
            await SeedUsersAsync(userManager, logger);

            // Seed accounts
            await SeedAccountsAsync(context, logger);

            // Seed balance history with sample data
            await SeedBalanceHistoryAsync(context, userManager, logger);

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        var roles = new[]
        {
            new ApplicationRole { Name = Roles.Admin, NormalizedName = Roles.Admin.ToUpper() },
            new ApplicationRole { Name = Roles.User, NormalizedName = Roles.User.ToUpper() }
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    logger.LogInformation("Created role: {RoleName}", role.Name);
                }
                else
                {
                    logger.LogError("Failed to create role {RoleName}: {Errors}",
                        role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var users = new[]
        {
            new
            {
                UserName = "admin",
                Email = "admin@adra.com",
                Password = "Admin@123",
                Role = Roles.Admin,
                IsActive = true
            },
            new
            {
                UserName = "john.doe",
                Email = "john.doe@adra.com",
                Password = "User@123",
                Role = Roles.User,
                IsActive = true
            }
        };

        foreach (var userData in users)
        {
            if (await userManager.FindByNameAsync(userData.UserName) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = userData.UserName,
                    Email = userData.Email,
                    NormalizedUserName = userData.UserName.ToUpper(),
                    NormalizedEmail = userData.Email.ToUpper(),
                    EmailConfirmed = true,
                    IsActive = userData.IsActive,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(user, userData.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, userData.Role);
                    logger.LogInformation("Created user: {UserName} with role {Role}", userData.UserName, userData.Role);
                }
                else
                {
                    logger.LogError("Failed to create user {UserName}: {Errors}",
                        userData.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private static async Task SeedAccountsAsync(AppDbContext context, ILogger logger)
    {
        if (!context.Accounts.Any())
        {
            var accounts = new[]
            {
                new Account { Name = "R&D", IsActive = true },
                new Account { Name = "Canteen", IsActive = true },
                new Account { Name = "CEO's car expenses", IsActive = true },
                new Account { Name = "Marketing", IsActive = true },
                new Account { Name = "Parking fines", IsActive = true }
            };

            context.Accounts.AddRange(accounts);
            await context.SaveChangesAsync();
            logger.LogInformation("Created {Count} sample accounts", accounts.Length);
        }
    }

    private static async Task SeedBalanceHistoryAsync(AppDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
    {
        if (!context.BalanceHistories.Any())
        {
            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                logger.LogWarning("Admin user not found, skipping balance history seeding");
                return;
            }

            var accounts = await context.Accounts.ToListAsync();
            if (!accounts.Any())
            {
                logger.LogWarning("No accounts found, skipping balance history seeding");
                return;
            }

            var random = new Random();
            var balanceHistories = new List<BalanceHistory>();

            // Create balance history for the last 6 months
            for (int monthOffset = 1; monthOffset < 6; monthOffset++)
            {
                var date = DateTime.Now.AddMonths(-monthOffset);
                var year = date.Year;
                var month = date.Month;

                foreach (var account in accounts)
                {
                    // Generate specific base amounts for our seeded accounts
                    var baseAmount = account.Name switch
                    {
                        "R&D" => random.Next(50000, 150000), // Research & Development typically has higher budgets
                        "Canteen" => random.Next(5000, 25000), // Food/catering expenses
                        "CEO's car expenses" => random.Next(8000, 20000), // Executive vehicle expenses
                        "Marketing" => random.Next(-5000, 60000), // Marketing campaigns and activities
                        "Parking fines" => random.Next(500, 3000), // Smaller amounts for fines
                        _ => random.Next(1000, 10000) // Fallback for any unexpected accounts
                    };

                    // Add some variation month to month
                    var variation = random.Next(-15, 16) / 100.0; // -15% to +15% variation
                    var amount = (decimal)(baseAmount * (1 + variation));

                    balanceHistories.Add(new BalanceHistory
                    {
                        Id = Guid.NewGuid(),
                        AccountId = account.Id,
                        Year = year,
                        Month = month,
                        Amount = amount,
                        UploadedBy = adminUser.Id,
                        UploadedAt = DateTime.UtcNow.AddMonths(-monthOffset).AddDays(-random.Next(1, 28))
                    });
                }
            }

            context.BalanceHistories.AddRange(balanceHistories);
            await context.SaveChangesAsync();
            logger.LogInformation("Created {Count} balance history records for {MonthCount} months",
                balanceHistories.Count, 6);
        }
    }
}
