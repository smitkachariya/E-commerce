using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace E_commerce.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Message = "Shopping Cart - Coming soon!";
            return View();
        }
    }
}