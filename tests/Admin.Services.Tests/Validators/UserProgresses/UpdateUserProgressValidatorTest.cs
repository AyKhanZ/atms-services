using ATMS.Admin.Contracts.Commands.UserProgresses;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Validation.UserProgresses;
using ATMS.Application.Exceptions.Auth;
using Moq;

namespace Admin.Services.Tests.Validators.UserProgresses;

public class UpdateUserProgressValidatorTest : BaseValidatorTest
{
    private UpdateUserProgressValidator CreateValidator(string userType = "Client")
    {
        CurrentUserMock.Setup(x => x.UserType).Returns(userType);
        CurrentUserMock.Setup(x => x.Id).Returns(Guid.NewGuid());

        return new UpdateUserProgressValidator(
            DictionariesRepositoryMock.Object,
            UserRepositoryMock.Object,
            CurrentUserMock.Object);
    }

    [Fact]
    public async Task Should_Throw_When_User_Not_Found()
    {
        var validator = CreateValidator();

        UserRepositoryMock.Setup(x => x.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var cmd = new UpdateUserProgressCommand();

        await Assert.ThrowsAsync<AuthException>(() => validator.ValidateAsync(cmd));
    }

    [Fact]
    public async Task Should_Fail_When_Invalid_Password()
    {
        var validator = CreateValidator();

        UserRepositoryMock.Setup(x => x.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User());

        var cmd = new UpdateUserProgressCommand
        {
            Password = "123"
        };

        var result = await validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
    }

    [Theory]
    [MemberData(nameof(InvalidOrganizationIds))]
    public async Task Should_Require_OrganizationId_For_Client(Guid? organizationId)
    {
        var validator = CreateValidator();

        UserRepositoryMock.Setup(x => x.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User());

        var cmd = new UpdateUserProgressCommand
        {
            OrganizationId = organizationId
        };

        var result = await validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
    }
    
    public static IEnumerable<object?[]> InvalidOrganizationIds()
    {
        yield return [null];
        yield return [Guid.Empty];
    }
}