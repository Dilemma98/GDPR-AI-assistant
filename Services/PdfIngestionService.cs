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

        foreach( var pdfPath in Directory.GetFiles(_documentsPath, "*.pdf"))
        {
            using var document = PdfDocument.Open(pdfPath);

            var fullText = string.Join(" ", document.GetPages()
                .Select(p => p.Text));

            // Split text to smaller parts
            for (int i = 0; i < fullText.Length; i += chunkSize)
            {
                var chunk = fullText.Substring(i, Math.Min(chunkSize, fullText.Length - i));
                if(!string.IsNullOrWhiteSpace(chunk))
                    chunks.Add(chunk);
            }
        }
        return chunks;
    }
}