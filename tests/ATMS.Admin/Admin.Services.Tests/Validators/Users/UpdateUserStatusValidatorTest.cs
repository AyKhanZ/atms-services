using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.Users;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Service.Validation.Users;
using Moq;

namespace Admin.Services.Tests.Validators.Users;

public class UpdateUserStatusValidatorTest : BaseValidatorTest
{
    private readonly UpdateUserStatusValidator _validator;

    public UpdateUserStatusValidatorTest()
    {
        _validator = new UpdateUserStatusValidator(DictionariesRepositoryMock.Object);
    }

    [Fact]
    public async Task Validate_WhenValid_PassesValidation()
    {
        DictionariesRepositoryMock
            .Setup(r => r.IsUserStatusExistAsync(
                It.IsAny<Expression<Func<UserStatus, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateUserStatusCommand { Id = Guid.NewGuid(), UserStatusId = 1 };

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenIdEmpty_FailsValidation()
    {
        var command = new UpdateUserStatusCommand { Id = Guid.Empty, UserStatusId = 1 };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Id));
    }

    [Fact]
    public async Task Validate_WhenUserStatusIdEmpty_FailsValidation()
    {
        var command = new UpdateUserStatusCommand { Id = Guid.NewGuid(), UserStatusId = 0 };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.UserStatusId));
    }

    [Fact]
    public async Task Validate_WhenUserStatusNotFound_FailsValidation()
    {
        DictionariesRepositoryMock
            .Setup(r => r.IsUserStatusExistAsync(
                It.IsAny<Expression<Func<UserStatus, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new UpdateUserStatusCommand { Id = Guid.NewGuid(), UserStatusId = 99 };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.UserStatusId));
    }
}