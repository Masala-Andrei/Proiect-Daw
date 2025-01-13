using Proiect_DAW.Data;
using Proiect_DAW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.EntityFrameworkCore;


namespace Proiect_DAW.Controllers
{
    //[Authorize]

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
        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult New()
        {
            Product product = new Product();
            product.Categ = GetAllCategories();
            return View(product);
        }

        [Authorize(Roles = "Colaborator,Admin")]
        [HttpPost]
        public IActionResult New(Product product)
        {
            product.UserId = _userManager.GetUserId(User);

            // Products added by collaborators need validation
            if (User.IsInRole("Colaborator"))
            {
                product.Validated = false; // Mark product as not validated
            }
            else if (User.IsInRole("Admin"))
            {
                product.Validated = true; // Admin-added products are automatically validated
            }

            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();

                TempData["message"] = "Produsul a fost adăugat cu succes.";
                TempData["messageType"] = "alert-success";

                if (User.IsInRole("Colaborator"))
                {
                    return RedirectToAction("ValidateProducts"); // Redirect to validation queue
                }

                return RedirectToAction("Index"); // Admins can immediately see their products
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
                                      .Include("User")
                                       .Where(p => p.Validated);


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

            var productStockStatuses = products.Select(p => new
            {
                ProductId = p.Id,
                StockStatus = p.Stock > 0 ? "In stoc" : "Indisponibil"
            }).ToDictionary(p => p.ProductId, p => p.StockStatus);

            ViewBag.ProductStockStatuses = productStockStatuses;

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


            if (product == null || (!product.Validated && !User.IsInRole("Admin")))
            {
                TempData["message"] = "Produsul nu este disponibil.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }


            double averageRating = product.UserRatings != null && product.UserRatings.Count > 0
            ? product.UserRatings
            .Where(ur => ur.Number >= 1 && ur.Number <= 5) // Filtrează doar ratingurile valide
            .Average(ur => ur.Number) ?? 0.0 // Media ratingurilor valide sau 0.0
            : 0.0;

            product.Rating = averageRating;

            int totalRatings = product.UserRatings?.Count ?? 0;
            ViewBag.TotalRatings = totalRatings;

            int productInCartQuantity = 0;
            int remainingStock = product.Stock - productInCartQuantity;
            ViewBag.ProductInCartQuantity = productInCartQuantity;
            ViewBag.RemainingStock = remainingStock;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

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
                        product.Validated = true;
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

        // Metoda pentru a adăuga produsul în coș
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == productId);

            if (product == null || quantity <= 0)
            {
                TempData["message"] = "Produsul sau cantitatea nu este validă.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = productId });
            }

            // Dacă produsul există și cantitatea este validă, adăugăm în coș
            var userId = _userManager.GetUserId(User);

            var order = db.Orders.FirstOrDefault(c => c.Status != "Plasata" && c.UserId == userId);

            if (order == null)
            {
                // Create a new order if it doesn't exist
                order = new Order
                {
                    UserId = userId,
                    Status = "Neplasata",
                    Date = DateTime.Now
                };
                db.Orders.Add(order);  // Add the new order to the context
                db.SaveChanges();  // Save the order first, so that it gets an ID
            }

            // At this point, the order is guaranteed to have an ID
            var cartItem = db.ProductOrders.FirstOrDefault(c => c.ProductId == productId && c.Order.Status != "Plasata" && c.Order.UserId == userId);

            if (cartItem != null)
            {
                // If the product is already in the cart, update the quantity
                cartItem.Quantity += quantity;
                cartItem.Price = cartItem.Quantity * cartItem.Product.Price;  // Update the price based on the new quantity
            }
            else
            {
                // If the product is not in the cart, add it
                db.ProductOrders.Add(new ProductOrder
                {
                    ProductId = productId,
                    Price = quantity * product.Price,
                    Quantity = quantity,
                    OrderId = order.Id  // Assign the OrderId after saving the Order
                });
            }

            db.SaveChanges();



            TempData["message"] = "Produsul a fost adăugat în coș!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Index"); // Poți modifica ruta pentru a duce către coșul de cumpărături
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

        [HttpGet]
        [Authorize] // Doar utilizatorii autentificați pot adăuga rating-uri
        public IActionResult AddRating(int productId)
        {
            var product = db.Products.FirstOrDefault(p => p.Id == productId && p.Validated);
            if (product == null)
            {
                TempData["message"] = "Produsul nu a fost găsit.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            ViewBag.ProductTitle = product.Title;
            return View(new UserRating { ProductId = productId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Colaborator,RegisteredUser")]
        public IActionResult AddRating(UserRating userRating)
        {
            if (userRating.Number < 1 || userRating.Number > 5)
            {
                ModelState.AddModelError("Number", "Rating-ul trebuie să fie între 1 și 5.");
            }

            if (ModelState.IsValid)
            {
                var existingRating = db.UserRatings
                                       .FirstOrDefault(ur => ur.ProductId == userRating.ProductId && ur.UserId == _userManager.GetUserId(User));

                if (existingRating != null)
                {
                    // Actualizează rating-ul existent
                    existingRating.Number = userRating.Number;
                    TempData["message"] = "Rating-ul a fost actualizat.";
                }
                else
                {
                    // Adaugă un rating nou
                    userRating.UserId = _userManager.GetUserId(User);
                    db.UserRatings.Add(userRating);
                    TempData["message"] = "Rating-ul a fost adăugat.";
                }

                TempData["messageType"] = "alert-success";
                db.SaveChanges();
                return RedirectToAction("Show", new { id = userRating.ProductId });
            }

            var product = db.Products.FirstOrDefault(p => p.Id == userRating.ProductId);
            ViewBag.ProductTitle = product?.Title;
            return View(userRating);
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

