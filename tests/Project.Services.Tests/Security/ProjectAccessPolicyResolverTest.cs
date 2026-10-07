using ATMS.Application.Interfaces;
using ATMS.Application.Security;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Security;
using Moq;

namespace Project.Services.Tests.Security;

public sealed class ProjectAccessPolicyResolverTest
{
    private readonly Mock<ICommentRepository> _comments = new();
    private readonly Mock<IWorkProjectRepository> _workProjects = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly ProjectAccessPolicyResolver resolver;

    public ProjectAccessPolicyResolverTest()
    {
        _currentUser.SetupGet(user => user.Id).Returns(_userId);
        resolver = new ProjectAccessPolicyResolver(_comments.Object, _workProjects.Object, _currentUser.Object);
    }

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

    [Theory]
    [InlineData(true, ProjectPermissionEnum.CommentEdit)]
    [InlineData(false, ProjectPermissionEnum.CommentDelete)]
    public async Task ResolveAsync_CommentDelete_OwnNeedsEditAndOthersNeedDelete(
        bool own,
        ProjectPermissionEnum expectedPermission)
    {
        var request = new CommentScopedRequest(Guid.NewGuid(), Guid.NewGuid());
        _comments.Setup(repository => repository.GetAuthorIdAsync(
                request.ProjectId, request.CommentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(own ? _userId : Guid.NewGuid());

        var result = await resolver.ResolveAsync(
            ProjectAccessPolicy.CommentDelete,
            request,
            CancellationToken.None);

        Assert.Equal([expectedPermission], result);
    }

    [Fact]
    public async Task ResolveAsync_CommentDeleteOfMissingComment_LeavesNotFoundToValidator()
    {
        var request = new CommentScopedRequest(Guid.NewGuid(), Guid.NewGuid());
        _comments.Setup(repository => repository.GetAuthorIdAsync(
                request.ProjectId, request.CommentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var result = await resolver.ResolveAsync(
            ProjectAccessPolicy.CommentDelete,
            request,
            CancellationToken.None);

        Assert.Equal([ProjectPermissionEnum.CommentEdit], result);
    }

    [Fact]
    public async Task ResolveAsync_CommentDeleteWithoutCommentScopedRequest_ReturnsNoPermissions()
    {
        var result = await resolver.ResolveAsync(
            ProjectAccessPolicy.CommentDelete,
            new ProjectScopedRequest(Guid.NewGuid()),
            CancellationToken.None);

        Assert.Empty(result);
        _comments.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(nameof(RoleIds.OrgClientManager), true)]
    [InlineData(nameof(RoleIds.OrgClientViewer), true)]
    [InlineData(nameof(RoleIds.ProjectManager), false)]
    [InlineData(nameof(RoleIds.BusinessConsultant), false)]
    [InlineData(nameof(RoleIds.Developer), false)]
    public async Task ResolveAsync_ParticipantDelete_ClientManagerMayRemoveOnlyClients(string roleName, bool client)
    {
        var request = new ParticipantScopedRequest(Guid.NewGuid(), Guid.NewGuid());
        _workProjects.Setup(repository => repository.GetParticipantRoleIdAsync(
                request.ProjectId, request.ParticipantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(GetRoleId(roleName));

        var result = await resolver.ResolveAsync(
            ProjectAccessPolicy.ParticipantDelete,
            request,
            CancellationToken.None);

        Assert.Equal(
            client
                ? [ProjectPermissionEnum.ParticipantDelete, ProjectPermissionEnum.ParticipantDeleteClient]
                : [ProjectPermissionEnum.ParticipantDelete],
            result);
    }

    [Fact]
    public async Task ResolveAsync_ParticipantDeleteOfMissingParticipant_AsksForFullRight()
    {
        var request = new ParticipantScopedRequest(Guid.NewGuid(), Guid.NewGuid());

        var result = await resolver.ResolveAsync(
            ProjectAccessPolicy.ParticipantDelete,
            request,
            CancellationToken.None);

        Assert.Equal([ProjectPermissionEnum.ParticipantDelete], result);
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

    private sealed record CommentScopedRequest(Guid ProjectId, Guid CommentId) : IProjectCommentScopedRequest;

    private sealed record ParticipantScopedRequest(Guid ProjectId, Guid ParticipantId) : IProjectParticipantScopedRequest;
}
