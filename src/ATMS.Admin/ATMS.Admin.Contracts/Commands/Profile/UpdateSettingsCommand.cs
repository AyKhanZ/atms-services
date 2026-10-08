using ATMS.Admin.Contracts.Models.Profile;
using ATMS.Application.Security;
using ATMS.Admin.Contracts.Security;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ATMS.Admin.Contracts.Commands.Profile;

[ExceptSuperAdminAccess]
[CompletedOnboardingAccess]
public sealed class UpdateSettingsCommand : IRequest<ProfileModel>
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required int GenderId { get; set; }
    public required int MaritalStatusId { get; set; }
    public required string Position { get; set; }
    public required string PhoneNumber { get; set; }
    public DateOnly BirthDate { get; set; }
    public int LanguageId { get; set; }
    public IFormFile? Avatar { get; set; }
}
