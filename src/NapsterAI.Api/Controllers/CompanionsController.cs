using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Services;

namespace NapsterAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanionsController : ControllerBase
{
    private readonly INapsterService _napsterService;
    private readonly ILogger<CompanionsController> _logger;

    public CompanionsController(INapsterService napsterService, ILogger<CompanionsController> logger)
    {
        _napsterService = napsterService;
        _logger = logger;
    }

    /// <summary>
    /// Browses Napster's stock AI companions, optionally filtered by name, gender, or ethnicity.
    /// </summary>
    /// <remarks>GET /api/companions?search=alex&amp;gender=female&amp;pageIndex=0&amp;pageSize=20</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<CompanionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<PagedResultDto<CompanionDto>>> Browse(
        [FromQuery] string? search,
        [FromQuery] string? gender,
        [FromQuery] string? ethnicity,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Received request to browse companions (search={Search})", search);

        var result = await _napsterService.BrowseCompanionsAsync(search, gender, ethnicity, pageIndex, pageSize, cancellationToken);
        return Ok(result);
    }
}
