# PMC
- Add-Migration name -Project src\ATMS.Admin\ATMS.Admin.Data -StartupProject src\ATMS.Admin\ATMS.Admin.API
- Update-Database -Project src\ATMS.Admin\ATMS.Admin.Data -StartupProject src\ATMS.Admin\ATMS.Admin.API

# CLI
- dotnet ef migrations add Initial --project src\ATMS.Admin\ATMS.Admin.Data --startup-project src\ATMS.Admin\ATMS.Admin.API
- dotnet ef database update --project src\ATMS.Admin\ATMS.Admin.Data --startup-project src\ATMS.Admin\ATMS.Admin.API
- dotnet ef migrations remove --project src\ATMS.Admin\ATMS.Admin.Data --startup-project src\ATMS.Admin\ATMS.Admin.API

- dotnet ef migrations add Initial --project src\ATMS.Project\ATMS.Project.Data --startup-project src\ATMS.Project\ATMS.Project.API
- dotnet ef database update --project src\ATMS.Project\ATMS.Project.Data --startup-project src\ATMS.Project\ATMS.Project.API

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

```angular2html
PS C:\Projects\Learning\.Net\src> dotnet ef database update --project src\ATMS.Admin\ATMS.Admin.Data --startup-project src\ATMS.Admin\ATMS.Admin.API
Could not execute because the specified command or file was not found.
Possible reasons for this include:
  * You misspelled a built-in dotnet command.
  * You intended to execute a .NET program, but dotnet-ef does not exist.
  * You intended to run a global tool, but a dotnet-prefixed executable with this name could not be found on the PATH.

dotnet new tool-manifest
dotnet tool install dotnet-ef
dotnet tool run dotnet-ef database update --project src\ATMS.Admin\ATMS.Admin.Data --startup-project src\ATMS.Admin\ATMS.Admin.API
dotnet tool run dotnet-ef database update --project src\ATMS.Project\ATMS.Project.Data --startup-project src\ATMS.Project\ATMS.Project.API
```

---

```aiignore

        private readonly ImagesOptions _imagesOptions =
            configuration.GetSection(nameof(ImagesOptions)).Get<ImagesOptions>()
                ?? throw new ConfigurationException(ConfigurationErrorType.ImagesSectionNotFound,
                    string.Format(LogMessages.ConfigSectionNotFound, nameof(ImagesOptions)));
                    
        private readonly QueueOptions _queueOptions =
            configuration.GetSection(nameof(QueueOptions)).Get<QueueOptions>()
                ?? throw new ConfigurationException(ConfigurationErrorType.ImagesSectionNotFound,
                    string.Format(LogMessages.ConfigSectionNotFound, nameof(QueueOptions)));
```

```
    -- Tables DROP
        DROP TABLE "__EFMigrationsHistory";

        DROP TABLE "RefreshRevokedTokens";
        DROP TABLE "PasswordResetTokens";
        
        DROP TABLE "RolePermissions";
        DROP TABLE "UserRoles";
        
        DROP TABLE "GenderTranslations";
        DROP TABLE "MaritalStatusTranslations";
        DROP TABLE "UserStatusTranslations";
        DROP TABLE "PermissionTranslations";
        DROP TABLE "UserTypeTranslations";
        
        DROP TABLE "Roles";
        DROP TABLE "Permissions";
        
        DROP TABLE "Users";
        
        DROP TABLE "Genders";
        DROP TABLE "UserTypes";
        DROP TABLE "MaritalStatuses";
        DROP TABLE "UserStatuses";
    
    -- Migrations
        select *
        from "__EFMigrationsHistory";
    
    -- Dictionaries
        select * from "Permissions";
        select * from "UserStatuses";
        select * from "MaritalStatuses";
        select * from "Genders";
        
        select * from "GenderTranslations";
        select * from "MaritalStatusTranslations";
        select * from "UserStatusTranslations";
        select * from "PermissionTranslations";
    
    -- Entities
        SELECT * FROM "Users";
        SELECT * FROM "Roles";
        SELECT * FROM "UserRoles";
        SELECT * FROM "RolePermissions";
        SELECT * FROM "RefreshRevokedTokens";
```

```angular2html
    select * from "__EFMigrationsHistory";
    
    select * from "ProjectKindTranslations";

    select * from "ProjectKinds";
```