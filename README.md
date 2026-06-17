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
- Tracks processed files in `InventoryKpiSystem/processed-files/processed-files.json`.
- Writes JSON reports to `InventoryKpiSystem/reports`.
- Exposes inventory, product, KPI, and import workflows through ASP.NET Core endpoints.
- Includes a PostgreSQL EF Core foundation for future database persistence work.

File-based import behavior remains in place. The PostgreSQL schema foundation is available, but FIFO and KPI workflows have not been rewritten to use database persistence yet.

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

Database-backed runtime workflows are planned as future work. The current API still uses the file-based sample/runtime data under `InventoryKpiSystem/` for import, inventory state, and reports.

## PostgreSQL

`Inventory.Infrastructure` contains the EF Core foundation:

- `InventoryDbContext`
- Entity mappings under `src/Inventory.Infrastructure/Persistence/Configurations`
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

## Reports

KPI reports are written as formatted JSON files with timestamped names:

```text
InventoryKpiSystem/reports/kpi-report-yyyyMMddHHmmss.json
```

## Tests

Unit tests live under `tests/Inventory.Application.Tests` and cover the application-level inventory and KPI behavior.

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
- 5 tests passed.
