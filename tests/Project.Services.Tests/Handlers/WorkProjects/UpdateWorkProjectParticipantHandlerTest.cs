using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.WorkProjects;
using ATMS.Project.Services.Security.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Project.Services.Tests.Handlers.WorkProjects;

public sealed class UpdateWorkProjectParticipantHandlerTest
{
    [Fact]
    public async Task Handle_TracksReplacementRoleAsAdded()
    {
        using var context = CreateContext();
        var project = CreateProject();
        var participant = project.WorkProjectParticipants.Single();
        var currentRole = participant.WorkProjectParticipantRoles.Single();
        var replacementRoleId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        context.Attach(project);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.Id).Returns(currentUserId);

        var repository = new Mock<IWorkProjectRepository>();
        repository
            .Setup(x => x.FindAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        repository
            .Setup(x => x.Touch(project))
            .Callback(() => context.Entry(project).State = EntityState.Modified);
        repository
            .Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
            .Callback(() => context.ChangeTracker.DetectChanges())
            .Returns(Task.CompletedTask);

        var cache = new Mock<ICacheService>();
        cache
            .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var projectPermissionService = new Mock<IProjectPermissionService>();
        projectPermissionService
            .Setup(x => x.RemoveUserPermissionsAsync(
                project.Id,
                participant.UserId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateWorkProjectParticipantHandler(
            currentUser.Object,
            repository.Object,
            cache.Object,
            projectPermissionService.Object);

        await handler.Handle(new UpdateWorkProjectParticipantCommand
        {
            ProjectId = project.Id,
            ParticipantId = participant.Id,
            RoleId = replacementRoleId
        }, CancellationToken.None);

        var replacementRole = participant.WorkProjectParticipantRoles.Single(x => !ReferenceEquals(x, currentRole));
        Assert.Equal(EntityState.Added, context.Entry(replacementRole).State);
        Assert.NotEqual(Guid.Empty, replacementRole.Id);
        Assert.True(currentRole.IsDeleted);
        Assert.Equal(currentUserId, currentRole.DeletedById);
    }

    private static ProjectDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=participant-role-tests;Username=test;Password=test")
            .Options;
        return new ProjectDbContext(options);
    }

    private static WorkProject CreateProject()
    {
        var project = new WorkProject
        {
            Id = Guid.NewGuid(),
            Code = "PROJECT-1",
            Title = "Project"
        };
        var participant = new WorkProjectParticipant
        {
            Id = Guid.NewGuid(),
            WorkProject = project,
            WorkProjectId = project.Id,
            UserId = Guid.NewGuid()
        };
        participant.WorkProjectParticipantRoles.Add(new WorkProjectParticipantRole
        {
            Id = Guid.NewGuid(),
            WorkProjectParticipant = participant,
            WorkProjectParticipantId = participant.Id,
            RoleId = Guid.NewGuid()
        });
        project.WorkProjectParticipants.Add(participant);
        return project;
    }
}
