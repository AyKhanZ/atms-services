using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Authentication;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    private readonly IUserRepository userRepository;

    public LoginValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email is required .")
            .MustAsync(IsEmailExist)
            .WithMessage("User with such email doesn't exist .");

        RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Password is required .");

        this.userRepository = userRepository;
    }

    private async Task<bool> IsEmailExist(string email, CancellationToken cancellationToken)
    {
        return await userRepository.IsExistAsync(email, cancellationToken);
    }
}
