using ATMS.Admin.Contracts.Commands.UserProgresses;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.UserProgresses;

public class SubmitUserProgressValidator : AbstractValidator<SubmitUserProgressCommand>
{
    private readonly IUserProgressRepository _userProgressRepository;
    private readonly ICurrentUser _currentUser;

    public SubmitUserProgressValidator(
        IUserProgressRepository userProgressRepository,
        IUserRepository userRepository,
        ICurrentUser currentUser)
    {
        _currentUser = currentUser;
        _userProgressRepository = userProgressRepository;
        
        RuleFor(c => c)
            .CustomAsync(async (_, _, cancellationToken) =>
            {
                var user = await userRepository.GetAsync(currentUser.Id, cancellationToken);
                if (user is null)
                {
                    throw new AuthException(AuthErrorType.InvalidToken, AuthMessages.InvalidToken);
                }
                
                if (user.HasCompletedSurvey)
                {
                    throw new AuthException(AuthErrorType.AccountInactive, AuthMessages.UserProgressAlreadyCompleted);
                }
            });
        
        RuleFor(c => c)
            .MustAsync(IsAllStepsCompletedAsync).WithMessage(AuthMessages.UserProgressNotAllStepsCompleted);
    }

    private async Task<bool> IsAllStepsCompletedAsync(SubmitUserProgressCommand command,
        CancellationToken cancellationToken)
    {
        var progress = await _userProgressRepository.FindAsync(p => p.UserId == _currentUser.Id, cancellationToken)
                       ?? throw new AuthException(AuthErrorType.InvalidToken, AuthMessages.InvalidToken);

        var requiredSteps = progress.UserProgressType == UserProgressTypeEnum.ClientManager ? 3 : 2;

        return progress.CurrentStep == requiredSteps;
    }
}