using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Interfaces;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators;

public abstract class BaseValidatorTest
{
    protected readonly Faker Faker = new();
    protected readonly Mock<ICurrentUser> CurrentUserMock = new();
    protected readonly Mock<IUserRepository> UserRepositoryMock = new();
    protected readonly Mock<IDictionariesRepository> DictionariesRepositoryMock = new();
    protected readonly Mock<IOnboardingRepository> OnboardingRepositoryMock = new();
}
