using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace GDPR_AI_assistant.Services;

public class PdfIngestionService
{
    private readonly string _documentsPath;

    // Dependency Injection
    public PdfIngestionService(IWebHostEnvironment env)
    {
        _documentsPath = Path.Combine(env.ContentRootPath, "Documents");
    }

    public List<string> ExtractChunks(int chunkSize = 1000)
    {
        var chunks = new List<string>();

        foreach (var pdfPath in Directory.GetFiles(_documentsPath, "*.pdf"))
        {
            using var document = PdfDocument.Open(pdfPath);

            var fullText = string.Join(" ", document.GetPages()
                .Select(p => p.Text));

            fullText = System.Text.RegularExpressions.Regex.Replace(fullText, @"\s+", " ");

            // Split per article
            var articleSplits = System.Text.RegularExpressions.Regex
               .Split(fullText, @"(?=Artikel\s*\d+)")
               .Where(s => !string.IsNullOrWhiteSpace(s) && s.Length > 200)
               .ToList();

            // If split worked, use it
            chunks.AddRange(articleSplits);

            foreach (var chunk in articleSplits.Take(5))
            {
                Console.WriteLine("---- CHUNK ----");
                Console.WriteLine(chunk.Substring(0, Math.Min(200, chunk.Length)));
            }
        }
        return chunks;
    }
}