# PMC
- Add-Migration name -Project src\ATMS.Project\ATMS.Project.Data -StartupProject src\ATMS.Project\ATMS.Project.API
- Update-Database -Project src\ATMS.Project\ATMS.Project.Data -StartupProject src\ATMS.Project\ATMS.Project.API

# CLI
- dotnet ef migrations add Initial --project src\ATMS.Project\ATMS.Project.Data --startup-project src\ATMS.Project\ATMS.Project.API
- dotnet ef database update --project src\ATMS.Project\ATMS.Project.Data --startup-project src\ATMS.Project\ATMS.Project.API
- dotnet ef migrations remove --project src\ATMS.Project\ATMS.Project.Data --startup-project src\ATMS.Project\ATMS.Project.API
