using Microsoft.AspNetCore.Mvc;
using Moq;
using NapsterAI.Api.Controllers.RealEstate;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Controllers.RealEstate;

public class RealEstateUnitTypesControllerTests
{
    [Fact]
    public async Task List_ReturnsOkWithResults_WhenServiceSucceeds()
    {
        IReadOnlyList<UnitTypeDto> expected = [new UnitTypeDto { UnitTypeId = 1, TypeName = "1 Bedroom" }];

        var serviceMock = new Mock<IRealEstateUnitTypeService>();
        serviceMock.Setup(s => s.ListAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new RealEstateUnitTypesController(serviceMock.Object);
        var result = await controller.List(null, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<UnitTypeDto>>(okResult.Value));
    }

    [Fact]
    public async Task GetById_PropagatesRealEstateResourceNotFoundException_WhenMissing()
    {
        var serviceMock = new Mock<IRealEstateUnitTypeService>();
        serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateResourceNotFoundException("Unit type 999 was not found."));

        var controller = new RealEstateUnitTypesController(serviceMock.Object);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(() => controller.GetById(999, CancellationToken.None));
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenServiceSucceeds()
    {
        var request = new CreateUnitTypeRequestDto { ProjectId = 1, TypeName = "1 Bedroom", Category = "Apartment" };
        var created = new UnitTypeDto { UnitTypeId = 7, TypeName = "1 Bedroom" };

        var serviceMock = new Mock<IRealEstateUnitTypeService>();
        serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var controller = new RealEstateUnitTypesController(serviceMock.Object);
        var result = await controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(7, Assert.IsType<UnitTypeDto>(createdResult.Value).UnitTypeId);
    }

    [Fact]
    public async Task Create_PropagatesInvalidRequestException_WhenCategoryInvalid()
    {
        var request = new CreateUnitTypeRequestDto { ProjectId = 1, TypeName = "1 Bedroom", Category = "Bogus" };

        var serviceMock = new Mock<IRealEstateUnitTypeService>();
        serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidRequestException("category must be one of: Apartment, Villa, Townhouse, Duplex, Penthouse, Office, Retail, Hotel."));

        var controller = new RealEstateUnitTypesController(serviceMock.Object);

        await Assert.ThrowsAsync<InvalidRequestException>(() => controller.Create(request, CancellationToken.None));
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var request = new UpdateUnitTypeRequestDto { ProjectId = 1, TypeName = "2 Bedroom", Category = "Apartment" };
        var updated = new UnitTypeDto { UnitTypeId = 1, TypeName = "2 Bedroom" };

        var serviceMock = new Mock<IRealEstateUnitTypeService>();
        serviceMock.Setup(s => s.UpdateAsync(1, request, It.IsAny<CancellationToken>())).ReturnsAsync(updated);

        var controller = new RealEstateUnitTypesController(serviceMock.Object);
        var result = await controller.Update(1, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("2 Bedroom", Assert.IsType<UnitTypeDto>(okResult.Value).TypeName);
    }

    [Fact]
    public async Task Update_PropagatesRealEstateResourceNotFoundException_WhenMissing()
    {
        var request = new UpdateUnitTypeRequestDto { ProjectId = 1, TypeName = "2 Bedroom", Category = "Apartment" };

        var serviceMock = new Mock<IRealEstateUnitTypeService>();
        serviceMock.Setup(s => s.UpdateAsync(999, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateResourceNotFoundException("Unit type 999 was not found."));

        var controller = new RealEstateUnitTypesController(serviceMock.Object);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(() => controller.Update(999, request, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenServiceSucceeds()
    {
        var serviceMock = new Mock<IRealEstateUnitTypeService>();
        serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var controller = new RealEstateUnitTypesController(serviceMock.Object);
        var result = await controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_PropagatesRealEstateConflictException_WhenInventoryReferences()
    {
        var serviceMock = new Mock<IRealEstateUnitTypeService>();
        serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateConflictException("Unit type 1 still has inventory units and cannot be deleted."));

        var controller = new RealEstateUnitTypesController(serviceMock.Object);

        await Assert.ThrowsAsync<RealEstateConflictException>(() => controller.Delete(1, CancellationToken.None));
    }
}
