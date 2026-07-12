using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Models.Organizations;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Providers.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Validation.Account;
using ATMS.Application.Exceptions.Resources;
using ATMS.Data.Constants;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators.Account;

public class RegisterValidatorTest
{
    private readonly Faker _faker;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IRoleRepository> _roleRepository;
    private readonly RegisterUserValidator _validator;
    private readonly Mock<IOrganizationProvider> _provider;

    public RegisterValidatorTest()
    {
        _faker = new Faker();
        _userRepository = new Mock<IUserRepository>();
        _roleRepository = new Mock<IRoleRepository>();
        _provider = new Mock<IOrganizationProvider>();

        _validator = new RegisterUserValidator(_userRepository.Object, _roleRepository.Object, _provider.Object);

        SetupEmailUnique(true);
        SetupRoleExists(true);
        SetupOrganizationExists(true);
    }

    public static TheoryData<Guid, bool> AllowedRoleCases => new()
    {
        { RoleIds.Employee, false },
        { RoleIds.ClientManager, true },
        { RoleIds.Client, true }
    };

    public static TheoryData<Guid> NotAllowedRoleCases => new()
    {
        RoleIds.SuperAdmin,
        RoleIds.ProjectManager,
        RoleIds.BusinessConsultant,
        RoleIds.Developer,
        RoleIds.OrgClientManager,
        RoleIds.OrgClientViewer
    };

    private RegisterCommand CreateCommand(
        string? email = null,
        string? name = null,
        string? surname = null,
        Guid? roleId = null,
        Guid? organizationId = null)
    {
        return new RegisterCommand
        {
            Email = email ?? _faker.Internet.Email(),
            Name = name ?? _faker.Name.FirstName(),
            Surname = surname ?? _faker.Name.LastName(),
            RoleId = roleId ?? RoleIds.Employee,
            OrganizationId = organizationId
        };
    }

    private void SetupEmailUnique(bool isUnique) =>
        _userRepository
            .Setup(r => r.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(!isUnique);

    private void SetupRoleExists(bool exists) =>
        _roleRepository
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists ? new Role { Id = Guid.NewGuid(), Name = "Admin" } : null);

    private void SetupOrganizationExists(bool exists) =>
        _provider
            .Setup(p => p.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists ? new OrganizationModel() : null);

    [Fact]
    public async Task Validate_WithValidCommand_ReturnsSuccess()
    {
        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenNameIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(name: string.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.NameRequired);
    }

    [Fact]
    public async Task Validate_WhenNameExceedsMaxLength_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(name: _faker.Random.String(101)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == string.Format(AccountMessages.NameShouldBeLessThan, 100));
    }

    [Fact]
    public async Task Validate_WhenSurnameIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(surname: string.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.SurnameRequired);
    }

    [Fact]
    public async Task Validate_WhenSurnameExceedsMaxLength_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(surname: _faker.Random.String(101)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == string.Format(AccountMessages.SurnameShouldBeLessThan, 100));
    }

    [Fact]
    public async Task Validate_WhenEmailIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(email: string.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.EmailRequired);
    }

    [Fact]
    public async Task Validate_WhenEmailFormatIsInvalid_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(email: "not-an-email"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ValidationMessages.InvalidEmailFormat);
    }

    [Fact]
    public async Task Validate_WhenEmailExceedsMaxLength_ReturnsFailure()
    {
        var email = $"{_faker.Random.String2(92, "abcdefghijklmnopqrstuvwxyz")}@mail.com";

        var result = await _validator.ValidateAsync(CreateCommand(email: email));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == string.Format(AccountMessages.EmailShouldBeLessThan, 100));
    }

    [Fact]
    public async Task Validate_WhenEmailAlreadyExists_ReturnsFailure()
    {
        SetupEmailUnique(false);

        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.UserAlreadyExists);
    }

    [Fact]
    public async Task Validate_WhenRoleIdIsEmpty_ReturnsFailure()
    {
        var result = await _validator.ValidateAsync(CreateCommand(roleId: Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == ValidationMessages.RoleIdRequired);
    }

    [Fact]
    public async Task Validate_WhenRoleNotFound_ReturnsFailure()
    {
        SetupRoleExists(false);

        var result = await _validator.ValidateAsync(CreateCommand());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == RoleMessages.NotFound);
    }

    [Theory]
    [MemberData(nameof(NotAllowedRoleCases))]
    public async Task Validate_WhenRoleIsNotAllowedForRegistration_ReturnsFailure(Guid roleId)
    {
        var result = await _validator.ValidateAsync(CreateCommand(roleId: roleId));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == RoleMessages.NotFound);
    }

    [Theory]
    [MemberData(nameof(AllowedRoleCases))]
    public async Task Validate_WhenAllowedRoleHasValidOrganizationRequirement_ReturnsSuccess(Guid roleId, bool requiresOrganization)
    {
        var result = await _validator.ValidateAsync(CreateCommand(
            roleId: roleId,
            organizationId: requiresOrganization ? Guid.NewGuid() : null));

        Assert.True(result.IsValid);
    }

    [Theory]
    [MemberData(nameof(AllowedRoleCases))]
    public async Task Validate_WhenRoleRequiresOrganization_AndOrganizationIdIsEmpty_ReturnsExpectedResult(Guid roleId, bool requiresOrganization)
    {
        var result = await _validator.ValidateAsync(CreateCommand(roleId: roleId, organizationId: null));

        if (requiresOrganization)
        {
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.OrganizationIdRequired);
            return;
        }

        Assert.True(result.IsValid);
    }

    [Theory]
    [MemberData(nameof(AllowedRoleCases))]
    public async Task Validate_WhenRoleRequiresOrganization_AndOrganizationNotFound_ReturnsExpectedResult(Guid roleId, bool requiresOrganization)
    {
        SetupOrganizationExists(false);

        var result = await _validator.ValidateAsync(CreateCommand(roleId: roleId, organizationId: Guid.NewGuid()));

        if (requiresOrganization)
        {
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == AccountMessages.OrganizationIdNotExist);
            return;
        }

        Assert.True(result.IsValid);
    }
}