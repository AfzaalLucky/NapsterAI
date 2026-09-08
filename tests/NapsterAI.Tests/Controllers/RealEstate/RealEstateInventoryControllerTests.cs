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

public class RealEstateInventoryControllerTests
{
    [Fact]
    public async Task List_ReturnsOkWithResults_WhenServiceSucceeds()
    {
        var expected = new PagedResultDto<InventoryDto>
        {
            Items = [new InventoryDto { InventoryId = 1, UnitNumber = "MV-A-0501", Status = "Available" }],
            TotalCount = 1,
            FilteredCount = 1
        };

        var serviceMock = new Mock<IRealEstateInventoryService>();
        serviceMock
            .Setup(s => s.ListAsync(
                null, null, 2, null, null, null, null, null,
                "Available", null, null, null, "price", false, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new RealEstateInventoryController(serviceMock.Object, Mock.Of<IRealEstatePaymentPlanService>(), NullLogger<RealEstateInventoryController>.Instance);

        var result = await controller.List(
            projectId: null, unitTypeId: null, minBedrooms: 2, maxBedrooms: null,
            minPrice: null, maxPrice: null, minAreaSqFt: null, maxAreaSqFt: null,
            status: "Available", viewType: null, furnishingStatus: null, search: null,
            sortBy: "price", sortDescending: false, pageIndex: 0, pageSize: 20, cancellationToken: CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<PagedResultDto<InventoryDto>>(okResult.Value);
        Assert.Single(value.Items);
        Assert.Equal("MV-A-0501", value.Items[0].UnitNumber);
    }

    [Fact]
    public async Task List_PropagatesInvalidRequestException_ForBadSortBy()
    {
        var serviceMock = new Mock<IRealEstateInventoryService>();
        serviceMock
            .Setup(s => s.ListAsync(
                null, null, null, null, null, null, null, null,
                null, null, null, null, "alphabetical", false, 0, 20, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidRequestException("sortBy must be one of: price, area, listingDate."));

        var controller = new RealEstateInventoryController(serviceMock.Object, Mock.Of<IRealEstatePaymentPlanService>(), NullLogger<RealEstateInventoryController>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(() => controller.List(
            projectId: null, unitTypeId: null, minBedrooms: null, maxBedrooms: null,
            minPrice: null, maxPrice: null, minAreaSqFt: null, maxAreaSqFt: null,
            status: null, viewType: null, furnishingStatus: null, search: null,
            sortBy: "alphabetical", sortDescending: false, pageIndex: 0, pageSize: 20, cancellationToken: CancellationToken.None));
    }
}
