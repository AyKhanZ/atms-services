using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Organization;

public class OrganizationValidator : AbstractValidator<OrganizationCommand>
{
    private readonly IOrganizationRepository _organizationRepository;
    
    public OrganizationValidator(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
        
        RuleFor(x => x.Title).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ValidationMessages.TitleRequired)
            .MaximumLength(100).WithMessage(_ => string.Format(ValidationMessages.TitleShouldBeLessThan, 100))
            .MustAsync(IsTitleUniqueAsync).WithMessage(ValidationMessages.TitleAlreadyExists);
        
        RuleFor(x => x.Voen).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(OrganizationMessages.VoenRequired)
            .MaximumLength(20).WithMessage(_ => string.Format(OrganizationMessages.VoenShouldBeLessThan, 20))
            .MustAsync(IsVoenUniqueAsync).WithMessage(OrganizationMessages.VoenAlreadyTaken);
    }

    private async Task<bool> IsTitleUniqueAsync(OrganizationCommand command, string title, CancellationToken cancellationToken)
    {
        var updateId = (command as UpdateOrganizationCommand)?.Id;

        return !await _organizationRepository.IsExistAsync(
            o => o.Title == title && (!updateId.HasValue || o.Id != updateId.Value),
            cancellationToken);
    }
    
    private async Task<bool> IsVoenUniqueAsync(OrganizationCommand command, string voen, CancellationToken cancellationToken)
    {
        var updateId = (command as UpdateOrganizationCommand)?.Id;

        return !await _organizationRepository.IsExistAsync(
            o => o.Voen == voen && (!updateId.HasValue || o.Id != updateId.Value),
            cancellationToken);
    }
}
