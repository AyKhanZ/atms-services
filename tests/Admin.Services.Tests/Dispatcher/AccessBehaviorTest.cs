using ATMS.Application.Dispatcher.Behaviors;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Interfaces;
using ATMS.Application.Security;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using MediatR;
using Moq;

namespace Admin.Services.Tests.Dispatcher;

public class AccessBehaviorTest
{
    private readonly Mock<ICurrentUser> _currentUser = new();
    public AccessBehaviorTest()
    {
        _currentUser.SetupGet(user => user.RoleId).Returns(Guid.NewGuid());
        _currentUser.SetupGet(user => user.Permissions).Returns(new HashSet<string>());
    }

    [Fact]
    public async Task Handle_RequestWithoutAccessAttributes_ContinuesPipeline()
    {
        var behavior = CreateBehavior<PublicRequest>();

        var result = await behavior.Handle(new PublicRequest(), Next, CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Handle_AnyDeclaredSystemPermission_AllowsRequest()
    {
        _currentUser.SetupGet(user => user.Permissions)
            .Returns(new HashSet<string> { PermissionEnum.UserEdit.ToString() });
        var behavior = CreateBehavior<SystemRequest>();

        var result = await behavior.Handle(new SystemRequest(), Next, CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Handle_MissingSystemPermission_DeniesRequest()
    {
        var behavior = CreateBehavior<SystemRequest>();

        await Assert.ThrowsAsync<AuthException>(
            () => behavior.Handle(new SystemRequest(), Next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SuperAdmin_BypassesPermissionChecks()
    {
        _currentUser.SetupGet(user => user.RoleId).Returns(RoleIds.SuperAdmin);
        var behavior = CreateBehavior<SystemRequest>();

        var result = await behavior.Handle(
            new SystemRequest(),
            Next,
            CancellationToken.None);

        Assert.Equal("handled", result);
    }

    private AccessBehavior<TRequest, string> CreateBehavior<TRequest>() where TRequest : notnull
        => new(_currentUser.Object);

    private static Task<string> Next(CancellationToken _) => Task.FromResult("handled");

    private sealed record PublicRequest : IRequest<string>;

    [Access(PermissionEnum.UserView)]
    [Access(PermissionEnum.UserEdit)]
    private sealed record SystemRequest : IRequest<string>;

}
