using System;
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

			string[] roles = new[] { "Customer", "Seller" };
			foreach (var role in roles)
			{
				if (!await roleManager.RoleExistsAsync(role))
				{
					await roleManager.CreateAsync(new IdentityRole(role));
				}
			}

			// One-time cleanup: remove legacy Admin role and revoke it from any users
			await RemoveAdminRoleAndUsersAsync(roleManager, userManager);
		}

		private static async Task RemoveAdminRoleAndUsersAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
		{
			// If Admin role exists, remove it and demote any users to Customer
			var adminRole = await roleManager.FindByNameAsync("Admin");
			if (adminRole != null)
			{
				var usersInAdmin = await userManager.GetUsersInRoleAsync("Admin");
				foreach (var u in usersInAdmin)
				{
					// Remove Admin role
					await userManager.RemoveFromRoleAsync(u, "Admin");

					// Ensure user has at least Customer role (unless already Seller)
					if (!await userManager.IsInRoleAsync(u, "Seller") && !await userManager.IsInRoleAsync(u, "Customer"))
					{
						await userManager.AddToRoleAsync(u, "Customer");
					}
				}

				// Finally remove the Admin role definition
				await roleManager.DeleteAsync(adminRole);
			}

			// Optionally lock the well-known default admin account if it exists to prevent login
			var defaultAdminEmail = "admin@example.com";
			var defaultAdminUser = await userManager.FindByEmailAsync(defaultAdminEmail);
			if (defaultAdminUser != null)
			{
				// Ensure default admin does not retain elevated rights
				if (await userManager.IsInRoleAsync(defaultAdminUser, "Admin"))
				{
					await userManager.RemoveFromRoleAsync(defaultAdminUser, "Admin");
				}
				if (!await userManager.IsInRoleAsync(defaultAdminUser, "Seller") && !await userManager.IsInRoleAsync(defaultAdminUser, "Customer"))
				{
					await userManager.AddToRoleAsync(defaultAdminUser, "Customer");
				}

				// Soft-disable the account (safer than hard delete due to FK constraints)
				defaultAdminUser.LockoutEnabled = true;
				defaultAdminUser.LockoutEnd = DateTimeOffset.MaxValue;
				await userManager.UpdateAsync(defaultAdminUser);
			}
		}
	}
}


