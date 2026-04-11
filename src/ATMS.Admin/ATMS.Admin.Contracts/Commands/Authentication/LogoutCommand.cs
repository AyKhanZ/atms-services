using System.Text.Json.Serialization;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Authentication;

public class LogoutCommand : IRequest
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public required string RefreshToken { get; init; }
}
