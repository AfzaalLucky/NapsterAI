using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;

namespace NapsterAI.Api.Controllers.RealEstate;

/// <summary>
/// CRM leads - unlike the catalog controllers (Projects/Inventory/...), there is no
/// legitimate anonymous read here (customer PII), so the whole controller requires
/// AgentOrAdmin rather than opening up individual GET actions.
/// </summary>
[ApiController]
[Route("api/realestate/leads")]
[Authorize(Policy = "AgentOrAdmin")]
public class RealEstateLeadsController : ControllerBase
{
    private readonly IRealEstateLeadService _leadService;
    private readonly ILogger<RealEstateLeadsController> _logger;

    public RealEstateLeadsController(IRealEstateLeadService leadService, ILogger<RealEstateLeadsController> logger)
    {
        _leadService = leadService;
        _logger = logger;
    }

    /// <remarks>GET /api/realestate/leads?status=New&amp;salesAgentId=1&amp;pageIndex=0&amp;pageSize=20</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<LeadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<LeadDto>>> List(
        [FromQuery] string? status,
        [FromQuery] int? salesAgentId,
        [FromQuery] int? projectId,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _leadService.ListAsync(status, salesAgentId, projectId, pageIndex, pageSize, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LeadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _leadService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(LeadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeadDto>> Create([FromBody] CreateLeadRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating lead for {Email}", request.CustomerEmail);

        var lead = await _leadService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = lead.LeadId }, lead);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(LeadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadDto>> UpdateStatus(int id, [FromBody] UpdateLeadStatusRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _leadService.UpdateStatusAsync(id, request, GetActingUserId(), cancellationToken));
    }

    [HttpPost("{id:int}/assign")]
    [ProducesResponseType(typeof(LeadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadDto>> Assign(int id, [FromBody] AssignLeadRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _leadService.AssignAsync(id, request, GetActingUserId(), cancellationToken));
    }

    [HttpGet("{id:int}/activities")]
    [ProducesResponseType(typeof(IReadOnlyList<LeadActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<LeadActivityDto>>> ListActivities(int id, CancellationToken cancellationToken)
    {
        return Ok(await _leadService.ListActivitiesAsync(id, cancellationToken));
    }

    [HttpPost("{id:int}/activities")]
    [ProducesResponseType(typeof(LeadActivityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadActivityDto>> AddActivity(int id, [FromBody] CreateLeadActivityRequestDto request, CancellationToken cancellationToken)
    {
        var activity = await _leadService.AddActivityAsync(id, request, GetActingUserId(), cancellationToken);
        return CreatedAtAction(nameof(ListActivities), new { id }, activity);
    }

    /// <summary>
    /// The "sub" claim (JWT UserId) is remapped to ClaimTypes.NameIdentifier by
    /// JwtSecurityTokenHandler's default inbound claim mapping - see AuthService for where it's issued.
    /// </summary>
    private int? GetActingUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
