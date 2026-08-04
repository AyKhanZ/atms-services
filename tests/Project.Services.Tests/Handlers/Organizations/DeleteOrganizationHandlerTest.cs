using System.Linq.Expressions;
using ATMS.Application.Exceptions.Entity;
using ATMS.Project.Contracts.Commands.Organizations;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Handlers.Organizations;
using Moq;

namespace Project.Services.Tests.Handlers.Organizations;

public class DeleteOrganizationHandlerTest : BaseHandlerTest
{
    private readonly DeleteOrganizationHandler _handler;

    public DeleteOrganizationHandlerTest()
    {
        _handler = new DeleteOrganizationHandler(
            OrganizationRepositoryMock.Object,
            CurrentUserMock.Object);
    }

    [Fact]
    public async Task Handle_SoftDeletesEntityAndSaves()
    {
        var userId = Guid.NewGuid();
        var entity = new Organization { Id = Guid.NewGuid(), IsDeleted = false };
        var command = new DeleteOrganizationCommand { Id = entity.Id };

        CurrentUserMock.Setup(u => u.Id).Returns(userId);
        OrganizationRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _handler.Handle(command, CancellationToken.None);

        Assert.True(entity.IsDeleted);
        Assert.Equal(userId, entity.DeletedById);
        Assert.NotNull(entity.DeletedAt);
        OrganizationRepositoryMock.Verify(r => r.SaveAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SetsDeletedAtToUtcNow()
    {
        var before = DateTime.UtcNow;
        var entity = new Organization { Id = Guid.NewGuid() };
        var command = new DeleteOrganizationCommand { Id = entity.Id };

        CurrentUserMock.Setup(u => u.Id).Returns(Guid.NewGuid());
        OrganizationRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(entity.DeletedAt);
        Assert.True(entity.DeletedAt >= before && entity.DeletedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsEntityException()
    {
        OrganizationRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        var command = new DeleteOrganizationCommand { Id = Guid.NewGuid() };

        await Assert.ThrowsAsync<EntityException>(() =>
            _handler.Handle(command, CancellationToken.None));

        OrganizationRepositoryMock.Verify(r => r.SaveAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }
}