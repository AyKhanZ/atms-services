namespace ATMS.Infrastructure.Options;

public class DatabaseOptions
{
    public required string SqlConnection { get; init; }
}

public sealed class AdminDatabaseOptions : DatabaseOptions;

public sealed class ProjectDatabaseOptions : DatabaseOptions;
