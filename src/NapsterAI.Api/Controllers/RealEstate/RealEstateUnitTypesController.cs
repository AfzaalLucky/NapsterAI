using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;

namespace NapsterAI.Api.Controllers.RealEstate;

// GET actions are public; writes require AgentOrAdmin (see RealEstateProjectsController).
[ApiController]
[Route("api/realestate/unit-types")]
public class RealEstateUnitTypesController : ControllerBase
{
    private readonly IRealEstateUnitTypeService _unitTypeService;

    public RealEstateUnitTypesController(IRealEstateUnitTypeService unitTypeService)
    {
        _unitTypeService = unitTypeService;
    }

    /// <remarks>GET /api/realestate/unit-types?projectId=1</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UnitTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UnitTypeDto>>> List([FromQuery] int? projectId, CancellationToken cancellationToken)
    {
        return Ok(await _unitTypeService.ListAsync(projectId, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UnitTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UnitTypeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _unitTypeService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "AgentOrAdmin")]
    [ProducesResponseType(typeof(UnitTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UnitTypeDto>> Create([FromBody] CreateUnitTypeRequestDto request, CancellationToken cancellationToken)
    {
        var unitType = await _unitTypeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = unitType.UnitTypeId }, unitType);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AgentOrAdmin")]
    [ProducesResponseType(typeof(UnitTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UnitTypeDto>> Update(int id, [FromBody] UpdateUnitTypeRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _unitTypeService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AgentOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _unitTypeService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
