using ATMS.Application.Exceptions.Resources;
using ATMS.Contracts.Requests;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using FluentValidation;

namespace ATMS.Application.Dispatcher.Validation;

public abstract class KeysetPagedRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : GetKeysetPaginationRequest
{
    protected KeysetPagedRequestValidator()
    {
        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage(ValidationMessages.PageSizeOutOfRange);

        RuleFor(request => request.SortDirection)
            .IsInEnum()
            .WithMessage(ValidationMessages.InvalidSortDirection);

        RuleFor(request => request.Cursor)
            .Must((request, cursor) => IsValidCursor(cursor, request.SortDirection))
            .WithMessage(ValidationMessages.InvalidCursor)
            .When(request => !string.IsNullOrWhiteSpace(request.Cursor));
    }

    private static bool IsValidCursor(string? value, SortDirectionEnum direction)
        => KeysetCursor.TryDecode(value, out var cursor) && cursor?.SortDirection == direction;
}

public sealed class KeysetPagedRequestValidator : KeysetPagedRequestValidator<GetKeysetPaginationRequest>;
