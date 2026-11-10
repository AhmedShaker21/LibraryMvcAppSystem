using LibraryMvcApp.Services.Interfaces;
using System.Text.RegularExpressions;
using Tesseract;
using UglyToad.PdfPig;

namespace LibraryMvcApp.Services.Implementations
{
    public class PdfFormExtractor : IPdfFormExtractor
    {
        public async Task<List<int>> ExtractSerialsAsync(string pdfPath, int departmentNo)
        {
            var result = new List<int>();

            try
            {
                string tessDataPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "tessdata"
                );

                using var engine = new TesseractEngine(
                    tessDataPath,
                    "ara",
                    EngineMode.Default
                );

                using var pdf = PdfDocument.Open(pdfPath);

                foreach (var page in pdf.GetPages())
                {
                    string text = page.Text;

                    // Regex: ن / 53 / 206
                    var matches = Regex.Matches(
                        text,
                        @$"ن\s*/\s*{departmentNo}\s*/\s*(\d+)"
                    );

                    foreach (Match m in matches)
                    {
                        if (int.TryParse(m.Groups[1].Value, out int serial))
                        {
                            result.Add(serial);
                        }
                    }
                }
            }
            catch
            {
                return new List<int>();
            }

            return result.Distinct().OrderBy(x => x).ToList();
        }
    }
}
