using Microsoft.AspNetCore.Identity;
using SmartHrSystem.Models;

namespace SmartHrSystem.Data;

public static class DbSeeder
{
    public static async Task SeedAdminUserAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        const string email = "admin@smarthr.com";
        const string password = "Admin123!";

        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, password);
        }
    }
}