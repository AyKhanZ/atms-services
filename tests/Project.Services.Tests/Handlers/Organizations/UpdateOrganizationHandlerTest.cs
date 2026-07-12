using System.Linq.Expressions;
using ATMS.Application.Exceptions.Entity;
using ATMS.Infrastructure.Images;
using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.Organizations;
using Moq;

namespace Project.Services.Tests.Handlers.Organizations;

public class UpdateOrganizationHandlerTest : BaseHandlerTest
{
    private readonly UpdateOrganizationHandler _handler;

    public UpdateOrganizationHandlerTest()
    {
        _handler = new UpdateOrganizationHandler(ImageStorageMock.Object, OrganizationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_UpdatesEntityAndSaves()
    {
        var entity = new Organization
        {
            Id = Guid.NewGuid(),
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10)
        };
        var command = new UpdateOrganizationCommand
        {
            Id = entity.Id,
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10)
        };

        OrganizationRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(command.Title, entity.Title);
        Assert.Equal(command.Voen, entity.Voen);
        OrganizationRepositoryMock.Verify(r => r.SaveAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenLogoPathIsNull_DoesNotUpdateLogo()
    {
        var originalLogo = Faker.Random.AlphaNumeric(100);
        var entity = new Organization { Id = Guid.NewGuid(), LogoPath = originalLogo };
        var command = new UpdateOrganizationCommand
        {
            Id = entity.Id,
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10)
        };

        OrganizationRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(originalLogo, entity.LogoPath);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsEntityException()
    {
        OrganizationRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        var command = new UpdateOrganizationCommand
        {
            Id = Guid.NewGuid(),
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10)
        };

        await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));

        OrganizationRepositoryMock.Verify(r => r.SaveAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenLogoProvided_UpdatesLogoAndDeletesOldLogo()
    {
        var oldLogo = "organizations/old-logo.png";
        var newLogo = "organizations/new-logo.png";
        var entity = new Organization
        {
            Id = Guid.NewGuid(),
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10),
            LogoPath = oldLogo
        };
        var logo = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
        var command = new UpdateOrganizationCommand
        {
            Id = entity.Id,
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10),
            Logo = logo.Object
        };

        OrganizationRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        ImageStorageMock
            .Setup(s => s.SaveAsync(logo.Object, ImageStorageFolder.Organizations, entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredImage(newLogo, "http://localhost/images/organizations/new-logo.png", "image/png", 512));

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(newLogo, entity.LogoPath);
        ImageStorageMock.Verify(s => s.DeleteAsync(oldLogo, It.IsAny<CancellationToken>()), Times.Once);
    }
}