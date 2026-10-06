using ATMS.Application.Exceptions.Entity;
using ATMS.Caching.Constants;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Data.Messaging;
using ATMS.Messaging.Configuration;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkProjects;
using Moq;

namespace Project.Services.Tests.Handlers.WorkProjects;

public class InviteWorkProjectParticipantHandlerTest : BaseHandlerTest
{
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock = new();
    private readonly Guid _inviterId = Guid.NewGuid();
    private readonly WorkProject _project = new()
    {
        Id = Guid.NewGuid(),
        Title = "Customer portal",
        OrganizationId = Guid.NewGuid()
    };

    public InviteWorkProjectParticipantHandlerTest()
    {
        CurrentUserMock.Setup(x => x.Id).Returns(_inviterId);
        WorkProjectRepositoryMock
            .Setup(x => x.FindRootAsync(_project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_project);
    }

    [Fact]
    public async Task Handle_SavesPendingClientViewerInvitation()
    {
        WorkProjectInvitation? saved = null;
        WorkProjectInvitationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<WorkProjectInvitation>(), It.IsAny<CancellationToken>()))
            .Callback<WorkProjectInvitation, CancellationToken>((invitation, _) => saved = invitation);

        await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal(_project.Id, saved.WorkProjectId);
        Assert.Equal("Nigar@Client.az", saved.Email);
        Assert.Equal("NIGAR@CLIENT.AZ", saved.NormalizedEmail);
        Assert.Equal("Nigar", saved.Name);
        Assert.Equal("Huseynova", saved.Surname);
        Assert.Equal(RoleIds.OrgClientViewer, saved.RoleId);
        Assert.Equal((int)WorkProjectInvitationStatusEnum.Pending, saved.Status);
        Assert.Equal(_inviterId, saved.InvitedById);
        Assert.Null(saved.ProcessedAt);
    }

    [Fact]
    public async Task Handle_AsksAdminForTheAccountWithProjectTitle()
    {
        await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        _outboxRepositoryMock.Verify(x => x.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserInvited,
            new UserInvitedEvent(
                "Nigar@Client.az",
                "Nigar",
                "Huseynova",
                _project.OrganizationId,
                _inviterId,
                _project.Title),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SavesInvitationAndEventTogether_ThenFlushesProjectCache()
    {
        var steps = new List<string>();
        WorkProjectInvitationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<WorkProjectInvitation>(), It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("invitation"));
        _outboxRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<UserInvitedEvent>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("event"))
            .ReturnsAsync(Guid.NewGuid());
        WorkProjectRepositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("save"))
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        Assert.Equal(["invitation", "event", "save"], steps);
        WorkProjectRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        VerifyAllLocalizedCacheEntriesRemoved(language => CacheKeys.Project.ProjectById(_project.Id, language));
    }

    [Fact]
    public async Task Handle_WhenProjectIsMissing_ThrowsNotFoundAndSendsNothing()
    {
        var command = CreateCommand();
        command.ProjectId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<EntityException>(
            () => CreateHandler().Handle(command, CancellationToken.None));

        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
        WorkProjectRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<UserInvitedEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private InviteWorkProjectParticipantHandler CreateHandler()
    {
        return new InviteWorkProjectParticipantHandler(
            CurrentUserMock.Object,
            WorkProjectRepositoryMock.Object,
            WorkProjectInvitationRepositoryMock.Object,
            _outboxRepositoryMock.Object,
            CacheServiceMock.Object);
    }

    private InviteWorkProjectParticipantCommand CreateCommand()
    {
        return new InviteWorkProjectParticipantCommand
        {
            ProjectId = _project.Id,
            Email = "  Nigar@Client.az ",
            Name = " Nigar",
            Surname = "Huseynova "
        };
    }
}
