using System.Text.RegularExpressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Account;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    private readonly IUserRepository _userRepository;
    
    public ChangePasswordValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required")
            .MustAsync(IsUserExistAsync).WithMessage("No account found with this email")
            .EmailAddress().WithMessage("Please enter a valid email (e.g. user@example.com)");
        
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Old password is required");

        RuleFor(x => x.NewPassword).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 symbols")
            .MaximumLength(40).WithMessage("Password must be less than 40 symbols")
            .Must(IsValidPassword)
            .WithMessage("Password must include uppercase, number, special char (!@#$%^&*()-_=+), no spaces");

        _userRepository = userRepository;
    }

    private Task<bool> IsUserExistAsync(string email, CancellationToken cancellationToken)
    {
        var result = _userRepository.IsExistAsync(u => u.Email == email, cancellationToken);
        return result;
    }

    private bool IsValidPassword(string password)
    {
        // Explains:
        // ^                  - start
        // (?=.*[A-Z])        - at least one Uppercase letter
        // (?=.*\d)           - at least one number
        // (?=.*[!@#$%^&*()\-_+=]) - at least one special symbol
        // [A-Za-z\d!@#$%^&*()\-_+=] {6,40} - only valid symbols, length 6-40
        // $                  - end
        var regex = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()\-_+=])[A-Za-z\d!@#$%^&*()\-_+=]{6,40}$");
        return regex.IsMatch(password);
    }
}