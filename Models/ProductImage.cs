using System.ComponentModel.DataAnnotations;

namespace E_commerce.Models
{
	public class ProductImage
	{
		[Key]
		public int ImageId { get; set; }

		[Required]
		public string ImagePath { get; set; }

		public int ProductId { get; set; }

		public Product Product { get; set; }
	}
}


