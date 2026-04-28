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
        var keywords = question.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var topChunks = _chunks
            .Select(chunk => (
                Text: chunk,
                Score: keywords.Count(kw => chunk.ToLower().Contains(kw.ToLower()))
            ))
            .OrderByDescending(x => x.Score)
            .Take(3)
            .Select(x => x.Text.Length > 2000 ? x.Text.Substring(0, 2000) : x.Text)
            .ToList();

        // Fallback om inga chunks matchade
        if (!topChunks.Any())
            topChunks = _chunks.Take(3).ToList();

        var context = string.Join("\n\n", topChunks);

        // Build prompt
        var prompt = $"""
            Du är en GDPR-expert. Svara på frågan baserat ENDAST på följande utdrag från GDPR-förordningen.
            Om svaret inte finns i texten, var då tydlig med det. Om frågan inte är GDPR-relaterad, försök leda tillbaka till GDPR.

            GDPR-utdrag:
            {context}

            Fråga: {question}

            Svar:
        """;

        var response = await _kernel.InvokePromptAsync(prompt);
        return response.ToString();
    }
}