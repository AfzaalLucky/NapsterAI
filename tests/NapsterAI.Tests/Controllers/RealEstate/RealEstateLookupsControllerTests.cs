using Microsoft.AspNetCore.Mvc;
using Moq;
using NapsterAI.Api.Controllers.RealEstate;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Controllers.RealEstate;

public class RealEstateLookupsControllerTests
{
    [Fact]
    public async Task List_ReturnsOkWithResults_WhenServiceSucceeds()
    {
        IReadOnlyList<LookupDto> expected = [new LookupDto { LookupId = 1, LookupType = "ProjectType", Code = "Residential" }];

        var serviceMock = new Mock<IRealEstateLookupService>();
        serviceMock.Setup(s => s.ListAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new RealEstateLookupsController(serviceMock.Object);
        var result = await controller.List(null, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<LookupDto>>(okResult.Value));
    }

    [Fact]
    public async Task List_PassesTypeFilter_ToService()
    {
        IReadOnlyList<LookupDto> expected = [new LookupDto { LookupId = 2, LookupType = "ProjectType", Code = "Commercial" }];

        var serviceMock = new Mock<IRealEstateLookupService>();
        serviceMock.Setup(s => s.ListAsync("ProjectType", It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new RealEstateLookupsController(serviceMock.Object);
        var result = await controller.List("ProjectType", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);
        serviceMock.Verify(s => s.ListAsync("ProjectType", It.IsAny<CancellationToken>()), Times.Once);
    }
}
