# Food Safety Inspection Tracker

A web application for local council food safety inspectors to record and track premises inspections and follow-up actions. Built with ASP.NET Core MVC, Entity Framework Core, and Serilog.

## Tech Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core + SQLite
- ASP.NET Core Identity (role-based auth)
- Serilog (console + rolling file sink)
- xUnit tests
- GitHub Actions CI

## Features

- **Premises management** — track food premises across towns with risk ratings
- **Inspection recording** — log inspection scores, outcomes and notes
- **Follow-up tracking** — create and manage follow-up actions with due dates
- **Dashboard** — real-time aggregations with filtering by town and risk rating
- **Role-based access** — Admin, Inspector, and Viewer roles with server-side enforcement
- **Audit logging** — all key actions logged via Serilog with structured properties
- **Global error handling** — friendly error pages with exception logging

## Test Accounts

| Role      | Email                 | Password     |
|-----------|-----------------------|--------------|
| Admin     | <admin@food.ie>         | Admin123!    |
| Inspector | <inspector@food.ie>     | Inspect123!  |
| Viewer    | <viewer@food.ie>        | Viewer123!   |

### Role Permissions

| Action                        | Admin | Inspector | Viewer |
|-------------------------------|-------|-----------|--------|
| View premises / inspections   | ✓     | ✓         | ✓      |
| Create inspections / follow-ups | ✓   | ✓         | ✗      |
| Edit / delete anything        | ✓     | ✗         | ✗      |
| View dashboard                | ✓     | ✓         | ✓      |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Run locally

```bash
git clone https://github.com/YOUR_USERNAME/oop-s2-2-mvc-75203.git
cd oop-s2-2-mvc-75203/FoodSafetyTracker
dotnet run
```

Navigate to `http://localhost:5101` and log in with one of the test accounts above.

The database is created and seeded automatically on first run. No manual setup required.

### Run tests

```bash
cd oop-s2-2-mvc-75203
dotnet test --configuration Release
```

## Logging

Serilog is configured with:

- **Console sink** — all log output visible during development
- **Rolling file sink** — daily log files written to `logs/foodsafety-YYYYMMDD.log`
- **Enrichment** — Application name, Environment, and Thread ID on every log entry

Log levels used:

- `Information` — successful create/update actions with entity IDs
- `Warning` — business rule violations (e.g. follow-up due date before inspection date)
- `Error` — caught exceptions with full context

## CI

GitHub Actions runs on every push and pull request to `main`:

- Restores dependencies
- Builds in Release configuration
- Runs all xUnit tests
