using Microsoft.AspNetCore.Identity;

namespace E_commerce.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string FullName => $"{FirstName} {LastName}";
	}
}


