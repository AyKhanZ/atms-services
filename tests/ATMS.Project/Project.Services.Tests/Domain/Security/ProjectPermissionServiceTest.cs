using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Security;
using Moq;

namespace Project.Services.Tests.Domain.Security;

public sealed class ProjectPermissionServiceTest
{
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
