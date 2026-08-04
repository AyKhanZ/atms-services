using ATMS.Application.Models;
using ATMS.Data.Constants;
using ATMS.Project.Contracts.Models.Organizations;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Models.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Resources;
using AutoMapper;

namespace ATMS.Project.Services.Mappers;

public class EntityToModelProfile : Profile
{
    public EntityToModelProfile()
    {
        CreateMap<Organization, OrganizationModel>();
        
        CreateMap<Organization, OrganizationItemModel>();
        
        
        CreateMap<User, UserModel>();

        
        CreateMap<Organization, WorkProjectOrganizationModel>();

        CreateMap<Role, WorkProjectRoleModel>()
            .ForMember(x => x.Name, expression => expression.MapFrom(x => GetProjectRoleName(x)));

        CreateMap<Role, DictionaryModel<Guid>>()
            .ForMember(x => x.Name, expression => expression.MapFrom(x => GetProjectRoleName(x)))
            .ForMember(x => x.Code, expression => expression.MapFrom(x => x.Name));

        CreateMap<WorkProjectParticipant, WorkProjectParticipantModel>()
            .ForMember(x => x.Name, expression => expression.MapFrom(x => x.User.Name))
            .ForMember(x => x.Surname, expression => expression.MapFrom(x => x.User.Surname))
            .ForMember(x => x.Email, expression => expression.MapFrom(x => x.User.Email))
            .ForMember(
                x => x.Role,
                expression => expression.MapFrom(x => x.WorkProjectParticipantRoles.Single().Role));
        
        CreateMap<WorkProject, WorkProjectModel>()
            .ForMember(
                x => x.Participants,
                expression => expression.MapFrom(x => x.WorkProjectParticipants));
        
        CreateMap<WorkProject, WorkProjectItemModel>();
    }

    private string GetProjectRoleName(Role role)
    {
        return role.Id switch
        {
            var id when id == RoleIds.ProjectManager => WorkProjectMessages.ProjectManager,
            var id when id == RoleIds.BusinessConsultant => WorkProjectMessages.BusinessConsultant,
            var id when id == RoleIds.Developer => WorkProjectMessages.Developer,
            var id when id == RoleIds.OrgClientManager => WorkProjectMessages.OrgClientManager,
            var id when id == RoleIds.OrgClientViewer => WorkProjectMessages.OrgClientViewer,
            _ => role.Name
        };
    }
}
