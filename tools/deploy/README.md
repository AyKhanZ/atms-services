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
   server only nginx should be reachable from outside; the gateway sits behind it.
6. **Frontend.** `atms-application/src/environments/environment.prod.ts` still points at `http://localhost:5000`.
   Set the real gateway address before `npm run build`, otherwise the browser calls the user's own machine.
7. **nginx** in front: serves the built frontend, does HTTPS, proxies the API to the gateway.
8. **Rate limiting** (`Specs/20-rate-limiting.md`). The APIs limit per user and per action; nginx cuts plain
   floods per IP before they reach .NET.
   - nginx, in front of the gateway:
     ```nginx
     limit_req_zone  $binary_remote_addr zone=api:10m rate=20r/s;
     limit_conn_zone $binary_remote_addr zone=addr:10m;

     location / {
         limit_req zone=api burst=50 nodelay;
         limit_req_status 429;
         limit_conn addr 30;
         limit_conn_status 429;
         client_max_body_size 26m;   # attachments are up to 25 MB

         proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
         proxy_set_header X-Forwarded-Proto $scheme;
         proxy_pass http://gateway;
     }
     ```
     `429`, not the default `503`: the frontend treats `503` as "server is down" and opens that page.
   - **`ProxyOptions:KnownNetworks`** in `appsettings.Production.json`: the nginx address in the Gateway, the docker
     network of the gateway in both APIs (CIDR, e.g. `"172.18.0.0/16"`). Empty means only localhost is trusted, so
     every user gets the proxy's IP and the whole site shares one login limit. Never trust everyone: anyone could
     then send a fake `X-Forwarded-For` and get around every IP limit.
   - Only nginx is reachable from outside, not the gateway (step 5) — otherwise its IP limit is skipped.
   - With Cloudflare in front, nginx takes the visitor's IP from Cloudflare, and only from Cloudflare addresses:
     ```nginx
     real_ip_header CF-Connecting-IP;
     set_real_ip_from <each Cloudflare range from cloudflare.com/ips>;
     ```
   - `RateLimitOptions:Enabled = false` switches the limits off. Any number from the spec can be changed in
     `appsettings.json` (`RateLimitOptions:Creates:DailyLimit` …) without a rebuild, a restart is enough.
   - `NotificationsOptions:MaxEmailsPerDay = 300` is sized for Gmail (about 500 a day). With SendGrid, SES or
     Mailgun raise it.
9. **Rotate the old secrets.** The values that used to be in `appsettings.json` are in the git history: new Gmail
   app password, new JWT key, new database and RabbitMQ passwords.
