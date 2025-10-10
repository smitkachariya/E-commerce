using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using E_commerce.Data;
using E_commerce.Models;

namespace E_commerce.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SellerDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: SellerDashboard
        public async Task<IActionResult> Index()
        {
            var sellerId = _userManager.GetUserId(User);
            
            // Get seller's products with inventory data
            var products = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .Include(p => p.Images)
                .ToListAsync();

            // Get sales data from orders (exclude cancelled/returned)
            var orderItems = await _context.OrderItems
                .AsNoTracking()
                .Include(oi => oi.Order)
                .Where(oi => oi.Product.SellerId == sellerId
                             && oi.Order != null
                             && oi.Order.Status != OrderStatus.Cancelled
                             && oi.Order.Status != OrderStatus.Returned)
                .ToListAsync();

            // Calculate dashboard metrics
            var dashboardData = new SellerDashboardViewModel
            {
                // Inventory Overview
                TotalProducts = products.Count,
                TotalInventoryValue = products.Sum(p => p.Price * p.Stock),
                TotalStock = products.Sum(p => p.Stock),
                LowStockProducts = products.Where(p => p.Stock <= 5).Count(),
                OutOfStockProducts = products.Where(p => p.Stock == 0).Count(),

                // Sales Overview
                TotalOrders = orderItems.Select(oi => oi.OrderId).Distinct().Count(),
                TotalRevenue = orderItems.Sum(oi => oi.Price * oi.Quantity),
                TotalItemsSold = orderItems.Sum(oi => oi.Quantity),

                // Recent activity
                RecentOrders = orderItems
                    .Where(oi => oi.Order.OrderDate >= DateTime.Now.AddDays(-7))
                    .OrderByDescending(oi => oi.Order.OrderDate)
                    .Take(10)
                    .ToList(),

                // Product performance
                Products = products,
                ProductPerformance = products.Select(p => new ProductPerformanceViewModel
                {
                    Product = p,
                    TotalSold = orderItems.Where(oi => oi.ProductId == p.ProductId).Sum(oi => oi.Quantity),
                    TotalRevenue = orderItems.Where(oi => oi.ProductId == p.ProductId).Sum(oi => oi.Price * oi.Quantity),
                    CurrentInventoryValue = p.Price * p.Stock,
                    LastSaleDate = orderItems.Where(oi => oi.ProductId == p.ProductId)
                        .OrderByDescending(oi => oi.Order.OrderDate)
                        .FirstOrDefault()?.Order.OrderDate
                }).ToList(),

                // Monthly sales data for charts
                MonthlySales = GetMonthlySalesData(orderItems),
                
                // Category breakdown
                CategoryBreakdown = products.GroupBy(p => p.Category)
                    .Select(g => new CategoryBreakdownViewModel
                    {
                        Category = g.Key,
                        ProductCount = g.Count(),
                        TotalStock = g.Sum(p => p.Stock),
                        TotalValue = g.Sum(p => p.Price * p.Stock),
                        Revenue = orderItems.Where(oi => g.Any(p => p.ProductId == oi.ProductId)).Sum(oi => oi.Price * oi.Quantity)
                    }).ToList()
            };

            return View(dashboardData);
        }

        // GET: SellerDashboard/InventoryManager
        public async Task<IActionResult> InventoryManager()
        {
            var sellerId = _userManager.GetUserId(User);
            
            var products = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .Include(p => p.Images)
                .OrderBy(p => p.Stock)
                .ToListAsync();

            // Get sales data for each product
            var productPerformance = new List<ProductInventoryViewModel>();
            
            foreach (var product in products)
            {
                var orderItems = await _context.OrderItems
                    .AsNoTracking()
                    .Include(oi => oi.Order)
                    .Where(oi => oi.ProductId == product.ProductId
                                 && oi.Order != null
                                 && oi.Order.Status != OrderStatus.Cancelled
                                 && oi.Order.Status != OrderStatus.Returned)
                    .ToListAsync();

                var totalSold = orderItems.Sum(oi => oi.Quantity);
                var totalRevenue = orderItems.Sum(oi => oi.Price * oi.Quantity);
                var avgSalesPerMonth = orderItems.Any() ? 
                    totalSold / Math.Max(1, (DateTime.Now - orderItems.Min(oi => oi.Order.OrderDate)).Days / 30.0) : 0;

                productPerformance.Add(new ProductInventoryViewModel
                {
                    Product = product,
                    TotalSold = (int)totalSold,
                    TotalRevenue = totalRevenue,
                    CurrentInventoryValue = product.Price * product.Stock,
                    AverageSalesPerMonth = Math.Round(avgSalesPerMonth, 1),
                    LastSaleDate = orderItems.OrderByDescending(oi => oi.Order.OrderDate).FirstOrDefault()?.Order.OrderDate,
                    RecommendedRestock = Math.Max(0, (int)(avgSalesPerMonth * 2) - product.Stock), // 2 months buffer
                    StockStatus = product.Stock == 0 ? "Out of Stock" : 
                                 product.Stock <= 5 ? "Low Stock" : 
                                 product.Stock <= 20 ? "Medium Stock" : "Good Stock"
                });
            }

            return View(productPerformance);
        }

        // POST: SellerDashboard/UpdateStock
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int productId, int newStock)
        {
            var sellerId = _userManager.GetUserId(User);
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.SellerId == sellerId);

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            var oldStock = product.Stock;
            product.Stock = Math.Max(0, newStock);
            
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = $"Stock updated from {oldStock} to {product.Stock}",
                newStock = product.Stock,
                inventoryValue = (product.Price * product.Stock).ToString("F2")
            });
        }

        // GET: SellerDashboard/SalesAnalytics
        public async Task<IActionResult> SalesAnalytics()
        {
            var sellerId = _userManager.GetUserId(User);
            
            var orderItems = await _context.OrderItems
                .AsNoTracking()
                .Include(oi => oi.Order)
                .Where(oi => oi.Product.SellerId == sellerId
                             && oi.Order != null
                             && oi.Order.Status != OrderStatus.Cancelled
                             && oi.Order.Status != OrderStatus.Returned)
                .ToListAsync();

            var analytics = new SalesAnalyticsViewModel
            {
                TotalRevenue = orderItems.Sum(oi => oi.Price * oi.Quantity),
                TotalOrders = orderItems.Select(oi => oi.OrderId).Distinct().Count(),
                AverageOrderValue = orderItems.Any() ? 
                    (decimal)orderItems.GroupBy(oi => oi.OrderId).Average(g => (double)g.Sum(oi => oi.Price * oi.Quantity)) : 0,
                
                // Revenue by month
                MonthlyRevenue = orderItems
                    .GroupBy(oi => new { oi.Order.OrderDate.Year, oi.Order.OrderDate.Month })
                    .Select(g => new MonthlyRevenueViewModel
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Sum(oi => oi.Price * oi.Quantity),
                        OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
                    })
                    .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
                    .Take(12)
                    .ToList(),

                // Top selling products
                TopProducts = orderItems
                    .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                    .Select(g => new TopProductViewModel
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        TotalSold = g.Sum(oi => oi.Quantity),
                        TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity)
                    })
                    .OrderByDescending(tp => tp.TotalRevenue)
                    .Take(10)
                    .ToList(),

                // Daily sales for last 30 days
                DailySales = GetDailySalesData(orderItems)
            };

            return View(analytics);
        }

        private List<MonthlySalesViewModel> GetMonthlySalesData(List<OrderItem> orderItems)
        {
            return orderItems
                .GroupBy(oi => new { oi.Order.OrderDate.Year, oi.Order.OrderDate.Month })
                .Select(g => new MonthlySalesViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Sales = g.Sum(oi => oi.Price * oi.Quantity),
                    OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
                })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToList();
        }

        private List<DailySalesViewModel> GetDailySalesData(List<OrderItem> orderItems)
        {
            var last30Days = Enumerable.Range(0, 30)
                .Select(i => DateTime.Now.Date.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            return last30Days.Select(date => new DailySalesViewModel
            {
                Date = date,
                Sales = orderItems
                    .Where(oi => oi.Order.OrderDate.Date == date)
                    .Sum(oi => oi.Price * oi.Quantity),
                OrderCount = orderItems
                    .Where(oi => oi.Order.OrderDate.Date == date)
                    .Select(oi => oi.OrderId)
                    .Distinct()
                    .Count()
            }).ToList();
        }
    }
}