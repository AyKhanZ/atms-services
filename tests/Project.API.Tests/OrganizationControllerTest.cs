using ATMS.Project.API.Controllers.v1;
using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Contracts.Models.Organization;
using ATMS.Project.Contracts.Requests.Organizations;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Project.API.Tests;

public class OrganizationControllerTest : BaseControllerTest
{
    private readonly OrganizationController _controller;

    public OrganizationControllerTest()
    {
        _controller = new OrganizationController(MediatorMock.Object);
    }

    [Fact]
    public async Task Index_Returns200_WithResult()
    {
        var expected = new OrganizationItemModel[] { new(), new() };
        var request = new GetOrganizationsRequest();

        MediatorMock
            .Setup(m => m.Send(request, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Index(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Equal(expected, ok.Value);
    }

    [Fact]
    public async Task Get_Returns200_WithResult()
    {
        var id = Guid.NewGuid();
        var expected = new OrganizationModel();

        MediatorMock
            .Setup(m => m.Send(
                It.Is<GetOrganizationRequest>(r => r.Id == id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Equal(expected, ok.Value);
    }

    [Fact]
    public async Task Create_Returns201_WithId()
    {
        var id = Guid.NewGuid();
        var command = new CreateOrganizationCommand
        {
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10),
            LogoPath = Faker.Random.String(10)
        };

        MediatorMock
            .Setup(m => m.Send(command, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(id);

        var result = await _controller.Create(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Equal(id, created.Value);
        Assert.Equal(nameof(_controller.Get), created.ActionName);
    }

    [Fact]
    public async Task Update_Returns204()
    {
        var command = new UpdateOrganizationCommand
        {
            Id = Guid.NewGuid(),
            Title = Faker.Company.CompanyName(),
            Voen = Faker.Random.AlphaNumeric(10),
            LogoPath = Faker.Random.String(10)
        };

        MediatorMock
            .Setup(m => m.Send(command, 
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var result = await _controller.Update(command, CancellationToken.None);

        var noContent = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContent.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns204()
    {
        var id = Guid.NewGuid();

        MediatorMock
            .Setup(m => m.Send(
                It.Is<DeleteOrganizationCommand>(c => c.Id == id),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var result = await _controller.Delete(id, CancellationToken.None);

        var noContent = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContent.StatusCode);
    }
}