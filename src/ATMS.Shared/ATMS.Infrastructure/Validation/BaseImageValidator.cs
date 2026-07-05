using System.Linq.Expressions;
using ATMS.Infrastructure.Options;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ATMS.Infrastructure.Validation;

public abstract class BaseImageValidator<T> : AbstractValidator<T>
{
    private readonly ImagesOptions _imagesOptions;

    protected BaseImageValidator(IConfiguration configuration)
    {
        _imagesOptions = configuration
            .GetSection(nameof(ImagesOptions))
            .Get<ImagesOptions>()
            ?? new ImagesOptions
            {
                ImagesRootPath = string.Empty,
                BaseImageUrl = string.Empty
            };
    }

    protected void RuleForRequiredImage(
        Expression<Func<T, IFormFile?>> expression,
        string requiredMessage)
    {
        RuleFor(expression)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage(requiredMessage)
            .Must(file => file is { Length: > 0 }).WithMessage(requiredMessage)
            .Must(file => file!.Length <= _imagesOptions.MaxFileSizeBytes)
            .WithMessage("Image file is too large.")
            .Must(file => IsAllowedContentType(file!.ContentType))
            .WithMessage("Unsupported image format.");
    }

    private bool IsAllowedContentType(string? contentType)
    {
        return !string.IsNullOrWhiteSpace(contentType) &&
               _imagesOptions.AllowedContentTypes.Any(allowed =>
                   string.Equals(allowed, contentType, StringComparison.OrdinalIgnoreCase));
    }
}
