using LibraryMvcApp.Helpers;
using LibraryMvcApp.Models;
using LibraryMvcApp.Services.Interfaces;
using LibraryMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryMvcApp.Controllers
{
    /// <summary>
    /// فهرس التعليمات — a standalone, printable page listing every book flagged IsInstruction
    /// (the flag is set when a book is added / edited).
    /// </summary>
    [Authorize]
    public class InstructionsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFolderPermissionService _permissions;

        public InstructionsController(AppDbContext context, IFolderPermissionService permissions)
        {
            _context = context;
            _permissions = permissions;
        }

        private bool IsAdmin => User.IsInRole("Admin");

        // Unify alef shapes / taa marbuta / yaa and drop the leading "ال",
        // so "تعليمات", "التعليمات" and "التعليمات " all normalize to the same value.
        private static string Normalize(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            var n = s.Trim()
                .Replace('أ', 'ا').Replace('إ', 'ا').Replace('آ', 'ا')
                .Replace('ة', 'ه').Replace('ى', 'ي')
                .ToLowerInvariant();

            return n.StartsWith("ال") && n.Length > 2 ? n.Substring(2) : n;
        }

        // =========================
        // GET: /Instructions
        // =========================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new InstructionsVm();

            // Every book flagged IsInstruction, wherever it lives in the folder tree.
            var folders = await _context.Folders
                .AsNoTracking()
                .Select(f => new { f.Id, f.Name, f.ParentFolderId })
                .ToListAsync();

            var byId = folders.ToDictionary(f => f.Id);

            string FolderPath(int? folderId)
            {
                var parts = new List<string>();
                var seen = new HashSet<int>();
                while (folderId != null && byId.TryGetValue(folderId.Value, out var f) && seen.Add(f.Id))
                {
                    parts.Insert(0, f.Name);
                    folderId = f.ParentFolderId;
                }
                return string.Join(" / ", parts);
            }

            var books = await _context.Books
                .AsNoTracking()
                .Include(b => b.Folder)
                .Where(b => b.IsInstruction)
                .ToListAsync();

            vm.Items = books
                .Select(b => new InstructionRow
                {
                    Book = b,
                    SubPath = FolderPath(b.FolderId),
                    CanEdit = b.Folder != null
                        ? _permissions.CanManage(b.Folder, User)
                        : IsAdmin
                })
                .OrderBy(r => r.Book.DocumentNumber, NaturalStringComparer.Instance)
                .ThenBy(r => r.Book.Title, NaturalStringComparer.Instance)
                .ToList();

            vm.IsAdmin = IsAdmin;

            return View(vm);
        }

        // =========================
        // POST: edit document number / issue / amendment data of an instruction
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInstruction(InstructionMetaVm vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["StatusError"] = "القيم المدخلة غير صحيحة.";
                return RedirectToAction(nameof(Index), "Instructions", null, "instructionsSection");
            }

            var book = await _context.Books
                .Include(b => b.Folder)
                .FirstOrDefaultAsync(b => b.Id == vm.Id);

            if (book == null)
                return NotFound();

            bool allowed = book.Folder != null
                ? _permissions.CanManage(book.Folder, User)
                : IsAdmin;

            if (!allowed)
                return Forbid();

            book.DocumentNumber = string.IsNullOrWhiteSpace(vm.DocumentNumber) ? null : vm.DocumentNumber.Trim();
            book.IssueNumber = vm.IssueNumber;
            book.IssueDate = vm.IssueDate;
            book.AmendmentNumber = vm.AmendmentNumber;
            book.AmendmentDate = vm.AmendmentDate;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "تم حفظ بيانات الوثيقة.";
            return RedirectToAction(nameof(Index), "Instructions", null, "instructionsSection");
        }

        // =========================
        // POST: take a book out of the instructions index
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveInstruction(int id)
        {
            var book = await _context.Books
                .Include(b => b.Folder)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            bool allowed = book.Folder != null
                ? _permissions.CanManage(book.Folder, User)
                : IsAdmin;

            if (!allowed)
                return Forbid();

            book.IsInstruction = false;
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "تم إخراج الملف من فهرس التعليمات.";
            return RedirectToAction(nameof(Index), "Instructions", null, "instructionsSection");
        }

        // =========================
        // POST (admin): one-click flag of every book already sitting in the "تعليمات" folder tree
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FlagFromFolder()
        {
            var folders = await _context.Folders
                .AsNoTracking()
                .Select(f => new { f.Id, f.Name, f.ParentFolderId })
                .ToListAsync();

            const string wanted = "تعليمات";

            var roots = folders.Where(f => Normalize(f.Name) == wanted).ToList();
            if (roots.Count == 0)
                roots = folders.Where(f => Normalize(f.Name).Contains(wanted)).ToList();

            if (roots.Count == 0)
            {
                TempData["StatusError"] = "لم يتم العثور على مجلد باسم «تعليمات».";
                return RedirectToAction(nameof(Index), "Instructions", null, "instructionsSection");
            }

            var children = folders
                .Where(f => f.ParentFolderId != null)
                .ToLookup(f => f.ParentFolderId!.Value);

            var visited = new HashSet<int>();
            var queue = new Queue<int>(roots.Select(r => r.Id));

            while (queue.Count > 0)
            {
                var id = queue.Dequeue();
                if (!visited.Add(id))
                    continue;

                foreach (var c in children[id])
                    queue.Enqueue(c.Id);
            }

            var folderIds = visited.ToList();

            var toFlag = await _context.Books
                .Where(b => !b.IsInstruction && b.FolderId != null && folderIds.Contains(b.FolderId.Value))
                .ToListAsync();

            foreach (var b in toFlag)
                b.IsInstruction = true;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = toFlag.Count == 0
                ? "كل الملفات داخل مجلد التعليمات متعلَّمة بالفعل."
                : $"تم تعليم {toFlag.Count} ملف كتعليمات.";

            return RedirectToAction(nameof(Index), "Instructions", null, "instructionsSection");
        }
    }
}
