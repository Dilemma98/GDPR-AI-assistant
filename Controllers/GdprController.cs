using Microsoft.AspNetCore.Mvc;
using GDPR_AI_assistant.Services;

namespace GDPR_AI_assistant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GdprController : ControllerBase
{
    private readonly RagService _ragService;

    public GdprController(RagService ragService)
    {
        _ragService = ragService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] string question)
    {
        if (string.IsNullOrWhiteSpace(question))
            return BadRequest("Frågan får inte vara tom.");

        var answer = await _ragService.AskAsync(question);

        return Ok(new { question, answer });
    }
}
