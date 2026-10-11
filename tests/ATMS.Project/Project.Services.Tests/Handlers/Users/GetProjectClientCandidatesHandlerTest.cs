using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Requests.Users;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Users;
using Moq;

namespace Project.Services.Tests.Handlers.Users;

public class GetProjectClientCandidatesHandlerTest : BaseHandlerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly GetProjectClientCandidatesHandler _handler;

    public GetProjectClientCandidatesHandlerTest()
    {
        _handler = new GetProjectClientCandidatesHandler(
            WorkProjectRepositoryMock.Object,
            _userRepositoryMock.Object,
            MapperMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsActiveClientsOfTheProjectOrganization()
    {
        var projectId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var client = new User
        {
            Id = Guid.NewGuid(),
            UserType = (int)UserTypeEnum.Client,
            OrganizationId = organizationId,
            IsActive = true
        };
        var inactive = new User
        {
            Id = Guid.NewGuid(),
            UserType = (int)UserTypeEnum.Client,
            OrganizationId = organizationId,
            IsActive = false
        };
        var otherOrganization = new User
        {
            Id = Guid.NewGuid(),
            UserType = (int)UserTypeEnum.Client,
            OrganizationId = Guid.NewGuid(),
            IsActive = true
        };
        var employee = new User
        {
            Id = Guid.NewGuid(),
            UserType = (int)UserTypeEnum.Employee,
            OrganizationId = organizationId,
            IsActive = true
        };
        var expected = new[] { new UserModel { Id = client.Id } };

        WorkProjectRepositoryMock
            .Setup(repository => repository.FindRootAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkProject { Id = projectId, OrganizationId = organizationId });
        _userRepositoryMock
            .Setup(repository => repository.GetManyAsync(
                It.IsAny<ACriteria<User>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ACriteria<User>, CancellationToken>((criteria, _) =>
                Task.FromResult(criteria.Apply(new[] { client, inactive, otherOrganization, employee }.AsQueryable()).ToList()));
        MapperMock
            .Setup(mapper => mapper.Map<UserModel[]>(
                It.Is<List<User>>(items => items.Count == 1 && items[0].Id == client.Id)))
            .Returns(expected);

        var result = await _handler.Handle(
            new GetProjectClientCandidatesRequest { ProjectId = projectId },
            CancellationToken.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Handle_WhenProjectHasNoOrganization_ReturnsEmpty()
    {
        var projectId = Guid.NewGuid();
        WorkProjectRepositoryMock
            .Setup(repository => repository.FindRootAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkProject { Id = projectId });

        var result = await _handler.Handle(
            new GetProjectClientCandidatesRequest { ProjectId = projectId },
            CancellationToken.None);

        Assert.Empty(result);
        _userRepositoryMock.Verify(
            repository => repository.GetManyAsync(It.IsAny<ACriteria<User>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProjectIsMissing_ThrowsNotFound()
    {
        var projectId = Guid.NewGuid();
        WorkProjectRepositoryMock
            .Setup(repository => repository.FindRootAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkProject?)null);

        await Assert.ThrowsAsync<EntityException>(() => _handler.Handle(
            new GetProjectClientCandidatesRequest { ProjectId = projectId },
            CancellationToken.None));
    }
}
