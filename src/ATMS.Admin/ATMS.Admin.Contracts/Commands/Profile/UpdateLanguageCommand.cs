using MediatR;
using ATMS.Application.Security;
using ATMS.Admin.Contracts.Security;

namespace ATMS.Admin.Contracts.Commands.Profile;

[ExceptSuperAdminAccess]
[CompletedOnboardingAccess]
public class UpdateLanguageCommand : IRequest
{
    public required string Language { get; set; }
}
