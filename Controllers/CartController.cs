using E_commerce.Data;
using E_commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_commerce.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.Images)
                .Include(c => c.Product.Seller)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var total = cartItems.Sum(c => c.Product.Price * c.Quantity);
            ViewBag.Total = total;

            return View(cartItems);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User);
            
            // Check if product exists and is available
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Stock < quantity)
            {
                TempData["Error"] = "Product not available or insufficient stock.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            // Check if item already exists in cart
            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (existingCartItem != null)
            {
                // Update quantity
                existingCartItem.Quantity += quantity;
                
                // Check stock limit
                if (existingCartItem.Quantity > product.Stock)
                {
                    TempData["Error"] = "Cannot add more items. Stock limit reached.";
                    return RedirectToAction("Details", "Products", new { id = productId });
                }
            }
            else
            {
                // Add new item
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Item added to cart successfully!";
            
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = _userManager.GetUserId(User);
            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Cart item not found." });
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Item removed from cart." });
            }

            if (quantity > cartItem.Product.Stock)
            {
                return Json(new { success = false, message = "Insufficient stock available." });
            }

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();

            var newSubtotal = cartItem.Product.Price * quantity;
            return Json(new { success = true, newSubtotal = newSubtotal.ToString("C") });
        }

        // POST: Cart/RemoveItem
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var userId = _userManager.GetUserId(User);
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item removed from cart.";
            }

            return RedirectToAction("Index");
        }

        // GET: Cart/GetCartItemCount (for navbar badge)
        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userId = _userManager.GetUserId(User);
            var count = await _context.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);

            return Json(count);
        }

        // POST: Cart/ClearCart
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cart cleared successfully.";
            return RedirectToAction("Index");
        }
    }
}