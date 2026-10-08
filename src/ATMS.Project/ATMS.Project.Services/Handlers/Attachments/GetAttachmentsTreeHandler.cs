using ATMS.Application.Models;
using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Contracts.Requests.Attachments;
using ATMS.Project.Data.Criteria.Attachments;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Attachments;

// only the tree levels with file counts, files are loaded when a branch is opened
public sealed class GetAttachmentsTreeHandler(
    IAttachmentRepository attachmentRepository) : IRequestHandler<GetAttachmentsTreeRequest, AttachmentTreeModel>
{
    public async Task<AttachmentTreeModel> Handle(GetAttachmentsTreeRequest request, CancellationToken cancellationToken)
    {
        var ownerTasks = new AttachmentOwnerTasksFilter { ProjectId = request.ProjectId }
            .And(new WorkTasksOfLiveWorkCriteria());
        var tickets = await attachmentRepository.GetTicketCountsAsync(ownerTasks, cancellationToken);

        var groups = tickets
            .GroupBy(ticket => ticket.GroupId)
            .OrderBy(group => group.First().GroupCreatedAt)
            .Select(group => new AttachmentTreeGroupModel
            {
                Id = group.Key,
                Title = group.First().GroupTitle,
                FileCount = group.Sum(ticket => ticket.FileCount),
                Milestones = group
                    .GroupBy(ticket => ticket.MilestoneId)
                    .OrderBy(milestone => milestone.First().MilestoneCreatedAt)
                    .Select(milestone => new AttachmentTreeMilestoneModel
                    {
                        Id = milestone.Key,
                        Title = milestone.First().MilestoneTitle,
                        FileCount = milestone.Sum(ticket => ticket.FileCount),
                        Tickets = milestone
                            .OrderBy(ticket => ticket.TicketCreatedAt)
                            .Select(ticket => new AttachmentTreeTicketModel
                            {
                                WorkTicket = new DictionaryModel<Guid>
                                {
                                    Id = ticket.TicketId,
                                    Code = ticket.TicketCode,
                                    Name = ticket.TicketTitle
                                },
                                FileCount = ticket.FileCount
                            })
                            .ToArray()
                    })
                    .ToArray()
            })
            .ToArray();

        return new AttachmentTreeModel
        {
            FileCount = groups.Sum(group => group.FileCount),
            Groups = groups
        };
    }
}
