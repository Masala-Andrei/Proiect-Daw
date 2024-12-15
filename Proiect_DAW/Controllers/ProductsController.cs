////using Ganss.Xss;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Proiect_DAW.Data;
//using Proiect_DAW.Models;

//namespace Proiect_DAW.Controllers
//{
//    public class ProductsController : Controller
//    {
//        private readonly ApplicationDbContext db;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly RoleManager<IdentityRole> _roleManager;
//        public ProductsController(
//        ApplicationDbContext context,
//        UserManager<ApplicationUser> userManager,
//        RoleManager<IdentityRole> roleManager
//        )
//        {
//            db = context;
//            _userManager = userManager;
//            _roleManager = roleManager;
//        }

//        // Se adauga articolul in baza de date
//        // Doar utilizatorii cu rolul Editor si Admin pot adauga articole in platforma
//        [HttpPost]
//        [Authorize(Roles = "Editor,Admin")]
//        public IActionResult New(Product product)
//        {
//            var sanitizer = new HtmlSanitizer();

//            // preluam Id-ul utilizatorului care posteaza articolul
//            product.UserId = _userManager.GetUserId(User);

//            if (ModelState.IsValid)
//            {
//                //product.Description = sanitizer.Sanitize(article.Content);

//                db.Products.Add(product);
//                db.SaveChanges();
//                TempData["message"] = "Produsul a fost adaugat";
//                TempData["messageType"] = "alert-success";
//                return RedirectToAction("Index");
//            }
//            else
//            {
//                product.Categ = GetAllCategories();
//                return View(product);
//            }
//        }
//    }
//}
