# BAIM — backend (atms-services)

BAIM is a web app for projects, tasks and teamwork, in the same family as Jira or Azure DevOps.
The main difference: the company's **clients get access too**, so they can see how their project is
going without asking a manager every day.

This repository is the backend. The Angular client lives in `../atms-application`.

## What it does

- **Projects** with a plan: groups → milestones → tickets → tasks → subtasks.
- **Tasks page** — a board (New / In Progress / Done) and a list, with filters, drag and drop and search.
- **Comments** with @mentions and links to other tickets and tasks.
- **Files** on tasks and subtasks.
- **History** of every change to a project, ticket, task or file.
- **Notifications** — the bell in the app, plus email for the things you have to act on.
- **Dashboard** — counts, charts, deadlines and recent activity.
- **Global search** by code or title.
- **Roles and permissions** — super admin, employees (PM, developer, analyst…), clients.
- **Live updates** — the board, comments and the bell update without a page reload (SignalR).
- Three languages: English, Russian, Azerbaijani.

## How it is built

```
Angular client (:4200)
        │
        ▼
Gateway (YARP, :5000)
   ├── /admin/*   ──►  Admin API   (:5216)  ──►  PostgreSQL atms_admin
   └── /project/* ──►  Project API (:5033)  ──►  PostgreSQL atms_project

Admin API  ◄── RabbitMQ events ──►  Project API      (users, invitations)
Both APIs  ──►  Redis                                 (cache)
```

| Service | What it owns |
|---|---|
| **Admin** | users, login and sessions, roles, onboarding, account emails |
| **Project** | organizations, projects, tickets, tasks, board, comments, files, history, notifications, dashboard, search |
| **Gateway** | one address for the client, routes requests to Admin or Project |

The services talk to each other only through RabbitMQ events. Each one writes the event into its
own database table (outbox) in the same transaction as the data, and a background job sends it.
The receiving side remembers what it already processed (inbox), so a message delivered twice is
handled once.

**Stack:** .NET 10, ASP.NET Core, EF Core + PostgreSQL 16, MediatR (CQRS), FluentValidation,
AutoMapper, RabbitMQ, Redis, SignalR, YARP, .NET Aspire for local runs, xUnit + Moq for tests.

## Project layout

```
src/
  ATMS.Admin/      API, Contracts, Data, Service
  ATMS.Project/    API, Contracts, Data, Services
  ATMS.Gateway/    the YARP gateway
  ATMS.Shared/     code both services use: base entities, criteria, pipeline behaviors,
                   exceptions, messaging, caching, email templates, swagger
tests/             unit tests, same layout as src: ATMS.Admin/, ATMS.Project/, ATMS.Shared/
tools/             ATMS.Email.Previewer — renders every email template to HTML
docker/            .NET Aspire AppHost
```

Inside a service the parts are always the same:

- **API** — thin controllers: take the request, send it through MediatR, return the result.
- **Contracts** — requests, commands and the models the client gets back.
- **Data** — entities, EF configurations, migrations, repositories, criteria (query filters).
- **Service(s)** — handlers, validators, mappers, domain services, background jobs, consumers.

## Run it locally

1. Start the infrastructure (two databases, RabbitMQ, Redis):

   ```bash
   docker compose up -d
   ```

2. Apply the migrations:

   ```bash
   dotnet ef database update --project src/ATMS.Admin/ATMS.Admin.Data --startup-project src/ATMS.Admin/ATMS.Admin.API
   dotnet ef database update --project src/ATMS.Project/ATMS.Project.Data --startup-project src/ATMS.Project/ATMS.Project.API
   ```

3. Start the Gateway and both APIs — either the Aspire AppHost (`docker/ATMS.Aspire.AppHost`, one
   dashboard with all logs) or each project on its own.

4. Start the client from `../atms-application` (`npm start`) and open http://localhost:4200.

| Thing | Address |
|---|---|
| Gateway | http://localhost:5000 |
| Admin API | http://localhost:5216 |
| Project API | http://localhost:5033 |
| PostgreSQL admin / project | localhost:5434 / localhost:5435 |
| RabbitMQ UI | http://localhost:15672 |
| Redis | localhost:6379 |

New migration (example for Project):

```bash
dotnet ef migrations add <Name> --project src/ATMS.Project/ATMS.Project.Data --startup-project src/ATMS.Project/ATMS.Project.API
```

## Tests

```bash
dotnet test atms-services.sln
```

Only unit tests live here: no database and no Docker, the whole suite runs in under a minute.
