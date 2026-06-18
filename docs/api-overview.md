# API Overview

The API is available at `http://localhost:5258` when running with Docker Compose.

Swagger UI:

```text
http://localhost:5258/swagger
```

## Endpoints

| Method | Path | Purpose |
| --- | --- | --- |
| GET | `/health` | Checks whether the API process is running. |
| GET | `/health/db` | Checks PostgreSQL connectivity. |
| GET | `/api/products` | Lists persisted products, falling back to in-memory state when needed. |
| GET | `/api/inventory` | Lists inventory quantities, value, sales, and FIFO purchase lots. |
| GET | `/api/kpis` | Calculates KPI values from current inventory state. |
| POST | `/api/import/run` | Imports file-based sample data and persists the resulting state to PostgreSQL. |

## Response Shape

Successful responses use:

```json
{
  "success": true,
  "data": {}
}
```

Errors use ASP.NET Core `ProblemDetails`:

```json
{
  "type": "about:blank",
  "title": "Database connection failed.",
  "status": 503,
  "detail": "The API could not connect to the configured PostgreSQL database."
}
```

Every response includes `X-Correlation-Id`.

## Quick Calls

```bash
curl http://localhost:5258/health
curl http://localhost:5258/health/db
curl http://localhost:5258/api/products
curl http://localhost:5258/api/inventory
curl http://localhost:5258/api/kpis
curl -X POST http://localhost:5258/api/import/run
```

Docker startup seeds demo data when PostgreSQL is empty, so `GET /api/products`, `GET /api/inventory`, and `GET /api/kpis` are useful before running import manually.
