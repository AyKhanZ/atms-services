using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Enums;
using ATMS.Admin.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Authentication;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    private readonly IUserRepository _userRepository;

    public LoginValidator(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
        
        RuleFor(x => x).Cascade(CascadeMode.Stop)
            // Deleted status
            .MustAsync(IsStatusDeletedAsync)
            .WithMessage("Your account is not active anymore. Please, contact support .")
            // Locked status
            .CustomAsync(async (command, context, cancellationToken) =>
            {
                var user = await _userRepository.FindAsync(u => u.Email == command.Email, cancellationToken);

                if (user?.UserStatusId == (int)UserStatusEnum.Locked &&
                    user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
                {
                    var remainingTime = user.LockoutEnd.Value - DateTime.UtcNow;
                    context.AddFailure($"Account is locked. Try again in {remainingTime.Minutes} minutes. ");
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
    }

    private Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken)
    {
        return _userRepository.IsExistAsync(u => u.Email == email, cancellationToken);
    }

    private async Task<bool> IsEmailConfirmedAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindAsync(u => u.Email == email, cancellationToken);

        return user?.EmailConfirmed ?? false;
    }

    private async Task<bool> IsStatusDeletedAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .FindAsync(u => u.Email == command.Email, cancellationToken);

        return user?.UserStatusId != (int)UserStatusEnum.Inactive;
    }
}
