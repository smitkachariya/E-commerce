using System.ComponentModel.DataAnnotations;

namespace E_commerce.Models
{
	public class OrderItem
	{
		public int OrderItemId { get; set; }
		
		[Required]
		public int OrderId { get; set; }
		public Order Order { get; set; }
		
		[Required]
		public int ProductId { get; set; }
		public Product Product { get; set; }
		
		[Required]
		[Range(1, int.MaxValue)]
		public int Quantity { get; set; }
		
		[Required]
		[Range(0.01, double.MaxValue)]
		public decimal Price { get; set; }
		
		// Product snapshot at time of order
		[StringLength(200)]
		public string ProductName { get; set; }
		
		[StringLength(100)]
		public string ProductCategory { get; set; }
		
		public string ProductImagePath { get; set; }
		
		// Calculated Properties
		public decimal SubTotal => Price * Quantity;
	}
}


