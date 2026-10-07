using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Constants;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Enums;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.WorkProjects;
using ATMS.Project.Services.Notifications.Interfaces;
using ATMS.Project.Services.Security.Interfaces;
using ATMS.Project.Services.Validation.WorkProjects;
using FluentValidation;
using Moq;

namespace Project.Services.Tests.Handlers.WorkProjects;

public class AddWorkProjectParticipantHandlerTest
{
    private readonly Mock<IWorkProjectRepository> workProjectRepository = new();
    private readonly Mock<ICacheService> cache = new();
    private readonly Mock<IProjectPermissionService> projectPermissionService = new();
    private readonly Mock<IWorkProjectNotificationService> notifications = new();

    [Fact]
    public async Task Handle_WhenClientInvitePermissionMatchesTarget_AddsParticipant()
    {
        var command = CreateCommand(RoleIds.OrgClientViewer);
        var project = new WorkProject { Id = command.ProjectId };
        workProjectRepository
            .Setup(repository => repository.FindAsync(command.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        SetupSave(null);

        await CreateHandler().Handle(command, CancellationToken.None);

        var participant = Assert.Single(project.WorkProjectParticipants);
        Assert.Equal(command.UserId, participant.UserId);
        Assert.Equal(command.RoleId, Assert.Single(participant.WorkProjectParticipantRoles).RoleId);
        workProjectRepository.Verify(
            repository => repository.SaveParticipantWithinLimitAsync(
                project.Id,
                command.UserId,
                WorkProjectParticipantLimit.Max,
                It.IsAny<CancellationToken>()),
            Times.Once);
        foreach (var language in SupportedLanguages.All)
        {
            cache.Verify(
                service => service.RemoveAsync(
                    CacheKeys.Project.ProjectById(project.Id, language),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task Handle_TellsTheNewParticipantInTheSameSave()
    {
        var command = CreateCommand(RoleIds.Developer);
        var project = new WorkProject { Id = command.ProjectId };
        var steps = new List<string>();
        workProjectRepository
            .Setup(repository => repository.FindAsync(command.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        notifications
            .Setup(service => service.NotifyParticipantsAddedAsync(
                project,
                It.Is<IEnumerable<Guid>>(userIds => userIds.SequenceEqual(new[] { command.UserId })),
                It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("notify"))
            .Returns(Task.CompletedTask);
        workProjectRepository
            .Setup(repository => repository.SaveParticipantWithinLimitAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("save"))
            .ReturnsAsync((WorkProjectParticipantRefusal?)null);

        await CreateHandler().Handle(command, CancellationToken.None);

        Assert.Equal(["notify", "save"], steps);
    }

    // An invitation or another add took the place, or the same person was added, after validation.
    [Theory]
    [InlineData(WorkProjectParticipantRefusal.AlreadyParticipant)]
    [InlineData(WorkProjectParticipantRefusal.LimitReached)]
    public async Task Handle_WhenRefusedUnderTheLock_FailsOnUserAndKeepsCaches(WorkProjectParticipantRefusal refusal)
    {
        var command = CreateCommand(RoleIds.OrgClientViewer);
        workProjectRepository
            .Setup(repository => repository.FindAsync(command.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkProject { Id = command.ProjectId });
        SetupSave(refusal);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => CreateHandler().Handle(command, CancellationToken.None));

        Assert.Equal(nameof(command.UserId), Assert.Single(exception.Errors).PropertyName);
        cache.Verify(service => service.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        projectPermissionService.Verify(
            service => service.RemoveUserPermissionsAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupSave(WorkProjectParticipantRefusal? refusal)
    {
        workProjectRepository
            .Setup(repository => repository.SaveParticipantWithinLimitAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refusal);
    }

    private AddWorkProjectParticipantHandler CreateHandler() => new(
        workProjectRepository.Object,
        cache.Object,
        projectPermissionService.Object,
        notifications.Object);

    private static AddWorkProjectParticipantCommand CreateCommand(Guid roleId) => new()
    {
        ProjectId = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        RoleId = roleId
    };
}
