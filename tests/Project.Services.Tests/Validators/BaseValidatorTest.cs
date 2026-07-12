using ATMS.Project.Data.Repositories.Interfaces;
using Bogus;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Project.Services.Tests.Validators;

public abstract class BaseValidatorTest
{
    protected readonly Faker Faker = new();
    protected readonly Mock<IOrganizationRepository> OrganizationRepositoryMock = new();

    protected static IConfiguration CreateImagesConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ImagesOptions:ImagesRootPath"] = string.Empty,
                ["ImagesOptions:BaseImageUrl"] = "http://localhost/images",
                ["ImagesOptions:MaxFileSizeBytes"] = "5242880",
                ["ImagesOptions:MaxPixelCount"] = "12000000",
                ["ImagesOptions:AllowedContentTypes:0"] = "image/jpeg",
                ["ImagesOptions:AllowedContentTypes:1"] = "image/jfif",
                ["ImagesOptions:AllowedContentTypes:2"] = "image/png",
                ["ImagesOptions:AllowedContentTypes:3"] = "image/webp"
            })
            .Build();
    }
}