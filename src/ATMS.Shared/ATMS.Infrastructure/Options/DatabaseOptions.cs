namespace ATMS.Infrastructure.Options;

public class DatabaseOptions
{
    public required string SqlConnection { get; init; }
    public required string MongoConnection { get; init; }
    public required string MongoDatabase { get; init; }
}

public class AdminDatabaseOptions : DatabaseOptions;

public class ProjectDatabaseOptions : DatabaseOptions;
