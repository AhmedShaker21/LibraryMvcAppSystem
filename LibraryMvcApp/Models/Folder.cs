namespace LibraryMvcApp.Models
{
    public class Folder
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int? ParentFolderId { get; set; }
        public Folder? ParentFolder { get; set; }
        public string? AllowedRole { get; set; }
        public ICollection<Folder> SubFolders { get; set; } = new List<Folder>();
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }


}
