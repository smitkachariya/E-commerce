using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace E_commerce.Models
{
    public class CheckoutViewModel
    {
        // Shipping Information
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string ContactEmail { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        [Display(Name = "Street Address")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        [Display(Name = "City")]
        public string ShippingCity { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        [Display(Name = "Postal/Zip Code")]
        public string ShippingPostalCode { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        [Display(Name = "Country")]
        public string ShippingCountry { get; set; } = "United States";

        // Order Notes
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Order Notes (Optional)")]
        public string Notes { get; set; }

        // Payment Method (for future implementation)
        [Required(ErrorMessage = "Please select a payment method")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "CashOnDelivery";

        // Cart Items (for display)
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; } = 0;
        public decimal Tax { get; set; }
        public decimal Total { get; set; }

        // Saved addresses support
        public List<CustomerAddress> SavedAddresses { get; set; } = new List<CustomerAddress>();
        public int? SelectedAddressId { get; set; }
        public bool UseSavedAddress => SelectedAddressId.HasValue;
        public bool SaveThisAddress { get; set; }
        public bool MakeDefault { get; set; }
        // Explicit toggle to indicate user chose to use a new address instead of saved
        public bool UsingNewAddress { get; set; }
    }

    public class OrderConfirmationViewModel
    {
        public Order Order { get; set; }
        public string OrderNumber { get; set; }
        public string EstimatedDelivery { get; set; }
        public string Message { get; set; }
    }
}