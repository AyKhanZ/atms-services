using System.Linq.Expressions;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.Attachments;
using Moq;

namespace Project.Services.Tests.Validators.Attachments;

public class DeleteAttachmentValidatorTest : BaseValidatorTest
{
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _attachmentId = Guid.NewGuid();
    private readonly Mock<IAttachmentRepository> _attachmentRepositoryMock = new();

    public DeleteAttachmentValidatorTest()
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

    private DeleteAttachmentValidator Validator() =>
        new(WorkProjectsRepositoryMock.Object, _attachmentRepositoryMock.Object);

    [Fact]
    public async Task Validate_WhenFileIsInTheProject_IsValid()
    {
        var result = await Validator().ValidateAsync(new DeleteAttachmentCommand
        {
            ProjectId = _projectId,
            AttachmentId = _attachmentId
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenFileIsNotInTheProject_ReportsTheFile()
    {
        var result = await Validator().ValidateAsync(new DeleteAttachmentCommand
        {
            ProjectId = _projectId,
            AttachmentId = Guid.NewGuid()
        });

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(DeleteAttachmentCommand.AttachmentId), error.PropertyName);
    }

    [Fact]
    public async Task Validate_WhenIdentifiersAreMissing_ReportsBoth()
    {
        var result = await Validator().ValidateAsync(new DeleteAttachmentCommand());

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(DeleteAttachmentCommand.ProjectId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(DeleteAttachmentCommand.AttachmentId));
    }
}
