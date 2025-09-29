using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace E_commerce.Models
{
	public class Product
	{
		public int ProductId { get; set; }

		[Required]
		[StringLength(200)]
		public string Name { get; set; }

		[Required]
		[MinLength(20)]
		public string Description { get; set; }

		[Range(0.01, double.MaxValue)]
		public decimal Price { get; set; }

		[StringLength(100)]
		public string Category { get; set; }

		[Range(0, int.MaxValue)]
		public int Stock { get; set; }

		[Required]
		public string SellerId { get; set; }

		public ApplicationUser Seller { get; set; }

		public List<ProductImage> Images { get; set; } = new List<ProductImage>();
	}
}


