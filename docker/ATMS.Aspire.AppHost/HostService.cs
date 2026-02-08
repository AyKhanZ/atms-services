namespace ATMS.Aspire.AppHost;

public class HostService(IDistributedApplicationBuilder builder)
{
    public IResourceBuilder<ProjectResource> AddProject<T>(string name) where T : IProjectMetadata, new()
    {
        var project = builder.AddProject<T>(name);

        return project;
    }

    public IResourceBuilder<ContainerResource> AddPostgres()
    {
        return builder
            .AddContainer("postgres", "postgres:16-alpine")
            .WithContainerName("atms_postgres_sql")
            .WithVolume("atms_postgresdata", "/var/lib/postgresql/data")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithEnvironment("POSTGRES_DB", "atms")
            .WithEnvironment("POSTGRES_USER", "atmsuser")
            .WithEnvironment("POSTGRES_PASSWORD", "atmspassword")
            .WithEndpoint(5432, 5432);
    }

    public IResourceBuilder<ContainerResource> AddMongo()
    {
        return builder
            .AddContainer("mongo", "mongo:7")
            .WithContainerName("atms_mongo")
            .WithVolume("atms_mongodata", "/data/db")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithEnvironment("MONGO_INITDB_DATABASE", "atms")
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "atmsuser")
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "atmspassword")
            .WithEndpoint(27017, 27017);
    }
}
