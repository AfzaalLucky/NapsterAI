using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;

namespace NapsterAI.Api.Controllers.RealEstate;

/// <summary>
/// Scheduled property viewings. Reads require AgentOrAdmin, but Create is anonymous - it's
/// the public "Book a viewing" form and the EdgeMCP "bookViewing" tool's entrypoint - and
/// rate-limited (see Program.cs) since it's an unauthenticated write.
/// </summary>
[ApiController]
[Route("api/realestate/viewings")]
[Authorize(Policy = "AgentOrAdmin")]
public class RealEstateViewingsController : ControllerBase
{
    private readonly IRealEstateViewingService _viewingService;
    private readonly ILogger<RealEstateViewingsController> _logger;

    public RealEstateViewingsController(IRealEstateViewingService viewingService, ILogger<RealEstateViewingsController> logger)
    {
        _viewingService = viewingService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ViewingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<ViewingDto>>> List(
        [FromQuery] int? leadId, [FromQuery] int? inventoryId, [FromQuery] string? status,
        [FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _viewingService.ListAsync(leadId, inventoryId, status, pageIndex, pageSize, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ViewingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ViewingDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _viewingService.GetByIdAsync(id, cancellationToken));
    }

    /// <remarks>Anonymous - public "Book a viewing" form / EdgeMCP "bookViewing" tool. Transactionally upserts Customer + Lead + Viewing.</remarks>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous-writes")]
    [ProducesResponseType(typeof(ViewingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ViewingDto>> Create([FromBody] CreateViewingRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating viewing request for inventory {InventoryId} from {Email}", request.InventoryId, request.CustomerEmail);

        var viewing = await _viewingService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = viewing.ViewingId }, viewing);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ViewingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ViewingDto>> UpdateStatus(int id, [FromBody] UpdateViewingStatusRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _viewingService.UpdateStatusAsync(id, request, cancellationToken));
    }
}
