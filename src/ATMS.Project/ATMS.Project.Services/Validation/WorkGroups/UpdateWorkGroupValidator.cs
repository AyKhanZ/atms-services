using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkGroups;

public class UpdateWorkGroupValidator : AbstractValidator<UpdateWorkGroupCommand>
{
    private readonly IWorkGroupRepository _workGroupRepository;

    public UpdateWorkGroupValidator(IWorkGroupRepository workGroupRepository)
    {
        _workGroupRepository = workGroupRepository;

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);

        RuleFor(x => x.WorkGroupId)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);

        RuleFor(x => x).SetValidator(new WorkGroupValidator());

        RuleFor(x => x)
            .CustomAsync(ValidateUniqueTitleAsync)
            .When(ShouldCheckTitle);
    }

    private async Task ValidateUniqueTitleAsync(
        UpdateWorkGroupCommand command,
        ValidationContext<UpdateWorkGroupCommand> context,
        CancellationToken cancellationToken)
    {
        var workGroup = await _workGroupRepository.FindAsync(
            command.ProjectId,
            command.WorkGroupId,
            cancellationToken);
        if (workGroup is null)
        {
            return;
        }

        var titleExists = await _workGroupRepository.IsSiblingTitleExistAsync(
            command.ProjectId,
            workGroup.ParentWorkGroupId,
            command.Title.Trim().ToLowerInvariant(),
            command.WorkGroupId,
            cancellationToken);

        if (titleExists)
        {
            context.AddFailure(
                nameof(command.Title),
                WorkGroupMessages.DuplicateTitle(workGroup.ParentWorkGroupId.HasValue));
        }
    }

    private static bool ShouldCheckTitle(UpdateWorkGroupCommand command)
    {
        return command.ProjectId != Guid.Empty &&
               command.WorkGroupId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(command.Title) &&
               command.Title.Trim().Length <= 100;
    }
}
