using System;

namespace LibraryMvcApp.Models
{
    public class Book
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Author { get; set; }

        public string FilePath { get; set; } = string.Empty;
        public string CoverImagePath { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public int? FolderId { get; set; }
        public Folder? Folder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // true = this book is an instruction / procedure and appears in the "موقف المكتبة" instructions index
        public bool IsInstruction { get; set; }

        // ---- Document control data (Library Status / موقف المكتبة) ----
        public string? DocumentNumber { get; set; }   // رقم الوثيقة
        public int? IssueNumber { get; set; }         // رقم الإصدار
        public DateTime? IssueDate { get; set; }      // تاريخ الإصدار
        public int? AmendmentNumber { get; set; }     // رقم التعديل
        public DateTime? AmendmentDate { get; set; }  // تاريخ التعديل
    }
}
