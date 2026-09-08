using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;

namespace NapsterAI.Api.Controllers.RealEstate;

// Real estate sales agents - distinct from Napster's own /api/agents (AI conversation
// agent configs). GET actions are public (agent directory / "meet the team" pages); writes
// are AdminOnly, same rationale as RealEstateOrganizationsController.
[ApiController]
[Route("api/realestate/agents")]
public class RealEstateAgentsController : ControllerBase
{
    private readonly IRealEstateAgentService _agentService;

    public RealEstateAgentsController(IRealEstateAgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RealEstateAgentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RealEstateAgentDto>>> List([FromQuery] int? organizationId, CancellationToken cancellationToken)
    {
        return Ok(await _agentService.ListAsync(organizationId, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RealEstateAgentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RealEstateAgentDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _agentService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(RealEstateAgentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RealEstateAgentDto>> Create([FromBody] CreateRealEstateAgentRequestDto request, CancellationToken cancellationToken)
    {
        var agent = await _agentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = agent.SalesAgentId }, agent);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(RealEstateAgentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RealEstateAgentDto>> Update(int id, [FromBody] UpdateRealEstateAgentRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _agentService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _agentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
