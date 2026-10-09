using ATMS.Admin.Contracts.Commands.Users;
using ATMS.Admin.Service.Validation.Users;
using ATMS.Data.Enums;

namespace Admin.Services.Tests.Validators.Users;

public class UpdateUserStatusValidatorTest
{
    private readonly UpdateUserStatusValidator _validator = new();

    [Theory]
    [InlineData(UserStatusEnum.Active)]
    [InlineData(UserStatusEnum.Inactive)]
    public async Task Validate_WhenStatusIsActiveOrInactive_PassesValidation(UserStatusEnum status)
    {
        var command = new UpdateUserStatusCommand { Id = Guid.NewGuid(), UserStatusId = (int)status };

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenIdEmpty_FailsValidation()
    {
        var command = new UpdateUserStatusCommand { Id = Guid.Empty, UserStatusId = (int)UserStatusEnum.Active };

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

    [Theory]
    [InlineData(UserStatusEnum.Locked)]
    [InlineData((UserStatusEnum)99)]
    public async Task Validate_WhenStatusIsNotActiveOrInactive_FailsValidation(UserStatusEnum status)
    {
        var command = new UpdateUserStatusCommand { Id = Guid.NewGuid(), UserStatusId = (int)status };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.UserStatusId));
    }
}
