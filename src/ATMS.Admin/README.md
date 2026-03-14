# PMC
- Add-Migration name -Project src\ATMS.Admin\ATMS.Admin.Data -StartupProject src\ATMS.Admin\ATMS.Admin.API
- Update-Database -Project src\ATMS.Admin\ATMS.Admin.Data -StartupProject src\ATMS.Admin\ATMS.Admin.API

# CLI
- dotnet ef migrations add name --project src\ATMS.Admin\ATMS.Admin.Data --startup-project src\ATMS.Admin\ATMS.Admin.API
- dotnet ef database update --project src\ATMS.Admin\ATMS.Admin.Data --startup-project src\ATMS.Admin\ATMS.Admin.API

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

        var redirectUrlOptions = configuration.GetSection(nameof(RedirectUrlOptions)).Get<RedirectUrlOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.RedirectUrlSectionNotFound,
            $"Configuration for section '{nameof(RedirectUrlOptions)}' is not found or could not be loaded.");
```
