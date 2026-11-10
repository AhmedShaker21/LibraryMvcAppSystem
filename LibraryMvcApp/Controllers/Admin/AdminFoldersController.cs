using LibraryMvcApp.Models;
using LibraryMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryMvcApp.Controllers.Admin
{
    [Authorize]
    public class FolderController : Controller
    {
        private readonly AppDbContext _context;

        public FolderController(AppDbContext context)
        {
            _context = context;
        }

        // =======================================================
        // 📌 Index (Root / Folder)
        // =======================================================
        [HttpGet]
        public async Task<IActionResult> Index(int? id)
        {
            Folder folder;

            if (id == null || id == 0)
            {
                // ROOT
                folder = new Folder
                {
                    Id = 0,
                    Name = "Main",
                    SubFolders = await _context.Folders
                        .Where(f => f.ParentFolderId == null)
                        .OrderBy(f => f.Name)
                        .ToListAsync(),
                    Books = await _context.Books
                        .Where(b => b.FolderId == null)
                        .OrderBy(b => b.Title)
                        .ToListAsync()
                };

                ViewBag.Breadcrumb = new List<Folder>();
            }
            else
            {
                folder = await _context.Folders
                    .Include(f => f.SubFolders)
                    .Include(f => f.Books)
                    .FirstOrDefaultAsync(f => f.Id == id.Value);

                if (folder == null)
{
    return RedirectToAction(
        "Index",
        "Error",
        new { message = "الفولدر غير موجود" }
    );
}


                ViewBag.Breadcrumb = await GetBreadcrumbPath(id.Value);
            }

            return View(folder);
        }

        // =======================================================
        // 📌 Create Folder
        // =======================================================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string name,
            int? parentId,
            string? allowedRole)
        {
            if (string.IsNullOrWhiteSpace(name))
                return RedirectToAction(nameof(Index), new { id = parentId });

            if (parentId == 0)
                parentId = null;

            var folder = new Folder
            {
                Name = name.Trim(),
                ParentFolderId = parentId,
                AllowedRole = string.IsNullOrWhiteSpace(allowedRole)
                    ? null
                    : allowedRole.Trim()
            };

            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { id = parentId });
        }

        // =======================================================
        // 📌 Rename Folder
        // =======================================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rename(int id, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return RedirectToAction(nameof(Index));

            var folder = await _context.Folders.FindAsync(id);
            if (folder == null)
                return NotFound();

            bool canManageFolder =
                User.IsInRole("Admin") ||
                (!string.IsNullOrEmpty(folder.AllowedRole) &&
                 User.IsInRole(folder.AllowedRole));

            if (!canManageFolder)
                return Forbid(); // أو Unauthorized()

            folder.Name = newName.Trim();
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { id = folder.ParentFolderId });
        }

        // =======================================================
        //  Delete Folder (Recursive)
        // =======================================================
        [Authorize] // أي يوزر مسجل دخول
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var folder = await _context.Folders
                .Include(f => f.SubFolders)
                .Include(f => f.Books)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (folder == null)
                return NotFound();

            bool canManageFolder =
                User.IsInRole("Admin") ||
                (!string.IsNullOrEmpty(folder.AllowedRole) &&
                 User.IsInRole(folder.AllowedRole));

            if (!canManageFolder)
                return Forbid(); // أو Unauthorized()

            int? parentId = folder.ParentFolderId;

            await DeleteRecursive(folder);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { id = parentId });
        }


        // =======================================================
        // 📌 Recursive Delete Helper
        // =======================================================
        private async Task DeleteRecursive(Folder folder)
        {
            // حذف الكتب
            _context.Books.RemoveRange(folder.Books);

            // حذف الفولدرات الفرعية
            foreach (var sub in folder.SubFolders.ToList())
            {
                var child = await _context.Folders
                    .Include(f => f.SubFolders)
                    .Include(f => f.Books)
                    .FirstAsync(f => f.Id == sub.Id);

                await DeleteRecursive(child);
            }

            _context.Folders.Remove(folder);
        }

        // =======================================================
        // 📌 Search (Folders + Books)
        // =======================================================
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            var vm = new SearchViewModel();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();

                vm.Folders = await _context.Folders
                    .Where(f => f.Name.Contains(q))
                    .OrderBy(f => f.Name)
                    .ToListAsync();

                vm.Books = await _context.Books
                    .Where(b =>
                        b.Title.Contains(q) ||
                        (b.Description != null && b.Description.Contains(q))
                    )
                    .OrderBy(b => b.Title)
                    .ToListAsync();
            }

            return PartialView("_SearchResult", vm);
        }

        // =======================================================
        // 📌 Breadcrumb Helper
        // =======================================================
        private async Task<List<Folder>> GetBreadcrumbPath(int folderId)
        {
            var path = new List<Folder>();
            var current = await _context.Folders.FindAsync(folderId);

            while (current != null)
            {
                path.Add(current);

                if (current.ParentFolderId == null)
                    break;

                current = await _context.Folders.FindAsync(current.ParentFolderId);
            }

            path.Reverse();
            return path;
        }
    }
}
