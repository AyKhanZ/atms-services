using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Contracts.Requests.Attachments;
using ATMS.Project.Data.Criteria.Attachments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Attachments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Attachments;
using FluentValidation;
using Moq;

namespace Project.Services.Tests.Handlers.Attachments;

public class GetAttachmentsHandlerTest : BaseHandlerTest
{
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Mock<IAttachmentRepository> _attachmentRepositoryMock = new();

    public GetAttachmentsHandlerTest()
    {
        MapperMock
            .Setup(mapper => mapper.Map<AttachmentOwnerTasksFilter>(It.IsAny<object>()))
            .Returns(new AttachmentOwnerTasksFilter { ProjectId = _projectId });
        MapperMock
            .Setup(mapper => mapper.Map<AttachmentModel>(It.IsAny<object>()))
            .Returns(() => new AttachmentModel());
    }

    private GetAttachmentsHandler Handler() => new(_attachmentRepositoryMock.Object, MapperMock.Object);

    private void Returns(int count)
    {
        var items = Enumerable.Range(0, count)
            .Select(index => new AttachmentListItem(
                Guid.NewGuid(),
                $"{index}.pdf",
                "application/pdf",
                1,
                DateTime.UtcNow,
                new AttachmentAuthor(Guid.NewGuid(), "A", "B", null),
                new AttachmentOwnerTask(Guid.NewGuid(), "1", "Task"),
                null))
            .ToArray();

        _attachmentRepositoryMock
            .Setup(repository => repository.GetManyAsync(
                It.IsAny<ACriteria<WorkTask>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);
    }

    [Fact]
    public async Task Handle_WithOneScope_ReturnsTheFiles()
    {
        Returns(3);

        var result = await Handler().Handle(
            new GetAttachmentsRequest { ProjectId = _projectId, WorkTicketId = Guid.NewGuid() },
            CancellationToken.None);

        Assert.Equal(3, result.Items.Count);
        Assert.False(result.HasMore);
    }

    // One more row than the cap is read, so the client learns the list was cut without a count query.
    [Fact]
    public async Task Handle_WhenMoreThan1000Files_ReturnsTheFirst1000AndSaysThereIsMore()
    {
        Returns(1001);

        var result = await Handler().Handle(
            new GetAttachmentsRequest { ProjectId = _projectId, WorkTicketId = Guid.NewGuid() },
            CancellationToken.None);

        Assert.Equal(1000, result.Items.Count);
        Assert.True(result.HasMore);
        _attachmentRepositoryMock.Verify(repository => repository.GetManyAsync(
            It.IsAny<ACriteria<WorkTask>>(),
            1001,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    public static TheoryData<Guid?, Guid?, Guid?> WrongScopes => new()
    {
        { null, null, null },
        { Guid.NewGuid(), Guid.NewGuid(), null },
        { Guid.NewGuid(), null, Guid.NewGuid() },
        { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
    };

    [Theory]
    [MemberData(nameof(WrongScopes))]
    public async Task Handle_WithoutExactlyOneScope_ThrowsValidationException(
        Guid? workTaskId, Guid? parentWorkTaskId, Guid? workTicketId)
    {
        await Assert.ThrowsAsync<ValidationException>(() => Handler().Handle(new GetAttachmentsRequest
        {
            ProjectId = _projectId,
            WorkTaskId = workTaskId,
            ParentWorkTaskId = parentWorkTaskId,
            WorkTicketId = workTicketId
        }, CancellationToken.None));
    }
}
