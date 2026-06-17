# Inventory KPI Monitoring System

Inventory KPI Monitoring System is a .NET 10 console application for importing product and invoice files, maintaining inventory state, and producing inventory KPI reports.

The current codebase uses a Clean Architecture layout. Domain rules and application services are separated from file parsing, JSON persistence, reporting, and console presentation.

## Project Structure

```text
src/
  Inventory.Domain/          Core entities, enums, and value objects
  Inventory.Application/     Interfaces, DTOs, import logic, FIFO costing, and KPI services
  Inventory.Infrastructure/  File readers, JSON snapshot storage, processed-file registry, reporting
  Inventory.ConsoleApp/      Console startup, file monitoring, and interactive report menu

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

## KPI Calculations

The application currently calculates:

- Total SKUs with stock or sales activity
- Total stock value
- Out-of-stock item count
- Average daily sales
- Average inventory age

## File Import

Product and invoice data are file-based JSON inputs. The console app loads historical files at startup, then monitors the product and invoice folders for additional files.

The console project links the sample data from `InventoryKpiSystem/Data` into the build output, while still resolving the root sample data folder when run from the repository root.

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

## Run

```bash
dotnet run --project src/Inventory.ConsoleApp/Inventory.ConsoleApp.csproj
```

When running, the app syncs historical product and invoice data, saves the inventory snapshot, starts folder monitoring, and opens the console report menu.

## Current Validation

- `dotnet build InventoryKpiSystem.sln` succeeded.
- `dotnet test InventoryKpiSystem.sln` succeeded.
- 5 tests passed.
