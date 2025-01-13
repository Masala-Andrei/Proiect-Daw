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
        if (userId == null)
        {
            TempData["message"] = "Trebuie să fiți autentificat pentru a adăuga produse în coș.";
            TempData["messageType"] = "alert-danger";
            return RedirectToAction("Index", "Products");
        }

        var product = db.Products.Find(productId);

        if (product == null || quantity <= 0 || quantity > product.Stock)
        {
            TempData["message"] = "Produsul nu există sau cantitatea solicitată nu este disponibilă.";
            TempData["messageType"] = "alert-danger";
            return RedirectToAction("Show", "Products", new { id = productId });
        }

        // Check if the user already has an active "Pending" order
        var existingOrder = db.Orders.FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");

        if (existingOrder == null)
        {
            // Create a new order if there is no existing "Pending" order
            existingOrder = new Order
            {
                UserId = userId,
                Date = DateTime.Now,
                Status = "Pending"
            };
            db.Orders.Add(existingOrder);
            db.SaveChanges();
        }

        // Check if the product is already in the cart
        var existingCartItem = db.ProductOrders.FirstOrDefault(po => po.ProductId == productId && po.OrderId == existingOrder.Id);

        if (existingCartItem != null)
        {
            // If the product exists, update its quantity
            existingCartItem.Quantity += quantity;

            // Ensure the new quantity does not exceed stock
            //if (existingCartItem.Quantity > product.Stock)
            //{
            //    TempData["message"] = "Cantitatea solicitată depășește stocul disponibil.";
            //    TempData["messageType"] = "alert-danger";
            //    return RedirectToAction("Show", "Products", new { id = productId });
            //}

            // Update price based on new quantity
            existingCartItem.Price = existingCartItem.Quantity * product.Price;
        }
        else
        {
            // If the product doesn't exist in the cart, add it as a new item
            var cartItem = new ProductOrder
            {
                ProductId = productId,
                Quantity = quantity,
                Price = quantity * product.Price,
                OrderId = existingOrder.Id
            };

            db.ProductOrders.Add(cartItem);
        }

        // Decrease stock in the database
        product.Stock -= quantity;

        db.SaveChanges();

        // Provide a success message to the user
        TempData["message"] = "Produsul a fost adăugat în coș.";
        TempData["messageType"] = "alert-success";

        return RedirectToAction("Show", "Products", new { id = productId });
    }




    // Scoate un produs din coș

    [HttpPost]
    public IActionResult RemoveFromCart(int cartItemId)
    {
        var cartItem = db.ProductOrders.FirstOrDefault(po => po.Id == cartItemId);

        if (cartItem != null)
        {
            var product = db.Products.Find(cartItem.ProductId);

            if (product != null)
            {
                // Increase stock because the product is removed from the cart
                product.Stock += cartItem.Quantity;
            }

            // Remove the cart item
            db.ProductOrders.Remove(cartItem);
            db.SaveChanges();

            TempData["message"] = "Produsul a fost eliminat din coș.";
            TempData["messageType"] = "alert-warning";
        }
        else
        {
            TempData["message"] = "Produsul nu a fost găsit în coș.";
            TempData["messageType"] = "alert-danger";
        }

        return RedirectToAction("Index");
    }



    // Plasează comanda
    [HttpPost]
    [HttpPost]
    public IActionResult PlaceOrder()
    {
        var userId = _userManager.GetUserId(User);
        var existingOrder = db.Orders.FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");

        if (existingOrder == null)
        {
            TempData["message"] = "Nu aveți produse în coș.";
            TempData["messageType"] = "alert-danger";
            return RedirectToAction("Index");
        }

        var cartItems = db.ProductOrders.Where(ci => ci.OrderId == existingOrder.Id).ToList();

        if (!cartItems.Any())
        {
            TempData["message"] = "Coșul este gol.";
            TempData["messageType"] = "alert-danger";
            return RedirectToAction("Index");
        }

        // Create a new order with status "Plasata"
        var order = new Order
        {
            UserId = userId,
            Date = DateTime.Now,
            Status = "Plasata"
        };

        db.Orders.Add(order);
        db.SaveChanges();

        // Remove cart items from the database
        foreach (var cartItem in cartItems)
        {
            db.ProductOrders.Remove(cartItem);
        }

        // Update the status of the existing order to "Plasata"
        existingOrder.Status = "Plasata";
        db.SaveChanges();

        // Provide success message
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