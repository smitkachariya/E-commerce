using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Message = "Products page - Coming soon!";
            return View();
        }

        public IActionResult MyProducts()
        {
            ViewBag.Message = "My Products page - Coming soon!";
            return View();
        }

        public IActionResult Create()
        {
            ViewBag.Message = "Add Product page - Coming soon!";
            return View();
        }
    }
}