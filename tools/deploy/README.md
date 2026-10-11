# Deploy checklist

How the services get their settings, and what to check before a server goes live. Running the stack on a server
is described in `deploy/README.md`.

## How settings are split

| Where | What | In git |
|---|---|---|
| `appsettings.json` | the same everywhere: token lifetimes, limits, time zone, SMTP server | yes |
| `appsettings.Development.json` | local addresses: localhost, ports | yes |
| `appsettings.Production.json` | addresses inside the docker network (`rabbitmq`, `redis`, `admin-api:8080`), the same on every server | yes |
| user-secrets (local) / `.env` on the server | the secrets and everything that differs per server (public host, passwords, JWT key) | no |

The app picks the file by `ASPNETCORE_ENVIRONMENT`: `launchSettings.json` sets `Development` locally, compose sets
`Production`. User-secrets are read only in `Development`. On a server `deploy/docker-compose.yml` turns `.env` into
environment variables, `:` becomes `__` (`JwtOptions__Key=...`). Any empty or missing value stops the app at start
and names it.

Key Vault is not used: one of the servers is not in Azure, and both servers take their values the same way.
`push-secrets.ps1` is kept from that plan and is not part of the deploy.

## Steps

1. **`.env` on the server** — a copy of the local `atms-services/.env` (it holds the server keys too), never in git.
2. **Ports.** Only `web` publishes ports (80, 443). Databases, RabbitMQ, Redis and the gateway stay inside the
   docker network. The root `docker-compose.yml` with open ports is for local work only.
3. **Frontend.** `environment.prod.ts` has an empty `apiUrl`: the browser calls `/admin/...` and `/project/...` on the
   host the page came from, and nginx in the `atms-web` image proxies them to the gateway.
4. **nginx** (`atms-application/nginx/default.conf.template`): HTTPS, the Angular app, proxy to the gateway.
5. **Rate limiting** (`Specs/20-rate-limiting.md`). The APIs limit per user and per action; nginx cuts plain
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
   - **`ProxyOptions:KnownNetworks`** in `appsettings.Production.json` of the Gateway and both APIs is
     `"172.30.0.0/24"`, the subnet fixed in `deploy/docker-compose.yml`; change both together. Empty means only localhost is trusted, so
     every user gets the proxy's IP and the whole site shares one login limit. Never trust everyone: anyone could
     then send a fake `X-Forwarded-For` and get around every IP limit.
   - Only nginx is reachable from outside, not the gateway (step 2) — otherwise its IP limit is skipped.
   - With Cloudflare in front, nginx takes the visitor's IP from Cloudflare, and only from Cloudflare addresses:
     ```nginx
     real_ip_header CF-Connecting-IP;
     set_real_ip_from <each Cloudflare range from cloudflare.com/ips>;
     ```
   - `RateLimitOptions:Enabled = false` switches the limits off. Any number from the spec can be changed in
     `appsettings.json` (`RateLimitOptions:Creates:DailyLimit` …) without a rebuild, a restart is enough.
   - `NotificationsOptions:MaxEmailsPerDay = 300` is sized for Gmail (about 500 a day). With SendGrid, SES or
     Mailgun raise it.
6. **Rotate the old secrets.** The values that used to be in `appsettings.json` are in the git history: new Gmail
   app password, new JWT key, new database and RabbitMQ passwords.
