using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NapsterAI.Api.Configuration;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Services.RealEstate;

public class AuthServiceTests
{
    private static RealEstateDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RealEstateDbContext(options);
    }

    private static IOptions<JwtOptions> JwtOptions() => Options.Create(new JwtOptions
    {
        SigningKey = "unit-test-signing-key-at-least-32-bytes-long!!",
        AccessTokenExpiryMinutes = 60,
        RefreshTokenExpiryDays = 7
    });

    private static async Task<User> SeedUserAsync(RealEstateDbContext db, string email = "admin@example.com", string password = "P@ssw0rd!", string role = RealEstateRoles.Admin)
    {
        var user = new User { Email = email, PasswordHash = PasswordHasher.Hash(password), Role = role };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenEmailOrPasswordMissing()
    {
        using var db = CreateContext();
        var service = new AuthService(db, JwtOptions(), NullLogger<AuthService>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => service.LoginAsync(new LoginRequestDto { Email = "", Password = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenUserDoesNotExist()
    {
        using var db = CreateContext();
        var service = new AuthService(db, JwtOptions(), NullLogger<AuthService>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.LoginAsync(
            new LoginRequestDto { Email = "nobody@example.com", Password = "whatever" }, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenPasswordWrong()
    {
        using var db = CreateContext();
        await SeedUserAsync(db);
        var service = new AuthService(db, JwtOptions(), NullLogger<AuthService>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.LoginAsync(
            new LoginRequestDto { Email = "admin@example.com", Password = "wrong-password" }, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenUserInactive()
    {
        using var db = CreateContext();
        var user = await SeedUserAsync(db);
        user.IsActive = false;
        await db.SaveChangesAsync();
        var service = new AuthService(db, JwtOptions(), NullLogger<AuthService>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.LoginAsync(
            new LoginRequestDto { Email = "admin@example.com", Password = "P@ssw0rd!" }, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_Succeeds_AndIssuesTokens()
    {
        using var db = CreateContext();
        await SeedUserAsync(db);
        var service = new AuthService(db, JwtOptions(), NullLogger<AuthService>.Instance);

        var result = await service.LoginAsync(new LoginRequestDto { Email = "admin@example.com", Password = "P@ssw0rd!" }, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.Equal(RealEstateRoles.Admin, result.Role);
        Assert.True(result.AccessTokenExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task RefreshAsync_Throws_WhenTokenUnknown()
    {
        using var db = CreateContext();
        var service = new AuthService(db, JwtOptions(), NullLogger<AuthService>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.RefreshAsync(
            new RefreshTokenRequestDto { RefreshToken = "does-not-exist" }, CancellationToken.None));
    }

    [Fact]
    public async Task RefreshAsync_Succeeds_AndRotatesToken()
    {
        using var db = CreateContext();
        await SeedUserAsync(db);
        var service = new AuthService(db, JwtOptions(), NullLogger<AuthService>.Instance);
        var loginResult = await service.LoginAsync(new LoginRequestDto { Email = "admin@example.com", Password = "P@ssw0rd!" }, CancellationToken.None);

        var refreshed = await service.RefreshAsync(new RefreshTokenRequestDto { RefreshToken = loginResult.RefreshToken }, CancellationToken.None);

        Assert.NotEqual(loginResult.RefreshToken, refreshed.RefreshToken);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.RefreshAsync(
            new RefreshTokenRequestDto { RefreshToken = loginResult.RefreshToken }, CancellationToken.None));
    }

    [Fact]
    public async Task RefreshAsync_Throws_WhenTokenExpired()
    {
        using var db = CreateContext();
        var user = await SeedUserAsync(db);
        user.RefreshToken = "expired-token";
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(-1);
        await db.SaveChangesAsync();
        var service = new AuthService(db, JwtOptions(), NullLogger<AuthService>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.RefreshAsync(
            new RefreshTokenRequestDto { RefreshToken = "expired-token" }, CancellationToken.None));
    }
}
