using System.Text.Json.Serialization;
using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

public class UpdateWorkProjectStatusCommand : IRequest
{
    [JsonIgnore]
    public required Guid Id { get; set; }

    public required int ProjectStatusId { get; set; }
}
