using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkTasks;

public class DeleteWorkTaskValidator : AbstractValidator<DeleteWorkTaskCommand>
{
    public DeleteWorkTaskValidator()
    {
        RuleFor(command => command.ProjectId)
            .NotEmpty().WithMessage(WorkTaskMessages.ProjectRequired);

        RuleFor(command => command.WorkTaskId)
            .NotEmpty().WithMessage(WorkTaskMessages.TaskRequired);
    }
}
