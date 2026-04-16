using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Providers.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Account;

public class RegisterUserValidator : AbstractValidator<RegisterCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationProvider _organizationProvider;

    public RegisterUserValidator(IUserRepository userRepository,
        IOrganizationProvider organizationProvider)
    {
        _userRepository = userRepository;
        _organizationProvider = organizationProvider;
        
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(AccountMessages.NameRequired)
            .MaximumLength(50)
            .WithMessage(_ => string.Format(AccountMessages.NameShouldBeLessThan, 50));

        RuleFor(x => x.Surname).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(AccountMessages.SurnameRequired)
            .MaximumLength(100)
            .WithMessage(_ => string.Format(AccountMessages.SurnameShouldBeLessThan, 100));

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage(ValidationMessages.RoleIdRequired)
            .IsInEnum().WithMessage(RoleMessages.NotFound);

        RuleFor(x => x.OrganizationId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.OrganizationIdRequired)
            .MustAsync(IsOrganizationExistAsync).WithMessage(AccountMessages.OrganizationIdNotExist)
            .When(x => x.RoleId == RoleIds.Client || x.RoleId == RoleIds.ClientManager);

        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(AccountMessages.EmailRequired)
            .EmailAddress()
            .WithMessage(ValidationMessages.InvalidEmailFormat)
            .MustAsync(IsEmailUnique)
            .WithMessage(AccountMessages.UserAlreadyExists);
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

    private async Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken)
    {
        return !await _userRepository.IsExistAsync(u => u.Email == email, cancellationToken);
    }
}
