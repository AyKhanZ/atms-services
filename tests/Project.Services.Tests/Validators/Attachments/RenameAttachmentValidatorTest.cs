using System.Linq.Expressions;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Attachments;
using ATMS.Project.Services.Validation.Attachments;
using Moq;

namespace Project.Services.Tests.Validators.Attachments;

public class RenameAttachmentValidatorTest : BaseValidatorTest
{
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _attachmentId = Guid.NewGuid();
    private readonly Mock<IAttachmentRepository> _attachmentRepositoryMock = new();

    public RenameAttachmentValidatorTest()
    {
        WorkProjectsRepositoryMock
            .Setup(repository => repository.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _attachmentRepositoryMock
            .Setup(repository => repository.IsAttachmentExistAsync(_projectId, _attachmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private RenameAttachmentValidator Validator() =>
        new(WorkProjectsRepositoryMock.Object, _attachmentRepositoryMock.Object, new AttachmentFileNameService());

    private RenameAttachmentCommand Command(string? fileName) => new()
    {
        ProjectId = _projectId,
        AttachmentId = _attachmentId,
        FileName = fileName
    };

    [Theory]
    [InlineData("Specification v2")]
    [InlineData("Отчёт за сентябрь")]
    public async Task Validate_WhenNameIsFine_IsValid(string fileName)
    {
        var result = await Validator().ValidateAsync(Command(fileName));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a/b")]
    [InlineData("a:b")]
    [InlineData("what?")]
    public async Task Validate_WhenNameIsEmptyOrHasForbiddenCharacters_ReportsTheName(string? fileName)
    {
        var result = await Validator().ValidateAsync(Command(fileName));

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(RenameAttachmentCommand.FileName), error.PropertyName);
    }

    [Fact]
    public async Task Validate_WhenNameIsLongerThan200Characters_ReportsTheName()
    {
        var result = await Validator().ValidateAsync(Command(new string('a', 201)));

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(RenameAttachmentCommand.FileName), error.PropertyName);
    }

    [Fact]
    public async Task Validate_WhenFileIsNotInTheProject_ReportsTheFile()
    {
        var command = Command("name");
        command.AttachmentId = Guid.NewGuid();

        var result = await Validator().ValidateAsync(command);

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(RenameAttachmentCommand.AttachmentId), error.PropertyName);
    }
}
