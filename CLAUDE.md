# Orlando Up — project context for the coding agent

Bilingual (en-US / pt-BR) rental storefront + back-office for mobility scooters, wheelchairs and
strollers delivered to hotels around the Orlando theme parks. Own fleet, operated by Ronatrip
(Orlando) under the brand **Orlando Up**, domain `orlandoup.com`. Single developer; you are the
implementer, Claude Web (Cowork) writes the specs, Rod is the operator and decides.

## Where things live — read before deciding

- `Docs/decisions.md` — numbered decisions **with reasons**. A decision is changed by a new
  numbered line, never by silently doing otherwise.
- `Docs/architecture.md` — stack, layout, domain model, localization, payments, environments.
- `Docs/roadmap.md` — phases; nothing from a later phase is started while an earlier line is open.
- `Docs/open-questions.md` — facts only Rod has, each with the assumption in force until answered.
- `Docs/fila-cc.md` — **the instruction queue.** You execute exactly the line named by the operator
  ("execute the `aguardando` line whose description starts with …"), from the spec it points to.
- `Docs/spec-*.md` — one per front. `Docs/resumo-conversa-N.md` — how each conversation closed.
- `Docs/controles/*.tsv` + `Docs/medir-controles.sh` — measurable invariants; run
  `bash Docs/medir-controles.sh verificar Docs/controles/<file>.tsv` at the end of every front.
- `Docs/market-notes.md` — park rules and competitor facts; prices there are dated, re-check
  before they reach a page.

## Stack (Docs/decisions.md D10–D16)

.NET 10 LTS · ASP.NET Core **Razor Pages** (site + `/admin`) · **Minimal APIs** under `/api/v1` ·
EF Core 10 + SQL Server (LocalDB locally, Azure SQL in the cloud) · ASP.NET Core Identity for
staff only · Stripe Checkout (hosted) · Brevo SMTP behind `IEmailSender` · Azure App Service Linux ·
GitHub Actions. One web project `src/OrlandoUp.Web` layered by folder (`Domain/`, `Application/`,
`Infrastructure/`, `Pages/`, `Api/`) and one test project `tests/OrlandoUp.Tests`; the
architecture test enforces the layering.

## Rules that prevent silent damage

- **English** for code, identifiers, UI default, commit messages and engineering docs; the process
  files (`fila-cc`, `resumo-conversa-N`, `atrito-conversa-N`, `backlog-conhecido`, `controles`)
  stay in Portuguese (D1).
- **Money** is `decimal(10,2)` USD; cents only inside the Stripe adapter. **A null price never
  coalesces to zero** — missing price is an error (D15).
- **Time**: rental/delivery dates are `DateOnly` (+ `TimeOnly` windows) in Orlando; audit fields are
  UTC instants named `…Utc`. All "now/today" comes from `IClock`. Never `DateTime.Now` (D16).
- **Culture**: formatting culture is always `en-US`; only the UI culture switches (`en-US`,
  `pt-BR`). Never let `decimal` binding meet a comma (D20). Public URLs: `/…` English, `/pt/…`
  Portuguese (D21). Every user-visible string goes through the localizer or a translation table;
  a resource key present in one culture and missing in the other fails the tests.
- **Secrets never enter the repository** — user-secrets locally, App Service settings in Azure.
  Never emit a command with an invented value or a placeholder; ask for the real value (D24).
- **Database**: no `Migrate()`/`EnsureCreated()` on boot; migrations are explicit commands.
  Additive migrations are applied to production **before** the code that needs them is deployed.
  Before any `CREATE UNIQUE INDEX` on a table with data, check duplicates over **all** rows. Run
  `SELECT DB_NAME()` before any data query outside local development (D12).
- **Bookings and prices** are snapshotted at booking time; a later price change never alters a paid
  booking. Status transitions are validated in `Domain`, not in pages.
- **Effects** (e-mail, Stripe, webhooks) are neutralized in tests and in local runs before the first
  scenario — the outbox and the recipient allow-list exist for that.
- **Accessibility is a requirement** (D9): labelled inputs, visible focus, 4.5:1 contrast, no
  colour-only meaning, keyboard-reachable everything.

## Process

- **Reversible change is an adjustment, not a front**: CSS, markup, text, refactor, docs — execute
  directly, one commit, report after. Touching database, migration, real e-mail, deploy,
  authentication or secrets is a front: spec + queue line + plan reviewed before the first file
  changes.
- **Step 0 of every front, before altering any file**: read the spec, measure what it asserts
  (`grep`, `wc`, `dotnet --list-sdks`), write the plan to `scratchpad/<front>/plano.md` (not
  committed), report any contradiction between spec and tree in the plan — the spec is never
  obeyed against the measurement.
- **Every artefact you produce is a file in the repository and is committed without waiting for
  "commit it"** — stop reports in `Docs/relatorio-<front>-etapa-N.md`, committed before asking
  for approval. Code, config and production data still require explicit confirmation.
- A queue line goes `aguardando` → `concluido` with the commit hash, edited in place; a finished
  line is never rewritten. Fronts close with **two commits**: content first, then the closing
  commit that records the content hash in the queue.
- Session open and close: `git status --short` (including untracked) **and** `git diff --stat`;
  then `bash Docs/medir-controles.sh verificar` on every `Docs/controles/*.tsv`, reported, never
  silenced.
- Shell: never prefix commands with `cd`; use `git -C`, `dotnet --project`. On Windows never
  create repository files with PowerShell `>` (writes UTF-16) — use the editor tools or
  `[IO.File]::WriteAllText` with UTF-8 without BOM. Never rewrite a whole file with a tool that
  normalizes line endings.
- Tooling on SDK 10, measured in leva 01: `dotnet new sln --format sln` (the default is `.slnx`,
  which the hook and the controls do not know); `dotnet new`, `dotnet ef migrations add` and
  some builds write a UTF-8 BOM the pre-commit hook refuses — strip it before staging.
- **No push from the agent** unless the queue line says so: the remote and its credentials are the
  operator's, and a pushed commit cannot be rewritten (its hash may already be recorded).

## Commands

```
dotnet restore
dotnet build
dotnet test
dotnet run --project src/OrlandoUp.Web                # ports in launchSettings.json
dotnet ef migrations add <Name> --project src/OrlandoUp.Web
dotnet ef database update --project src/OrlandoUp.Web
dotnet run --project src/OrlandoUp.Web -- seed-catalog
dotnet run --project src/OrlandoUp.Web -- seed-admin  # reads AdminSeed:Email/Password from user-secrets
bash Docs/medir-controles.sh verificar Docs/controles/<front>.tsv
git config core.hooksPath .githooks                    # once per clone
```
