# Running ATMS on a server

Everything runs from `/srv/atms`: `docker-compose.yml`, `docker-compose.override.yml` (the server's limits, copied from
`docker-compose.<server>.yml`) and `.env`. docker compose reads all three on its own.
Images come from the private registry `ghcr.io/aykhanz`. CI copies the compose files on every deploy; `.env`
is created by hand once and never leaves the server.

## Services

| Service | What it is | Reachable from outside |
|---|---|---|
| `web` | nginx: the Angular app, HTTPS, proxy to the gateway | 80, 443 |
| `gateway` | YARP, routes `/admin` and `/project` | no |
| `admin-api`, `project-api` | the two APIs | no |
| `admin-db`, `project-db` | Postgres 16 | no |
| `rabbitmq`, `redis` | queue and cache | no |
| `volumes-init` | gives the `images` and `attachments` volumes to the API user, then exits | no |

Data lives in Docker volumes: `atms_admin-db-data`, `atms_project-db-data`, `atms_rabbitmq-data`, `atms_images`,
`atms_attachments`. `docker compose down` keeps them; `down -v` deletes them — never run it on a live server.

## First start

1. `docker login ghcr.io -u aykhanz` with a token that has only `read:packages`.
2. Copy `.env` from the local `atms-services/.env` (`scp`), with `PUBLIC_HOST` set to this server.
3. Certificate, while nothing listens on port 80 yet:
   ```bash
   sudo apt install -y certbot
   sudo certbot certonly --standalone -d "$PUBLIC_HOST" \
     --pre-hook "cd /srv/atms && docker compose stop web" \
     --post-hook "cd /srv/atms && docker compose start web"
   ```
   The hooks are saved, and certbot's timer renews the certificate on its own.
4. `docker compose up -d`. Empty databases get their tables on the first start, and Admin creates the super
   admin from `.env`.

## Everyday commands

```bash
docker compose ps                       # what runs, health of the databases and the queue
docker compose logs -f --tail 100 admin-api
docker compose pull && docker compose up -d && docker image prune -f   # what CI does on deploy
docker compose restart project-api
```

Pin a build: `ATMS_TAG=sha-1a2b3c4` in `.env`, then `docker compose up -d`.

## Migrations

New migrations are not applied on start once a database has tables. Apply them as super admin:

```bash
curl -X POST "https://$PUBLIC_HOST/admin/api/v1/migrations/up" -H "Authorization: Bearer <token>" -H "Content-Length: 0"
```

Admin migrates first, then calls Project; the answer has `admin` and `project`. To roll back, pass the migration
to roll back **to** (it stays applied, everything after it is reverted):
`POST /admin/api/v1/migrations/down?MigrationName=...` or `POST /project/api/v1/migrations/down?MigrationName=...`.
