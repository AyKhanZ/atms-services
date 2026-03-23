using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Authentication;

public class LogoutValidator : AbstractValidator<LogoutCommand>
{
    private readonly IUserRepository _userRepository;
    
    public LogoutValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        
        RuleFor(x => x.RefreshToken).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("RefreshToken is required .")
            .MustAsync(IsRefreshTokenExistAsync)
            .WithMessage("User with such refresh token doesn't exist .");
    }
    
    private async Task<bool> IsRefreshTokenExistAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindAsync(u => u.RefreshToken == refreshToken, cancellationToken);
        return user is not null;
    }
}