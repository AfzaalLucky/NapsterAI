using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Services;

namespace NapsterAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaqsController : ControllerBase
{
    private readonly INapsterService _napsterService;
    private readonly ILogger<FaqsController> _logger;

    public FaqsController(INapsterService napsterService, ILogger<FaqsController> logger)
    {
        _napsterService = napsterService;
        _logger = logger;
    }

    /// <summary>
    /// Lists FAQ collections - named sets of question/answer pairs an agent can be pointed at.
    /// </summary>
    /// <remarks>GET /api/faqs?search=billing&amp;pageIndex=0&amp;pageSize=20</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<FaqCollectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<PagedResultDto<FaqCollectionDto>>> List(
        [FromQuery] string? search,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Received request to list FAQ collections (search={Search})", search);

        var result = await _napsterService.ListFaqCollectionsAsync(search, pageIndex, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new FAQ collection, optionally pre-populated with question/answer pairs.
    /// </summary>
    /// <remarks>POST /api/faqs { "name": "...", "faqs": [{ "question": "...", "answer": "..." }] }</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(FaqCollectionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<FaqCollectionDto>> Create([FromBody] CreateFaqCollectionRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received request to create FAQ collection {Name}", request.Name);

        var faqCollection = await _napsterService.CreateFaqCollectionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = faqCollection.Id }, faqCollection);
    }

    /// <summary>
    /// Lists the question/answer items in one FAQ collection.
    /// </summary>
    /// <remarks>GET /api/faqs/{faqCollectionId}/items?pageIndex=0&amp;pageSize=20</remarks>
    [HttpGet("{faqCollectionId}/items")]
    [ProducesResponseType(typeof(PagedResultDto<FaqItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<PagedResultDto<FaqItemDto>>> ListItems(
        string faqCollectionId,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Received request to list items for FAQ collection {FaqCollectionId}", faqCollectionId);

        var result = await _napsterService.ListFaqItemsAsync(faqCollectionId, pageIndex, pageSize, cancellationToken);
        return Ok(result);
    }
}
