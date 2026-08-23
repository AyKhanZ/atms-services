using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkGroups;

public class CreateWorkGroupValidator : AbstractValidator<CreateWorkGroupCommand>
{
    private readonly IWorkGroupRepository _workGroupRepository;

    public CreateWorkGroupValidator(IWorkGroupRepository workGroupRepository)
    {
        _workGroupRepository = workGroupRepository;

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);

        RuleFor(x => x.ParentWorkGroupId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage(ValidationMessages.IdRequired);

        RuleFor(x => x).SetValidator(new WorkGroupValidator());

        RuleFor(x => x)
            .CustomAsync(ValidateUniqueTitleAsync)
            .When(ShouldCheckTitle);
    }

    private async Task ValidateUniqueTitleAsync(
        CreateWorkGroupCommand command,
        ValidationContext<CreateWorkGroupCommand> context,
        CancellationToken cancellationToken)
    {
        var titleExists = await _workGroupRepository.IsSiblingTitleExistAsync(
            command.ProjectId,
            command.ParentWorkGroupId,
            command.Title.Trim().ToLowerInvariant(),
            excludedWorkGroupId: null,
            cancellationToken);

        if (titleExists)
        {
            context.AddFailure(
                nameof(command.Title),
                WorkGroupMessages.DuplicateTitle(command.ParentWorkGroupId.HasValue));
        }
    }

    private static bool ShouldCheckTitle(CreateWorkGroupCommand command)
    {
        return command.ProjectId != Guid.Empty &&
               command.ParentWorkGroupId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(command.Title) &&
               command.Title.Trim().Length <= 100;
    }
}
