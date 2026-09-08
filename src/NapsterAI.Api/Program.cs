using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NapsterAI.Api.Configuration;
using NapsterAI.Api.Data;
using NapsterAI.Api.Middleware;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services;
using NapsterAI.Api.Services.RealEstate;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration -----------------------------------------------------
// Binds the "Napster" section from appsettings.json / user-secrets / env vars.
// See README.md for how to supply Napster:ApiKey without committing it.
builder.Services
    .AddOptions<NapsterOptions>()
    .Bind(builder.Configuration.GetSection(NapsterOptions.SectionName))
    .ValidateDataAnnotations();

// --- Logging -------------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// --- Real Estate data access ---------------------------------------------
// New local SQL Server persistence for the Real Estate module (the existing Napster
// integration proxies live data and persists nothing locally). Kept as its own
// DbContext/connection string - see database\RealEstateDB.sql for the original schema
// this supersedes, and RealEstateImplementationPlan.md for the full rationale.
// Skipped under the "Testing" environment - RealEstateApiFactory (integration tests)
// registers an isolated EF Core InMemory context instead, since composing a second
// AddDbContext call with a different provider on top of this one throws at runtime
// ("Only a single database provider can be registered").
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<RealEstateDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("RealEstateDb")));
}

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IRealEstateProjectService, RealEstateProjectService>();
builder.Services.AddScoped<IRealEstateUnitTypeService, RealEstateUnitTypeService>();
builder.Services.AddScoped<IRealEstateInventoryService, RealEstateInventoryService>();
builder.Services.AddScoped<IRealEstateAmenityService, RealEstateAmenityService>();
builder.Services.AddScoped<IRealEstateLocationService, RealEstateLocationService>();
builder.Services.AddScoped<IRealEstateLookupService, RealEstateLookupService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRealEstateOrganizationService, RealEstateOrganizationService>();
builder.Services.AddScoped<IRealEstateAgentService, RealEstateAgentService>();
builder.Services.AddScoped<IRealEstateInquiryService, RealEstateInquiryService>();
builder.Services.AddScoped<IRealEstateLeadService, RealEstateLeadService>();
builder.Services.AddScoped<IRealEstateViewingService, RealEstateViewingService>();
builder.Services.AddScoped<IRealEstatePaymentPlanService, RealEstatePaymentPlanService>();
builder.Services.AddScoped<IRealEstateAnalyticsService, RealEstateAnalyticsService>();

// --- Auth (JWT bearer) -----------------------------------------------
// Net-new - no auth of any kind existed before this. See Configuration\JwtOptions.cs and
// README for how to supply Jwt:SigningKey without committing it (same convention as
// Napster:ApiKey). If it's missing, generate a random one for this run only so the app
// still starts locally - tokens issued now just won't validate after a restart.
if (string.IsNullOrWhiteSpace(builder.Configuration["Jwt:SigningKey"]))
{
    var generatedKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    builder.Configuration["Jwt:SigningKey"] = generatedKey;
    Console.WriteLine(
        "[WARN] Jwt:SigningKey is not configured - using a random ephemeral key for this run. " +
        "Tokens issued now will fail validation after a restart. Set it via 'dotnet user-secrets set Jwt:SigningKey <key>'.");
}

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "NapsterAI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "NapsterAI";
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]!; // guaranteed non-empty above

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole(RealEstateRoles.Admin))
    .AddPolicy("AgentOrAdmin", policy => policy.RequireRole(RealEstateRoles.Agent, RealEstateRoles.Admin));

// --- Rate limiting ------------------------------------------------------
// Blunts abuse of the anonymous write endpoints (POST inquiries/viewings - the public
// contact-us/booking forms and the EdgeMCP createLead/bookViewing tools - and login, to slow
// down credential-stuffing attempts). Partitioned per client IP so one abusive caller doesn't
// exhaust the limit for everyone else.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("anonymous-writes", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = 10,
            QueueLimit = 0
        }));

    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = 5,
            QueueLimit = 0
        }));
});

// --- Services --------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Lets local frontend dev servers (Playground, the new Real Estate frontend) call this API
// cross-origin. Config-driven via the "Cors" section (see Configuration\CorsOptions.cs)
// instead of a hardcoded origin list, so prod origins can be added without a code change.
const string AppCorsPolicy = "AppCors";
var configuredCorsOrigins = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()?.AllowedOrigins ?? [];
var corsOrigins = configuredCorsOrigins.Length > 0
    ? configuredCorsOrigins
    : ["http://localhost:5173", "http://localhost:4173", "http://localhost:5174"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(AppCorsPolicy, policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "NapsterAI API",
        Version = "v1",
        Description = "A small ASP.NET Core API that wraps the Napster Companion/Agent platform API."
    });
});

// Typed HttpClient for talking to Napster, with a sane timeout, the required
// X-Api-Key header, and a retry policy for transient failures (network blips, 5xx, 429).
builder.Services.AddHttpClient<INapsterService, NapsterService>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<NapsterOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);

    if (!string.IsNullOrWhiteSpace(options.ApiVersion))
    {
        client.DefaultRequestHeaders.Add("X-API-Version", options.ApiVersion);
    }
})
.AddPolicyHandler(GetRetryPolicy());

var app = builder.Build();

// Apply pending EF Core migrations and seed dev sample data on startup in Development,
// so `dotnet run` gives a working RealEstateDB with no manual migration step.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<RealEstateDbContext>();
    await dbContext.Database.MigrateAsync();
    await RealEstateDbInitializer.SeedAsync(dbContext);
}

// --- Middleware pipeline -----------------------------------------------
// Catches exceptions from anywhere below and turns them into ProblemDetails responses.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(AppCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // 5xx and 408
        .OrResult(response => response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)));
}

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program
{
}
