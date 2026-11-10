using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LibraryMvcApp.Models
{
    public class FormUploadVm
    {
        [Required]
        public int DepartmentNo { get; set; }

        [Required]
        public IFormFile PdfFile { get; set; } = null!;
    }
}
