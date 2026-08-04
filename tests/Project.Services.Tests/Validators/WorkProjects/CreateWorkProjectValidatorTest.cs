using System.Linq.Expressions;
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
            .Setup(x => x.GetManyAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<Guid>, CancellationToken>((ids, _) => Task.FromResult(
                ids.Select(id => new User { Id = id }).ToList()));
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
        command.OrganizationId = null;

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenParticipantExistsWithoutOrganization_FailsValidation()
    {
        var command = CreateCommand();
        command.OrganizationId = null;
        command.Participants =
        [
            new WorkProjectParticipantCommand
            {
                UserId = Guid.NewGuid(),
                RoleId = Guid.NewGuid()
            }
        ];

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.OrganizationId));
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
            ProjectStatusId = 1
        };
    }
}
