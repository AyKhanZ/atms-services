using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.Organizations;
using Moq;

namespace Project.Services.Tests.Handlers.Organizations;

public class CreateOrganizationHandlerTest : BaseHandlerTest
{
    private readonly CreateOrganizationHandler _handler;

    public CreateOrganizationHandlerTest()
    {
        _handler = new CreateOrganizationHandler(
            MapperMock.Object,
            OrganizationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsNewGuid()
    {
        var command = new CreateOrganizationCommand
        {
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10),
            LogoPath = Faker.Random.AlphaNumeric(100)
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
            Voen = Faker.Random.AlphaNumeric(10),
            LogoPath = Faker.Random.AlphaNumeric(100)
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
            Voen = Faker.Random.AlphaNumeric(10),
            LogoPath = Faker.Random.AlphaNumeric(100)
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
}