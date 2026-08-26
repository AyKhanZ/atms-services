using System.Text.Json.Serialization;
using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;

namespace ATMS.Admin.Contracts.Commands.Users;

[Access(PermissionEnum.UserDelete)]
public class UpdateUserStatusCommand : IRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public required int UserStatusId { get; init; }
}
