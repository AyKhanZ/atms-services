using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Providers.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using ATMS.Data.Constants;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Account;

public class RegisterUserValidator : AbstractValidator<RegisterCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IOrganizationProvider _organizationProvider;

    public RegisterUserValidator(IUserRepository userRepository,
        IRoleRepository roleRepository,
        IOrganizationProvider organizationProvider)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _organizationProvider = organizationProvider;
        
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ValidationMessages.NameRequired)
            .MaximumLength(100)
            .WithMessage(_ => string.Format(ValidationMessages.NameShouldBeLessThan, 100));

        RuleFor(x => x.Surname).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ValidationMessages.SurnameRequired)
            .MaximumLength(100)
            .WithMessage(_ => string.Format(ValidationMessages.SurnameShouldBeLessThan, 100));

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage(ValidationMessages.RoleIdRequired)
            .Must(BeAllowedRegistrationRole).WithMessage(RoleMessages.NotFound)
            .MustAsync(IsRoleExistAsync).WithMessage(RoleMessages.NotFound);

        RuleFor(x => x.OrganizationId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.OrganizationIdRequired)
            .MustAsync(IsOrganizationExistAsync).WithMessage(AccountMessages.OrganizationIdNotExist)
            .When(x => x.RoleId == RoleIds.Client || x.RoleId == RoleIds.ClientManager);

        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ValidationMessages.EmailRequired)
            .EmailAddress()
            .WithMessage(ValidationMessages.InvalidEmailFormat)
            .MaximumLength(100)
            .WithMessage(_ => string.Format(ValidationMessages.EmailShouldBeLessThan, 100))
            .MustAsync(IsEmailUnique)
            .WithMessage(AccountMessages.UserAlreadyExists);
    }

    private static bool BeAllowedRegistrationRole(Guid roleId)
    {
        return roleId == RoleIds.Employee || roleId == RoleIds.ClientManager || roleId == RoleIds.Client;
    }

    private async Task<bool> IsOrganizationExistAsync(Guid? organizationId, CancellationToken cancellationToken)
    {
        if (!organizationId.HasValue)
        {
            return false;
        }
        
        var result = await _organizationProvider.GetAsync(organizationId.Value, cancellationToken);
        
        return result is not null;
    }
    
    private async Task<bool> IsRoleExistAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var result = await _roleRepository.GetAsync(r => r.Id == roleId, cancellationToken);
        
        return result is not null;
    }

    private async Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken)
    {
        return !await _userRepository.IsExistAsync(u => u.Email == email, cancellationToken);
    }
}