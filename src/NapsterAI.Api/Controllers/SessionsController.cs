using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Services;

namespace NapsterAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly INapsterService _napsterService;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(INapsterService napsterService, ILogger<SessionsController> logger)
    {
        _napsterService = napsterService;
        _logger = logger;
    }

    /// <summary>
    /// Lists past conversation sessions, optionally filtered by companion, client, or type.
    /// </summary>
    /// <remarks>GET /api/sessions?companionId=...&amp;sessionType=webrtc&amp;pageIndex=0&amp;pageSize=20</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<SessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<PagedResultDto<SessionDto>>> List(
        [FromQuery] string? companionId,
        [FromQuery] string? externalClientId,
        [FromQuery] string? sessionType,
        [FromQuery] string? search,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Received request to list sessions (companionId={CompanionId})", companionId);

        var result = await _napsterService.ListSessionsAsync(
            companionId, externalClientId, sessionType, search, pageIndex, pageSize, cancellationToken);

        return Ok(result);
    }
}
