# yanitor

Yanitor is a lightweight and efficient task management application designed to help you organize your daily activities with ease. Whether you're managing personal tasks or collaborating with a team, Yanitor provides a simple and intuitive interface to keep you on track.

## TODO

- create a key vault
- create communication service for email
- add secrets to key vault
- add OTP sign in with email
- add task details
  - detailed description
  - video
  - links to products

## Data storage plan: House configurations and tasks

This section outlines how to persist users' house configurations and active tasks. It focuses on a pragmatic progression from local storage to multi-user cloud persistence.

- Phase 1: local development
  - Storage: browser `localStorage` for quick iteration (per user agent, non-secure, no sync).
  - Shape:
    - `houseConfig`: rooms, items, schedules, preferences.
    - `tasks`: generated task instances with status, due dates.
  - Pros: minimal setup, fast iteration.
  - Cons: device-bound, no backup, limited size.

- Phase 2: local file database
  - Storage: `SQLite` via `EF Core` in the Blazor Server app.
  - Entities:
    - `User` (Id, identity key, display name).
    - `House` (Id, UserId FK, name, timezone).
    - `HouseItem` (Id, HouseId FK, name, room, category, frequency, metadata).
    - `TaskTemplate` (Id, HouseId FK, item linkage, recurrence rule, default priority).
    - `ActiveTask` (Id, HouseId FK, template linkage, status, dueAt, completedAt, notes).
    - `AuditLog` (Id, UserId, action, entity, at).
  - Notes:
    - Use `UTC` for timestamps; track `Timezone` on `House`.
    - Keep `ActiveTask` immutable history via completion records or audit.
    - Add `RowVersion` for optimistic concurrency.

- Phase 3: cloud multi-user
  - Storage options:
    - `Azure SQL` + `EF Core` (recommended for relational consistency).
    - `Azure Cosmos DB` (if flexible schemas and global distribution are required).
  - Authentication: `Microsoft Entra ID` (Azure AD) or `Identity` with external providers.
  - Multi-tenancy:
    - Tenant boundary by `UserId` or `HouseId` in all tables.
    - Server-side authorization checks (policy-based).
  - Backups & migration:
    - Automated backups, migrations via `EF Core` migrations.
    - Seed default templates per locale (`en`, `nb-NO`).

- Serialization & localization
  - Persist stable identifiers; avoid storing localized strings in the database.
  - Store canonical names/keys; map to localized display via resource files.
  - Use JSON columns for flexible item metadata when needed (SQLite/Azure SQL).

- Indexing & performance
  - Index by `HouseId`, `status`, `dueAt` for task queries.
  - Background generation of `ActiveTask` from `TaskTemplate` using recurrence rules.

- Security
  - Enforce per-house authorization; validate input; use parameterized queries via `EF Core`.
  - Do not store secrets in the DB; use Azure Key Vault for configuration secrets.

## Hosting and deployment plan

This section defines hosting and deployment choices aligned with storage phases.

- Phase 1: local dev hosting
  - Host: run `Yanitor.Web` via `dotnet run` (Kestrel) on localhost.
  - Storage: browser `localStorage` only.
  - Deployment: none; developer machine.

- Phase 2: single-node server
  - Host: Blazor Server on a single VM/container.
    - Options: Docker container on `Azure Container Apps` or `Azure App Service` (Linux), or a VM.
  - Storage: `SQLite` file or `Azure SQL` (preferred for reliability).
  - Configuration:
    - Connection strings via environment variables or `Secret Manager` locally; `App Service` settings in Azure.
    - Use `Azure Key Vault` for secrets.
  - CI/CD: GitHub Actions workflow
    - Build and test.
    - Run `dotnet ef database update` for migrations on deploy (or `sqlcmd` for Azure SQL).

- Phase 3: cloud multi-user, scalable
  - Host: `Azure App Service` or `Azure Kubernetes Service` for scale-out.
  - Storage:
    - `Azure SQL` with geo-redundant backups, or `Cosmos DB` for global distribution.
  - Session & state:
    - Blazor Server requires sticky sessions or `Azure SignalR Service` to scale reliably.
  - Networking & security:
    - `Azure Front Door`/`App Gateway` (HTTPS, WAF), custom domains.
    - Private endpoints to DB, VNet integration when needed.
  - Observability:
    - `Azure Application Insights` for logging/metrics.
    - Structured logging with `ILogger`.
  - CI/CD:
    - GitHub Actions with environment gates (dev/staging/prod).
    - Blue/green or rolling deployments; DB migrations handled with pre-deploy step.

- Data considerations across environments
  - Use separate databases per environment; never share prod with dev/test.
  - Apply migrations safely (Idempotent scripts, backups before upgrade).
  - Seed data per environment where needed.

- Compliance and backups
  - Enable automated backups and retention (Azure SQL/Cosmos DB).
  - Document recovery procedures and RPO/RTO goals.

