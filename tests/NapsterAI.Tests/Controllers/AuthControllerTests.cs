using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NapsterAI.Api.Controllers;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Services.RealEstate;
using Xunit;

namespace NapsterAI.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_ReturnsOkWithResult_WhenServiceSucceeds()
    {
        var request = new LoginRequestDto { Email = "admin@example.com", Password = "P@ssw0rd!" };
        var expected = new AuthResultDto { AccessToken = "access", RefreshToken = "refresh", Email = request.Email, Role = "Admin" };

        var serviceMock = new Mock<IAuthService>();
        serviceMock.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new AuthController(serviceMock.Object, NullLogger<AuthController>.Instance);
        var result = await controller.Login(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("access", Assert.IsType<AuthResultDto>(okResult.Value).AccessToken);
    }

    [Fact]
    public async Task Login_PropagatesInvalidRequestException_WhenCredentialsWrong()
    {
        var request = new LoginRequestDto { Email = "admin@example.com", Password = "wrong" };

        var serviceMock = new Mock<IAuthService>();
        serviceMock.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidRequestException("Invalid email or password."));

        var controller = new AuthController(serviceMock.Object, NullLogger<AuthController>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(() => controller.Login(request, CancellationToken.None));
    }

    [Fact]
    public async Task Refresh_ReturnsOkWithResult_WhenServiceSucceeds()
    {
        var request = new RefreshTokenRequestDto { RefreshToken = "valid-token" };
        var expected = new AuthResultDto { AccessToken = "new-access", RefreshToken = "new-refresh", Email = "admin@example.com", Role = "Admin" };

        var serviceMock = new Mock<IAuthService>();
        serviceMock.Setup(s => s.RefreshAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new AuthController(serviceMock.Object, NullLogger<AuthController>.Instance);
        var result = await controller.Refresh(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("new-access", Assert.IsType<AuthResultDto>(okResult.Value).AccessToken);
    }

    [Fact]
    public async Task Refresh_PropagatesInvalidRequestException_WhenTokenInvalid()
    {
        var request = new RefreshTokenRequestDto { RefreshToken = "bogus" };

        var serviceMock = new Mock<IAuthService>();
        serviceMock.Setup(s => s.RefreshAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidRequestException("Invalid or expired refresh token."));

        var controller = new AuthController(serviceMock.Object, NullLogger<AuthController>.Instance);

        await Assert.ThrowsAsync<InvalidRequestException>(() => controller.Refresh(request, CancellationToken.None));
    }
}
