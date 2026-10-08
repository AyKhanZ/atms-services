using ATMS.Admin.Contracts.Models;
using ATMS.Application.Security;
using ATMS.Admin.Contracts.Security;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Account;

[ExceptSuperAdminAccess]
[CompletedOnboardingAccess]
public sealed class ChangePasswordCommand : IRequest<AccessInfoModel>
{
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}
