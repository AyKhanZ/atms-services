using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.Organizations;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Organizations;

public class DeleteOrganizationValidator : AbstractValidator<DeleteOrganizationCommand>
{
    public DeleteOrganizationValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);
    }
}