using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Caching.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.WorkProjects;
using Moq;

namespace Project.Services.Tests.Handlers.WorkProjects;

public class CancelWorkProjectInvitationHandlerTest : BaseHandlerTest
{
    private readonly CancelWorkProjectInvitationCommand _command = new()
    {
        ProjectId = Guid.NewGuid(),
        InvitationId = Guid.NewGuid()
    };

    [Fact]
    public async Task Handle_MarksInvitationCancelledAndFlushesProjectCache()
    {
        var invitation = new WorkProjectInvitation
        {
            Id = _command.InvitationId,
            WorkProjectId = _command.ProjectId,
            Status = (int)WorkProjectInvitationStatusEnum.Pending
        };
        WorkProjectInvitationRepositoryMock
            .Setup(x => x.FindPendingAsync(_command.ProjectId, _command.InvitationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        await CreateHandler().Handle(_command, CancellationToken.None);

        Assert.Equal((int)WorkProjectInvitationStatusEnum.Cancelled, invitation.Status);
        Assert.NotNull(invitation.ProcessedAt);
        WorkProjectInvitationRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        VerifyAllLocalizedCacheEntriesRemoved(language => CacheKeys.Project.ProjectById(_command.ProjectId, language));
    }

    [Fact]
    public async Task Handle_WhenInvitationIsNoLongerPending_ThrowsNotFoundAndSavesNothing()
    {
        var exception = await Assert.ThrowsAsync<EntityException>(
            () => CreateHandler().Handle(_command, CancellationToken.None));

        Assert.Equal(EntityErrorTypeEnum.NotFound, exception.ErrorType);
        WorkProjectInvitationRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private CancelWorkProjectInvitationHandler CreateHandler()
    {
        return new CancelWorkProjectInvitationHandler(
            WorkProjectInvitationRepositoryMock.Object,
            CacheServiceMock.Object);
    }
}
