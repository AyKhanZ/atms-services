using MediatR;
using Newtonsoft.Json;

namespace ATMS.Admin.Contracts.Commands.Profile;

public class UpdatePhotoCommand : IRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public required string FileName { get; set; }
}