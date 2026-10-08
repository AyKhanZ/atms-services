using Bogus;
using MediatR;
using Moq;

namespace Project.API.Tests;

public abstract class BaseControllerTest
{
    protected readonly Faker Faker = new();
    protected readonly Mock<IMediator> MediatorMock = new();
}