using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.WorkProjects;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkProjects;

public class DeleteWorkProjectValidator : AbstractValidator<DeleteWorkProjectCommand>
{
    public DeleteWorkProjectValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);
    }
}
