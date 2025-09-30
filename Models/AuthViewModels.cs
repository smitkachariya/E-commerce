using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_commerce.Models
{
	public class RegisterViewModel
	{
		[Required]
		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		[Required]
		[Display(Name = "Last Name")]
		public string LastName { get; set; }

		[Required]
		[EmailAddress]
		[Display(Name = "Email Address")]
		public string Email { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		[Required]
		[Display(Name = "I am a")]
		public string Role { get; set; }

		public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>
		{
			new SelectListItem { Text = "Customer - I want to buy products", Value = "Customer" },
			new SelectListItem { Text = "Seller - I want to sell products", Value = "Seller" }
		};
	}

	public class LoginViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }
	}
}


