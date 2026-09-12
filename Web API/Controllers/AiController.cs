using System.Security.Claims;
using Application.Features.Ai.Ask;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController(IMediator mediator) : ControllerBase
{
    private const int MaxQuestionLength = 1000;
    private readonly IMediator _mediator = mediator;

    [HttpGet("Ask")]
    [Authorize(Roles = "Company Admin")]
    public async Task<IActionResult> Ask([FromQuery] string question, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return BadRequest("question is required.");
        }

        if (question.Length > MaxQuestionLength)
        {
            return BadRequest($"question must be {MaxQuestionLength} characters or fewer.");
        }

        // Tenant scope comes from the token only — never from the request.
        if (!long.TryParse(User.FindFirstValue("CompanyId"), out var companyId))
        {
            return Forbid();
        }

        var result = await _mediator.Send(new AskHrAssistantQuery(question.Trim(), companyId), ct);
        return Ok(result);
    }
}
