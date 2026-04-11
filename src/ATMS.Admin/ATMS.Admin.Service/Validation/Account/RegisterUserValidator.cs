using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Account;

public class RegisterUserValidator : AbstractValidator<RegisterCommand>
{
    private readonly IUserRepository _userRepository;

    public RegisterUserValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(AccountMessages.NameRequired)
            .MaximumLength(50)
            .WithMessage(x => string.Format(AccountMessages.NameShouldBeLessThan, 50));

        RuleFor(x => x.Surname).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(AccountMessages.SurnameRequired)
            .MaximumLength(100)
            .WithMessage(x => string.Format(AccountMessages.SurnameShouldBeLessThan, 100));

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage(ValidationMessages.RoleIdRequired);

        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(AccountMessages.EmailRequired)
            .EmailAddress()
            .WithMessage(ValidationMessages.InvalidEmailFormat)
            .MustAsync(IsEmailUnique)
            .WithMessage(AccountMessages.UserAlreadyExists);
    }

    private async Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken)
    {
        return !await _userRepository.IsExistAsync(u => u.Email == email, cancellationToken);
    }
}
