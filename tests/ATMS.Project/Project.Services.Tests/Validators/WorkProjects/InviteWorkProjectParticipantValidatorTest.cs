using System.Linq.Expressions;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Validation.WorkProjects;
using Moq;

namespace Project.Services.Tests.Validators.WorkProjects;

public class InviteWorkProjectParticipantValidatorTest : BaseValidatorTest
{
    private readonly Mock<IWorkProjectInvitationRepository> _invitationRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly WorkProject _project = new()
    {
        Id = Guid.NewGuid(),
        OrganizationId = Guid.NewGuid()
    };
    private readonly List<WorkProjectInvitation> _invitations = [];
    private readonly InviteWorkProjectParticipantValidator _validator;

    public InviteWorkProjectParticipantValidatorTest()
    {
        WorkProjectsRepositoryMock
            .Setup(x => x.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        WorkProjectsRepositoryMock
            .Setup(x => x.FindAsync(_project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_project);
        _invitationRepositoryMock
            .Setup(x => x.GetLivePendingAsync(_project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_invitations);

        _validator = new InviteWorkProjectParticipantValidator(
            WorkProjectsRepositoryMock.Object,
            _invitationRepositoryMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Validate_WhenEmailIsNew_PassesValidation()
    {
        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Nigar", "Huseynova")]
    [InlineData("   ", "Nigar", "Huseynova")]
    [InlineData("not-an-email", "Nigar", "Huseynova")]
    [InlineData("nigar@client.az", "", "Huseynova")]
    [InlineData("nigar@client.az", "Nigar", " ")]
    public async Task Validate_WhenFieldIsEmptyOrInvalid_FailsOnThatField(string email, string name, string surname)
    {
        var command = CreateCommand();
        command.Email = email;
        command.Name = name;
        command.Surname = surname;

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(nameof(InviteWorkProjectParticipantCommand.Name), 50)]
    [InlineData(nameof(InviteWorkProjectParticipantCommand.Surname), 100)]
    [InlineData(nameof(InviteWorkProjectParticipantCommand.Email), 100)]
    public async Task Validate_WhenFieldIsLongerThanLimit_FailsOnThatField(string field, int limit)
    {
        var command = CreateCommand();
        var tooLong = new string('a', limit + 1);
        switch (field)
        {
            case nameof(command.Name):
                command.Name = tooLong;
                break;
            case nameof(command.Surname):
                command.Surname = tooLong;
                break;
            default:
                command.Email = new string('a', limit - 5) + "@x.com";
                break;
        }

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == field);
    }

    [Theory]
    [InlineData(nameof(InviteWorkProjectParticipantCommand.Name), 50)]
    [InlineData(nameof(InviteWorkProjectParticipantCommand.Surname), 100)]
    public async Task Validate_WhenFieldIsExactlyAtLimit_PassesValidation(string field, int limit)
    {
        var command = CreateCommand();
        if (field == nameof(command.Name))
        {
            command.Name = new string('a', limit);
        }
        else
        {
            command.Surname = new string('a', limit);
        }

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenProjectIsInternal_FailsOnEmail()
    {
        _project.OrganizationId = null;

        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(InviteWorkProjectParticipantCommand.Email));
    }

    [Fact]
    public async Task Validate_WhenProjectDoesNotExist_FailsOnProjectId()
    {
        WorkProjectsRepositoryMock
            .Setup(x => x.IsExistAsync(
                It.IsAny<Expression<Func<WorkProject, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(InviteWorkProjectParticipantCommand.ProjectId));
    }

    [Theory]
    [InlineData(20, 0)]
    [InlineData(19, 1)]
    [InlineData(15, 5)]
    public async Task Validate_WhenParticipantsAndPendingInvitationsReachLimit_Fails(int participants, int invitations)
    {
        AddParticipants(participants);
        AddInvitations(invitations);

        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(InviteWorkProjectParticipantCommand.Email));
    }

    [Fact]
    public async Task Validate_WhenOnePlaceIsLeft_PassesValidation()
    {
        AddParticipants(18);
        AddInvitations(1);

        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("nigar@client.az")]
    [InlineData("  NIGAR@Client.az ")]
    public async Task Validate_WhenEmailBelongsToParticipant_Fails(string email)
    {
        _project.WorkProjectParticipants.Add(new WorkProjectParticipant
        {
            UserId = Guid.NewGuid(),
            User = new User { Email = "Nigar@client.az", NormalizedEmail = "NIGAR@CLIENT.AZ" }
        });
        var command = CreateCommand();
        command.Email = email;

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.Email));
        _userRepositoryMock.Verify(
            x => x.IsEmailTakenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Validate_WhenEmailIsAlreadyInvitedToProject_Fails()
    {
        _invitations.Add(new WorkProjectInvitation { NormalizedEmail = "NIGAR@CLIENT.AZ" });
        var command = CreateCommand();
        command.Email = "Nigar@Client.az";

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.Email));
    }

    [Fact]
    public async Task Validate_WhenTheUserIsInactive_Fails()
    {
        _userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { NormalizedEmail = "NIGAR@CLIENT.AZ", IsActive = false });

        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(InviteWorkProjectParticipantCommand.Email));
        _userRepositoryMock.Verify(
            x => x.IsEmailTakenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Validate_WhenEmailBelongsToAnotherUser_Fails()
    {
        _userRepositoryMock
            .Setup(x => x.IsEmailTakenAsync("NIGAR@CLIENT.AZ", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(InviteWorkProjectParticipantCommand.Email));
    }

    private void AddParticipants(int count)
    {
        for (var i = 0; i < count; i++)
        {
            _project.WorkProjectParticipants.Add(new WorkProjectParticipant
            {
                UserId = Guid.NewGuid(),
                User = new User { Email = $"user{i}@client.az", NormalizedEmail = $"USER{i}@CLIENT.AZ" }
            });
        }
    }

    private void AddInvitations(int count)
    {
        for (var i = 0; i < count; i++)
        {
            _invitations.Add(new WorkProjectInvitation { NormalizedEmail = $"INVITED{i}@CLIENT.AZ" });
        }
    }

    private InviteWorkProjectParticipantCommand CreateCommand()
    {
        return new InviteWorkProjectParticipantCommand
        {
            ProjectId = _project.Id,
            Email = "nigar@client.az",
            Name = "Nigar",
            Surname = "Huseynova"
        };
    }
}
