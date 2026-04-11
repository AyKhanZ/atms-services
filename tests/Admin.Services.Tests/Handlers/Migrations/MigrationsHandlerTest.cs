using ATMS.Admin.Contracts.Commands.Migration;
using ATMS.Admin.Data.Infrastructure.Migrations;
using ATMS.Admin.Service.Handlers.Migratons;
using Moq;

namespace Admin.Services.Tests.Handlers.Migrations;

public class MigrationsHandlerTest : BaseHandlerTest
{
    private readonly MigrationsHandler _handler;
 
    public MigrationsHandlerTest()
    {
        _handler = new MigrationsHandler(MigrationRunnerMock.Object);
    }
 
    [Fact]
    public async Task Handle_ApplyMigrations_ReturnsMigrationModel()
    {
        var migrationResult = new MigrationResult
        {
            AppliedMigrations = ["Migration1", "Migration2"],
            ErrorMessage = null
        };
 
        MigrationRunnerMock
            .Setup(r => r.MigrateUpAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(migrationResult);
 
        var result = await _handler.Handle(new ApplyMigrationsCommand(), CancellationToken.None);
 
        Assert.Null(result.ErrorMessage);
        Assert.Equal(migrationResult.AppliedMigrations, result.AppliedMigrations);
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
    }
}
