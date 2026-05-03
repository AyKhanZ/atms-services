using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Service.Validation.Profile;

namespace Admin.Services.Tests.Validators.Profile;

public class UpdatePhotoValidatorTest : BaseValidatorTest
{
    private readonly UpdatePhotoValidator _validator = new();

    [Fact]
    public async Task Validate_WhenValid_PassesValidation()
    {
        var command = new UpdatePhotoCommand { Id = Guid.NewGuid(), FileName = Faker.Random.String(10) };

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenIdEmpty_FailsValidation()
    {
        var command = new UpdatePhotoCommand { Id = Guid.Empty, FileName = Faker.Random.String(10) };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Id));
    }

    [Fact]
    public async Task Validate_WhenFileNameEmpty_FailsValidation()
    {
        var command = new UpdatePhotoCommand { Id = Guid.NewGuid(), FileName = "" };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.FileName));
    }

    [Fact]
    public async Task Validate_WhenFileNameTooLong_FailsValidation()
    {
        var command = new UpdatePhotoCommand { Id = Guid.NewGuid(), FileName = new string('a', 51) };

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.FileName));
    }
}