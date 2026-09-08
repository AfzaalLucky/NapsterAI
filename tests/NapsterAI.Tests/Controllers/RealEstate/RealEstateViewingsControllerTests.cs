using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NapsterAI.Api.Controllers.RealEstate;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Controllers.RealEstate;

public class RealEstateViewingsControllerTests
{
    [Fact]
    public async Task List_ReturnsOkWithResults_WhenServiceSucceeds()
    {
        var expected = new PagedResultDto<ViewingDto>
        {
            Items = [new ViewingDto { ViewingId = 1, Status = "Requested" }],
            TotalCount = 1,
            FilteredCount = 1
        };

        var serviceMock = new Mock<IRealEstateViewingService>();
        serviceMock.Setup(s => s.ListAsync(null, null, null, 0, 20, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new RealEstateViewingsController(serviceMock.Object, NullLogger<RealEstateViewingsController>.Instance);
        var result = await controller.List(null, null, null, 0, 20, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(Assert.IsType<PagedResultDto<ViewingDto>>(okResult.Value).Items);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenServiceSucceeds()
    {
        var request = new CreateViewingRequestDto { CustomerName = "Jane", CustomerEmail = "jane@example.com", InventoryId = 1, ScheduledDate = DateTime.UtcNow.AddDays(2) };
        var created = new ViewingDto { ViewingId = 11, Status = "Requested" };

        var serviceMock = new Mock<IRealEstateViewingService>();
        serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var controller = new RealEstateViewingsController(serviceMock.Object, NullLogger<RealEstateViewingsController>.Instance);
        var result = await controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(11, Assert.IsType<ViewingDto>(createdResult.Value).ViewingId);
    }

    [Fact]
    public async Task Create_PropagatesInvalidRequestException_WhenInventoryUnknown()
    {
        var request = new CreateViewingRequestDto { CustomerName = "Jane", CustomerEmail = "jane@example.com", InventoryId = 999, ScheduledDate = DateTime.UtcNow.AddDays(2) };

        var serviceMock = new Mock<IRealEstateViewingService>();
        serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidRequestException("inventoryId 999 does not reference an existing inventory unit."));

        var controller = new RealEstateViewingsController(serviceMock.Object, NullLogger<RealEstateViewingsController>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(() => controller.Create(request, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatus_PropagatesRealEstateResourceNotFoundException_WhenMissing()
    {
        var request = new UpdateViewingStatusRequestDto { Status = "Completed" };

        var serviceMock = new Mock<IRealEstateViewingService>();
        serviceMock.Setup(s => s.UpdateStatusAsync(999, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateResourceNotFoundException("Viewing 999 was not found."));

        var controller = new RealEstateViewingsController(serviceMock.Object, NullLogger<RealEstateViewingsController>.Instance);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(() => controller.UpdateStatus(999, request, CancellationToken.None));
    }
}
