using ATMS.Application.Exceptions.Entity;
using ATMS.Infrastructure.Files;
using ATMS.Project.Contracts.Requests.Attachments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Attachments;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Project.Services.Tests.Handlers.Attachments;

public sealed class GetAttachmentContentHandlerTest : BaseHandlerTest, IDisposable
{
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Mock<IAttachmentRepository> _attachmentRepositoryMock = new();
    private readonly Mock<IFileStorage> _fileStorageMock = new();
    private readonly string _physicalPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");

    public void Dispose()
    {
        if (File.Exists(_physicalPath))
        {
            File.Delete(_physicalPath);
        }
    }

    private GetAttachmentContentHandler Handler() =>
        new(
            _attachmentRepositoryMock.Object,
            _fileStorageMock.Object,
            new FileSignatureService(new ConfigurationBuilder().Build()));

    private Attachment Stored(string contentType)
    {
        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            FileName = "Отчёт.pdf",
            ContentType = contentType,
            RelativePath = "p/2026/09/file.pdf"
        };
        _attachmentRepositoryMock
            .Setup(repository => repository.FindAsync(_projectId, attachment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attachment);
        _fileStorageMock.Setup(storage => storage.GetFullPath(attachment.RelativePath)).Returns(_physicalPath);
        return attachment;
    }

    [Theory]
    [InlineData("application/pdf", true)]
    [InlineData("application/zip", false)]
    public async Task Handle_WhenFileIsOnDisk_ReturnsItsPathNameAndWhetherItCanBePreviewed(string contentType, bool canPreview)
    {
        var attachment = Stored(contentType);
        await File.WriteAllBytesAsync(_physicalPath, [1]);

        var content = await Handler().Handle(
            new GetAttachmentContentRequest { ProjectId = _projectId, AttachmentId = attachment.Id },
            CancellationToken.None);

        Assert.Equal(_physicalPath, content.PhysicalPath);
        Assert.Equal("Отчёт.pdf", content.FileName);
        Assert.Equal(contentType, content.ContentType);
        Assert.Equal(canPreview, content.CanPreview);
    }

    [Fact]
    public async Task Handle_WhenRowExistsButFileIsMissing_ThrowsNotFound()
    {
        var attachment = Stored("application/pdf");

        var exception = await Assert.ThrowsAsync<EntityException>(() => Handler().Handle(
            new GetAttachmentContentRequest { ProjectId = _projectId, AttachmentId = attachment.Id },
            CancellationToken.None));

        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }

    [Fact]
    public async Task Handle_WhenFileIsNotInTheProject_ThrowsNotFound()
    {
        var exception = await Assert.ThrowsAsync<EntityException>(() => Handler().Handle(
            new GetAttachmentContentRequest { ProjectId = _projectId, AttachmentId = Guid.NewGuid() },
            CancellationToken.None));

        Assert.Equal(EntityErrorType.NotFound, exception.ErrorType);
    }
}
