using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Infrastructure.Options;
using Bogus;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Admin.Services.Tests.Security;

public abstract class BaseServiceTest
{
    protected readonly Faker Faker = new();
    
    protected readonly Mock<IUserRepository> UserRepositoryMock = new();
    protected readonly Mock<IUniqueTokenService> UniqueTokenServiceMock = new();
    
    protected const int ValidRefreshExpirationInDays = 7;
    protected const int EmailConfirmationTokenExpirationInHours = 24;
    protected const int PasswordResetTokenExpirationInHours = 2;
    protected const int JwtValidExpirationMinutes = 60;

    protected IConfiguration BuildConfiguration()
    {
        var jwtSettings = new Dictionary<string, string?>
        {
            [$"{nameof(JwtOptions)}:{nameof(JwtOptions.Key)}"] = Faker.Random.AlphaNumeric(64),
            [$"{nameof(JwtOptions)}:{nameof(JwtOptions.Issuer)}"] = Faker.Random.AlphaNumeric(32),
            [$"{nameof(JwtOptions)}:{nameof(JwtOptions.Audience)}"] = Faker.Random.AlphaNumeric(32),
            [$"{nameof(JwtOptions)}:{nameof(JwtOptions.TokenExpirationInMinutes)}"] = JwtValidExpirationMinutes.ToString(),
            [$"{nameof(JwtOptions)}:{nameof(JwtOptions.RefreshTokenExpirationInDays)}"] = ValidRefreshExpirationInDays.ToString(),
            [$"{nameof(JwtOptions)}:{nameof(JwtOptions.EmailConfirmationTokenExpirationInHours)}"] = EmailConfirmationTokenExpirationInHours.ToString(),
            [$"{nameof(JwtOptions)}:{nameof(JwtOptions.PasswordResetTokenExpirationInHours)}"] = PasswordResetTokenExpirationInHours.ToString()
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(jwtSettings)
            .Build();
    }
    
    protected User CreateUser(
        Guid? id = null,
        string? email = null,
        string? name = null,
        string? surname = null,
        bool? hasCompletedSurvey = null,
        bool? emailConfirmed = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Email = email ?? Faker.Internet.Email(),
            Name = name ?? Faker.Name.FirstName(),
            Surname = surname ?? Faker.Name.LastName(),
            HasCompletedSurvey = hasCompletedSurvey ?? Faker.Random.Bool(),
            EmailConfirmed = emailConfirmed ?? Faker.Random.Bool()
        };
}
