using FluentValidation;
using FollowUpCrm.Shared.Results;

namespace FollowUpCrm.Api.Middleware;

public sealed class GlobalExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

            var (statusCode, message, errors) = CreateErrorResponse(exception);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.FailureResponse(message, errors);

            await context.Response.WriteAsJsonAsync(response);
        }
    }

    private (int StatusCode, string Message, IReadOnlyCollection<string> Errors) CreateErrorResponse(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                validationException.Errors.Select(error => error.ErrorMessage).ToArray()),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                exception.Message,
                Array.Empty<string>()),
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                exception.Message,
                Array.Empty<string>()),
            _ => (
                StatusCodes.Status500InternalServerError,
                _environment.IsProduction()
                    ? "An unexpected error occurred. Please try again later."
                    : exception.Message,
                Array.Empty<string>())
        };
    }
}
