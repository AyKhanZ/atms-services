using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Authentication;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    private readonly IUserRepository _userRepository;

    public RefreshTokenValidator(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
        
        RuleFor(x => x.RefreshToken).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Token is required .")
            .MustAsync(IsRefreshTokenExistAsync)
            .WithMessage("User with such refresh token doesn't exist .")
            .MustAsync(IsRefreshTokenExtendedAsync)
            .WithMessage("Refresh token lifetime exceeded. Please log in again.");
    }

    private async Task<bool> IsRefreshTokenExistAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindAsync(u => u.RefreshToken == refreshToken, cancellationToken);
        return user is not null;
    }

    private async Task<bool> IsRefreshTokenExtendedAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindAsync(u => u.RefreshToken == refreshToken, cancellationToken);

        return user?.RefreshTokenExpiresAt > DateTime.UtcNow;
    }
}
