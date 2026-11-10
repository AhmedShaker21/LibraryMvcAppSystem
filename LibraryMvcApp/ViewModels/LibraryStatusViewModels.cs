using System.ComponentModel.DataAnnotations;
using LibraryMvcApp.Models;

namespace LibraryMvcApp.ViewModels
{
    public class LibraryStatusVm
    {
        public List<FormEntry> Forms { get; set; } = new();
        public bool CanEditForms { get; set; }

        /// <summary>FormEntry.Id -> Book.Id of the library file that holds that form (when one is found).</summary>
        public Dictionary<int, int> FormBookIds { get; set; } = new();

    }

    public class InstructionsVm
    {
        public List<InstructionRow> Items { get; set; } = new();

        /// <summary>Admins get the "flag everything inside the تعليمات folder" shortcut.</summary>
        public bool IsAdmin { get; set; }
    }

    public class InstructionRow
    {
        public Book Book { get; set; } = null!;

        /// <summary>Full folder path of the book (e.g. "تعليمات / قطاع الجودة").</summary>
        public string SubPath { get; set; } = string.Empty;

        public bool CanEdit { get; set; }
    }

    /// <summary>Posted from the "edit issue / amendment" dialog of the forms index.</summary>
    public class FormMetaVm
    {
        public int Id { get; set; }

        [Range(0, 9999)]
        public int? IssueNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? IssueDate { get; set; }

        [Range(0, 9999)]
        public int? AmendmentNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? AmendmentDate { get; set; }
    }

    /// <summary>Posted from the "edit" dialog of the instructions index.</summary>
    public class InstructionMetaVm : FormMetaVm
    {
        [StringLength(100)]
        public string? DocumentNumber { get; set; }
    }
}
