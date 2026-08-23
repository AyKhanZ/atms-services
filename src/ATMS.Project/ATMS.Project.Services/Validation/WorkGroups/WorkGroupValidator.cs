using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.WorkGroups;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkGroups;

public class WorkGroupValidator : AbstractValidator<WorkGroupCommand>
{
    public WorkGroupValidator()
    {
        RuleFor(x => x.Title).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ValidationMessages.TitleRequired)
            .Must(title => !string.IsNullOrWhiteSpace(title)).WithMessage(ValidationMessages.TitleRequired)
            .Must(title => title.Trim().Length <= 100).WithMessage(_ => string.Format(ValidationMessages.TitleShouldBeLessThan, 100));
    }
}
