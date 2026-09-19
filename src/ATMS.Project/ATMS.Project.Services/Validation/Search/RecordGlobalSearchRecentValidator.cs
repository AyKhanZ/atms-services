using ATMS.Project.Contracts.Commands.Search;
using ATMS.Data.Enums;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Search;

public class RecordGlobalSearchRecentValidator : AbstractValidator<RecordGlobalSearchRecentCommand>
{
    public RecordGlobalSearchRecentValidator()
    {
        RuleFor(command => command.ItemId)
            .NotEmpty().WithMessage(GlobalSearchMessages.ItemRequired);

        RuleFor(command => command.ItemType)
            .Must(itemType => Enum.IsDefined((GlobalSearchItemType)itemType))
            .WithMessage(GlobalSearchMessages.ItemTypeUnsupported);
    }
}
