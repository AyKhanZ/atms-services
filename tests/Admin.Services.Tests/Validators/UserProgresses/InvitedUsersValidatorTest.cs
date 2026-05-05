using System.Linq.Expressions;
using ATMS.Admin.Contracts.Commands.UserProgresses;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Validation.UserProgresses;
using FluentValidation.TestHelper;
using Moq;

namespace Admin.Services.Tests.Validators.UserProgresses;

public class InvitedUsersValidatorTest : BaseValidatorTest
{
    private readonly InvitedUsersValidator _validator;

    public InvitedUsersValidatorTest()
    {
        _validator = new InvitedUsersValidator(UserRepositoryMock.Object);
    }

    [Fact]
    public async Task Should_Have_Error_When_Email_Not_Unique()
    {
        // Arrange
        var command = new InvitedUsersCommand
        {
            Name = Faker.Name.FullName(),
            Surname = Faker.Name.FullName(),
            Email = Faker.Internet.Email()
        };

        UserRepositoryMock
            .Setup(x => x.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Should_Pass_When_Data_Is_Valid()
    {
        var command = new InvitedUsersCommand
        {
            Name = Faker.Name.FullName(),
            Surname = Faker.Name.FullName(),
            Email = Faker.Internet.Email()
        };

        UserRepositoryMock
            .Setup(x => x.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}