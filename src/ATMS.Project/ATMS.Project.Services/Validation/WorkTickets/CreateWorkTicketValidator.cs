using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkTickets;

public class CreateWorkTicketValidator : AbstractValidator<CreateWorkTicketCommand>
{
    public CreateWorkTicketValidator(
        IWorkTicketRepository workTicketRepository,
        IDictionariesRepository dictionariesRepository)
    {
        RuleFor(command => command.ProjectId)
            .NotEmpty().WithMessage(WorkTicketMessages.ProjectRequired);

        RuleFor(command => command)
            .SetValidator(new WorkTicketValidator(workTicketRepository, dictionariesRepository));
    }
}
