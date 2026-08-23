using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkGroups;

public class DeleteWorkGroupCommand : IRequest
{
    public Guid ProjectId { get; set; }

    public Guid WorkGroupId { get; set; }
}
