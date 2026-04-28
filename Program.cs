using Microsoft.SemanticKernel;
using GDPR_AI_assistant.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var groqApiKey = builder.Configuration["Groq:ApiKey"]
    ?? throw new InvalidOperationException("Groq API key saknas!");

builder.Services.AddSingleton(sp =>
{
    var kernel = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion(
            modelId: "llama-3.3-70b-versatile",
            apiKey: groqApiKey,
            endpoint: new Uri("https://api.groq.com/openai/v1"))
        .Build();
    return kernel;
});

builder.Services.AddSingleton<PdfIngestionService>();
builder.Services.AddSingleton<RagService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();