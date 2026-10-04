using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Notifications;
using ATMS.Project.Contracts.Requests.Notifications;
using ATMS.Project.Data.Criteria.Notifications;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.History;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Services.Modules;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Project.Services.Tests.Mappers;

public sealed class NotificationMapperTest
{
    private static IMapper Mapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMapperServices();
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    [Fact]
    public void MapRow_CarriesTypeActorTargetAndParameters()
    {
        var row = new NotificationRow
        {
            Id = Guid.NewGuid(),
            Type = (int)NotificationTypeEnum.TaskStatusChanged,
            CreatedAt = new DateTime(2026, 10, 2, 9, 0, 0, DateTimeKind.Utc),
            ReadAt = new DateTime(2026, 10, 2, 10, 0, 0, DateTimeKind.Utc),
            Actor = new HistoryPerson(Guid.NewGuid(), "Leyla", "Mammadova", "users/leyla.png"),
            WorkProjectId = Guid.NewGuid(),
            EntityType = (int)NotificationEntityTypeEnum.WorkTask,
            EntityId = Guid.NewGuid(),
            WorkTicketId = Guid.NewGuid(),
            CommentId = Guid.NewGuid(),
            Parameters = new NotificationParameters
            {
                ProjectTitle = "Project Alpha",
                TaskCode = "41",
                TaskTitle = "Payment form",
                TaskKind = (int)WorkTaskKindEnum.Subtask,
                FromStatusId = 1,
                ToStatusId = 3,
                Deadline = new DateOnly(2026, 10, 5)
            },
            EntityDeleted = true,
            CommentDeleted = true
        };

        var model = Mapper().Map<NotificationModel>(row);

        Assert.Equal(row.Id, model.Id);
        Assert.Equal((int)NotificationTypeEnum.TaskStatusChanged, model.Type);
        Assert.Equal(row.CreatedAt, model.CreatedAt);
        Assert.Equal(row.ReadAt, model.ReadAt);
        Assert.Equal(row.Actor.Id, model.Actor?.Id);
        Assert.Equal("Leyla", model.Actor?.Name);
        Assert.Equal("users/leyla.png", model.Actor?.AvatarPath);
        Assert.Equal(row.WorkProjectId, model.ProjectId);
        Assert.Equal((int)NotificationEntityTypeEnum.WorkTask, model.EntityType);
        Assert.Equal(row.EntityId, model.EntityId);
        Assert.Equal(row.WorkTicketId, model.WorkTicketId);
        Assert.Equal(row.CommentId, model.CommentId);
        Assert.Equal("Project Alpha", model.Parameters.ProjectTitle);
        Assert.Equal("41", model.Parameters.TaskCode);
        Assert.Equal("Payment form", model.Parameters.TaskTitle);
        Assert.Equal((int)WorkTaskKindEnum.Subtask, model.Parameters.TaskKind);
        Assert.Equal(1, model.Parameters.FromStatusId);
        Assert.Equal(3, model.Parameters.ToStatusId);
        Assert.Equal(new DateOnly(2026, 10, 5), model.Parameters.Deadline);
        Assert.True(model.EntityDeleted);
        Assert.True(model.CommentDeleted);
    }

    [Fact]
    public void MapRow_WithoutActor_IsFromTheSystem()
    {
        var row = new NotificationRow
        {
            Type = (int)NotificationTypeEnum.DueToday,
            Parameters = new NotificationParameters()
        };

        Assert.Null(Mapper().Map<NotificationModel>(row).Actor);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MapRequest_CarriesUnreadOnly(bool unreadOnly)
    {
        var filter = Mapper().Map<NotificationFilter>(new GetNotificationsRequest { UnreadOnly = unreadOnly });

        Assert.Equal(unreadOnly, filter.UnreadOnly);
    }
}
