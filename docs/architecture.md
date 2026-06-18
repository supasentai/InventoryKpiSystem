# Architecture

The Inventory KPI Monitoring System uses a Clean Architecture layout.

```text
Reviewer / Developer
        |
        v
Inventory.Api
  - Minimal API endpoints
  - Swagger/OpenAPI
  - Request logging, correlation id, ProblemDetails
        |
        v
Inventory.Application
  - Import workflow contracts
  - FIFO costing service
  - KPI service
        |
        v
Inventory.Domain
  - Product
  - InventoryItem
  - StockLot
  - StockMovement
  - Invoice and InvoiceLine
        ^
        |
Inventory.Infrastructure
  - File readers
  - JSON report and snapshot storage
  - EF Core PostgreSQL persistence
  - Optional database seed data
```

## Runtime Flow

```text
File-based sample data
        |
        v
POST /api/import/run
        |
        v
Application import + FIFO logic
        |
        v
PostgreSQL persisted inventory state
        |
        v
GET /api/products
GET /api/inventory
GET /api/kpis
```

Docker startup applies EF Core migrations and seeds demo data when the database is empty. Existing import behavior remains available for replacing the seeded state with file-based input data.

## Boundaries

- Domain contains business entities and rules.
- Application contains use-case services and contracts.
- Infrastructure contains file, JSON, PostgreSQL, repository, and seeding details.
- API exposes application behavior over HTTP.
- Tests cover application services and API endpoints.

No frontend, authentication, message queue, cache, Kubernetes, or LLM features are included.
