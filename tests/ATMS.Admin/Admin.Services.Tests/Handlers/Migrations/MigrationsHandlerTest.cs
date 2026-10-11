using ATMS.Admin.Contracts.Commands.Migration;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Service.Handlers.Migrations;
using ATMS.Admin.Service.Providers.Interfaces;
using ATMS.Data.Migrations;
using Moq;

namespace Admin.Services.Tests.Handlers.Migrations;

public class MigrationsHandlerTest : BaseHandlerTest
{
    private readonly Mock<IMigrationsProvider> _migrationsProvider = new();
    private readonly MigrationsHandler _handler;

    public MigrationsHandlerTest()
    {
        _handler = new MigrationsHandler(MigrationRunnerMock.Object, _migrationsProvider.Object);
    }

    [Fact]
    public async Task Handle_ApplyMigrations_ReturnsBothDatabasesWhenAdminSucceeds()
    {
        var migrationResult = new MigrationResult
        {
            AppliedMigrations = ["Migration1", "Migration2"],
            ErrorMessage = null
        };
        var project = new MigrationModel
        {
            AppliedMigrations = ["ProjectMigration1"]
        };

        MigrationRunnerMock
            .Setup(r => r.MigrateUpAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(migrationResult);
        _migrationsProvider
            .Setup(provider => provider.ApplyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var result = await _handler.Handle(new ApplyMigrationsCommand(), CancellationToken.None);

        Assert.Null(result.Admin.ErrorMessage);
        Assert.True(result.Admin.Success);
        Assert.Equal(migrationResult.AppliedMigrations, result.Admin.AppliedMigrations);
        Assert.Same(project, result.Project);
        _migrationsProvider.Verify(
            provider => provider.ApplyAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ApplyMigrations_DoesNotCallProjectWhenAdminFails()
    {
        MigrationRunnerMock
            .Setup(r => r.MigrateUpAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MigrationResult
            {
                ErrorMessage = "Admin migration failed."
            });

        var result = await _handler.Handle(new ApplyMigrationsCommand(), CancellationToken.None);

        Assert.False(result.Admin.Success);
        Assert.Equal("Admin migration failed.", result.Admin.ErrorMessage);
        Assert.Null(result.Project);
        _migrationsProvider.Verify(
            provider => provider.ApplyAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ApplyMigrations_KeepsAdminResultWhenProjectCallFails()
    {
        MigrationRunnerMock
            .Setup(r => r.MigrateUpAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MigrationResult
            {
                AppliedMigrations = ["AdminMigration"]
            });
        _migrationsProvider
            .Setup(provider => provider.ApplyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MigrationModel
            {
                ErrorMessage = "Connection refused."
            });

        var result = await _handler.Handle(new ApplyMigrationsCommand(), CancellationToken.None);

        Assert.True(result.Admin.Success);
        Assert.Equal(["AdminMigration"], result.Admin.AppliedMigrations);
        Assert.NotNull(result.Project);
        Assert.False(result.Project.Success);
        Assert.Equal("Connection refused.", result.Project.ErrorMessage);
    }

    [Fact]
    public async Task Handle_DownMigration_ReturnsMigrationModel()
    {
        const string migrationName = "Migration1";
        var migrationResult = new MigrationResult
        {
            RolledBackMigration = migrationName,
            ErrorMessage = null
        };

        MigrationRunnerMock
            .Setup(r => r.MigrateDownAsync(migrationName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(migrationResult);

        var result = await _handler.Handle(new DownMigrationCommand { MigrationName = migrationName }, CancellationToken.None);

        Assert.Null(result.ErrorMessage);
        Assert.Equal(migrationName, result.RolledBackMigration);
        _migrationsProvider.Verify(
            provider => provider.ApplyAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
