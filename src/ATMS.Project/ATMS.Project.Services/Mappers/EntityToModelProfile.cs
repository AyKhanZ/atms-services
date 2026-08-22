using ATMS.Application.Models;
using ATMS.Application.Localization;
using ATMS.Project.Contracts.Models.Organizations;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Models.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Entities.Dictionaries;
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

        CreateMap<User, AuditUserModel>();

        CreateMap<ProjectType, DictionaryModel>()
            .ForMember(
                x => x.Name,
                expression => expression.MapFrom(x => x.Translations.Resolve(CultureHelper.CurrentLanguage, x.Code)));

        CreateMap<ProjectKind, DictionaryModel>()
            .ForMember(
                x => x.Name,
                expression => expression.MapFrom(x => x.Translations.Resolve(CultureHelper.CurrentLanguage, x.Code)));

        CreateMap<ProjectStatus, DictionaryModel>()
            .ForMember(
                x => x.Name,
                expression => expression.MapFrom(x => x.Translations.Resolve(CultureHelper.CurrentLanguage, x.Code)));

        CreateMap<Role, WorkProjectRoleModel>();

        CreateMap<Role, DictionaryModel<Guid>>()
            .ForMember(x => x.Code, expression => expression.MapFrom(x => x.Name));

        CreateMap<WorkProjectParticipant, WorkProjectParticipantModel>()
            .ForMember(x => x.Name, expression => expression.MapFrom(x => x.User.Name))
            .ForMember(x => x.Surname, expression => expression.MapFrom(x => x.User.Surname))
            .ForMember(x => x.Email, expression => expression.MapFrom(x => x.User.Email))
            .ForMember(x => x.AvatarPath, expression => expression.MapFrom(x => x.User.AvatarPath))
            .ForMember(
                x => x.Category,
                expression => expression.MapFrom(x => x.User.OrganizationId.HasValue ? "client" : "staff"))
            .ForMember(
                x => x.Role,
                expression => expression.MapFrom(x => x.WorkProjectParticipantRoles.Single().Role));
        
        CreateMap<WorkProject, WorkProjectModel>()
            .ForMember(
                x => x.Participants,
                expression => expression.MapFrom(x => x.WorkProjectParticipants));
        
        CreateMap<WorkProject, WorkProjectItemModel>();
    }

}
