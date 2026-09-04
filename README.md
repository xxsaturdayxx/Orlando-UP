# Orlando Up

Rent a mobility scooter, wheelchair or stroller for the Orlando theme parks — delivered to your
hotel or vacation home, in English and Portuguese. Own fleet, local team.

This repository holds the website, the booking engine, the back-office and, later, the API that
the mobile app will use. Stack: .NET 10 · ASP.NET Core Razor Pages · EF Core + SQL Server ·
Stripe · Azure App Service.

## Status

Phase 0 — repository foundation (2026-09-04). No application code yet; the first application
front is `Docs/spec-01-foundation.md`. See `Docs/roadmap.md`.

## Start here

| Want to… | Read |
|---|---|
| Understand why things are the way they are | `Docs/decisions.md` |
| See how the pieces fit | `Docs/architecture.md` |
| Know what comes next | `Docs/roadmap.md` |
| Know what is still unknown | `Docs/open-questions.md` |
| Work as the coding agent | `CLAUDE.md`, then `Docs/fila-cc.md` |

## Running locally (after spec 01 is executed)

Prerequisites: .NET 10 SDK, SQL Server LocalDB (ships with Visual Studio 2022), `dotnet-ef`
(`dotnet tool install --global dotnet-ef`).

```
git config core.hooksPath .githooks
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=OrlandoUpDb;Trusted_Connection=True;" --project src/OrlandoUp.Web
dotnet ef database update --project src/OrlandoUp.Web
dotnet run --project src/OrlandoUp.Web -- seed-catalog
dotnet run --project src/OrlandoUp.Web
```

Then open the HTTPS URL printed by `dotnet run`; `/pt/` is the Portuguese site.

## Licence

Proprietary. © Ronatrip Tours & Travel / Orlando Up. All rights reserved.
