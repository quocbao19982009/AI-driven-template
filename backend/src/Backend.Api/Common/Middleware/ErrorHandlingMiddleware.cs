using System.Net;
using System.Text.Json;
using Backend.Common.Exceptions;
using Backend.Common.Models;

namespace Backend.Common.Middleware;

public class ErrorHandlingMiddleware
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            NotFoundException ex => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(ex.Message)
            ),
            ValidationException ex => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(ex.Message, ex.Errors)
            ),
            ForbiddenAccessException ex => (
                HttpStatusCode.Forbidden,
                ApiResponse<object>.Fail(ex.Message)
            ),
            UnauthorizedAccessException ex => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail(ex.Message)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("An unexpected error occurred.")
            )
        };

        switch (statusCode)
        {
            case HttpStatusCode.InternalServerError:
                _logger.LogError(exception, "Unhandled exception");
                break;
            case HttpStatusCode.NotFound:
            case HttpStatusCode.BadRequest:
            case HttpStatusCode.Forbidden:
            case HttpStatusCode.Unauthorized:
                _logger.LogWarning("Request failed with {StatusCode}: {Message}", (int)statusCode, exception.Message);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, s_jsonOptions);

        await context.Response.WriteAsync(json);
    }
}
