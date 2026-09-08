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

public class RealEstateInquiriesControllerTests
{
    [Fact]
    public async Task List_ReturnsOkWithResults_WhenServiceSucceeds()
    {
        var expected = new PagedResultDto<InquiryDto>
        {
            Items = [new InquiryDto { InquiryId = 1, Channel = "Website" }],
            TotalCount = 1,
            FilteredCount = 1
        };

        var serviceMock = new Mock<IRealEstateInquiryService>();
        serviceMock.Setup(s => s.ListAsync(null, 0, 20, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new RealEstateInquiriesController(serviceMock.Object, NullLogger<RealEstateInquiriesController>.Instance);
        var result = await controller.List(null, 0, 20, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(Assert.IsType<PagedResultDto<InquiryDto>>(okResult.Value).Items);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenServiceSucceeds()
    {
        var request = new CreateInquiryRequestDto { CustomerName = "Jane", CustomerEmail = "jane@example.com", Channel = "Website" };
        var created = new InquiryDto { InquiryId = 9, Channel = "Website" };

        var serviceMock = new Mock<IRealEstateInquiryService>();
        serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var controller = new RealEstateInquiriesController(serviceMock.Object, NullLogger<RealEstateInquiriesController>.Instance);
        var result = await controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(9, Assert.IsType<InquiryDto>(createdResult.Value).InquiryId);
    }

    [Fact]
    public async Task Create_PropagatesInvalidRequestException_WhenChannelInvalid()
    {
        var request = new CreateInquiryRequestDto { CustomerName = "Jane", CustomerEmail = "jane@example.com", Channel = "Bogus" };

        var serviceMock = new Mock<IRealEstateInquiryService>();
        serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidRequestException("channel must be one of: Website, AI Assistant, Call, WalkIn."));

        var controller = new RealEstateInquiriesController(serviceMock.Object, NullLogger<RealEstateInquiriesController>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(() => controller.Create(request, CancellationToken.None));
    }

    [Fact]
    public async Task ConvertToLead_PropagatesRealEstateConflictException_WhenAlreadyConverted()
    {
        var serviceMock = new Mock<IRealEstateInquiryService>();
        serviceMock.Setup(s => s.ConvertToLeadAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealEstateConflictException("Inquiry 1 was already converted to lead 5."));

        var controller = new RealEstateInquiriesController(serviceMock.Object, NullLogger<RealEstateInquiriesController>.Instance);

        await Assert.ThrowsAsync<RealEstateConflictException>(() => controller.ConvertToLead(1, CancellationToken.None));
    }

    [Fact]
    public async Task ConvertToLead_ReturnsOk_WhenServiceSucceeds()
    {
        var lead = new LeadDto { LeadId = 5, Status = "New" };

        var serviceMock = new Mock<IRealEstateInquiryService>();
        serviceMock.Setup(s => s.ConvertToLeadAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(lead);

        var controller = new RealEstateInquiriesController(serviceMock.Object, NullLogger<RealEstateInquiriesController>.Instance);
        var result = await controller.ConvertToLead(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(5, Assert.IsType<LeadDto>(okResult.Value).LeadId);
    }
}
