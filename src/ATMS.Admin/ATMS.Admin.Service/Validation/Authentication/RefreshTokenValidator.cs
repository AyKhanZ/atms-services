using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Interfaces;
using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace ATMS.Admin.Service.Validation.Authentication;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    private readonly IUserRepository userRepository;
    private readonly JwtOptions _jwtOptions;

    public RefreshTokenValidator(IUserRepository userRepository, IConfiguration configuration)
    {
        RuleFor(x => x.RefreshToken).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Token is required .")
            .MustAsync(IsRefreshTokenExistAsync)
            .WithMessage("User with such refresh token doesn't exist .")
            .MustAsync(IsRefreshTokenExtendedAsync)
            .WithMessage("Refresh token lifetime exceeded. Please log in again.")
            .MustAsync(IsRefreshTokenWithinMaxLifetimeAsync)
            .WithMessage("Refresh token exceeded its maximum allowed lifetime, please log in again.");

        this.userRepository = userRepository;
        _jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()
                                             ?? throw new ConfigurationException(ConfigurationErrorType.JwtSectionNotFound,
                                                 $"Configuration for section '{nameof(JwtOptions)}' is not found or could not be loaded.");
    }

    private async Task<bool> IsRefreshTokenExistAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(u => u.RefreshToken == refreshToken, cancellationToken);
        return user is not null;
    }

    private async Task<bool> IsRefreshTokenExtendedAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(u => u.RefreshToken == refreshToken, cancellationToken);

        return user?.RefreshTokenExpiryTime > DateTime.UtcNow;
    }

    private async Task<bool> IsRefreshTokenWithinMaxLifetimeAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(u => u.RefreshToken == refreshToken, cancellationToken);

        return user?.RefreshTokenCreatedAt.AddDays(_jwtOptions.MaxRefreshTokenLifetimeExpirationInDays) > DateTime.UtcNow;
    }
}
