using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Enums;
using ATMS.Admin.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Authentication;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    private readonly IUserRepository userRepository;

    public LoginValidator(
        IUserRepository userRepository)
    {
        RuleFor(x => x).Cascade(CascadeMode.Stop)
            // Deleted status
            .MustAsync(IsStatusDeletedAsync)
            .WithMessage("Your account is not active anymore. Please, contact support .")
            // Locked status
            .CustomAsync(async (command, context, cancellationToken) =>
            {
                var result = await IsStatusLockedAsync(command, cancellationToken);
                if (result)
                {
                    var message = await GetMessageLockedAsync(command, cancellationToken);
                    context.AddFailure(message);
                }
            });

        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email is required .")
            .MustAsync(IsEmailExistAsync)
            .WithMessage("User with such email doesn't exist .")
            .MustAsync(IsEmailConfirmedAsync)
            .WithMessage("Email not confirmed .");

        RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Password is required .");

        this.userRepository = userRepository;
    }

    private Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken)
    {
        return userRepository.IsExistAsync(u => u.Email == email, cancellationToken);
    }

    private async Task<bool> IsEmailConfirmedAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(u => u.Email == email, cancellationToken);

        return user?.EmailConfirmed ?? false;
    }

    private async Task<bool> IsStatusLockedAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository
            .FindAsync(u => u.Email == command.Email, cancellationToken);

        return user?.UserStatusId == (int)UserStatusEnum.Locked && user.LockoutEnd > DateTime.UtcNow;
    }

    private async Task<string> GetMessageLockedAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository
            .FindAsync(u => u.Email == command.Email, cancellationToken);

        var remainingTime = user.LockoutEnd - DateTime.UtcNow;
        return $"Account is locked. Try again in {remainingTime.Minutes} minutes. ";
    }

    private async Task<bool> IsStatusDeletedAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository
            .FindAsync(u => u.Email == command.Email, cancellationToken);

        return user?.UserStatusId == (int)UserStatusEnum.Inactive;
    }
}
