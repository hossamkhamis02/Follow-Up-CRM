using MediatR;
using FollowUpCrm.Shared.Results;

namespace FollowUpCrm.Api.Modules.Identity.Features;

public sealed record RegisterCommand(string Email, string Password, string DisplayName) : IRequest<Result<Guid>>;
public sealed record LoginCommand(string Email, string Password) : IRequest<Result<TokenResponse>>;
public sealed record RefreshTokenCommand(string Token, string RefreshToken) : IRequest<Result<TokenResponse>>;
public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<Result<UserResponse>>;
public sealed record GetUserByIdQuery(Guid Id) : IRequest<Result<UserResponse>>;

public record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
public record UserResponse(Guid Id, string Email, string DisplayName);