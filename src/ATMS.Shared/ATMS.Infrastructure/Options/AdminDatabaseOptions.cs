using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options.Interfaces;

namespace ATMS.Infrastructure.Options;

public class AdminDatabaseOptions : IOptions
{
    public string SqlConnection { get; set; }
    public string MongoConnection { get; set; }
    public string MongoDatabase { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SqlConnection))
        {
            throw new ConfigurationException(ConfigurationErrorType.Database_SqlConnectionNotFound, "AdminDatabaseOptions: 'SqlConnection' not found .");
        }

        if (string.IsNullOrWhiteSpace(MongoConnection))
        {
            throw new ConfigurationException(ConfigurationErrorType.Database_MongoConnectionNotFound, "AdminDatabaseOptions: 'MongoConnection' not found .");
        }

        if (string.IsNullOrWhiteSpace(MongoDatabase))
        {
            throw new ConfigurationException(ConfigurationErrorType.Database_MongoDatabaseNotFound, "AdminDatabaseOptions: 'MongoDatabase' not found .");
        }
    }
}
