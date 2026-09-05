# Orlando Up

Rent a mobility scooter, wheelchair or stroller for the Orlando theme parks — delivered to your
hotel or vacation home, in English and Portuguese. Own fleet, local team.

This repository holds the website, the booking engine, the back-office and, later, the API that
the mobile app will use. Stack: .NET 10 · ASP.NET Core Razor Pages · EF Core + SQL Server ·
Stripe · Azure App Service.

## Status

Phase 1 — application foundation, done 2026-09-05 (leva 01, content commit `5d538ba`): the site
runs in English and Portuguese from the database, with the catalog seed, staff login at `/admin`,
63 tests and 18 controls green. Prices and company data are placeholders (`Docs/open-questions.md`
Q1, Q2, Q9). Next: phase 2, the public site with real fleet data. See `Docs/roadmap.md`.

## Start here

| Want to… | Read |
|---|---|
| Understand why things are the way they are | `Docs/decisions.md` |
| See how the pieces fit | `Docs/architecture.md` |
| Know what comes next | `Docs/roadmap.md` |
| Know what is still unknown | `Docs/open-questions.md` |
| Work as the coding agent | `CLAUDE.md`, then `Docs/fila-cc.md` |

## Running locally

Prerequisites: .NET 10 SDK, SQL Server LocalDB (ships with Visual Studio 2022), `dotnet-ef`
(`dotnet tool install --global dotnet-ef`), and a trusted HTTPS development certificate
(`dotnet dev-certs https --check`, else `dotnet dev-certs https --trust`).

**Secrets never enter the repository** (`Docs/decisions.md` D24). The three keys below live in
user-secrets; you write the values, and the project already carries its user-secrets id.

```
git config core.hooksPath .githooks
dotnet restore

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=OrlandoUpDb;Trusted_Connection=True;" --project src/OrlandoUp.Web
dotnet user-secrets set "AdminSeed:Email" "<the first admin's e-mail>" --project src/OrlandoUp.Web
dotnet user-secrets set "AdminSeed:Password" "<12 characters or more>" --project src/OrlandoUp.Web

dotnet ef database update --project src/OrlandoUp.Web
dotnet run --project src/OrlandoUp.Web -- seed-catalog
dotnet run --project src/OrlandoUp.Web -- seed-admin

dotnet build
dotnet test
dotnet run --project src/OrlandoUp.Web
```

The application never creates or migrates the schema on start-up (D12): `dotnet ef database
update` is an explicit command, and a missing connection string is a start-up failure, not a
silent fallback. An `AdminSeed:Password` shorter than 12 characters is refused by `seed-admin`
with the reason printed, and no account is half-created.

**Ports** (`src/OrlandoUp.Web/Properties/launchSettings.json`): HTTPS **7420**, HTTP **5420**.

| Address | What it is |
|---|---|
| `https://localhost:7420/` | the site in English |
| `https://localhost:7420/pt` | the same site in Portuguese |
| `https://localhost:7420/admin` | back-office; anonymous is sent to `/admin/login` |
| `https://localhost:7420/healthz` | `200` with `{"status":"ok","database":"ok"}`; `503` and `"database":"unreachable"` when the database is down |
| `https://localhost:7420/robots.txt` | `Disallow: /` while the site is not public |

`seed-catalog` writes 7 products, 6 add-ons and 4 delivery zones (112 rows); `seed-admin` creates
the `Admin` and `Staff` roles and the first account. Both refuse to write over rows that already
exist, so re-running them is safe.

The measurable invariants of this front are in `Docs/controles/foundation.tsv`; the gate that reads
them, run at the end of every front, is:

```
bash Docs/medir-controles.sh verificar Docs/controles/foundation.tsv
```

## Licence

Proprietary. © Ronatrip Tours & Travel / Orlando Up. All rights reserved.
