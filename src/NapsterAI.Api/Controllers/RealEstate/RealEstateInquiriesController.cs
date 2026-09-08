using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;

namespace NapsterAI.Api.Controllers.RealEstate;

/// <summary>
/// Raw first-touch contact. Reads require AgentOrAdmin (CRM data), but Create is anonymous -
/// it's the public "Request info" form and the EdgeMCP "createLead" tool's entrypoint - and
/// rate-limited (see Program.cs) since it's an unauthenticated write.
/// </summary>
[ApiController]
[Route("api/realestate/inquiries")]
[Authorize(Policy = "AgentOrAdmin")]
public class RealEstateInquiriesController : ControllerBase
{
    private readonly IRealEstateInquiryService _inquiryService;
    private readonly ILogger<RealEstateInquiriesController> _logger;

    public RealEstateInquiriesController(IRealEstateInquiryService inquiryService, ILogger<RealEstateInquiriesController> logger)
    {
        _inquiryService = inquiryService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<InquiryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<InquiryDto>>> List(
        [FromQuery] int? projectId, [FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _inquiryService.ListAsync(projectId, pageIndex, pageSize, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InquiryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InquiryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _inquiryService.GetByIdAsync(id, cancellationToken));
    }

    /// <remarks>Anonymous - public contact-us form / EdgeMCP "createLead" tool.</remarks>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous-writes")]
    [ProducesResponseType(typeof(InquiryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InquiryDto>> Create([FromBody] CreateInquiryRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating inquiry from {Channel} for {Email}", request.Channel, request.CustomerEmail);

        var inquiry = await _inquiryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = inquiry.InquiryId }, inquiry);
    }

    [HttpPost("{id:int}/convert-to-lead")]
    [ProducesResponseType(typeof(LeadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LeadDto>> ConvertToLead(int id, CancellationToken cancellationToken)
    {
        return Ok(await _inquiryService.ConvertToLeadAsync(id, cancellationToken));
    }
}
