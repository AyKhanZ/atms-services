using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.WorkProjects;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkProjects;

public class DeleteWorkProjectParticipantValidator : AbstractValidator<DeleteWorkProjectParticipantCommand>
{
    public DeleteWorkProjectParticipantValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage(ValidationMessages.IdRequired);
        RuleFor(x => x.ParticipantId).NotEmpty().WithMessage(ValidationMessages.IdRequired);
    }
}
