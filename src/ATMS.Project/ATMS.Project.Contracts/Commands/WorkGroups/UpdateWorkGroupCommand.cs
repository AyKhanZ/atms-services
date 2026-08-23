using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkGroups;

public class UpdateWorkGroupCommand : WorkGroupCommand, IRequest
{
    public Guid ProjectId { get; set; }

    public Guid WorkGroupId { get; set; }
}
