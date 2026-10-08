using ATMS.Application.Dispatcher.Behaviors;
using ATMS.Application.Dispatcher.Validation;
using ATMS.Contracts.Requests;
using ATMS.Data.Criteria;
using FluentValidation;
using MediatR;

namespace ATMS.Application.Dispatcher.Tests.Behaviors;

public class ValidationBehaviorTest
{
    [Fact]
    public async Task Handle_WhenRequestHasNoSpecificValidator_StillValidatesKeysetPagination()
    {
        var behavior = new ValidationBehavior<KeysetListRequest, KeysetPagedResult<string>>(
            [],
            new PagedRequestValidator(),
            new KeysetPagedRequestValidator());
        var request = new KeysetListRequest { PageSize = 51 };
        var nextCalled = false;

        Task<KeysetPagedResult<string>> Next(CancellationToken _)
        {
            nextCalled = true;
            return Task.FromResult(new KeysetPagedResult<string>());
        }

        await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            request,
            Next,
            CancellationToken.None));

        Assert.False(nextCalled);
    }

    public sealed class KeysetListRequest : GetKeysetPaginationRequest, IRequest<KeysetPagedResult<string>>;
}
