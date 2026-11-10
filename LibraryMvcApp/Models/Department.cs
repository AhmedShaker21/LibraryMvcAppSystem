namespace LibraryMvcApp.Models
{
    public class Department
    {
        public int Id { get; set; }

        // رقم الإدارة (53 – 50 …)
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public int StartFormNumber { get; set; }

        public ICollection<FormEntry> FormEntries { get; set; }
            = new List<FormEntry>();
    }

}
