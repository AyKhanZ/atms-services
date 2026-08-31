using ATMS.Project.Contracts.Commands.Organizations;
using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using AutoMapper;

namespace ATMS.Project.Services.Mappers;

public class CommandToEntityProfile : Profile
{
    public CommandToEntityProfile()
    {
        CreateMap<CreateOrganizationCommand, Organization>();
        CreateMap<UpdateOrganizationCommand, Organization>();

        CreateMap<CreateWorkProjectCommand, WorkProject>();
        CreateMap<UpdateWorkProjectCommand, WorkProject>();

        CreateMap<CreateWorkTicketCommand, WorkTicket>()
            .ForMember(destination => destination.WorkProjectId,
                options => options.MapFrom(source => source.ProjectId))
            .ForMember(destination => destination.WorkGroupId,
                options => options.MapFrom(source => source.MilestoneId));

        CreateMap<UpdateWorkTicketCommand, WorkTicket>()
            .ForMember(destination => destination.WorkGroupId,
                options => options.MapFrom(source => source.MilestoneId));
    }
}
