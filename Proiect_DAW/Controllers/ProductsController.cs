using Proiect_DAW.Data;
using Proiect_DAW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.EntityFrameworkCore;


namespace Proiect_DAW.Controllers
{
    [Authorize]

    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ProductsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult New()
        {
            Product product = new Product();

            //product.Rating = 1;

            product.Categ = GetAllCategories();

            return View(product);
        }

        // Se adauga articolul in baza de date
        // Doar utilizatorii cu rolul Editor si Admin pot adauga articole in platforma
        [Authorize(Roles = "Colaborator,Admin")]
        [HttpPost]

        public IActionResult New(Product product)
        {
            //var sanitizer = new HtmlSanitizer();


            // preluam Id-ul utilizatorului care posteaza articolul
            product.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {

                db.Products.Add(product);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                product.Categ = GetAllCategories();
                return View(product);
            }
        }

        public IActionResult Index()
        {
            var products = db.Products.Include("Category")
                                      .Include("User");
            // MOTOR DE CAUTARE

            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 

                // Cautare in articol (Title si Content)

                List<int> articleIds = db.Products.Where
                                        (
                                         at => at.Title.Contains(search)
                                         || at.Description.Contains(search)
                                        ).Select(a => a.Id).ToList();

                // Cautare in comentarii (Content)
                List<int> articleIdsOfCommentsWithSearchString = db.Reviews
                                        .Where
                                        (
                                         c => c.Content.Contains(search)
                                        ).Select(c => (int)c.ProductId).ToList();

                // Se formeaza o singura lista formata din toate id-urile selectate anterior
                List<int> mergedIds = articleIds.Union(articleIdsOfCommentsWithSearchString).ToList();


                // Lista articolelor care contin cuvantul cautat
                // fie in articol -> Title si Content
                // fie in comentarii -> Content
                products = db.Products.Where(article => mergedIds.Contains(article.Id))
                                      .Include("Category")
                                      .Include("User");

            }

            // ViewBag.OriceDenumireSugestiva
            ViewBag.Products = products;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }


            return View();
        }

        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Colaborator"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }

        public IActionResult Show(int id)
        {
            Product product = db.Products.Include("Category")
                                         .Include("Reviews")
                                         .Include("User")
                                         .Include("Reviews.User")
                                         .Include("UserRatings")
                                          .Where(prod => prod.Id == id)
                                          .First();





            double averageRating = product.UserRatings != null && product.UserRatings.Count > 0
            ? product.UserRatings
            .Where(ur => ur.Number >= 1 && ur.Number <= 5) // Filtrează doar ratingurile valide
            .Average(ur => ur.Number) ?? 0.0 // Media ratingurilor valide sau 0.0
            : 0.0;

            product.Rating = averageRating;


            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(product);
        }


        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult Edit(int id)
        {

            Product product = db.Products.Include("Category")
                                         .Where(prod => prod.Id == id)
                                         .First();

            product.Categ = GetAllCategories();

            if ((product.UserId == _userManager.GetUserId(User)) ||
                User.IsInRole("Admin"))
            {
                return View(product);
            }
            else
            {

                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult Edit(int id, Product requestProduct)
        {
            Product product = db.Products.Find(id);

            if (ModelState.IsValid)
            {
                if ((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                {
                    product.Title = requestProduct.Title;
                    product.Description = requestProduct.Description;
                    product.CategoryId = requestProduct.CategoryId;
                    product.Price = requestProduct.Price;
                    product.Stock = requestProduct.Stock;

                    if (User.IsInRole("Admin"))
                    {
                        product.Validated = requestProduct.Validated;
                    }
                    else
                    {
                        product.Validated = false;
                    }

                    TempData["message"] = "Produsul a fost modificat cu succes.";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveți dreptul să faceți modificări asupra unui produs care nu vă aparține.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestProduct.Categ = GetAllCategories();
                return View(requestProduct);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show([FromForm] Review review)
        {
            // Setăm data și utilizatorul curent
            review.Date = DateTime.Now;
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "Utilizatorul nu este autentificat.");
            }
            else
            {
                review.UserId = userId;
            }

            // Verificăm validitatea modelului
            if (ModelState.IsValid)
            {
                try
                {
                    db.Reviews.Add(review);
                    db.SaveChanges();
                    return Redirect("/Products/Show/" + review.ProductId);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Eroare la salvarea review-ului: " + ex.Message);
                }
            }

            // În caz de eroare, reîncărcăm pagina produsului
            Product prod = db.Products.Include("Category")
                                      .Include("User")
                                      .Include("Reviews")
                                      .Include("Reviews.User")
                                      .FirstOrDefault(p => p.Id == review.ProductId);

            if (prod == null)
            {
                return NotFound("Produsul nu a fost găsit.");
            }

            ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
            return View(prod);
        }



        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Include("Reviews")
                                         .Where(prod => prod.Id == id)
                                         .FirstOrDefault();

            if (product == null)
            {
                TempData["message"] = "Produsul nu a fost găsit.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            if ((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                db.Products.Remove(product);
                db.SaveChanges();

                TempData["message"] = "Produsul a fost șters cu succes.";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveți dreptul să ștergeți acest produs.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }


        [Authorize(Roles = "Admin")]
        public IActionResult ValidateProducts()
        {
            var unvalidatedProducts = db.Products
                                        .Where(p => !p.Validated)
                                        .Include(p => p.Category)
                                        .ToList();

            return View(unvalidatedProducts);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ValidateProducts(int id)
        {
            var product = db.Products.Find(id);

            if (product == null)
            {
                TempData["message"] = "Produsul nu a fost găsit.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("ValidateProducts");
            }

            product.Validated = true;
            db.SaveChanges();

            TempData["message"] = "Produsul a fost validat cu succes.";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("ValidateProducts");
        }


        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName
                });
            }
            

            return selectList;
        }
    }
}

