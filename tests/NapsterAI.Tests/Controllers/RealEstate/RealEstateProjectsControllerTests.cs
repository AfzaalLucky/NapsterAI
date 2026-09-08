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

public class RealEstateProjectsControllerTests
{
    [Fact]
    public async Task List_ReturnsOkWithResults_WhenServiceSucceeds()
    {
        var expected = new PagedResultDto<ProjectDto>
        {
            Items = [new ProjectDto { ProjectId = 1, ProjectCode = "PRJ-001", ProjectName = "Marina Vista Towers" }],
            TotalCount = 1,
            FilteredCount = 1
        };

        var serviceMock = new Mock<IRealEstateProjectService>();
        serviceMock
            .Setup(s => s.ListAsync("Dubai", null, null, null, null, null, null, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new RealEstateProjectsController(serviceMock.Object, Mock.Of<IRealEstatePaymentPlanService>(), NullLogger<RealEstateProjectsController>.Instance);

        var result = await controller.List("Dubai", null, null, null, null, null, null, 0, 20, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<PagedResultDto<ProjectDto>>(okResult.Value);
        Assert.Single(value.Items);
        Assert.Equal("Marina Vista Towers", value.Items[0].ProjectName);
    }

    [Fact]
    public async Task GetById_PropagatesRealEstateResourceNotFoundException_WhenMissing()
    {
        var serviceMock = new Mock<IRealEstateProjectService>();
        serviceMock
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateResourceNotFoundException("Project 999 was not found."));

        var controller = new RealEstateProjectsController(serviceMock.Object, Mock.Of<IRealEstatePaymentPlanService>(), NullLogger<RealEstateProjectsController>.Instance);

        // The controller doesn't catch this itself - it's left to the global
        // ExceptionHandlingMiddleware, so we just confirm it propagates as expected.
        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => controller.GetById(999, CancellationToken.None));
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenServiceSucceeds()
    {
        var request = new CreateProjectRequestDto
        {
            ProjectCode = "PRJ-100",
            ProjectName = "New Project",
            ProjectType = "Residential",
            Status = "Planning",
            Country = "UAE",
            City = "Dubai",
            Currency = "AED"
        };
        var created = new ProjectDto { ProjectId = 42, ProjectCode = request.ProjectCode, ProjectName = request.ProjectName };

        var serviceMock = new Mock<IRealEstateProjectService>();
        serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var controller = new RealEstateProjectsController(serviceMock.Object, Mock.Of<IRealEstatePaymentPlanService>(), NullLogger<RealEstateProjectsController>.Instance);

        var result = await controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var value = Assert.IsType<ProjectDto>(createdResult.Value);
        Assert.Equal(42, value.ProjectId);
    }

    [Fact]
    public async Task Create_PropagatesRealEstateConflictException_ForDuplicateProjectCode()
    {
        var request = new CreateProjectRequestDto { ProjectCode = "PRJ-001", ProjectName = "Dup", ProjectType = "Residential", Status = "Planning", Country = "UAE", City = "Dubai", Currency = "AED" };

        var serviceMock = new Mock<IRealEstateProjectService>();
        serviceMock
            .Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateConflictException("A project with code 'PRJ-001' already exists."));

        var controller = new RealEstateProjectsController(serviceMock.Object, Mock.Of<IRealEstatePaymentPlanService>(), NullLogger<RealEstateProjectsController>.Instance);

        await Assert.ThrowsAsync<RealEstateConflictException>(() => controller.Create(request, CancellationToken.None));
    }
}
