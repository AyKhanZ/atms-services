# PMC
- Add-Migration name -Project src\ATMS.Admin\ATMS.Admin.Data -StartupProject src\ATMS.Admin\ATMS.Admin.API
- Update-Database -Project src\ATMS.Admin\ATMS.Admin.Data -StartupProject src\ATMS.Admin\ATMS.Admin.API

# CLI
- dotnet ef migrations add name --project src\ATMS.Admin\ATMS.Admin.Data --startup-project src\ATMS.Admin\ATMS.Admin.API
- dotnet ef database update --project src\ATMS.Admin\ATMS.Admin.Data --startup-project src\ATMS.Admin\ATMS.Admin.API
- dotnet ef migrations remove --project src\ATMS.Admin\ATMS.Admin.Data --startup-project src\ATMS.Admin\ATMS.Admin.API

---
#### If Error(1):
```aiignore
    Build started...
    Build succeeded.
    The Entity Framework tools version '9.0.4' is older than that of the runtime '10.0.3'.
    Update the tools for the latest features and bug fixes. See https://aka.ms/AAc1fbw for more information.
```
#### Then:
```aiignore
    PS C:\Projects\Learning\.Net> dotnet tool update --global dotnet-ef
    Tool 'dotnet-ef' was successfully updated from version '9.0.4' to version '10.0.5'.
```

---

```aiignore
        var adminOptions = configuration.GetSection(nameof(AdminOptions)).Get<AdminOptions>() 
            ?? throw new ConfigurationException(ConfigurationErrorType.AdminSectionNotFound,
            $"Configuration for section '{nameof(AdminOptions)}' is not found or could not be loaded.");

        var imagesOptions = configuration.GetSection(nameof(ImagesOptions)).Get<ImagesOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.ImagesSectionNotFound,
            $"Configuration for section '{nameof(ImagesOptions)}' is not found or could not be loaded.");
            
        var queueOptions = configuration.GetSection(nameof(QueueOptions)).Get<QueueOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.QueueSectionNotFound,
            $"Configuration for section '{nameof(QueueOptions)}' is not found or could not be loaded.");
```

```
    -- Tables DROP
        DROP TABLE "__EFMigrationsHistory";
        DROP TABLE "Genders";
        DROP TABLE "Roles";
        DROP TABLE "MaritalStatuses";
        DROP TABLE "Permissions";
        DROP TABLE "RefreshRevokedTokens";
        DROP TABLE "RolePermissions";
        DROP TABLE "UserRoles";
        DROP TABLE "Users";
        DROP TABLE "UserStatuses";
    
    -- Migrations
        select *
        from "__EFMigrationsHistory";
    
    -- Dictionaries
        SELECT * FROM "Permissions";
        SELECT * FROM "Genders";
        SELECT * FROM "MaritalStatuses";
        SELECT * FROM "UserStatuses";
    
    -- Entities
        SELECT * FROM "Users";
        SELECT * FROM "Roles";
        SELECT * FROM "UserRoles";
        SELECT * FROM "RolePermissions";
        SELECT * FROM "RefreshRevokedTokens";
```