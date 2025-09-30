using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace E_commerce.Models
{
	public class Order
	{
		public int OrderId { get; set; }
		
		[Required]
		public string CustomerId { get; set; }
		public ApplicationUser Customer { get; set; }
		
		[Required]
		public DateTime OrderDate { get; set; } = DateTime.Now;
		
		[Required]
		public OrderStatus Status { get; set; } = OrderStatus.Pending;
		
		[Required]
		[Range(0.01, double.MaxValue)]
		public decimal TotalAmount { get; set; }
		
		// Shipping Information
		[Required]
		[StringLength(200)]
		public string ShippingAddress { get; set; }
		
		[Required]
		[StringLength(100)]
		public string ShippingCity { get; set; }
		
		[Required]
		[StringLength(20)]
		public string ShippingPostalCode { get; set; }
		
		[Required]
		[StringLength(100)]
		public string ShippingCountry { get; set; }
		
		// Contact Information
		[Required]
		[StringLength(100)]
		public string ContactName { get; set; }
		
		[Required]
		[StringLength(20)]
		public string ContactPhone { get; set; }
		
		[Required]
		[EmailAddress]
		public string ContactEmail { get; set; }
		
		// Order Tracking
		public string OrderNumber { get; set; }
		public DateTime? ShippedDate { get; set; }
		public DateTime? DeliveredDate { get; set; }
		public string TrackingNumber { get; set; }
		public string Notes { get; set; }
		
		// Navigation Properties
		public List<OrderItem> Items { get; set; } = new List<OrderItem>();
		
		// Calculated Properties
		public decimal SubTotal => Items?.Sum(i => i.Price * i.Quantity) ?? 0;
		public decimal ShippingCost => 0; // Free shipping for now
		public decimal Tax => SubTotal * 0.10m; // 10% tax
		public decimal Total => SubTotal + ShippingCost + Tax;
	}
	
	public enum OrderStatus
	{
		Pending = 1,
		Processing = 2,
		Shipped = 3,
		Delivered = 4,
		Cancelled = 5,
		Returned = 6
	}
}


