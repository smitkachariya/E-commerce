using System.Linq; 
using System.Threading.Tasks;
using E_commerce.Data;
using E_commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Controllers
{
    [Authorize]
    public class AddressesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Addresses
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var addresses = await _context.CustomerAddresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Label)
                .ToListAsync();
            return View(addresses);
        }

        // GET: /Addresses/Create
        public IActionResult Create() => View(new CustomerAddress());

        // POST: /Addresses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerAddress model)
        {
            var userId = _userManager.GetUserId(User);
            if (!ModelState.IsValid)
                return View(model);

            model.UserId = userId;
            if (model.IsDefault)
            {
                var existingDefaults = await _context.CustomerAddresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
                foreach (var d in existingDefaults) d.IsDefault = false;
            }
            else if (!await _context.CustomerAddresses.AnyAsync(a => a.UserId == userId))
            {
                // First address becomes default implicitly
                model.IsDefault = true;
            }

            _context.CustomerAddresses.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Address added.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Addresses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var address = await _context.CustomerAddresses.FirstOrDefaultAsync(a => a.CustomerAddressId == id && a.UserId == userId);
            if (address == null) return NotFound();
            return View(address);
        }

        // POST: /Addresses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerAddress model)
        {
            var userId = _userManager.GetUserId(User);
            var address = await _context.CustomerAddresses.FirstOrDefaultAsync(a => a.CustomerAddressId == id && a.UserId == userId);
            if (address == null) return NotFound();
            if (!ModelState.IsValid) return View(model);

            address.Label = model.Label;
            address.RecipientName = model.RecipientName;
            address.Phone = model.Phone;
            address.Street = model.Street;
            address.City = model.City;
            address.State = model.State;
            address.PostalCode = model.PostalCode;
            address.Country = model.Country;
            address.UpdatedAt = System.DateTime.UtcNow;

            if (model.IsDefault && !address.IsDefault)
            {
                var existingDefaults = await _context.CustomerAddresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
                foreach (var d in existingDefaults) d.IsDefault = false;
                address.IsDefault = true;
            }
            else if (!model.IsDefault && address.IsDefault)
            {
                // Keep at least one default
                if (await _context.CustomerAddresses.CountAsync(a => a.UserId == userId) > 1)
                {
                    address.IsDefault = false;
                    var firstOther = await _context.CustomerAddresses.Where(a => a.UserId == userId && a.CustomerAddressId != id).FirstAsync();
                    firstOther.IsDefault = true;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Address updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Addresses/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var address = await _context.CustomerAddresses.FirstOrDefaultAsync(a => a.CustomerAddressId == id && a.UserId == userId);
            if (address == null) return NotFound();
            _context.CustomerAddresses.Remove(address);
            await _context.SaveChangesAsync();

            // Ensure there is still a default
            var remaining = await _context.CustomerAddresses.Where(a => a.UserId == userId).ToListAsync();
            if (remaining.Any() && !remaining.Any(a => a.IsDefault))
            {
                remaining.First().IsDefault = true;
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Address deleted.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Addresses/SetDefault/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefault(int id)
        {
            var userId = _userManager.GetUserId(User);
            var address = await _context.CustomerAddresses.FirstOrDefaultAsync(a => a.CustomerAddressId == id && a.UserId == userId);
            if (address == null) return NotFound();
            var existing = await _context.CustomerAddresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
            foreach (var d in existing) d.IsDefault = false;
            address.IsDefault = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Default address updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
