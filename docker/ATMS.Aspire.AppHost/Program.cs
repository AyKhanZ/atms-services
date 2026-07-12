var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ATMS_Gateway>("atms-gateway");

builder.AddProject<Projects.ATMS_Admin_API>("atms-admin-api");

builder.AddProject<Projects.ATMS_Project_API>("atms-project-api");

builder.Build().Run();