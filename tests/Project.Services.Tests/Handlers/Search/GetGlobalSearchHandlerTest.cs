using ATMS.Application.Dispatcher.Behaviors;
using ATMS.Application.Dispatcher.Modules;
using ATMS.Contracts.Requests;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Search;
using ATMS.Project.Contracts.Requests.Search;
using ATMS.Project.Data.Models.Search;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Search;
using ATMS.Project.Services.Modules;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Project.Services.Tests.Handlers.Search;

public class GetGlobalSearchHandlerTest : BaseHandlerTest
{
    private readonly Mock<IGlobalSearchRepository> _repository = new(MockBehavior.Strict);
    private readonly Mock<IGlobalSearchRecentRepository> _recentRepository = new(MockBehavior.Strict);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_UsesCurrentUsersAccessAndOneSearchCall(bool superAdmin)
    {
        var userId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        CurrentUserMock.SetupGet(user => user.Id).Returns(userId);
        CurrentUserMock.SetupGet(user => user.RoleId).Returns(superAdmin ? RoleIds.SuperAdmin : RoleIds.Employee);
        _repository.Setup(repository => repository.SearchAsync(
                userId, superAdmin, "Payment", 5, It.IsAny<string>(), cancellation.Token))
            .ReturnsAsync([
                // Six project rows for a take of five: the repository reads one past the limit so
                // the handler can answer "there is more" without a count.
                .. Enumerable.Range(0, 6).Select(_ => Row(GlobalSearchItemType.Project)),
                Row(GlobalSearchItemType.Subtask)
            ]);
        using var provider = CreateMapperProvider();
        var handler = new GetGlobalSearchHandler(CurrentUserMock.Object, _repository.Object, _recentRepository.Object, provider.GetRequiredService<IMapper>());

        var result = await handler.Handle(new GetGlobalSearchRequest { Q = "  Payment  " }, cancellation.Token);

        Assert.Equal(5, result.Projects.Items.Length);
        Assert.True(result.Projects.HasMore);
        Assert.Single(result.Subtasks.Items);
        Assert.False(result.Subtasks.HasMore);
        Assert.Empty(result.Tickets.Items);
        Assert.Empty(result.Tasks.Items);
        Assert.False(result.Tasks.HasMore);
        _repository.VerifyAll();
        _repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Handle_EmptyQueryReturnsRecentWithoutSearching(string? query)
    {
        var rows = new[] { Row(GlobalSearchItemType.Ticket), Row(GlobalSearchItemType.Project) };
        _recentRepository.Setup(repository => repository.GetRecentAsync(
                CurrentUserMock.Object.Id, false, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows);
        using var provider = CreateMapperProvider();
        var handler = new GetGlobalSearchHandler(CurrentUserMock.Object, _repository.Object, _recentRepository.Object, provider.GetRequiredService<IMapper>());

        var result = await handler.Handle(new GetGlobalSearchRequest { Q = query }, CancellationToken.None);

        Assert.Equal(rows.Select(row => row.Id), result.Recent.Select(item => item.Id));
        Assert.Empty(result.Projects.Items);
        _recentRepository.VerifyAll();
        _repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("x", 5, "Q")]
    [InlineData(" # ", 5, "Q")]
    [InlineData("##", 5, "Q")]
    [InlineData("Payment", 0, "Take")]
    [InlineData("Payment", -1, "Take")]
    [InlineData("Payment", 51, "Take")]
    [InlineData(null, 51, "Take")]
    public async Task Pipeline_InvalidInputFailsBeforeDatabase(string query, int take, string property)
    {
        var services = new ServiceCollection();
        services.AddSharedValidationServices();
        services.AddValidationServices();
        using var provider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<GetGlobalSearchRequest, GlobalSearchModel>(
            provider.GetServices<IValidator<GetGlobalSearchRequest>>(),
            provider.GetRequiredService<IValidator<GetPaginationRequest>>(),
            provider.GetRequiredService<IValidator<GetKeysetPaginationRequest>>());
        var handler = new GetGlobalSearchHandler(CurrentUserMock.Object, _repository.Object, _recentRepository.Object, MapperMock.Object);
        var request = new GetGlobalSearchRequest { Q = query, Take = take };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            request, cancellationToken => handler.Handle(request, cancellationToken), CancellationToken.None));

        Assert.Equal(property, Assert.Single(exception.Errors).PropertyName);
        _repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(100, 50)]
    public async Task Handle_AcceptsLengthAndTakeBoundaries(int length, int take)
    {
        var query = new string('a', length);
        _repository.Setup(repository => repository.SearchAsync(
                It.IsAny<Guid>(), false, query, take, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        using var provider = CreateMapperProvider();
        var handler = new GetGlobalSearchHandler(CurrentUserMock.Object, _repository.Object, _recentRepository.Object, provider.GetRequiredService<IMapper>());

        var result = await handler.Handle(new GetGlobalSearchRequest { Q = query, Take = take }, CancellationToken.None);

        Assert.Empty(result.Projects.Items);
        _repository.VerifyAll();
    }

    [Fact]
    public async Task Handle_PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        _repository.Setup(repository => repository.SearchAsync(
                It.IsAny<Guid>(), false, "test", 5, It.IsAny<string>(), cancellation.Token))
            .ThrowsAsync(new OperationCanceledException(cancellation.Token));
        var handler = new GetGlobalSearchHandler(CurrentUserMock.Object, _repository.Object, _recentRepository.Object, MapperMock.Object);

        await Assert.ThrowsAsync<OperationCanceledException>(() => handler.Handle(
            new GetGlobalSearchRequest { Q = "test" }, cancellation.Token));
    }

    private static GlobalSearchRow Row(GlobalSearchItemType type) => new()
    {
        ItemType = type, Id = Guid.NewGuid(), Code = "42", Title = "Payment", ProjectId = Guid.NewGuid(),
        ProjectCode = "1", ProjectTitle = "Project", StatusId = 1, StatusCode = "New", StatusName = "New"
    };

    private static ServiceProvider CreateMapperProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMapperServices();
        return services.BuildServiceProvider();
    }
}
