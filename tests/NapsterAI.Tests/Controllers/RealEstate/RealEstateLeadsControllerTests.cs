using Microsoft.AspNetCore.Http;
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

public class RealEstateLeadsControllerTests
{
    private static RealEstateLeadsController CreateController(Mock<IRealEstateLeadService> serviceMock) =>
        new(serviceMock.Object, NullLogger<RealEstateLeadsController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

    [Fact]
    public async Task List_ReturnsOkWithResults_WhenServiceSucceeds()
    {
        var expected = new PagedResultDto<LeadDto>
        {
            Items = [new LeadDto { LeadId = 1, Status = "New" }],
            TotalCount = 1,
            FilteredCount = 1
        };

        var serviceMock = new Mock<IRealEstateLeadService>();
        serviceMock.Setup(s => s.ListAsync(null, null, null, 0, 20, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = CreateController(serviceMock);
        var result = await controller.List(null, null, null, 0, 20, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<PagedResultDto<LeadDto>>(okResult.Value);
        Assert.Single(value.Items);
    }

    [Fact]
    public async Task GetById_PropagatesRealEstateResourceNotFoundException_WhenMissing()
    {
        var serviceMock = new Mock<IRealEstateLeadService>();
        serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateResourceNotFoundException("Lead 999 was not found."));

        var controller = CreateController(serviceMock);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(() => controller.GetById(999, CancellationToken.None));
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenServiceSucceeds()
    {
        var request = new CreateLeadRequestDto { CustomerName = "Jane", CustomerEmail = "jane@example.com" };
        var created = new LeadDto { LeadId = 42, Status = "New" };

        var serviceMock = new Mock<IRealEstateLeadService>();
        serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var controller = CreateController(serviceMock);
        var result = await controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var value = Assert.IsType<LeadDto>(createdResult.Value);
        Assert.Equal(42, value.LeadId);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsOk_WhenServiceSucceeds()
    {
        var request = new UpdateLeadStatusRequestDto { Status = "Contacted" };
        var updated = new LeadDto { LeadId = 1, Status = "Contacted" };

        var serviceMock = new Mock<IRealEstateLeadService>();
        serviceMock.Setup(s => s.UpdateStatusAsync(1, request, null, It.IsAny<CancellationToken>())).ReturnsAsync(updated);

        var controller = CreateController(serviceMock);
        var result = await controller.UpdateStatus(1, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("Contacted", Assert.IsType<LeadDto>(okResult.Value).Status);
    }

    [Fact]
    public async Task Assign_PropagatesInvalidRequestException_WhenSalesAgentUnknown()
    {
        var request = new AssignLeadRequestDto { SalesAgentId = 999 };

        var serviceMock = new Mock<IRealEstateLeadService>();
        serviceMock.Setup(s => s.AssignAsync(1, request, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidRequestException("salesAgentId 999 does not reference an existing sales agent."));

        var controller = CreateController(serviceMock);

        await Assert.ThrowsAsync<InvalidRequestException>(() => controller.Assign(1, request, CancellationToken.None));
    }

    [Fact]
    public async Task AddActivity_ReturnsCreatedAtAction_WhenServiceSucceeds()
    {
        var request = new CreateLeadActivityRequestDto { ActivityType = "Note", Notes = "Called customer" };
        var created = new LeadActivityDto { LeadActivityId = 5, ActivityType = "Note" };

        var serviceMock = new Mock<IRealEstateLeadService>();
        serviceMock.Setup(s => s.AddActivityAsync(1, request, null, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var controller = CreateController(serviceMock);
        var result = await controller.AddActivity(1, request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(5, Assert.IsType<LeadActivityDto>(createdResult.Value).LeadActivityId);
    }
}
