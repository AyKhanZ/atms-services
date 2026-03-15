using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using FluentValidation;
using System.Text.RegularExpressions;

namespace ATMS.Admin.Service.Validation.Account;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;

    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()\-_+=])[A-Za-z\d!@#$%^&*()\-_+=]{6,40}$",
        RegexOptions.Compiled);

    public ResetPasswordValidator(IPasswordResetTokenRepository passwordResetTokenRepository)
    {
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 symbols")
            .MaximumLength(40).WithMessage("Password must be less than 40 symbols")
            .Must(IsValidPassword).WithMessage("Password must include uppercase, number, special char (!@#$%^&*()-_=+), no spaces");

        RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("ConfirmPassword is required.")
            .Equal(x => x.Password).WithMessage("Password and confirmation password do not match.");

        RuleFor(x => x.Token).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Reset password token is required.")
            .MustAsync(IsTokenExistAsync).WithMessage("Invalid password reset token.")
            .MustAsync(IsTokenExpiredAsync).WithMessage("Expired password reset token.");

        _passwordResetTokenRepository = passwordResetTokenRepository;
    }

    private Task<bool> IsTokenExistAsync(string token, CancellationToken cancellationToken)
    {
        return _passwordResetTokenRepository.IsExistAsync(token, cancellationToken);
    }

    private async Task<bool> IsTokenExpiredAsync(string token, CancellationToken cancellationToken)
    {
        var entity = await _passwordResetTokenRepository.FindAsync(t => t.Token == token, cancellationToken);

        if (entity == null) return false;

        return entity.ExpiresAt > DateTime.UtcNow;
    }

    private bool IsValidPassword(string password)
    {
        return PasswordRegex.IsMatch(password);
    }
}
