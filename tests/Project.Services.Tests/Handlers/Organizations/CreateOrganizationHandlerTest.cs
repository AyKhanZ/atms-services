using ATMS.Infrastructure.Images;
using ATMS.Project.Contracts.Commands.Organizations;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.Organizations;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Project.Services.Tests.Handlers.Organizations;

public class CreateOrganizationHandlerTest : BaseHandlerTest
{
    private readonly CreateOrganizationHandler _handler;

    public CreateOrganizationHandlerTest()
    {
        _handler = new CreateOrganizationHandler(
            MapperMock.Object,
            ImageStorageMock.Object,
            OrganizationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsNewGuid()
    {
        var command = new CreateOrganizationCommand
        {
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10)
        };
        var entity = new Organization();

        MapperMock
            .Setup(m => m.Map<Organization>(command))
            .Returns(entity);

        OrganizationRepositoryMock
            .Setup(r => r.CreateAsync(entity,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public async Task Handle_AssignsNewIdToEntity()
    {
        var command = new CreateOrganizationCommand
        {
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10)
        };
        var entity = new Organization { Id = Guid.Empty };

        MapperMock
            .Setup(m => m.Map<Organization>(command))
            .Returns(entity);

        await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public async Task Handle_CallsRepositoryOnce()
    {
        var command = new CreateOrganizationCommand
        {
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10)
        };
        var entity = new Organization();

        MapperMock
            .Setup(m => m.Map<Organization>(command))
            .Returns(entity);

        await _handler.Handle(command, CancellationToken.None);

        OrganizationRepositoryMock.Verify(
            r => r.CreateAsync(entity, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenLogoProvided_SavesLogoPath()
    {
        var logo = new Mock<IFormFile>();
        var command = new CreateOrganizationCommand
        {
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10),
            Logo = logo.Object
        };
        var entity = new Organization();
        var logoPath = $"organizations/{Guid.NewGuid()}/logo.png";

        MapperMock
            .Setup(m => m.Map<Organization>(command))
            .Returns(entity);

        ImageStorageMock
            .Setup(s => s.SaveAsync(
                logo.Object,
                ImageStorageFolder.Organizations,
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredImage(logoPath, $"/images/{logoPath}", "image/png", 512));

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(logoPath, entity.LogoPath);
    }
}
