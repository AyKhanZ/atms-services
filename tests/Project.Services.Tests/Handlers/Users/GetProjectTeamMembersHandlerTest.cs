using System.Linq.Expressions;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Requests.Users;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.Users;
using Moq;

namespace Project.Services.Tests.Handlers.Users;

public class GetProjectTeamMembersHandlerTest : BaseHandlerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly GetProjectTeamMembersHandler _handler;

    public GetProjectTeamMembersHandlerTest()
    {
        _handler = new GetProjectTeamMembersHandler(
            _userRepositoryMock.Object,
            MapperMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyNonClientUsers()
    {
        var teamMember = new User { Id = Guid.NewGuid(), UserType = (int)UserTypeEnum.Employee };
        var client = new User { Id = Guid.NewGuid(), UserType = (int)UserTypeEnum.Client };
        var users = new[] { teamMember, client };
        var expected = new[] { new UserModel { Id = teamMember.Id } };

        _userRepositoryMock
            .Setup(repository => repository.GetManyAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<User, bool>>, CancellationToken>((predicate, _) =>
                Task.FromResult(users.Where(predicate.Compile()).ToList()));
        MapperMock
            .Setup(mapper => mapper.Map<UserModel[]>(
                It.Is<List<User>>(items => items.Count == 1 && items[0].Id == teamMember.Id)))
            .Returns(expected);

        var result = await _handler.Handle(new GetProjectTeamMembersRequest(), CancellationToken.None);

        Assert.Equal(expected, result);
    }
}
