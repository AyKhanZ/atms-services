using System.Text.Json.Serialization;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Users;

public class UpdateUserStatusCommand : IRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public required int UserStatusId { get; init; }
}