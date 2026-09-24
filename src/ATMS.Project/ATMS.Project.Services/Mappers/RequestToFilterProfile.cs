using ATMS.Project.Contracts.Requests.Attachments;
using ATMS.Project.Data.Criteria.Attachments;
using ATMS.Project.Contracts.Requests.Organizations;
using ATMS.Project.Contracts.Requests.WorkProjects;
using ATMS.Project.Contracts.Requests.WorkTaskBoard;
using ATMS.Project.Data.Criteria.Organizations;
using ATMS.Project.Data.Criteria.WorkProjectParticipants;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.Criteria.WorkTasks;
using AutoMapper;

namespace ATMS.Project.Services.Mappers;

public class RequestToFilterProfile : Profile
{
    public RequestToFilterProfile()
    {
        CreateMap<GetWorkTaskBoardRequest, WorkTaskBoardFilter>();

        CreateMap<GetWorkTaskBoardCountsRequest, WorkTaskBoardFilter>();

        CreateMap<GetWorkTaskBoardAssigneesRequest, WorkTaskBoardAssigneesFilter>();

        CreateMap<GetOrganizationsRequest, OrganizationFilter>();
        CreateMap<GetWorkProjectsRequest, WorkProjectsFilter>();

        CreateMap<GetAttachmentsRequest, AttachmentOwnerTasksFilter>();
    }
}
