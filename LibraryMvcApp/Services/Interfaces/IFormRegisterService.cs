using LibraryMvcApp.Models;

namespace LibraryMvcApp.Services.Interfaces
{
    public interface IFormRegisterService
    {
        Task AddFormAsync(FormEntry entry);
        Task<int> GetLastFormNumberAsync(int departmentNo);
        Task<List<FormEntry>> GetByDepartmentAsync(int departmentNo);
        Task<List<FormEntry>> GetAllAsync();
        Task DeleteAsync(int id);
    }
}
