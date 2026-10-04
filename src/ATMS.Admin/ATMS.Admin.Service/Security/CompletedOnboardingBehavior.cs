using ATMS.Admin.Contracts.Security;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Application.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Security;

public sealed class CompletedOnboardingBehavior<TRequest, TResponse>(
    ICurrentUser currentUser) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly bool RequiresCompletedOnboarding = typeof(TRequest)
        .IsDefined(typeof(CompletedOnboardingAccessAttribute), inherit: false);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (RequiresCompletedOnboarding && !currentUser.HasCompletedOnboarding)
        {
            throw new ConflictException(OnboardingMessages.OnboardingNotCompleted);
        }

        return await next(cancellationToken);
    }
}
