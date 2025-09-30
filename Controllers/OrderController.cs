using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace E_commerce.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        [Authorize(Roles = "Customer")]
        public IActionResult MyOrders()
        {
            ViewBag.Message = "My Orders - Coming soon!";
            return View();
        }

        [Authorize(Roles = "Seller")]
        public IActionResult SellerOrders()
        {
            ViewBag.Message = "Seller Orders - Coming soon!"; 
            return View();
        }
    }
}