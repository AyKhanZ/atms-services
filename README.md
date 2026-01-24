# ATMS

**ATMS** — Activity Tracking Management System

A modular, microservice-based platform for managing work items, tasks, features, bugs, and team collaboration. Designed to support any project methodology — Agile, Scrum, Kanban, or Waterfall — in an enterprise-ready environment.

---

## 🚀 Features

- **Work Items & Tasks**: Create, assign, track, and manage tasks across multiple projects.
- **Epics & Features**: Organize work hierarchically for long-term planning.
- **Bugs & Issues**: Track defects and progress through resolution.
- **Boards & Dashboards**: Visualize tasks with Kanban boards and metrics dashboards.
- **Comments & Collaboration**: Team communication directly on tasks.
- **Notifications & Messenger**: Internal messaging system for team updates.
- **Multi-Methodology Support**: Works for Agile, Scrum, Waterfall, or custom workflows.
- **Microservices Architecture**: Independent services for scalability and maintainability.

---

## 🏗 Architecture Overview

ATMS is designed as a microservice platform:

| Service                 | Responsibility                                   |
|-------------------------|-------------------------------------------------|
| **atms-auth**           | User authentication and authorization           |
| **atms-workitems**      | CRUD operations for tasks, epics, features     |
| **atms-boards**         | Board and workflow management                   |
| **atms-comments**       | Task and item commenting                        |
| **atms-messenger**      | Internal team messaging                         |
| **atms-dashboard**      | Aggregated metrics, analytics, reporting       |

All services communicate over REST APIs.

---

## ⚡ Tech Stack

- **Backend:** .NET 10 / C# (microservices)
- **Frontend:** Angular 21
- **Database:** PostgreSQL / MongoDB (per service)
- **Messaging / Event Bus:** RabbitMQ / Kafka
- **Authentication:** JWT / IdentityServer
- **Containerization:** Docker & Docker Compose
- **CI/CD:** GitHub Actions (optional: can integrate with Azure DevOps)

---

## 📦 Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL / MongoDB running locally or via Docker
