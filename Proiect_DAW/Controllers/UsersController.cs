//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Proiect_DAW.Data;
//using Proiect_DAW.Models;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Proiect_DAW.Controllers
//{
//    [Authorize(Roles = "Admin")]
//    public class UsersController : Controller
//    {
//        private readonly ApplicationDbContext db;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly RoleManager<IdentityRole> _roleManager;

//        public UsersController(
//        ApplicationDbContext context,
//        UserManager<ApplicationUser> userManager,
//        RoleManager<IdentityRole> roleManager
//        )
//        {
//            db = context;
//            _userManager = userManager;
//            _roleManager = roleManager;
//        }

//        // GET: Users
//        public async Task<IActionResult> Index()
//        {
//            var users = _userManager.Users.ToList();
//            var roles = _roleManager.Roles.ToList();

//            var model = new UserRoles
//            {
//                Users = users,
//                Roles = roles
//            };

//            return View(model);
//        }

//        // POST: Manage User Roles
//        [HttpPost]
//        public async Task<IActionResult> ManageUserRole(string userId, string role)
//        {
//            var user = await _userManager.FindByIdAsync(userId);

//            if (user == null)
//            {
//                TempData["message"] = "User not found.";
//                TempData["messageType"] = "alert-danger";
//                return RedirectToAction("Index");
//            }

//            var currentRoles = await _userManager.GetRolesAsync(user);

//            if (currentRoles.Contains(role))
//            {
//                await _userManager.RemoveFromRoleAsync(user, role);
//                TempData["message"] = $"Role '{role}' removed from user.";
//                TempData["messageType"] = "alert-success";
//            }
//            else
//            {
//                await _userManager.AddToRoleAsync(user, role);
//                TempData["message"] = $"Role '{role}' added to user.";
//                TempData["messageType"] = "alert-success";
//            }

//            return RedirectToAction("Index");
//        }
//    }
//}
