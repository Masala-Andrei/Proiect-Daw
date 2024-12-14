using Microsoft.AspNetCore.Mvc;

namespace Proiect_DAW.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
