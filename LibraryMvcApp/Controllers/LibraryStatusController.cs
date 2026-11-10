using LibraryMvcApp.Models;
using LibraryMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryMvcApp.Controllers
{
    /// <summary>
    /// موقف المكتبة — printable forms index (from FormEntries).
    /// The instructions index lives on its own page: see InstructionsController.
    /// </summary>
    [Authorize]
    public class LibraryStatusController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LibraryStatusController(
            AppDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private bool IsAdmin => User.IsInRole("Admin");

        private async Task<int?> GetCurrentUserDepartmentIdAsync()
        {
            var userId = _userManager.GetUserId(User);

            return await _context.UserDepartments
                .Where(x => x.UserId == userId)
                .Select(x => (int?)x.DepartmentId)
                .FirstOrDefaultAsync();
        }

        // =========================
        // GET: /LibraryStatus
        // =========================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new LibraryStatusVm();

            // ---------- 1) Forms index ----------
            // Same visibility rule as FormsRegister: admin sees all, others only their department.
            IQueryable<FormEntry> formsQuery = _context.FormEntries.AsNoTracking();

            if (!IsAdmin && !User.IsInRole("Viewer"))
            {
                var deptId = await GetCurrentUserDepartmentIdAsync();
                formsQuery = deptId == null
                    ? formsQuery.Where(_ => false)
                    : formsQuery.Where(f => f.DepartmentId == deptId);
            }

            vm.Forms = await formsQuery
                .OrderBy(f => f.DepartmentNo)
                .ThenBy(f => f.FormNumber)
                .ToListAsync();

            // every listed form is one this user is allowed to edit
            vm.CanEditForms = vm.Forms.Count > 0 && !Helpers.ReadOnlyRoleFilter.IsReadOnly(User);

            // ---------- link every form to its file in the library (for the "عرض" button) ----------
            if (vm.Forms.Count > 0)
            {
                var books = await _context.Books
                    .AsNoTracking()
                    .Select(b => new { b.Id, b.Title })
                    .ToListAsync();

                var normBooks = books
                    .Select(b => (b.Id, Key: NormalizeKey(b.Title)))
                    .Where(b => b.Key.Length > 0)
                    .ToList();

                foreach (var f in vm.Forms)
                {
                    var numberKey = NormalizeKey(f.FullNumber);
                    var nameKey = NormalizeKey(f.FormName);

                    int bestId = 0, bestScore = 0;

                    foreach (var (id, key) in normBooks)
                    {
                        int score = 0;

                        if (numberKey.Length >= 4 && key.Contains(numberKey)) score = 3;          // title carries the form number
                        else if (nameKey.Length >= 4 && key == nameKey) score = 2;                // same name
                        else if (nameKey.Length >= 6 && (key.Contains(nameKey) ||
                                 (key.Length >= 6 && nameKey.Contains(key)))) score = 1;         // name inside title (or the reverse)

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestId = id;
                            if (score == 3) break;
                        }
                    }

                    if (bestId != 0)
                        vm.FormBookIds[f.Id] = bestId;
                }
            }

            return View(vm);
        }

        // Lower-case, Arabic-Indic digits -> Latin, unify alef / taa marbuta / yaa and keep letters+digits only,
        // so "N/2/01E", "n-2-01e" and "N 2 01E" compare equal.
        private static string NormalizeKey(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            var sb = new System.Text.StringBuilder(s.Length);
            foreach (var ch in s.ToLowerInvariant())
            {
                var c = ch switch
                {
                    >= '\u0660' and <= '\u0669' => (char)('0' + (ch - '\u0660')),
                    >= '\u06F0' and <= '\u06F9' => (char)('0' + (ch - '\u06F0')),
                    'أ' or 'إ' or 'آ' => 'ا',
                    'ة' => 'ه',
                    'ى' => 'ي',
                    _ => ch
                };

                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
            }

            return sb.ToString();
        }

        // =========================
        // POST: edit issue / amendment data of a form
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateForm(FormMetaVm vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["StatusError"] = "القيم المدخلة غير صحيحة.";
                return RedirectToAction(nameof(Index), "LibraryStatus", null, "formsSection");
            }

            var form = await _context.FormEntries.FindAsync(vm.Id);
            if (form == null)
                return NotFound();

            if (!IsAdmin)
            {
                var deptId = await GetCurrentUserDepartmentIdAsync();
                if (deptId == null || deptId != form.DepartmentId)
                    return Forbid();
            }

            if (vm.IssueNumber.HasValue) form.Review = vm.IssueNumber.Value;
            if (vm.IssueDate.HasValue) form.CreatedAt = vm.IssueDate.Value;   // CreatedAt is not nullable
            form.AmendmentNumber = vm.AmendmentNumber;
            form.AmendmentDate = vm.AmendmentDate;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "تم حفظ بيانات النموذج.";
            return RedirectToAction(nameof(Index), "LibraryStatus", null, "formsSection");
        }
    }
}
