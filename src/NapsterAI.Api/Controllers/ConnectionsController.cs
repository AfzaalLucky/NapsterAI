using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Services;

namespace NapsterAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConnectionsController : ControllerBase
{
    private readonly INapsterService _napsterService;
    private readonly ILogger<ConnectionsController> _logger;

    public ConnectionsController(INapsterService napsterService, ILogger<ConnectionsController> logger)
    {
        _napsterService = napsterService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a live connection token for a companion. The returned token is
    /// what a client (web/voice) uses to establish the actual WebRTC/WebSocket
    /// session directly with Napster - our API only brokers it.
    /// </summary>
    /// <remarks>POST /api/connections { "companionId": "...", "providerConfig": { ... } }</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ConnectionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<ConnectionDto>> Create([FromBody] CreateConnectionRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received request to create a connection for companion {CompanionId}", request.CompanionId);

        var connection = await _napsterService.CreateConnectionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = connection.ConnectionId }, connection);
    }
}
