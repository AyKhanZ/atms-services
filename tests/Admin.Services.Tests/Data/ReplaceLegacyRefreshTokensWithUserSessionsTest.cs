using ATMS.Admin.Data.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Admin.Services.Tests.Data;

public class ReplaceLegacyRefreshTokensWithUserSessionsTest
{
    [Fact]
    public void Up_CreatesUserSessionsAndRemovesLegacyStorage()
    {
        var operations = TestMigration.BuildUpOperations();

        Assert.Contains(operations, operation =>
            operation is CreateTableOperation { Name: "UserSessions" });
        Assert.Contains(operations, operation =>
            operation is DropTableOperation { Name: "RefreshRevokedTokens" });
        Assert.Contains(operations, operation =>
            operation is DropColumnOperation { Name: "RefreshToken", Table: "Users" });
        Assert.Contains(operations, operation =>
            operation is DropColumnOperation { Name: "RefreshTokenExpiresAt", Table: "Users" });
    }

    [Fact]
    public void Down_RemovesUserSessionsAndRestoresLegacyStorage()
    {
        var operations = TestMigration.BuildDownOperations();

        Assert.Contains(operations, operation =>
            operation is DropTableOperation { Name: "UserSessions" });
        Assert.Contains(operations, operation =>
            operation is CreateTableOperation { Name: "RefreshRevokedTokens" });
        Assert.Contains(operations, operation =>
            operation is AddColumnOperation { Name: "RefreshToken", Table: "Users" });
        Assert.Contains(operations, operation =>
            operation is AddColumnOperation { Name: "RefreshTokenExpiresAt", Table: "Users" });
    }

    private sealed class TestMigration : ReplaceLegacyRefreshTokensWithUserSessions
    {
        public static IReadOnlyList<MigrationOperation> BuildUpOperations()
        {
            var builder = new MigrationBuilder("Npgsql.EntityFrameworkCore.PostgreSQL");
            new TestMigration().Up(builder);
            return builder.Operations;
        }

        public static IReadOnlyList<MigrationOperation> BuildDownOperations()
        {
            var builder = new MigrationBuilder("Npgsql.EntityFrameworkCore.PostgreSQL");
            new TestMigration().Down(builder);
            return builder.Operations;
        }
    }
}
