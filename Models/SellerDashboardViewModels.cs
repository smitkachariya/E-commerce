using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace E_commerce.Models
{
    // Main Seller Dashboard ViewModel
    public class SellerDashboardViewModel
    {
        // Inventory Overview
        public int TotalProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int TotalStock { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }

        // Sales Overview
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalItemsSold { get; set; }

        // Collections
        public List<OrderItem> RecentOrders { get; set; } = new List<OrderItem>();
        public List<Product> Products { get; set; } = new List<Product>();
        public List<ProductPerformanceViewModel> ProductPerformance { get; set; } = new List<ProductPerformanceViewModel>();
        public List<MonthlySalesViewModel> MonthlySales { get; set; } = new List<MonthlySalesViewModel>();
        public List<CategoryBreakdownViewModel> CategoryBreakdown { get; set; } = new List<CategoryBreakdownViewModel>();
    }

    // Product Performance ViewModel
    public class ProductPerformanceViewModel
    {
        public Product Product { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CurrentInventoryValue { get; set; }
        public DateTime? LastSaleDate { get; set; }
        public decimal ProfitMargin => TotalRevenue > 0 ? ((TotalRevenue - (Product?.Price ?? 0 * TotalSold)) / TotalRevenue * 100) : 0;
    }

    // Product Inventory Management ViewModel
    public class ProductInventoryViewModel
    {
        public Product Product { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CurrentInventoryValue { get; set; }
        public double AverageSalesPerMonth { get; set; }
        public DateTime? LastSaleDate { get; set; }
        public int RecommendedRestock { get; set; }
        public string StockStatus { get; set; }
        
        public string GetStockStatusClass()
        {
            return StockStatus switch
            {
                "Out of Stock" => "badge-danger",
                "Low Stock" => "badge-warning",
                "Medium Stock" => "badge-info",
                "Good Stock" => "badge-success",
                _ => "badge-secondary"
            };
        }
    }

    // Sales Analytics ViewModel
    public class SalesAnalyticsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        
        public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; } = new List<MonthlyRevenueViewModel>();
        public List<TopProductViewModel> TopProducts { get; set; } = new List<TopProductViewModel>();
        public List<DailySalesViewModel> DailySales { get; set; } = new List<DailySalesViewModel>();
    }

    // Monthly Sales ViewModel
    public class MonthlySalesViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Sales { get; set; }
        public int OrderCount { get; set; }
        
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    }

    // Monthly Revenue ViewModel
    public class MonthlyRevenueViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    }

    // Category Breakdown ViewModel
    public class CategoryBreakdownViewModel
    {
        public string Category { get; set; }
        public int ProductCount { get; set; }
        public int TotalStock { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Revenue { get; set; }
        
        public decimal AverageProductValue => ProductCount > 0 ? TotalValue / ProductCount : 0;
    }

    // Top Product ViewModel
    public class TopProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // Daily Sales ViewModel
    public class DailySalesViewModel
    {
        public DateTime Date { get; set; }
        public decimal Sales { get; set; }
        public int OrderCount { get; set; }
        
        public string DateString => Date.ToString("MMM dd");
    }

    // Quick Stock Update ViewModel
    public class QuickStockUpdateViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int CurrentStock { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a positive number")]
        public int NewStock { get; set; }
        
        public decimal Price { get; set; }
        public string Reason { get; set; }
    }

    // Inventory Alert ViewModel
    public class InventoryAlertViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int CurrentStock { get; set; }
        public string AlertType { get; set; } // "OutOfStock", "LowStock", "OverStock"
        public string Message { get; set; }
        public DateTime AlertDate { get; set; }
        public bool IsResolved { get; set; }
    }
}