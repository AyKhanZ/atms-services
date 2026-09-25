# Realtime interceptor PostgreSQL tests

The database transaction tests in `ProjectRealtimeInterceptorTest` use a PostgreSQL connection from `ATMS_REALTIME_TEST_DB`. The database must contain `public."Tasks"`; each test creates a temporary copy of that table and leaves no test rows in the persistent table. Without the variable, these tests are reported as skipped.

Run them from `atms-services` in PowerShell:

```powershell
$env:ATMS_REALTIME_TEST_DB = 'Host=localhost;Port=5432;Database=atms_project;Username=<user>;Password=<password>'
dotnet test tests/Project.Services.Tests/Project.Services.Tests.csproj -c Release --filter FullyQualifiedName~ProjectRealtimeInterceptorTest
```
