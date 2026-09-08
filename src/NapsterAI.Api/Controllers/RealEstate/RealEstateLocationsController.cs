using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;

namespace NapsterAI.Api.Controllers.RealEstate;

[ApiController]
[Route("api/realestate/locations")]
public class RealEstateLocationsController : ControllerBase
{
    private readonly IRealEstateLocationService _locationService;

    public RealEstateLocationsController(IRealEstateLocationService locationService)
    {
        _locationService = locationService;
    }

    /// <remarks>GET /api/realestate/locations?country=UAE&amp;city=Dubai - powers search filter dropdowns/autocomplete.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LocationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LocationDto>>> List([FromQuery] string? country, [FromQuery] string? city, CancellationToken cancellationToken)
    {
        return Ok(await _locationService.ListAsync(country, city, cancellationToken));
    }
}
