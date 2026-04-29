using Microsoft.SemanticKernel;

namespace GDPR_AI_assistant.Services;

public class RagService
{
    private readonly Kernel _kernel;
    private readonly PdfIngestionService _pdfIngestion;
    private List<string> _chunks = new();
    private bool _isIndexed = false;

    public RagService(Kernel kernel, PdfIngestionService pdfIngestion)
    {
        _kernel = kernel;
        _pdfIngestion = pdfIngestion;
    }

    public Task IndexDocumentsAsync()
    {
        if (_isIndexed) return Task.CompletedTask;
        _chunks = _pdfIngestion.ExtractChunks();
        _isIndexed = true;
        return Task.CompletedTask;
    }

    public async Task<string> AskAsync(string question)
    {
        await IndexDocumentsAsync();

        // Hitta relevanta chunks via keyword-matchning
        var stopWords = new[] { "vad", "är", "och", "det", "som", "en", "i", "på", "för" };

        var keywords = question
            .ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !stopWords.Contains(w))
            .ToList();

        var topChunks = _chunks
    .Select(chunk =>
    {
        var score = keywords.Count(kw => chunk.ToLower().Contains(kw))
            + (chunk.Length < 1500 ? 2 : 0);
        if (chunk.ToLower().Contains("artikel"))
            score += 2;

        return new { Text = chunk, Score = score };
    })
    .Where(x => x.Score > 0)
    .OrderByDescending(x => x.Score)
    .Take(3)
    .ToList();



        // Fallback om inga chunks matchade
        if (!topChunks.Any())
        {
            return "Jag hittar inte detta i GDPR-texten jag har.";
        }

        var context = string.Join("\n\n", topChunks
    .Select(x => x.Text.Length > 1200
        ? x.Text.Substring(0, 1200)
        : x.Text));

        // Build prompt
        var prompt = $"""
            Du är en GDPR-expert. Svara på frågan baserat ENDAST på följande utdrag från GDPR-förordningen.
           Om svaret inte finns i texten, säg: 'Jag hittar inte detta i den tillgängliga GDPR-texten.' Gissa aldrig.
            
            Om frågan inte är GDPR-relaterad, försök leda tillbaka till GDPR.

            GDPR-utdrag:
            {context}

            Fråga: {question}

            Svar:
        """;

        var response = await _kernel.InvokePromptAsync(prompt);
        return response.ToString();
    }
}