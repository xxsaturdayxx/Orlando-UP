# Spec — Leva 04: catalog administration (the first screens that write, and the test client that can reach them)

**Date:** 2026-09-07, conversation 4. **Pair:** conversation 4 ↔ leva 04. **Executor:** Claude Code,
strongest model — the leva changes the schema once, introduces the first authenticated write path
in the application, and builds the test harness every later leva will inherit.

**What this leva closes:** `/admin` reads and never writes. Measured on 2026-09-07: the four page
models under `src/OrlandoUp.Web/Pages/Admin/` are `Index` (three `CountAsync`), `Products/Index`
(one projection, `AsNoTracking`), `Login` and `Logout`; none carries an `OnPost` handler that
touches the catalog, and `Pages/Admin/Products/Index.cshtml:13` renders the resource key
`Admin_ProductsReadOnly` saying so on the screen. Every fact a visitor reads today can only be
changed by editing `CatalogSeedData.cs`, deleting the catalog and re-seeding — which is also the
only way to correct a model name when Q13 is answered.

**Why it runs before leva 03.** `Docs/roadmap.md` declares phase 4 as depending on phase 3, and it
does — for the half of phase 4 that reads bookings (today's deliveries, booking timeline, unit
assignment, calendar). The half specified here reads nothing that does not already exist. Running
it now buys three things leva 03 would otherwise pay for: the operator stops needing a commit to
fix content, the `IsActive` store-default defect is closed before a screen exercises it, and the
authenticated test client — which leva 03 needs for its own admin screens — exists already. This
inversion is recorded as `Docs/decisions.md` D33; leva 03 keeps its number and comes next.

---

> **AMENDMENT EMENDA-04-01 — 2026-09-07, review of `scratchpad/leva04/plano.md` (Claude Web).**
> The plan of leva 04 was reviewed against this spec and against the tree at `200c687`. The body of
> this spec is **not** rewritten — the corrections below amend it, and where a correction and the
> body disagree, **the correction wins**. Everything here was measured, not recalled; the command
> that produced each number is in the item. The agent applies these before writing the first line
> of code, and P1 does not open until they are in the revised plan.
>
> **Verdict: execute after the corrections below.** The plan is complete and reviewable: it names
> the files, declares the migration and whether it touches data, measures every control at the
> initial HEAD, and stopped where it should have. Four of its findings are accepted as written
> (A9–A12) and two of its recommendations are refused (A2, A3).
>
> **A1 — THIS ONE INVERTS A RESULT. The test §8.2 item 1 asks for, as this spec words it, would be
> green before AND after the migration.** The plan measured, on EF Core 10.0.11, that a store
> default declared **by value** (`HasDefaultValue(true)`) makes EF set the property's sentinel to
> that value, so an explicit `false` **is** sent and reads back `false`; only a default declared
> **by SQL** drops it, and this repository declares none that way. So a round-trip test writing
> `false` passes today, before anything changes — a false green by construction, which
> `Docs/regras-de-controle.md` forbids. **The plan's replacement is adopted:** a pair, where the
> form half asserts over the **model** (`IEntityType`/`IProperty`) that the four properties carry
> no store default and that the sentinel is back to the neutral value — red before the migration,
> green after — and the behaviour half keeps the `false` round-trip, which passes both ways and
> proves the write path works. Control C01 of the new `.tsv` is the same assertion measured outside
> the compiler. **The sentence "this is the test that justifies the migration" in §8.2 item 1 is
> withdrawn**; what justifies the migration is D34 as hygiene, stated in A11.
>
> **A2 — REFUSED: `CatalogWriter` and `AuditTrail` are NOT static. `Program.cs` receives exactly two
> registration lines, and §11.1 is amended to allow them.** The plan is right that §11.1 puts
> `Program.cs` in the negative list and that a scoped service needs a line there; it is the reviewer's
> omission, not a design constraint. The project's own rule settles it: *a control that gets in the
> way of the design is to be revised, never the design bent to fit it.* `CatalogQueries` — the
> sibling this spec names in §8.3 — is a class holding the `DbContext`, registered `AddScoped` at
> `Program.cs:111`; the writer is its mirror and is registered the same way. The static classes of
> this codebase (`PricingTierRules`, `TranslationPicker`, `StructuredData`) are all **pure**; none
> holds a `DbContext`. **New negative control, replacing the blanket one for this file:**
> `Program.cs` appears in the leva's range diff with **exactly 2 added lines and 0 removed**, and
> both added lines are service registrations. Anything else in that file is a stop.
>
> **A3 — REFUSED as written, and it is a gap this spec left open: a product created through the
> screen is born HIDDEN.** The create handler sets `IsActive = false` explicitly; the edit screen is
> where it is published. `Product.cs:35` initialises `= true`, which is right for a row the seeder
> writes and fail-**open** for a row a half-filled form writes: the moment the operator clicks save,
> a product with no description, no highlights, no price and no image is on `/rentals`. The C#
> initialiser stays as it is (control C03 of the new `.tsv` still holds); the handler overrides it.
> This composes with A10 — create asks the minimum, saves a hidden draft, redirects to the editor,
> and publishing is a deliberate second gesture. **A test asserts it**: the product created through
> the POST reads `IsActive = false`. That test is also the only place in the leva where the real
> product path exercises an explicit `false` on one of the four repaired columns, which is what A1
> is about.
>
> **A4 — `Product.UpdatedAtUtc` is written on every save of a product, from `IClock.UtcNow`.**
> Measured: `grep -rn UpdatedAtUtc src tests`, excluding `bin`, `obj` and `Migrations`, returns
> **one** line — the declaration at `Domain/Product.cs:52`. Nothing writes it and nothing reads it.
> The editor is the first thing in the project that can fill it, and a column that stays null
> forever is a fact the next front will read as "never edited" and trust. `Unit` has no such column
> and does not gain one in this leva: the audit row is its record, and adding a column to `Units`
> is outside §11.1.
>
> **A5 — the highlights round-trip test of §8.2 item 9 also carries `<`, `>` and `&`, not only a
> quote and a backslash.** Measured: `CatalogSeeder.cs:20` serialises highlights with
> `JavaScriptEncoder.UnsafeRelaxedJsonEscaping`, which leaves `<` and `>` alone. That is safe here
> and stays safe **for a measured reason, not by luck**: highlights never enter a `<script>` — the
> only fields that reach the JSON-LD block are `Name` and `Tagline`
> (`Application/StructuredData.cs`, the `Product` builder), and that block is written with the
> encoder of `StructuredData.cs` `ScriptSafeEncoder`, which calls `ForbidCharacters('<','>','&')`
> after `AllowRange(UnicodeRanges.All)`. The test with the three characters is what keeps the
> reason true. **And the P3 report states, in one sentence:** this leva makes `Name` and `Tagline`
> editable through a screen for the first time, and the guard that protects them is at
> serialisation, so it holds whatever the source of the text is.
>
> **A6 — control C01 of the new `.tsv` must catch `HasDefaultValue(false)` as well.** It measures 4
> today, which is exactly the count of `HasDefaultValue(true)` in
> `Infrastructure/Data/Configurations`; anchored on that literal, a future boolean column declared
> with a `false` default passes the control while breaking D34, whose wording is *no boolean column
> carries a store default* — both directions. The reach sibling C02 stays as proposed.
>
> **A7 — control C05 anchors on the handler NAME, never on the return type.** `Docs/regras-de-controle.md`
> rule 4, corollary: anchoring a count of entry points on the return type repeats inside the control
> the hole that reflection exists to close. The plan's arithmetic is right — 2 write handlers and 0
> audit calls today, 6 and 4 at the end, difference 2 in both, where the 2 is the allowlist of the
> two session handlers — and the P2 report states the form of the command, not only its result.
>
> **A8 — one control is missing, and it is the one that makes D1/04 measurable: the test seam appears
> in ZERO files under `src/`.** `TestAuthHandler` and `FormPost` counted over `src/` alone, expected
> 0, with a reach sibling asserting the same identifiers are at least 1 under `tests/` — otherwise
> zero over zero is also zero and the control is green measuring nothing. D1/04 exists so that no
> authentication bypass can ship; today nothing in the tree would catch one that did.
>
> **A9 — accepted, and this spec's §5 is corrected: `/admin/products/create` asks only what a
> product needs to exist and redirects to the editor of the new product.** The plan's §9 is a
> deviation by **improvement**, registered here rather than left in the plan: it needs no shared
> partial, no file outside §11.1 and no further amendment, and it makes price bands and add-on links
> edited against a row that exists instead of assembled in the air. The wording of §5 that calls
> `/admin/products/create` "the product editor, empty" is replaced by this.
>
> **A10 — the three open points of §10 are answered, all (a), as the plan recommends.** **K1:** the
> unit's product is an editable field, and the audit line names the product it left and the one it
> joined; making it read-only after creation is code added, not removed, and this leva deletes
> nothing. **K2:** `/admin/audit` shows the 100 most recent rows, flat, no filter; a filter buys
> little while one person writes and becomes its own front when there are two. **K3:** nothing is
> saved and the form comes back whole with the message naming which of the six pricing problems it
> is — the objection this spec raised against (a) does not hold, because Razor re-renders from the
> bound properties and the typing comes back; and (b) has a cost (a) does not, which is leaving the
> operator believing a product is on sale when the flag was refused.
>
> **A11 — D34 stands, and its stated MECHANISM is now known to be wrong. Neither is fixed by the
> agent.** The plan's measurement contradicts the mechanism written in D32, D34 and the comment at
> `ProductConfiguration.cs:28-34`, all of which describe the by-value default as the one that drops
> an explicit `false`. The migration is still written and still applied: D34 is the operator's
> decision, it is defensible as schema hygiene on its own — the schema comes to say what the model
> means, and the rule *a boolean column is `IsRequired()` and nothing else* closes the door on the
> by-SQL form, which is the one that does bite — and it is **behaviourally neutral**, which the P1
> report proves rather than asserts. The agent is right not to edit `Docs/decisions.md` or that
> comment: both are in the negative list, and a decision changes only by a new numbered line.
> **The P1 report carries the measurement in full**, naming EF Core 10.0.11 and stating that it was
> run on SQLite and that the finding is about EF's update pipeline and not about the provider — so
> that the correction of the prose is the reviewer's, dated, and recorded as a new decision when P1
> lands.
>
> **A12 — three numbers in the body of this spec are corrected, none of them a divergence.** (i) §0
> says `git ls-files` counts **157** and `git rev-list --left-right --count origin/main...main`
> reads **`0 0`**; both were measured at `cacd725`, before this spec's own commit existed. At
> `200c687` they read **158** and **`0 1`**, the difference being this file and the commit that
> brought it. `src/` is 109 in both, as the body says. The unpushed commit is the operator's to
> push and is not the agent's business. (ii) The opening paragraph says there are **four** page
> models under `Pages/Admin/`; there are **five** — `Pages/Admin/Language.cshtml.cs` is the fifth,
> and it writes the administration's culture cookie (D4/01). The assertion that matters is
> unchanged and was re-checked: **no POST handler under `Pages/Admin/` touches the catalog.**
> (iii) §3 inherits "137 tests" as `[H]`; the plan measured **138** passing at `200c687`. The
> pending mark is discharged with that number.
>
> **Nothing else in the plan is corrected.** The step-0 measurements, the `quem-ancora` and
> `proibidos` sweeps, the eleven controls the plan adds to the seven this spec named, the zero-count
> of every new identifier, the row counts, the harness of §6.3 with its three proofs, and the
> execution order of §10 are accepted as written.
>
> **Proof of reading, required in the next artifact the agent produces:** a search for the string
> `EMENDA-04-01` in the revised `scratchpad/leva04/plano.md`, expected `>= 1`, with the count
> reported.

---

> **AMENDMENT EMENDA-04-02 — 2026-09-08, review of the revised `scratchpad/leva04/plano.md`
> (Claude Web).** Second round. Items are numbered `B` to keep them apart from the `A` items of
> `EMENDA-04-01`, which stays in force except where corrected below.
>
> **Verdict: execute. P1 is open.** The twelve items A1–A12 are applied; the twelve controls of
> `Docs/controles/admin-catalog.tsv` were re-measured by the reviewer at `ec01a99` and every value
> is identical to the one the plan reports. Two items of `EMENDA-04-01` are corrected here, and
> both corrections are the agent's, not the reviewer's.
>
> **B1 — A6 is withdrawn: its diagnosis was a supposition presented as a measurement.** A6 stated
> that control C01 was "anchored on the literal `true`", inferring it from the fact that it measured
> 4. The reviewer had not read the command — the first round of the plan reported labels and values
> only — and the command already covered both directions. Re-measured today, from the written file,
> against synthetic targets outside the repository: the form matches `HasDefaultValue(true)` and
> `HasDefaultValue(false)` (2 of 2) and matches neither `HasDefaultValue(0)`, `HasDefaultValue(0m)`
> nor `HasDefaultValueSql(...)` (0). **What A6 asked for is satisfied; the reason it gave was
> wrong.** The rule the reviewer broke is the project's own: an assertion about what a file shows is
> emitted only after opening that file.
>
> **B2 — A8 was incomplete, and the control it asked for was born a false green. The agent caught it
> with the two-sided assertion, which is the assertion working.** A8 asked for a reach sibling
> asserting the test-seam identifiers appear at least once under `tests/`, and did not carry into it
> the sweep rule this repository already pays for. Reproduced by the reviewer today, outside the
> repository: with nothing but a binary under `bin/`, the naive form returns `sim` — green before
> the seam exists; with `-I` and `--exclude-dir=bin --exclude-dir=obj` it returns `nao`, and returns
> `sim` only once a real source file carries the seam. The identifier of the antiforgery helper is
> **`FormPoster`**, not `FormPost`, because the shorter string occurs inside compiled assemblies.
> The header of `Docs/controles/public-site.tsv` records the same trap for the same reason; A8
> should have cited it. **Controls C11 and C12 as written in the plan are adopted as written.**
>
> **B3 — nothing else is corrected.** The re-measurement of all 35 existing controls at `ec01a99`
> (a scope-changing amendment forces it, and the agent did it rather than copying the values from
> `200c687`), the two-sided assertions of C01, C05 and C11/C12 run from the written file, the
> negative of A2 moved to the P3 report as a scope negative measured against the commit range rather
> than into the permanent `.tsv`, and the re-anchoring of C05's audit operand on `[.]Record[(]` now
> that the services are injected, are all accepted as written. The execution order of §9 stands.
>
> **Proof of reading, required in the next artifact the agent produces:** a search for the string
> `EMENDA-04-02` in `Docs/relatorio-leva-04-etapa-1.md`, expected `>= 1`, with the count reported.

---

> **AMENDMENT EMENDA-04-03 — 2026-09-08, review of the P1 report and of the migration
> (Claude Web).** Third round. Items are numbered `C`. `EMENDA-04-01` and `EMENDA-04-02` stay in
> force.
>
> **Verdict on the migration: apply, after C1.** Classified by the reviewer from the generated SQL,
> not from the `.cs`, as the `revisao-migration-efcore` skill requires.
>
> - **`Up`: zero destructive statements and zero data statements.** Four guarded
>   `DROP CONSTRAINT` of default constraints (a default constraint holds no data), one
>   `CREATE TABLE [AuditEntries]`, one non-unique unfiltered `CREATE INDEX` on a table that is
>   empty by construction, one history row. The whole script is inside one transaction. The plan
>   promised no data statement and said an emitted `UPDATE` would be a stop; none was emitted.
> - **`Down`: one destructive statement, and it is the right one.** `DROP TABLE [AuditEntries]`
>   loses the audit rows written since the migration — inherent to rolling back the table that holds
>   them, and it destroys nothing that predates the migration. The four defaults are put back.
> - **Traps checked, one by one:** no `HasData` anywhere, so nothing reconciles; `AuditAction` is
>   persisted as `int` with explicit values appended at the end of `Domain/Enums.cs`, displacing
>   nothing; the index is neither unique nor filtered, so no `QUOTED_IDENTIFIER` requirement and no
>   duplicate check needed; no self-referencing foreign key, so no cascade cycle; `OccurredAtUtc` is
>   `datetime2` and an **instant**, and no calendar date is added; every string column is bounded
>   (`nvarchar(256)`, `(40)`, `(400)`) with no `nvarchar(max)` by inattention; nothing is renamed, so
>   no drop-and-add masquerading as a rename; the only new `NOT NULL` columns are on the new empty
>   table.
> - Re-measured by the reviewer at `ac01f5e`: no BOM on the three generated files; the four
>   configuration diffs are one line each and nothing else; the 17 controls of `public-site.tsv` are
>   on target and the 18 of `foundation.tsv` are too except C14 and C15, which answer `127` to the
>   reviewer's shell for want of `dotnet`; control C01 of the new `.tsv` moved 4 → **0** and C03
>   held at 0, which is the pair proving the store defaults left without costing the C# initialisers.
>
> **C1 — the §7 finding is answered (b): the fifth default constraint is removed in THIS migration,
> and §4.1 is amended from four columns to five.** The agent measured, in `sys.default_constraints`
> of `OrlandoUpDb`, that `Products.IsBookable` carries a `DEFAULT ((0))` created by leva 02's
> `AddColumn<bool>(… defaultValue: false)` — a constraint SQL Server made permanent, that the model
> snapshot never carried, and that therefore no later migration removes. He was right to stop rather
> than widen the migration on his own. **(b) is chosen because leaving it makes D34 false in the
> database on the day it becomes true in the model**, and a decision that holds in one of its two
> places is the defect this project has already paid for three times. The block is written by hand,
> with the same dynamic `DECLARE`/`EXEC` the generator emits, because the constraint's name is
> server-generated and differs between databases. **It is symmetric: the `Down` puts the
> `IsBookable` default back**, so a rollback lands on the state this migration found and not on a
> third state no migration describes. The removal is behaviourally inert, and that is measured in
> §7 of the report, not assumed: with `valueGenerated=Never` the EF names the column in every
> `INSERT`, and this repository contains no raw `INSERT` that omits it. The report gains a
> `## Revisão (Claude Web, 2026-09-08)` section recording the added block and its re-measurement,
> and the operator applies only after that section is committed.
>
> **C2 — §11.1 is opened for one more thing, and only this: the comment at
> `ProductConfiguration.cs:28-34` is rewritten in the content commit.** It describes the mechanism
> backwards, and after this migration it also sits under an `IsActive` that no longer has a default,
> which makes its "on this one, on purpose" ambiguous as well as wrong. Rules 2 and 3 of
> `Docs/regras-de-controle.md` exist because prose sitting next to a control and describing it
> wrongly is a defect already bought here. The new text says what was measured — that the form which
> swallows an explicit `false` is `HasDefaultValueSql` on a non-nullable `bool`, not
> `HasDefaultValue(value)` — and it **describes the forbidden form without transcribing it**, so it
> cannot break control C01. Nothing else in that file is touched.
>
> **C3 — `Docs/decisions.md` D35 records the correction of the mechanism, written by the reviewer,
> dated today, with the provenance of the measurement attached.** The agent was right not to touch
> D32, D34 or the comment on his own. D35 does not change what D34 *does*; it corrects why.
>
> **C4 — nothing else. The report's own answers are accepted:** the step-0 re-measurement at
> `bc8b762` rather than copied from the plan, the row counts taken before the migration was written,
> the A11 measurement re-run rather than quoted, the snapshot diff that names where the A1 form
> assertion will measure in step 3, and the explicit list of what P1 deliberately did not do. The
> `.tsv` staying in `scratchpad/` until the content commit is accepted as the plan stated, on the
> condition that it lands **before** the end-of-session gate, since that gate verifies three files.
>
> **Proof of reading, required in the next artifact the agent produces:** a search for the string
> `EMENDA-04-03` in the `## Revisão` section of `Docs/relatorio-leva-04-etapa-1.md`, expected
> `>= 1`, with the count reported.

---

## 0. Execution surface

**Launcher phrase:** this spec is executed by the line of `Docs/fila-cc.md` dated `2026-09-07`
whose description starts with *"LEVA 04 — CATALOG ADMINISTRATION"*. **Not "the `aguardando`
line"** — that line, by name.

**Tree state at receipt, measured 2026-09-07:** HEAD is a descendant of `cacd725`
(`docs: resumo da conversa 3`); the commit that adds this spec and its queue line comes after it
and is the expected HEAD. `git status --porcelain` **empty**. `git rev-list --left-right --count
origin/main...main` reads `0 0` — nothing to push, nothing to pull. `git ls-files` counts **157**
files, **109** under `src/`. `Docs/controles/foundation.tsv` has 18 controls and
`Docs/controles/public-site.tsv` has 17; every control except C14 and C15 of `foundation.tsv` was
re-measured from the file on 2026-09-07 and is on target. **C14 and C15 were not measured by the
reviewer**: they shell out to `dotnet build` and `dotnet test`, and `dotnet` does not exist in the
reviewer's shell — it answers `127`, which is `command not found` and not a red gate. The agent
measures those two at step 0 and reports the real value; if either is red on arrival, that is a
stop with a report.

A Windows CRLF warning and a stale git index are not divergence — they exist on some trees and not
others; ignore them. Any other modified or untracked file outside `scratchpad/` is a stop with a
report.

**Files the front ALTERS:** the closed list is §11.1. A file altered outside it is a stop with a
report, **without a cardinal** — the list discriminates the intruder, never a count, which ages
between writing and execution.

**Files the front PRODUCES as record** (authorized by this declaration; they are not scope drift):
`scratchpad/leva04/plano.md` (never committed); `Docs/relatorio-leva-04-etapa-N.md`, one per stop,
committed before approval is asked; `Docs/controles/admin-catalog.tsv`; `Docs/conferencia-leva-04.md`.

**Files the TOOL generates coupled:** `dotnet ef migrations add` rewrites
`src/OrlandoUp.Web/Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs` and writes the
migration's own `.Designer.cs`. Both are authorized by this declaration and are not a stop.

**Steps that need a human hand, with the exact command.** The operator's shell is **PowerShell on
Windows**; the commands below are written for it and carry no Bash syntax.

1. **Apply the migration, after P1 is approved** — the operator runs it, not the agent:

   ```
   dotnet ef database update --project src/OrlandoUp.Web
   ```

2. **Strip the BOM before `git add`.** `CLAUDE.md:82-83` records it and `.githooks/pre-commit`
   enforces it: `dotnet ef migrations add` writes UTF-8 with a byte-order mark and the hook refuses
   a file whose first three bytes are `efbbbf`. This leva generates one migration plus its
   designer plus the snapshot — three files at risk.

3. **Sign in to `/admin`** during the visual check. The credentials are the two user-secrets seeded
   in leva 01 (`AdminSeed:Email`, `AdminSeed:Password`); they belong to the operator and are never
   printed by the agent (D24).

4. **The visual check of §9 is the operator's**, on his machine, at `https://localhost:7420`, after
   the migration is applied. What it does not reach goes into `Docs/conferencia-leva-04.md` with
   the measured reason, never left unsaid.

**Four mandatory stops.** In each one the report is committed **before** approval is asked.

| Stop | When | What it carries |
|---|---|---|
| **P0** | after step 0, before altering any file | the plan, with the answers to the three open points of §10 |
| **P1** | migration written and **not applied** | the script classified by the `revisao-migration-efcore` skill, with row counts of all four affected tables measured before |
| **P2** | the test harness of §8.1 green, before the first CRUD screen exists | proof that an authenticated test client reaches an admin page and that a POST with the antiforgery token succeeds — and that the same POST **without** the token fails |
| **P3** | everything written, suite green, before the content commit | the full run, both `.tsv` verified, the visual check pending |

P2 is not ceremony. Measured 2026-09-07: `grep -rn "AuthenticationHandler\|ClaimsPrincipal\|TestAuthHandler\|Antiforgery\|RequestVerificationToken\|CookieContainer\|SignIn" tests/ --include=*.cs`
returns **nothing**. No test in this repository can open an admin page other than `/admin/login`,
and no test can post a form. Until P2 is green, every screen this leva writes would be untested by
construction.

---

## 1. What the leva delivers, in plain words

Rod signs in at `/admin`, opens a product, and changes what the site says about it — the English
name and the Portuguese one, the tagline, the description, the bullet list of highlights, the
dimensions, whether it is visible, whether it is on sale, the price bands, which add-ons it offers.
He saves, opens `/rentals` in another tab, and the change is already there. He can create a product
that does not exist yet, and he can hide one without losing it.

He also keeps the fleet: a list of the physical units, each with the tag stuck on the machine, its
serial, its status (available, in maintenance, retired) and the date it was bought. When a scooter
goes to the shop he marks it, and when Q13 is answered he corrects the model names from the labels
without touching a source file.

Every write leaves a line: who, when, what changed. `/admin/audit` shows the most recent ones.

**What this is not.** It is not the booking side of the back office — there are no bookings yet, so
today's deliveries, the booking timeline, unit assignment and the calendar are not here and cannot
be. It is not delivery-zone or add-on administration: the zones and the add-ons themselves stay
seed data in this leva (only the *link* between a product and an existing add-on is editable). It
is not company settings — those live in `appsettings.json` and moving them to the database is its
own front (§12). And it is not a delete feature: nothing in this leva removes a row.

---

## 2. Decisions of this leva

All settled on 2026-09-07, before any block of this spec existed. `[operator]` marks Rod's answers;
`[assistant]` marks the ones taken by the reviewer under the autonomy clause of
`Docs/protocolo-conversa.md`.

**D1/04 — Test authentication is a seam in the TEST project, never in the application.**
`[assistant]` `SiteFactory` gains `ConfigureTestServices` registering an
`AuthenticationHandler<AuthenticationSchemeOptions>` that issues a principal carrying
`Roles.Admin`, and a helper that returns a client already authenticated. `Program.cs` is not
touched, and no environment check is added to it. Motive: an environment-conditional bypass in
production start-up is a hole that ships — it is exactly one misconfigured `ASPNETCORE_ENVIRONMENT`
away from an open administration, and nothing in the suite would notice. Measured: `Program.cs`
contains no `IsEnvironment`/`Testing` branch today (`grep -nE "Testing|IsEnvironment"
src/OrlandoUp.Web/Program.cs` returns only the `IsDevelopment()` of line 138); this decision keeps
it that way.

**D2/04 — Antiforgery stays ON in tests, and the token is read from the rendered form.**
`[assistant]` The test helper does a `GET` of the page, extracts the value of the hidden
`__RequestVerificationToken` input from the returned HTML, and sends it in the `POST` body; the
antiforgery cookie travels because `WebApplicationFactory`'s client handles cookies. Motive:
disabling antiforgery in the test host would make the suite green on a page that cannot be posted
in production, and it would stop proving that the form is a real Razor `<form method="post">` — the
only shape that gets the hidden field injected. Measured: there is no antiforgery configuration
anywhere (`grep -rni antiforgery src tests` returns nothing outside `obj/`), so the application
runs on the Razor Pages default, which validates every non-GET handler.

**D3/04 — No admin markup branches on culture, and no admin partial is created under
`Pages/Shared/`.** `[assistant]` The product editor shows both translations at once, laid out
unconditionally side by side; anything that depends on the current culture is decided in the page
model, which is a `.cs` file. The admin navigation of D9/04 lives inside `_AdminLayout.cshtml`
itself. Motive, measured: control **C09** of `public-site.tsv` scans `Pages/` **without excluding
`Admin`**, and its expected value is the literal two-path list
`…/Pages/Shared/_AdminLayout.cshtml,…/Pages/Shared/_Layout.cshtml`; one new `.cshtml` under
`Pages/Admin/` mentioning `CurrentUICulture`, `SiteCultures` or `isPortuguese` turns it into three
paths and breaks a control of leva 02, which is closed. Control **C10** excludes `Pages/Admin/`
with `--exclude-dir=Admin` and excludes the `_AdminLayout` lines by name — but it does **not**
exclude `Pages/Shared/`, so an admin partial there carrying `asp-page=` without
`asp-route-culture=` would break it too.

**D4/04 — The store default leaves all four `IsActive` columns in one migration, and the existing
rows are filled by an explicit statement.** `[operator]` Measured 2026-09-07 with
`grep -rn HasDefaultValue src/OrlandoUp.Web/Infrastructure/Data/Configurations`: six occurrences,
four of them `bool` — `ProductConfiguration.cs:26`, `AddOnConfiguration.cs:20`,
`DeliveryZoneConfiguration.cs:22`, `DeliveryLocationConfiguration.cs:18`. The backlog entry of
2026-09-06 recorded only the first. The defect is the one written in `ProductConfiguration.cs:28-34`
to explain why `IsBookable` was left without a default: a store default on a non-nullable `bool`
makes the provider unable to tell *the caller said false* from *the caller said nothing*, so the
`false` is dropped and the row is inserted visible. Rod chose to close all four now: the repair is
mechanical and identical in the four, and reopening a migration in a later leva costs more than
doing it while one is already open. `TurnaroundDays` (default `0`) and `SalesTaxRate` (default `0m`)
share the mechanism but are **not** touched — see §12.

**D5/04 — The slug stays editable, with a warning on the screen, and becomes read-only at phase 5.**
`[operator]` `Domain/Product.cs:8` calls the slug "stable after publication"; it is the public
address of the product and the sitemap is generated from it. Indexing is off today
(`appsettings.json`, `Seo:AllowIndexing: false`), and Q13 says the real model names are still to be
read off the labels — so a correction is cheap exactly now and expensive later. The field carries a
visible warning that changing it changes the public address, and a slug change is one of the audited
actions of D7/04. The screen makes it read-only when Q13 is closed and the site is opened to
indexing; that is a phase-5 gate item, not this leva.

**D6/04 — Highlights are edited as one line per item; raw JSON is never shown or accepted.**
`[assistant]` The page model splits the textarea on line breaks, drops blanks, and serializes;
reading does the inverse. Motive, measured: `Infrastructure/Data/CatalogQueries.cs:192-195` catches
`JsonException` and returns an empty list. A screen that accepts hand-typed JSON is a screen where a
typo silently empties the highlights of a product with nothing failing anywhere — the same class of
defect D15 exists for.

**D7/04 — The audit trail is written explicitly by each admin write handler through one service, not
by a `SaveChanges` interceptor.** `[assistant]` Motive, measured: a global interceptor fires
wherever `SaveChangesAsync` is called, and two of those callers are tests with no `HttpContext` and
no signed-in user — `SeoTests.cs:144-146` writes `hidden.IsActive = false` to prove the sitemap
drops a hidden product, and `SeedingTests` writes the whole catalog. An interceptor that demands an
actor would turn those red for a reason unrelated to what they test; one that tolerates a missing
actor writes audit rows that say nobody did it, which is worse than no audit. The guarantee an
interceptor would give is replaced by the relation control of §11.2 item 4: *number of write
handlers under `Pages/Admin/` minus number of audit calls = 0*.

**D8/04 — The pricing-tier editor calls `PricingTierRules.Validate` and refuses to save on anything
but `None`. It never reimplements the rule.** `[assistant]` `Domain/PricingTierRules.cs:3-6` says
the rule lives outside the pages "so that every caller — seeder, admin screen, booking — gets the
same answer"; the admin screen is the caller it was written for. Measured: the database does **not**
enforce the set-level rule — `PricingTierConfiguration.cs:24` creates `(ProductId, MinDays)` as a
**non-unique** index, and the three check constraints of `:13-15` are per row. Overlap, gap and a
missing open-ended band are caught only here.

**D9/04 — `/admin` gains a navigation bar in `_AdminLayout.cshtml`, and the per-page back-links go
away.** `[assistant]` Measured: today the whole navigation is three hand-written links
(`_AdminLayout.cshtml:26` to the dashboard, `Admin/Index.cshtml:28` to the product list,
`Admin/Products/Index.cshtml:10` back to the dashboard). This leva adds four screens; continuing
the pattern multiplies back-links and still leaves no way to reach the fleet from the catalog. The
bar carries: dashboard, products, fleet, audit. It lives in `_AdminLayout.cshtml` and not in a
shared partial (D3/04).

**D10/04 — `Admin` and `Staff` both write; no new policy is created.** `[assistant]` The folder
convention `AuthorizeFolder("/Admin", AuthorizationPolicies.Staff)` (`Program.cs:104`) covers every
new page with no code. A write-only policy that distinguishes the two roles is ceremony while one
person holds both; it becomes real when a delivery employee gets an account. Recorded as backlog.

**D11/04 — A product cannot be saved without an English name.** `[assistant]` Measured:
`Application/Catalog/TranslationPicker.cs:29` falls back to `SiteCultures.English` when the
requested culture has no row, and `CatalogQueries.cs:41-46` omits from the card list a product with
neither. So a product whose English row is blank disappears from the site **in both cultures**,
silently, with a 200 on the page that no longer lists it. The Portuguese name is optional and its
absence renders the existing `Admin_MissingTranslation` marker in the list.

**D12/04 — Nothing in this leva deletes a row.** `[assistant]` `ProductConfiguration.cs:56-59`
makes the product→unit relation `OnDelete(Restrict)`, so a product with fleet cannot be deleted by
the database anyway; `IsActive = false` is the hide, and it is what the whole catalog read path
already honours. A unit that leaves service becomes `UnitStatus.Retired`. Motive: delete is the one
gesture that cannot be reviewed after the fact, and the audit trail of a deleted row has nothing to
point at.

**D13/04 — The records of `Application/Catalog/CatalogViews.cs` are not touched; the admin carries
its own input models.** `[assistant]` Measured: `SeoTests.cs:261-273` constructs
`new ProductDetail(...)` **positionally, with 17 arguments**. Changing the shape of that record
does not fail an assertion — it fails compilation, which turns C14 and C15 of `foundation.tsv` red
together and stops the suite before a single test runs.

---

## 3. The measured terrain

Everything `[V]` on 2026-09-07, read from the working tree at `cacd725` through the file bridge.
One row per file; the command that reached **that** file is in the third column. `dotnet` is not
reachable from the reviewer's shell, so nothing below was compiled or executed.

| File | Fact | How it was measured |
|---|---|---|
| `src/OrlandoUp.Web/Program.cs` | `:104` `AuthorizeFolder("/Admin", AuthorizationPolicies.Staff)` and `:105` `AllowAnonymousToPage("/Admin/Login")`. Every new page under `Pages/Admin/` is closed by convention, with no attribute and no registration. | `cat -n src/OrlandoUp.Web/Program.cs` |
| `src/OrlandoUp.Web/Program.cs` | No `[Authorize]` attribute exists anywhere in `src/`. | `grep -rn "\[Authorize" src --include=*.cs --include=*.cshtml` → empty outside `obj/` |
| `src/OrlandoUp.Web/Program.cs` | No antiforgery registration, no `IMemoryCache`, no `OutputCache`, no `ResponseCache`. The only `Cache-Control` is `:154`, on static files. | `grep -rniE "antiforgery|responsecache|outputcache|IMemoryCache" src` → empty outside `obj/` |
| `src/OrlandoUp.Web/Infrastructure/Localization/CultureRouteConvention.cs` | `:27` returns early for any `ViewEnginePath` starting with `/Admin`. A new admin page gets no culture route, automatically. | `cat -n` |
| `src/OrlandoUp.Web/Infrastructure/Localization/PublicPages.cs` | `:32-33` derives the public set from `IActionDescriptorCollectionProvider`; `:68-71` `IsPublic` excludes `/Admin`, `/Error`, `/Shared`. Not a typed list. | `cat -n` |
| `src/OrlandoUp.Web/Infrastructure/Data/CatalogQueries.cs` | `:27-33` reads `Products` from the `DbContext` per request, `AsNoTracking()`, filtered by `IsActive`. Registered scoped at `Program.cs:111`. **An edit in the admin shows on the public site on the next request; there is no reload step.** | `cat -n` + the cache grep above |
| `src/OrlandoUp.Web/Infrastructure/Data/CatalogQueries.cs` | `:192-195` catches `JsonException` reading `Highlights` and returns an empty list — malformed JSON is swallowed silently. | `cat -n` |
| `src/OrlandoUp.Web/Application/Catalog/TranslationPicker.cs` | `:29` falls back to English when the requested culture has no row. | `cat -n` |
| `src/OrlandoUp.Web/Infrastructure/Data/Configurations/ProductConfiguration.cs` | `:26` `IsActive` carries `HasDefaultValue(true)`; `:35` `IsBookable` deliberately does not; `:28-34` is the comment that diagnoses the defect. | `cat -n` |
| `src/OrlandoUp.Web/Infrastructure/Data/Configurations/AddOnConfiguration.cs` | `:20` `IsActive` carries `HasDefaultValue(true)`. | `grep -rn HasDefaultValue …/Configurations` |
| `src/OrlandoUp.Web/Infrastructure/Data/Configurations/DeliveryZoneConfiguration.cs` | `:22` `IsActive` carries `HasDefaultValue(true)`; `:21` `SalesTaxRate` carries `HasDefaultValue(0m)`. | idem |
| `src/OrlandoUp.Web/Infrastructure/Data/Configurations/DeliveryLocationConfiguration.cs` | `:18` `IsActive` carries `HasDefaultValue(true)`. | idem |
| `src/OrlandoUp.Web/Infrastructure/Data/Configurations/UnitConfiguration.cs` | `:15` `AssetTag` max 40 and required; `:16` unique index on `AssetTag`; `:22` `PurchasedOn` is `HasColumnType("date")`. No default value anywhere in the file. | `cat -n` |
| `src/OrlandoUp.Web/Infrastructure/Data/Configurations/PricingTierConfiguration.cs` | `:13-15` three per-row check constraints; `:22` `Amount` precision `(10,2)`; `:24` index `(ProductId, MinDays)` **not unique**. | `cat -n` |
| `src/OrlandoUp.Web/Infrastructure/Data/Configurations/ProductTranslationConfiguration.cs` | `:21` unique index `(ProductId, Culture)`; `:16` `Name` max 120; `:17` `Tagline` max 200; `:18` `Description` and `:19` `Highlights` required and unbounded. | `cat -n` |
| `src/OrlandoUp.Web/Infrastructure/Data/Configurations/ProductAddOnConfiguration.cs` | `:13` composite key `(ProductId, AddOnId)` — a duplicate link is impossible by primary key; cascade on both sides (`:18`, `:23`). | `cat -n` |
| `src/OrlandoUp.Web/Domain/Unit.cs` | Has `CreatedAtUtc` (`:24`) and **no** `UpdatedAtUtc`; `Product.cs:52` has one. | `cat -n` on both |
| `src/OrlandoUp.Web/Pages/Shared/_AdminLayout.cshtml` | `:4-5` branches on `CultureInfo.CurrentUICulture` / `SiteCultures.Portuguese`; `:18` loads `~/css/site.css` — the admin shares the public stylesheet, there is no `admin.css`. | `cat -n` |
| `src/OrlandoUp.Web/Pages/Admin/Products/Index.cshtml.cs` | `:29-45` projects with `AsNoTracking()` and **no** `IsActive` filter; `:27-28` states the reason. | `cat -n` |
| `src/OrlandoUp.Web/Pages/Admin/Login.cshtml` | `:17` is a bare `<form method="post">` — the tag helper injects the antiforgery field, nothing is written by hand. It is the shape every new form must copy. | `cat -n` |
| `src/OrlandoUp.Web/Resources/SharedResource.resx` | 163 `<data name=` entries, of which 23 start with `Admin_`. | `grep -c '<data name=' …resx` → 163; `grep -o 'name="Admin_[^"]*"' …resx \| sort -u \| wc -l` → 23 |
| `src/OrlandoUp.Web/wwwroot/css/site.css` | `.field`, `.field label`, `.field input`, `.error-summary`, `.table-scroll`, `.button--quiet`, `.todo`, `.stat` exist. **No rule for `select`, `textarea` or `checkbox`.** | `grep -n` per selector |
| `src/OrlandoUp.Web/Infrastructure/Seeding/CatalogSeeder.cs` | `:31-37` inserts into emptiness or does nothing, and `:13-16` says why: the rows become editable content the moment an administrator touches them. `:50`, `:81`, `:176`, `:197` write `IsActive = true` literally — the seeder never exercises the `false` path where the defect lives. | `cat -n` |
| `src/OrlandoUp.Web/Infrastructure/Seeding/CatalogSeedData.cs` | The only place in `src/` where a catalog slug or zone code is typed. Nothing outside `Infrastructure/Seeding/` carries one. | `grep -rn "<the eleven identifiers>" src \| grep -v Seeding/` → only two `CategoryArt.cs` lines, which are SVG file names keyed by the enum, not slugs |
| `tests/OrlandoUp.Tests/` (whole folder) | **No authentication, no antiforgery handling, no cookie container.** | `grep -rn "AuthenticationHandler\|ClaimsPrincipal\|TestAuthHandler\|WithWebHostBuilder\|Antiforgery\|RequestVerificationToken\|CookieContainer\|SignIn" tests/ --include=*.cs` → empty |
| `tests/OrlandoUp.Tests/SiteFactory.cs` | `:42` environment `Testing`; `:52-66` swaps the provider for SQLite in memory; `:83` and `:98` create the schema with `EnsureCreatedAsync()` from the model. **The suite never runs a migration.** | `cat -n` |
| `tests/OrlandoUp.Tests/SeoTests.cs` | `:144-146` writes `hidden.IsActive = false` and calls `SaveChangesAsync()` with no `HttpContext`. `:261-273` builds `ProductDetail` with 17 positional arguments. | `cat -n` |
| `tests/OrlandoUp.Tests/RenderedTextTests.cs` | `:28-49` is a hand-typed list of 22 addresses; `:49` is `/admin/login`, the only admin page in it. `:61-67` asserts that no page body contains the **name** of any of the 163 resource keys. | `cat -n` |
| `tests/OrlandoUp.Tests/SiteBehaviourTests.cs` | `:112-124` `PublicPathList`, 21 typed addresses, feeds two `[Theory]` here and the cross-check at `SeoTests.cs:88`. `:273-289` asserts that no registered service name contains `EmailSender`. | `cat -n` |
| `tests/OrlandoUp.Tests/DomainTests.cs` | `:50` asserts `product.IsActive` is true — that assertion is about the **C# initializer** `Product.cs:35`, not about the store default. `:44-46` is a comment that becomes false once the store default is removed. | `cat -n` |
| `Docs/controles/public-site.tsv` | C09 scans `Pages/` with no `Admin` exclusion and expects a literal two-path list. C10 excludes `--exclude-dir=Admin` and the `_AdminLayout` lines, but **not** `Pages/Shared/`. C16 scans `src/` for eleven catalog identifiers excluding only the file named `CatalogSeedData.cs`. | `cat` + `bash Docs/medir-controles.sh verificar Docs/controles/public-site.tsv` |
| `Docs/controles/foundation.tsv` | C17 expects `0` occurrences of the two null-to-zero forms across `src/` (`.cs` and `.cshtml`). C06 expects the real-clock read to live in exactly one file, `Infrastructure/SystemClock.cs`. C11 expects Markdig/Ganss to enter through `Application/RichText.cs` alone. | `cat` + `verificar` |

**Inherited and not re-checked `[H]`, therefore pending, not fact:** the suite counts 137 tests and
both `.tsv` were green at the close of leva 02 — read from the agent's report in
`Docs/resumo-conversa-3.md` §1, never measured by the reviewer, because C14 and C15 need `dotnet`.

---

## 4. The schema change

One migration. Suggested name: `RemoveActiveFlagStoreDefaultsAndAddAuditEntries` — the project names
migrations verb-plus-object in PascalCase, with no leva number (`InitialCreate`,
`AddIsBookableAndOptionalDimensions`).

### 4.1 The four store defaults

`HasDefaultValue(true)` is removed from the model for `Products.IsActive`, `AddOns.IsActive`,
`DeliveryZones.IsActive` and `DeliveryLocations.IsActive`. The C# initializer `= true` stays on all
four domain classes — it is what `DomainTests.cs:50` asserts, and it is the right default for a row
somebody creates without thinking about visibility. What must not exist is the **store** default,
which is what makes the provider drop an explicit `false`.

The `Up` drops the four column defaults. It does **not** need to fill existing rows: the columns are
already `NOT NULL` and every existing row already carries a value — unlike
`AddIsBookableAndOptionalDimensions`, which added a column and therefore had to state what the old
rows meant. The agent measures the row counts of the four tables **before** writing the migration
and puts them in the P1 report; if any of the four is non-zero and the generated `Up` contains a
data statement, that is a stop.

The `Down` restores the four defaults, and this direction is honest: putting a default back cannot
invent a value for a row that already has one.

**The comment at `DomainTests.cs:44-46` becomes false in this leva and is rewritten in it.** It
currently explains the store default as the mechanism that fills old rows and contrasts `IsBookable`
with `IsActive`. Rules 2 and 3 of `Docs/regras-de-controle.md` exist because prose that sits next to
a control and describes it wrongly is a defect this project has already paid for.

### 4.2 The audit table

A new entity, `Domain/AuditEntry.cs`, and its configuration. It is a plain POCO with no navigation
property and no foreign key to Identity: an audit row must survive the deletion of the account that
wrote it, and `ArchitectureTests.cs:17` forbids `OrlandoUp.Domain` from depending on any other
layer.

| Column | Type | Null | Why |
|---|---|---|---|
| `Id` | `int` identity | no | — |
| `OccurredAtUtc` | `datetime2` | no | An **instant**, not a calendar date. Read from `IClock` (D16), never `DateTime.UtcNow` — control C06 of `foundation.tsv` expects the real clock to be read in one file only. |
| `ActorEmail` | `nvarchar(256)` | no | The signed-in user's name at the moment of the write, copied as text. A row, not a link: the account can be renamed or removed and the record must still say who. |
| `EntityType` | `nvarchar(40)` | no | `Product`, `ProductTranslation`, `PricingTier`, `ProductAddOn`, `Unit`. Written from `nameof`, never a typed string literal. |
| `EntityId` | `int` | no | The key of the row that changed. For `ProductAddOn`, which has a composite key, it is the **product** id — and the summary names the add-on. |
| `Action` | `int` (enum `AuditAction`) | no | `Created = 1`, `Updated = 2`, `Deactivated = 3`, `Reactivated = 4`. Explicit numbers, like every other enum in `Domain/Enums.cs`: the values are persisted and a reordered member would silently repoint existing rows. |
| `Summary` | `nvarchar(400)` | no | One human sentence in English, for a human to read: what changed, from what to what. Never a serialized diff. |

Index on `(OccurredAtUtc)` descending, because the only read is "the most recent ones".

**No `HasDefaultValue` on any column of this table.** That is the whole point of D4/04.

---

## 5. The screens

Every route is written lowercase and by hand on the `@page` directive, as the four existing admin
pages do (`@page "/admin"`, `@page "/admin/products"`, `@page "/admin/login"`).

| Route | What it does |
|---|---|
| `/admin/products` | **exists**; gains an "edit" link per row and a "new product" button. `Admin_ProductsReadOnly` is removed from both `.resx` files. |
| `/admin/products/create` | the product editor, empty. On save it redirects to the edit screen of the new product. |
| `/admin/products/edit/{id:int}` | the product editor, loaded. One form, four blocks: the product itself, the two translations, the price bands, the add-on links. |
| `/admin/units` | the fleet list: tag, product, status, serial, purchase date. Ordered by product then tag. Shows retired units too, marked. |
| `/admin/units/create` | one unit. |
| `/admin/units/edit/{id:int}` | one unit. |
| `/admin/audit` | the 100 most recent audit rows, newest first, read-only. |

The dashboard at `/admin` keeps its three counts and loses its single link to the product list —
navigation moves to the bar of D9/04.

### 5.1 The product editor, block by block

**The product.** Slug (with the D5/04 warning), category, seat configuration (only meaningful for
strollers), max rider weight, width, length, seat width, range, turnaround days, sort order, image
path, `IsActive`, `IsBookable`.

Absence is a first-class value on this screen and must survive the round trip: a dimension nobody
measured is an **empty field**, saved as `NULL`, and it comes back empty. It is never shown as `0`
and never saved as `0`. This is D15 and it is control C17 — see §11.2 item 2 for the form the code
must not use.

`IsBookable` is refused when the product has no valid price list: saving a product as bookable while
`PricingTierRules.Validate` returns anything but `None` fails validation with the reason named. That
is the same coherence `CatalogSeeder.cs:116-134` enforces on the seed, applied to the screen.

**The two translations.** Both visible at once, unconditionally (D3/04): name, tagline, description
(Markdown), highlights (one per line, D6/04). The English name is required (D11/04). The Portuguese
row may be absent entirely; saving with every Portuguese field blank deletes that translation row
rather than storing empties, and the list screen then shows the existing `Admin_MissingTranslation`
marker.

The description is stored as Markdown and rendered by `Application/RichText.cs` when the public page
reads it. **The editor does not preview it**, and nothing in this leva imports `Markdig` or `Ganss`:
`ArchitectureTests.cs:43` asserts the exact list of types that carry those packages, and control C11
of `foundation.tsv` asserts the exact file. A preview is worth its own front with its own control.

**The price bands.** A repeating row: min days, max days (empty means open-ended), mode, amount.
Validation calls `PricingTierRules.Validate` on the whole proposed set and refuses to save on
`Empty`, `InvalidBand`, `DoesNotStartAtOneDay`, `Overlap`, `Gap` or `NoOpenEndedBand`, naming which
one — the six members already exist in `Domain/PricingTierSetProblem.cs`. Each gets a resource key.

**The add-on links.** A checkbox per existing active add-on. Checking creates the `ProductAddOn`
row, unchecking removes it; the composite primary key makes a duplicate impossible. A product that
is not bookable carries no links, matching `CatalogSeeder.cs:133-134` and the assertion at
`DomainTests.cs:192`.

### 5.2 The unit screen

Asset tag (required, max 40, **unique across the fleet**), product, serial, status, notes, purchase
date. The unique index at `UnitConfiguration.cs:16` means a duplicate tag must be caught as a
validation message, not as a database exception reaching the user: the screen checks before saving
and still catches the update exception, because two operators can race.

`PurchasedOn` is a **calendar date in Orlando**, stored as `date` — not an instant. `CreatedAtUtc`
is an **instant**, written from `IClock` at creation and never touched again. `Unit` has no
`UpdatedAtUtc` column and this leva does not add one; the audit trail is what records the change.

Changing the product a unit belongs to is open point K1 of §10.

---

## 6. Validation, and what each refusal says

Every refusal renders in the existing `p.error-summary` shape with `role="alert"`, as
`Pages/Admin/Login.cshtml:14` does, and every message is a resource key present in both cultures.

| Rule | Where it comes from | Refusal |
|---|---|---|
| English name required | D11/04 | the product cannot be saved |
| Slug required, unique, max 80 | `ProductConfiguration.cs:15-16` | named as a duplicate, not as an exception |
| One translation row per culture | unique index `ProductTranslationConfiguration.cs:21` | impossible to reach through this screen; still caught |
| Price set valid | `PricingTierRules.Validate` | the specific problem is named |
| Bookable requires a valid price set | D8/04, `CatalogSeeder.cs:116-125` | the checkbox is refused, the rest of the form is kept |
| Not bookable carries no add-on link | `CatalogSeeder.cs:133-134` | the links are refused |
| Asset tag unique, max 40 | `UnitConfiguration.cs:15-16` | named as a duplicate |
| Amount > 0, min days ≥ 1, max ≥ min | check constraints `PricingTierConfiguration.cs:13-15` | caught before the database sees it |
| Every decimal field | D20, `CLAUDE.md:41-44` | binding is `en-US`; a comma-decimal input is a validation error, never a silent reinterpretation |

---

## 7. Resource keys

Every visible string is a key in **both** `SharedResource.resx` and `SharedResource.pt-BR.resx`, and
no value is blank in either — `LocalizationParityTests.cs:16` compares the key sets and `:32` refuses
an empty value. Control C12 of `foundation.tsv` measures the same thing from the files.

**Every new key keeps the `Admin_` prefix.** This is not a naming preference: `RenderedTextTests.cs`
takes the **names** of all 163 keys and asserts that none of them appears as a substring in the body
of any of 22 pages, most of them public. A key named after a CSS class, an id, a route segment or a
common word would turn a public page red in a leva that never touched it. The 23 keys that exist
today all carry the prefix.

`Admin_ProductsReadOnly` is **removed** from both files in this leva, because the screen stops being
read-only. Removal is symmetric or the parity test fails.

---

## 8. Tests

### 8.1 The harness — first delivery, and stop P2

Three pieces, all inside `tests/OrlandoUp.Tests/`, none of them in `src/`:

1. **An authentication handler for tests**, registered through `ConfigureTestServices` in
   `SiteFactory`, issuing a principal in role `Roles.Admin` (D1/04). It exposes a client factory so
   a test asks for an authenticated client explicitly; the default client stays anonymous, so
   `SiteBehaviourTests.cs:54` keeps proving the gate.
2. **An antiforgery helper** that GETs a page, extracts `__RequestVerificationToken` from the
   returned HTML and posts a form with it (D2/04).
3. **Seeded fleet data in the fixture.** The tests below read products, tiers, add-ons and units;
   `SiteFactory.SeedAsync()` already writes the whole catalog, so the fixture is sufficient — the
   agent confirms it at step 0 rather than assuming, because a test whose fixture lacks its row
   fails by exception, not by assertion.

**P2 is approved on three proofs, in the same report:** an authenticated GET of an admin page
returns 200; a POST carrying the token succeeds; the same POST **without** the token is rejected.
The third is what proves the second is measuring something.

### 8.2 What the leva proves

Every absence assertion states a presence in the same method — a "does not contain" alone passes on
a page that redirected, errored or came back empty.

1. **The store default is really gone.** Insert a product with `IsActive = false` through the
   `DbContext` and read it back as `false`. Under SQLite, `EnsureCreated` builds the schema from the
   model, so the defect and its repair both reproduce there. Same test for the add-on, the zone and
   the location. **This is the test that justifies the migration**, and without it the repair rots
   at the first refactor.
2. **The editor writes what it was given.** Post a change to a product's English name and read it
   back on the public `/rentals` page — one test that crosses the whole path, admin write to public
   read, and proves the no-cache finding of §3 rather than trusting it.
3. **An absent dimension survives the round trip.** Post the form with the width field empty; the
   column reads `NULL`, and the product page shows no transport badge (`FitsDisneyTransport` is
   `null`, three answers not two).
4. **A product cannot be saved as bookable with a broken price set** — one test per
   `PricingTierSetProblem` member that a screen can produce.
5. **The English name cannot be blanked** (D11/04).
6. **A duplicate asset tag is refused as validation**, and the presence half asserts that a
   different tag saves.
7. **Every write leaves exactly one audit row**, with the actor, the entity type from `nameof` and
   an action from the enum. One test per write handler.
8. **The hidden product stays visible in the administration** — the property
   `Admin/Products/Index.cshtml.cs:27-28` states and no test asserts today.
9. **Highlights round-trip as lines**, and a highlight containing a quote or a backslash comes back
   intact — the serializer is the only thing between a typo and a silently empty list.

### 8.3 What the leva must not break

Named here so the agent recognizes a red as its own doing: `SeoTests.cs:201` (the sitemap says
nothing about `/admin`), `SeoTests.cs:88` (the two lists of public pages agree — **do not add any
address to `SiteBehaviourTests.PublicPathList`**), `CultureRoutingTests.cs:64` (a Portuguese page
links nowhere outside the prefix — **do not link `/admin` from the public layout**),
`SiteBehaviourTests.cs:273` (nothing can send a message to anybody — this leva registers no sender),
`SeedingTests.cs:31` (nine hard cardinals of the seed — this leva does not touch `CatalogSeedData`),
`ArchitectureTests.cs:32` (the application layer knows nothing about infrastructure — **the write
service goes in `Infrastructure/Data/`, beside `CatalogQueries`, never in `Application/`**).

---

## 9. Visual check

**Before any item, in one block:** (1) rebuild — the schema changed and the pages are new;
(2) apply the migration first (§0 step 1), because every screen here reads a column whose default
just moved; (3) sign in as the seeded administrator, the only role that exists; (4) the database is
the LocalDB `OrlandoUpDb` with the real fleet already in it — seven products, ten units, eight
tiers, twelve add-on links; (5) **step 1 is the proof of a fresh build**: the navigation bar of
D9/04 is visible on `/admin`, which cannot be true of the old binary.

**Roteiro.** Each item says what is expected on screen.

1. `/admin` shows the navigation bar with four destinations, and the three counts unchanged.
2. `/admin/products` shows an edit link per row and a "new product" button, and no longer says the
   screen is read-only.
3. Open a scooter, change the Portuguese tagline, save. `/pt/rentals` shows it **without restarting
   anything**.
4. Clear the width of the wheelchair, save, reopen: the field is empty, not `0`. Its public page
   shows no transport badge.
5. Try to make a stroller bookable: refused, naming the price problem. Add a valid band set, try
   again: accepted, and `/rentals` starts showing a price for it. **Undo this** — the strollers are
   not bought (`CatalogSeedData.cs:24-25`).
6. Create a product, then hide it with `IsActive` off: it disappears from `/rentals` and 404s on its
   own address, and it is still listed in `/admin/products`.
7. `/admin/units`: mark a scooter as in maintenance, then create a unit with a tag that already
   exists — refused with a message, not a stack trace.
8. `/admin/audit` lists every gesture above, newest first, each with the operator's e-mail.
9. Keyboard only, on one editor screen: every field reachable by Tab, every focus ring visible, the
   error summary announced. D9 makes accessibility a requirement of the administration too.
10. The same editor at 375 px wide: the price band rows and the add-on checkboxes stay usable and
    nothing scrolls the page sideways.

**The result is written to `Docs/conferencia-leva-04.md`**, including whatever the check could not
reach and the measured reason.

---

## 10. Open points for Rod, answered at P0

The agent carries these into the plan and does not start without the answers.

**K1 — Can a unit be moved to another product?** (a) Yes, the product is an editable field of the
unit; (b) no, the product is chosen at creation and fixed afterwards, and a mistake is corrected by
retiring the unit and creating another. Cost of (a): a unit that moves takes its history with it and
the audit line has to say so. Cost of (b): a typo at creation costs a row.

**K2 — What does `/admin/audit` show?** (a) The 100 most recent rows, flat, no filter — the smallest
honest screen; (b) the same with a filter by entity type and by actor. (b) costs a form and buys
little while one person writes.

**K3 — When a product is saved as bookable with a broken price set, what happens to the rest of the
form?** (a) Nothing is saved and the whole form comes back with the message; (b) everything except
the bookable flag is saved, and the message says the flag was refused. (a) is safer and loses
typing; (b) never loses typing and can leave the operator believing the product is on sale.

---

## 11. Controls

### 11.1 Files the front alters

**New:**
`src/OrlandoUp.Web/Domain/AuditEntry.cs`;
`src/OrlandoUp.Web/Infrastructure/Data/Configurations/AuditEntryConfiguration.cs`;
one migration plus its designer under `src/OrlandoUp.Web/Infrastructure/Data/Migrations/`;
the write service and the audit service under `src/OrlandoUp.Web/Infrastructure/Data/`;
the page pairs (`.cshtml` + `.cshtml.cs`) for `Products/Create`, `Products/Edit`, `Units/Index`,
`Units/Create`, `Units/Edit`, `Audit/Index` under `src/OrlandoUp.Web/Pages/Admin/`;
`tests/OrlandoUp.Tests/AdminCrudTests.cs` and the harness files of §8.1;
`Docs/controles/admin-catalog.tsv`; `Docs/conferencia-leva-04.md`;
`Docs/relatorio-leva-04-etapa-N.md`.

**Modified, and only in this:**
`AppDbContext.cs` (one `DbSet`);
`ProductConfiguration.cs`, `AddOnConfiguration.cs`, `DeliveryZoneConfiguration.cs`,
`DeliveryLocationConfiguration.cs` (the store default leaves; nothing else);
`Domain/Enums.cs` (the `AuditAction` enum);
`AppDbContextModelSnapshot.cs` (tool-generated);
`Pages/Shared/_AdminLayout.cshtml` (the navigation bar);
`Pages/Admin/Index.cshtml` and `Pages/Admin/Products/Index.cshtml` (links, and the read-only note
goes);
`Resources/SharedResource.resx` and `SharedResource.pt-BR.resx` (new keys, one key removed);
`wwwroot/css/site.css` (form controls that do not exist yet: `select`, `textarea`, `checkbox`, the
repeating row);
`tests/OrlandoUp.Tests/SiteFactory.cs` (the test-only registrations of D1/04);
`tests/OrlandoUp.Tests/DomainTests.cs` (**only** the comment at `:44-46`, which this leva makes
false — no assertion changes);
`Docs/fila-cc.md` (the Estado and Commit columns of this leva's line).

**Negative, by diff column:** `Program.cs`, `CatalogSeedData.cs`, `CatalogSeeder.cs`,
`CatalogQueries.cs`, `Application/Catalog/CatalogViews.cs`, `TranslationPicker.cs`, `RichText.cs`,
`PricingTierRules.cs`, `Api/SitemapEndpoints.cs`, `Infrastructure/Localization/` (all four files),
`appsettings.json`, `CLAUDE.md`, `Docs/decisions.md`, `Docs/architecture.md`, `Docs/roadmap.md`,
`Docs/open-questions.md`, `Docs/market-notes.md`, `Docs/backlog-conhecido.md`,
`Docs/protocolo-conversa.md`, `Docs/regras-de-controle.md`, `Docs/medir-controles.sh`, the summaries
and the atrito files, `Docs/spec-01-foundation.md`, `Docs/spec-02-public-site.md`, the leva 01 and
leva 02 reports and conferences, `Docs/controles/foundation.tsv`, `Docs/controles/public-site.tsv`,
`.githooks/pre-commit`, `.gitattributes`, `.github/workflows/ci.yml`, and every test file except
the two named as modified above — each appears in **zero** lines of `git diff --stat` over the
leva's commit range.

**Both existing `.tsv` files are in the negative list on purpose.** If this leva moves one of their
controls, that is a **stop with a proposed amendment**, never a silent edit — and by the project's
rule, an amendment that changes scope forces every control already proposed to be re-measured at
HEAD before it counts.

### 11.2 Invariants for `Docs/controles/admin-catalog.tsv`

The exact command shape is the agent's — he read the code. The invariants:

1. **No configuration file declares a store default on a `bool` column.** Anchored on the form, over
   `Infrastructure/Data/Configurations/`, expected 0 after the leva. Its reach assertion is a
   sibling that counts the configuration files scanned and asserts the sweep arrives.
2. **Absence never becomes zero**, over the files this leva adds. This duplicates C17 of
   `foundation.tsv`, which is **permanent and already scans all of `src/`** — so this leva does
   **not** create a parallel control; it re-measures C17 at HEAD in step 0 and reports it at every
   stop. The three coalescing forms on the same target are what the rule requires, and the existing
   control already covers them.
3. **The catalog identifiers are still typed in one file only.** Control C16 of `public-site.tsv` is
   permanent in effect and scans `src/` excluding the file named `CatalogSeedData.cs`. A slug in a
   placeholder, in an example, **or in a comment** of a new admin page breaks it. Step 0 measures
   it; every stop reports it. Note the exclusion is by **base file name**, so a new seed-like file
   under another name would not be excluded.
4. **Every write handler under `Pages/Admin/` calls the audit service** — expressed as a
   **relation**, not a count: *number of `OnPost…Async` handler names under `Pages/Admin/` minus
   number of audit-service call sites in the same folder = 0*. Anchored on the handler **name**, never
   on the return type. It needs a sibling reach assertion, because zero minus zero is also zero: the
   sibling asserts the handler operand is at least the number of write screens this leva creates.
5. **The real clock is read in one file only** — C06 of `foundation.tsv`, permanent, whose expected
   value is a literal path. The audit timestamp must come from `IClock`. Re-measured at step 0.
6. **Markdig and Ganss still enter through one type only** — C11 of `foundation.tsv` and
   `ArchitectureTests.cs:43`, which are complementary: the control scans `using` lines in `*.cs` and
   would miss a fully qualified call or a call inside a `.cshtml`; the architecture test scans the
   compiled assembly and catches both. Neither alone is enough. This leva adds no preview, so both
   stay put.
7. **Culture branching in markup lives in the two layouts only** — C09 of `public-site.tsv`, whose
   expected value is a literal two-path list and which does **not** exclude `Pages/Admin/`. This is
   D3/04 expressed as a control that already exists; re-measured at step 0 and at every stop.
8. **No admin address in the typed public list** — the public path list of the test project stays at
   its current length, expressed as the relation between it and the framework-derived set that
   `SeoTests.cs:88` already compares.
9. **No resource key is dropped from one culture only** — C12 of `foundation.tsv`, permanent, which
   this leva moves in both directions (keys added, one removed). Re-measured.

**Every control rule of the project applies** — `Docs/regras-de-controle.md`, twelve rules, each
bought with a real defect. Two of them bite this leva specifically: rule 2 (a comment counts in a
control and can break another front's control) and rule 4 (an absolute count of a shared file is not
a control; the durable form is the relation).

### 11.3 What STEP 0 measures, before altering any file

1. `dotnet --list-sdks`, `dotnet ef --version`, `sqllocaldb info`, and `SELECT DB_NAME()` through
   the configured connection, reading `OrlandoUpDb`.
2. `bash Docs/medir-controles.sh verificar Docs/controles/foundation.tsv` and the same for
   `public-site.tsv` — **all 35 on target before anything is touched**. A control already red on
   arrival is a stop. This includes C14 and C15, which the reviewer could not measure.
3. `bash Docs/medir-controles.sh quem-ancora` over every file §11.1 lists as new or modified — the
   mode exists precisely because a spec once claimed no existing `.tsv` would move and knocked down
   eight controls. Whatever it names beyond the controls listed in §11.2 goes into the plan as a
   contradiction.
4. `bash Docs/medir-controles.sh proibidos` before writing a single comment in a new file.
5. A `grep` over `src/` and `tests/` for the identifiers this leva creates — `AuditEntry`,
   `AuditAction`, and the names chosen for the write service and the test handler — **expected
   zero**. Any hit is a stop, and the name changes before the type exists.
6. Row counts of `Products`, `AddOns`, `DeliveryZones` and `DeliveryLocations`, for the P1 report.
7. The proposal for `Docs/controles/admin-catalog.tsv` with `medir` run at the initial HEAD.
8. Confirmation that `SiteFactory.SeedAsync()` writes every row the tests of §8.2 read.
9. **Any contradiction between this spec and the tree is reported in the plan.** The spec is never
   obeyed against the measurement.

---

## 12. Out of scope, and why

**Delivery zones, delivery locations and add-ons themselves.** Their `IsActive` store default is
repaired here (D4/04) but no screen creates or edits them. Motive: the zone data is blocked on Q3
(areas, fees, cut-off) and the add-on prices on the same answers; a screen built now would be
rebuilt when Q3 closes.

**Company settings.** They live in `appsettings.json`, and the four `TODO-` markers there are what
control C16 of `foundation.tsv` counts, at a threshold of `>= 4` with a measured value of exactly 4
— zero margin. Moving them to the database is a schema change **plus** the retirement of that
control **plus** the closing of Q12, and it is its own front. When it happens, the control is retired
with it; the threshold is never lowered.

**`TurnaroundDays` and `SalesTaxRate`.** They carry store defaults with the same mechanism, but they
are not `bool`: for a numeric column the provider's inability to distinguish `0` from silence is a
real but different problem, and neither is written by a screen in this leva. Recorded as backlog with
the leva that will bite named.

**A separate write role.** D10/04. Recorded as backlog: it becomes real when a delivery employee gets
an account.

**Markdown preview in the editor.** §5.1. It would move `ArchitectureTests.cs:43` and control C11,
both of which assert an exact list; that is worth a front with its own control, not a corner of this
one.

**Deleting anything.** D12/04.

**Bookings and everything that reads them** — today's deliveries, the booking timeline, unit
assignment, the calendar, refunds. They are the other half of roadmap phase 4 and they cannot exist
before leva 03.

### 12.1 Statements that were re-checked and are TRUE — do not "fix" them

- **`/admin` is excluded from culture routing and from the sitemap by the `/Admin` prefix of
  `ViewEnginePath`**, in `CultureRouteConvention.cs:27` and `PublicPages.cs:68-71`. New admin pages
  inherit both exclusions with no registration. Nothing is to be added anywhere for them.
- **Every page under `Pages/Admin/` is protected by the folder convention at `Program.cs:104`.** No
  `[Authorize]` attribute is needed, and none exists in the repository.
- **Antiforgery needs no configuration.** A plain `<form method="post">` rendered by Razor gets the
  hidden field from the tag helper, and `AddRazorPages` validates it on every non-GET handler.
- **The public site holds no copy of the catalog.** No cache, no reload command, no invalidation
  step: `CatalogQueries` is scoped and queries per request. An edit in the administration is visible
  on the next request. Do not add a cache in this leva, and do not write a reload step that has
  nothing to reload.
- **`Products.IsBookable` has no store default, and that is correct** —
  `ProductConfiguration.cs:35` and the comment above it. It is the model this leva copies onto the
  four `IsActive` columns, not a defect to make consistent in the other direction.
- **`Admin/Products/Index.cshtml.cs` deliberately does not filter `IsActive`.** The administration
  sees hidden products; that is the point of a soft hide.

---

## 13. Closing

**Two commits.** The content commit first — code, tests, migration, resources, CSS, the new `.tsv`,
`Docs/conferencia-leva-04.md`. Then the closing commit, which writes the hash of the first into the
Commit column of this leva's line in `Docs/fila-cc.md`. A single-commit front cannot record its own
hash.

**Queue balance:** on receipt this file has **1** `aguardando` line; at the end, **0**, and this line
reads `concluido` with the content commit's short hash. The total number of rows does not change.

**End of session:** the whole `git status --short` and `git diff --stat`, plus
`bash Docs/medir-controles.sh verificar` on all three `Docs/controles/*.tsv`, reported and never
silenced.

**Push is the operator's**, never the agent's.
