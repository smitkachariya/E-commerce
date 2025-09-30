using E_commerce.Data;
using E_commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_commerce.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Order/Checkout
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.Images)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty. Add some products before checkout.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            var viewModel = new CheckoutViewModel
            {
                ContactName = user.UserName,
                ContactEmail = user.Email,
                CartItems = cartItems,
                SubTotal = cartItems.Sum(c => c.Product.Price * c.Quantity),
                ShippingCost = 0,
                Tax = cartItems.Sum(c => c.Product.Price * c.Quantity) * 0.10m,
                Total = cartItems.Sum(c => c.Product.Price * c.Quantity) * 1.10m
            };

            return View(viewModel);
        }

        // POST: Order/Checkout
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload cart items if validation fails
                var userId = _userManager.GetUserId(User);
                model.CartItems = await _context.CartItems
                    .Include(c => c.Product)
                    .ThenInclude(p => p.Images)
                    .Where(c => c.UserId == userId)
                    .ToListAsync();
                
                model.SubTotal = model.CartItems.Sum(c => c.Product.Price * c.Quantity);
                model.Tax = model.SubTotal * 0.10m;
                model.Total = model.SubTotal + model.Tax;
                
                return View(model);
            }

            var customerId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.Images)
                .Where(c => c.UserId == customerId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // Check stock availability
            foreach (var cartItem in cartItems)
            {
                if (cartItem.Product.Stock < cartItem.Quantity)
                {
                    TempData["Error"] = $"Sorry, {cartItem.Product.Name} doesn't have enough stock. Available: {cartItem.Product.Stock}";
                    return RedirectToAction("Index", "Cart");
                }
            }

            // Create order
            var order = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                TotalAmount = model.Total,
                ShippingAddress = model.ShippingAddress,
                ShippingCity = model.ShippingCity,
                ShippingPostalCode = model.ShippingPostalCode,
                ShippingCountry = model.ShippingCountry,
                ContactName = model.ContactName,
                ContactPhone = model.ContactPhone,
                ContactEmail = model.ContactEmail,
                Notes = model.Notes,
                OrderNumber = GenerateOrderNumber()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order items and update stock
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price,
                    ProductName = cartItem.Product.Name,
                    ProductCategory = cartItem.Product.Category,
                    ProductImagePath = cartItem.Product.Images.FirstOrDefault()?.ImagePath
                };

                _context.OrderItems.Add(orderItem);

                // Update product stock
                cartItem.Product.Stock -= cartItem.Quantity;
            }

            // Clear cart
            _context.CartItems.RemoveRange(cartItems);
            
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your order has been placed successfully!";
            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
        }

        // GET: Order/OrderConfirmation
        [Authorize]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == userId);

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new OrderConfirmationViewModel
            {
                Order = order,
                OrderNumber = order.OrderNumber,
                EstimatedDelivery = order.OrderDate.AddDays(7).ToString("MMMM dd, yyyy"),
                Message = "Thank you for your order! We'll send you email updates about your order status."
            };

            return View(viewModel);
        }

        // POST: Order/QuickCheckout - Creates order with default shipping info
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> QuickCheckout()
        {
            var customerId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.Images)
                .Where(c => c.UserId == customerId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // Check stock availability
            foreach (var cartItem in cartItems)
            {
                if (cartItem.Product.Stock < cartItem.Quantity)
                {
                    TempData["Error"] = $"Sorry, {cartItem.Product.Name} doesn't have enough stock. Available: {cartItem.Product.Stock}";
                    return RedirectToAction("Index", "Cart");
                }
            }

            var user = await _userManager.GetUserAsync(User);
            var total = cartItems.Sum(c => c.Product.Price * c.Quantity) * 1.10m; // Including 10% tax

            // Create order with default/minimal information
            var order = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                TotalAmount = total,
                ShippingAddress = "Default Address - Please update in order details",
                ShippingCity = "Default City",
                ShippingPostalCode = "00000",
                ShippingCountry = "United States",
                ContactName = user.UserName ?? "Customer",
                ContactPhone = "000-000-0000",
                ContactEmail = user.Email ?? "customer@example.com",
                Notes = "Quick checkout order - shipping details may need updating",
                OrderNumber = GenerateOrderNumber()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order items and update stock
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price,
                    ProductName = cartItem.Product.Name,
                    ProductCategory = cartItem.Product.Category,
                    ProductImagePath = cartItem.Product.Images.FirstOrDefault()?.ImagePath
                };

                _context.OrderItems.Add(orderItem);

                // Update product stock
                cartItem.Product.Stock -= cartItem.Quantity;
            }

            // Clear cart
            _context.CartItems.RemoveRange(cartItems);
            
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your order has been placed successfully! You can update shipping details in My Orders.";
            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
        }

        // GET: Order/MyOrders
        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Order/Details
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/SellerOrders
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> SellerOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.Items.Any(oi => oi.Product.SellerId == userId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // POST: Order/UpdateStatus
        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.Items.Any(oi => oi.Product.SellerId == userId));

            if (order == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            order.Status = status;
            
            if (status == OrderStatus.Shipped)
            {
                order.ShippedDate = DateTime.Now;
            }
            else if (status == OrderStatus.Delivered)
            {
                order.DeliveredDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Order status updated successfully." });
        }

        private string GenerateOrderNumber()
        {
            return "ORD" + DateTime.Now.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999);
        }
    }
}