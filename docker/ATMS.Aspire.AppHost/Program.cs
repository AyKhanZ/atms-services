var builder = DistributedApplication.CreateBuilder(args);

var hostService = new ATMS.Aspire.AppHost.HostService(builder);

hostService.AddProject<Projects.ATMS_Admin_API>("atms-api");

builder.AddProject<Projects.ATMS_Project_API>("atms-project-api");

builder.Build().Run();