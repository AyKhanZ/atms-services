using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Contracts.Requests.Attachments;
using ATMS.Project.Data.Criteria.Attachments;
using ATMS.Project.Data.Models.Attachments;
using ATMS.Project.Services.Modules;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Project.Services.Tests.Mappers;

public sealed class AttachmentMapperTest
{
    private static IMapper Mapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMapperServices();
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    [Fact]
    public void MapListItem_CarriesTheFileItsAuthorAndItsPlaceInTheHierarchy()
    {
        var item = new AttachmentListItem(
            Guid.NewGuid(),
            "spec.pdf",
            "application/pdf",
            2048,
            new DateTime(2026, 9, 22, 10, 0, 0, DateTimeKind.Utc),
            new AttachmentAuthor(Guid.NewGuid(), "Rustam", "Aliyev", "users/rustam.png"),
            new AttachmentOwnerTask(Guid.NewGuid(), "70", "Stripe webhook"),
            new AttachmentOwnerTask(Guid.NewGuid(), "34", "Payment gateway"));

        var model = Mapper().Map<AttachmentModel>(item);

        Assert.Equal(item.Id, model.Id);
        Assert.Equal("spec.pdf", model.FileName);
        Assert.Equal(2048, model.Size);
        Assert.Equal("Rustam", model.CreatedBy.Name);
        Assert.Equal("users/rustam.png", model.CreatedBy.AvatarPath);
        Assert.Equal("70", model.WorkTask.Code);
        Assert.Equal("Stripe webhook", model.WorkTask.Name);
        Assert.Equal("Payment gateway", model.ParentWorkTask!.Name);
    }

    [Fact]
    public void MapListItem_OfATask_HasNoParent()
    {
        var item = new AttachmentListItem(
            Guid.NewGuid(),
            "a.txt",
            "text/plain",
            1,
            DateTime.UtcNow,
            new AttachmentAuthor(Guid.NewGuid(), "A", "B", null),
            new AttachmentOwnerTask(Guid.NewGuid(), "1", "Task"),
            null);

        Assert.Null(Mapper().Map<AttachmentModel>(item).ParentWorkTask);
    }

    [Fact]
    public void MapRequest_ToOwnerTasksFilter_KeepsEveryScope()
    {
        var request = new GetAttachmentsRequest
        {
            ProjectId = Guid.NewGuid(),
            WorkTicketId = Guid.NewGuid()
        };

        var filter = Mapper().Map<AttachmentOwnerTasksFilter>(request);

        Assert.Equal(request.ProjectId, filter.ProjectId);
        Assert.Equal(request.WorkTicketId, filter.WorkTicketId);
        Assert.Null(filter.WorkTaskId);
        Assert.Null(filter.ParentWorkTaskId);
    }
}
