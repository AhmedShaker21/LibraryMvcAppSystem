using LibraryMvcApp.Models;

namespace LibraryMvcApp.ViewModels
{
    /// <summary>One page of the "كل النماذج" admin list (server-side paging + filtering).</summary>
    public class FormsAllVm
    {
        public List<FormEntry> Items { get; set; } = new();

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; } = 1;

        public int? DepartmentNo { get; set; }
        public string? Q { get; set; }
    }
}
