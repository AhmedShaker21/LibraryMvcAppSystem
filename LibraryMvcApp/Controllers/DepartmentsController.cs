using LibraryMvcApp.Models;
using LibraryMvcApp.Services.Interfaces;
using LibraryMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryMvcApp.Controllers
{
    [Authorize]
    public class DepartmentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFormRegisterService _service;
        
    private readonly UserManager<IdentityUser> _userManager;

        public DepartmentsController(
            AppDbContext context,
            IFormRegisterService service,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _service = service;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            List<DepartmentWithLastFormVm> result;

            if (User.IsInRole("Admin") || User.IsInRole("Viewer"))
            {
                result = await _context.Departments
                    .Select(d => new DepartmentWithLastFormVm
                    {
                        Department = d,
                        LastFormNumber = _context.FormEntries
                            .Where(f => f.DepartmentId == d.Id)
                            .Max(f => (int?)f.FormNumber) ?? d.StartFormNumber
                    })
                    .ToListAsync();

                return View(result);
            }
            var userId = _userManager.GetUserId(User);

            var userDepartmentId = await _context.UserDepartments
                .Where(x => x.UserId == userId)
                .Select(x => x.DepartmentId)
                .FirstOrDefaultAsync();

            if (userDepartmentId == 0)
            {
                TempData["ErrorMessage"] = "لم يتم ربطك بأي إدارة.";
                return RedirectToAction("AccessDenied", "Account");
            }

            result = await _context.Departments
                .Where(d => d.Id == userDepartmentId)
                .Select(d => new DepartmentWithLastFormVm
                {
                    Department = d,
                    LastFormNumber = _context.FormEntries
                        .Where(f => f.DepartmentId == d.Id)
                        .Max(f => (int?)f.FormNumber) ?? d.StartFormNumber
                })
                .ToListAsync();

            return View(result);
        }

        // =========================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =========================
        // POST: Departments/Create
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartmentVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // منع تكرار رقم الإدارة
            bool codeExists = await _context.Departments
                .AnyAsync(d => d.Code == vm.Code);

            if (codeExists)
            {
                ModelState.AddModelError("Code", "رقم الإدارة موجود بالفعل");
                return View(vm);
            }

            var department = new Department
            {
                Name = vm.Name,                
                Code = vm.Code,
                StartFormNumber = vm.StartFormNumber
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments
                .Include(d => d.FormEntries)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return NotFound();

            if (department.FormEntries.Any())
            {
                TempData["Error"] = "لا يمكن حذف الإدارة لأنها تحتوي على نماذج";
                return RedirectToAction(nameof(Index));
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف الإدارة بنجاح";
            return RedirectToAction(nameof(Index));
        }

    }

}
