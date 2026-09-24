using ATMS.Data.Enums;
using ATMS.Infrastructure.Files;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Attachments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Attachments;
using ATMS.Project.Services.Handlers.Attachments;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using FluentValidation;
using Moq;

namespace Project.Services.Tests.Handlers.Attachments;

public class UploadAttachmentHandlerTest : BaseHandlerTest
{
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _workTaskId = Guid.NewGuid();
    private readonly Mock<IAttachmentRepository> _attachmentRepositoryMock = new();
    private readonly Mock<IFileStorage> _fileStorageMock = new();
    private Attachment? _added;

    public UploadAttachmentHandlerTest()
    {
        _fileStorageMock
            .Setup(storage => storage.SaveAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("stored/path.pdf");
        _attachmentRepositoryMock
            .Setup(repository => repository.AddWithinLimitAsync(It.IsAny<Attachment>(), 100, It.IsAny<CancellationToken>()))
            .Callback<Attachment, int, CancellationToken>((attachment, _, _) => _added = attachment)
            .ReturnsAsync(true);
        _attachmentRepositoryMock
            .Setup(repository => repository.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AttachmentListItem(
                Guid.NewGuid(),
                "spec.pdf",
                "application/pdf",
                4,
                DateTime.UtcNow,
                new AttachmentAuthor(Guid.NewGuid(), "A", "B", null),
                new AttachmentOwnerTask(_workTaskId, "1", "Task"),
                null));
        MapperMock
            .Setup(mapper => mapper.Map<AttachmentModel>(It.IsAny<object>()))
            .Returns(new AttachmentModel());
    }

    private UploadAttachmentHandler Handler() =>
        new(
            _attachmentRepositoryMock.Object,
            _fileStorageMock.Object,
            new FileSignatureService(new ConfigurationBuilder().Build()),
            new AttachmentFileNameService(),
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["AttachmentsOptions:RootPath"] = "attachments" })
                .Build(),
            MapperMock.Object);

    private UploadAttachmentCommand Command(string fileName) => new()
    {
        ProjectId = _projectId,
        WorkTaskId = _workTaskId,
        File = new FormFile(new MemoryStream("%PDF"u8.ToArray()), 0, 4, "file", fileName)
    };

    [Fact]
    public async Task Handle_StoresTheFileUnderTheProjectAndMonthAndSavesTheRow()
    {
        var now = DateTime.UtcNow;

        await Handler().Handle(Command("Specification.PDF"), CancellationToken.None);

        _fileStorageMock.Verify(storage => storage.SaveAsync(
            It.IsAny<IFormFile>(),
            $"{_projectId:N}/{now:yyyy}/{now:MM}",
            "pdf",
            It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(_added);
        Assert.Equal(AttachmentOwnerTypeEnum.Task, _added.OwnerType);
        Assert.Equal(_workTaskId, _added.OwnerId);
        Assert.Equal("Specification.pdf", _added.FileName);
        Assert.Equal("stored/path.pdf", _added.RelativePath);
        Assert.Equal("application/pdf", _added.ContentType);
        Assert.Equal(4, _added.Size);
        _attachmentRepositoryMock.Verify(
            repository => repository.AddWithinLimitAsync(It.IsAny<Attachment>(), 100, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // A row that failed to save must not leave an orphan file on the disk.
    [Fact]
    public async Task Handle_WhenTheRowCannotBeSaved_DeletesTheStoredFile()
    {
        _attachmentRepositoryMock
            .Setup(repository => repository.AddWithinLimitAsync(It.IsAny<Attachment>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => Handler().Handle(Command("a.pdf"), CancellationToken.None));

        _fileStorageMock.Verify(storage => storage.DeleteAsync("stored/path.pdf", It.IsAny<CancellationToken>()), Times.Once);
    }

    // Two uploads at 99 files: the second loses the race under the task's lock and is refused,
    // and its file must not stay behind on the disk.
    [Fact]
    public async Task Handle_WhenAnotherUploadTookTheLastPlace_RemovesTheFileAndRefuses()
    {
        _attachmentRepositoryMock
            .Setup(repository => repository.AddWithinLimitAsync(It.IsAny<Attachment>(), 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => Handler().Handle(Command("a.pdf"), CancellationToken.None));

        Assert.Equal(nameof(UploadAttachmentCommand.WorkTaskId), Assert.Single(exception.Errors).PropertyName);
        _fileStorageMock.Verify(storage => storage.DeleteAsync("stored/path.pdf", It.IsAny<CancellationToken>()), Times.Once);
        _attachmentRepositoryMock.Verify(repository => repository.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
