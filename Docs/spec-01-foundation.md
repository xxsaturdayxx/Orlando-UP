# Spec — Leva 01: application foundation (the site runs in two languages from the database)

**Date:** 2026-09-04, conversation 1. **Executor:** Claude Code, strongest model — this leva
creates the schema, the Identity setup and the localization plumbing that every later front
inherits; a shortcut here is paid for in every leva after it.

**What this leva closes:** the repository has documents and no application code
`[V, git ls-files at bfbe6f3 = 13 files, none under src/ or tests/]`.

**Language of this spec:** English (Docs/decisions.md D1). Section names follow the project's
spec skeleton; the control rules it relies on are in `Docs/regras-de-controle.md` (Portuguese,
copied from the operator's skill on 2026-09-04).

> **Review note — 2026-09-04, conversation 2 (Claude Web), after reading `scratchpad/leva01/plano.md`.**
> The body below is unchanged; where this note and the body disagree, **this note wins**.
>
> **Decisions taken by Rod on the plan's open points (K4, K5, C17):**
> - **K4 → (a).** `#F26B1D` stays as the action *surface* (ink `#1F2933` text on it, 4.85:1). When
>   orange is *text or icon* (links, highlighted price) the site uses the derived token
>   **`--color-action-text: #B84A0C`** (4.99:1 on off-white). Registered in `Docs/architecture.md` §12.
>   The name is `action-text`, not `action-ink` — `ink` is already the `#1F2933` token.
> - **K5 → (a).** §8 item (4) is amended: the generator meta emits `OrlandoUp <short hash>` plus the
>   suffix `+dirty` while the working tree has uncommitted changes; the conference records the value
>   seen and states that `+dirty` **is** the proof of a fresh build. The two-commit closing of
>   `Docs/fila-cc.md` stands.
> - **C17/C18 stay** in `Docs/controles/foundation.tsv` (D15). 18 controls.
>
> **Corrections the plan must absorb before the first file changes (numbered; the report answers each):**
> 1. The `IDesignTimeDbContextFactory` reads `ConnectionStrings:DefaultConnection` from
>    user-secrets (`AddUserSecrets(typeof(Program).Assembly, optional: true)`) and environment
>    variables, and only falls back to `UseSqlServer()` without a string when the key is absent —
>    otherwise `dotnet ef database update` fails in E6. No connection string in any file.
> 2. The startup fail-fast on a missing connection string must be bypassable by the test host: the
>    test `WebApplicationFactory` injects the key via `UseSetting` and replaces `DbContextOptions`
>    with SQLite in-memory — otherwise `dotnet test` (C15) and `ci.yml` on Ubuntu never pass.
> 3. C05 pattern becomes `DateTime(Offset)?[.](Now|Today)`; a sibling control asserts `UtcNow`
>    occurs in exactly one file of `src/` (the `IClock` implementation), file name in the label.
> 4. C11 becomes discriminating: `grep -rIlE 'using (Markdig|Ganss)' --include='*.cs' src | sort |
>    paste -sd,` expected `src/OrlandoUp.Web/Application/RichText.cs`.
> 5. Rule 3 of `Docs/regras-de-controle.md` applies inside `src/`: no code comment transcribes the
>    forms C05/C09/C17 search for (schema-creation calls, local clock reads, coalescing to zero).
> 6. Middleware order is written in `Program.cs` with a comment: `UseRequestLocalization` after
>    `UseRouting` (the culture provider reads a route value) and before authorization / endpoints.
> 7. The language switcher link to English from a `/pt/...` page overrides the ambient route value
>    explicitly (`asp-route-culture=""`).
> 8. P1 gains a human item: `dotnet dev-certs https --check`, else `dotnet dev-certs https --trust`.
> 9. Two gaps of this spec, decided now: public pages list only `IsActive = true` products; a
>    translation missing for the requested culture falls back to `en-US`, never 404.
>
> **Also noted:** §3 "24 decisions" reads 26 today by the same command (D25 and its note); the
> step-0 grep of the radical excludes `scratchpad/`; the P2 migration review is done by Claude Web
> in the conversation — the agent stops, commits the report and waits.
>
> **P2 review — 2026-09-04, conversation 2, migration `20260904233355_InitialCreate` (report
> `Docs/relatorio-leva-01-etapa-1.md`).** SQL script classified independently by Claude Web: 40
> statements, 0 destructive, 21 attention items all justified (12 cascades on child-only rows, the
> two history-bearing FKs `Units→Products` and `DeliveryLocations→DeliveryZones` are NO ACTION, 10
> unique indexes on tables born empty, the only INSERT is `__EFMigrationsHistory`). Every column of
> §4 matched against the SQL; the 7 enums carry explicit values; no `HasData` in `src/`. **Approved.**
> Accepted deviations (improvements, spec amended here): the options class is
> **`SiteLocalizationOptions`** (the framework already owns `LocalizationOptions`); `dotnet new sln
> --format sln` is required on SDK 10 (default is `.slnx`); tool-generated files carry a UTF-8 BOM the
> pre-commit hook refuses — strip it after every `migrations add` and `dotnet new`.


---

## 0. Execution surface

**Launcher phrase:** this spec is executed by the line of `Docs/fila-cc.md` dated `2026-09-04`
whose description starts with *"LEVA 01 — APPLICATION FOUNDATION"*. Not "the `aguardando`
line" — that line, by name.

**Tree state at receipt** (measured 2026-09-04 by Claude Web): HEAD is `bfbe6f3` **or a
descendant of it**; `git status --porcelain` is **empty** — everything Claude Web wrote is
committed. There is no remote yet unless Rod created it (`git remote -v` may be empty or show
`origin`; both are fine). On Windows, `warning: LF will be replaced by CRLF` messages and an index
that needs `git update-index --refresh` are possibilities to ignore, never divergence. Any tracked
file modified or any untracked file at receipt is a stop with a report.

**Files the front ALTERS:** the closed list is §9.1. A file outside it is a stop with a report,
**no cardinal**.

**Files the front PRODUCES as record** (authorized, not scope drift): `scratchpad/leva01/plano.md`
(the plan — not committed, `scratchpad/` is ignored, and execution starts only after Rod reviews
it); `Docs/relatorio-leva-01-etapa-N.md` for every mandatory stop (committed before asking
approval); `Docs/controles/foundation.tsv` (proposed in step 0, committed with the content commit);
`Docs/conferencia-leva-01.md` (the visual-check results, §8).

**Files the TOOL generates coupled:** `OrlandoUp.sln` (`dotnet new sln`), the two `.csproj`
files and `Properties/launchSettings.json` (from the templates, then edited), everything under
`src/OrlandoUp.Web/Infrastructure/Data/Migrations/` including `AppDbContextModelSnapshot.cs`
(`dotnet ef migrations add InitialCreate`), and `bin/`/`obj/` (ignored). Authorized by this
declaration.

**Steps that need a human hand, with the exact command:**

1. **Rod, before the leva starts:** .NET 10 SDK installed (`dotnet --list-sdks` shows a `10.0.x`
   line), `dotnet tool update --global dotnet-ef`, LocalDB present (`sqllocaldb info` lists
   `MSSQLLocalDB`). If any is missing, the agent stops at step 0 with the exact line it measured.
2. **The agent applies migrations to LocalDB only** — the development database
   `Server=(localdb)\MSSQLLocalDB;Database=OrlandoUpDb` is the only database that exists in this
   phase; no other connection string is configured anywhere. This is the one place where "apply
   migration" is not a human step.
3. **Rod sets the two user-secrets** the agent cannot invent (the agent prints the two commands
   with the key names only and waits): `ConnectionStrings:DefaultConnection` (value above) and
   `AdminSeed:Email` + `AdminSeed:Password` for the first admin. Never a placeholder value inside
   a command.
4. **Visual check:** the agent runs it and writes `Docs/conferencia-leva-01.md`; Rod confirms in
   his own browser at the end of the leva (§8).
5. **Push:** Rod, after the closing commit — the remote and its credentials are his (CLAUDE.md).

---

## 1. What the leva delivers, in plain words

Rod runs `dotnet run --project src/OrlandoUp.Web`, opens the site and sees Orlando Up: header
with the name, navigation, a language switch, a footer with the company data from configuration.
The home page has a hero, three cards for the three kinds of visitor (keeping up with the family,
a medical reason, seniors), and the seven products read from the database with "from US$ X/day".
Clicking "Português" shows the same page at `/pt` in Portuguese; every link on that page keeps
`/pt`. A product page shows name, tagline, description, highlights, capacity and dimensions,
and the "fits Disney buses" badge when the dimensions allow it. `/admin` asks for a login; after
logging in with the seeded admin Rod sees a dashboard with the counts of products, units and
locations, and a read-only list of products with their translations.

What this leva is **not**: no booking, no dates picker, no availability, no payment, no e-mail
sent, no API beyond `/healthz`, no PWA, no deploy, no content pages beyond the placeholders
listed in §6. Those are levas 02–05 (`Docs/roadmap.md`).

---

## 2. Decisions of this leva

Project-wide decisions are D1–D24 in `Docs/decisions.md`. These are local to the leva; all
**[assistant]** unless marked, decided 2026-09-04.

**D1/01 — `OrlandoUp.sln` at the repository root; `src/OrlandoUp.Web` and `tests/OrlandoUp.Tests`;
root namespace `OrlandoUp`.** The layering is by folder inside the web project (D11):
`Domain/`, `Application/`, `Infrastructure/`, `Pages/`, `Api/`, `Resources/`, `wwwroot/`.

**D2/01 — Packages, all pinned to an explicit version in the csproj:** the latest stable `10.0.x`
of `Microsoft.EntityFrameworkCore.SqlServer`, `.Design`, `.Tools`,
`Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Mvc.Testing`; the
latest stable of `NetArchTest.Rules`, `xunit`, `xunit.runner.visualstudio`,
`Microsoft.NET.Test.Sdk`, `Markdig`, `HtmlSanitizer`. The versions chosen are reported in the
plan. Markdig + HtmlSanitizer are called only from one class, `Application/RichText.cs` —
Markdig alone does not sanitize (same pair and same rule as Ronatrip).

**D3/01 — Public URL culture prefix by route.** Every public page gets an optional first segment
constrained to `pt`; no prefix is `en-US`, `pt` is `pt-BR`. Implementation is the agent's choice
(a page route model convention plus a route-data request culture provider is the expected shape);
the observable behaviour is fixed by the tests in §7: `/` renders `<html lang="en">`, `/pt`
renders `<html lang="pt-BR">`, `/pt/rentals` renders the catalog in Portuguese, links generated
inside a `/pt/...` page stay under `/pt/`, and `/es` is a 404.

**D4/01 — Admin is localized too, by cookie.** The delivery team is Brazilian. `/admin/*` reads
the UI culture from a cookie set by the same switcher; default `en-US`.

**D5/01 — Seed data by explicit commands, never `HasData`.** `dotnet run -- seed-catalog` inserts
the §5 content only when the `Products` table is empty; `dotnet run -- seed-admin` creates the
first `Admin` from user-secrets and refuses if any user already holds the role. Reason: `HasData`
turns admin-editable rows into `UpdateData` migrations that overwrite edits (Ronatrip's lesson,
`[H]`), and D23 forbids a config-only admin seed.

**D6/01 — Fixed local ports: HTTPS `7420`, HTTP `5420`** in `launchSettings.json`, so every
document and test can name them.

**D7/01 — Hand-written CSS with design tokens, no framework, no build step.** Tokens from
`Docs/architecture.md` §12 as custom properties in `wwwroot/css/site.css`; Nunito self-hosted
as `woff2` under `wwwroot/fonts/` (OFL licence file next to it); system sans for body text.

**D8/01 — Identity for staff only.** Roles `Admin` and `Staff`; no registration page; login at
`/admin/login`; lockout on; cookie 12 h sliding; password policy: 12+ characters, no other
composition rule (length beats symbols).

**D9/01 — `IClock` in `Application/`, implemented in `Infrastructure/`:** `UtcNow` and
`TodayInOrlando()`; the time zone is resolved by trying the IANA id `America/New_York` first and
the Windows id `Eastern Standard Time` second, so the same code runs on Rod's Windows machine and
on Linux App Service.

**D10/01 — `/healthz` returns 200 and `{ "status": "ok", "database": "ok" }` when
`AppDbContext.Database.CanConnectAsync()` is true, else 503.** App Service health probes use it
in phase 5.

**D11/01 — `robots.txt` says `Disallow: /` while `Seo:AllowIndexing` is `false` (the default);
phase 5 flips the setting.** The site must not be indexed with placeholder prices.

**D12/01 — Placeholder facts are labelled once, in the seeder's header comment, and tracked in
`Docs/open-questions.md` Q1/Q2/Q9;** the product specs and prices in §5 are typical values of
each class, never presented as Ronatrip's fleet. Company data comes from `Company` options whose
default values are the literal strings `TODO-legal-name`, `TODO-address`, `TODO-phone`,
`TODO-email`, so a page that shows them is visibly unfinished.

---

## 3. Measured terrain

All `[V]` on 2026-09-04, measured by Claude Web in the Linux shell mounted on the repository;
one row per file, with the command that reached **that** file.

| File / place | Fact | Command |
|---|---|---|
| repository | 13 tracked files, none under `src/` or `tests/` | `git ls-files \| wc -l` → 13; `[ -d src ]` → no |
| repository | HEAD `bfbe6f3`, working tree clean | `git rev-parse --short HEAD`; `git status --porcelain \| wc -l` → 0 |
| `Docs/decisions.md` | 24 numbered decisions | `grep -c '^\*\*D[0-9]' Docs/decisions.md` → 24 |
| `Docs/medir-controles.sh` | self-test passes, 41 cases, 0 failures | `bash Docs/medir-controles.sh autoteste` → exit 0 |
| `Docs/regras-de-controle.md` | 136 lines, md5 `bb68305db023e912c01a70432b52a612` | `wc -l`; `md5sum` |
| `.githooks/pre-commit` | passes on the staged foundation files (exit 0) | `sh .githooks/pre-commit` after `git add -A` |
| Linux shell | `dotnet` **absent** — nothing about the SDK is measured here | `which dotnet` → empty |
| Rod's Windows machine | SDK version, `dotnet-ef`, LocalDB: **unknown — step 0 measures them** | `dotnet --list-sdks`; `dotnet ef --version`; `sqllocaldb info` |
| `ronatrip-website` (sibling repo) | Razor Pages on `net8.0`, Identity, EF Core SqlServer 8.0.8, MailKit, QuestPDF, Azure Blob, App Service Windows via Web Deploy | `cat RonaTripNew.csproj` — reference only; **nothing is copied from it into this repository by the agent** |

`[H]` — none. Every fact this spec relies on was measured today.

---

## 4. Schema of this leva

Tables created by `InitialCreate`, plus the ASP.NET Core Identity tables with the default names.
All money `decimal(10,2)`; all text `nvarchar` with the lengths below; calendar dates `date`;
instants `datetime2` named `…Utc`. Booking tables are **not** in this leva (leva 03 adds them,
additively).

**Products**

| Column | Type | Null | Why |
|---|---|---|---|
| `Id` | int identity PK | no | |
| `Slug` | nvarchar(80), unique | no | URL segment, English, never changes after publication |
| `Category` | int (enum `ProductCategory`: `MobilityScooter`=1, `Wheelchair`=2, `Stroller`=3) | no | explicit numeric values so a reordered enum never rewrites data |
| `Configuration` | int (enum `SeatConfiguration`: `Single`=1, `Double`=2, `Triple`=3, `Infant`=4) | yes | only strollers have it |
| `MaxRiderWeightLb` | int | yes | strollers carry per-child limits in `Highlights` instead |
| `WidthIn`, `LengthIn` | decimal(5,1) | no | needed for the transport badge and park limits |
| `SeatWidthIn` | decimal(5,1) | yes | scooters and wheelchairs |
| `RangeMiles` | decimal(5,1) | yes | scooters only |
| `TurnaroundDays` | int, default 0 | no | buffer for availability (leva 03 reads it) |
| `IsActive` | bit, default 1 | no | soft hide, never delete a product with history |
| `SortOrder` | int | no | |
| `ImagePath` | nvarchar(260) | yes | `img/products/<slug>.webp`; null shows the category illustration |
| `CreatedAtUtc`, `UpdatedAtUtc` | datetime2 | no / yes | |

`FitsDisneyTransport` is **computed in the domain** (`WidthIn <= 30 && LengthIn <= 48`), not stored.

**ProductTranslations** — `Id`, `ProductId` FK (cascade), `Culture` nvarchar(10) (`en-US`, `pt-BR`),
`Name` nvarchar(120), `Tagline` nvarchar(200) null, `Description` nvarchar(max) (Markdown),
`Highlights` nvarchar(max) (JSON array of strings). Unique `(ProductId, Culture)`.

**Units** — `Id`, `ProductId` FK (restrict), `AssetTag` nvarchar(40) unique, `SerialNumber`
nvarchar(80) null, `Status` int (enum `UnitStatus`: `Available`=1, `Maintenance`=2, `Retired`=3),
`Notes` nvarchar(max) null, `PurchasedOn` date null, `CreatedAtUtc`.

**PricingTiers** — `Id`, `ProductId` FK (cascade), `MinDays` int, `MaxDays` int null, `Mode` int
(enum `TierMode`: `FlatPerRental`=1, `PerDay`=2), `Amount` decimal(10,2). Check constraints:
`MinDays >= 1`, `MaxDays IS NULL OR MaxDays >= MinDays`, `Amount > 0`. Domain rule (tested):
tiers of one product never overlap and cover every length from 1 to open-ended.

**AddOns** — `Id`, `Code` nvarchar(40) unique (`cup-holder`…), `PricingMode` int (enum
`AddOnPricingMode`: `PerRental`=1, `PerDay`=2), `Amount` decimal(10,2), `IsActive`, `SortOrder`;
**AddOnTranslations** — `(AddOnId, Culture)` unique, `Name` nvarchar(120), `Description`
nvarchar(400) null. **ProductAddOns** — `(ProductId, AddOnId)` composite PK.

**DeliveryZones** — `Id`, `Code` nvarchar(40) unique, `Kind` int (enum `ZoneKind`:
`DisneyResort`=1, `UniversalResort`=2, `HotelOrResort`=3, `VacationHome`=4, `Other`=9),
`DeliveryFee` decimal(10,2), `HandoverMode` int (enum `HandoverMode`: `MeetAndGreet`=1,
`FrontDesk`=2, `Doorstep`=3), `SalesTaxRate` decimal(6,4) default 0, `IsActive`, `SortOrder`;
**DeliveryZoneTranslations** — `(ZoneId, Culture)` unique, `Name` nvarchar(120), `Instructions`
nvarchar(max) null (Markdown shown to the customer).

**DeliveryLocations** — `Id`, `ZoneId` FK (restrict), `Name` nvarchar(160), `Address`
nvarchar(300) null, `Notes` nvarchar(400) null, `IsActive`, `SortOrder`. Unique `(ZoneId, Name)`.

---

## 5. Seed content (`seed-catalog`)

Seven products, English and Portuguese translations, placeholder tiers, six add-ons, four zones,
ten locations. **Every number below is a typical value for the class, labelled as placeholder by
D12/01; Rod's fleet replaces them (open questions Q1, Q2).**

| Slug | Category / config | Max rider (lb) | W × L (in) | Seat (in) | Range (mi) | Tiers (US$) |
|---|---|---|---|---|---|---|
| `standard-scooter` | MobilityScooter | 300 | 21 × 41 | 17 | 12 | 1–2 d flat 75; 3–6 d 32/d; 7+ d 27/d |
| `heavy-duty-scooter` | MobilityScooter | 400 | 24 × 47 | 20 | 15 | 1–2 d flat 95; 3–6 d 38/d; 7+ d 33/d |
| `standard-wheelchair` | Wheelchair | 300 | 25 × 42 | 18 | — | 1–2 d flat 40; 3+ d 12/d |
| `single-stroller` | Stroller / Single | — | 24 × 40 | — | — | 1–2 d flat 35; 3+ d 10/d |
| `double-stroller` | Stroller / Double | — | 30 × 48 | — | — | 1–2 d flat 45; 3+ d 13/d |
| `triple-stroller` | Stroller / Triple | — | 31 × 52 | — | — | 1–2 d flat 60; 3+ d 18/d |
| `infant-stroller` | Stroller / Infant | — | 23 × 40 | — | — | 1–2 d flat 35; 3+ d 10/d |

Translations: `Name`, `Tagline`, a 3–5 sentence `Description` in Markdown and 4 `Highlights`
each, written by the agent in natural English and natural Brazilian Portuguese (not machine
literal), mentioning for scooters: charge overnight in the room, remove the key when parked, the
Disney bus/Skyliner fit (30 × 48), delivery tested and charged; for strollers: the Disney
31 × 52 limit and that wagons are not allowed in the parks (`Docs/market-notes.md`). One
`Unit` per product with `AssetTag` `<SLUG>-001`.

Add-ons (`Code` — mode — US$): `cup-holder` — PerRental — 5; `cane-holder` — PerRental — 5;
`sunshade` — PerDay — 3; `rear-basket` — PerRental — 8; `rain-cover` — PerRental — 5 (strollers
only); `damage-waiver` — PerRental — 20 (scooters and wheelchairs). Names and one-line
descriptions in both cultures.

Zones (`Code` — kind — fee — hand-over): `disney-resorts` — DisneyResort — 0 — MeetAndGreet
(instructions: Disney allows only its featured provider to leave equipment with Bell Services,
so we meet you in person at the resort at the agreed time); `universal-resorts` —
UniversalResort — 0 — MeetAndGreet; `idrive-lbv-hotels` — HotelOrResort — 0 — FrontDesk;
`vacation-homes` — VacationHome — 25 — Doorstep. `SalesTaxRate` 0 on all (open question Q4).

Locations (zone — name): disney-resorts — Disney's Pop Century Resort; Disney's Art of Animation
Resort; Disney's All-Star Movies Resort; Disney's Caribbean Beach Resort; Disney's Contemporary
Resort; Disney's Grand Floridian Resort & Spa. universal-resorts — Universal's Cabana Bay Beach
Resort; Universal's Endless Summer Resort – Surfside Inn. idrive-lbv-hotels — Hilton Orlando
Buena Vista Palace; Rosen Inn International Drive.

---

## 6. Pages, behaviour, configuration

**Program.cs** wires: EF Core SqlServer from `ConnectionStrings:DefaultConnection` (startup fails
with a message naming the user-secrets command when absent — never a default connection string);
Identity with roles; request localization per D20/D21/D3/01/D4/01; `IClock`; `RichText`; options
classes `CompanyOptions`, `SeoOptions`, `LocalizationOptions`; the `seed-catalog` / `seed-admin`
command switches (run and exit 0/1 without starting Kestrel); HSTS + HTTPS redirect outside
Development; static files with cache headers; `/healthz`; `robots.txt` per D11/01; status code
pages `/error/404` and `/error/500` localized.

**Public pages** (all with a `/pt` twin, all reading strings from `Resources/`):

| Route | Content |
|---|---|
| `/` | hero (headline, sub-headline, primary CTA to `/rentals`), three audience cards, product cards from DB (image or category illustration, name, capacity badge, "fits Disney buses" pill when applicable, "from US$ X/day" = lowest per-day amount among the product's tiers, or the flat tier amount divided by its `MaxDays` when there is no per-day tier), "how it works" strip (choose dates → we deliver → enjoy → we pick up), footer with `Company` data |
| `/rentals` | products grouped by category, same cards, category anchors |
| `/rentals/{slug}` | name, tagline, image, specs table (capacity, dimensions, seat, range), highlights, description (Markdown via `RichText`), badge, price tiers table, add-ons available, CTA "Booking opens soon" disabled button in this leva (leva 03 replaces it) |
| `/how-it-works` | four steps + the meet-and-greet explanation from the Disney zone instructions |
| `/faq` | 8 questions from resources (age 18 to drive a scooter, charging, park transport, rain, cancellation "see terms", strollers and wagons, what to bring, contact) |
| `/contact` | company phone, WhatsApp click-to-chat link, e-mail, hours — from `Company` options; no form in this leva |
| `/privacy`, `/terms` | placeholder pages with a visible "draft — not yet reviewed" banner in both languages |
| `/error/404`, `/error/500` | localized |

**Layout:** skip link, header (wordmark "Orlando Up", nav: Rentals, How it works, FAQ, Contact),
language switcher (EN / PT) that keeps the current page, footer (company data, links to privacy
and terms, "© {year} {Company.TradeName}"), `<html lang>`, `hreflang` alternates + `x-default`,
canonical URL, Open Graph basics, `<meta name="robots" content="noindex">` while `Seo:AllowIndexing`
is false. Mobile-first, 18 px body on ≥ 768 px, focus visible, 4.5:1 contrast on every token pair
used (the agent lists the pairs and their ratios in the plan).

**Admin** (`/admin/*`, `[Authorize(Roles = "Admin,Staff")]` by folder convention; `/admin/login`
anonymous): `login`, `logout` (POST), `index` dashboard with counts (products, units, locations)
and a banner "Catalog contains placeholder data — Docs/open-questions.md Q1/Q2" while the
`standard-scooter` product still carries the seed description hash (simplest: a `Settings` row
is out of scope; compare the `Description` of `standard-scooter` with the seeder's constant),
`products/index` read-only table (slug, category, EN name, PT name, active, units count).

**Resources:** `Resources/SharedResource.resx` (English, the key **is** the English text or a
stable key — the agent chooses and applies one convention) and `Resources/SharedResource.pt-BR.resx`.
The parity test in §7 fails on any key missing on either side.

**Configuration shape** in `appsettings.json` (values non-secret): `Company` (`LegalName`,
`TradeName` = `Orlando Up`, `Address`, `Phone`, `WhatsApp`, `Email`, `Hours`, all `TODO-…` except
the trade name), `Seo:AllowIndexing` false, `Seo:CanonicalHost` `orlandoup.com`, `Localization`
(`DefaultCulture` `en-US`, `SupportedUICultures` [`en-US`, `pt-BR`]), `Logging`. No
`ConnectionStrings` section at all.

---

## 7. Tests (`tests/OrlandoUp.Tests`)

Tests that prove the invariants, not tests that exercise code:

1. **Architecture** (NetArchTest): types in `OrlandoUp.Domain` depend on no other project
   namespace; types in `OrlandoUp.Application` do not depend on `OrlandoUp.Infrastructure` or
   `OrlandoUp.Pages`; no type outside `OrlandoUp.Application.RichText` references `Markdig` or
   `Ganss.Xss`.
2. **Localization parity:** the set of keys in `SharedResource.resx` equals the set in
   `SharedResource.pt-BR.resx`; no value is empty; the test reads the files from disk.
3. **Culture routing** (WebApplicationFactory, SQLite in-memory or the SqlServer provider with a
   test database — the agent chooses and states it): `GET /` → 200, body contains
   `<html lang="en"`; `GET /pt` → 200, `lang="pt-BR"`; `GET /pt/rentals` → 200; `GET /es` → 404;
   every `href` under `/pt/rentals` that points to a public page starts with `/pt/` or `/pt`.
4. **Domain:** `Product.FitsDisneyTransport` is true at 30 × 48 and false at 30.1 × 48;
   `PricingTier` validation rejects overlapping tiers and gaps; `FromPricePerDay` picks the lowest
   per-day amount.
5. **Clock:** `TodayInOrlando()` on a fixed UTC instant `2026-03-08T06:30:00Z` returns
   `2026-03-08` (01:30 EST, before the DST switch at 02:00) and on `2026-03-08T07:30:00Z` returns
   `2026-03-08` (03:30 EDT) — the test proves the zone resolves on the current OS.
6. **Seed guard:** `seed-catalog` on a non-empty `Products` table inserts nothing and returns 0;
   `seed-admin` with an existing Admin returns 1 and creates nothing.
7. **Health:** `/healthz` returns 200 with the JSON of D10/01.

Effects to neutralize before the first scenario: there are none in this leva (no e-mail, no
Stripe), and the test proves it by asserting that no `IEmailSender` implementation is registered.

---

## 8. Visual check

**Before any item, in one block:** (1) `dotnet build` clean, then `dotnet run --project
src/OrlandoUp.Web` on the ports of D6/01; (2) database: LocalDB `OrlandoUpDb` after
`dotnet ef database update` and `seed-catalog`; (3) user: the seeded admin for the `/admin` items,
anonymous for the rest; (4) **step 1 is the proof of a fresh build** — the footer year and the
`<meta name="generator" content="OrlandoUp <git short hash>">` the layout emits from
`ThisAssembly`/informational version must show the current commit; (5) proof of a change is a
screenshot or a re-navigation, never a text extractor in the same round.

| # | Do | Expect |
|---|---|---|
| 1 | open `https://localhost:7420/` | generator meta = current short hash; hero, three cards, seven product cards with "from US$" |
| 2 | click PT | `/pt`, same layout in Portuguese, switcher shows EN |
| 3 | from `/pt` click a product | `/pt/rentals/<slug>` in Portuguese, badge present on `standard-scooter`, absent on `triple-stroller` |
| 4 | keyboard only: Tab from the address bar | skip link appears first; every nav item and card gets a visible focus ring |
| 5 | width 375 px | no horizontal scroll, menu usable |
| 6 | `/admin` anonymous | redirect to `/admin/login` |
| 7 | log in | dashboard counts 7 / 7 / 10, placeholder banner visible |
| 8 | `/healthz` | 200 JSON; stop LocalDB (`sqllocaldb stop MSSQLLocalDB`) → 503; start it again |
| 9 | `/robots.txt` | `Disallow: /` |
| 10 | `/es` | localized 404 |

**The result is written to `Docs/conferencia-leva-01.md`,** one line per item with what was seen;
items the agent cannot reach (a real phone) are listed with the reason, never omitted.

---

## 9. Controls

### 9.1 Files the front alters

**New:** `OrlandoUp.sln`; everything under `src/OrlandoUp.Web/` and `tests/OrlandoUp.Tests/`;
`.github/workflows/ci.yml` (restore, build, test on push and pull request, .NET 10, Ubuntu);
`Docs/controles/foundation.tsv`; `Docs/conferencia-leva-01.md`; `Docs/relatorio-leva-01-etapa-N.md`
when a stop happens.

**Modified, and only in this:** `README.md` (the "Running locally" section: confirm ports and
commands after they exist); `Docs/fila-cc.md` (the State and Commit columns of this leva's line,
and the closing line); `.gitignore` only if the SDK produced a file the current list misses
(reported in the plan).

**Negatives, by column of the diff:** `CLAUDE.md`, `Docs/decisions.md`, `Docs/architecture.md`,
`Docs/roadmap.md`, `Docs/open-questions.md`, `Docs/market-notes.md`, `Docs/backlog-conhecido.md`,
`Docs/protocolo-conversa.md`, `Docs/regras-de-controle.md`, `Docs/medir-controles.sh`,
`Docs/resumo-conversa-1.md`, `Docs/atrito-conversa-1.md`, this spec, `.githooks/pre-commit`,
`.gitattributes` — untouched. A contradiction found in one of them is
reported in the plan, not fixed by the agent.

### 9.2 Invariants for `Docs/controles/foundation.tsv`

The exact command form is the agent's (it has read the code); the invariants are these, and every
rule in `Docs/regras-de-controle.md` applies:

1. `presenca` of `OrlandoUp.sln` at the root, of `src/OrlandoUp.Web/OrlandoUp.Web.csproj`, of
   `tests/OrlandoUp.Tests/OrlandoUp.Tests.csproj`, of `Resources/SharedResource.pt-BR.resx`.
2. `conta-re` of `DateTime\.(Now|Today)` over `src/` = 0, with the reach assertion that
   `IClock` occurs ≥ 1 in `src/` (rule 5: a subtraction or a zero needs its sibling proving the
   scan reaches the files).
3. `conta` of `ConnectionStrings` in `src/OrlandoUp.Web/appsettings.json` = 0, with the reach
   assertion that `Company` occurs ≥ 1 in the same file.
4. `conta-re` of `Migrate\(|MigrateAsync\(|EnsureCreated\(` over `src/` = 0 (D12), reach: `UseSqlServer` ≥ 1.
5. `conta-re` of `using Markdig|using Ganss` over `src/` = 1 file (only `RichText.cs`) —
   expressed as a `cmd` that counts files, with the file name in the label.
6. `cmd`: the two resx key sets are equal → prints `equal` (the parity test proves it at test
   time; the control proves it at the gate without building).
7. `cmd`: `dotnet build OrlandoUp.sln --nologo -v q` exit code → `0`, and `dotnet test --nologo
   -v q` exit code → `0`.
8. `conta` of `TODO-` in `src/OrlandoUp.Web/appsettings.json` ≥ 4 while Q9 is open (this control
   **expires** when Rod answers Q9; the label says so).

### 9.3 What STEP 0 measures, before altering any file

1. `dotnet --list-sdks` contains a `10.0.` line; `dotnet ef --version` prints a `10.` version;
   `sqllocaldb info` lists `MSSQLLocalDB`. Any miss is a stop with the line measured.
2. The `grep` of the radical `OrlandoUp` over the tree outside `Docs/`, `README.md` and
   `CLAUDE.md` — expected **0** (nothing under `src/` exists).
3. The value at HEAD of every existing control the new files displace — there is **no**
   `Docs/controles/*.tsv` yet (`ls Docs/controles` → empty), so nothing is displaced; state that.
4. The proposal of `Docs/controles/foundation.tsv` with `bash Docs/medir-controles.sh medir`
   run at the initial HEAD (most values will read 0/`nao` — that is the point: they must move).
5. The shape of the reflection-based tests, anchored on **names** (`SharedResource`,
   `IClock`, `RichText`), never on return types.
6. Any contradiction between this spec and the tree is reported in the plan; the spec is not
   obeyed against the measurement.

**Closing is two commits:** the content commit (code, tests, controls, conference), then the
closing commit that writes that commit's hash into the `Commit` column of the leva's line in
`Docs/fila-cc.md`. No push from the agent.

---

## 10. Out of scope, and why

- **Booking, availability, checkout, Stripe** — leva 03 (`Docs/roadmap.md`); they need the
  answers to Q2–Q6 and a staging environment (D14).
- **Public forms (contact, quote)** — every public form needs the e-mail outbox and the recipient
  allow-list (architecture §6), which arrive with leva 03; a form without them sends real mail.
- **Admin CRUD for the catalog** — leva 04; this leva ships read-only so that the schema can
  still change cheaply after Rod sees it.
- **Images of the real fleet** — Q11; the agent ships one simple SVG illustration per category
  under `wwwroot/img/`, drawn by itself, no third-party artwork.
- **Deploy workflows, App Service, DNS** — leva 05 (`gate-de-deploy-azure` skill).
- **PWA manifest and service worker** — leva 06.
- **Spanish** — backlog (`Docs/backlog-conhecido.md`).
