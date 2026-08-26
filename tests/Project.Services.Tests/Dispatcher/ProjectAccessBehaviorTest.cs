using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Interfaces;
using ATMS.Application.Security;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using ATMS.Project.Services.Dispatcher.Behaviors;
using ATMS.Project.Services.Security.Interfaces;
using MediatR;
using Moq;

namespace Project.Services.Tests.Dispatcher;

public class ProjectAccessBehaviorTest
{
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IProjectPermissionService> _projectPermissions = new();

    public ProjectAccessBehaviorTest()
    {
        _currentUser.SetupGet(user => user.RoleId).Returns(Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_RequestWithoutProjectAccessAttribute_ContinuesPipeline()
    {
        var behavior = CreateBehavior<PublicRequest>();

        var result = await behavior.Handle(new PublicRequest(), Next, CancellationToken.None);

        Assert.Equal("handled", result);
        _projectPermissions.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_RequiredProjectPermission_AllowsRequest()
    {
        var projectId = Guid.NewGuid();
        _projectPermissions
            .Setup(provider => provider.HasAnyPermissionAsync(
                projectId,
                It.IsAny<IReadOnlyCollection<ProjectPermissionEnum>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var behavior = CreateBehavior<ProjectRequest>();

        var result = await behavior.Handle(new ProjectRequest(projectId), Next, CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Handle_MissingProjectPermission_DeniesRequest()
    {
        var projectId = Guid.NewGuid();
        _projectPermissions
            .Setup(provider => provider.HasAnyPermissionAsync(
                projectId,
                It.IsAny<IReadOnlyCollection<ProjectPermissionEnum>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var behavior = CreateBehavior<ProjectRequest>();

        await Assert.ThrowsAsync<AuthException>(() =>
            behavior.Handle(new ProjectRequest(projectId), Next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SuperAdmin_BypassesProjectPermissionCheck()
    {
        _currentUser.SetupGet(user => user.RoleId).Returns(RoleIds.SuperAdmin);
        var behavior = CreateBehavior<ProjectRequest>();

        var result = await behavior.Handle(
            new ProjectRequest(Guid.NewGuid()),
            Next,
            CancellationToken.None);

        Assert.Equal("handled", result);
        _projectPermissions.VerifyNoOtherCalls();
    }

    private ProjectAccessBehavior<TRequest, string> CreateBehavior<TRequest>() where TRequest : notnull
        => new(_currentUser.Object, _projectPermissions.Object);

    private static Task<string> Next(CancellationToken _) => Task.FromResult("handled");

    private sealed record PublicRequest : IRequest<string>;

    [ProjectAccess(ProjectPermissionEnum.ProjectEdit)]
    private sealed record ProjectRequest(Guid ProjectId) : IRequest<string>, IProjectScopedRequest;
}
