using Microsoft.AspNetCore.Mvc;
using Moq;
using NapsterAI.Api.Controllers.RealEstate;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Controllers.RealEstate;

public class RealEstateLocationsControllerTests
{
    [Fact]
    public async Task List_ReturnsOkWithResults_WhenServiceSucceeds()
    {
        IReadOnlyList<LocationDto> expected = [new LocationDto { LocationId = 1, Country = "UAE", City = "Dubai" }];

        var serviceMock = new Mock<IRealEstateLocationService>();
        serviceMock.Setup(s => s.ListAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new RealEstateLocationsController(serviceMock.Object);
        var result = await controller.List(null, null, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<LocationDto>>(okResult.Value));
    }

    [Fact]
    public async Task List_PassesCountryAndCityFilters_ToService()
    {
        IReadOnlyList<LocationDto> expected = [new LocationDto { LocationId = 2, Country = "UAE", City = "Dubai" }];

        var serviceMock = new Mock<IRealEstateLocationService>();
        serviceMock.Setup(s => s.ListAsync("UAE", "Dubai", It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new RealEstateLocationsController(serviceMock.Object);
        var result = await controller.List("UAE", "Dubai", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);
        serviceMock.Verify(s => s.ListAsync("UAE", "Dubai", It.IsAny<CancellationToken>()), Times.Once);
    }
}
