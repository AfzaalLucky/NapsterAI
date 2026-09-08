using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;

namespace NapsterAI.Api.Controllers.RealEstate;

// GET actions are public; writes require AgentOrAdmin (see RealEstateProjectsController).
[ApiController]
[Route("api/realestate/amenities")]
public class RealEstateAmenitiesController : ControllerBase
{
    private readonly IRealEstateAmenityService _amenityService;

    public RealEstateAmenitiesController(IRealEstateAmenityService amenityService)
    {
        _amenityService = amenityService;
    }

    /// <remarks>GET /api/realestate/amenities?projectId=1&amp;category=Recreational</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AmenityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AmenityDto>>> List([FromQuery] int? projectId, [FromQuery] string? category, CancellationToken cancellationToken)
    {
        return Ok(await _amenityService.ListAsync(projectId, category, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AmenityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AmenityDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _amenityService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "AgentOrAdmin")]
    [ProducesResponseType(typeof(AmenityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AmenityDto>> Create([FromBody] CreateAmenityRequestDto request, CancellationToken cancellationToken)
    {
        var amenity = await _amenityService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = amenity.AmenityId }, amenity);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AgentOrAdmin")]
    [ProducesResponseType(typeof(AmenityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AmenityDto>> Update(int id, [FromBody] UpdateAmenityRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _amenityService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AgentOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _amenityService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
