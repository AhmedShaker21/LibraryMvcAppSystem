using LibraryMvcApp.Models;
using LibraryMvcApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryMvcApp.Controllers.Admin
{
    [Authorize]
    public class AdminBooksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UploadStorage _storage;

        public AdminBooksController(AppDbContext context, UploadStorage storage)
        {
            _context = context;
            _storage = storage;
        }

        // ============================================================
        // GET: Create Book
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Create(int folderId)
        {
            var folder = await _context.Folders.FindAsync(folderId);
            if (folder == null)
                return NotFound();

            ViewBag.FolderId = folderId;
            return View();
        }

        // ============================================================
        // POST: Create Books (MULTI)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string title,
            string? description,
            string? author,
            int folderId,
            bool isInstruction,
            List<IFormFile> files,
            IFormFile? coverImage)
        {
            var folder = await _context.Folders.FindAsync(folderId);
            if (folder == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("", "Title is required");
                ViewBag.FolderId = folderId;
                return View();
            }

            if (files == null || !files.Any())
            {
                ModelState.AddModelError("", "At least one book file is required");
                ViewBag.FolderId = folderId;
                return View();
            }

            foreach (var file in files)
            {
                if (file.Length == 0)
                    continue;

                var book = new Book
                {
                    Title = title,
                    Description = description,
                    Author = author,
                    FolderId = folderId,
                    IsInstruction = isInstruction
                };

                await UploadBookFile(book, file);
                await UploadCover(book, coverImage);

                _context.Books.Add(book);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Folder", new { id = folderId });
        }

        // ============================================================
        // GET: Edit Book
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books
                .Include(b => b.Folder)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            ViewBag.FolderId = book.FolderId;
            return View(book);
        }

        // ============================================================
        // POST: Edit Book
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string title,
            string? description,
            string? author,
            int folderId,
            bool isInstruction,
            IFormFile? file,
            IFormFile? coverImage)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("", "Title is required");
                ViewBag.FolderId = folderId;
                return View(book);
            }

            book.Title = title;
            book.Description = description;
            book.Author = author;
            book.IsInstruction = isInstruction;

            await UploadBookFile(book, file);
            await UploadCover(book, coverImage);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Folder", new { id = book.FolderId });
        }

        // ============================================================
        // POST: Delete Book
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int folderId)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Folder", new { id = folderId });
        }

        // ============================================================
        // PRIVATE HELPERS
        // ============================================================
        private async Task UploadBookFile(Book book, IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return;

            var uploadsPath = _storage.Folder("books");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            book.FilePath = $"/uploads/books/{fileName}";
            book.FileType = Path.GetExtension(file.FileName).TrimStart('.');
        }

        private async Task UploadCover(Book book, IFormFile? coverImage)
        {
            if (coverImage == null || coverImage.Length == 0)
                return;

            var uploadsPath = _storage.Folder("covers");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(coverImage.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await coverImage.CopyToAsync(stream);

            book.CoverImagePath = $"/uploads/covers/{fileName}";
        }
    }
}
