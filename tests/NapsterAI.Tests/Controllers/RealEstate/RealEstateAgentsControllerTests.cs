using Microsoft.AspNetCore.Mvc;
using Moq;
using NapsterAI.Api.Controllers.RealEstate;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Controllers.RealEstate;

public class RealEstateAgentsControllerTests
{
    [Fact]
    public async Task List_ReturnsOkWithResults_WhenServiceSucceeds()
    {
        IReadOnlyList<RealEstateAgentDto> expected = [new RealEstateAgentDto { SalesAgentId = 1, FullName = "Sam Agent" }];

        var serviceMock = new Mock<IRealEstateAgentService>();
        serviceMock.Setup(s => s.ListAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new RealEstateAgentsController(serviceMock.Object);
        var result = await controller.List(null, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<RealEstateAgentDto>>(okResult.Value));
    }

    [Fact]
    public async Task Create_PropagatesInvalidRequestException_WhenOrganizationUnknown()
    {
        var request = new CreateRealEstateAgentRequestDto { OrganizationId = 999, FullName = "Sam Agent" };

        var serviceMock = new Mock<IRealEstateAgentService>();
        serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidRequestException("organizationId 999 does not reference an existing organization."));

        var controller = new RealEstateAgentsController(serviceMock.Object);

        await Assert.ThrowsAsync<InvalidRequestException>(() => controller.Create(request, CancellationToken.None));
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenServiceSucceeds()
    {
        var request = new CreateRealEstateAgentRequestDto { OrganizationId = 1, FullName = "Sam Agent" };
        var created = new RealEstateAgentDto { SalesAgentId = 3, OrganizationId = 1, FullName = "Sam Agent" };

        var serviceMock = new Mock<IRealEstateAgentService>();
        serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var controller = new RealEstateAgentsController(serviceMock.Object);
        var result = await controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(3, Assert.IsType<RealEstateAgentDto>(createdResult.Value).SalesAgentId);
    }

    [Fact]
    public async Task Delete_PropagatesRealEstateConflictException_WhenStillReferenced()
    {
        var serviceMock = new Mock<IRealEstateAgentService>();
        serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateConflictException("Sales agent 1 is still referenced and cannot be deleted."));

        var controller = new RealEstateAgentsController(serviceMock.Object);

        await Assert.ThrowsAsync<RealEstateConflictException>(() => controller.Delete(1, CancellationToken.None));
    }
}
