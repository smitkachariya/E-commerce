using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using E_commerce.Data;
using E_commerce.Models;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace E_commerce.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: Products (Public - Browse all products)
        public async Task<IActionResult> Index(string searchString, string category)
        {
            var products = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Seller)
                .Where(p => p.Stock > 0);

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                products = products.Where(p => p.Category == category);
            }

            ViewBag.Categories = new[] { "Electronics", "Clothing", "Books", "Home & Garden", "Sports", "Beauty", "Toys", "Automotive", "Food & Beverages", "Other" };
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = category;

            return View(await products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/MyProducts (Seller only)
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> MyProducts()
        {
            var sellerId = _userManager.GetUserId(User);
            var products = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.SellerId == sellerId)
                .ToListAsync();

            return View(products);
        }

        // GET: Products/Create (Seller only)
        [Authorize(Roles = "Seller")]
        public IActionResult Create()
        {
            return View(new CreateProductViewModel());
        }

        // POST: Products/Create (Seller only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Category = model.Category,
                    Stock = model.Stock,
                    SellerId = _userManager.GetUserId(User)
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Handle image uploads
                if (model.Images != null && model.Images.Count > 0)
                {
                    await SaveProductImages(product.ProductId, model.Images);
                }

                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(MyProducts));
            }

            return View(model);
        }

        // GET: Products/Edit/5 (Seller only)
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Edit(int id)
        {
            var sellerId = _userManager.GetUserId(User);
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.SellerId == sellerId);

            if (product == null)
            {
                return NotFound();
            }

            var model = new EditProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Category = product.Category,
                Stock = product.Stock,
                ExistingImages = product.Images.Select(i => i.ImagePath).ToList()
            };

            return View(model);
        }

        // POST: Products/Edit/5 (Seller only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Edit(int id, EditProductViewModel model)
        {
            if (id != model.ProductId)
            {
                return NotFound();
            }

            var sellerId = _userManager.GetUserId(User);
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id && p.SellerId == sellerId);

            if (product == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.Category = model.Category;
                product.Stock = model.Stock;

                _context.Update(product);
                await _context.SaveChangesAsync();

                // Handle new image uploads
                if (model.Images != null && model.Images.Count > 0)
                {
                    await SaveProductImages(product.ProductId, model.Images);
                }

                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction(nameof(MyProducts));
            }

            return View(model);
        }

        // POST: Products/Delete/5 (Seller only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Delete(int id)
        {
            var sellerId = _userManager.GetUserId(User);
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.SellerId == sellerId);

            if (product == null)
            {
                return NotFound();
            }

            // Delete product images from file system
            foreach (var image in product.Images)
            {
                var imagePath = Path.Combine(_environment.WebRootPath, image.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction(nameof(MyProducts));
        }

        private async Task SaveProductImages(int productId, List<IFormFile> images)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploadsFolder);

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var productImage = new ProductImage
                    {
                        ProductId = productId,
                        ImagePath = $"/images/products/{fileName}"
                    };

                    _context.ProductImages.Add(productImage);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}