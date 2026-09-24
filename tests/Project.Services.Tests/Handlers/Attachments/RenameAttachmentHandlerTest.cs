using ATMS.Application.Exceptions.Entity;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Attachments;
using ATMS.Project.Services.Handlers.Attachments;
using Moq;

namespace Project.Services.Tests.Handlers.Attachments;

public class RenameAttachmentHandlerTest : BaseHandlerTest
{
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Mock<IAttachmentRepository> _attachmentRepositoryMock = new();

    private RenameAttachmentHandler Handler() =>
        new(_attachmentRepositoryMock.Object, new AttachmentFileNameService());

    [Fact]
    public async Task Handle_ChangesTheNameAndKeepsTheExtension()
    {
        var attachment = new Attachment { Id = Guid.NewGuid(), FileName = "old.pdf" };
        _attachmentRepositoryMock
            .Setup(repository => repository.FindAsync(_projectId, attachment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attachment);

        await Handler().Handle(new RenameAttachmentCommand
        {
            ProjectId = _projectId,
            AttachmentId = attachment.Id,
            FileName = " Final spec "
        }, CancellationToken.None);

        Assert.Equal("Final spec.pdf", attachment.FileName);
        _attachmentRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFileIsGone_ThrowsNotFound()
    {
        var exception = await Assert.ThrowsAsync<EntityException>(() => Handler().Handle(new RenameAttachmentCommand
        {
            ProjectId = _projectId,
            AttachmentId = Guid.NewGuid(),
            FileName = "name"
        }, CancellationToken.None));

        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }
}
