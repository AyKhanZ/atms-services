using System.Linq.Expressions;
using ATMS.Infrastructure.Files;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.Attachments;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Project.Services.Tests.Validators.Attachments;

public class UploadAttachmentValidatorTest : BaseValidatorTest
{
    private const long MaxFileSizeBytes = 1024;
    private const int MaxFilesPerOwner = 3;

    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _workTaskId = Guid.NewGuid();
    private readonly Mock<IAttachmentRepository> _attachmentRepositoryMock = new();

    public UploadAttachmentValidatorTest()
    {
        WorkProjectsRepositoryMock
            .Setup(repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        WorkTasksRepositoryMock
            .Setup(repository => repository.IsWorkTaskExistAsync(_projectId, _workTaskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private UploadAttachmentValidator Validator()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AttachmentsOptions:RootPath"] = "attachments",
                ["AttachmentsOptions:MaxFileSizeBytes"] = MaxFileSizeBytes.ToString(),
                ["AttachmentsOptions:MaxFilesPerOwner"] = MaxFilesPerOwner.ToString()
            })
            .Build();

        return new UploadAttachmentValidator(
            configuration,
            WorkProjectsRepositoryMock.Object,
            WorkTasksRepositoryMock.Object,
            _attachmentRepositoryMock.Object,
            new FileSignatureService(configuration));
    }

    private UploadAttachmentCommand Command(IFormFile? file) => new()
    {
        ProjectId = _projectId,
        WorkTaskId = _workTaskId,
        File = file
    };

    private static IFormFile File(byte[] content, string name) =>
        new FormFile(new MemoryStream(content), 0, content.Length, "file", name);

    private static readonly byte[] Pdf = "%PDF-1.7 content"u8.ToArray();

    [Fact]
    public async Task Validate_WhenFileMatchesTheRules_IsValid()
    {
        var result = await Validator().ValidateAsync(Command(File(Pdf, "spec.pdf")));

        Assert.True(result.IsValid);
    }

    public static TheoryData<string, byte[]?> InvalidFiles => new()
    {
        { "missing", null },
        { "empty.pdf", [] },
        { "big.pdf", Pdf.Concat(new byte[MaxFileSizeBytes]).ToArray() },
        { "virus.exe", [0x4D, 0x5A, 0x90, 0x00] },
        { "drawing.svg", "<svg onload=alert(1)>"u8.ToArray() },
        { "no-extension", Pdf },
        { "renamed.pdf", [0x4D, 0x5A, 0x90, 0x00] },
        { "fake.png", Pdf }
    };

    [Theory]
    [MemberData(nameof(InvalidFiles))]
    public async Task Validate_WhenFileBreaksARule_ReportsTheFile(string name, byte[]? content)
    {
        var result = await Validator().ValidateAsync(Command(content is null ? null : File(content, name)));

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(UploadAttachmentCommand.File), error.PropertyName);
    }

    [Fact]
    public async Task Validate_WhenTaskAlreadyHoldsTheMaximum_ReportsTheTask()
    {
        _attachmentRepositoryMock
            .Setup(repository => repository.CountByWorkTaskAsync(_workTaskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MaxFilesPerOwner);

        var result = await Validator().ValidateAsync(Command(File(Pdf, "spec.pdf")));

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(UploadAttachmentCommand.WorkTaskId), error.PropertyName);
    }

    [Fact]
    public async Task Validate_WhenTaskIsNotInTheProject_ReportsTheTask()
    {
        var command = Command(File(Pdf, "spec.pdf"));
        command.WorkTaskId = Guid.NewGuid();

        var result = await Validator().ValidateAsync(command);

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(UploadAttachmentCommand.WorkTaskId), error.PropertyName);
    }

    [Fact]
    public async Task Validate_WhenIdentifiersAreMissing_ReportsBoth()
    {
        var result = await Validator().ValidateAsync(new UploadAttachmentCommand { File = File(Pdf, "spec.pdf") });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UploadAttachmentCommand.ProjectId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UploadAttachmentCommand.WorkTaskId));
    }
}
