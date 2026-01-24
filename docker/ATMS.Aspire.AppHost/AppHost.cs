var builder = DistributedApplication.CreateBuilder(args);

var mongoDb = builder.AddMongoDB("atms-mongodb")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithMongoExpress()
    .WithDataVolume("mongo-data")
    .AddDatabase("atms-admin-db");

var postgres = builder.AddPostgres("atms-sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin()
    .WithDataVolume("sql-data")
    .AddDatabase("atms-db");

var redis = builder.AddRedis("atms-redis")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRedisInsight()
    .WithDataVolume("redis-data");

var rabbitMq = builder.AddRabbitMQ("atms-rabbitmq")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithManagementPlugin()
    .WithDataVolume("rabbitmq-data");

builder.AddProject<Projects.ATMS_Admin_API>("atms-admin-api")
    .WithReference(mongoDb)
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WaitFor(mongoDb)
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(rabbitMq);


builder.Build().Run();