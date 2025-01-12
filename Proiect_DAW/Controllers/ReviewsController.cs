using Proiect_DAW.Data;
using Proiect_DAW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.EntityFrameworkCore;


namespace Proiect_DAW.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ReviewsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            // Retrieve the review from the database
            var review = db.Reviews.FirstOrDefault(r => r.Id == id);

            // If review not found, return NotFound
            if (review == null)
            {
                return NotFound("Review-ul nu a fost găsit.");
            }

            // Only allow the user who created the review or an admin to edit it
            var userId = _userManager.GetUserId(User);
            if (review.UserId != userId && !User.IsInRole("Admin"))
            {
                TempData["message"] = "Nu aveți permisiunea să editați acest review.";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Products/Show/" + review.ProductId);
            }

            // Pass the review to the view for editing
            return View(review);
        }


        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id, Review requestReview)
        {
            Review review = db.Reviews.Find(id);

            if (ModelState.IsValid)
            {
                if ((review.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                {
                    review.Content = requestReview.Content;
                    review.Date = DateTime.Now;

                    db.SaveChanges();

                    TempData["message"] = "Review-ul a fost modificat cu succes.";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Show", "Products", new { id = review.ProductId });
                }
                else
                {
                    TempData["message"] = "Nu aveți dreptul să modificați acest review.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Show", "Products", new { id = review.ProductId });
                }
            }
            else
            {
                requestReview.Product = db.Products.FirstOrDefault(p => p.Id == requestReview.ProductId);
                return View(requestReview);
            }
        }



        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id)
        {
            var review = db.Reviews.FirstOrDefault(r => r.Id == id);

            if (review == null)
            {
                TempData["message"] = "Review-ul nu a fost găsit.";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Products/Show/" + review.ProductId);
            }

            var userId = _userManager.GetUserId(User);
            if (review.UserId != userId && !User.IsInRole("Admin"))
            {
                TempData["message"] = "Nu aveți permisiunea să ștergeți acest review.";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Products/Show/" + review.ProductId);
            }

            try
            {
                db.Reviews.Remove(review);
                db.SaveChanges();

                TempData["message"] = "Review-ul a fost șters cu succes.";
                TempData["messageType"] = "alert-success";
            }
            catch (Exception ex)
            {
                TempData["message"] = "Eroare la ștergerea review-ului: " + ex.Message;
                TempData["messageType"] = "alert-danger";
            }

            return Redirect("/Products/Show/" + review.ProductId);
        }
    }
}
    