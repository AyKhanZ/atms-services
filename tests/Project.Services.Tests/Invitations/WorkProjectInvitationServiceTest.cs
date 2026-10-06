using ATMS.Application.Interfaces;
using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Invitations;
using ATMS.Project.Services.Security.Interfaces;
using Moq;

namespace Project.Services.Tests.Invitations;

public class WorkProjectInvitationServiceTest
{
    private readonly Mock<IWorkProjectInvitationRepository> _invitationRepositoryMock = new();
    private readonly Mock<IWorkProjectRepository> _workProjectRepositoryMock = new();
    private readonly Mock<IAuditActorScope> _auditActorMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<IProjectPermissionService> _permissionServiceMock = new();
    private readonly Guid _organizationId = Guid.NewGuid();
    private readonly List<WorkProjectInvitation> _invitations = [];

    public WorkProjectInvitationServiceTest()
    {
        _invitationRepositoryMock
            .Setup(x => x.GetPendingByEmailAsync("NIGAR@CLIENT.AZ", It.IsAny<CancellationToken>()))
            .ReturnsAsync(_invitations);
    }

    [Fact]
    public async Task SettlePendingAsync_WhenClientOfProjectOrganization_AddsClientViewerParticipant()
    {
        var project = AddProject(_organizationId);
        var invitation = AddInvitation(project.Id);
        var user = CreateUser();

        await CreateService().SettlePendingAsync(user, CancellationToken.None);

        var participant = Assert.Single(project.WorkProjectParticipants);
        Assert.Equal(user.Id, participant.UserId);
        Assert.Equal(RoleIds.OrgClientViewer, Assert.Single(participant.WorkProjectParticipantRoles).RoleId);
        Assert.Equal((int)WorkProjectInvitationStatusEnum.Accepted, invitation.Status);
        Assert.NotNull(invitation.ProcessedAt);
        _workProjectRepositoryMock.Verify(x => x.Touch(project), Times.Once);
        _permissionServiceMock.Verify(x => x.RemoveUserPermissionsAsync(
            project.Id,
            user.Id,
            It.IsAny<CancellationToken>()), Times.Once);
        VerifyProjectCacheRemoved(project.Id);
    }

    public static TheoryData<string> RejectedCases => new()
    {
        "another organization",
        "employee",
        "already participant",
        "internal project",
        "project deleted"
    };

    [Theory]
    [MemberData(nameof(RejectedCases))]
    public async Task SettlePendingAsync_WhenUserCannotJoinAsClient_RejectsWithoutParticipant(string reason)
    {
        var user = CreateUser();
        WorkProject? project = null;
        switch (reason)
        {
            case "another organization":
                project = AddProject(Guid.NewGuid());
                break;
            case "employee":
                project = AddProject(_organizationId);
                user.UserType = (int)UserTypeEnum.Employee;
                break;
            case "already participant":
                project = AddProject(_organizationId);
                project.WorkProjectParticipants.Add(new WorkProjectParticipant { UserId = user.Id });
                break;
            case "internal project":
                project = AddProject(null);
                break;
        }

        var invitation = AddInvitation(project?.Id ?? Guid.NewGuid());
        var participantsBefore = project?.WorkProjectParticipants.Count ?? 0;

        await CreateService().SettlePendingAsync(user, CancellationToken.None);

        Assert.Equal((int)WorkProjectInvitationStatusEnum.Rejected, invitation.Status);
        Assert.NotNull(invitation.ProcessedAt);
        Assert.Equal(participantsBefore, project?.WorkProjectParticipants.Count ?? 0);
        _workProjectRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        _permissionServiceMock.Verify(x => x.RemoveUserPermissionsAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
        VerifyProjectCacheRemoved(invitation.WorkProjectId);
    }

    // Two people can invite the same email into two projects; history must name each of them.
    [Fact]
    public async Task SettlePendingAsync_WhenInvitedToSeveralProjects_SavesEachUnderItsInviter()
    {
        var firstInviter = Guid.NewGuid();
        var secondInviter = Guid.NewGuid();
        var first = AddProject(_organizationId);
        var second = AddProject(_organizationId);
        AddInvitation(first.Id, firstInviter);
        AddInvitation(second.Id, secondInviter);
        var steps = new List<string>();
        _auditActorMock
            .Setup(x => x.ActAs(It.IsAny<Guid>()))
            .Callback<Guid>(id => steps.Add(id == firstInviter ? "act:first" : "act:second"));
        _workProjectRepositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("save"))
            .Returns(Task.CompletedTask);

        await CreateService().SettlePendingAsync(CreateUser(), CancellationToken.None);

        Assert.Equal(["act:first", "save", "act:second", "save"], steps);
        Assert.Single(first.WorkProjectParticipants);
        Assert.Single(second.WorkProjectParticipants);
    }

    [Fact]
    public async Task SettlePendingAsync_WhenNothingIsPending_SavesNothing()
    {
        await CreateService().SettlePendingAsync(CreateUser(), CancellationToken.None);

        _workProjectRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        _auditActorMock.Verify(x => x.ActAs(It.IsAny<Guid>()), Times.Never);
    }

    private WorkProject AddProject(Guid? organizationId)
    {
        var project = new WorkProject
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId
        };
        _workProjectRepositoryMock
            .Setup(x => x.FindAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        return project;
    }

    private WorkProjectInvitation AddInvitation(Guid projectId, Guid? inviterId = null)
    {
        var invitation = new WorkProjectInvitation
        {
            Id = Guid.NewGuid(),
            WorkProjectId = projectId,
            NormalizedEmail = "NIGAR@CLIENT.AZ",
            RoleId = RoleIds.OrgClientViewer,
            Status = (int)WorkProjectInvitationStatusEnum.Pending,
            InvitedById = inviterId ?? Guid.NewGuid()
        };
        _invitations.Add(invitation);
        return invitation;
    }

    private User CreateUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "Nigar@Client.az",
            NormalizedEmail = "NIGAR@CLIENT.AZ",
            OrganizationId = _organizationId,
            UserType = (int)UserTypeEnum.Client
        };
    }

    private void VerifyProjectCacheRemoved(Guid projectId)
    {
        foreach (var language in SupportedLanguages.All)
        {
            _cacheMock.Verify(
                x => x.RemoveAsync(CacheKeys.Project.ProjectById(projectId, language), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    private WorkProjectInvitationService CreateService()
    {
        return new WorkProjectInvitationService(
            _invitationRepositoryMock.Object,
            _workProjectRepositoryMock.Object,
            _auditActorMock.Object,
            _cacheMock.Object,
            _permissionServiceMock.Object);
    }
}
