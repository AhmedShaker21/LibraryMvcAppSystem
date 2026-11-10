using System.ComponentModel.DataAnnotations;

namespace LibraryMvcApp.ViewModels
{
    public class CreateUserVM
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        [Required]
        public string Role { get; set; } = "";

        [Required]
        public int DepartmentId { get; set; }
    
}
}
