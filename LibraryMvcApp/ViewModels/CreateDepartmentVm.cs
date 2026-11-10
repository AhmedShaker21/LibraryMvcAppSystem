using System.ComponentModel.DataAnnotations;

namespace LibraryMvcApp.ViewModels
{
    public class CreateDepartmentVm
    {
        [Required(ErrorMessage = "اسم الإدارة مطلوب")]
        public string Name { get; set; }

        [Required(ErrorMessage = "رقم الإدارة مطلوب")]
        [Range(1, 9999)]
        public int Code { get; set; }

        [Required(ErrorMessage = "رقم البداية مطلوب")]
        [Range(0, 9999)]
        public int StartFormNumber { get; set; }
    }
}
