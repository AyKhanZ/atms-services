using ATMS.Admin.Contracts.Commands.Users;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using ATMS.Data.Enums;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Users;

public sealed class UpdateUserStatusValidator : AbstractValidator<UpdateUserStatusCommand>
{
    public UpdateUserStatusValidator()
    {
        RuleFor(s => s.Id)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);

        // locked is only the automatic 15-minute pause after five wrong passwords
        RuleFor(s => s.UserStatusId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.UserStatusRequired)
            .Must(statusId => statusId is (int)UserStatusEnum.Active or (int)UserStatusEnum.Inactive)
            .WithMessage(ProfileMessages.UserStatusNotSupported);
    }
}