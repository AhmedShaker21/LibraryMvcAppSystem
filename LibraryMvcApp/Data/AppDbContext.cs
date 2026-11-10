using LibraryMvcApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<Folder> Folders { get; set; }
    public DbSet<FormEntry> FormEntries { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<UserDepartment> UserDepartments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Performance: the forms lists filter / sort on these columns
        modelBuilder.Entity<FormEntry>()
            .HasIndex(f => new { f.DepartmentNo, f.FormNumber });

        modelBuilder.Entity<Book>()
            .HasIndex(b => b.IsInstruction);

        modelBuilder.Entity<UserDepartment>()
    .HasIndex(x => x.UserId)
    .IsUnique();

        // =========================
        // Departments
        // =========================
        modelBuilder.Entity<Department>()
            .HasIndex(d => d.Code)
            .IsUnique();

        //modelBuilder.Entity<Department>().HasData(
        //    new Department { Id = 1, Code = 53, Name = "إدارة الجودة", StartFormNumber = 200 },
        //    new Department { Id = 2, Code = 50, Name = "إدارة السلامة", StartFormNumber = 200 },
        //    new Department { Id = 3, Code = 73, Name = "تأهيل وتدريب العاملين", StartFormNumber = 200 },
        //    new Department { Id = 4, Code = 74, Name = "الإدارة الطبية", StartFormNumber = 200 },
        //    new Department { Id = 5, Code = 81, Name = "إدارة المشتريات", StartFormNumber = 200 }
        //);

        //modelBuilder.Entity<FormEntry>().HasData(

      
        //    new FormEntry { Id = 1, DepartmentId = 1, DepartmentNo = 53, ProcedureName = "تحديد وتقييم مظاهر التأثير البيئي والسلامة", ProcedureCode = "ACFE/HS P 53-01", FormName = "خريطة تحليل الأنشطة والعمليات", FormNumber = 230, FullNumber = "ن / 53 / 230", CreatedAt = new DateTime(2026, 1, 1) },
        //    new FormEntry { Id = 2, DepartmentId = 1, DepartmentNo = 53, ProcedureName = "تحديد وتقييم مظاهر التأثير البيئي والسلامة", ProcedureCode = "ACFE/HS P 53-01", FormName = "جدول الحصر العام لمصادر التأثير البيئى", FormNumber = 231, FullNumber = "ن / 53 / 231", CreatedAt = new DateTime(2026, 1, 1) },
        //    new FormEntry { Id = 3, DepartmentId = 1, DepartmentNo = 53, ProcedureName = "تحديد وتقييم مظاهر التأثير البيئي والسلامة", ProcedureCode = "ACFE/HS P 53-01", FormName = "جدول تقييم العناصر البيئية", FormNumber = 232, FullNumber = "ن / 53 / 232", CreatedAt = new DateTime(2026, 1, 1) },
        //    new FormEntry { Id = 4, DepartmentId = 1, DepartmentNo = 53, ProcedureName = "تحديد وتقييم مظاهر التأثير البيئي والسلامة", ProcedureCode = "ACFE/HS P 53-01", FormName = "جدول الحصر العام للمصادر الهامة", FormNumber = 233, FullNumber = "ن / 53 / 233", CreatedAt = new DateTime(2026, 1, 1) },
        //    new FormEntry { Id = 5, DepartmentId = 1, DepartmentNo = 53, ProcedureName = "أسلوب مواجهة حالات الطوارئ", ProcedureCode = "ACFE/HSP-53-06", FormName = "تحديد مخاطر العمل بالموقع", FormNumber = 240, FullNumber = "ن / 53 / 240", CreatedAt = new DateTime(2026, 1, 1) },

        //    new FormEntry { Id = 20, DepartmentId = 3, DepartmentNo = 73, ProcedureName = "تأهيل وتدريب العاملين بالعمليات الخاصة", ProcedureCode = "ACFQ/E/HS P 73-02", FormName = "حصر أسماء العاملين فى العمليات الخاصة", FormNumber = 239, FullNumber = "ن / 73 / 239", CreatedAt = new DateTime(2026, 1, 1) },

        //    new FormEntry { Id = 30, DepartmentId = 4, DepartmentNo = 74, ProcedureName = "الإدارة الطبية", ProcedureCode = "ACFQ/E/HSP 74-01", FormName = "طلب توقيع كشف طبى", FormNumber = 200, FullNumber = "ن / 74 / 200", CreatedAt = new DateTime(2026, 1, 1) },
        //    new FormEntry { Id = 31, DepartmentId = 4, DepartmentNo = 74, ProcedureName = "الإدارة الطبية", ProcedureCode = "ACFQ/E/HSP 74-01", FormName = "نموذج تحويل للمستشفى", FormNumber = 201, FullNumber = "ن / 74 / 201", CreatedAt = new DateTime(2026, 1, 1) },

        //    new FormEntry { Id = 40, DepartmentId = 5, DepartmentNo = 81, ProcedureName = "إجراء عمليات الشراء", ProcedureCode = "ACFQP 81-02", FormName = "طلب الشراء", FormNumber = 254, FullNumber = "ن / 81 / 254", CreatedAt = new DateTime(2026, 1, 1) },
        //    new FormEntry { Id = 41, DepartmentId = 5, DepartmentNo = 81, ProcedureName = "تقييم الموردين", ProcedureCode = "ACFQP 81-03", FormName = "سجل الموردين المعتمدين", FormNumber = 242, FullNumber = "ن / 81 / 242", CreatedAt = new DateTime(2026, 1, 1) }
        //);
    }
}
