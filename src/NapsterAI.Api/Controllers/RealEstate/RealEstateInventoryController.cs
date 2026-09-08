using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;

namespace NapsterAI.Api.Controllers.RealEstate;

// GET actions are public (including for the EdgeMCP searchInventory tool); writes require
// AgentOrAdmin (see RealEstateProjectsController).
[ApiController]
[Route("api/realestate/inventory")]
public class RealEstateInventoryController : ControllerBase
{
    private readonly IRealEstateInventoryService _inventoryService;
    private readonly IRealEstatePaymentPlanService _paymentPlanService;
    private readonly ILogger<RealEstateInventoryController> _logger;

    public RealEstateInventoryController(
        IRealEstateInventoryService inventoryService, IRealEstatePaymentPlanService paymentPlanService, ILogger<RealEstateInventoryController> logger)
    {
        _inventoryService = inventoryService;
        _paymentPlanService = paymentPlanService;
        _logger = logger;
    }

    /// <remarks>
    /// GET /api/realestate/inventory?projectId=1&amp;minBedrooms=2&amp;maxPrice=2000000&amp;status=Available&amp;sortBy=price
    /// This is the search endpoint the EdgeMCP "searchInventory" tool (Phase 9) calls.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<InventoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResultDto<InventoryDto>>> List(
        [FromQuery] int? projectId,
        [FromQuery] int? unitTypeId,
        [FromQuery] int? minBedrooms,
        [FromQuery] int? maxBedrooms,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] decimal? minAreaSqFt,
        [FromQuery] decimal? maxAreaSqFt,
        [FromQuery] string? status,
        [FromQuery] string? viewType,
        [FromQuery] string? furnishingStatus,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _inventoryService.ListAsync(
            projectId, unitTypeId, minBedrooms, maxBedrooms, minPrice, maxPrice, minAreaSqFt, maxAreaSqFt,
            status, viewType, furnishingStatus, search, sortBy, sortDescending, pageIndex, pageSize, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _inventoryService.GetByIdAsync(id, cancellationToken));
    }

    /// <remarks>
    /// Anonymous, read-only computation - the EdgeMCP "calculatePaymentPlan" tool calls this
    /// with only anonymous-visitor privileges, same as any public-site visitor.
    /// </remarks>
    [HttpPost("{id:int}/payment-plan")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaymentPlanScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentPlanScheduleDto>> CalculatePaymentPlan(int id, [FromBody] CalculatePaymentPlanRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _paymentPlanService.CalculateAsync(id, request, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "AgentOrAdmin")]
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InventoryDto>> Create([FromBody] CreateInventoryRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating inventory unit {UnitNumber} for project {ProjectId}", request.UnitNumber, request.ProjectId);

        var inventory = await _inventoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = inventory.InventoryId }, inventory);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AgentOrAdmin")]
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InventoryDto>> Update(int id, [FromBody] UpdateInventoryRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _inventoryService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AgentOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _inventoryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
