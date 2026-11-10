using LibraryMvcApp.Models;

namespace LibraryMvcApp.ViewModels
{
    public class SearchViewModel
    {
        public List<Folder> Folders { get; set; } = new();
        public List<Book> Books { get; set; } = new();
    }
}
