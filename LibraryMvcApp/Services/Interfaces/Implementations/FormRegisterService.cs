//using LibraryMvcApp.Models;
//using LibraryMvcApp.Services.Interfaces;
//using Microsoft.EntityFrameworkCore;

//namespace LibraryMvcApp.Services.Implementations
//{
//    public class FormRegisterService : IFormRegisterService
//    {
//        private readonly AppDbContext _context;

//        public FormRegisterService(AppDbContext context)
//        {
//            _context = context;
//        }

//        public async Task AddFormAsync(FormEntry entry)
//        {
//            var department = await _context.Departments
//                .SingleOrDefaultAsync(d => d.Id == entry.DepartmentId);

//            if (department == null)
//                throw new Exception("Department not found");

//            entry.DepartmentNo = department.Code;

//            var lastFormNumber = await _context.FormEntries
//                .Where(x => x.DepartmentId == department.Id)
//                .Select(x => (int?)x.FormNumber)
//                .MaxAsync()
//                ?? department.StartFormNumber;

//            entry.FormNumber = lastFormNumber + 1;
//            entry.FullNumber = $"ن / {entry.DepartmentNo} / {entry.FormNumber}";
//            entry.CreatedAt = DateTime.Now;

//            _context.FormEntries.Add(entry);
//            await _context.SaveChangesAsync();
//        }

//        public async Task<List<FormEntry>> GetByDepartmentAsync(int departmentNo)
//        {
//            return await _context.FormEntries
//                .Where(x => x.DepartmentNo == departmentNo)
//                .OrderBy(x => x.FormNumber)
//                .ToListAsync();
//        }
//        public async Task<List<FormEntry>> GetAllAsync()
//        {
//            return await _context.FormEntries
//                .Include(x => x.Department)
//                .OrderByDescending(x => x.FormNumber) 
//                .ToListAsync();
//        }


//        public async Task<int> GetLastFormNumberAsync(int departmentNo)
//        {
//            var department = await _context.Departments
//                .FirstOrDefaultAsync(d => d.Code == departmentNo);

//            if (department == null)
//                throw new Exception("Department not found");

//            var lastFormNumber = await _context.FormEntries
//                .Where(f => f.DepartmentId == department.Id)
//                .Select(f => (int?)f.FormNumber)
//                .MaxAsync();

//            return lastFormNumber ?? department.StartFormNumber;
//        }

//        public async Task DeleteAsync(int id)
//        {
//            var entry = await _context.FormEntries.FindAsync(id);
//            if (entry == null)
//                throw new Exception("Form entry not found");

//            _context.FormEntries.Remove(entry);
//            await _context.SaveChangesAsync();
//        }
//    }
//}
using LibraryMvcApp.Models;
using LibraryMvcApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryMvcApp.Services.Implementations
{
    public class FormRegisterService : IFormRegisterService
    {
        private readonly AppDbContext _context;

        public FormRegisterService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddFormAsync(FormEntry entry)
        {
            var department = await _context.Departments
                .SingleOrDefaultAsync(d => d.Id == entry.DepartmentId);

            if (department == null)
                throw new Exception("Department not found");

            entry.DepartmentNo = department.Code;

            // لو المستخدم لم يدخل رقم → استخدم StartFormNumber أو آخر رقم + 1
            if (entry.FormNumber <= 0)
            {
                var lastFormNumber = await _context.FormEntries
                    .Where(x => x.DepartmentId == department.Id)
                    .Select(x => (int?)x.FormNumber)
                    .MaxAsync();

                entry.FormNumber = lastFormNumber != null
                    ? lastFormNumber.Value + 1
                    : department.StartFormNumber;
            }
            else
            {
                // تحقق من عدم التكرار
                var exists = await _context.FormEntries
                    .AnyAsync(x =>
                        x.DepartmentId == department.Id &&
                        x.FormNumber == entry.FormNumber);

                if (exists)
                    throw new Exception("رقم النموذج موجود بالفعل");
            }

            // توليد الرقم الكامل
            entry.FullNumber = $"ن / {entry.DepartmentNo} / {entry.FormNumber}";

            entry.CreatedAt = DateTime.Now;

            _context.FormEntries.Add(entry);

            await _context.SaveChangesAsync();
        }

        public async Task<List<FormEntry>> GetByDepartmentAsync(int departmentNo)
        {
            return await _context.FormEntries
                .Where(x => x.DepartmentNo == departmentNo)
                .OrderBy(x => x.FormNumber)
                .ToListAsync();
        }

        public async Task<List<FormEntry>> GetAllAsync()
        {
            return await _context.FormEntries
                .Include(x => x.Department)
                .OrderByDescending(x => x.FormNumber)
                .ToListAsync();
        }

        public async Task<int> GetLastFormNumberAsync(int departmentNo)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Code == departmentNo);

            if (department == null)
                throw new Exception("Department not found");

            var lastFormNumber = await _context.FormEntries
                .Where(f => f.DepartmentId == department.Id)
                .Select(f => (int?)f.FormNumber)
                .MaxAsync();

            return lastFormNumber ?? department.StartFormNumber;
        }

        public async Task DeleteAsync(int id)
        {
            var entry = await _context.FormEntries.FindAsync(id);

            if (entry == null)
                throw new Exception("Form entry not found");

            _context.FormEntries.Remove(entry);

            await _context.SaveChangesAsync();
        }
    }
}
