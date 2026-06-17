# Inventory KPI Monitoring System

Inventory KPI Monitoring System is a .NET 10 application for importing product and invoice files, maintaining inventory state, and producing inventory KPI reports.

The current codebase uses a Clean Architecture layout. Domain rules and application services are separated from file parsing, JSON persistence, reporting, console presentation, and the HTTP API layer.

## Project Structure

```text
src/
  Inventory.Domain/          Core entities, enums, and value objects
  Inventory.Application/     Interfaces, DTOs, import logic, FIFO costing, and KPI services
  Inventory.Infrastructure/  File readers, JSON snapshot storage, processed-file registry, reporting
  Inventory.ConsoleApp/      Console startup, file monitoring, and interactive report menu
  Inventory.Api/             ASP.NET Core Web API endpoints and OpenAPI documentation

tests/
  Inventory.Application.Tests/  Unit tests for application services

InventoryKpiSystem/
  Data/Products/product.txt     Sample product import data
  Data/Invoices/*.txt           Sample invoice import data
  processed-files/              Runtime idempotency registry
  reports/                      JSON KPI report output
  inventory-snapshot.json       Runtime inventory snapshot
```

## Core Behavior

- Imports product files from `InventoryKpiSystem/Data/Products`.
- Imports invoice files from `InventoryKpiSystem/Data/Invoices`.
- Processes purchase invoices as stock additions.
- Processes sales invoices as stock reductions.
- Applies FIFO inventory logic by consuming the oldest purchase lots first.
- Calculates inventory KPIs from the current inventory state.
- Persists inventory state to `InventoryKpiSystem/inventory-snapshot.json`.
- Persists imported products, invoices, inventory items, stock lots, and stock movements to PostgreSQL through the API import endpoint.
- Tracks processed files in `InventoryKpiSystem/processed-files/processed-files.json`.
- Writes JSON reports to `InventoryKpiSystem/reports`.
- Exposes inventory, product, KPI, and import workflows through ASP.NET Core endpoints.
- Includes PostgreSQL EF Core repositories for persisted inventory state.

File-based import behavior remains the source of input data. PostgreSQL is used for persisted inventory state after `POST /api/import/run`. FIFO and KPI business logic are unchanged.

## KPI Calculations

The application currently calculates:

- Total SKUs with stock or sales activity
- Total stock value
- Out-of-stock item count
- Average daily sales
- Average inventory age

## File Import

Product and invoice data are file-based JSON inputs. The console app loads historical files at startup, then monitors the product and invoice folders for additional files. The API can run the same import workflow through `POST /api/import/run`.

The console project links the sample data from `InventoryKpiSystem/Data` into the build output, while still resolving the root sample data folder when run from the repository root.

## API

`Inventory.Api` is an ASP.NET Core Web API project that exposes the existing application services over HTTP without changing the FIFO or KPI business logic.

API endpoint mappings are organized under `src/Inventory.Api/Endpoints`, and API dependency registration lives under `src/Inventory.Api/Extensions`.

Endpoints:

- `GET /health`
- `GET /health/db`
- `GET /api/products`
- `GET /api/inventory`
- `GET /api/kpis`
- `POST /api/import/run`

Swagger UI and OpenAPI documentation are available at:

```text
/swagger
/openapi/v1.json
```

The current API still reads source import files from `InventoryKpiSystem/`. After import, `GET /api/products`, `GET /api/inventory`, and `GET /api/kpis` read from PostgreSQL when database data is available.

Successful API responses use a consistent wrapper:

```json
{
  "success": true,
  "data": {}
}
```

Common error responses use ASP.NET Core `ProblemDetails`, including:

- `400 Bad Request` for unsupported request payloads.
- `404 Not Found` for missing import folders or missing import files.
- `503 Service Unavailable` when PostgreSQL health checks fail.
- `500 Internal Server Error` when import processing or database persistence fails unexpectedly.

## PostgreSQL

`Inventory.Infrastructure` contains the EF Core persistence layer:

- `InventoryDbContext`
- Entity mappings under `src/Inventory.Infrastructure/Persistence/Configurations`
- Repository implementations under `src/Inventory.Infrastructure/Persistence/Repositories`
- Initial migration under `src/Inventory.Infrastructure/Persistence/Migrations`

The API registers `InventoryDbContext` with the `InventoryDb` connection string.

Default connection string shape:

```json
{
  "ConnectionStrings": {
    "InventoryDb": "Host=localhost;Port=5432;Database=inventory_kpi;Username=postgres;Password=postgres"
  }
}
```

Apply migrations:

```bash
dotnet ef database update --project src/Inventory.Infrastructure/Inventory.Infrastructure.csproj --startup-project src/Inventory.Api/Inventory.Api.csproj --context InventoryDbContext
```

Create a new migration:

```bash
dotnet ef migrations add MigrationName --project src/Inventory.Infrastructure/Inventory.Infrastructure.csproj --startup-project src/Inventory.Api/Inventory.Api.csproj --context InventoryDbContext --output-dir Persistence/Migrations
```

Database health check:

```text
GET /health/db
```

Run the database-backed import:

```bash
curl -X POST http://localhost:5258/api/import/run
```

Then view persisted data:

```text
GET /api/products
GET /api/inventory
GET /api/kpis
```

LLM features are not included in the current scope.

## Reports

KPI reports are written as formatted JSON files with timestamped names:

```text
InventoryKpiSystem/reports/kpi-report-yyyyMMddHHmmss.json
```

## Tests

Unit tests live under `tests/Inventory.Application.Tests` and cover the application-level inventory and KPI behavior.

API integration tests use `Microsoft.AspNetCore.Mvc.Testing` with `WebApplicationFactory`. Repository and database health services are replaced with test doubles so endpoint tests can run without a live PostgreSQL instance.

## Build

```bash
dotnet build InventoryKpiSystem.sln
```

## Test

```bash
dotnet test InventoryKpiSystem.sln
```

## Run Console App

```bash
dotnet run --project src/Inventory.ConsoleApp/Inventory.ConsoleApp.csproj
```

When running, the app syncs historical product and invoice data, saves the inventory snapshot, starts folder monitoring, and opens the console report menu.

## Run API

```bash
dotnet run --project src/Inventory.Api/Inventory.Api.csproj
```

The API reads and writes the same file-based sample/runtime data under `InventoryKpiSystem/`.

## Current Validation

- `dotnet build InventoryKpiSystem.sln` succeeded.
- `dotnet test InventoryKpiSystem.sln` succeeded.
- 10 tests passed.
