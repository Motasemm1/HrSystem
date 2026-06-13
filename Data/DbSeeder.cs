using Microsoft.AspNetCore.Identity;
using SmartHrSystem.Models;

namespace SmartHrSystem.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // seed Identity roles
        string[] roles = ["HR", "Employee"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // seed HR admin user
        const string hrEmail = "admin@smarthr.com";
        const string hrPassword = "Admin123!";

        const string empEmail = "osama@smarthr.com";
        const string empPassword = "Osama123!";


        if (await userManager.FindByEmailAsync(hrEmail) == null)
        {
            var hrUser = new ApplicationUser
            {
                UserName = hrEmail,
                Email = hrEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(hrUser, hrPassword);
            await userManager.AddToRoleAsync(hrUser, "HR");
        }
        if (await userManager.FindByEmailAsync(empEmail) == null)
        {
            var empUser = new ApplicationUser
            {
                UserName = empEmail,
                Email = empEmail,
                EmailConfirmed = true,
                EmployeeId = 9 
            };

            await userManager.CreateAsync(empUser, empPassword);
            await userManager.AddToRoleAsync(empUser, "Employee");
        }
    }
}