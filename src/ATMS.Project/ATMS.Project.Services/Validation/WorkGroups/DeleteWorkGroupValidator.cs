using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.WorkGroups;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkGroups;

public class DeleteWorkGroupValidator : AbstractValidator<DeleteWorkGroupCommand>
{
    public DeleteWorkGroupValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);

        RuleFor(x => x.WorkGroupId)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);
    }
}
