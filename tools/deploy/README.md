# Deploy checklist

Nothing is deployed yet. This is what has to happen before the first production run.

## How settings are split

| Where | What | In git |
|---|---|---|
| `appsettings.json` | the same everywhere: token lifetimes, limits, time zone, SMTP server | yes |
| `appsettings.Development.json` | local addresses: localhost, ports | yes |
| `appsettings.Production.json` | server addresses, **empty until there is a server** | yes |
| user-secrets (local) / Key Vault (server) | the 7 secrets: both `SqlConnection`, `JwtOptions:Key`, `EmailOptions:Password`, `QueueOptions:Password`, `AdminOptions:Email`, `AdminOptions:Password` | no |

The app picks the file by `ASPNETCORE_ENVIRONMENT`: `launchSettings.json` sets `Development` locally, on a server
nothing is set, so it is `Production`. User-secrets are read only in `Development`. Any empty or missing value
stops the app at start and names it.

## Steps

1. **Fill `appsettings.Production.json`** in both APIs with the server addresses (frontend URL, gateway URL,
   RabbitMQ and Redis host).
2. **Key Vault.** Create it, give the server access, then push the secrets:
   ```powershell
   az login
   .\tools\deploy\push-secrets.ps1 -VaultName <vault> -SecretsFile <file with production values>
   ```
   The file has the shape of the local user-secrets file (right click `ATMS.Admin.API` → Manage User Secrets),
   with production values. Keep it outside the repo and delete it afterwards.
   Run in PowerShell 7 (`pwsh`).
3. **Read Key Vault in the APIs.** Packages `Azure.Extensions.AspNetCore.Configuration.Secrets` and
   `Azure.Identity`, and in both `Program.cs`:
   ```csharp
   if (builder.Environment.IsProduction())
       builder.Configuration.AddAzureKeyVault(new Uri(vaultUrl), new DefaultAzureCredential());
   ```
   Until then the same keys work as environment variables, `:` becomes `__` (`JwtOptions__Key=...`).
4. **docker compose on the server** reads its values from a `.env` next to `docker-compose.yml`. Create it on the
   server by hand with the same keys as the local `.env` (it never goes to git). The values must match the connection strings and
   `QueueOptions` in Key Vault.
5. **Close the ports.** `docker-compose.yml` publishes the database, RabbitMQ and Redis ports for local work. On the
   server only the gateway (and nginx) should be reachable from outside.
6. **Frontend.** `atms-application/src/environments/environment.prod.ts` still points at `http://localhost:5000`.
   Set the real gateway address before `npm run build`, otherwise the browser calls the user's own machine.
7. **nginx** in front: serves the built frontend, does HTTPS, proxies the API to the gateway.
8. **Rotate the old secrets.** The values that used to be in `appsettings.json` are in the git history: new Gmail
   app password, new JWT key, new database and RabbitMQ passwords.
