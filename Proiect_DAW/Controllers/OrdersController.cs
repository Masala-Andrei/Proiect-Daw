using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_DAW.Data;
using Proiect_DAW.Models;

public class OrdersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _db = context;
        _userManager = userManager;
    }

    // Afișează coșul curent
    public IActionResult Index()
    {
        var cartItems = GetCartItems();
        return View(cartItems);
    }

    // Adaugă produs în coș
    [HttpPost]
    public IActionResult AddToCart(int productId, int quantity)
    {
        var userId = _userManager.GetUserId(User);
        var product = _db.Products.Find(productId);

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

        _db.ProductOrders.Add(cartItem);
        _db.SaveChanges();

        TempData["message"] = "Produsul a fost adăugat în coș.";
        TempData["messageType"] = "alert-success";

        return RedirectToAction("Index");
    }

    // Scoate un produs din coș
    [HttpPost]
    public IActionResult RemoveFromCart(int cartItemId)
    {
        var cartItem = _db.ProductOrders.Find(cartItemId);
        if (cartItem != null)
        {
            _db.ProductOrders.Remove(cartItem);
            _db.SaveChanges();
            TempData["message"] = "Produsul a fost eliminat din coș.";
            TempData["messageType"] = "alert-warning";
        }

        return RedirectToAction("Index");
    }

    // Plasează comanda
    [HttpPost]
    public IActionResult PlaceOrder()
    {
        var userId = _userManager.GetUserId(User);
        var cartItems = _db.Orders.Where(ci => ci.UserId == userId).ToList();

        if (!cartItems.Any())
        {
            TempData["message"] = "Nu aveți produse în coș.";
            TempData["messageType"] = "alert-danger";
            return RedirectToAction("Index");
        }

        // Creăm o comandă cu statusul "Plasată"
        var order = new Order
        {
            UserId = userId,
            Date = DateTime.Now,
            Status = "Plasata"
        };

        _db.Orders.Add(order);
        _db.SaveChanges();

        
        TempData["message"] = "Comanda a fost plasată cu succes.";
        TempData["messageType"] = "alert-success";

        return RedirectToAction("Index", "Products");
    }

    // Funcție pentru a obține articolele din coș
    private List<ProductOrder> GetCartItems()
    {
        var userId = _userManager.GetUserId(User);
        return _db.ProductOrders.Include("Product").Where(ci => ci.Order.UserId == userId && ci.Order.Status != "Plasata").ToList();
    }
}
