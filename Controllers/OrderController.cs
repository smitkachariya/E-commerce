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

            // Load saved addresses
            var savedAddresses = await _context.CustomerAddresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Label)
                .ToListAsync();

            var selected = savedAddresses.FirstOrDefault(a => a.IsDefault) ?? savedAddresses.FirstOrDefault();

            var viewModel = new CheckoutViewModel
            {
                ContactName = user.UserName,
                ContactEmail = user.Email,
                CartItems = cartItems,
                SubTotal = cartItems.Sum(c => c.Product.Price * c.Quantity),
                ShippingCost = 0,
                Tax = cartItems.Sum(c => c.Product.Price * c.Quantity) * 0.10m,
                Total = cartItems.Sum(c => c.Product.Price * c.Quantity) * 1.10m,
                SavedAddresses = savedAddresses
            };

            if (selected != null)
            {
                viewModel.SelectedAddressId = selected.CustomerAddressId;
                viewModel.ShippingAddress = selected.Street;
                viewModel.ShippingCity = selected.City;
                viewModel.ShippingPostalCode = selected.PostalCode;
                viewModel.ShippingCountry = selected.Country;
                viewModel.ContactName = selected.RecipientName;
                viewModel.ContactPhone = selected.Phone;
            }

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
                model.SavedAddresses = await _context.CustomerAddresses
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenBy(a => a.Label)
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

            // If user selected a saved address and didn't choose to use a new one, override form fields
            if (!model.UsingNewAddress && model.SelectedAddressId.HasValue)
            {
                var saved = await _context.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.CustomerAddressId == model.SelectedAddressId && a.UserId == customerId);
                if (saved != null)
                {
                    model.ShippingAddress = saved.Street;
                    model.ShippingCity = saved.City;
                    model.ShippingPostalCode = saved.PostalCode;
                    model.ShippingCountry = saved.Country;
                    model.ContactName = saved.RecipientName;
                    model.ContactPhone = saved.Phone;
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

            // Save new address if requested and not using existing saved one
            if ((model.UsingNewAddress || !model.SelectedAddressId.HasValue) && model.SaveThisAddress)
            {
                // If making default, unset previous default
                if (model.MakeDefault)
                {
                    var existingDefaults = await _context.CustomerAddresses
                        .Where(a => a.UserId == customerId && a.IsDefault)
                        .ToListAsync();
                    foreach (var def in existingDefaults)
                    {
                        def.IsDefault = false;
                    }
                }

                var newAddress = new CustomerAddress
                {
                    UserId = customerId,
                    Label = string.IsNullOrWhiteSpace(model.ShippingAddress) ? "Address" : (model.ShippingAddress.Length > 25 ? model.ShippingAddress.Substring(0,25) : model.ShippingAddress),
                    RecipientName = model.ContactName,
                    Phone = model.ContactPhone,
                    Street = model.ShippingAddress,
                    City = model.ShippingCity,
                    State = null,
                    PostalCode = model.ShippingPostalCode,
                    Country = model.ShippingCountry,
                    IsDefault = model.MakeDefault || !(await _context.CustomerAddresses.AnyAsync(a => a.UserId == customerId))
                };
                _context.CustomerAddresses.Add(newAddress);
            }

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


        // GET: Order/MyOrders
        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Add debugging info
            ViewBag.CurrentUser = user.Email;
            ViewBag.CurrentUserRoles = userRoles;
            ViewBag.IsCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            ViewBag.IsSeller = await _userManager.IsInRoleAsync(user, "Seller");
            
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Order/CheckUserRole - Diagnostic action to check user role
        [Authorize]
        public async Task<IActionResult> CheckUserRole()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            var isSeller = await _userManager.IsInRoleAsync(user, "Seller");
            
            return Json(new { 
                UserId = user.Id,
                Email = user.Email,
                Roles = roles,
                IsCustomer = isCustomer,
                IsSeller = isSeller 
            });
        }

        // GET: Order/DebugSavedAddresses - Diagnostic: list saved addresses for current user
        [Authorize]
        public async Task<IActionResult> DebugSavedAddresses()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            var addresses = await _context.CustomerAddresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Label)
                .Select(a => new {
                    a.CustomerAddressId,
                    a.Label,
                    a.RecipientName,
                    a.Phone,
                    a.Street,
                    a.City,
                    a.State,
                    a.PostalCode,
                    a.Country,
                    a.IsDefault,
                    a.CreatedAt
                })
                .ToListAsync();

            return Json(new {
                UserId = userId,
                Email = user?.Email,
                AddressCount = addresses.Count,
                Addresses = addresses
            });
        }

        // GET: Order/FixMyRole - Utility to assign Customer role to current user
        [Authorize]
        public async Task<IActionResult> FixMyRole()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            
            // If user has no roles, assign Customer role
            if (!roles.Any())
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                return Json(new { 
                    success = true, 
                    message = "Customer role assigned successfully!",
                    newRoles = await _userManager.GetRolesAsync(user)
                });
            }
            
            return Json(new { 
                success = false, 
                message = "User already has roles assigned",
                currentRoles = roles 
            });
        }

        // GET: Order/Details
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Check if user is Customer - if not, show debugging info
            if (!await _userManager.IsInRoleAsync(user, "Customer"))
            {
                ViewBag.DebuggingInfo = $"User {user.Email} has roles: {string.Join(", ", userRoles)}. This action requires Customer role.";
                ViewBag.UserId = userId;
                ViewBag.UserRoles = userRoles;
                
                // For now, allow access but show the debugging info
                // return View("AccessDenied");
            }
            
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

        // POST: Order/Cancel - Cancel order by customer
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Cancel(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == userId);

            if (order == null)
            {
                return Json(new { success = false, message = "Order not found or you don't have permission to cancel it." });
            }

            // Only allow cancellation for Pending or Processing orders
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
            {
                return Json(new { success = false, message = "This order cannot be cancelled as it has already been shipped or delivered." });
            }

            // Restore stock for cancelled items
            foreach (var item in order.Items)
            {
                var product = item.Product;
                if (product != null)
                {
                    product.Stock += item.Quantity;
                    _context.Update(product);
                }
            }

            // Update order status
            order.Status = OrderStatus.Cancelled;
            _context.Update(order);
            
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Order has been cancelled successfully. Stock has been restored." });
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