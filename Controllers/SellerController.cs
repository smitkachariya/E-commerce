using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace E_commerce.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        public IActionResult Dashboard()
        {
            // Redirect legacy seller dashboard route to the data-driven dashboard
            return RedirectToAction("Index", "SellerDashboard");
        }
    }
}