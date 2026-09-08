using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;

namespace NapsterAI.Api.Controllers.RealEstate;

// GET actions are deliberately left without [Authorize] - this is the public catalog.
// Writes require AgentOrAdmin; Approve/Reject (admin moderation of agent-submitted
// listings) require AdminOnly.
[ApiController]
[Route("api/realestate/projects")]
public class RealEstateProjectsController : ControllerBase
{
    private readonly IRealEstateProjectService _projectService;
    private readonly IRealEstatePaymentPlanService _paymentPlanService;
    private readonly ILogger<RealEstateProjectsController> _logger;

    public RealEstateProjectsController(
        IRealEstateProjectService projectService, IRealEstatePaymentPlanService paymentPlanService, ILogger<RealEstateProjectsController> logger)
    {
        _projectService = projectService;
        _paymentPlanService = paymentPlanService;
        _logger = logger;
    }

    /// <remarks>GET /api/realestate/projects?city=Dubai&amp;projectType=Residential&amp;isFeatured=true&amp;pageIndex=0&amp;pageSize=20</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResultDto<ProjectDto>>> List(
        [FromQuery] string? city,
        [FromQuery] string? projectType,
        [FromQuery] string? status,
        [FromQuery] bool? isFeatured,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? search,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _projectService.ListAsync(city, projectType, status, isFeatured, minPrice, maxPrice, search, pageIndex, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, cancellationToken);
        return Ok(project);
    }

    [HttpGet("{id:int}/unit-types")]
    [ProducesResponseType(typeof(IReadOnlyList<UnitTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<UnitTypeDto>>> GetUnitTypes(int id, CancellationToken cancellationToken)
    {
        return Ok(await _projectService.GetUnitTypesAsync(id, cancellationToken));
    }

    [HttpGet("{id:int}/amenities")]
    [ProducesResponseType(typeof(IReadOnlyList<AmenityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AmenityDto>>> GetAmenities(int id, CancellationToken cancellationToken)
    {
        return Ok(await _projectService.GetAmenitiesAsync(id, cancellationToken));
    }

    [HttpGet("{id:int}/media")]
    [ProducesResponseType(typeof(IReadOnlyList<MediaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<MediaDto>>> GetMedia(int id, CancellationToken cancellationToken)
    {
        return Ok(await _projectService.GetMediaAsync(id, cancellationToken));
    }

    [HttpGet("{id:int}/payment-plan-milestones")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentPlanMilestoneDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PaymentPlanMilestoneDto>>> GetPaymentPlanMilestones(int id, CancellationToken cancellationToken)
    {
        return Ok(await _paymentPlanService.ListMilestonesAsync(id, cancellationToken));
    }

    [HttpPost("{id:int}/payment-plan-milestones")]
    [Authorize(Policy = "AgentOrAdmin")]
    [ProducesResponseType(typeof(PaymentPlanMilestoneDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentPlanMilestoneDto>> AddPaymentPlanMilestone(int id, [FromBody] CreatePaymentPlanMilestoneRequestDto request, CancellationToken cancellationToken)
    {
        var milestone = await _paymentPlanService.AddMilestoneAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetPaymentPlanMilestones), new { id }, milestone);
    }

    [HttpPost]
    [Authorize(Policy = "AgentOrAdmin")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Real Estate project {ProjectCode}", request.ProjectCode);

        var project = await _projectService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = project.ProjectId }, project);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AgentOrAdmin")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjectDto>> Update(int id, [FromBody] UpdateProjectRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _projectService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AgentOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _projectService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> Approve(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving Real Estate project {ProjectId}", id);
        return Ok(await _projectService.ApproveAsync(id, cancellationToken));
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> Reject(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rejecting Real Estate project {ProjectId}", id);
        return Ok(await _projectService.RejectAsync(id, cancellationToken));
    }
}
