using LibraryMvcApp.Models;

namespace LibraryMvcApp.ViewModels
{
    public class DepartmentWithLastFormVm
    {
        public Department Department { get; set; } = null!;
        public int LastFormNumber { get; set; }
    }
}
