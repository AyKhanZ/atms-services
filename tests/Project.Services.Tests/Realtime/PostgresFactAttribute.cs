namespace Project.Services.Tests.Realtime;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ATMS_REALTIME_TEST_DB")))
        {
            Skip = "Set ATMS_REALTIME_TEST_DB to run the PostgreSQL transaction tests.";
        }
    }
}
