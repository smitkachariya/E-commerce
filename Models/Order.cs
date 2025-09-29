using System;
using System.Collections.Generic;

namespace E_commerce.Models
{
	public class Order
	{
		public int OrderId { get; set; }
		public string CustomerId { get; set; }
		public ApplicationUser Customer { get; set; }
		public DateTime Date { get; set; }
		public string Status { get; set; }
		public List<OrderItem> Items { get; set; } = new List<OrderItem>();
	}
}


