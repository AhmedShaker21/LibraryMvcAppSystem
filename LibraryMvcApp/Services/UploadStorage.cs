namespace LibraryMvcApp.Services
{
    /// <summary>
    /// Where uploaded books / covers live on disk.
    /// Configured by "Storage:UploadsPath" in appsettings.json (e.g. "D:\\LibraryFiles"),
    /// laid out as {root}\books\*.pdf and {root}\covers\*.jpg and served at /uploads/...
    /// Falls back to wwwroot\uploads when the setting is empty. Files that still sit in the
    /// old wwwroot\uploads folder keep working (read fallback), so nothing breaks while moving.
    /// </summary>
    public class UploadStorage
    {
        public string Root { get; }

        private readonly string? _legacyRoot;

        public UploadStorage(IConfiguration config, IWebHostEnvironment env)
        {
            _legacyRoot = string.IsNullOrEmpty(env.WebRootPath)
                ? null
                : Path.GetFullPath(Path.Combine(env.WebRootPath, "uploads"));

            var configured = config["Storage:UploadsPath"];
            Root = string.IsNullOrWhiteSpace(configured)
                ? (_legacyRoot ?? Path.GetFullPath("uploads"))
                : Path.GetFullPath(configured);

            try
            {
                Directory.CreateDirectory(Root);
                Directory.CreateDirectory(Path.Combine(Root, "books"));
                Directory.CreateDirectory(Path.Combine(Root, "covers"));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Cannot use the uploads folder '{Root}' (Storage:UploadsPath in appsettings.json). " +
                    "Make sure the drive exists and the app has write permission.", ex);
            }
        }

        /// <summary>Physical folder for new uploads, e.g. Folder("books").</summary>
        public string Folder(string sub) => Path.Combine(Root, sub);

        /// <summary>
        /// Maps a stored path like "/uploads/books/x.pdf" to an existing physical file.
        /// Looks in the configured root first, then in the legacy wwwroot\uploads folder.
        /// Never leaves those folders.
        /// </summary>
        public bool TryResolve(string? storedPath, out string fullPath)
        {
            fullPath = string.Empty;

            if (string.IsNullOrWhiteSpace(storedPath))
                return false;

            var rel = storedPath.Replace('\\', '/').TrimStart('/');
            if (rel.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
                rel = rel.Substring("uploads/".Length);

            foreach (var baseDir in new[] { Root, _legacyRoot })
            {
                if (string.IsNullOrEmpty(baseDir))
                    continue;

                var baseFull = Path.GetFullPath(baseDir)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;

                var candidate = Path.GetFullPath(Path.Combine(baseFull, rel));

                if (candidate.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase)
                    && System.IO.File.Exists(candidate))
                {
                    fullPath = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
