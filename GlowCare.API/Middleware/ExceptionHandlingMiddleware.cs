using GlowCare.API.Helpers;
using System.Text.Json;

namespace GlowCare.API.Middleware;

/// <summary>
/// Global exception handler. Registered FIRST in the pipeline so it catches
/// errors thrown by any later middleware/controller/service.
///
/// - AppException carries an HTTP status + message from the service layer
///   (business rule violations like "not enough stock", "not your order").
/// - Anything else is logged as an error and returned as a generic 500 so we
///   never leak stack traces or database details to the client.
///
/// Services therefore never build HTTP responses themselves - they just throw.
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
        catch (AppException ex)
        {
            // Expected business error - log as warning, return the given status.
            _logger.LogWarning(
                "Business error {Status} for {Method} {Path}: {Message}",
                ex.StatusCode, context.Request.Method, context.Request.Path, ex.Message);

            await WriteErrorAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            // Unexpected error - log the full details server-side only.
            _logger.LogError(ex,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError,
                "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        // If the response already started (e.g. the controller wrote part of the
        // body), we cannot change the status code anymore.
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new { statusCode, message });
        await context.Response.WriteAsync(payload);
    }
}
