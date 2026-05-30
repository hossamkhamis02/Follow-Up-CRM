using System.Security.Claims;
using FluentValidation;
using FollowUpCrm.Api.Authentication;
using FollowUpCrm.Infrastructure.Persistence;
using FollowUpCrm.Infrastructure.Persistence.Entities;
using FollowUpCrm.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FollowUpCrm.Api.Modules.Auth;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        return services;
    }

    public static IEndpointRouteBuilder MapAuthModule(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", RegisterAsync)
            .AllowAnonymous()
            .WithName("Register")
            .Produces<Result<AuthResponse>>(StatusCodes.Status201Created)
            .Produces<Result>(StatusCodes.Status400BadRequest);

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .Produces<Result<AuthResponse>>()
            .Produces<Result>(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", GetCurrentUserAsync)
            .RequireAuthorization()
            .WithName("GetCurrentAuthUser")
            .Produces<CurrentUserResponse>()
            .Produces<Result>(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        IValidator<RegisterRequest> validator,
        ApplicationDbContext dbContext,
        IJwtTokenService tokenService,
        IOptions<JwtOptions> jwtOptions,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Results.BadRequest(Result.Failure(string.Join(" ", validationResult.Errors.Select(error => error.ErrorMessage))));

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExists = await dbContext.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
        if (emailExists)
            return Results.BadRequest(Result.Failure("A user with this email already exists."));

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = CreateAuthResponse(user, tokenService, jwtOptions.Value);

        return Results.Created($"/api/auth/users/{user.Id}", Result<AuthResponse>.Success(response));
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IValidator<LoginRequest> validator,
        ApplicationDbContext dbContext,
        IJwtTokenService tokenService,
        IOptions<JwtOptions> jwtOptions,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Results.BadRequest(Result.Failure(string.Join(" ", validationResult.Errors.Select(error => error.ErrorMessage))));

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.SingleOrDefaultAsync(entity => entity.Email == normalizedEmail, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Results.Json(Result.Failure("Invalid email or password.", 401), statusCode: StatusCodes.Status401Unauthorized);

        var response = CreateAuthResponse(user, tokenService, jwtOptions.Value);

        return Results.Ok(Result<AuthResponse>.Success(response));
    }

    [Authorize]
    private static IResult GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirstValue("UserId");
        var email = principal.FindFirstValue("Email");
        var fullName = principal.FindFirstValue("FullName");
        var role = principal.FindFirstValue("Role");

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Json(Result.Failure("Invalid authentication token.", 401), statusCode: StatusCodes.Status401Unauthorized);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(role))
            return Results.Json(Result.Failure("Invalid authentication token.", 401), statusCode: StatusCodes.Status401Unauthorized);

        return Results.Ok(new CurrentUserResponse(userId, email, fullName, role));
    }

    private static AuthResponse CreateAuthResponse(User user, IJwtTokenService tokenService, JwtOptions jwtOptions)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.ExpiresInMinutes);
        var token = tokenService.GenerateToken(user, expiresAt);

        return new AuthResponse(user.Id, user.FullName, user.Email, user.Role.ToString(), token, expiresAt);
    }
}
