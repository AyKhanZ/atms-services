using ATMS.Data.Criterias;
using ATMS.Project.Contracts.Models.Organization;
using ATMS.Project.Contracts.Requests.Organizations;
using ATMS.Project.Data.Criterias.Organizations;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.Organizations;
using Moq;

namespace Project.Services.Tests.Handlers.Organizations;

public class GetOrganizationsHandlerTest : BaseHandlerTest
{
    private readonly GetOrganizationsHandler _handler;
 
    public GetOrganizationsHandlerTest()
    {
        _handler = new GetOrganizationsHandler(OrganizationRepositoryMock.Object, MapperMock.Object);
    }
 
    [Fact]
    public async Task Handle_ShouldReturnMappedPagedResult()
    {
        // Arrange
        var request = new GetOrganizationsRequest { Page = 1, PageSize = 10 };
        var filter = new OrganizationFilter();
 
        var organizations = new PagedResult<Organization>
        {
            Items =
            [
                new Organization { Id = Guid.NewGuid(), Title = "Org 1" },
                new Organization { Id = Guid.NewGuid(), Title = "Org 2" }
            ],
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
 
        var expectedModels = organizations.Items
            .Select(o => new OrganizationItemModel { Id = o.Id, Title = o.Title })
            .ToArray();
 
        MapperMock
            .Setup(m => m.Map<OrganizationFilter>(request))
            .Returns(filter);
 
        OrganizationRepositoryMock
            .Setup(r => r.GetAsync(
                filter,
                It.IsAny<PaginationCriteria<Organization>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(organizations);
 
        MapperMock
            .Setup(m => m.Map<OrganizationItemModel>(It.IsAny<Organization>()))
            .Returns<Organization>(o => expectedModels.First(m => m.Id == o.Id));
 
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
 
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Length);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }
 
    [Fact]
    public async Task Handle_ShouldMapRequestToFilter()
    {
        // Arrange
        var request = new GetOrganizationsRequest
        {
            Title = "Test",
            Voen = "123",
            Page = 2,
            PageSize = 5
        };
 
        var filter = new OrganizationFilter { Title = "Test", Voen = "123" };
        var emptyResult = new PagedResult<Organization>();
 
        MapperMock
            .Setup(m => m.Map<OrganizationFilter>(request))
            .Returns(filter);
 
        OrganizationRepositoryMock
            .Setup(r => r.GetAsync(
                filter,
                It.IsAny<PaginationCriteria<Organization>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);
 
        // Act
        await _handler.Handle(request, CancellationToken.None);
 
        // Assert
        MapperMock.Verify(m => m.Map<OrganizationFilter>(request), Times.Once);
    }
 
    [Fact]
    public async Task Handle_ShouldPassCorrectPaginationCriteria()
    {
        // Arrange
        var request = new GetOrganizationsRequest { Page = 3, PageSize = 25 };
        var filter = new OrganizationFilter();
        var emptyResult = new PagedResult<Organization>();
 
        MapperMock
            .Setup(m => m.Map<OrganizationFilter>(request))
            .Returns(filter);
 
        PaginationCriteria<Organization>? capturedPagination = null;
        OrganizationRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<OrganizationFilter>(),
                It.IsAny<PaginationCriteria<Organization>>(),
                It.IsAny<CancellationToken>()))
            .Callback<ACriteria<Organization>, PaginationCriteria<Organization>, CancellationToken>(
                (_, pagination, _) => capturedPagination = pagination)
            .ReturnsAsync(emptyResult);
 
        // Act
        await _handler.Handle(request, CancellationToken.None);
 
        // Assert
        Assert.NotNull(capturedPagination);
        Assert.Equal(3, capturedPagination.Page);
        Assert.Equal(25, capturedPagination.PageSize);
    }
 
    [Fact]
    public async Task Handle_WhenRepositoryReturnsEmpty_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var request = new GetOrganizationsRequest { Page = 1, PageSize = 20 };
        var filter = new OrganizationFilter();
        var emptyResult = new PagedResult<Organization> { Items = [], TotalCount = 0, Page = 1, PageSize = 20 };
 
        MapperMock.Setup(m => m.Map<OrganizationFilter>(request)).Returns(filter);
        OrganizationRepositoryMock
            .Setup(r => r.GetAsync(filter,
                It.IsAny<PaginationCriteria<Organization>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);
 
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
 
        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
 
    [Fact]
    public async Task Handle_ShouldCallMapForEachOrganization()
    {
        // Arrange
        var request = new GetOrganizationsRequest { Page = 1, PageSize = 10 };
        var filter = new OrganizationFilter();
 
        var organizations = new PagedResult<Organization>
        {
            Items =
            [
                new Organization { Id = Guid.NewGuid() },
                new Organization { Id = Guid.NewGuid() },
                new Organization { Id = Guid.NewGuid() }
            ],
            TotalCount = 3
        };
 
        MapperMock.Setup(m => m.Map<OrganizationFilter>(request)).Returns(filter);
        MapperMock.Setup(m => m.Map<OrganizationItemModel>(It.IsAny<Organization>()))
            .Returns(new OrganizationItemModel());
        OrganizationRepositoryMock
            .Setup(r => r.GetAsync(filter,
                It.IsAny<PaginationCriteria<Organization>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(organizations);
 
        // Act
        await _handler.Handle(request, CancellationToken.None);
 
        // Assert
        MapperMock.Verify(m => m.Map<OrganizationItemModel>(It.IsAny<Organization>()), Times.Exactly(3));
    }
 
    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        var request = new GetOrganizationsRequest();
        var filter = new OrganizationFilter();
 
        MapperMock.Setup(m => m.Map<OrganizationFilter>(request)).Returns(filter);
        OrganizationRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<OrganizationFilter>(),
                It.IsAny<PaginationCriteria<Organization>>(), token))
            .ReturnsAsync(new PagedResult<Organization>());
 
        // Act
        await _handler.Handle(request, token);
 
        // Assert
        OrganizationRepositoryMock.Verify(r => r.GetAsync(
            It.IsAny<OrganizationFilter>(),
            It.IsAny<PaginationCriteria<Organization>>(),
            token), Times.Once);
    }
}