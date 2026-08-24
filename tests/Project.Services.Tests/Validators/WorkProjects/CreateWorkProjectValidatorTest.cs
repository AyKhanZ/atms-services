using System.Linq.Expressions;
using ATMS.Data.Constants;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.WorkProjects;
using Moq;

namespace Project.Services.Tests.Validators.WorkProjects;

public class CreateWorkProjectValidatorTest : BaseValidatorTest
{
    private readonly Mock<IWorkProjectRepository> _workProjectRepositoryMock = new();
    private readonly Mock<IDictionariesRepository> _dictionariesRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoleRepository> _roleRepositoryMock = new();
    private readonly CreateWorkProjectValidator _validator;

    public CreateWorkProjectValidatorTest()
    {
        _workProjectRepositoryMock
            .Setup(x => x.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        OrganizationRepositoryMock
            .Setup(x => x.IsExistAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _dictionariesRepositoryMock
            .Setup(x => x.IsProjectTypeExistAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _dictionariesRepositoryMock
            .Setup(x => x.IsProjectKindExistAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _dictionariesRepositoryMock
            .Setup(x => x.IsProjectStatusExistAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepositoryMock
            .Setup(x => x.GetManyAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<ACriteria<User>>(),
                It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<Guid>, ACriteria<User>, CancellationToken>((ids, criteria, _) => Task.FromResult(
                criteria.Apply(ids.Select(id => new User { Id = id }).AsQueryable()).ToList()));
        _roleRepositoryMock
            .Setup(x => x.GetManyAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<Guid>, CancellationToken>((ids, _) => Task.FromResult(
                ids.Select(id => new Role { Id = id }).ToList()));

        _validator = new CreateWorkProjectValidator(
            _workProjectRepositoryMock.Object,
            OrganizationRepositoryMock.Object,
            _dictionariesRepositoryMock.Object,
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object);
    }

    [Fact]
    public async Task Validate_WhenCommandIsValid_PassesValidation()
    {
        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenOrganizationAndParticipantsAreEmpty_PassesValidation()
    {
        var command = CreateCommand();
        command.ProjectKindId = (int)ProjectKindEnum.Internal;
        command.OrganizationId = null;
        command.Participants = [];

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenInternalParticipantExistsWithoutOrganization_PassesValidation()
    {
        var command = CreateCommand();
        command.ProjectKindId = (int)ProjectKindEnum.Internal;
        command.OrganizationId = null;
        command.Participants =
        [
            new WorkProjectParticipantCommand
            {
                UserId = Guid.NewGuid(),
                RoleId = RoleIds.Developer
            }
        ];

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(ProjectKindEnum.Support)]
    [InlineData(ProjectKindEnum.External)]
    [InlineData(ProjectKindEnum.OneTime)]
    public async Task Validate_WhenNonInternalProjectHasNoOrganization_FailsValidation(
        ProjectKindEnum projectKind)
    {
        var command = CreateCommand();
        command.ProjectKindId = (int)projectKind;
        command.OrganizationId = null;

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.OrganizationId));
    }

    [Theory]
    [InlineData(ProjectKindEnum.Support)]
    [InlineData(ProjectKindEnum.External)]
    [InlineData(ProjectKindEnum.OneTime)]
    public async Task Validate_WhenNonInternalProjectHasNoParticipants_FailsValidation(
        ProjectKindEnum projectKind)
    {
        var command = CreateCommand();
        command.ProjectKindId = (int)projectKind;
        command.Participants = [];

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.Participants));
    }

    [Fact]
    public async Task Validate_WhenInternalProjectHasOrganization_FailsValidation()
    {
        var command = CreateCommand();
        command.ProjectKindId = (int)ProjectKindEnum.Internal;

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.OrganizationId));
    }

    [Fact]
    public async Task Validate_WhenInternalProjectHasNoOrganizationAndHasTeamParticipant_PassesValidation()
    {
        var command = CreateCommand();
        command.ProjectKindId = (int)ProjectKindEnum.Internal;
        command.OrganizationId = null;
        command.Participants =
        [
            new WorkProjectParticipantCommand
            {
                UserId = Guid.NewGuid(),
                RoleId = RoleIds.ProjectManager
            }
        ];

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenClientParticipantUsesClientRoleAndBelongsToOrganization_PassesValidation()
    {
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupUser(userId, UserTypeEnum.Client, organizationId);
        var command = CreateCommand();
        command.OrganizationId = organizationId;
        command.Participants =
        [
            new WorkProjectParticipantCommand
            {
                UserId = userId,
                RoleId = RoleIds.OrgClientViewer
            }
        ];

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validate_WhenParticipantRoleDoesNotMatchUserSide_FailsValidation(bool isClient)
    {
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupUser(
            userId,
            isClient ? UserTypeEnum.Client : UserTypeEnum.Employee,
            isClient ? organizationId : null);
        var command = CreateCommand();
        command.OrganizationId = organizationId;
        command.Participants =
        [
            new WorkProjectParticipantCommand
            {
                UserId = userId,
                RoleId = isClient ? RoleIds.Developer : RoleIds.OrgClientManager
            }
        ];

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.Participants));
    }

    [Fact]
    public async Task Validate_WhenClientParticipantDoesNotBelongToSelectedOrganization_FailsValidation()
    {
        var userId = Guid.NewGuid();
        SetupUser(userId, UserTypeEnum.Client, Guid.NewGuid());
        var command = CreateCommand();
        command.Participants =
        [
            new WorkProjectParticipantCommand
            {
                UserId = userId,
                RoleId = RoleIds.OrgClientManager
            }
        ];

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.Participants));
    }

    [Fact]
    public async Task Validate_WhenTitleIsLongerThanOneHundredCharacters_FailsValidation()
    {
        var command = CreateCommand();
        command.Title = new string('A', 101);

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.Title));
    }

    [Fact]
    public async Task Validate_WhenEndDateIsBeforeStartDate_FailsValidation()
    {
        var command = CreateCommand();
        command.StartDate = new DateTime(2026, 2, 2);
        command.EndDate = new DateTime(2026, 2, 1);

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.EndDate));
    }

    [Fact]
    public async Task Validate_WhenParticipantsExceedLimit_FailsValidation()
    {
        var command = CreateCommand();
        command.Participants = Enumerable.Range(0, 21)
            .Select(_ => new WorkProjectParticipantCommand
            {
                UserId = Guid.NewGuid(),
                RoleId = Guid.NewGuid()
            })
            .ToArray();

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.Participants));
    }

    [Fact]
    public async Task Validate_WhenParticipantIsDuplicated_FailsValidation()
    {
        var userId = Guid.NewGuid();
        var command = CreateCommand();
        command.Participants =
        [
            new WorkProjectParticipantCommand { UserId = userId, RoleId = Guid.NewGuid() },
            new WorkProjectParticipantCommand { UserId = userId, RoleId = Guid.NewGuid() }
        ];

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.Participants));
    }

    private CreateWorkProjectCommand CreateCommand()
    {
        return new CreateWorkProjectCommand
        {
            Title = "Project",
            OrganizationId = Guid.NewGuid(),
            ProjectTypeId = 1,
            ProjectKindId = 1,
            ProjectStatusId = 1,
            Participants =
            [
                new WorkProjectParticipantCommand
                {
                    UserId = Guid.NewGuid(),
                    RoleId = RoleIds.Developer
                }
            ]
        };
    }

    private void SetupUser(Guid id, UserTypeEnum userType, Guid? organizationId)
    {
        _userRepositoryMock
            .Setup(x => x.GetManyAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.Contains(id)),
                It.IsAny<ACriteria<User>>(),
                It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<Guid>, ACriteria<User>, CancellationToken>((_, criteria, _) => Task.FromResult(
                criteria.Apply(new[]
                {
                    new User
                    {
                        Id = id,
                        UserType = (int)userType,
                        OrganizationId = organizationId
                    }
                }.AsQueryable()).ToList()));
    }
}
