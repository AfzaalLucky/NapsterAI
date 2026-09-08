using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NapsterAI.Api.Controllers;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos;
using NapsterAI.Api.Services;
using Xunit;

namespace NapsterAI.Tests.Controllers;

public class CompanionsControllerTests
{
    [Fact]
    public async Task Browse_ReturnsOkWithResults_WhenServiceSucceeds()
    {
        var expected = new PagedResultDto<CompanionDto>
        {
            Items = [new CompanionDto { Id = "cmp.1", FirstName = "Alex", LastName = "Rivers" }],
            TotalCount = 1,
            FilteredCount = 1
        };

        var serviceMock = new Mock<INapsterService>();
        serviceMock
            .Setup(s => s.BrowseCompanionsAsync("alex", null, null, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new CompanionsController(serviceMock.Object, NullLogger<CompanionsController>.Instance);

        var result = await controller.Browse("alex", null, null, 0, 20, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<PagedResultDto<CompanionDto>>(okResult.Value);
        Assert.Single(value.Items);
        Assert.Equal("Alex", value.Items[0].FirstName);
    }

    [Fact]
    public async Task Browse_PropagatesInvalidRequestException_ForBadGender()
    {
        var serviceMock = new Mock<INapsterService>();
        serviceMock
            .Setup(s => s.BrowseCompanionsAsync(null, "robot", null, 0, 20, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidRequestException("gender must be one of: male, female, nonBinary."));

        var controller = new CompanionsController(serviceMock.Object, NullLogger<CompanionsController>.Instance);

        // The controller itself doesn't catch this - it's left to the global
        // ExceptionHandlingMiddleware, so we just confirm it propagates as expected.
        await Assert.ThrowsAsync<InvalidRequestException>(
            () => controller.Browse(null, "robot", null, 0, 20, CancellationToken.None));
    }
}
