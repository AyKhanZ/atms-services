using ATMS.Data.Migrations;
using ATMS.Project.Contracts.Commands.Migration;
using ATMS.Project.Services.Handlers.Migrations;
using Moq;

namespace Project.Services.Tests.Handlers.Migrations;

public class MigrationsHandlerTest
{
    private readonly Mock<IMigrationRunner> _migrationRunner = new();
    private readonly MigrationsHandler _handler;

    public MigrationsHandlerTest()
    {
        _handler = new MigrationsHandler(_migrationRunner.Object);
    }

    [Fact]
    public async Task Handle_ApplyMigrations_ReturnsAppliedMigrations()
    {
        _migrationRunner
            .Setup(runner => runner.MigrateUpAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MigrationResult
            {
                AppliedMigrations = ["ProjectMigration"]
            });

        var result = await _handler.Handle(new ApplyMigrationsCommand(), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(["ProjectMigration"], result.AppliedMigrations);
    }

    [Fact]
    public async Task Handle_ApplyMigrations_ReturnsErrorMessageWhenMigrationFails()
    {
        _migrationRunner
            .Setup(runner => runner.MigrateUpAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MigrationResult
            {
                ErrorMessage = "Migration failed."
            });

        var result = await _handler.Handle(new ApplyMigrationsCommand(), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Migration failed.", result.ErrorMessage);
        Assert.Empty(result.AppliedMigrations);
    }

    [Fact]
    public async Task Handle_DownMigration_ReturnsRolledBackMigration()
    {
        const string migrationName = "ProjectMigration";

        _migrationRunner
            .Setup(runner => runner.MigrateDownAsync(migrationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MigrationResult
            {
                RolledBackMigration = migrationName
            });

        var result = await _handler.Handle(
            new DownMigrationCommand { MigrationName = migrationName },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(migrationName, result.RolledBackMigration);
    }
}
