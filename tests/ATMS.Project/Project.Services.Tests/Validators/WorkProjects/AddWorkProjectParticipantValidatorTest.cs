using ATMS.Data.Constants;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.WorkProjects;
using Moq;

namespace Project.Services.Tests.Validators.WorkProjects;

public class AddWorkProjectParticipantValidatorTest : BaseValidatorTest
{
    private readonly Mock<IWorkProjectInvitationRepository> _invitationRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoleRepository> _roleRepositoryMock = new();
    private readonly WorkProject _project = new()
    {
        Id = Guid.NewGuid(),
        OrganizationId = Guid.NewGuid()
    };
    private readonly List<WorkProjectInvitation> _invitations = [];
    private readonly AddWorkProjectParticipantValidator _validator;

    public AddWorkProjectParticipantValidatorTest()
    {
        WorkProjectsRepositoryMock
            .Setup(x => x.FindAsync(_project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_project);
        _invitationRepositoryMock
            .Setup(x => x.GetLivePendingAsync(_project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_invitations);
        _userRepositoryMock
            .Setup(x => x.GetManyAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<ACriteria<User>>(),
                It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<Guid>, ACriteria<User>, CancellationToken>((ids, _, _) => Task.FromResult(
                ids.Select(id => new User
                {
                    Id = id,
                    UserType = (int)UserTypeEnum.Client,
                    OrganizationId = _project.OrganizationId
                }).ToList()));
        _roleRepositoryMock
            .Setup(x => x.GetManyAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<Guid>, CancellationToken>((ids, _) => Task.FromResult(
                ids.Select(id => new Role { Id = id }).ToList()));

        _validator = new AddWorkProjectParticipantValidator(
            WorkProjectsRepositoryMock.Object,
            _invitationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object);
    }

    [Fact]
    public async Task Validate_WhenThereIsRoom_PassesValidation()
    {
        AddParticipants(18);
        AddInvitations(1);

        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.True(result.IsValid);
    }

    // Invitations become participants within seconds: counting only participants let the project
    // grow past the limit once the pending ones arrived.
    [Theory]
    [InlineData(20, 0)]
    [InlineData(19, 1)]
    [InlineData(10, 10)]
    public async Task Validate_WhenParticipantsAndPendingInvitationsReachLimit_FailsOnUser(int participants, int invitations)
    {
        AddParticipants(participants);
        AddInvitations(invitations);

        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(AddWorkProjectParticipantCommand.UserId));
    }

    [Fact]
    public async Task Validate_WhenTheUserIsInactive_FailsOnUser()
    {
        var command = CreateCommand();
        _userRepositoryMock
            .Setup(x => x.GetManyAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<ACriteria<User>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new User
                {
                    Id = command.UserId,
                    UserType = (int)UserTypeEnum.Employee,
                    IsActive = false
                }
            ]);

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.UserId));
    }

    private void AddParticipants(int count)
    {
        for (var i = 0; i < count; i++)
        {
            _project.WorkProjectParticipants.Add(new WorkProjectParticipant { UserId = Guid.NewGuid() });
        }
    }

    private void AddInvitations(int count)
    {
        for (var i = 0; i < count; i++)
        {
            _invitations.Add(new WorkProjectInvitation { NormalizedEmail = $"INVITED{i}@CLIENT.AZ" });
        }
    }

    private AddWorkProjectParticipantCommand CreateCommand()
    {
        return new AddWorkProjectParticipantCommand
        {
            ProjectId = _project.Id,
            UserId = Guid.NewGuid(),
            RoleId = RoleIds.OrgClientViewer
        };
    }
}
