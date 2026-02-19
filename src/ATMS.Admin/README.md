# PMC
- Add-Migration name -Project src\ATMS.Admin\ATMS.Admin.Data -StartupProject src\ATMS.Admin\ATMS.Admin.API
- Update-Database -Project src\ATMS.Admin\ATMS.Admin.Data -StartupProject src\ATMS.Admin\ATMS.Admin.API

# CLI
- dotnet ef migrations add name --project src\ATMS.Admin\ATMS.Admin.Data --startup-project src\ATMS.Admin\ATMS.Admin.API
- dotnet ef database update --project src\ATMS.Admin\ATMS.Admin.Data --startup-project src\ATMS.Admin\ATMS.Admin.API