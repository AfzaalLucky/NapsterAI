using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;

namespace NapsterAI.Api.Controllers.RealEstate;

[ApiController]
[Route("api/realestate/lookups")]
public class RealEstateLookupsController : ControllerBase
{
    private readonly IRealEstateLookupService _lookupService;

    public RealEstateLookupsController(IRealEstateLookupService lookupService)
    {
        _lookupService = lookupService;
    }

    /// <remarks>
    /// GET /api/realestate/lookups?type=ProjectType - powers admin UI dropdowns.
    /// Valid types: ProjectType, ProjectStatus, InventoryStatus, UnitCategory, FurnishingStatus, AmenityCategory, ViewType.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LookupDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupDto>>> List([FromQuery] string? type, CancellationToken cancellationToken)
    {
        return Ok(await _lookupService.ListAsync(type, cancellationToken));
    }
}
