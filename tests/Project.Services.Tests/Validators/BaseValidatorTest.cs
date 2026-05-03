using ATMS.Project.Data.Repositories.Interfaces;
using Bogus;
using Moq;

namespace Project.Services.Tests.Validators;

public abstract class BaseValidatorTest
{
    protected readonly Faker Faker = new();
    protected readonly Mock<IOrganizationRepository> OrganizationRepositoryMock = new();
}