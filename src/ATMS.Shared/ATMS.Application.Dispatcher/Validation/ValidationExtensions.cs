using ATMS.Application.Exceptions.Resources;
using FluentValidation;

namespace ATMS.Application.Dispatcher.Validation;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, DateTime> IsInDateRange<T>(
        this IRuleBuilder<T, DateTime> ruleBuilder,
        DateTime? minimum = null,
        DateTime? maximum = null)
    {
        var min = minimum ?? new DateTime(1900, 1, 1);
        var max = maximum ?? new DateTime(2100, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        return ruleBuilder
            .InclusiveBetween(min, max)
            .WithMessage(string.Format(
                ValidationMessages.DateOutOfRange,
                min.ToString("dd.MM.yyyy"),
                max.ToString("dd.MM.yyyy")));
    }

    public static IRuleBuilderOptions<T, DateTime?> IsInDateRange<T>(
        this IRuleBuilder<T, DateTime?> ruleBuilder,
        DateTime? minimum = null,
        DateTime? maximum = null)
    {
        var min = minimum ?? new DateTime(1900, 1, 1);
        var max = maximum ?? new DateTime(2100, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        return ruleBuilder
            .Must(value => !value.HasValue || value.Value >= min && value.Value <= max)
            .WithMessage(string.Format(
                ValidationMessages.DateOutOfRange,
                min.ToString("dd.MM.yyyy"),
                max.ToString("dd.MM.yyyy")));
    }

    public static IRuleBuilderOptions<T, DateOnly?> IsInDateRange<T>(
        this IRuleBuilder<T, DateOnly?> ruleBuilder,
        DateOnly? minimum = null,
        DateOnly? maximum = null)
    {
        var min = minimum ?? new DateOnly(1900, 1, 1);
        var max = maximum ?? new DateOnly(2100, 12, 31);

        return ruleBuilder
            .Must(value => !value.HasValue || value.Value >= min && value.Value <= max)
            .WithMessage(string.Format(
                ValidationMessages.DateOutOfRange,
                min.ToString("dd.MM.yyyy"),
                max.ToString("dd.MM.yyyy")));
    }

    public static IRuleBuilderOptions<T, DateOnly> IsInDateRange<T>(
        this IRuleBuilder<T, DateOnly> ruleBuilder,
        DateOnly? minimum = null,
        DateOnly? maximum = null)
    {
        var min = minimum ?? new DateOnly(1900, 1, 1);
        var max = maximum ?? new DateOnly(2100, 12, 31);

        return ruleBuilder
            .InclusiveBetween(min, max)
            .WithMessage(string.Format(
                ValidationMessages.DateOutOfRange,
                min.ToString("dd.MM.yyyy"),
                max.ToString("dd.MM.yyyy")));
    }
}
