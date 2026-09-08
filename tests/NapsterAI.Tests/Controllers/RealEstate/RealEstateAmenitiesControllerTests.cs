using Microsoft.AspNetCore.Mvc;
using Moq;
using NapsterAI.Api.Controllers.RealEstate;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Controllers.RealEstate;

public class RealEstateAmenitiesControllerTests
{
    [Fact]
    public async Task List_ReturnsOkWithResults_WhenServiceSucceeds()
    {
        IReadOnlyList<AmenityDto> expected = [new AmenityDto { AmenityId = 1, AmenityName = "Rooftop Pool" }];

        var serviceMock = new Mock<IRealEstateAmenityService>();
        serviceMock.Setup(s => s.ListAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new RealEstateAmenitiesController(serviceMock.Object);
        var result = await controller.List(null, null, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<AmenityDto>>(okResult.Value));
    }

    [Fact]
    public async Task GetById_PropagatesRealEstateResourceNotFoundException_WhenMissing()
    {
        var serviceMock = new Mock<IRealEstateAmenityService>();
        serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateResourceNotFoundException("Amenity 999 was not found."));

        var controller = new RealEstateAmenitiesController(serviceMock.Object);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(() => controller.GetById(999, CancellationToken.None));
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenServiceSucceeds()
    {
        var request = new CreateAmenityRequestDto { ProjectId = 1, AmenityName = "Rooftop Pool", Category = "Recreational" };
        var created = new AmenityDto { AmenityId = 7, AmenityName = "Rooftop Pool" };

        var serviceMock = new Mock<IRealEstateAmenityService>();
        serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var controller = new RealEstateAmenitiesController(serviceMock.Object);
        var result = await controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(7, Assert.IsType<AmenityDto>(createdResult.Value).AmenityId);
    }

    [Fact]
    public async Task Create_PropagatesInvalidRequestException_WhenCategoryInvalid()
    {
        var request = new CreateAmenityRequestDto { ProjectId = 1, AmenityName = "Rooftop Pool", Category = "Bogus" };

        var serviceMock = new Mock<IRealEstateAmenityService>();
        serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidRequestException("category must be one of: Recreational, Security, Wellness, Convenience, Business."));

        var controller = new RealEstateAmenitiesController(serviceMock.Object);

        await Assert.ThrowsAsync<InvalidRequestException>(() => controller.Create(request, CancellationToken.None));
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var request = new UpdateAmenityRequestDto { ProjectId = 1, AmenityName = "Infinity Pool", Category = "Wellness" };
        var updated = new AmenityDto { AmenityId = 1, AmenityName = "Infinity Pool" };

        var serviceMock = new Mock<IRealEstateAmenityService>();
        serviceMock.Setup(s => s.UpdateAsync(1, request, It.IsAny<CancellationToken>())).ReturnsAsync(updated);

        var controller = new RealEstateAmenitiesController(serviceMock.Object);
        var result = await controller.Update(1, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("Infinity Pool", Assert.IsType<AmenityDto>(okResult.Value).AmenityName);
    }

    [Fact]
    public async Task Update_PropagatesRealEstateResourceNotFoundException_WhenMissing()
    {
        var request = new UpdateAmenityRequestDto { ProjectId = 1, AmenityName = "Infinity Pool", Category = "Wellness" };

        var serviceMock = new Mock<IRealEstateAmenityService>();
        serviceMock.Setup(s => s.UpdateAsync(999, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateResourceNotFoundException("Amenity 999 was not found."));

        var controller = new RealEstateAmenitiesController(serviceMock.Object);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(() => controller.Update(999, request, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenServiceSucceeds()
    {
        var serviceMock = new Mock<IRealEstateAmenityService>();
        serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var controller = new RealEstateAmenitiesController(serviceMock.Object);
        var result = await controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_PropagatesRealEstateResourceNotFoundException_WhenMissing()
    {
        var serviceMock = new Mock<IRealEstateAmenityService>();
        serviceMock.Setup(s => s.DeleteAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateResourceNotFoundException("Amenity 999 was not found."));

        var controller = new RealEstateAmenitiesController(serviceMock.Object);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(() => controller.Delete(999, CancellationToken.None));
    }
}
