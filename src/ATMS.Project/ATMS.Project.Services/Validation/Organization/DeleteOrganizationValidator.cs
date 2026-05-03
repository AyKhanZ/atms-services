using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.Organization;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Organization;

public class DeleteOrganizationValidator : AbstractValidator<DeleteOrganizationCommand>
{
    public DeleteOrganizationValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);
    }
}