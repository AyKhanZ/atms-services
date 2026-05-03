using System.Linq.Expressions;
using ATMS.Application.Exceptions.Entity;
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
        _handler = new UpdateOrganizationHandler(OrganizationRepositoryMock.Object);
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
            Voen = Faker.Random.AlphaNumeric(10),
            LogoPath = Faker.Random.AlphaNumeric(100)
        };

        OrganizationRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(command.Title, entity.Title);
        Assert.Equal(command.Voen, entity.Voen);
        Assert.Equal(command.LogoPath, entity.LogoPath);
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
            Voen = Faker.Random.AlphaNumeric(10),
            LogoPath = null
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
            Voen = Faker.Random.AlphaNumeric(10),
            LogoPath = Faker.Random.AlphaNumeric(100)
        };

        await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));

        OrganizationRepositoryMock.Verify(r => r.SaveAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }
}