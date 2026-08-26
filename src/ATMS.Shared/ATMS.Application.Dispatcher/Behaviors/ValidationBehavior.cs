using ATMS.Contracts.Requests;
using FluentValidation;
using MediatR;

namespace ATMS.Application.Dispatcher.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    IValidator<GetPaginationRequest> pagedRequestValidator,
    IValidator<GetKeysetPaginationRequest> keysetPagedRequestValidator)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next(cancellationToken);
        
        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (request is GetPaginationRequest pagedRequest)
        {
            var result = await pagedRequestValidator.ValidateAsync(pagedRequest, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (request is GetKeysetPaginationRequest keysetPagedRequest)
        {
            var result = await keysetPagedRequestValidator.ValidateAsync(keysetPagedRequest, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next(cancellationToken);
    }
}
