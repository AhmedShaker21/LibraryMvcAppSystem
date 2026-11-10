namespace LibraryMvcApp.Services.Interfaces
{
    public interface IPdfFormExtractor
    {
        Task<List<int>> ExtractSerialsAsync(string pdfPath, int departmentNo);
    }
}
