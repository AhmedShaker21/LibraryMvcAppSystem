//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;

//namespace LibraryMvcApp.Controllers.Admin
//{
//    [Authorize(Roles = "Admin")]
//    public class AdminUsersController : Controller
//    {
//        private readonly UserManager<IdentityUser> _userManager;
//        private readonly RoleManager<IdentityRole> _roleManager;

//        public AdminUsersController(
//            UserManager<IdentityUser> userManager,
//            RoleManager<IdentityRole> roleManager)
//        {
//            _userManager = userManager;
//            _roleManager = roleManager;
//        }

//        // =======================
//        // LIST USERS
//        // =======================
//        public IActionResult Index()
//        {
//            var users = _userManager.Users.ToList();
//            return View(users);
//        }

//        // =======================
//        // CREATE USER (GET)
//        // =======================
//        [HttpGet]
//        public IActionResult Create()
//        {
//            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
//            return View();
//        }

//        // =======================
//        // CREATE USER (POST)
//        // =======================
//        [HttpPost]
//        public async Task<IActionResult> Create(string email, string password, string role)
//        {
//            if (string.IsNullOrWhiteSpace(email) ||
//                string.IsNullOrWhiteSpace(password))
//            {
//                ModelState.AddModelError("", "Email and Password are required");
//                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
//                return View();
//            }

//            var user = new IdentityUser
//            {
//                Email = email,
//                UserName = email
//            };

//            var result = await _userManager.CreateAsync(user, password);

//            if (!result.Succeeded)
//            {
//                foreach (var err in result.Errors)
//                    ModelState.AddModelError("", err.Description);

//                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
//                return View();
//            }

//            // Assign Role
//            if (!string.IsNullOrEmpty(role))
//            {
//                await _userManager.AddToRoleAsync(user, role);
//            }

//            return RedirectToAction(nameof(Index));
//        }

//        // =======================
//        // DELETE USER
//        // =======================
//        [HttpPost]
//        public async Task<IActionResult> Delete(string id)
//        {
//            var user = await _userManager.FindByIdAsync(id);

//            if (user != null)
//            {
//                await _userManager.DeleteAsync(user);
//            }

//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
using LibraryMvcApp.Models;
using LibraryMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryMvcApp.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUsersController(
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // =======================
        // LIST USERS
        // =======================
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // =======================
        // CREATE USER (GET)
        // =======================
        [HttpGet]
        public IActionResult Create()
        {
            ReloadLists();
            return View(new CreateUserVM());
        }

        // =======================
        // CREATE USER (POST)
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVM model)
        {
            // 1️⃣ Validation
            if (!ModelState.IsValid)
            {
                ReloadLists();
                return View(model);
            }

            // 2️⃣ تأكد إن Department موجود
            var departmentExists = _context.Departments
                .Any(d => d.Id == model.DepartmentId);

            if (!departmentExists)
            {
                TempData["ErrorMessage"] = "الإدارة المختارة غير موجودة.";
                return RedirectToAction("AccessDenied", "Account");
            }

            // 3️⃣ تأكد إن Role موجود
            if (!string.IsNullOrEmpty(model.Role))
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.Role);
                if (!roleExists)
                {
                    TempData["ErrorMessage"] = "الدور (Role) المختار غير موجود.";
                    return RedirectToAction("AccessDenied", "Account");
                }
            }

            // 4️⃣ إنشاء المستخدم
            var user = new IdentityUser
            {
                Email = model.Email,
                UserName = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);

                ReloadLists();
                return View(model);
            }

            // 5️⃣ إضافة Role
            if (!string.IsNullOrEmpty(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            // 6️⃣ ربط المستخدم بالإدارة
            _context.UserDepartments.Add(new UserDepartment
            {
                UserId = user.Id,
                DepartmentId = model.DepartmentId
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =======================
        // DELETE USER
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "المستخدم غير موجود.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var userDepartment = _context.UserDepartments
                .FirstOrDefault(x => x.UserId == user.Id);

            if (userDepartment != null)
            {
                _context.UserDepartments.Remove(userDepartment);
            }

            await _userManager.DeleteAsync(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =======================
        // Helper
        // =======================
        private void ReloadLists()
        {
            ViewBag.Roles = _roleManager.Roles
                .Select(r => r.Name!)
                .ToList();

            ViewBag.Departments = _context.Departments
                .OrderBy(d => d.Code)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"{d.Code} - {d.Name}"
                })
                .ToList();
        }
    }
}

