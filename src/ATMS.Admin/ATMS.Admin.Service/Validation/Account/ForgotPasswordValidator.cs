using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Application.Exceptions.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Account;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ValidationMessages.EmailRequired)
            .EmailAddress().WithMessage(ValidationMessages.InvalidEmailFormat);
    }
}