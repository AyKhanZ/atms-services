using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Contracts.Requests.Attachments;
using ATMS.Project.Data.Criteria.Attachments;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ATMS.Project.Services.Handlers.Attachments;

// Not paged: a task holds at most 100 files and a ticket rarely more than a few hundred, and the
// client draws a tree from the whole list. The cap keeps a runaway ticket from loading everything.
public class GetAttachmentsHandler(
    IAttachmentRepository attachmentRepository,
    IMapper mapper) : IRequestHandler<GetAttachmentsRequest, AttachmentListModel>
{
    private const int MaxItems = 1000;

    public async Task<AttachmentListModel> Handle(GetAttachmentsRequest request, CancellationToken cancellationToken)
    {
        var scopes = new[] { request.WorkTaskId, request.ParentWorkTaskId, request.WorkTicketId }
            .Count(id => id.HasValue);

        if (scopes != 1)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(GetAttachmentsRequest.WorkTaskId), AttachmentMessages.FilterRequired)
            ]);
        }

        var ownerTasks = mapper.Map<AttachmentOwnerTasksFilter>(request).And(new WorkTasksOfLiveWorkCriteria());
        var items = await attachmentRepository.GetManyAsync(ownerTasks, MaxItems + 1, cancellationToken);

        return new AttachmentListModel
        {
            Items = items.Take(MaxItems).Select(mapper.Map<AttachmentModel>).ToArray(),
            HasMore = items.Length > MaxItems
        };
    }
}
