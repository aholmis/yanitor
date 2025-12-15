# Storage Plan: Persist House Configuration in SQL

Scope: Blazor Server app, .NET 10, EF Core with SQL provider. Persist house configuration, items, and tasks.

Assumptions and initial setup (provided):
- Single house per user (v1).
- Seed first user: Anders.
- Use SQLite for development and initial deployment.

1) Data model and mapping
- Define persistence entities: `User`, `House`, `HouseConfiguration`, `HouseRoom`, `HouseItem`, `MaintenanceTaskDef`, `ActiveTask`.
- Keys: `Guid` IDs; use `rowversion` for concurrency (emulated on SQLite via integer version or timestamp field).
- Map enums/strings (e.g., `RoomType`) with value converters.
- Create adapters to map Domain <-> EF entities to keep UI/domain pure.
- User-to-house: one-to-one (single house per user).

2) ORM and provider
- Use EF Core with SQLite provider for v1.
- Packages: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`.
- Keep SQL Server-ready abstractions for future (optional).

3) DbContext and configurations
- Implement `YanitorDbContext` with `DbSet`s for all entities.
- Use `IEntityTypeConfiguration<T>` for each entity; configure relationships, indexes, required fields.
- Owned types where appropriate.

4) Connection/configuration
- Add SQLite connection string in `appsettings.json` + `appsettings.Development.json` (e.g., `Data Source=yanitor.db`).
- Register DbContext in `Program.cs` with `AddDbContext` using `UseSqlite` (Scoped lifetime).
- Store secrets with User Secrets/Key Vault for future non-SQLite providers.

5) Migrations and setup
- Enable EF migrations; create initial migration.
- Apply migrations via CLI or at startup with `Database.Migrate()`.
- Seed initial data: create `User` (Name: Anders), create a `House` linked to Anders, and optionally a starter `HouseConfiguration`.

6) Services/repositories
- Refactor services to use EF:
  - `IHouseConfigurationService`: `HasConfigurationAsync(userId)`, `GetCurrentConfigurationAsync(userId)`, `SaveConfigurationAsync(userId, config)`, update/delete operations.
  - `IItemProvider`: load items by configuration.
  - `IActiveTaskService`: CRUD, next due computation.
- Keep domain logic in services; use `AsNoTracking` for reads.

7) Transactions and concurrency
- Wrap multi-entity updates in transactions.
- Add optimistic concurrency (SQLite: manual version column or last-write-wins with conflict checks).

8) Seeding and localization
- Seed static task definitions (`MaintenanceTaskDef`) with localization keys (`NameKey`, `DescriptionKey`).
- UI uses resource files; DB stores keys only.

9) Validation and constraints
- Required constraints and max lengths.
- Indexes on `User.Name` (unique), `NextDueDate`, and FK relationships.
- Server-side validation for builder inputs.

10) Performance
- Use projections and pagination.
- Avoid N+1; include where needed.

11) UI impact
- Update builder save flow to persist for current user (Anders initially).
- `MyHouse`/`Components` pages read counts from EF-backed services.
- Handle empty-state from DB.

12) Testing
- Unit tests for mappings and services using SQLite provider in-memory or file-based.
- Integration tests validating migrations and CRUD.

13) Deployment/ops
- Migrations in CI/CD or at startup.
- Environment-specific connection strings.
- DB health checks.

Task Breakdown and Estimates
- Entities + configurations: 4–6h
- DbContext + DI + config: 1–2h
- Migrations + seeding Anders: 1–2h
- Service refactor: 6–10h
- Mapping adapters: 2–3h
- UI wiring: 2–4h
- Tests: 3–6h
- Deployment/config: 1–2h

Next Steps
- Add EF Core packages, define entities and DbContext, scaffold initial migration, seed Anders + house, refactor services to EF, wire UI, add tests.