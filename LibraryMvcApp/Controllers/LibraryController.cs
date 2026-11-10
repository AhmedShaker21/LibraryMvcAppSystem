using LibraryMvcApp.Models;
using LibraryMvcApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Syncfusion.DocIO.DLS;
using Syncfusion.Pdf;
using Syncfusion.DocIORenderer;

namespace LibraryMvcApp.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private static readonly FileExtensionContentTypeProvider ContentTypes = new();

        private readonly AppDbContext _context;
        private readonly UploadStorage _storage;

        public LibraryController(AppDbContext context, UploadStorage storage)
        {
            _context = context;
            _storage = storage;
        }

        public async Task<IActionResult> Index()
        {
            var books = await _context.Books
                .OrderBy(b => b.Title)
                .ToListAsync();

            return View(books);
        }

        // =======================================================
        // Reader page (the file itself is streamed by Preview / Download)
        // =======================================================
        public async Task<IActionResult> Details(int id, int? folderId)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            ViewBag.FolderId = folderId ?? book.FolderId;
            ViewBag.FileExists = TryResolvePath(book, out _);

            return View(book);
        }

        // =======================================================
        // Inline preview — PDF is streamed as-is, Word is converted to PDF
        // =======================================================
        public async Task<IActionResult> Preview(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null || !TryResolvePath(book, out var fullPath))
                return NotFound();

            var type = (book.FileType ?? string.Empty).Trim().ToLowerInvariant();

            if (type == "pdf")
                return PhysicalFile(fullPath, "application/pdf", enableRangeProcessing: true);

            if (type == "doc" || type == "docx")
            {
                try
                {
                    return File(ConvertWordToPdf(fullPath), "application/pdf");
                }
                catch (Exception)
                {
                    // conversion failed → fall back to the original file
                    return RedirectToAction(nameof(Download), new { id });
                }
            }

            return RedirectToAction(nameof(Download), new { id });
        }

        // =======================================================
        // Download original file
        // =======================================================
        public async Task<IActionResult> Download(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null || !TryResolvePath(book, out var fullPath))
                return NotFound();

            if (!ContentTypes.TryGetContentType(fullPath, out var contentType))
                contentType = "application/octet-stream";

            var extension = Path.GetExtension(fullPath);
            var safeTitle = string.Concat(
                (string.IsNullOrWhiteSpace(book.Title) ? "file" : book.Title.Trim())
                    .Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

            return PhysicalFile(fullPath, contentType, safeTitle + extension, enableRangeProcessing: true);
        }

        // =======================================================
        // Helpers
        // =======================================================

        /// <summary>
        /// Resolves book.FilePath (e.g. "/uploads/books/xxx.pdf") to a physical file under the
        /// configured uploads folder (Storage:UploadsPath), with the old wwwroot\uploads as fallback.
        /// Returns false when the path is empty, escapes those folders, or the file is missing.
        /// </summary>
        private bool TryResolvePath(Book book, out string fullPath)
            => _storage.TryResolve(book.FilePath, out fullPath);

        private static byte[] ConvertWordToPdf(string fullPath)
        {
            using FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            WordDocument wordDoc = new WordDocument(fileStream, Syncfusion.DocIO.FormatType.Automatic);
            DocIORenderer renderer = new DocIORenderer();

            try
            {
                PdfDocument pdfDocument = renderer.ConvertToPDF(wordDoc);

                using MemoryStream pdfStream = new MemoryStream();
                pdfDocument.Save(pdfStream);
                pdfDocument.Close(true);

                return pdfStream.ToArray();
            }
            finally
            {
                renderer.Dispose();
                wordDoc.Close();
            }
        }
    }
}
