using System.Threading.Tasks;
using E_commerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace E_commerce.Data
{
	public static class SeedData
	{
		public static async Task InitializeAsync(IServiceScope scope)
		{
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

			string[] roles = new[] { "Customer", "Seller", "Admin" };
			foreach (var role in roles)
			{
				if (!await roleManager.RoleExistsAsync(role))
				{
					await roleManager.CreateAsync(new IdentityRole(role));
				}
			}

			var adminEmail = "admin@example.com";
			var adminUser = await userManager.FindByEmailAsync(adminEmail);
			if (adminUser == null)
			{
				adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
				await userManager.CreateAsync(adminUser, "Admin@123");
				await userManager.AddToRoleAsync(adminUser, "Admin");
			}
		}
	}
}


