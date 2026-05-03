using ATMS.Admin.Data.Repositories.Interfaces;
using Bogus;
using Moq;

namespace Admin.Services.Tests.Validators;

public abstract class BaseValidatorTest
{
    protected readonly Faker Faker = new();
    protected readonly Mock<IDictionariesRepository> DictionariesRepositoryMock = new();
}