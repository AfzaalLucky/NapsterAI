using Microsoft.AspNetCore.Mvc;
using Moq;
using NapsterAI.Api.Controllers.RealEstate;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Controllers.RealEstate;

public class RealEstateOrganizationsControllerTests
{
    [Fact]
    public async Task List_ReturnsOkWithResults_WhenServiceSucceeds()
    {
        IReadOnlyList<OrganizationDto> expected = [new OrganizationDto { OrganizationId = 1, Name = "Acme Realty" }];

        var serviceMock = new Mock<IRealEstateOrganizationService>();
        serviceMock.Setup(s => s.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new RealEstateOrganizationsController(serviceMock.Object);
        var result = await controller.List(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<OrganizationDto>>(okResult.Value));
    }

    [Fact]
    public async Task GetById_PropagatesRealEstateResourceNotFoundException_WhenMissing()
    {
        var serviceMock = new Mock<IRealEstateOrganizationService>();
        serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateResourceNotFoundException("Organization 999 was not found."));

        var controller = new RealEstateOrganizationsController(serviceMock.Object);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(() => controller.GetById(999, CancellationToken.None));
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenServiceSucceeds()
    {
        var request = new CreateOrganizationRequestDto { Name = "Acme Realty" };
        var created = new OrganizationDto { OrganizationId = 7, Name = "Acme Realty" };

        var serviceMock = new Mock<IRealEstateOrganizationService>();
        serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var controller = new RealEstateOrganizationsController(serviceMock.Object);
        var result = await controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(7, Assert.IsType<OrganizationDto>(createdResult.Value).OrganizationId);
    }

    [Fact]
    public async Task Delete_PropagatesRealEstateConflictException_WhenSalesAgentsReference()
    {
        var serviceMock = new Mock<IRealEstateOrganizationService>();
        serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateConflictException("Organization 1 still has sales agents and cannot be deleted."));

        var controller = new RealEstateOrganizationsController(serviceMock.Object);

        await Assert.ThrowsAsync<RealEstateConflictException>(() => controller.Delete(1, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenServiceSucceeds()
    {
        var serviceMock = new Mock<IRealEstateOrganizationService>();
        serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var controller = new RealEstateOrganizationsController(serviceMock.Object);
        var result = await controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }
}
