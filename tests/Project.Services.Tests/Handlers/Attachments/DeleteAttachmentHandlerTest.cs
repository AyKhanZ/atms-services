using ATMS.Application.Exceptions.Entity;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Attachments;
using Moq;

namespace Project.Services.Tests.Handlers.Attachments;

public class DeleteAttachmentHandlerTest : BaseHandlerTest
{
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Mock<IAttachmentRepository> _attachmentRepositoryMock = new();

    private DeleteAttachmentHandler Handler() => new(CurrentUserMock.Object, _attachmentRepositoryMock.Object);

    [Fact]
    public async Task Handle_MarksTheFileDeletedByTheCurrentUser()
    {
        var attachment = new Attachment { Id = Guid.NewGuid(), FileName = "a.pdf" };
        var userId = Guid.NewGuid();
        CurrentUserMock.SetupGet(user => user.Id).Returns(userId);
        _attachmentRepositoryMock
            .Setup(repository => repository.FindAsync(_projectId, attachment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attachment);

        await Handler().Handle(
            new DeleteAttachmentCommand { ProjectId = _projectId, AttachmentId = attachment.Id },
            CancellationToken.None);

        Assert.True(attachment.IsDeleted);
        Assert.NotNull(attachment.DeletedAt);
        Assert.Equal(userId, attachment.DeletedById);
        _attachmentRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFileIsGone_ThrowsNotFound()
    {
        var exception = await Assert.ThrowsAsync<EntityException>(() => Handler().Handle(
            new DeleteAttachmentCommand { ProjectId = _projectId, AttachmentId = Guid.NewGuid() },
            CancellationToken.None));

        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }
}
