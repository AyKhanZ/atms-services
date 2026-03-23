using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Account;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepository;

    public ForgotPasswordValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is invalid")
            .MustAsync(IsUserExistAsync).WithMessage("No account found with this email");
    }

    private Task<bool> IsUserExistAsync(string email, CancellationToken cancellationToken)
    {
        return _userRepository.IsExistAsync(u => u.Email == email, cancellationToken);
    }
}