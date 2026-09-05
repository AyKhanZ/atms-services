using ATMS.Application.Dispatcher.Behaviors;
using ATMS.Application.Dispatcher.Validation;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.WorkTasks;
using FluentValidation;
using MediatR;

namespace Project.Services.Tests.Behaviors;

public class ValidationBehaviorTest
{
    [Fact]
    public async Task Handle_WhenRequestHasNoSpecificValidator_StillValidatesKeysetPagination()
    {
        var behavior = new ValidationBehavior<GetWorkTasksRequest, KeysetPagedResult<WorkTaskModel>>(
            [],
            new PagedRequestValidator(),
            new KeysetPagedRequestValidator());
        var request = new GetWorkTasksRequest
        {
            ProjectId = Guid.NewGuid(),
            PageSize = 51
        };
        var nextCalled = false;

        Task<KeysetPagedResult<WorkTaskModel>> Next(CancellationToken _)
        {
            nextCalled = true;
            return Task.FromResult(new KeysetPagedResult<WorkTaskModel>());
        }

        await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            request,
            Next,
            CancellationToken.None));

        Assert.False(nextCalled);
    }
}
