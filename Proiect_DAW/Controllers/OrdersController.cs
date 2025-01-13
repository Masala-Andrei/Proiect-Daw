using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_DAW.Data;
using Proiect_DAW.Models;

public class OrdersController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        db = context;
        _userManager = userManager;
    }

    [Authorize(Roles = "User,Editor,Admin")]

    // Afișează coșul curent
    public IActionResult Index()
    {
        var cartItems = GetCartItems();
        return View(cartItems);
    }

    // Adaugă produs în coș
    [HttpPost]
    [Authorize(Roles = "User, Editor, Admin")]
    public IActionResult AddToCart(int productId, int quantity)
    {
        var userId = _userManager.GetUserId(User);
        var product = db.Products.Find(productId);

        if (product == null || quantity <= 0 || quantity > product.Stock)
        {
            TempData["message"] = "Produsul nu există sau cantitatea solicitată nu este disponibilă.";
            TempData["messageType"] = "alert-danger";
            return RedirectToAction("Index", "Products");
        }

        var cartItem = new ProductOrder
        {
            ProductId = productId,
            Quantity = quantity,
            Price = quantity * product.Price,
            Product = product
        };

        db.ProductOrders.Add(cartItem);
        db.SaveChanges();

        TempData["message"] = "Produsul a fost adăugat în coș.";
        TempData["messageType"] = "alert-success";

        return RedirectToAction("Index");
    }

    // Scoate un produs din coș
    [HttpPost]
    [Authorize(Roles = "User,Editor,Admin")]
    public IActionResult RemoveFromCart(int cartItemId, int orderId)
    {
        // Căutăm produsul în coșul de cumpărături pe baza cartItemId
        var cartItem = db.ProductOrders
                          .FirstOrDefault(c => c.Id == cartItemId && c.OrderId == orderId);

        // Verificăm dacă produsul există și dacă OrderId este corect
        if (cartItem != null)
        {
            // Eliminăm produsul din coș
            db.ProductOrders.Remove(cartItem);
            db.SaveChanges();

            // Mesaj de succes
            TempData["message"] = "Produsul a fost eliminat din coș.";
            TempData["messageType"] = "alert-warning";
        }
        else
        {
            // Dacă produsul nu a fost găsit sau OrderId nu se potrivește
            TempData["message"] = "Produsul nu a fost găsit sau nu face parte din coșul tău.";
            TempData["messageType"] = "alert-danger";
        }

        // Redirecționăm utilizatorul către pagina principală a coșului
        return RedirectToAction("Index");
    }


    // Plasează comanda
    [HttpPost]
    [Authorize(Roles = "User,Editor,Admin")]
    public IActionResult PlaceOrder()
    {
        var userId = _userManager.GetUserId(User);
        var cartItems = db.Orders.Where(ci => ci.UserId == userId).ToList();

        if (!cartItems.Any())
        {
            TempData["message"] = "Nu aveți produse în coș.";
            TempData["messageType"] = "alert-danger";
            return RedirectToAction("Index");
        }
        var order = db.Orders.FirstOrDefault(c => c.Status != "Plasata" && c.UserId == userId);

        order.Status = "Plasata";
       
        db.SaveChanges();

        
        TempData["message"] = "Comanda a fost plasată cu succes.";
        TempData["messageType"] = "alert-success";

        return RedirectToAction("Index", "Products");
    }

    // Funcție pentru a obține articolele din coș
    private List<ProductOrder> GetCartItems()
    {
        var userId = _userManager.GetUserId(User);
        return db.ProductOrders.Include("Product").Where(ci => ci.Order.UserId == userId && ci.Order.Status != "Plasata").ToList();
    }
}
