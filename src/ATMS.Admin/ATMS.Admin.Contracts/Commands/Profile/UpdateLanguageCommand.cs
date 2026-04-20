using MediatR;
using Newtonsoft.Json;

namespace ATMS.Admin.Contracts.Commands.Profile;

public class UpdateLanguageCommand : IRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }
    
    public required string Language { get; set; }
}