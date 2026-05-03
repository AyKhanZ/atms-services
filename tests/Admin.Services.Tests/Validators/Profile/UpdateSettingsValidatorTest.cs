using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Service.Validation.Profile;
using Moq;

namespace Admin.Services.Tests.Validators.Profile;

public class UpdateSettingsValidatorTest : BaseValidatorTest
{
    private readonly UpdateSettingsValidator _validator;

    public UpdateSettingsValidatorTest()
    {
        _validator = new UpdateSettingsValidator(DictionariesRepositoryMock.Object);
    }

    private void SetupDictionariesExist()
    {
        DictionariesRepositoryMock
            .Setup(r => r.IsGenderExistAsync(
                It.IsAny<Expression<Func<Gender, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        DictionariesRepositoryMock
            .Setup(r => r.IsMaritalStatusExistAsync(
                It.IsAny<Expression<Func<MaritalStatus, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private static UpdateSettingsCommand ValidCommand() => new()
    {
        Id = Guid.NewGuid(),
        Name = "John",
        Surname = "Doe",
        PhoneNumber = "+994501234567",
        Position = "Developer",
        BirthDate = DateTime.UtcNow.AddYears(-25),
        GenderId = 1,
        MaritalStatusId = 1
    };

    [Fact]
    public async Task Validate_WhenValid_PassesValidation()
    {
        SetupDictionariesExist();

        var result = await _validator.ValidateAsync(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenIdEmpty_FailsValidation()
    {
        var command = ValidCommand();
        command.Id = Guid.Empty;

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Id));
    }

    [Fact]
    public async Task Validate_WhenNameEmpty_FailsValidation()
    {
        var command = ValidCommand();
        command.Name = "";

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public async Task Validate_WhenNameTooLong_FailsValidation()
    {
        var command = ValidCommand();
        command.Name = new string('a', 51);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public async Task Validate_WhenSurnameEmpty_FailsValidation()
    {
        var command = ValidCommand();
        command.Surname = "";

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Surname));
    }

    [Fact]
    public async Task Validate_WhenSurnameTooLong_FailsValidation()
    {
        var command = ValidCommand();
        command.Surname = new string('a', 101);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Surname));
    }

    [Fact]
    public async Task Validate_WhenPhoneNumberEmpty_FailsValidation()
    {
        var command = ValidCommand();
        command.PhoneNumber = "";

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.PhoneNumber));
    }

    [Fact]
    public async Task Validate_WhenPhoneNumberInvalid_FailsValidation()
    {
        var command = ValidCommand();
        command.PhoneNumber = "not-a-phone";

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.PhoneNumber));
    }

    [Fact]
    public async Task Validate_WhenPositionEmpty_FailsValidation()
    {
        var command = ValidCommand();
        command.Position = "";

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Position));
    }

    [Fact]
    public async Task Validate_WhenPositionTooLong_FailsValidation()
    {
        var command = ValidCommand();
        command.Position = new string('a', 51);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Position));
    }

    [Fact]
    public async Task Validate_WhenBirthDateTooOld_FailsValidation()
    {
        var command = ValidCommand();
        command.BirthDate = new DateTime(1899, 12, 31);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.BirthDate));
    }

    [Fact]
    public async Task Validate_WhenBirthDateInFuture_FailsValidation()
    {
        var command = ValidCommand();
        command.BirthDate = DateTime.UtcNow.AddDays(1);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.BirthDate));
    }

    [Fact]
    public async Task Validate_WhenUserUnder18_FailsValidation()
    {
        var command = ValidCommand();
        command.BirthDate = DateTime.UtcNow.AddYears(-17);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.BirthDate));
    }

    [Fact]
    public async Task Validate_WhenGenderNotFound_FailsValidation()
    {
        DictionariesRepositoryMock
            .Setup(r => r.IsGenderExistAsync(
                It.IsAny<Expression<Func<Gender, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        DictionariesRepositoryMock
            .Setup(r => r.IsMaritalStatusExistAsync(
                It.IsAny<Expression<Func<MaritalStatus, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = ValidCommand();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.GenderId));
    }

    [Fact]
    public async Task Validate_WhenMaritalStatusNotFound_FailsValidation()
    {
        DictionariesRepositoryMock
            .Setup(r => r.IsGenderExistAsync(
                It.IsAny<Expression<Func<Gender, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        DictionariesRepositoryMock
            .Setup(r => r.IsMaritalStatusExistAsync(
                It.IsAny<Expression<Func<MaritalStatus, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = ValidCommand();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.MaritalStatusId));
    }
}