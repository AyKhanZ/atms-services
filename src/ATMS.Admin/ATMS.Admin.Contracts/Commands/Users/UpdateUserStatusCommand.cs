using System.Text.Json.Serialization;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Users;

public class UpdateUserStatusCommand : IRequest
{
    [JsonIgnore]
    public required Guid Id { get; set; }
    public required int UserStatusId { get; init; }
}