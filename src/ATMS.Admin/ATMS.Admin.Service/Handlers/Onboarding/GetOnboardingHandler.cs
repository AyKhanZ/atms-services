using ATMS.Admin.Contracts.Models.Onboarding;
using ATMS.Admin.Contracts.Requests.Onboarding;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Onboarding;

public sealed class GetOnboardingHandler(
    ICurrentUser currentUser,
    IOnboardingRepository onboardingRepository,
    IMapper mapper) : IRequestHandler<GetOnboardingRequest, OnboardingModel>
{
    public async Task<OnboardingModel> Handle(GetOnboardingRequest request, CancellationToken cancellationToken)
    {
        var progress = await onboardingRepository.GetAsync(currentUser.Id, cancellationToken)
            ?? throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);

        return mapper.Map<OnboardingModel>(progress);
    }
}
