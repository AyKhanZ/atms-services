using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Caching.Constants;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Data.Messaging;
using ATMS.Messaging.Configuration;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Enums;
using ATMS.Project.Services.Handlers.WorkProjects;
using ATMS.Project.Services.Validation.WorkProjects;
using FluentValidation;
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
    public async Task Handle_AddsPendingClientViewerInvitationUnderTheLimit()
    {
        WorkProjectInvitation? saved = null;
        WorkProjectInvitationRepositoryMock
            .Setup(x => x.AddWithinLimitAsync(
                It.IsAny<WorkProjectInvitation>(),
                WorkProjectParticipantLimit.Max,
                It.IsAny<CancellationToken>()))
            .Callback<WorkProjectInvitation, int, CancellationToken>((invitation, _, _) => saved = invitation)
            .ReturnsAsync((WorkProjectParticipantRefusalEnum?)null);

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
        VerifyAllLocalizedCacheEntriesRemoved(language => CacheKeys.Project.ProjectById(_project.Id, language));
    }

    // The outbox message must already be in the context when the locked insert saves: one commit for both.
    [Fact]
    public async Task Handle_AddsTheAdminEventBeforeTheLockedInsertSaves()
    {
        var steps = new List<string>();
        _outboxRepositoryMock
            .Setup(x => x.AddAsync(
                MessagingConstants.Exchanges.UserEvents,
                MessagingConstants.RoutingKeys.UserInvited,
                new UserInvitedEvent(
                    "Nigar@Client.az",
                    "Nigar",
                    "Huseynova",
                    _project.OrganizationId,
                    _inviterId,
                    _project.Title),
                It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("event"))
            .ReturnsAsync(Guid.NewGuid());
        WorkProjectInvitationRepositoryMock
            .Setup(x => x.AddWithinLimitAsync(
                It.IsAny<WorkProjectInvitation>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("invitation"))
            .ReturnsAsync((WorkProjectParticipantRefusalEnum?)null);

        await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        Assert.Equal(["event", "invitation"], steps);
    }

    // Two invitations at once: the second one is refused under the lock, even though it passed the validator.
    [Theory]
    [InlineData(WorkProjectParticipantRefusalEnum.AlreadyInvited)]
    [InlineData(WorkProjectParticipantRefusalEnum.LimitReached)]
    public async Task Handle_WhenRefusedUnderTheLock_FailsOnEmailAndKeepsTheCache(WorkProjectParticipantRefusalEnum refusal)
    {
        WorkProjectInvitationRepositoryMock
            .Setup(x => x.AddWithinLimitAsync(
                It.IsAny<WorkProjectInvitation>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refusal);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => CreateHandler().Handle(CreateCommand(), CancellationToken.None));

        var failure = Assert.Single(exception.Errors);
        Assert.Equal(nameof(InviteWorkProjectParticipantCommand.Email), failure.PropertyName);
        CacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProjectIsMissing_ThrowsNotFoundAndAddsNothing()
    {
        var command = CreateCommand();
        command.ProjectId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<EntityException>(
            () => CreateHandler().Handle(command, CancellationToken.None));

        Assert.Equal(EntityErrorTypeEnum.NotFound, exception.ErrorType);
        WorkProjectInvitationRepositoryMock.Verify(x => x.AddWithinLimitAsync(
            It.IsAny<WorkProjectInvitation>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
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
