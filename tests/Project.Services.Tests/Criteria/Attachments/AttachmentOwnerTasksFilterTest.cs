using ATMS.Project.Data.Criteria.Attachments;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria.Attachments;

public class AttachmentOwnerTasksFilterTest
{
    private static readonly Guid ProjectId = Guid.NewGuid();
    private static readonly Guid TicketId = Guid.NewGuid();
    private static readonly Guid OtherTicketId = Guid.NewGuid();

    private static readonly WorkTask Task = new() { Id = Guid.NewGuid(), WorkProjectId = ProjectId, WorkTicketId = TicketId };

    private static readonly WorkTask Subtask = new()
    {
        Id = Guid.NewGuid(),
        WorkProjectId = ProjectId,
        WorkTicketId = TicketId,
        ParentWorkTaskId = Task.Id
    };

    private static readonly WorkTask OtherTicketTask = new() { Id = Guid.NewGuid(), WorkProjectId = ProjectId, WorkTicketId = OtherTicketId };

    private static readonly WorkTask OtherProjectTask = new() { Id = Guid.NewGuid(), WorkProjectId = Guid.NewGuid(), WorkTicketId = TicketId };

    private static Guid[] Apply(AttachmentOwnerTasksFilter filter) =>
        filter.Apply(new[] { Task, Subtask, OtherTicketTask, OtherProjectTask }.AsQueryable())
            .Select(task => task.Id)
            .ToArray();

    [Fact]
    public void Apply_WithOnlyTheProject_KeepsEveryTaskOfTheProject()
    {
        var ids = Apply(new AttachmentOwnerTasksFilter { ProjectId = ProjectId });

        Assert.Equal([Task.Id, Subtask.Id, OtherTicketTask.Id], ids);
    }

    [Fact]
    public void Apply_WithATask_KeepsOnlyThatTask()
    {
        var ids = Apply(new AttachmentOwnerTasksFilter { ProjectId = ProjectId, WorkTaskId = Task.Id });

        Assert.Equal([Task.Id], ids);
    }

    [Fact]
    public void Apply_WithAParent_KeepsOnlyItsSubtasks()
    {
        var ids = Apply(new AttachmentOwnerTasksFilter { ProjectId = ProjectId, ParentWorkTaskId = Task.Id });

        Assert.Equal([Subtask.Id], ids);
    }

    [Fact]
    public void Apply_WithATicket_KeepsTasksAndSubtasksOfTheTicketInTheProject()
    {
        var ids = Apply(new AttachmentOwnerTasksFilter { ProjectId = ProjectId, WorkTicketId = TicketId });

        Assert.Equal([Task.Id, Subtask.Id], ids);
    }
}
