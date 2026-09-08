using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Services;

namespace NapsterAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgeBasesController : ControllerBase
{
    private readonly INapsterService _napsterService;
    private readonly ILogger<KnowledgeBasesController> _logger;

    public KnowledgeBasesController(INapsterService napsterService, ILogger<KnowledgeBasesController> logger)
    {
        _napsterService = napsterService;
        _logger = logger;
    }

    /// <summary>
    /// Lists knowledge bases - containers of reference documents an agent can be pointed at.
    /// </summary>
    /// <remarks>GET /api/knowledgebases?search=faq&amp;provider=azureOpenAI&amp;pageIndex=0&amp;pageSize=20</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<KnowledgeBaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<PagedResultDto<KnowledgeBaseDto>>> List(
        [FromQuery] string? provider,
        [FromQuery] string? search,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Received request to list knowledge bases (search={Search})", search);

        var result = await _napsterService.ListKnowledgeBasesAsync(provider, search, pageIndex, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new (initially empty) knowledge base.
    /// </summary>
    /// <remarks>POST /api/knowledgebases { "name": "...", "provider": "azureOpenAI" }</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(KnowledgeBaseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<KnowledgeBaseDto>> Create([FromBody] CreateKnowledgeBaseRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received request to create knowledge base {Name}", request.Name);

        var knowledgeBase = await _napsterService.CreateKnowledgeBaseAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = knowledgeBase.Id }, knowledgeBase);
    }
}
