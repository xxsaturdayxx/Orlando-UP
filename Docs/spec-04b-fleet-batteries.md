# Spec — Leva 04b: batteries and chargers in the fleet (the pieces that limit the day)

**Date:** 2026-09-09, conversation 5. **Pair:** conversation 5 ↔ leva 04b. **Executor:** Claude Code,
strongest model — the leva adds an entity the booking flow of leva 03 will read, and gets one
migration.

**Language of this spec:** English (`Docs/decisions.md` D1). The control rules it relies on are in
`Docs/regras-de-controle.md`; the skeleton follows `Docs/spec-04-admin-catalog.md`.

**What this leva closes:** the fleet the system knows about is **ten scooters, wheelchairs and
strollers**, and nothing else. Measured: `Domain/` holds `Product`, `Unit`, `PricingTier`, `AddOn`,
`ProductAddOn`, `DeliveryZone`, `DeliveryLocation`, `AuditEntry` and their translations, and **no
battery and no charger**. Meanwhile the operation owns **twelve batteries and fourteen chargers**,
and D36 computes what that means: with four scooters and six batteries per model, **the battery runs
out before the scooter does**. A booking flow written on top of today's model would sell a third
second-battery that does not exist.

**Why it runs before leva 03.** Leva 03 has to count batteries from its first line. Adding them
during it means a schema change in the middle of a booking front; adding them now means leva 03
inherits a fleet that already knows how to count. This front depends on **no open question**: D36
and D37 answered Q14 in full, and the one fact still missing (charger stock) arrived on 2026-09-09 —
**fourteen**.

**Numbering.** `Docs/decisions.md` D33 fixes leva N ↔ roadmap phase N. This is the second front of
phase 4, so it is **04b**; leva 03 keeps its number and remains the next booking front.

---

> **AMENDMENT EMENDA-04B-01 — 2026-09-09, K1 answered before the plan asked for it (Claude Web).**
> Where this note and the body disagree, **the note wins**.
>
> **A1 — K1 is answered, and the answer is a format rather than a list of what is printed today.**
> `Docs/decisions.md` D38 fixes the tag shape for the whole fleet: three letters, a dash, two digits.
> **The twelve battery tags this leva seeds are `BSC-01` through `BSC-06` and `BSP-01` through
> `BSP-06`**, and **`BSC-06` is the Extended Range one** — the survivor of the two Drive Scout XL
> (D37). All six `BSP` are Normal, because D37 confirms no Drive Spitfire XL is known to exist and
> Q15 asks whether one ever did. Rod prints and sticks these labels; the seed and the objects meet at
> the same string, which is the entire reason §10 preferred the real tags over generated ones.
>
> **A2 — the scooter and wheelchair tags are NOT changed by this leva.** They read
> `DRIVE-SCOUT-4-001` and friends, generated from the slug by `CatalogSeeder.cs:146`. D38 gives them
> `SCT-`, `SPT-` and `WCH-` tags, and Rod applies those **by hand through the screen leva 04 built** —
> ten rows, no migration, no code. `Domain/Unit.cs`, `UnitConfiguration.cs` and the `Units/` pages
> stay on this leva's negative list exactly as §11.1 already declares.
>
> **A3 — nothing about QR enters this leva.** D38 settles the symbology and the payload, and puts the
> scanner after leva 03 with its own spec — it brings the first JavaScript into a tree that has none.
> This front stores a string; nothing renders a code and nothing reads a camera.
>
> **Proof of reading, required in the next artifact the agent produces:** a search for the string
> `EMENDA-04B-01` in the revised `scratchpad/leva04b/plano.md`, expected `>= 1`, with the count
> reported.

---

## 0. Execution surface

**Launcher phrase:** this spec is executed by the line of `Docs/fila-cc.md` dated `2026-09-09` whose
description starts with *"LEVA 04B — FLEET BATTERIES"*. **Not "the `aguardando` line"** — that line,
by name.

**Tree state at receipt, measured 2026-09-09:** HEAD is a descendant of `75ec0b0`; the commit that
adds this spec, its conference roteiro and the queue line comes after it and is the expected HEAD.
`git status --porcelain` **empty**. `git ls-files` counts **187**, **127** under `src/` and **16**
under `tests/`. `origin/main...main` reads `0 2` — two commits of conversation 5 not yet pushed, and
the push is the operator's. Three control files, **47 controls**: `admin-catalog.tsv` 12 on target,
`public-site.tsv` 17 on target, `foundation.tsv` 18 with **2 off** — C14 and C15, which shell out to
`dotnet` and answer `127` to the reviewer for want of it. The agent measures those two at step 0; a
control genuinely red on arrival is a stop.

A Windows CRLF warning and a stale git index are not divergence. Any other modified or untracked
file outside `scratchpad/` is a stop with a report.

**Files the front ALTERS:** the closed list is §11.1. A file altered outside it is a stop with a
report, **without a cardinal**.

**Files the front PRODUCES as record:** `scratchpad/leva04b/plano.md` (never committed);
`Docs/relatorio-leva-04b-etapa-N.md`, one per stop, committed before approval is asked;
`Docs/controles/fleet-batteries.tsv`; and the results written into
**`Docs/conferencia-leva-04b.md`, which already exists in the tree as an empty-result roteiro** — see
§9.

**Files the TOOL generates coupled:** `dotnet ef migrations add` rewrites
`AppDbContextModelSnapshot.cs` and writes the migration's `.Designer.cs`.

**Steps that need a human hand.** The operator's shell is **PowerShell on Windows**.

1. **Apply the migration, after P1 is approved:**

   ```
   dotnet ef database update --project src/OrlandoUp.Web
   ```

2. **Strip the BOM before `git add`** — `CLAUDE.md:82-83`; the hook refuses `efbbbf`.
3. **Seed the twelve batteries and the charger count** — §4.4, and it is the operator who runs it,
   because it writes real fleet data.
4. **The visual check of §9 is the operator's**, filling the roteiro that already exists.

**Three mandatory stops.** In each one the report is committed **before** approval is asked. There
is no test-harness stop this time: the harness of leva 04 exists and is measured by controls C11 and
C12 of `admin-catalog.tsv`.

| Stop | When | What it carries |
|---|---|---|
| **P0** | after step 0, before altering any file | the plan, with the answer to the single open point of §10 |
| **P1** | migration written and **not applied** | the script classified by `revisao-migration-efcore`, with the row counts of every table it touches measured before, and `sys.default_constraints` read after the leva 04 lesson (D35) |
| **P2** | everything written, suite green | the full run, all four `.tsv` verified, the visual check pending |

---

## 1. What the leva delivers, in plain words

Rod opens the fleet and sees, beside the scooters, the **batteries**: twelve of them, each with the
tag stuck on it, each belonging to a scooter model, each marked Normal or XL, each available or in
maintenance or retired. He can add one when he buys one and retire one when a customer breaks one —
which has already happened once.

He also sees **how many chargers exist**: fourteen, a number and not a list, because any charger
fits any battery of either model and losing one is a matter of counting, not of naming.

And he sees two amounts he can change without asking anyone: **what a second battery costs per day**
by default, and **what a lost charger costs**.

**What this is not.** It is not booking: nothing here reserves a battery, because there are no
reservations yet. It is not a public page: the site says nothing new, and in particular **says
nothing about the XL** (D37). It is not the company-settings move: `appsettings.json` is not touched
and control C16 stays alive — see §12.

---

## 2. Decisions of this leva

`[operator]` marks Rod's; `[assistant]` marks the reviewer's, taken under the autonomy clause of
`Docs/protocolo-conversa.md` and open to one line of disagreement.

**D1/04b — A battery is a NEW entity that mirrors `Unit`, with a foreign key to the scooter
`Product`.** `[assistant]` The product row *is* the model (`drive-scout-4`), and D37 states a battery
belongs to the model rather than to a machine — so the same shape `Unit` already uses expresses it
exactly. **The two alternatives are excluded by measurement, not by taste.** Reusing `Unit` would
overload a type that means *a rentable machine*: it would move the dashboard counts of
`Admin/Index.cshtml.cs:25-27`, the nine hard cardinals of `SeedingTests.cs:40-52`, and the assertion
`Only_a_product_on_sale_carries_units` at `DomainTests.cs:201`. Making a battery a `Product` of its
own would put it in `CatalogQueries.ActiveCardsAsync`, which is the public catalog — exactly what
D37 forbids for the XL.

**D2/04b — `BatteryKind` is an attribute of the battery, never an allocation dimension.**
`[operator]`, D37. An XL satisfies every promise a Normal satisfies and costs the customer nothing
more, so **availability treats all batteries of a model as one pool**. Nothing in this leva, and
nothing in leva 03, may filter or reserve by kind. The kind exists so the operation knows what it
owns and so a future decision has something to stand on.

**D3/04b — The charger is a COUNT, not a table.** `[operator]`, D37: any charger fits any battery of
either model, and they carry no tag. A piece with no identity of its own gets a number. Fourteen
today, twelve batteries — **the two counts are independent and neither is derived from the other**,
which is why the number is stored rather than computed.

**D4/04b — The two editable amounts live in a small operational settings row, NOT in the company
settings.** `[assistant]` Measured: `CompanyOptions` is read through `IOptions<>` in five places of
`src/` — `Program.cs:21`, `_Layout.cshtml:1`, `_AdminLayout.cshtml:1`, `Contact.cshtml:2` and
`StructuredData.cs:66` — and control C16 of `foundation.tsv` counts the four `TODO-` markers **inside
`appsettings.json`**. Moving company data to the database would retire that control and rewire those
five call sites, for the sake of two numbers. **That is a front of its own, gated on Q12, and it is
not this one.** `appsettings.json` is on this leva's negative list.

**D5/04b — A battery has no `IsActive`.** `[assistant]` `UnitStatus` already carries
`Available | Maintenance | Retired`, and a retired battery is exactly the broken one Rod described.
Adding a second flag that means almost the same thing is how two sources of truth are born. The
battery reuses `UnitStatus`.

**D6/04b — No boolean column of this leva carries a store default, and the migration is read against
`sys.default_constraints` after it is applied.** `[operator]`, D34 and D35. The leva 04 lesson is
that a default created by `AddColumn(defaultValue:)` survives in the database invisibly to the model;
this leva creates a table, so it should create none — and the P1 report proves it rather than
assuming it.

---

## 3. The measured terrain

Everything `[V]` on 2026-09-09, read from the tree at `75ec0b0`. One row per file; the command that
reached **that** file is in the third column. `dotnet` and `OrlandoUpDb` are not reachable from the
reviewer's shell, so nothing below was compiled, executed or queried.

| File | Fact | How it was measured |
|---|---|---|
| `src/OrlandoUp.Web/Domain/Unit.cs` | the shape a battery copies: `Id`, `ProductId`, `Product?`, `AssetTag`, `SerialNumber?`, `Status` (initialised `Available`), `Notes?`, `PurchasedOn` (`DateOnly?`, a calendar date), `CreatedAtUtc`. **No `UpdatedAtUtc`.** | `cat -n` |
| `src/OrlandoUp.Web/Infrastructure/Data/Configurations/UnitConfiguration.cs` | `AssetTag` max 40 required, **unique index**; `SerialNumber` max 80; `PurchasedOn` `HasColumnType("date")`; **no `HasDefaultValue` anywhere in the file** | `cat -n` |
| `src/OrlandoUp.Web/Domain/Enums.cs` | `UnitStatus` = `Available 1`, `Maintenance 2`, `Retired 3`, with the comment that the numbers are persisted and must not be reordered. `AuditAction` was appended at the end by leva 04 | `cat -n` |
| `src/OrlandoUp.Web/Infrastructure/Seeding/CatalogSeedData.cs` | `drive-scout-4` and `drive-spitfire-ex` each carry `UnitCount = 4` and `RangeMiles = 9` — **the 9 is the Normal battery**, which is what the package promises | `grep -n` on the two slugs |
| `src/OrlandoUp.Web/Application/CompanyOptions.cs` + `Program.cs:21` | company data is bound from configuration as a singleton `IOptions<>`, not read from the database | `grep -rn "CompanyOptions\|IOptions<Company"` over `src` and `tests` |
| `Docs/controles/foundation.tsv:19` | C16 is `n=$(grep -cF 'TODO-' src/OrlandoUp.Web/appsettings.json); test ${n:-0} -ge 4` — it counts markers **in a committed file**, which is why D4/04b keeps them there | `grep -n C16` |
| `src/OrlandoUp.Web/appsettings.json:13-16` | the four markers are `Phone`, `WhatsApp`, `Email`, `Hours` — exactly four, zero margin over the threshold | `grep -n "TODO-"` |
| `src/OrlandoUp.Web/Pages/Admin/Units/` | three page pairs — `Index`, `Create`, `Edit` — are the pattern the battery screens copy | `git ls-files src/OrlandoUp.Web/Pages/Admin` |
| `src/OrlandoUp.Web/Infrastructure/Data/CatalogWriter.cs` | the scoped writer of leva 04, registered at `Program.cs`, beside `CatalogQueries`; `AuditTrail` sits next to it | read in leva 04's review |
| `Docs/controles/admin-catalog.tsv` | C05 is the relation *POST handlers under `Pages/Admin` minus audit calls = 2*, and C06 asserts the handler operand is `>= 6`. **Both move in this leva** — see §11.2 | `cat` |
| `src/OrlandoUp.Web/Infrastructure/Data/Migrations/` | the last migration is `20260908215113_RemoveActiveFlagStoreDefaultsAndAddAuditEntries`; names are verb-plus-object in PascalCase with no leva number | `ls -1` |

**Inherited and not re-checked `[H]`, therefore pending:** the suite counts 170 tests and the build
is clean — read from the agent's leva 04 report, because C14 and C15 need `dotnet`.

---

## 4. The schema change

One migration. Suggested name: `AddBatteriesAndOperationalSettings`.

### 4.1 `Batteries`

A new entity `Domain/Battery.cs`, a plain POCO with the navigation to `Product` that `Unit` has.

| Column | Type | Null | Why |
|---|---|---|---|
| `Id` | `int` identity | no | — |
| `ProductId` | `int` FK → `Products` | no | **the scooter model**, not a unit. `OnDelete(Restrict)`, like units: a model with batteries is not deleted |
| `AssetTag` | `nvarchar(40)` | no | the tag stuck on the battery. **Unique across the whole fleet**, one index, exactly as `Unit` does — a tag is a tag, and two pieces sharing one is a bug whichever kind they are |
| `Kind` | `int` (`BatteryKind`) | no | `Normal = 1`, `ExtendedRange = 2`. Explicit numbers, appended at the end of `Domain/Enums.cs`, like every other enum there |
| `RangeMiles` | `decimal(5,1)` | **yes** | what this battery actually delivers — 9 and 14 today. Nullable because a battery nobody measured must say nothing rather than zero (D15) |
| `Status` | `int` (`UnitStatus`) | no | reuses the existing enum (D5/04b) |
| `SerialNumber` | `nvarchar(80)` | yes | as `Unit` |
| `Notes` | `nvarchar(400)` | yes | where *"a customer broke this one"* is written |
| `PurchasedOn` | `date` | yes | a **calendar date** in Orlando, never an instant |
| `CreatedAtUtc` | `datetime2` | no | an **instant**, from `IClock` |

**No `HasDefaultValue` on any column** (D6/04b).

### 4.2 `OperationalSettings`

A single-row table, `Domain/OperationalSettings.cs`, holding what the administrator edits without a
deploy:

| Column | Type | Null | Why |
|---|---|---|---|
| `Id` | `int` | no | primary key, and the row is **always id 1** — the table is a settings sheet, not a list |
| `ChargerCount` | `int` | no | how many chargers exist. **14** today |
| `SecondBatteryPerDay` | `decimal(10,2)` | no | the default amount a second battery is sold for. Leva 03 prefills a reservation with it and lets the operator change or zero it (D37) |
| `LostChargerFee` | `decimal(10,2)` | no | **US$ 30 per charger**, the price of a replacement |
| `UpdatedAtUtc` | `datetime2` | yes | an instant, from `IClock`, so an edit is dated |

**The single row is created by the migration itself**, with the three values above, because a
settings table with no row is a null reference waiting on the first screen that reads it. The
screen edits; it never creates and never deletes.

### 4.3 What the `Down` does

Drops both tables. Honest: it removes what the `Up` created and nothing else. The batteries are
fleet data, so the operator is told, in the P1 report, that going down loses the twelve rows — which
is why they are seeded by a command he runs rather than by the migration.

### 4.4 Seeding the twelve

A command in the shape of the existing `seed-catalog`, refusing to run when `Batteries` is not
empty — the same guard as `CatalogSeeder.cs:31-37`, and for the same reason: once the administrator
touches a row, the seed file is history, not truth.

**Six Drive Scout: five Normal at 9 miles and one Extended Range at 14** — Rod owned two XL and a
customer broke one (D37). **Six Drive Spitfire: all Normal at 9 miles**, because D37 confirms only
the Drive Scout XL exists and Q15 asks whether a Spitfire XL was ever owned. The asset tags are the
open point of §10.

---

## 5. The screens

| Route | What it does |
|---|---|
| `/admin/batteries` | the list: tag, model, kind, range, status, purchase date. Ordered by model then tag. Shows retired ones, marked |
| `/admin/batteries/create` | one battery |
| `/admin/batteries/edit/{id:int}` | one battery |
| `/admin/settings` | the single row: charger count, default second-battery amount, lost-charger fee |

The navigation bar of `_AdminLayout.cshtml` gains two destinations, inside the layout and **never in
a partial under `Pages/Shared/`** — control C10 of `public-site.tsv` excludes `Pages/Admin/` and the
`_AdminLayout` lines by name, but does **not** exclude `Pages/Shared/`.

The dashboard at `/admin` gains two counts beside the three it has: **batteries** and **chargers**.

**Every write handler records exactly one audit line**, through the `AuditTrail` service of leva 04.
`EntityType` comes from `nameof`, never a typed string.

**No markup under `Pages/Admin/` branches on culture** — control C09 of `public-site.tsv` scans
`Pages/` without excluding `Admin` and expects the literal two-layout list.

---

## 6. Validation, and what each refusal says

Every refusal renders in the existing `p.error-summary` with `role="alert"`, and every message is a
resource key present in both cultures.

| Rule | Where it comes from | Refusal |
|---|---|---|
| Asset tag required, max 40, **unique across the fleet** | §4.1 | named as a duplicate, not as a database exception — and the screen checks before saving **and** still catches the exception, because two operators can race |
| The product must be a **scooter** | D37: batteries belong to scooter models | a battery cannot be attached to a wheelchair or a stroller |
| `RangeMiles` absent stays absent | D15, control C17 | empty saves as `NULL` and comes back empty, never `0` |
| `ChargerCount` ≥ 0 | §4.2 | a negative count is refused |
| The two amounts ≥ 0 | §4.2 | zero is legitimate — a courtesy battery is zero — and negative is not |
| Every decimal field | D20, `CLAUDE.md:41-44` | binding is `en-US`; a comma decimal is a validation error, never a silent reinterpretation |

---

## 7. Resource keys

Every visible string is a key in **both** `.resx` files, with no blank value, and **every new key
keeps the `Admin_` prefix** — `RenderedTextTests.cs:61-67` asserts that no page body contains the
**name** of any key, over 22 mostly public pages.

**The keys built by interpolation get the same treatment leva 04 gave them:** `BatteryKind` members
are named on screen through `Admin_BatteryKind{...}`, and the test
`Every_enum_member_a_screen_names_has_a_key_in_both_cultures` is **extended** to cover the new enum.
A key that nobody wrote is missing from **both** files equally, so the parity test cannot see it —
that is why the test exists.

---

## 8. Tests

Every absence assertion states a presence in the same method. The authenticated client and the
antiforgery helper of leva 04 are used as they are.

1. **The seed writes twelve batteries** — six per model, five Normal and one Extended Range on the
   Drive Scout, and the command run twice changes nothing.
2. **A duplicate asset tag is refused as validation**, and a different tag saves.
3. **A battery cannot be attached to a non-scooter product**, and one attached to a scooter saves.
4. **An absent range survives the round trip** — empty saves `NULL` and reads back empty.
5. **Retiring a battery keeps it on the administration list**, marked, and the count of available
   ones drops by one.
6. **Every write handler leaves exactly one audit line** — `Assert.Single`, not "at least one".
7. **The settings row exists after the migration and is edited, never created or deleted** — a
   second row is impossible through the screen.
8. **A negative charger count and a negative amount are refused**, and zero is accepted.
9. **The `BatteryKind` members all have keys in both cultures** — the extended interpolation test.
10. **No battery table column carries a store default**, asserted over the model the way the leva 04
    A1 pair does: read `RelationalAnnotationNames.DefaultValue` and `DefaultValueSql` from
    `IProperty`, never `GetDefaultValue()`, which answers the CLR default and would pass either way.

**What this leva must not break**, named so a red is recognised as its own doing: `SeoTests.cs:201`
(the sitemap says nothing about `/admin`), `SeoTests.cs:88` (**do not add any address to
`SiteBehaviourTests.PublicPathList`**), `CultureRoutingTests.cs:64` (**do not link `/admin` from the
public layout**), `SeedingTests.cs:31` (the nine cardinals — `CatalogSeedData` is **not** touched),
`DomainTests.cs:201` (`Only_a_product_on_sale_carries_units` — batteries are not units),
`ArchitectureTests.cs:32` (the write service goes in `Infrastructure/Data/`, never `Application/`).

---

## 9. Visual check

**The roteiro already exists in the tree, as `Docs/conferencia-leva-04b.md`, with one line per item
and an empty result column.** It is written by the reviewer with this spec, committed with it, and
filled by the operator. This is the change the friction review of conversation 4 proposed and the
first front to use it: a roteiro that lives in a chat message comes back as *"I saw everything,
seems fine"*.

The agent does not write that file. He reads it to know what the leva has to make visible, and the
P2 report names anything the roteiro asks for that the seed does not provide.

---

## 10. Open point for Rod, answered at P0

**K1 — What are the twelve asset tags?** The units of leva 02 were seeded with tags of the shape
`SLUG-001` generated by `CatalogSeeder.cs:146`, and that shape is a convention rather than a rule —
there is no regex and no check constraint, only the length and the unique index. Two possibilities,
and the batteries are physical objects with something already written on them: **(a)** the tags
printed on the real batteries, which Rod reads off them; **(b)** generated in the same shape as the
units, and corrected later through the screen this leva builds. **(a) is worth the five minutes**:
a tag the system invented is a tag nobody can match to the object in their hand, and matching is the
entire reason the tag exists.

---

## 11. Controls

### 11.1 Files the front alters

**New:** `Domain/Battery.cs`; `Domain/OperationalSettings.cs`;
`Infrastructure/Data/Configurations/BatteryConfiguration.cs` and
`OperationalSettingsConfiguration.cs`; one migration plus its designer; the battery seed data and
seeder beside `Infrastructure/Seeding/`; the page pairs for `Batteries/Index`, `Batteries/Create`,
`Batteries/Edit` and `Settings/Index` under `Pages/Admin/`; `Docs/controles/fleet-batteries.tsv`;
`Docs/relatorio-leva-04b-etapa-N.md`.

**Modified, and only in this:** `AppDbContext.cs` (two `DbSet`s); `Domain/Enums.cs` (`BatteryKind`,
**appended at the end**); `Infrastructure/Data/CatalogWriter.cs` (the battery and settings writes);
`Infrastructure/Seeding/SeedCommands.cs` (the new command); `Program.cs` (**at most one line**, if
the seed command needs registering — and if it needs none, **zero**); `Pages/Shared/_AdminLayout.cshtml`
(two navigation destinations); `Pages/Admin/Index.cshtml` and `.cshtml.cs` (two counts); both `.resx`;
`wwwroot/css/site.css` (only if a control genuinely has no style yet); `AdminCrudTests.cs`;
`AppDbContextModelSnapshot.cs` (tool-generated); `Docs/fila-cc.md` (the Estado and Commit columns of
this leva's line).

**Negative, by diff column:** `appsettings.json`, `CompanyOptions.cs`, `CatalogSeedData.cs`,
`CatalogSeeder.cs`, `CatalogQueries.cs`, `Application/Catalog/CatalogViews.cs`, `TranslationPicker.cs`,
`RichText.cs`, `PricingTierRules.cs`, `StructuredData.cs`, `Api/SitemapEndpoints.cs`, the four files
of `Infrastructure/Localization/`, `Domain/Unit.cs`, `UnitConfiguration.cs`, the `Units/` pages,
`CLAUDE.md`, `Docs/decisions.md`, `Docs/architecture.md`, `Docs/roadmap.md`, `Docs/open-questions.md`,
`Docs/market-notes.md`, `Docs/backlog-conhecido.md`, `Docs/protocolo-conversa.md`,
`Docs/regras-de-controle.md`, `Docs/medir-controles.sh`, the summaries and atrito files, the earlier
specs, conferences and reports, `Docs/conferencia-leva-04b.md`, **the three existing `.tsv` files**,
`.githooks/pre-commit`, `.gitattributes`, `.github/workflows/ci.yml`, and every test file except
`AdminCrudTests.cs` — each appears in **zero** lines of `git diff --stat` over the leva's range.

**`Docs/conferencia-leva-04b.md` is on the negative list on purpose:** the roteiro is the operator's
to fill, and an agent that writes results into it is writing a conference nobody held.

### 11.2 Controls

The exact command shape is the agent's. The invariants:

1. **No configuration file declares a store default on a boolean column** — C01 of
   `admin-catalog.tsv` is permanent and already measures this over the whole configurations folder.
   **Re-measured, not duplicated.**
2. **Absence never becomes zero** — C17 of `foundation.tsv`, permanent, over all of `src/`.
   Re-measured. It bites here: `RangeMiles` is `decimal?` from the form to the column.
3. **Every write handler under `Pages/Admin/` calls the audit service** — C05 of `admin-catalog.tsv`
   is the relation *handlers minus audit calls = 2*, and it is **permanent**: this leva adds four
   write handlers and four audit calls, so the difference stays 2 and **C06's operand rises from 6
   to 10**. The leva **re-measures both lines in the same commit**; it does not create a parallel
   pair.
4. **The four `TODO-` markers stay in `appsettings.json`** — C16 of `foundation.tsv` is untouched,
   and D4/04b exists so that it stays that way. Re-measured at step 0 and at every stop.
5. **No catalog identifier is typed in code** — C16 of `public-site.tsv`, over `src/` excluding the
   file named `CatalogSeedData.cs`. **The new battery seed file is NOT excluded by that command**, so
   it must not contain a scooter slug: it reaches the product by lookup, not by literal. This is the
   invariant most likely to bite, and it is new to this leva.
6. **The real clock is read in one file only** — C06 of `foundation.tsv`. `CreatedAtUtc` and
   `UpdatedAtUtc` come from `IClock`.
7. **The test seam does not exist in `src/`** — C11 and C12 of `admin-catalog.tsv`. Re-measured.
8. **New:** *no allocation or query filters batteries by kind* (D2/04b), expressed over the files
   this leva adds, with a reach sibling asserting the sweep finds the kind being **read for display**
   at least once — otherwise zero over zero is zero and the control is green measuring nothing.

**Every control rule of `Docs/regras-de-controle.md` applies.**

### 11.3 What STEP 0 measures

1. `dotnet --list-sdks`, `dotnet ef --version`, `sqllocaldb info`, `SELECT DB_NAME()` reading
   `OrlandoUpDb`.
2. `verificar` on **all three** existing `.tsv` — 47 controls on target before anything is touched,
   including C14 and C15, which the reviewer could not measure.
3. `quem-ancora` over every file §11.1 lists as new or modified, and `proibidos` before the first
   comment is written.
4. A `grep` over `src/` and `tests/` for `Battery`, `BatteryKind`, `OperationalSettings` and the
   names chosen for the seed and the command — **expected zero**. Any hit is a stop and the name
   changes before the type exists.
5. Row counts of `Products`, `Units` and `AuditEntries`, for the P1 report.
6. The proposal for `Docs/controles/fleet-batteries.tsv` with `medir` run at the initial HEAD.
7. Confirmation that the test fixture seeds what the §8 tests read.
8. **Any contradiction between this spec and the tree is reported in the plan.**

---

## 12. Out of scope, and why

**Booking anything.** No reservation exists, so nothing here reserves a battery. What this leva
gives leva 03 is a countable pool and the amounts to prefill with.

**The company settings move.** D4/04b, with the measurement that decided it. It stays a front of its
own, gated on Q12, and it retires control C16 when it happens.

**Any public page.** The site says nothing new, and **nothing about the XL** (D37). `RangeMiles` on
the product stays 9, which is what the package promises.

**A policy for a broken or lost battery.** That is Q15, opened on 2026-09-09 by the same answer that
closed Q14. It changes terms and the meaning of the `damage-waiver` add-on, not this schema.

**A `Spitfire` XL.** Q15's second half. The seed writes six Normal for that model because that is
what is known; a row is added through the screen the day one exists.

### 12.1 Statements re-checked and TRUE — do not "fix" them

- **`Unit` has no `UpdatedAtUtc` and does not gain one.** The audit line is its record. The same
  choice is made for `Battery`.
- **`UnitStatus` is persisted by number and its members must not be reordered.** `BatteryKind` is
  appended at the end of `Domain/Enums.cs` for the same reason.
- **The four `TODO-` markers in `appsettings.json` are alive on purpose**, and C16 measures exactly
  four with zero margin. Nothing in this leva touches them.
- **`RangeMiles` on the product is 9 for both scooters and that is correct** — it is the Normal
  battery, and the Normal battery is the package.

---

## 13. Closing

**Two commits.** Content first — code, tests, migration, resources, the new `.tsv`, and the
conference results the operator wrote into the roteiro. Then the closing commit, writing the hash of
the first into the Commit column of this leva's line in `Docs/fila-cc.md`.

**Queue balance:** on receipt **1** `aguardando`; at the end **0**, this line `concluido` with the
content hash, and the number of rows unchanged.

**End of session:** the whole `git status --short` and `git diff --stat`, plus
`bash Docs/medir-controles.sh verificar` on all **four** `Docs/controles/*.tsv`.

**Push is the operator's.**
