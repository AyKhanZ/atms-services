using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.Search;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Search;
using Moq;

namespace Project.Services.Tests.Handlers.Search;

public class RecordGlobalSearchRecentHandlerTest : BaseHandlerTest
{
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task Handle_UsesCurrentUserAndStaysSilentForUnavailableItems(bool superAdmin, bool available)
    {
        var userId = Guid.NewGuid();
        CurrentUserMock.SetupGet(user => user.Id).Returns(userId);
        CurrentUserMock.SetupGet(user => user.RoleId).Returns(superAdmin ? RoleIds.SuperAdmin : RoleIds.Employee);
        var command = new RecordGlobalSearchRecentCommand { ItemType = (int)GlobalSearchItemType.Task, ItemId = Guid.NewGuid() };
        using var cancellation = new CancellationTokenSource();
        var repository = new Mock<IGlobalSearchRecentRepository>(MockBehavior.Strict);
        repository.Setup(value => value.RecordRecentAsync(userId, superAdmin, (GlobalSearchItemType)command.ItemType, command.ItemId, cancellation.Token))
            .ReturnsAsync(available);
        var handler = new RecordGlobalSearchRecentHandler(CurrentUserMock.Object, repository.Object);

        // An item the caller cannot see is not written and not reported: the client sends this
        // while a page opens and has nothing to do with the answer.
        await handler.Handle(command, cancellation.Token);

        repository.VerifyAll();
        repository.VerifyNoOtherCalls();
    }
}
