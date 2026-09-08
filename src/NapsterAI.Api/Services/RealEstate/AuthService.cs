using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NapsterAI.Api.Configuration;
using NapsterAI.Api.Data;
using NapsterAI.Api.Exceptions;
using NapsterAI.Api.Models.Dtos.RealEstate;
using NapsterAI.Api.Models.Entities.RealEstate;

namespace NapsterAI.Api.Services.RealEstate;

/// <summary>
/// Hand-rolled JWT bearer auth against the local Users table (not ASP.NET Core Identity -
/// matches the codebase's already-lightweight, hand-rolled style, e.g. NapsterService's own
/// HttpClient wrapper instead of a generated client). Issues a short-lived JWT access token
/// plus an opaque, server-stored, rotated-on-use refresh token.
/// </summary>
public class AuthService : IAuthService
{
    private readonly RealEstateDbContext _db;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(RealEstateDbContext db, IOptions<JwtOptions> jwtOptions, ILogger<AuthService> logger)
    {
        _db = db;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidRequestException("email and password are required.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        // Deliberately the same error whether the account doesn't exist, is inactive, or the
        // password is wrong - don't tell an attacker which part failed.
        if (user is null || !user.IsActive || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", normalizedEmail);
            throw new InvalidRequestException("Invalid email or password.");
        }

        _logger.LogInformation("User {Email} logged in", user.Email);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResultDto> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new InvalidRequestException("refreshToken is required.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user is null || !user.IsActive || user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidRequestException("Invalid or expired refresh token.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    private async Task<AuthResultDto> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = GenerateAccessToken(user, out var accessTokenExpiresAt);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);

        // Rotated on every issue (login or refresh) so a leaked refresh token has a shrinking window of use.
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            Email = user.Email,
            Role = user.Role
        };
    }

    private string GenerateAccessToken(User user, out DateTime expiresAt)
    {
        expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.SalesAgentId is not null)
        {
            // Lets a future "my leads"/"my inventory" scoping filter by the caller's own agent
            // profile without a database round trip. See RealEstateLeadsController for a
            // representative read of this claim.
            claims.Add(new Claim("salesAgentId", user.SalesAgentId.Value.ToString()));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
}
