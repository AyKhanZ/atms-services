using ATMS.Application.Models;
using ATMS.Application.Localization;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Organizations;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Models.WorkProjects;
using ATMS.Project.Contracts.Models.WorkGroups;
using ATMS.Project.Contracts.Models.WorkItems;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Models.WorkTickets;
using ATMS.Project.Contracts.Models.Search;
using ATMS.Project.Data.Models.Search;
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

        CreateMap<GlobalSearchRow, GlobalSearchItemModel>()
            .ForMember(x => x.Project, expression => expression.MapFrom(row => new DictionaryModel<Guid>
            {
                Id = row.ProjectId,
                Code = row.ProjectCode,
                Name = row.ProjectTitle
            }))
            .ForMember(x => x.Status, expression => expression.MapFrom(row => new DictionaryModel
            {
                Id = row.StatusId,
                Code = row.StatusCode,
                Name = row.StatusName
            }))
            .ForMember(x => x.Assignee, expression => expression.MapFrom(row =>
                row.AssigneeId.HasValue && row.AssigneeName != null && row.AssigneeSurname != null && row.AssigneeEmail != null
                    ? new UserModel
                    {
                        Id = row.AssigneeId.Value,
                        Name = row.AssigneeName,
                        Surname = row.AssigneeSurname,
                        Email = row.AssigneeEmail,
                        AvatarPath = row.AssigneeAvatarPath
                    }
                    : null))
            .ForMember(x => x.Group, expression => expression.MapFrom(row =>
                row.GroupId.HasValue && row.GroupTitle != null
                    ? new GlobalSearchLocationModel { Id = row.GroupId.Value, Name = row.GroupTitle }
                    : null))
            .ForMember(x => x.Milestone, expression => expression.MapFrom(row =>
                row.MilestoneId.HasValue && row.MilestoneTitle != null
                    ? new GlobalSearchLocationModel { Id = row.MilestoneId.Value, Name = row.MilestoneTitle }
                    : null))
            .ForMember(x => x.Ticket, expression => expression.MapFrom(row =>
                row.TicketId.HasValue && row.TicketCode != null && row.TicketTitle != null
                    ? new DictionaryModel<Guid>
                    {
                        Id = row.TicketId.Value,
                        Code = row.TicketCode,
                        Name = row.TicketTitle
                    }
                    : null))
            .ForMember(x => x.ParentTask, expression => expression.MapFrom(row =>
                row.ParentTaskId.HasValue && row.ParentTaskCode != null && row.ParentTaskTitle != null
                    ? new DictionaryModel<Guid>
                    {
                        Id = row.ParentTaskId.Value,
                        Code = row.ParentTaskCode,
                        Name = row.ParentTaskTitle
                    }
                    : null));

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

        CreateMap<WorkGroupStatus, DictionaryModel>()
            .ForMember(
                x => x.Name,
                expression => expression.MapFrom(x => x.Translations.Resolve(CultureHelper.CurrentLanguage, x.Code)));

        CreateMap<WorkTicketType, DictionaryModel>()
            .ForMember(
                x => x.Name,
                expression => expression.MapFrom(x => x.Translations.Resolve(CultureHelper.CurrentLanguage, x.Code)));

        CreateMap<WorkTicketStatus, DictionaryModel>()
            .ForMember(
                x => x.Name,
                expression => expression.MapFrom(x => x.Translations.Resolve(CultureHelper.CurrentLanguage, x.Code)));

        CreateMap<WorkItemPriority, DictionaryModel>()
            .ForMember(
                x => x.Name,
                expression => expression.MapFrom(x => x.Translations.Resolve(CultureHelper.CurrentLanguage, x.Code)));

        CreateMap<WorkProjectParticipant, WorkItemAssigneeModel>()
            .ForMember(x => x.Name, expression => expression.MapFrom(x => x.User.Name))
            .ForMember(x => x.Surname, expression => expression.MapFrom(x => x.User.Surname))
            .ForMember(x => x.AvatarPath, expression => expression.MapFrom(x => x.User.AvatarPath));

        CreateMap<WorkTicket, WorkTicketModel>()
            .ForMember(x => x.MilestoneId, expression => expression.MapFrom(x => x.WorkGroupId))
            .ForMember(x => x.MilestoneTitle, expression => expression.MapFrom(x => x.WorkGroup.Title))
            .ForMember(
                x => x.GroupId,
                expression => expression.MapFrom(x => x.WorkGroup.ParentWorkGroupId.Value))
            .ForMember(
                x => x.GroupTitle,
                expression => expression.MapFrom(x =>
                    x.WorkGroup.ParentWorkGroup == null ? null : x.WorkGroup.ParentWorkGroup.Title));

        CreateMap<WorkTaskStatus, DictionaryModel>()
            .ForMember(
                x => x.Name,
                expression => expression.MapFrom(x => x.Translations.Resolve(CultureHelper.CurrentLanguage, x.Code)));

        CreateMap<WorkTask, WorkTaskModel>()
            .ForMember(x => x.WorkTicketCode, expression => expression.MapFrom(x => x.WorkTicket.Code))
            .ForMember(x => x.WorkTicketTitle, expression => expression.MapFrom(x => x.WorkTicket.Title))
            .ForMember(x => x.MilestoneId, expression => expression.MapFrom(x => x.WorkTicket.WorkGroupId))
            .ForMember(x => x.MilestoneTitle, expression => expression.MapFrom(x => x.WorkTicket.WorkGroup.Title))
            .ForMember(
                x => x.GroupId,
                expression => expression.MapFrom(x => x.WorkTicket.WorkGroup.ParentWorkGroupId.Value))
            .ForMember(
                x => x.GroupTitle,
                expression => expression.MapFrom(x => x.WorkTicket.WorkGroup.ParentWorkGroup.Title))
            .ForMember(x => x.ParentWorkTaskCode, expression => expression.MapFrom(x => x.ParentWorkTask == null ? null : x.ParentWorkTask.Code))
            .ForMember(x => x.ParentWorkTaskTitle, expression => expression.MapFrom(x => x.ParentWorkTask == null ? null : x.ParentWorkTask.Title));

        CreateMap<WorkGroup, WorkGroupModel>()
            .ForMember(
                x => x.Milestones,
                expression => expression.MapFrom(x => x.Children));

        CreateMap<WorkGroup, MilestoneOptionModel>()
            .ForMember(
                x => x.GroupId,
                expression => expression.MapFrom(x => x.ParentWorkGroupId.Value))
            .ForMember(
                x => x.GroupTitle,
                expression => expression.MapFrom(x =>
                    x.ParentWorkGroup == null ? null : x.ParentWorkGroup.Title));

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
                expression => expression.MapFrom(x =>
                    x.User.UserType == (int)UserTypeEnum.Employee
                        ? "staff"
                        : x.User.UserType == (int)UserTypeEnum.Client
                            ? "client"
                            : "admin"))
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
