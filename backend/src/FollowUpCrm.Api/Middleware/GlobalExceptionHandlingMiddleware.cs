using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using FollowUpCrm.Shared.Results;

namespace FollowUpCrm.Api.Middleware;

public class GlobalExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

            context.Response.StatusCode = exception is ArgumentException ? 400 : 500;
            context.Response.ContentType = "application/json";

            var error = exception is ArgumentException
                ? Result.Failure(exception.Message, 400)
                : Result.Failure("An unexpected error occurred. Please try again later.", 500);

            await context.Response.WriteAsJsonAsync(error);
        }
    }
}