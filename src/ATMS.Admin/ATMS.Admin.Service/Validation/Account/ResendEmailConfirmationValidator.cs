using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Account;

public class ResendEmailConfirmationValidator : AbstractValidator<ResendEmailConfirmationCommand>
{
    private readonly IUserRepository _userRepository;

    public ResendEmailConfirmationValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MustAsync(IsUserExistAsync).WithMessage("User with the specified email does not exist.")
            .MustAsync(IsEmailConfirmedAsync).WithMessage("Email is already confirmed.");
    }

    private Task<bool> IsEmailConfirmedAsync(string email, CancellationToken cancellationToken)
    {
        return _userRepository.IsExistAsync(u => u.Email == email && !u.EmailConfirmed, cancellationToken);
    }

    private Task<bool> IsUserExistAsync(string email, CancellationToken cancellationToken)
    {
        return _userRepository.IsExistAsync(u => u.Email == email, cancellationToken);
    }
}
