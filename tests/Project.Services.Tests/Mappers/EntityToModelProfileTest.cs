using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Search;
using ATMS.Project.Data.Models.Search;
using ATMS.Project.Services.Modules;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Project.Services.Tests.Mappers;

public class EntityToModelProfileTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MapGlobalSearchRow_UsesNestedModelsAndPreservesOptionalRelationships(bool hasParents)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMapperServices();
        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        var row = new GlobalSearchRow
        {
            ItemType = hasParents ? GlobalSearchItemType.Subtask : GlobalSearchItemType.Project,
            Id = Guid.NewGuid(),
            Code = "123",
            Title = "Result",
            ProjectId = Guid.NewGuid(),
            ProjectCode = "10",
            ProjectTitle = "Project",
            StatusId = 1,
            StatusCode = "New",
            StatusName = "Новая"
        };
        if (hasParents)
        {
            row.GroupId = Guid.NewGuid();
            row.GroupTitle = "Group";
            row.MilestoneId = Guid.NewGuid();
            row.MilestoneTitle = "Milestone";
            row.TicketId = Guid.NewGuid();
            row.TicketCode = "120";
            row.TicketTitle = "Ticket";
            row.ParentTaskId = Guid.NewGuid();
            row.ParentTaskCode = "122";
            row.ParentTaskTitle = "Parent";
            row.AssigneeId = Guid.NewGuid();
            row.AssigneeName = "Search";
            row.AssigneeSurname = "Tester";
            row.AssigneeEmail = "search@test.invalid";
            row.AssigneeAvatarPath = "avatar.png";
        }

        var result = mapper.Map<GlobalSearchItemModel>(row);

        Assert.Equal((int)row.ItemType, result.ItemType);
        Assert.Equal(row.ProjectId, result.Project.Id);
        Assert.Equal(row.ProjectCode, result.Project.Code);
        Assert.Equal(row.ProjectTitle, result.Project.Name);
        Assert.Equal(row.StatusId, result.Status.Id);
        Assert.Equal(row.StatusCode, result.Status.Code);
        Assert.Equal(row.StatusName, result.Status.Name);
        if (hasParents)
        {
            Assert.NotNull(result.Assignee);
            Assert.Equal(row.AssigneeId, result.Assignee.Id);
            Assert.Equal(row.AssigneeName, result.Assignee.Name);
            Assert.Equal(row.AssigneeSurname, result.Assignee.Surname);
            Assert.Equal(row.AssigneeEmail, result.Assignee.Email);
            Assert.Equal(row.AssigneeAvatarPath, result.Assignee.AvatarPath);
            Assert.NotNull(result.Group);
            Assert.Equal(row.GroupId, result.Group.Id);
            Assert.Equal(row.GroupTitle, result.Group.Name);
            Assert.NotNull(result.Milestone);
            Assert.Equal(row.MilestoneId, result.Milestone.Id);
            Assert.Equal(row.MilestoneTitle, result.Milestone.Name);
            Assert.NotNull(result.Ticket);
            Assert.Equal(row.TicketId, result.Ticket.Id);
            Assert.Equal(row.TicketCode, result.Ticket.Code);
            Assert.Equal(row.TicketTitle, result.Ticket.Name);
            Assert.NotNull(result.ParentTask);
            Assert.Equal(row.ParentTaskId, result.ParentTask.Id);
            Assert.Equal(row.ParentTaskCode, result.ParentTask.Code);
            Assert.Equal(row.ParentTaskTitle, result.ParentTask.Name);
        }
        else
        {
            Assert.Null(result.Assignee);
            Assert.Null(result.Group);
            Assert.Null(result.Milestone);
            Assert.Null(result.Ticket);
            Assert.Null(result.ParentTask);
        }
    }
}
