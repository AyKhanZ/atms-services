using ATMS.Application.Interfaces;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Notifications;
using ATMS.Project.Contracts.Requests.Notifications;
using ATMS.Project.Data.Criteria.Notifications;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Notifications;
using AutoMapper;
using Moq;

namespace Project.Services.Tests.Handlers.Notifications;

public sealed class GetNotificationsHandlerTest
{
    [Theory]
    [InlineData(SortDirectionEnum.Desc)]
    [InlineData(SortDirectionEnum.Asc)]
    public async Task Handle_ReadsTheCallersOwnPageNewestFirst(SortDirectionEnum requested)
    {
        var userId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(user => user.Id).Returns(userId);
        var request = new GetNotificationsRequest { UnreadOnly = true, PageSize = 10, SortDirection = requested };
        var filter = new NotificationFilter { UnreadOnly = true };
        var row = new NotificationRow { Id = Guid.NewGuid(), Parameters = new NotificationParameters() };
        var model = new NotificationModel { Id = row.Id, Parameters = new NotificationParametersModel() };
        var mapper = new Mock<IMapper>();
        mapper.Setup(map => map.Map<NotificationFilter>(request)).Returns(filter);
        mapper.Setup(map => map.Map<NotificationModel[]>(It.IsAny<object>())).Returns([model]);
        var repository = new Mock<INotificationRepository>();
        KeysetPaginationCriteria<NotificationRow>? pagination = null;
        repository
            .Setup(notifications => notifications.GetManyAsync(
                userId,
                filter,
                It.IsAny<KeysetPaginationCriteria<NotificationRow>>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, ACriteria<Notification>, KeysetPaginationCriteria<NotificationRow>, CancellationToken>(
                (_, _, value, _) => pagination = value)
            .ReturnsAsync(new KeysetPagedResult<NotificationRow>
            {
                Items = [row],
                NextCursor = "next",
                HasMore = true,
                PageSize = 10
            });

        var page = await new GetNotificationsHandler(repository.Object, currentUser.Object, mapper.Object)
            .Handle(request, CancellationToken.None);

        Assert.Equal([model], page.Items);
        Assert.Equal("next", page.NextCursor);
        Assert.True(page.HasMore);
        Assert.Equal(10, page.PageSize);
        Assert.Equal(SortDirectionEnum.Desc, pagination?.SortDirection);
        Assert.Equal(10, pagination?.PageSize);
    }
}
