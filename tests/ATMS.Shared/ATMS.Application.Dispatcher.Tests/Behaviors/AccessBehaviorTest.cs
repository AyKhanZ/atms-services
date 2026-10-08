using ATMS.Application.Dispatcher.Behaviors;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Interfaces;
using ATMS.Application.Security;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using MediatR;
using Moq;

namespace ATMS.Application.Dispatcher.Tests.Behaviors;

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
    public async Task Handle_AllDeclaredSystemPermissionAttributes_AllowsRequest()
    {
        _currentUser.SetupGet(user => user.Permissions)
            .Returns(new HashSet<string>
            {
                nameof(PermissionEnum.UserView),
                nameof(PermissionEnum.UserEdit)
            });
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
    public async Task Handle_PermissionsInsideOneAttribute_UsesOrSemantics()
    {
        _currentUser.SetupGet(user => user.Permissions)
            .Returns(new HashSet<string> { PermissionEnum.UserEdit.ToString() });
        var behavior = CreateBehavior<AlternativeSystemRequest>();

        var result = await behavior.Handle(new AlternativeSystemRequest(), Next, CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Handle_MultipleAttributes_UsesAndSemantics()
    {
        _currentUser.SetupGet(user => user.Permissions)
            .Returns(new HashSet<string> { nameof(PermissionEnum.UserView) });
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

    [Fact]
    public async Task Handle_ExceptSuperAdmin_DeniesSuperAdmin()
    {
        _currentUser.SetupGet(user => user.RoleId).Returns(RoleIds.SuperAdmin);
        var behavior = CreateBehavior<PersonalRequest>();

        var error = await Assert.ThrowsAsync<AuthException>(
            () => behavior.Handle(new PersonalRequest(), Next, CancellationToken.None));

        Assert.Equal(AuthErrorTypeEnum.Forbidden, error.AuthErrorType);
    }

    [Fact]
    public async Task Handle_ExceptSuperAdmin_AllowsOtherRolesWithoutPermission()
    {
        var behavior = CreateBehavior<PersonalRequest>();

        Assert.Equal("handled", await behavior.Handle(new PersonalRequest(), Next, CancellationToken.None));
    }

    private AccessBehavior<TRequest, string> CreateBehavior<TRequest>() where TRequest : notnull
        => new(_currentUser.Object);

    private static Task<string> Next(CancellationToken _) => Task.FromResult("handled");

    private sealed record PublicRequest : IRequest<string>;

    [Access(PermissionEnum.UserView)]
    [Access(PermissionEnum.UserEdit)]
    private sealed record SystemRequest : IRequest<string>;

    [Access(PermissionEnum.UserView, PermissionEnum.UserEdit)]
    private sealed record AlternativeSystemRequest : IRequest<string>;

    [ExceptSuperAdminAccess]
    private sealed record PersonalRequest : IRequest<string>;

}
