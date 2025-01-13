using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proiect_DAW.Data;
using Proiect_DAW.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Proiect_DAW.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManageUsersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManageUsersController(
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
            var users = from user in db.Users
                        orderby user.UserName
                        select user;

            ViewBag.UsersList = users;

            return View();
        }

        // Show User Details and Roles
        public async Task<IActionResult> Show(string id)
        {
            var user = db.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            ViewBag.Roles = roles;
            ViewBag.AllRoles = allRoles;
            ViewBag.UserCurent = await _userManager.GetUserAsync(User);

            return View(user);
        }

        // POST: Manage User Roles
        [HttpPost]
        public async Task<IActionResult> ManageUserRole(string userId, string selectedRole)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["message"] = "User not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!string.IsNullOrEmpty(selectedRole))
            {
                await _userManager.AddToRoleAsync(user, selectedRole);
                TempData["message"] = $"User's role updated to '{selectedRole}'.";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Role deleted.";
                TempData["messageType"] = "alert-danger";
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;
            ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.UserCurent = await _userManager.GetUserAsync(User);

            return View("Show", user);
        }
    }
}
