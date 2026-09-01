using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Interfaces;
using ATMS.Application.Security;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using ATMS.Project.Contracts.Requests.WorkTickets;
using ATMS.Project.Services.Dispatcher.Behaviors;
using ATMS.Project.Services.Security.Interfaces;
using MediatR;
using Moq;

namespace Project.Services.Tests.Dispatcher;

public class ProjectAccessBehaviorTest
{
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IProjectPermissionService> _projectPermissions = new();
    private readonly Mock<IProjectAccessPolicyResolver> _policyResolver = new();

    public ProjectAccessBehaviorTest()
    {
        _currentUser.SetupGet(user => user.RoleId).Returns(Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_RequestWithoutProjectAccessRequirement_ContinuesPipeline()
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
        SetupPermissions(projectId, ProjectPermissionEnum.ProjectEdit);
        var behavior = CreateBehavior<ProjectRequest>();

        var result = await behavior.Handle(new ProjectRequest(projectId), Next, CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Handle_MissingProjectPermission_DeniesRequest()
    {
        var projectId = Guid.NewGuid();
        SetupPermissions(projectId);
        var behavior = CreateBehavior<ProjectRequest>();

        await Assert.ThrowsAsync<AuthException>(() =>
            behavior.Handle(new ProjectRequest(projectId), Next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SuperAdmin_BypassesProjectPermissionCheck()
    {
        _currentUser.SetupGet(user => user.RoleId).Returns(RoleIds.SuperAdmin);
        var behavior = CreateBehavior<ProjectRequest>();

        var result = await behavior.Handle(new ProjectRequest(Guid.NewGuid()), Next, CancellationToken.None);

        Assert.Equal("handled", result);
        _projectPermissions.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WorkTicketGetRequest_RequiresProjectViewPermissionForItsProject()
    {
        var projectId = Guid.NewGuid();
        SetupPermissions(projectId, ProjectPermissionEnum.ProjectView);
        var behavior = CreateBehavior<GetWorkTicketRequest>();

        var result = await behavior.Handle(
            new GetWorkTicketRequest { ProjectId = projectId, WorkTicketId = Guid.NewGuid() },
            Next,
            CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Handle_WorkTicketListWithoutProjectMembership_DeniesRequest()
    {
        var projectId = Guid.NewGuid();
        SetupPermissions(projectId);
        var behavior = CreateBehavior<GetWorkTicketsRequest>();

        await Assert.ThrowsAsync<AuthException>(() => behavior.Handle(
            new GetWorkTicketsRequest { ProjectId = projectId },
            Next,
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PermissionsInOneAttribute_UsesOrSemantics()
    {
        var projectId = Guid.NewGuid();
        SetupPermissions(projectId, ProjectPermissionEnum.TicketEdit);
        var behavior = CreateBehavior<AlternativeProjectRequest>();

        var result = await behavior.Handle(new AlternativeProjectRequest(projectId), Next, CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Handle_MultipleProjectAttributes_RequiresEveryAttribute()
    {
        var projectId = Guid.NewGuid();
        SetupPermissions(projectId, ProjectPermissionEnum.ProjectView);
        var behavior = CreateBehavior<CumulativeProjectRequest>();

        await Assert.ThrowsAsync<AuthException>(() => behavior.Handle(
            new CumulativeProjectRequest(projectId),
            Next,
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ProjectAccessPolicy_RequiresResolvedPermission()
    {
        var projectId = Guid.NewGuid();
        SetupPermissions(projectId, ProjectPermissionEnum.ParticipantInviteClient);
        SetupPolicy(ProjectAccessPolicy.ParticipantInvite, ProjectPermissionEnum.ParticipantInviteClient);
        var behavior = CreateBehavior<InviteProjectRequest>();

        var result = await behavior.Handle(
            new InviteProjectRequest(projectId),
            Next,
            CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Handle_ProjectAccessPolicyWithoutResolvedPermission_DeniesRequest()
    {
        var projectId = Guid.NewGuid();
        SetupPermissions(projectId, ProjectPermissionEnum.ParticipantInviteClient);
        SetupPolicy(ProjectAccessPolicy.ParticipantInvite, ProjectPermissionEnum.ParticipantInviteEmployee);
        var behavior = CreateBehavior<InviteProjectRequest>();

        await Assert.ThrowsAsync<AuthException>(() =>
            behavior.Handle(new InviteProjectRequest(projectId), Next, CancellationToken.None));
    }

    private void SetupPermissions(Guid projectId, params ProjectPermissionEnum[] permissions)
    {
        _projectPermissions
            .Setup(provider => provider.GetPermissionCodesAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions.Select(permission => permission.ToString()).ToHashSet());
    }

    private void SetupPolicy(ProjectAccessPolicy policy, params ProjectPermissionEnum[] permissions)
    {
        _policyResolver
            .Setup(resolver => resolver.ResolveAsync(
                policy,
                It.IsAny<IProjectScopedRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);
    }

    private ProjectAccessBehavior<TRequest, string> CreateBehavior<TRequest>() where TRequest : notnull
        => new(_currentUser.Object, _projectPermissions.Object, _policyResolver.Object);

    private static Task<string> Next(CancellationToken _) => Task.FromResult("handled");

    private sealed record PublicRequest : IRequest<string>;

    [ProjectAccess(ProjectPermissionEnum.ProjectEdit)]
    private sealed record ProjectRequest(Guid ProjectId) : IRequest<string>, IProjectScopedRequest;

    [ProjectAccess(ProjectPermissionEnum.TicketEdit, ProjectPermissionEnum.TicketDelete)]
    private sealed record AlternativeProjectRequest(Guid ProjectId) : IRequest<string>, IProjectScopedRequest;

    [ProjectAccess(ProjectPermissionEnum.ProjectView)]
    [ProjectAccess(ProjectPermissionEnum.TicketEdit)]
    private sealed record CumulativeProjectRequest(Guid ProjectId) : IRequest<string>, IProjectScopedRequest;

    [ProjectAccess(ProjectAccessPolicy.ParticipantInvite)]
    private sealed record InviteProjectRequest(Guid ProjectId) :
        IRequest<string>,
        IProjectScopedRequest;
}
