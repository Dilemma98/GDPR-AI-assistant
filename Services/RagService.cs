// using Microsoft.SemanticKernel;
// using Microsoft.SemanticKernel.Embeddings;

// namespace GDPR_AI_assistant.Services;

// public class RagService
// {
//     private readonly Kernel _kernel;
//     private readonly PdfIngestionService _pdfIngestion;

//     private readonly List<(string Text, float[] Embedding)> _index = new();
//     private bool _isIndexed = false;

//     public RagService(Kernel kernel, PdfIngestionService pdfIngestion)
//     {
//         _kernel = kernel;
//         _pdfIngestion = pdfIngestion;
//     }

//     public async Task IndexDocumentsAsync()
//     {
//         if (_isIndexed) return;

//         var cachePath = "embeddings-cache.json";

//         // Om cache finns, ladda från disk istället för att anropa API:et
//         if (File.Exists(cachePath))
//         {
//             var json = await File.ReadAllTextAsync(cachePath);
//             var cached = System.Text.Json.JsonSerializer.Deserialize<List<CachedChunk>>(json)!;
//             foreach (var item in cached)
//                 _index.Add((item.Text, item.Embedding));
//             _isIndexed = true;
//             return;
//         }

//         var chunks = _pdfIngestion.ExtractChunks();
// #pragma warning disable SKEXP0001
//         var embeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();

//         foreach (var chunk in chunks)
//         {
//             var embedding = await embeddingService.GenerateEmbeddingAsync(chunk);
//             _index.Add((chunk, embedding.ToArray()));
//             await Task.Delay(2000);
//         }
// #pragma warning restore SKEXP0001

//         // Spara till disk så vi slipper göra om detta
//         var toCache = _index.Select(x => new CachedChunk { Text = x.Text, Embedding = x.Embedding }).ToList();
//         await File.WriteAllTextAsync(cachePath, System.Text.Json.JsonSerializer.Serialize(toCache));

//         _isIndexed = true;
//     }

//     private record CachedChunk
//     {
//         public string Text { get; init; } = "";
//         public float[] Embedding { get; init; } = [];
//     }

//     public async Task<string> AskAsync(string question)
//     {
//         await IndexDocumentsAsync();

//         var embeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();

//         // Convert question to vector so comparison to document can be made
//         var questionEmbedding = await embeddingService.GenerateEmbeddingAsync(question);

//         // Find 3 most relevant chunks by calculate cosine similarity
//         // Higher score = higher relevance
//         var topChunks = _index
//             .Select(item => (item.Text, Score: CosineSimilarity(questionEmbedding.ToArray(), item.Embedding)))
//             .OrderByDescending(x => x.Score)
//             .Take(3)
//             .Select(x => x.Text)
//             .ToList();

//         var context = string.Join("\n\n", topChunks);

//         // Build prompt, attaches context so that AI can answer based on GDPR text
//         var prompt = $"""
//             Du är en GDPR-expert. Svara på frågan baserat ENDAST på följande utdrag från GDPR-förordningen.
//             Om svaret inte finns i texten, var då tydlig med det.

//             GDPR-utdrag:
//             {context}

//             Fråga: {question}

//             Svar:
//         """;

//         // Send prompt to Gemini via Semantic Kernel and return answer
//         var response = await _kernel.InvokePromptAsync(prompt);
//         return response.ToString();
//     }

//     private static float CosineSimilarity(float[] a, float[] b)
//     {
//         var dot = a.Zip(b, (x, y) => x * y).Sum();
//         var magA = MathF.Sqrt(a.Sum(x => x * x));
//         var magB = MathF.Sqrt(b.Sum(x => x * x));
//         return dot / (magA * magB);
//     }
// }

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
                Score: keywords.Count(kw => chunk.ToLower().Contains(kw))
            ))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(3)
            .Select(x => x.Text)
            .ToList();

        // Fallback om inga chunks matchade
        if (!topChunks.Any())
            topChunks = _chunks.Take(3).ToList();

        var context = string.Join("\n\n", topChunks);

        // Build prompt
        var prompt = $"""
            Du är en GDPR-expert. Svara på frågan baserat ENDAST på följande utdrag från GDPR-förordningen.
            Om svaret inte finns i texten, var då tydlig med det.

            GDPR-utdrag:
            {context}

            Fråga: {question}

            Svar:
        """;

        var response = await _kernel.InvokePromptAsync(prompt);
        return response.ToString();
    }
}