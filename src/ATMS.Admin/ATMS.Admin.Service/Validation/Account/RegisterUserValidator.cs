using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Account;

public class RegisterUserValidator : AbstractValidator<RegisterCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public RegisterUserValidator(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Name is required .")
            .MaximumLength(50)
            .WithMessage("Name should be max 50 symbols .");

        RuleFor(x => x.Surname).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Surname is required .")
            .MaximumLength(100)
            .WithMessage("Surname should be max 100 symbols .");

        RuleFor(x => x.RoleId)
            .MustAsync(IsRoleExist)
            .WithMessage("Role doesn't exist .");

        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email is required .")
            .EmailAddress()
            .WithMessage("Email is invalid .")
            .MustAsync(IsEmailUnique)
            .WithMessage("User with this email already exist .");
    }

    private async Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken)
    {
        return !await _userRepository.IsExistAsync(u => u.Email == email, cancellationToken);
    }

    private async Task<bool> IsRoleExist(Guid roleId, CancellationToken cancellationToken)
    {
        return await _roleRepository.IsExistAsync(r => r.Id == roleId, cancellationToken);
    }
}
