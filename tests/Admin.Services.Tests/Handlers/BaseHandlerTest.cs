using ATMS.Admin.Data.Infrastructure.Migrations;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Email.Services.Interfaces;
using ATMS.Infrastructure.Options;
using ATMS.Messaging.Interfaces;
using AutoMapper;
using Bogus;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Admin.Services.Tests.Handlers;

public abstract class BaseHandlerTest
{
    protected readonly Faker Faker =  new();
    
    protected readonly Mock<IMapper> MapperMock = new();
    
    
    protected readonly Mock<ICurrentUser> CurrentUserMock = new();
    protected readonly Mock<ICacheService> CacheServiceMock = new();
    
    protected readonly Mock<IUserRepository> UserRepositoryMock = new();
    protected readonly Mock<IRoleRepository> RoleRepositoryMock = new();
    protected readonly Mock<IOnboardingRepository> OnboardingRepositoryMock = new();
    protected readonly Mock<IDictionariesRepository> DictionariesRepositoryMock = new();
    protected readonly Mock<IPasswordResetTokenRepository> PasswordResetTokenRepositoryMock = new();
    
    protected readonly Mock<IRefreshTokenService> RefreshTokenServiceMock = new();
    protected readonly Mock<IAccessTokenService> AccessTokenServiceMock = new();
    protected readonly Mock<IBlackListService> BlackListServiceMock = new();
    protected readonly Mock<IPasswordService> PasswordServiceMock = new();
    protected readonly Mock<IPasswordHasherService> PasswordHasherServiceMock = new();
    protected readonly Mock<IResetPasswordTokenService> ResetPasswordTokenServiceMock = new();
    protected readonly Mock<IMigrationRunner> MigrationRunnerMock = new();
    
    
    protected readonly Mock<IMessagePublisher> MessagePublisherMock = new();
    
    
    protected readonly Mock<IEmailConfirmationTokenService> EmailConfirmationTokenServiceMock = new();
    protected readonly Mock<IEmailSender> EmailSenderMock = new();
    
    protected const string BaseUrl = "https://example.com";
    protected const string ResetPasswordPage = "https://example.com/reset-password";
    
    protected IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{nameof(RedirectUrlOptions)}:{nameof(RedirectUrlOptions.BaseUrl)}"] = BaseUrl,
                [$"{nameof(RedirectUrlOptions)}:{nameof(RedirectUrlOptions.ResetPasswordPage)}"] = ResetPasswordPage
            })
            .Build();
}
