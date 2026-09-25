using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Security;
using Moq;

namespace Project.Services.Tests.Security;

public sealed class ProjectPermissionServiceTest
{
    public static TheoryData<Guid> ClientRoles => new() { RoleIds.Client, RoleIds.ClientManager };

    [Theory]
    [MemberData(nameof(ClientRoles))]
    public async Task IsClient_GlobalClientRole_IsClientWithoutRepositoryLookup(Guid roleId)
    {
        var user = new Mock<ICurrentUser>();
        user.SetupGet(current => current.RoleId).Returns(roleId);
        var repository = new Mock<IProjectPermissionRepository>();
        var service = new ProjectPermissionService(user.Object, repository.Object, new Mock<ICacheService>().Object);

        var isClient = await service.IsClientAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(isClient);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task IsClient_ProjectClientRole_IsClient()
    {
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = new Mock<ICurrentUser>();
        user.SetupGet(current => current.RoleId).Returns(RoleIds.Employee);
        user.SetupGet(current => current.Id).Returns(userId);
        var repository = new Mock<IProjectPermissionRepository>();
        repository.Setup(repo => repo.HasClientRoleAsync(projectId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = new ProjectPermissionService(user.Object, repository.Object, new Mock<ICacheService>().Object);

        var isClient = await service.IsClientAsync(projectId, CancellationToken.None);

        Assert.True(isClient);
    }

    [Fact]
    public async Task HasAnyPermission_SuperAdminBypassesProjectMembership()
    {
        var user = new Mock<ICurrentUser>();
        user.SetupGet(current => current.RoleId).Returns(RoleIds.SuperAdmin);
        var cache = new Mock<ICacheService>();
        var repository = new Mock<IProjectPermissionRepository>();
        var service = new ProjectPermissionService(user.Object, repository.Object, cache.Object);

        var allowed = await service.HasAnyPermissionAsync(
            Guid.NewGuid(), [ProjectPermissionEnum.ProjectView], CancellationToken.None);

        Assert.True(allowed);
        cache.VerifyNoOtherCalls();
        repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HasAnyPermission_OrdinaryUserUsesProjectPermissions(bool hasView)
    {
        var user = new Mock<ICurrentUser>();
        user.SetupGet(current => current.RoleId).Returns(RoleIds.Employee);
        user.SetupGet(current => current.Id).Returns(Guid.NewGuid());
        var cache = new Mock<ICacheService>();
        cache.Setup(service => service.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<string[]>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasView ? [nameof(ProjectPermissionEnum.ProjectView)] : []);
        var service = new ProjectPermissionService(
            user.Object,
            new Mock<IProjectPermissionRepository>().Object,
            cache.Object);

        var allowed = await service.HasAnyPermissionAsync(
            Guid.NewGuid(), [ProjectPermissionEnum.ProjectView], CancellationToken.None);

        Assert.Equal(hasView, allowed);
    }
}
