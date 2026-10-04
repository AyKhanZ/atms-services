using ATMS.Admin.Contracts.Models.Profile;
using ATMS.Application.Security;
using ATMS.Admin.Contracts.Security;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Profile;

[ExceptSuperAdminAccess]
[CompletedOnboardingAccess]
public sealed class GetProfileRequest : IRequest<ProfileModel>;
