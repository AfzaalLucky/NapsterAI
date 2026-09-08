using Microsoft.AspNetCore.Mvc;
using Moq;
using NapsterAI.Api.Controllers.RealEstate;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Controllers.RealEstate;

public class RealEstateAnalyticsControllerTests
{
    [Fact]
    public async Task LeadsSummary_ReturnsOkWithResult_WhenServiceSucceeds()
    {
        var expected = new LeadsSummaryDto { TotalLeads = 10, ConversionRatePercent = 25.0m, PipelineValue = 500_000m };

        var serviceMock = new Mock<IRealEstateAnalyticsService>();
        serviceMock.Setup(s => s.GetLeadsSummaryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new RealEstateAnalyticsController(serviceMock.Object);
        var result = await controller.LeadsSummary(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(10, Assert.IsType<LeadsSummaryDto>(okResult.Value).TotalLeads);
    }

    [Fact]
    public async Task InventorySummary_ReturnsOkWithResult_WhenServiceSucceeds()
    {
        var expected = new InventorySummaryDto { TotalProjects = 3, TotalUnits = 42, TotalListValue = 10_000_000m };

        var serviceMock = new Mock<IRealEstateAnalyticsService>();
        serviceMock.Setup(s => s.GetInventorySummaryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new RealEstateAnalyticsController(serviceMock.Object);
        var result = await controller.InventorySummary(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(42, Assert.IsType<InventorySummaryDto>(okResult.Value).TotalUnits);
    }
}
