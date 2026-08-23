using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkGroups;

public class CreateWorkGroupCommand : WorkGroupCommand, IRequest<Guid>
{
    public Guid ProjectId { get; set; }

    public Guid? ParentWorkGroupId { get; set; }
}
