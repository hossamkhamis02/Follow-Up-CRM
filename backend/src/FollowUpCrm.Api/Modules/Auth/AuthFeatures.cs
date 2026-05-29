using FluentValidation;
using FollowUpCrm.Infrastructure.Persistence.Entities;

namespace FollowUpCrm.Api.Modules.Auth;

public sealed record RegisterRequest(string FullName, string Email, string Password, UserRole Role);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(Guid UserId, string FullName, string Email, string Role, string Token, DateTime ExpiresAt);

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");

        RuleFor(request => request.Role)
            .IsInEnum();
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(request => request.Password)
            .NotEmpty();
    }
}
