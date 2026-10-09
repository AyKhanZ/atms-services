using System.Text.Json.Serialization;
using MediatR;
using ATMS.Application.Security;

namespace ATMS.Admin.Contracts.Commands.Users;

[SuperAdminAccess]
public sealed class UpdateUserStatusCommand : IRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public required int UserStatusId { get; init; }
}
