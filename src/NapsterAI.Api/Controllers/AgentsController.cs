using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Services;

namespace NapsterAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly INapsterService _napsterService;
    private readonly ILogger<AgentsController> _logger;

    public AgentsController(INapsterService napsterService, ILogger<AgentsController> logger)
    {
        _napsterService = napsterService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a Napster agent - a reusable, named configuration binding a
    /// companion persona to voice/provider settings, tools, and knowledge.
    /// </summary>
    /// <remarks>POST /api/agents { "companionId": "...", "name": "...", "providerSettings": { ... } }</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<AgentDto>> Create([FromBody] CreateAgentRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received request to create agent {Name}", request.Name);

        var agent = await _napsterService.CreateAgentAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = agent.Id }, agent);
    }
}
