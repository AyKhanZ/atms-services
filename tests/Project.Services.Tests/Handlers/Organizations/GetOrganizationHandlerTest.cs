using System.Linq.Expressions;
using ATMS.Project.Contracts.Models.Organizations;
using ATMS.Project.Contracts.Requests.Organizations;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.Organizations;
using Moq;

namespace Project.Services.Tests.Handlers.Organizations;

public class GetOrganizationHandlerTest : BaseHandlerTest
{
    private readonly GetOrganizationHandler _handler;

    public GetOrganizationHandlerTest()
    {
        _handler = new GetOrganizationHandler(
            OrganizationRepositoryMock.Object,
            MapperMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMappedOrganization()
    {
        var entity = new Organization { Id = Guid.NewGuid() };
        var expected = new OrganizationModel { Id = entity.Id };
        var request = new GetOrganizationRequest { Id = entity.Id };

        OrganizationRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        MapperMock
            .Setup(m => m.Map<OrganizationModel>(entity))
            .Returns(expected);

        var result = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectId()
    {
        var id = Guid.NewGuid();
        var entity = new Organization { Id = id };
        var request = new GetOrganizationRequest { Id = id };

        OrganizationRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        MapperMock
            .Setup(m => m.Map<OrganizationModel>(entity))
            .Returns(new OrganizationModel());

        await _handler.Handle(request, CancellationToken.None);

        OrganizationRepositoryMock.Verify(
            r => r.GetAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}