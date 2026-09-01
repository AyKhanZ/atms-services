using ATMS.Application.Security;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using ATMS.Project.Services.Security;

namespace Project.Services.Tests.Security;

public sealed class ProjectAccessPolicyResolverTest
{
    private readonly ProjectAccessPolicyResolver resolver = new();

    [Theory]
    [InlineData(nameof(RoleIds.OrgClientManager), ProjectPermissionEnum.ParticipantInviteClient)]
    [InlineData(nameof(RoleIds.OrgClientViewer), ProjectPermissionEnum.ParticipantInviteClient)]
    [InlineData(nameof(RoleIds.ProjectManager), ProjectPermissionEnum.ParticipantInviteEmployee)]
    [InlineData(nameof(RoleIds.BusinessConsultant), ProjectPermissionEnum.ParticipantInviteEmployee)]
    [InlineData(nameof(RoleIds.Developer), ProjectPermissionEnum.ParticipantInviteEmployee)]
    public async Task ResolveAsync_ParticipantInvite_ReturnsPermissionForTargetRole(
        string roleName,
        ProjectPermissionEnum expectedPermission)
    {
        var request = new RoleScopedRequest(Guid.NewGuid(), GetRoleId(roleName));

        var result = await resolver.ResolveAsync(
            ProjectAccessPolicy.ParticipantInvite,
            request,
            CancellationToken.None);

        Assert.Equal([expectedPermission], result);
    }

    [Fact]
    public async Task ResolveAsync_ParticipantInviteWithoutRoleScopedRequest_ReturnsNoPermissions()
    {
        var request = new ProjectScopedRequest(Guid.NewGuid());

        var result = await resolver.ResolveAsync(
            ProjectAccessPolicy.ParticipantInvite,
            request,
            CancellationToken.None);

        Assert.Empty(result);
    }

    private static Guid GetRoleId(string roleName)
    {
        return roleName switch
        {
            nameof(RoleIds.ProjectManager) => RoleIds.ProjectManager,
            nameof(RoleIds.BusinessConsultant) => RoleIds.BusinessConsultant,
            nameof(RoleIds.Developer) => RoleIds.Developer,
            nameof(RoleIds.OrgClientManager) => RoleIds.OrgClientManager,
            nameof(RoleIds.OrgClientViewer) => RoleIds.OrgClientViewer,
            _ => throw new ArgumentOutOfRangeException(nameof(roleName), roleName, null)
        };
    }

    private sealed record ProjectScopedRequest(Guid ProjectId) : IProjectScopedRequest;

    private sealed record RoleScopedRequest(Guid ProjectId, Guid RoleId) : IProjectRoleScopedRequest;
}
