using ATMS.Application.Security;
using ATMS.Project.Contracts.Commands.Migration;

namespace Project.Services.Tests.Contracts;

public class ApplyMigrationsCommandTest
{
    [Theory]
    [InlineData(typeof(ApplyMigrationsCommand))]
    [InlineData(typeof(DownMigrationCommand))]
    public void MigrationCommands_AllowOnlySuperAdmin(Type commandType)
    {
        Assert.True(commandType.IsDefined(typeof(SuperAdminAccessAttribute), false));
    }
}
