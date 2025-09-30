using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace E_commerce.Models
{
    public class CreateProductViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Product Name")]
        public string Name { get; set; }

        [Required]
        [MinLength(20, ErrorMessage = "Description must be at least 20 characters long")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price ($)")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be 0 or greater")]
        [Display(Name = "Stock Quantity")]
        public int Stock { get; set; }

        [Required]
        [Display(Name = "Product Images")]
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();

        public List<string> Categories { get; set; } = new List<string>
        {
            "Electronics",
            "Clothing",
            "Books",
            "Home & Garden",
            "Sports",
            "Beauty",
            "Toys",
            "Automotive",
            "Food & Beverages",
            "Other"
        };
    }

    public class EditProductViewModel : CreateProductViewModel
    {
        public int ProductId { get; set; }
        public List<string> ExistingImages { get; set; } = new List<string>();
    }
}