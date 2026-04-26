namespace ATMS.Infrastructure.Options;

public class DatabaseOptions
{
    public required string SqlConnection { get; init; }
}

public class AdminDatabaseOptions : DatabaseOptions;

public class ProjectDatabaseOptions : DatabaseOptions;
