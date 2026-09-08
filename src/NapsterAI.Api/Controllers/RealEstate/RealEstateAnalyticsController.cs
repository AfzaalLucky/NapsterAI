using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;

namespace NapsterAI.Api.Controllers.RealEstate;

/// <summary>Admin dashboard KPI aggregates. AgentOrAdmin - agents can see aggregate stats too, not just admins.</summary>
[ApiController]
[Route("api/realestate/analytics")]
[Authorize(Policy = "AgentOrAdmin")]
public class RealEstateAnalyticsController : ControllerBase
{
    private readonly IRealEstateAnalyticsService _analyticsService;

    public RealEstateAnalyticsController(IRealEstateAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("leads-summary")]
    [ProducesResponseType(typeof(LeadsSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadsSummaryDto>> LeadsSummary(CancellationToken cancellationToken)
    {
        return Ok(await _analyticsService.GetLeadsSummaryAsync(cancellationToken));
    }

    [HttpGet("inventory-summary")]
    [ProducesResponseType(typeof(InventorySummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventorySummaryDto>> InventorySummary(CancellationToken cancellationToken)
    {
        return Ok(await _analyticsService.GetInventorySummaryAsync(cancellationToken));
    }
}
