using LibraryMvcApp.Models;
using System.ComponentModel.DataAnnotations;

namespace LibraryMvcApp.ViewModels
{
    public class CreateFormVm
    {

        public int? DepartmentId { get; set; }
        public int DepartmentCode { get; set; }

        [Required]
        public int DepartmentNo { get; set; }  
        
        [Required]
        public string ProcedureName { get; set; }

        [Required]
        public string ProcedureCode { get; set; }

        [Required]
        public string FormName { get; set; }

        public List<Department>? Departments { get; set; }
        public int FormNumber { get; set; }

        public int LastFormNumber { get; set; }
    }


}
