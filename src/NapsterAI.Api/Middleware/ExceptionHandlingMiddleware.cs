using System.Net;
using Microsoft.AspNetCore.Mvc;
using NapsterAI.Api.Exceptions;

namespace NapsterAI.Api.Middleware;

/// <summary>
/// Central place that turns exceptions raised anywhere in the pipeline into
/// consistent RFC 7807 ProblemDetails responses, so controllers don't need
/// try/catch blocks of their own for the common failure cases.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidRequestException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Invalid request", ex.Message);
        }
        catch (NapsterResourceNotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, "Resource not found", ex.Message);
        }
        catch (NapsterConflictException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.Conflict, "Conflict", ex.Message);
        }
        catch (RealEstateResourceNotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, "Resource not found", ex.Message);
        }
        catch (RealEstateConflictException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.Conflict, "Conflict", ex.Message);
        }
        catch (NapsterApiException ex)
        {
            // An upstream failure isn't the client's fault, but it isn't a bug in
            // our code either - 502 Bad Gateway communicates "the API we depend on failed".
            _logger.LogError(ex, "Napster API call failed");
            await WriteProblemAsync(context, HttpStatusCode.BadGateway, "Napster API error", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(
                context,
                HttpStatusCode.InternalServerError,
                "Unexpected error",
                "An unexpected error occurred while processing the request.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string title, string detail)
    {
        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
