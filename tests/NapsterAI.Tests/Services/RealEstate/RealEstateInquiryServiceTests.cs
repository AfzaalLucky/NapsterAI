using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class RealEstateInquiryServiceTests
{
    private static RealEstateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RealEstateDbContext(options);
    }

    private static CreateInquiryRequestDto ValidRequest() => new()
    {
        CustomerName = "Jane Buyer",
        CustomerEmail = "jane@example.com",
        Channel = RealEstateLeadSources.Website,
        Message = "Interested in a 2BR unit."
    };

    [Fact]
    public async Task CreateAsync_Throws_WhenChannelInvalid()
    {
        using var db = CreateContext();
        var service = new RealEstateInquiryService(db, NullLogger<RealEstateInquiryService>.Instance);

        var request = ValidRequest();
        request.Channel = "Bogus";

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenProjectIdDoesNotExist()
    {
        using var db = CreateContext();
        var service = new RealEstateInquiryService(db, NullLogger<RealEstateInquiryService>.Instance);

        var request = ValidRequest();
        request.ProjectId = 999;

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Succeeds()
    {
        using var db = CreateContext();
        var service = new RealEstateInquiryService(db, NullLogger<RealEstateInquiryService>.Instance);

        var result = await service.CreateAsync(ValidRequest(), CancellationToken.None);

        Assert.True(result.InquiryId > 0);
        Assert.Null(result.ConvertedToLeadId);
        Assert.Equal("jane@example.com", result.Customer.Email);
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenNotFound()
    {
        using var db = CreateContext();
        var service = new RealEstateInquiryService(db, NullLogger<RealEstateInquiryService>.Instance);

        await Assert.ThrowsAsync<RealEstateResourceNotFoundException>(
            () => service.GetByIdAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task ConvertToLeadAsync_CreatesLead_AndLinksInquiry()
    {
        using var db = CreateContext();
        var service = new RealEstateInquiryService(db, NullLogger<RealEstateInquiryService>.Instance);
        var inquiry = await service.CreateAsync(ValidRequest(), CancellationToken.None);

        var lead = await service.ConvertToLeadAsync(inquiry.InquiryId, CancellationToken.None);

        Assert.True(lead.LeadId > 0);
        Assert.Equal(RealEstateLeadStatuses.New, lead.Status);

        var reloaded = await service.GetByIdAsync(inquiry.InquiryId, CancellationToken.None);
        Assert.Equal(lead.LeadId, reloaded.ConvertedToLeadId);
    }

    [Fact]
    public async Task ConvertToLeadAsync_Throws_WhenAlreadyConverted()
    {
        using var db = CreateContext();
        var service = new RealEstateInquiryService(db, NullLogger<RealEstateInquiryService>.Instance);
        var inquiry = await service.CreateAsync(ValidRequest(), CancellationToken.None);
        await service.ConvertToLeadAsync(inquiry.InquiryId, CancellationToken.None);

        await Assert.ThrowsAsync<RealEstateConflictException>(
            () => service.ConvertToLeadAsync(inquiry.InquiryId, CancellationToken.None));
    }
}
