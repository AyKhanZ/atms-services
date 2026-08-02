using ATMS.Admin.Contracts.Models.Onboarding;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Onboarding;

public class GetOnboardingRequest : IRequest<OnboardingModel>;
