# Spec — Leva 03: booking core — availability that counts batteries, a frozen quote, and bookings entered by staff (no payment yet)

**Date:** 2026-09-12, conversation 6. **Pair:** conversation 6 ↔ leva 03. **Executor:** Claude Code,
strongest model — the leva creates the four tables every later front reads, the availability rule
that decides what the site may promise, and one migration.

**Language of this spec:** English (`Docs/decisions.md` D1). The control rules it relies on are in
`Docs/regras-de-controle.md`; the skeleton follows `Docs/spec-04b-fleet-batteries.md`.

**What this leva closes:** the system knows the fleet — ten machines, twelve batteries, fourteen
chargers — and **cannot promise any of it to anyone**. Measured: `Domain/` holds no booking type
(`grep -rIlw Booking src tests` returns only `SharedResource.resx`, where the word sits inside the
text *"Booking opens soon"*); `Pages/Rentals/Details.cshtml:180` renders a **disabled** button that
says exactly that. Every reservation Ronatrip takes today lives in WhatsApp, and the site cannot
tell a visitor whether a Drive Scout is free on the 22nd — nor could staff, without counting by hand
that four scooters and six batteries per model mean **the battery runs out first** (D36).

**What it deliberately leaves to leva 03b:** the money. Stripe Checkout, the public booking form
that follows the quote, the confirmation e-mail with its manage link, the 30-minute hold and its
sweeper, and the refund — all of them wait on the Stripe account (Q6) and on the liability text
(Q15), and **none of them changes the availability math or the schema this leva writes**, which is
the reason the split is safe (D39).

---

> ## EMENDA-03-01 — 2026-09-13 — review of the P0 plan (`scratchpad/leva03/plano.md`). Wins over the body where they disagree.
>
> **Verdict: execute after the corrections below.** The plan is reviewable: files named, migration
> declared, out-of-scope stated, every §11.3 measurement present with the HEAD value. Re-measured by
> the reviewer before writing this note: `admin-catalog` C05 operands **9 − 7 = 2**; `CatalogSeeder.cs:31`
> `if (await db.Products.AnyAsync(...)) return 0` — the seeder never updates an existing row;
> `SeoTests.cs:197` is an **equality** on alternates, not a floor; `ClockTests.cs:50` already uses
> `new SystemClock(() => instant)`; `ArrangeSettingsRowAsync` (`AdminCrudTests.cs:881,909,1014`) and
> `RunBatterySeedAsync` exist; `origin/main...main` reads `0 0` (the operator pushed) and
> `git ls-files` reads **212** — §0's 210 was counted at `6da3ff6`, before this spec's own commit; both
> numbers are right for their commit and neither is a stop.
>
> **Corrections, numbered:**
>
> 1. **The default constraint is dropped in the same `Up`, not "if it stayed".** §4.5 says the P1
>    after-read decides. D35 already measured what happens: `AddColumn(defaultValue: 18)` leaves a
>    permanent unnamed `DF__…` constraint that no later migration removes. So the `Up` adds the column
>    with `defaultValue: 18` (it is what fills the single existing row) **and immediately runs a
>    `migrationBuilder.Sql` block that looks the constraint up in `sys.default_constraints` by table
>    and column and drops it by name** (dynamic SQL — the name is generated). P1 classifies that block;
>    the after-read of `sys.default_constraints` for `OperationalSettings.NextDayCutoffHour` expects
>    **zero rows**, and a row is a stop. *If this passes unnoticed:* the model says "no store default"
>    and the database keeps one forever — the exact D35 defect, on the column D34 exists to protect.
> 2. **K1 accepted, plus a type barrier.** `Booking` gets a static factory in `Domain/Booking.cs`
>    (`Booking.CreateByStaff(validated request, quote, actorEmail, nowUtc, acknowledgeOverbooking)`
>    — the agent names the parameters) that sets `Status = BookingStatus.Confirmed` and
>    `Source = BookingSource.Staff`; `BookingWriter` calls it and never assigns `Status`. **And
>    `Booking.Status` is declared `{ get; private set; }`** — EF Core writes private setters, and a
>    page that tried `booking.Status = x` would not compile. Control C03 of the new `.tsv` stays as the
>    belt; the private setter is the barrier (architecture, not discipline). C04 (≥ 2 assignments in
>    `Domain/`) is satisfied by the factory and by `Cancel`. `Docs/architecture.md` §2 already says
>    *"writes go through services so that booking invariants live in one place"* — the factory is
>    that place.
> 3. **K2: the architecture note covers all SIX divergences, not two.** §11.1 authorises one dated
>    note in `Docs/architecture.md`; its scope is widened to: the booking number (D6/03), the rounding
>    (D7/03), `BookingEvent.Summary` instead of `Data` JSON (§4.4), the windows as an enum instead of
>    `TimeOnly` pairs (§4.6), one `Phone` column instead of `Phone` + `WhatsApp` + `Country` (§4.1),
>    and the rules in `Domain/` with loaders in `Infrastructure/Data/` instead of
>    `IAvailabilityService`/`IBookingService` in `Application/` (§5–§6). One note, dated 2026-09-13,
>    placed after the §3 table, each point in one sentence with the spec section. The file's own
>    header says the code is measured and the file corrected; leaving four of six unwritten would
>    leave two versions circulating.
> 4. **§6 accepted: `FakeClock` delegates to `new SystemClock(() => _utcNow)`.** Deviation by
>    improvement over §9.6, and it supersedes both options written there: one zone table, C06 of
>    `foundation.tsv` unmoved (the fake never reads `DateTime.UtcNow`, and the control scans `src`
>    only — confirmed by reading the command). `IClock.NowInOrlando()` is added as §5.4 says.
> 5. **§5.1 (a) and (b) accepted.** C01's operand is
>    `PricingTier|[.]PricingTiers|CatalogQueries|[Zz]one[.]DeliveryFee|db[.]AddOns` and C02's floor
>    is **≥ 3** — the prose of §11.2 item 1 is superseded; `AddOn.Amount` and a bare `DeliveryFee`
>    would have matched the snapshot columns the detail page must print (the regra-8 false positive,
>    caught by the agent before it existed). C07 carries the `${s:-nenhum}` default so an empty
>    `paste` never writes `ERRO` as the expected value.
> 6. **K3 noted: 107 tests in 11 files** (four harness files carry no attribute). §3's "13 files" is
>    wrong and the 107 is right; no consequence.
> 7. **K4 accepted:** the settings row and the batteries are arranged **per test** through the
>    existing helpers, never in `InitializeAsync`; `The_dashboard_shows_the_chargers_as_not_set_when_the_settings_row_is_missing`
>    (`AdminCrudTests.cs:989`) keeps passing untouched (§12.1).
> 8. **K5 noted for P3:** `SeoTests.cs:197` asserts `addresses.Count × 3 == alternates`; `/book`
>    enters with both cultures and its `x-default`, and the P3 report prints the two numbers.
>
> **Confirmed, and not to be "fixed":** the C05 relation of `admin-catalog.tsv` stays at 2 by
> design (two new handlers, two new `.Record(` through `BookingTimeline`); `fleet-batteries.tsv`
> C06 is rebased 5 → 6 with the label untouched, and the plan's §4.2 sentence about the class/member
> gap goes into the P1 report verbatim; `public-site.tsv` C10 stays 0 because every `asp-page=` the
> leva writes outside `Admin/` carries `asp-route-culture=` on the same line.
>
> **Backlog, not this leva (written to `Docs/backlog-conhecido.md` by the reviewer):** a unit or
> battery moved to *Em manutenção* / *Baixada* after bookings exist can silently put a future date
> above the fleet, and no screen of this leva warns — the phase-4 half (assignment, calendar) is
> where that reading belongs.
>
> **Proof of reading:** `grep -c "EMENDA-03-01" Docs/relatorio-leva-03-etapa-1.md` — expected **≥ 1**.
> The P1 report also restates corrections 1 and 2 in its own words.

---

## 0. Execution surface

**Launcher phrase:** this spec is executed by the line of `Docs/fila-cc.md` dated `2026-09-12` whose
description starts with *"LEVA 03 — BOOKING CORE"*. **Not "the `aguardando` line"** — that line, by
name.

**Tree state at receipt, measured 2026-09-12:** HEAD is a descendant of `6da3ff6` (the closing
commit of conversation 5); the commit that adds this spec, its conference roteiro, the queue line
and D39–D42 comes after it and is the expected HEAD. `git status --porcelain` **empty**.
`git ls-files` counts **210**, **143** under `src/` and **16** under `tests/`.
`origin/main...main` reads `0 13` — thirteen commits not yet pushed, and the push is the
operator's. Four control files, **54 controls**: `admin-catalog.tsv` 12 on target,
`public-site.tsv` 17 on target, `fleet-batteries.tsv` 7 on target, `foundation.tsv` 18 with **2
off** — C14 and C15, which shell out to `dotnet` and answer `127` to the reviewer for want of it.
The agent measures those two at step 0; a control genuinely red on arrival is a stop.

A Windows CRLF warning and a stale git index are not divergence. Any other modified or untracked
file outside `scratchpad/` is a stop with a report.

**Files the front ALTERS:** the closed list is §11.1. A file altered outside it is a stop with a
report, **without a cardinal**.

**Files the front PRODUCES as record:** `scratchpad/leva03/plano.md` (never committed);
`Docs/relatorio-leva-03-etapa-N.md`, one per stop, committed before approval is asked;
`Docs/controles/booking-core.tsv`; and the results written into **`Docs/conferencia-leva-03.md`,
which already exists in the tree as an empty-result roteiro** — see §10. The agent never writes a
result into it.

**Files the TOOL generates coupled:** `dotnet ef migrations add AddBookings` rewrites
`AppDbContextModelSnapshot.cs` and writes `<timestamp>_AddBookings.Designer.cs`.

**Steps that need a human hand.** The operator's shell is **PowerShell on Windows**.

1. **Apply the migration, after P1 is approved:**

   ```
   dotnet ef database update --project src/OrlandoUp.Web
   ```

2. **Strip the BOM before `git add`** — `CLAUDE.md:82-83`; the hook refuses `efbbbf`.
3. **Set the turnaround on the three bookable products, through the screen leva 04 built** — the
   copy problem of D40: the seed file changes to `1`, but the site reads the `Products` rows of
   `OrlandoUpDb`, which the seed wrote as `0` on 2026-09-05 and which no migration rewrites. In
   `/admin/produtos`, open *Drive Scout 4*, *Drive Spitfire EX* and *Cadeira de rodas Drive*, put
   **1** in the field the screen calls **"Dias de intervalo"** and save. Three rows, no code. **Until
   this is done the availability of the running site is one day too generous**, and item 0b of the
   roteiro measures it.
4. **The visual check of §10 is the operator's**, filling the roteiro that already exists.

**Four mandatory stops.** In each one the report is committed **before** approval is asked.

| Stop | When | What it carries |
|---|---|---|
| **P0** | after step 0, before altering any file | the plan, with every measurement of §11.3 and every contradiction between this spec and the tree |
| **P1** | migration written and **not applied** | the script classified by `revisao-migration-efcore`; row counts of `Products`, `Units`, `Batteries`, `OperationalSettings` measured before; `sys.default_constraints` read after `database update` (D35 lesson) — the `OperationalSettings` column added here must carry **no** store default |
| **P2** | domain, rules, loaders and their tests written; **no screen yet** | the availability and quote tests of §9.1–9.3 green; the four scenes of §5.4 reproduced as tests, with the numbers |
| **P3** | everything written, suite green | the full run, **five** `.tsv` verified, the visual check pending |

P2 exists because the availability rule is the decision the whole leva carries, and a wrong rule
under a finished screen is the most expensive shape of defect this project knows: it looks done.

---

## 1. What the leva delivers, in plain words

A visitor opens a scooter page and, instead of *"Booking opens soon"*, finds **"Check availability
and price"**. He picks the equipment, the delivery day, the pickup day, how many, whether he wants a
second battery, the extras, and the hotel he is staying at. The site answers with one of two
sentences — **available for these dates**, or **sold out for these dates** — and, when available,
the price broken down the way he will pay it: rental, second battery, extras, delivery, tax, total.
It also tells him something no other rental site in Orlando tells him: if the scooters are free but
the batteries are not, it says *the second battery is not available for these dates* instead of
selling one that does not exist.

He cannot pay yet. The page says so, and tells him to write. That is leva 03b.

Rod, meanwhile, gets in `/admin` what WhatsApp has been giving him on paper: **Reservas**. He enters
the customer, the dates, the delivery and pickup windows, the place, the equipment with its second
batteries and the amount he charged for each, the extras, and a note about how it was paid. The
system checks the same availability the visitor sees, refuses what the fleet does not have — and
lets him **enter it anyway**, marked, when he has decided to solve it by hand (a Spitfire instead of
a Scout, a purchase). Every booking has a number, a history, and a cancel button that gives the
equipment back to the pool.

**What this is not.** It is not payment, e-mail, or a customer's *"manage my booking"* page (leva
03b). It is not today's delivery list, unit and battery assignment, or the calendar (the booking
half of phase 4, after this). It is not a coupon, a waiver, or a tax decision — Q4 and Q5 keep
their assumptions and the zone keeps its rate. And it does not touch the WhatsApp channel: the
staff form is where a WhatsApp reservation **lands**, not where it arrives.

---

## 2. Decisions of this leva

Four were asked of the operator on 2026-09-12, each with a concrete scene, before any block of this
spec existed; they are also D39–D42 of `Docs/decisions.md`. The rest are the assistant's, each with
the reason, so that Rod knows in one line where to disagree.

**D1/03 — Phase 3 is split: leva 03 is the booking core without payment; leva 03b is Stripe, the
public booking form, the e-mail and the hold.** **[operator]**, choosing between this, the whole
phase in one leva, and the aesthetics round. Reason: nothing in this leva waits on Q6 (Stripe
account) or Q15 (battery liability text); everything in 03b does. The precedent is 04/04b, and it
is the same shape — the schema and the rule come first, the surface that depends on an external
account comes second. The leva numbers follow the roadmap phases (D33), so both are phase 3.

**D2/03 — Turnaround is ONE day, for the machine and for its batteries.** **[operator]**, on the
scene *SCT-02 comes back Tuesday 20:00 at Pop Century; Wednesday 09:00 another customer wants a Scout
at Art of Animation* — answer: **no, it needs a day off** for cleaning, charge and check. The seed
carries `TurnaroundDays = 0` for every product (`CatalogSeeder.cs:80`); it moves to **1** for the
three bookable products and stays `0` for the four strollers, about which no fact exists. The
battery pool of a model uses **the model's `TurnaroundDays`** — one number, because the answer was
given for the scene of both pieces together and a battery is turned around on the machine's
schedule. The extension to the wheelchair is the assistant's: the scene was a scooter, the answer
named cleaning and check, which the wheelchair also needs; Rod edits the number on the screen if he
disagrees. **Consequence recorded as human step 3 of §0:** the running database is a copy the seed
does not refresh.

**D3/03 — Fixed delivery and pickup windows, and a next-day cut-off at 18:00 Orlando time.**
**[operator]**, confirming the assumption Q3 has carried since 2026-09-04: windows **8–10, 10–12,
14–16, 18–20**, and a customer who books after 18:00 gets the day after tomorrow as the earliest
delivery day. The windows are an enum with explicit numbers (four rows are not a table, and a
change is a one-line adjustment under CLAUDE.md's *reversible change* rule); the cut-off hour is a
column of `OperationalSettings`, editable on the settings screen, because it is the kind of number
that moves in December. The cut-off binds **the public page only**: staff enter what they have
decided to deliver, so the staff form refuses only the past (D9/03). Q3 stays open for what this
does not answer — the zones, their fees and the meet-and-greet routine.

**D4/03 — Availability is a BARRIER on the public site and a WARNING for staff.** **[operator]**, on
the scene *four Scouts booked 20–24 December, a fifth customer asks for 22–23* — answer: the site
says sold out, **the staff may enter it above the fleet, marked**. So the public quote never
produces a booking the fleet cannot serve; the staff form computes the same availability, refuses
by default, and accepts when the operator ticks the box that says he will solve it by hand. The
booking then carries `IsOverbooked = true`, its first event says so, and the list shows the badge.
Deliberate over-selling is a business decision (`modelo-de-reserva` skill, step 2), and this is it,
written down.

**D5/03 — Four tables: `Bookings`, `BookingLines`, `BookingAddOns`, `BookingEvents`. No `Payment`,
no `Coupon`, no `BookingUnit`, no `ManageToken`, no waiver fields.** **[assistant]**. Each of the
absent ones belongs to the front that first reads it (03b for payment, token and waiver; the phase-4
half for unit assignment), and a column nobody reads is a column nobody tests. Adding them later is
one additive migration each. What **is** here now, and would be expensive to add later, is the
**status enum with every member the architecture names, numbered explicitly**, and the rule of
which statuses hold inventory — so that 03b adds `PendingPayment` bookings without touching the
availability math (§5.1).

**D6/03 — The booking number is `OU-` followed by the row id padded to six digits, assigned in the
same transaction as the insert.** **[assistant]**, departing from `Docs/architecture.md` §3, which
says `OU-YYYY-NNNNNN` sequential. A year-scoped sequence needs a database sequence, which the SQLite
test host does not have; the identity id is already sequential and never reused, and the year is
in `CreatedAtUtc`. The number is stored (unique index) rather than derived, because it is what a
customer reads aloud on the phone and what a search box looks for. The architecture file gets the
correction (§11.1).

**D7/03 — Every amount on a booking is a snapshot, and the quote is the only thing that computes
one.** **[assistant]**, applying the CLAUDE.md rule *bookings and prices are snapshotted*. A
booking line stores the tier it was priced by (min, max, mode, amount), the unit price, the line
total, the second-battery amount per day and its total; an add-on line stores name, mode, amount,
quantity, total; the booking stores subtotal, add-ons, second batteries, delivery fee, **tax rate**
and tax, total. **No screen of this leva recomputes a stored booking from the catalog** — the
detail page prints columns. Rounding is `MidpointRounding.AwayFromZero` at two decimals, once, on the
tax line only (every other amount is an integer multiple of a two-decimal price): that is what
`PricingTier.DailyEquivalent()` already does at `Domain/PricingTier.cs:37`, and the architecture's
*ToEven* sentence is corrected to match the measured code (§11.1).

**D8/03 — Batteries and chargers are counted on the booking as NUMBERS; which physical battery goes
out is the assignment step of the next front.** **[assistant]**, reading D36 and D37 together. D36
says the administrator records *which* battery ids go out — at **delivery**, which is where units
are assigned too (architecture: *per-unit assignment happens later and never changes availability
math*). What availability needs today is the count: a scooter line of quantity *q* with
`ExtraBatteryCount = e` (0 ≤ e ≤ q, at most one second battery per scooter, D36) draws **q + e**
batteries from the model's pool and **q + e** chargers from the single charger count. The second
battery is priced **per line** by an amount typed on the booking, defaulted from
`OperationalSettings.SecondBatteryPerDay` (D37: a courtesy is zero and still consumes a battery).

**D9/03 — The staff form ignores the cut-off, not the calendar.** **[assistant]**. Staff enter what
they have decided to deliver, including tomorrow at 22:00 the night before; what they may not enter
is a delivery day earlier than today in Orlando, because that booking would hold inventory in the
past and never be delivered. The public page applies the cut-off of D3/03.

**D10/03 — One product per public quote; several products per staff booking.** **[assistant]**.
The public page has no JavaScript (D38 kept the tree at zero) and its form must re-render honestly
after every change; one product, chosen first, is the shape a page with no script can validate. The
staff form is a table with one row per bookable product, so a family renting a scooter and a
wheelchair is one booking with two lines — which is what the schema models. Add-ons attach to the
**line**, not to the booking, and in this leva an add-on's quantity is the line's quantity; the
column exists so a later screen can vary it.

**D11/03 — The place of delivery is a location when the zone has a list, and the zone itself with a
typed address when it does not.** **[assistant]**, reading the seed: three zones carry curated
locations, `vacation-homes` carries none (`CatalogSeedData.cs:245-253`). The select offers every
active location grouped by zone, and every active zone that has no location as its own option; a
zone option requires an address. The booking stores `DeliveryZoneId` always and
`DeliveryLocationId` when there is one — the fee and the tax rate come from the zone either way.

**D12/03 — An absent `OperationalSettings` row is a loud failure, never a zero.** **[assistant]**,
the lesson of EMENDA-04B-04. The row is created by the migration and no screen deletes it (control
C03 of `fleet-batteries.tsv`); a host without it is a deployment defect. The availability loader
throws `InvalidOperationException` naming the table, the public page turns that into the existing
fail-closed sentence (`Admin_ErrorSettingsMissing` has a public twin, §8), and the test fixture
**arranges the row** for every test that quotes — the same arrangement `AdminCrudTests.cs:895`
already makes, because `EnsureCreatedAsync` never runs the migration that creates it.

**D13/03 — The test host gets a fake clock.** **[assistant]**. Availability and the cut-off are
functions of *today in Orlando*, and a test that reads the machine clock passes on Tuesday and
fails on Saturday. `tests/OrlandoUp.Tests/FakeClock.cs` implements `IClock`; `SiteFactory`
replaces the registration in `ConfigureTestServices` and exposes the instance. It is frozen at the
real instant of the factory's construction unless a test sets it, so no existing test moves.
`SystemClock` stays the only reader of the machine clock (`foundation.tsv` C06 does not move: the
fake lives under `tests/`).

**D14/03 — The lengths a quote accepts are 1 to 60 days.** **[assistant]**. The open-ended tier
prices any length, so the cap is not about price; it bounds the day loop of the availability rule
and refuses the typo that asks for 2026-09-20 to 2027-09-20. A rental longer than that is a
conversation, and the page says so.

---

## 3. The measured terrain

Everything `[V]` on 2026-09-12 at HEAD `6da3ff6`, by the command in the last column, run in the
folder's shell. One row per file; a plural subject is not a measurement.

| File | Fact | Command |
|---|---|---|
| `src/OrlandoUp.Web/Domain/Product.cs:32` | `public int TurnaroundDays { get; set; }` — *"Buffer between two rentals of the same unit; availability reads it."* Nothing reads it today. | `grep -rn TurnaroundDays src --include=*.cs` |
| `src/OrlandoUp.Web/Infrastructure/Seeding/CatalogSeeder.cs:80` | `TurnaroundDays = 0,` — the seeder writes zero for every product; `SeedProduct` carries no turnaround field | same command |
| `src/OrlandoUp.Web/Pages/Admin/Products/Edit.cshtml.cs:83,187,347` | the edit page binds and writes `TurnaroundDays`; the screen's label is the key `Admin_FieldTurnaround`, pt-BR **"Dias de intervalo"** | same command; `grep -A1 'name="Admin_FieldTurnaround"' src/OrlandoUp.Web/Resources/SharedResource.pt-BR.resx` |
| `src/OrlandoUp.Web/Domain/Enums.cs` | 9 enums, every member numbered; `UnitStatus { Available = 1, Maintenance = 2, Retired = 3 }`; `ZoneKind`, `HandoverMode`, `AddOnPricingMode { PerRental = 1, PerDay = 2 }`, `TierMode { FlatPerRental = 1, PerDay = 2 }` | `cat` |
| `src/OrlandoUp.Web/Domain/Battery.cs:41` | `public UnitStatus Status` — a retired battery is the broken one; the pool of a model is `Batteries.Where(Status == Available)` | `cat` |
| `src/OrlandoUp.Web/Domain/OperationalSettings.cs` | three values: `ChargerCount` (int), `SecondBatteryPerDay` (decimal), `LostChargerFee` (decimal); `SingletonId = 1`; *"The row is created by the migration"* | `cat` |
| `src/OrlandoUp.Web/Domain/PricingTierRules.cs` | `Validate(IEnumerable<PricingTier>)` returns `PricingTierSetProblem`; `None` means *starts at one day, no overlap, reaches the open end* | `cat` |
| `src/OrlandoUp.Web/Domain/PricingTier.cs:37,44` | `DailyEquivalent()` rounds `AwayFromZero`; `Covers(int days)` exists | `cat` |
| `src/OrlandoUp.Web/Domain/DeliveryZone.cs` | `DeliveryFee`, `SalesTaxRate` (*"four decimal places; open question Q4 keeps it at zero"*), `HandoverMode`, `IsActive`, `Locations` | `cat` |
| `src/OrlandoUp.Web/Domain/DeliveryLocation.cs` | `ZoneId`, `Name` (*"not translated"*), `Address`, `Notes`, `IsActive` | `cat` |
| `src/OrlandoUp.Web/Domain/AuditEntry.cs` | the pattern a booking event copies: actor as text, `nameof` entity type, one English sentence, no navigation | `cat` |
| `src/OrlandoUp.Web/Infrastructure/Data/AppDbContext.cs:24-48` | 13 `DbSet`s; the last two are `Batteries` and `OperationalSettings` | `grep -n 'DbSet<'` |
| `src/OrlandoUp.Web/Infrastructure/Data/AuditTrail.cs:35` | `public void Record(string? actorEmail, string entityType, int entityId, AuditAction action, string summary)` — the method name `Record` is what C05 of `admin-catalog.tsv` counts | `grep -n 'public '` |
| `src/OrlandoUp.Web/Infrastructure/Data/CatalogQueries.cs` | `ActiveCardsAsync`, `ActiveDetailAsync`, `ActiveZonesAsync`, `ActiveLocationsByZoneAsync` — the read pattern the public pages use; registered scoped in `Program.cs:118` | `grep -n 'public '` |
| `src/OrlandoUp.Web/Infrastructure/Seeding/CatalogSeedData.cs:205-253` | four zones: `disney-resorts` (fee 0, MeetAndGreet, 6 locations), `universal-resorts` (0, MeetAndGreet, 2), `idrive-lbv-hotels` (0, FrontDesk, 2), `vacation-homes` (**25**, Doorstep, **no location**) | `sed -n 205,253p` |
| `src/OrlandoUp.Web/Infrastructure/Seeding/CatalogSeedData.cs:69-127` | three bookable products, tiers `1–2 flat 75 / 3–6 per day 32 / 7+ per day 27` for both scooters, `1–2 flat 40 / 3+ per day 12` for the wheelchair; four add-ons per bookable product | `sed -n 60,130p` |
| `src/OrlandoUp.Web/Infrastructure/Seeding/CatalogSeedData.cs:167-202` | six add-ons: `cup-holder` 5 PerRental, `cane-holder` 5 PerRental, `sunshade` **3 PerDay**, `rear-basket` 8 PerRental, `rain-cover` 5 PerRental, `damage-waiver` 20 PerRental | `sed -n 165,203p` |
| `src/OrlandoUp.Web/Program.cs:113-118` | `AddSingleton<IClock, SystemClock>`; scoped `CatalogQueries`, `CatalogWriter`, `AuditTrail`, `PublicPages`; **no service of this leva exists** | `sed -n 110,120p` |
| `src/OrlandoUp.Web/Program.cs:96-101` | `AuthorizeFolder("/Admin", …)`; a new folder under `Pages/Admin/` inherits the policy | `cat` |
| `src/OrlandoUp.Web/Infrastructure/Localization/PublicPages.cs:31-42,59-63` | the sitemap derives public pages from the registered Razor pages, excluding `/Admin`, `/Error`, `/Shared` and any page whose template has a parameter; **a new `Pages/Book/Index.cshtml` enters the sitemap by existing** | `cat` |
| `src/OrlandoUp.Web/Pages/Rentals/Details.cshtml:177-186` | `@if (product.IsBookable)` renders `<button … disabled>@L["Product_BookingSoon"]</button>` and `@L["Product_BookingSoonNote"]`; the `else` branch renders `Product_ComingSoonNote` (strollers) | `sed -n 170,189p` |
| `src/OrlandoUp.Web/Pages/Shared/_AdminLayout.cshtml:63-68` | **six** `<a asp-page=…>` in `nav.admin-nav`: Index, Products, Units, Batteries, Settings, Audit | `grep -n asp-page` |
| `src/OrlandoUp.Web/Pages/Shared/_Layout.cshtml:84-88,130-134` | the public nav has five links in two lists (header and footer), each with `asp-route-culture` — C10 of `public-site.tsv` is a relation over these; **this leva adds no nav link** | `grep -n asp-page` |
| `src/OrlandoUp.Web/Pages/Admin/Index.cshtml.cs:43-58` | `public int? ChargerCount` projected `(int?)row.ChargerCount` with `FirstOrDefaultAsync` — the D12/03 pattern | `sed -n 40,60p` |
| `src/OrlandoUp.Web/Pages/Admin/Settings/Index.cshtml.cs:37-44` | three `[BindProperty]`: `ChargerCount`, `SecondBatteryPerDay`, `LostChargerFee`; the page reads keys `Admin_FieldChargerCount`, `Admin_FieldSecondBattery`, `Admin_FieldLostCharger`, `Admin_ErrorSettingsMissing`, `Admin_Save`, `Admin_Saved` | `grep -n BindProperty`; `grep -o 'L\["Admin_[A-Za-z]*'` |
| `src/OrlandoUp.Web/Resources/SharedResource.resx` | **293** `<data name=` entries; pt-BR also 293 | `grep -c '<data name='` |
| `src/OrlandoUp.Web/Resources/SharedResource.pt-BR.resx` | `Admin_StatusAvailable` **"Disponível"**, `Admin_StatusMaintenance` **"Em manutenção"**, `Admin_StatusRetired` **"Baixada"**, `Admin_NotSet` **"Não definido"**, `Admin_UnitsTitle` **"Frota"**, `Admin_ProductsTitle` **"Produtos"**, `Admin_SettingsTitle` **"Configurações"**, `Admin_FieldChargerCount` **"Carregadores em estoque"**, `Admin_Save` **"Salvar"** | `grep -A1 'name="<key>"'` |
| `tests/OrlandoUp.Tests/SiteFactory.cs:63,111-125` | SQLite in memory; `EnsureCreatedAsync()` — *"The application itself never does this"*; `SeedAsync()` runs `CatalogSeeder` only; **no `OperationalSettings` row, no batteries**; `IClock` is not replaced | `cat` |
| `tests/OrlandoUp.Tests/SiteFactory.cs:96-107` | `CreateStaffClient()` adds the `TestAuthHandler` header — the credential every admin test uses | `cat` |
| `tests/OrlandoUp.Tests/AdminCrudTests.cs:895-901` | a test **arranges** the settings row by hand before reading it; `:999` another asserts the row is absent on a fresh host | `grep -n OperationalSettings` |
| `tests/OrlandoUp.Tests/AdminCrudTests.cs:635-647` | the interpolated-key test builds `Admin_Status{…}`, `Admin_Seat{…}`, `Admin_Tier{…}`, `Admin_Action{…}`, `Admin_BatteryKind{…}` and asserts **`Assert.Equal(15, keys.Count)`** — moves in this leva (§9.5) | `sed -n 630,648p` |
| `tests/OrlandoUp.Tests/AdminCrudTests.cs:943-960` | `No_battery_column_declares_a_store_default` walks `Battery` + `OperationalSettings` columns and asserts **`Assert.Equal(15, columns.Count)`** — moves to **16** when the cut-off column lands (§9.5) | `sed -n 940,962p` |
| `tests/OrlandoUp.Tests/SiteBehaviourTests.cs` | `PublicPathList` has **21** typed addresses; C09/C10 of `admin-catalog.tsv` read this block | `awk '/PublicPathList *=/{f=1} f{print} f&&/\];/{exit}'` |
| `tests/OrlandoUp.Tests/RenderedTextTests.cs:28-37+` | `No_page_prints_a_resource_key` is a `[Theory]` over `[InlineData]` paths; the new page's two addresses join it | `grep -n InlineData` |
| `tests/OrlandoUp.Tests/SeoTests.cs:55-66,155-164` | the sitemap test discovers `names.Count >= 8` public pages and `addresses.Count >= 20`; both floors survive one page more | `grep -n Count` |
| `tests/OrlandoUp.Tests/FormPoster.cs`, `FormFields.cs` | `ReadFormAsync`, `PostAsync`, `PostWithTokenAsync`, `FormFields.Set/Add/Remove` — the way every admin POST is exercised with antiforgery **on** | `grep -n 'public '` |
| `tests/OrlandoUp.Tests/*.cs` | **107** `[Fact]`/`[Theory]` attributes across 13 files (`AdminCrudTests` 40, `DomainTests` 16, `SiteBehaviourTests` 15, `SeoTests` 10, …) | `grep -c '\[Fact\]\|\[Theory\]'` |
| `Docs/controles/admin-catalog.tsv` C05/C06 | `OnPost[A-Za-z]*Async` occurrences under `Pages/Admin` minus `[.]Record[(]` = **2**; the reach floor is **9** | `awk -F'\t'` on the file |
| `Docs/controles/fleet-batteries.tsv` C06 | `public bool Is[A-Za-z]+` in `Domain/` = **5**; `IsOverbooked` makes it **6** (§11.2) | same |
| `Docs/controles/public-site.tsv` C16 | eleven catalog identifiers are forbidden in `src/` outside `CatalogSeedData.cs` — the new pages resolve products and zones by **query**, never by literal | same |
| `src/` and `tests/` | the word `Booking` occurs in **1** file (`SharedResource.resx`, inside a value); `Availability`, `Quote`, `DeliveryWindow`, `BookingLine`, `BookingEvent`, `BookingAddOn` occur **0** times | `grep -rIlw`, `grep -rIwE … \| wc -l` |

**`[H]` — inherited, not re-checked, and therefore pending, not fact:** the suite passes and the
solution builds (C14/C15, relayed by the agent at `bfaeac2`); the `Products` rows of `OrlandoUpDb`
carry `TurnaroundDays = 0` (the seed wrote it, nobody has edited it — human step 3 of §0 makes it
true either way); the `OperationalSettings` row of `OrlandoUpDb` reads `ChargerCount = 14`.

**Guards that already watch the corpus this leva edits.** `LocalizationParityTests` (3) fails on a
key present in one `.resx` and absent in the other; `RenderedTextTests` (1 theory) fails on any
listed page that prints a raw key; `AdminCrudTests.cs:635-647` fails on an interpolated key nobody
wrote; `SeoTests` fails if a public page lacks its alternates; `ArchitectureTests` (3) forbids
`Domain` from referencing any other layer — **the availability and quote rules go in `Domain/` and
take plain lists, never a `DbContext`**. None of these only reports; all four fail the run.

---

## 4. The schema change

One migration, **`AddBookings`**, additive: four tables, one column, no data statement. Every money
column is `decimal(10,2)` (D15) except the tax rate, `decimal(5,4)`, which mirrors
`DeliveryZone.SalesTaxRate`. No boolean carries a store default (D34; `admin-catalog.tsv` C01/C03
stay at 0). Every string column has an explicit `HasMaxLength`. Calendar dates are `DateOnly`,
instants are `DateTime` named `…Utc` (D16).

### 4.1 `Bookings`

| Column | Type | Null | Why |
|---|---|---|---|
| `Id` | int identity | no | |
| `Number` | nvarchar(16), unique | no | `OU-000123` (D6/03); written in the same transaction as the insert |
| `Status` | int (`BookingStatus`) | no | see §5.1 |
| `Source` | int (`BookingSource`) | no | `Online = 1` (03b), `Staff = 2` |
| `Culture` | nvarchar(5) | no | the customer's language, `en-US` or `pt-BR`; e-mails of 03b read it (architecture §4) |
| `FirstName`, `LastName` | nvarchar(100) | no | |
| `Email` | nvarchar(256) | no | |
| `Phone` | nvarchar(40) | no | WhatsApp is a phone number; one field |
| `DeliveryZoneId` | FK → `DeliveryZones`, restrict | no | fee and tax rate come from the zone (D11/03) |
| `DeliveryLocationId` | FK → `DeliveryLocations`, restrict | **yes** | absent when the zone has no list |
| `Address` | nvarchar(300) | yes | required by validation when `DeliveryLocationId` is null |
| `DeliveryNotes` | nvarchar(500) | yes | room, gate, "meet at the bus loop" |
| `StartDate`, `EndDate` | date (`DateOnly`) | no | delivery day, pickup day, Orlando calendar |
| `DeliveryWindow`, `PickupWindow` | int (`DeliveryWindow`) | no | D3/03 |
| `Days` | int | no | `EndDate − StartDate + 1`, stored because every total was computed from it |
| `Subtotal`, `ExtraBatteriesTotal`, `AddOnsTotal`, `DeliveryFee`, `Tax`, `Total` | decimal(10,2) | no | snapshots (D7/03) |
| `TaxRate` | decimal(5,4) | no | the zone's rate on the day, so a later rate change never rewrites this booking |
| `IsOverbooked` | bit, **no store default**, C# initialiser absent (type default `false`) | no | D4/03 |
| `StaffNotes` | nvarchar(1000) | yes | how it was paid, anything else |
| `CreatedAtUtc` | datetime2 | no | instant, from `IClock` |
| `CreatedByEmail` | nvarchar(256) | yes | the staff account; null when a customer creates it (03b) |
| `CancelledAtUtc` | datetime2 | yes | |
| `CancelReason` | nvarchar(500) | yes | required by validation on cancel |

Indexes: unique on `Number`; non-unique on `(StartDate, EndDate)` — the availability loader filters
on the overlap with a padded window and on `Status`; non-unique on `Status`.

### 4.2 `BookingLines`

| Column | Type | Null | Why |
|---|---|---|---|
| `Id` | int identity | no | |
| `BookingId` | FK → `Bookings`, cascade | no | a line never outlives its booking |
| `ProductId` | FK → `Products`, restrict | no | the product row is never deleted (soft hide), so the link survives |
| `ProductName` | nvarchar(200) | no | the name in the booking's culture on the day — the editor of leva 04 can rename a product tomorrow |
| `Quantity` | int | no | ≥ 1 |
| `ExtraBatteryCount` | int | no | 0 ≤ e ≤ `Quantity`; always 0 for a non-scooter (validated, §6.3) |
| `TierMinDays` | int | no | the band applied |
| `TierMaxDays` | int | **yes** | null = the open-ended band; the one legitimate null of the row |
| `TierMode` | int (`TierMode`) | no | |
| `TierAmount` | decimal(10,2) | no | |
| `UnitPrice` | decimal(10,2) | no | the rental of ONE unit for `Days` (flat: `TierAmount`; per day: `TierAmount × Days`) |
| `LineTotal` | decimal(10,2) | no | `UnitPrice × Quantity` |
| `ExtraBatteryPerDay` | decimal(10,2) | no | typed or defaulted; zero is a courtesy (D37) |
| `ExtraBatteriesTotal` | decimal(10,2) | no | `ExtraBatteryCount × ExtraBatteryPerDay × Days` |

### 4.3 `BookingAddOns`

| Column | Type | Null | Why |
|---|---|---|---|
| `Id` | int identity | no | |
| `BookingLineId` | FK → `BookingLines`, cascade | no | D10/03: an add-on belongs to a line |
| `AddOnId` | FK → `AddOns`, restrict | no | |
| `AddOnName` | nvarchar(200) | no | snapshot in the booking's culture |
| `PricingMode` | int (`AddOnPricingMode`) | no | snapshot |
| `Amount` | decimal(10,2) | no | snapshot |
| `Quantity` | int | no | = the line's quantity in this leva |
| `Total` | decimal(10,2) | no | PerRental: `Amount × Quantity`; PerDay: `Amount × Quantity × Days` |

### 4.4 `BookingEvents`

Mirrors `AuditEntry` (`Domain/AuditEntry.cs`): no navigation, actor copied as text.

| Column | Type | Null | Why |
|---|---|---|---|
| `Id` | int identity | no | |
| `BookingId` | FK → `Bookings`, cascade | no | |
| `OccurredAtUtc` | datetime2 | no | instant, from `IClock` |
| `ActorEmail` | nvarchar(256) | yes | null = the customer or the system (03b) |
| `Type` | int (`BookingEventType`) | no | `Created = 1`, `Cancelled = 2`, `Note = 3` |
| `Summary` | nvarchar(500) | no | one English sentence, never a serialized diff |

Index on `(BookingId, OccurredAtUtc)`.

### 4.5 `OperationalSettings` gains one column

`NextDayCutoffHour` — int, not null, **no store default in the model** (D34). The migration adds it
with `defaultValue: 18` **only as the value written into the single existing row**, and P1 reads
`sys.default_constraints` afterwards to confirm no permanent constraint stayed behind — if one did,
the migration gets an explicit `DROP CONSTRAINT` in the same `Up` (the D35 rule, applied before it
bites this time). Valid range 0–23, enforced by the screen (§7.4). `AdminCrudTests.cs:960` moves
from 15 to **16**.

### 4.6 Enums, all with explicit numbers, in `Domain/Enums.cs`

```
BookingStatus     Draft = 1, PendingPayment = 2, Confirmed = 3, Scheduled = 4, OutForDelivery = 5,
                  Active = 6, PickedUp = 7, Completed = 8, Expired = 20, Cancelled = 21, Refunded = 22
BookingSource     Online = 1, Staff = 2
DeliveryWindow    Morning = 1 (08–10), LateMorning = 2 (10–12), Afternoon = 3 (14–16), Evening = 4 (18–20)
BookingEventType  Created = 1, Cancelled = 2, Note = 3
```

The hours of a window live in one static table, `Domain/DeliveryWindows.cs`
(`(TimeOnly Start, TimeOnly End) Hours(DeliveryWindow)`), never in a page.

### 4.7 The seed

`SeedProduct` gains `int TurnaroundDays`; the three bookable products carry **1**, the four strollers
**0**; `CatalogSeeder.cs:80` reads it instead of writing the literal. `SeedingTests` cardinals do
not move (7 / 14 / 6 / 4 / 10 / 10 / 3 / 4 are counts of rows, not of turnaround). The seeder
**updates** `TurnaroundDays` on re-run only if the seeder already updates other product columns on
re-run — the agent measures `CatalogSeeder.cs` at step 0 and reports which it is; if the seeder
preserves existing rows, the human step 3 of §0 is the only way the running database learns the
number, and the plan says so in those words.

### 4.8 What the `Down` does

Drops the four tables and the column. Nothing else, because nothing else changed.

---

## 5. The rules, in `Domain/`, with no database in sight

### 5.1 The status machine — `Domain/BookingStatusRules.cs`

Two pure functions and one table:

- `HoldsInventory(BookingStatus)` is **true** for `PendingPayment`, `Confirmed`, `Scheduled`,
  `OutForDelivery`, `Active`; **false** for `Draft`, `PickedUp`, `Completed`, `Expired`,
  `Cancelled`, `Refunded`. This is the set the availability loader filters on, and the reason
  `PendingPayment` is in it today is D5/03: 03b's holds must not touch this file.
- `CanTransition(from, to)` — the legal edges of the architecture's machine. **This leva uses two
  of them**: `Confirmed → Cancelled` (staff cancel) and nothing else, because a staff booking is
  born `Confirmed`. The full table is written and tested now so that no later front redraws it.
- `Booking.Cancel(DateTime nowUtc, string reason)` and the constructor path are the only places
  that assign `Status`; a page never writes `Status =` (control C02 of §11.2).

### 5.2 Availability — `Domain/Availability.cs`

Pure, over plain inputs. The loader (§6.1) fetches; this computes.

```
Inputs
  request:      product P, [start, end], quantity q, extraBatteries e
  fleet:        unitsAvailable(P)     = Units.Count(Status == Available)
                batteriesAvailable(P) = Batteries.Count(Status == Available)   (scooters only)
                chargerCount          = OperationalSettings.ChargerCount
                turnaround(P)         = P.TurnaroundDays
  holding lines: every BookingLine whose Booking.Status HoldsInventory, with
                (product, start, end, quantity, extraBatteryCount, turnaround(product))

For each existing line L: padded(L) = [L.start − t(L.product), L.end + t(L.product)]
For each day d in [start, end]:
  unitsBusy(d)     = Σ L.quantity                         over lines of P with d ∈ padded(L)
  batteriesBusy(d) = Σ (L.quantity + L.extraBatteryCount) over lines of P with d ∈ padded(L)
  chargersBusy(d)  = Σ (L.quantity + L.extraBatteryCount) over ALL scooter lines with d ∈ padded(L)

unitsFree     = unitsAvailable(P)     − max_d unitsBusy(d)
batteriesFree = batteriesAvailable(P) − max_d batteriesBusy(d)      (scooters)
chargersFree  = chargerCount          − max_d chargersBusy(d)       (scooters)

Result: Available when q ≤ unitsFree and, for a scooter, q + e ≤ batteriesFree and q + e ≤ chargersFree.
        It also returns maxQuantity = min(unitsFree, batteriesFree, chargersFree) for scooters
        (unitsFree otherwise) and maxExtraBatteries = min(batteriesFree, chargersFree) − q, both
        floored at 0 — a staff booking above the fleet (D4/03) makes the raw numbers negative —
        so the page can say "only 2 left" and "the second battery is not available".
```

Padding is applied to the **existing** lines and compared with the **raw** request; that is
symmetric (a request ending the day before an existing start is caught by the existing line's
left padding) and it is the only place the turnaround is read. A product with **zero** batteries and
category `MobilityScooter` is unavailable at any quantity — a scooter model whose pool is empty
cannot go out (D36), and the rule says so rather than ignoring batteries when there are none.

### 5.3 The quote — `Domain/Quote.cs`

`Days = EndDate − StartDate + 1`, 1 ≤ Days ≤ 60 (D14/03). For each line: the product's tiers must
validate to `PricingTierSetProblem.None`, else the quote **fails closed** with the problem named —
never a zero price (D15); the band is the single tier that `Covers(Days)`; `UnitPrice` per §4.2;
`ExtraBatteriesTotal = e × perDay × Days`. Add-ons per §4.3. `Subtotal = Σ LineTotal`;
`ExtraBatteriesTotal`, `AddOnsTotal` are sums; `DeliveryFee = zone.DeliveryFee`;
`TaxRate = zone.SalesTaxRate`; `Tax = Round((Subtotal + ExtraBatteriesTotal + AddOnsTotal +
DeliveryFee) × TaxRate, 2, AwayFromZero)`; `Total` is the sum of the six. The quote returns the
full breakdown; the booking stores it verbatim.

### 5.4 The earliest delivery day — `Domain/BookingRules.cs`

`EarliestPublicStart(DateTime nowInOrlando, int cutoffHour)`: if `nowInOrlando.Hour < cutoffHour`
→ `today + 1`, else `today + 2`, where `today` is the date part of the same value. The Orlando wall
time comes from `IClock`: no member exposes the hour today (`Application/IClock.cs` has `UtcNow`
and `TodayInOrlando()`), so the leva adds `DateTime NowInOrlando()` to the interface and
`SystemClock` implements it from the zone it already resolves — same file, so `foundation.tsv` C06
does not move. Staff: `EarliestStaffStart = today` (D9/03).

**Four scenes the P2 report reproduces as tests, with these numbers** (seed fleet: 4 + 4 scooters,
6 + 6 batteries, 14 chargers, turnaround 1):

1. *Four Scouts booked 20–24 Dec, each with a second battery.* Batteries busy = 8 > 6 → the fourth
   booking is refused at the battery, not at the scooter: with three Scouts + three seconds (6
   batteries) the fourth Scout **without** a second is available on units (3 < 4) and refused on
   batteries (6 + 1 > 6). *The battery runs out first* — D36's arithmetic, as a test.
2. *Turnaround.* Scout booked 10–12 Dec (one line, q = 1). Request 13 Dec: padded existing = 9–13
   → unitsBusy(13) = 1 → 3 free of 4; request q = 4 on the 13th is refused, q = 3 accepted; on the
   14th q = 4 accepted.
3. *Cancel releases.* The same booking cancelled → status `Cancelled` does not hold → q = 4 on the
   13th accepted.
4. *Cut-off.* Fake clock at 2026-12-05 **22:59 UTC** = 17:59 Orlando (EST, UTC−5) → earliest
   6 Dec; at 23:00 UTC = 18:00 Orlando → earliest 7 Dec. The DST twin: 2026-07-05 21:59 UTC = 17:59
   EDT → 6 Jul; 22:00 UTC → 7 Jul.

---

## 6. The services, in `Infrastructure/Data/`

Same shape as `CatalogQueries` / `CatalogWriter` / `AuditTrail`: `sealed class`, scoped, reads
`AppDbContext`, registered in `Program.cs` beside them.

### 6.1 `AvailabilityQueries`

Loads, for a product and a date range, everything §5.2 needs, **into memory**, and calls the
domain function. It pulls the holding lines whose booking overlaps `[start − 60, end + 60]` — the
maximum padding is bounded by D14/03 and by the largest `TurnaroundDays`, which the loader reads
from the products it selects, not from a constant. Sums and maxima happen in C#: **SQLite, the test
provider, cannot translate an aggregate over `decimal`, and the loader never asks it to** — the
quantities summed here are `int`, and money is never aggregated in SQL anywhere in this leva. It
throws `InvalidOperationException` naming `OperationalSettings` when the row is absent (D12/03).

### 6.2 `QuoteBuilder`

Given the validated request (product(s), dates, quantities, extra batteries and their per-day
amount, add-on ids, zone), loads the tiers, add-ons and zone, resolves the names in the booking's
culture through the same fallback `CatalogQueries` uses (`TranslationPicker`), and calls
`Domain/Quote`. Returns the breakdown, never persists.

### 6.3 `BookingWriter`

`CreateByStaffAsync(request, actorEmail, acknowledgeOverbooking)`: validates (§7.3), runs
`AvailabilityQueries` per line, refuses with the list of shortfalls unless acknowledged, runs
`QuoteBuilder`, builds the `Booking` (`Status = Confirmed`, `Source = Staff`), saves, assigns
`Number = $"OU-{Id:D6}"`, saves again inside the same transaction, records the `Created` event
through `BookingTimeline.Record` with a sentence that names the overbooking when it happened.
`CancelAsync(bookingId, actorEmail, reason)`: `Booking.Cancel(...)`, event `Cancelled`.

### 6.4 `BookingTimeline`

`Record(string? actorEmail, int bookingId, BookingEventType type, string summary)` — the sibling
of `AuditTrail.Record`, and named `Record` **on purpose**: control C05 of `admin-catalog.tsv` counts
`OnPost…Async` handlers minus `.Record(` calls, and a booking's timeline **is** its audit. The plan
says this in those words so the remeasured control is not read as a coincidence. Booking pages
write **only** `BookingEvents`, never `AuditEntries` — one record per write, in the table whose
screen shows it.

---

## 7. The screens

All text through `IStringLocalizer<SharedResource>`; every key of §8 exists in both files. No
JavaScript. Money through `MoneyFormat`. Dates formatted by the pinned `en-US` culture (D20).

### 7.1 Public — `Pages/Book/Index.cshtml` at `/book` and `/pt/book`

GET, with the form on the page and the query string as its state (`?product=drive-scout-4` from the
product page preselects). Fields: equipment (select: `IsActive && IsBookable` products, name in the
UI culture), delivery day, pickup day (`type="date"`), how many (1–10), second battery
(0–10; help text quotes the default per-day amount from the settings row), extras (checkboxes of
**the selected product's** add-ons, rendered after the first submit or from the query
preselection), where you are staying (select per D11/03: `L{id}` for locations grouped by zone
`<optgroup>`, `Z{id}` for zones with no location). Button **"Check availability and price"**.

On a valid request the page shows: the day count sentence (*"5 days, from Dec 20, 2026 to
Dec 24, 2026"*), then **one** of: *Available for these dates* + the breakdown table; *Sold out for
these dates*; *Only N left for these dates* (when 0 < maxQuantity < q); and, for a scooter whose
units are free but batteries are not, *The second battery is not available for these dates* with
the breakdown of the rental **without** it. When the tax rate is zero the breakdown prints *Taxes
included* on the tax line instead of `$0.00` (Q4's assumption, visible). The page always ends with
the paragraph that says payment opens in the next release.

Validation, each with its key: start before the earliest day (the message names the earliest day);
end before start; more than 60 days; second battery on a non-scooter; second batteries above the
quantity; a zone option that needs no address (none: the public page takes no address; a zone
option **without** a list is accepted as the zone alone, and the address is asked in 03b — recorded
in §12).

The page is public, unauthenticated, in the sitemap by construction (`PublicPages.cs`), and gets
its two addresses in `PublicPathList` and in `RenderedTextTests`.

### 7.2 Product page — `Pages/Rentals/Details.cshtml:177-183`

The disabled button and its note go; in their place, for a bookable product, a link styled as the
primary button to `/Book/Index` with `asp-route-culture` **and** `asp-route-product="@product.Slug"`,
labelled `Product_CheckAvailability`. The `else` branch (strollers) is untouched. Keys
`Product_BookingSoon` and `Product_BookingSoonNote` are **removed from both `.resx`** (control C09
of §11.2).

### 7.3 Admin — `Pages/Admin/Bookings/{Index,Create,Details}.cshtml`

**Index** (`/admin/bookings`): newest first, at most 100; columns Número, Cliente, Datas, Entrega
(location or zone name), Situação (`Admin_BookingStatus{Status}`), Total, and the badge **"Acima da
frota"** when `IsOverbooked`. Empty state sentence. Button **"Nova reserva"**.

**Create** (`/admin/bookings/create`): the fields of §4.1 the staff can know — first name, last
name, e-mail, phone, customer's language (select en-US / pt-BR, default the admin's current UI
culture), delivery day, pickup day, delivery window, pickup window (selects of the four windows,
labelled `Admin_Window{Name}`), place (the same select as the public page), address, delivery
notes; then a table with **one row per bookable product**: name, quantity (0–10, default 0),
second batteries (0–10, default 0, disabled-looking but still posted for non-scooters — the
server refuses a non-zero value there), second battery per day (decimal, default from the settings
row), extras (checkboxes of that product's add-ons); internal notes; the checkbox **"Lançar acima
da frota (resolvo à mão)"**; button **"Criar reserva"**.

Validation: at least one row with quantity ≥ 1; the past (D9/03); end ≥ start; ≤ 60 days; address
when the place is a zone; a non-scooter row with second batteries; second batteries above the
quantity; a negative per-day amount. Availability: when any line is short and the box is unticked,
the page re-renders with **"Não disponível: {0}. Marque a caixa acima para lançar assim mesmo."**,
`{0}` being the shortfalls, one per product, in the words *Drive Scout 4: 3 de 4 disponíveis; 1
segunda bateria a menos* (the agent composes it from `maxQuantity`/`maxExtraBatteries`). Ticked →
saved with `IsOverbooked = true`. On success: redirect to Details with the flash **"Reserva {0}
criada"**.

**Details** (`/admin/bookings/{id:int}`): every stored column of the booking, its lines and add-ons
**as stored** (no catalog read — D7/03), the totals, the badge, the source
(`Admin_BookingSource{Source}`), and **Histórico**: the events, newest last. Below, when
`CanTransition(Status, Cancelled)`: the form **"Cancelar esta reserva"** with the required reason and
the button **"Cancelar reserva"**; otherwise the sentence that it cannot be cancelled from its
status. Cancel is a POST handler with antiforgery, writes through `BookingWriter.CancelAsync`, and
returns to the same page with **"Reserva cancelada"**.

### 7.4 Settings — `Pages/Admin/Settings/Index.cshtml`

One more field: **"Corte para o dia seguinte (hora, horário de Orlando)"**, integer 0–23, with its
help sentence. Same handler, same audit line (the existing `Record` call), one more property bound.

### 7.5 Dashboard and navigation

`Pages/Admin/Index.cshtml`: one more count, **"Reservas ativas"** — bookings whose status
`HoldsInventory`. A `Count` over a filtered set is an honest zero on an empty table (the row-vs-count
distinction of `Index.cshtml.cs:37-43` is respected: this is a count). `_AdminLayout.cshtml`: a
seventh link, **"Reservas"**, between Frota/Baterias and Configurações — **seven** destinations is
the build-fresh proof of the roteiro.

---

## 8. Resource keys

Both files, same keys (parity test). Text in the table is the value; the agent may improve
punctuation, never meaning. Keys built by interpolation are listed by pattern and counted in
`AdminCrudTests.cs:635-647`, which moves from **15** to **32** (11 statuses + 4 windows + 2
sources; §9.5). `{0}`-style placeholders are formatted with `string.Format`, money through
`MoneyFormat`.

| Key | en-US | pt-BR |
|---|---|---|
| `Product_CheckAvailability` | Check availability and price | Ver disponibilidade e preço |
| `Book_Title` | Check availability and price | Disponibilidade e preço |
| `Book_Lead` | Choose the equipment, your dates and where you are staying to see whether it is available and what it costs. | Escolha o equipamento, as datas e onde você vai ficar para ver se está disponível e quanto custa. |
| `Book_FieldProduct` | Equipment | Equipamento |
| `Book_FieldStartDate` | Delivery day | Dia da entrega |
| `Book_FieldEndDate` | Pickup day | Dia da retirada |
| `Book_FieldQuantity` | How many | Quantos |
| `Book_FieldExtraBatteries` | Second battery (scooters only) | Segunda bateria (só scooters) |
| `Book_FieldExtraBatteriesHelp` | Every scooter comes with one battery and its charger. A second one costs {0} per day and doubles the range. | Toda scooter já vem com uma bateria e o carregador dela. A segunda custa {0} por dia e dobra a autonomia. |
| `Book_FieldAddOns` | Extras | Extras |
| `Book_FieldPlace` | Where you are staying | Onde você vai ficar |
| `Book_Submit` | Check availability and price | Ver disponibilidade e preço |
| `Book_DaysCount` | {0} days, from {1} to {2} | {0} dias, de {1} a {2} |
| `Book_Available` | Available for these dates | Disponível nessas datas |
| `Book_SoldOut` | Sold out for these dates | Esgotado nessas datas |
| `Book_OnlyLeft` | Only {0} left for these dates | Restam só {0} nessas datas |
| `Book_ExtraBatteryUnavailable` | The second battery is not available for these dates — the price below is without it. | A segunda bateria não está disponível nessas datas — o preço abaixo é sem ela. |
| `Book_EarliestStart` | The earliest delivery day is {0}. | O primeiro dia de entrega possível é {0}. |
| `Book_ErrorEndBeforeStart` | The pickup day must be the delivery day or later. | O dia da retirada tem de ser o dia da entrega ou depois. |
| `Book_ErrorTooLong` | Rentals longer than {0} days: write to us. | Aluguéis de mais de {0} dias: fale com a gente. |
| `Book_ErrorExtraOnlyScooters` | Only a scooter takes a second battery. | Só scooter leva segunda bateria. |
| `Book_ErrorExtraAboveQuantity` | At most one second battery per scooter. | No máximo uma segunda bateria por scooter. |
| `Book_ErrorPlaceRequired` | Tell us where you are staying. | Diga onde você vai ficar. |
| `Book_PriceTitle` | Your price | Seu preço |
| `Book_LineRental` | Rental | Aluguel |
| `Book_LineExtraBatteries` | Second battery | Segunda bateria |
| `Book_LineAddOns` | Extras | Extras |
| `Book_LineDelivery` | Delivery | Entrega |
| `Book_LineTax` | Tax | Impostos |
| `Book_TaxIncluded` | Taxes included | Impostos incluídos |
| `Book_LineTotal` | Total | Total |
| `Book_PaymentSoon` | Payment and confirmation open in the next release. Until then, write to us and we will hold your dates. | Pagamento e confirmação chegam na próxima entrega. Até lá, fale com a gente e seguramos as suas datas. |
| `Book_ErrorUnavailableNow` | We cannot check availability right now. Please write to us. | Não conseguimos conferir a disponibilidade agora. Fale com a gente. |
| `Admin_BookingsTitle` | Bookings | Reservas |
| `Admin_BookingsLead` | Every reservation, whoever entered it. | Todas as reservas, seja quem for que lançou. |
| `Admin_BookingsNone` | No booking yet. | Nenhuma reserva ainda. |
| `Admin_BookingNew` | New booking | Nova reserva |
| `Admin_BookingCreateTitle` | New booking (entered by staff) | Nova reserva (lançada pela equipe) |
| `Admin_BookingCreateLead` | For a reservation that arrived by WhatsApp, phone or e-mail and was paid outside the site. | Para reserva que chegou por WhatsApp, telefone ou e-mail e foi paga fora do site. |
| `Admin_ColNumber` | Number | Número |
| `Admin_ColCustomer` | Customer | Cliente |
| `Admin_ColDates` | Dates | Datas |
| `Admin_ColPlace` | Delivery | Entrega |
| `Admin_ColStatus` | Status | Situação |
| `Admin_ColTotal` | Total | Total |
| `Admin_ColQuantity` | Quantity | Quantidade |
| `Admin_ColExtraBatteries` | Second batteries | Segundas baterias |
| `Admin_ColExtraBatteryPerDay` | Second battery per day (US$) | Segunda bateria por dia (US$) |
| `Admin_ColAddOns` | Extras | Extras |
| `Admin_BookingStatus{Draft…Refunded}` | Draft, Pending payment, Confirmed, Scheduled, Out for delivery, Active, Picked up, Completed, Expired, Cancelled, Refunded | Rascunho, Aguardando pagamento, Confirmada, Programada, Saiu para entrega, Em uso, Recolhida, Concluída, Expirada, Cancelada, Reembolsada |
| `Admin_BookingSource{Online,Staff}` | Online, Entered by staff | Online, Lançada pela equipe |
| `Admin_Window{Morning,LateMorning,Afternoon,Evening}` | 8–10 am, 10 am–12 pm, 2–4 pm, 6–8 pm | 8h–10h, 10h–12h, 14h–16h, 18h–20h |
| `Admin_FieldFirstName` | First name | Nome |
| `Admin_FieldLastName` | Last name | Sobrenome |
| `Admin_FieldEmail` | E-mail | E-mail |
| `Admin_FieldPhone` | Phone / WhatsApp | Telefone / WhatsApp |
| `Admin_FieldCustomerLanguage` | Customer's language | Idioma do cliente |
| `Admin_FieldStartDate` | Delivery day | Dia da entrega |
| `Admin_FieldEndDate` | Pickup day | Dia da retirada |
| `Admin_FieldDeliveryWindow` | Delivery window | Janela de entrega |
| `Admin_FieldPickupWindow` | Pickup window | Janela de retirada |
| `Admin_FieldPlace` | Delivery place | Local de entrega |
| `Admin_FieldAddress` | Address (when the place is not on the list) | Endereço (quando o local não está na lista) |
| `Admin_FieldDeliveryNotes` | Delivery notes (room, gate, instructions) | Observações de entrega (quarto, portão, instruções) |
| `Admin_BookingLinesTitle` | Equipment | Equipamentos |
| `Admin_FieldStaffNotes` | Internal notes (how it was paid, anything else) | Observações internas (como foi pago, o que mais for) |
| `Admin_FieldOverbook` | Enter above the fleet (I will solve it by hand) | Lançar acima da frota (resolvo à mão) |
| `Admin_FieldOverbookHelp` | Unticked, the system refuses what the fleet cannot serve on these dates. Ticked, it saves the booking and marks it. | Desmarcada, o sistema recusa o que a frota não tem nessas datas. Marcada, salva a reserva e marca. |
| `Admin_BookingCreate` | Create booking | Criar reserva |
| `Admin_BookingCreated` | Booking {0} created. | Reserva {0} criada. |
| `Admin_ErrorNoLine` | Pick at least one piece of equipment. | Escolha pelo menos um equipamento. |
| `Admin_ErrorNotAvailable` | Not available: {0}. Tick the box above to enter it anyway. | Não disponível: {0}. Marque a caixa acima para lançar assim mesmo. |
| `Admin_ErrorAddressRequired` | This place needs an address. | Este local precisa de endereço. |
| `Admin_ErrorStartInPast` | The delivery day has already passed. | O dia da entrega já passou. |
| `Admin_ErrorEndBeforeStart` | The pickup day must be the delivery day or later. | O dia da retirada tem de ser o dia da entrega ou depois. |
| `Admin_ErrorTooLong` | At most {0} days. | No máximo {0} dias. |
| `Admin_ErrorExtraOnlyScooters` | Only a scooter takes second batteries. | Só scooter leva segunda bateria. |
| `Admin_ErrorExtraAboveQuantity` | Second batteries cannot exceed the quantity. | Segundas baterias não podem passar da quantidade. |
| `Admin_ErrorNegativeAmount` | An amount cannot be negative. | Valor não pode ser negativo. |
| `Admin_BookingDetailTitle` | Booking {0} | Reserva {0} |
| `Admin_BookingOverbooked` | Above the fleet | Acima da frota |
| `Admin_BookingDays` | {0} days | {0} dias |
| `Admin_BookingEventsTitle` | History | Histórico |
| `Admin_BookingCancelTitle` | Cancel this booking | Cancelar esta reserva |
| `Admin_FieldCancelReason` | Reason | Motivo |
| `Admin_BookingCancel` | Cancel booking | Cancelar reserva |
| `Admin_BookingCancelled` | Booking cancelled. | Reserva cancelada. |
| `Admin_ErrorCannotCancel` | This booking cannot be cancelled from its current status. | Esta reserva não pode ser cancelada na situação atual. |
| `Admin_DashboardActiveBookings` | Active bookings | Reservas ativas |
| `Admin_FieldCutoffHour` | Next-day cut-off (hour, Orlando time) | Corte para o dia seguinte (hora, horário de Orlando) |
| `Admin_FieldCutoffHourHelp` | Until this hour a customer may book delivery for tomorrow; after it, the earliest is the day after tomorrow. | Até esta hora o cliente pode reservar entrega para amanhã; depois dela, o primeiro dia possível é depois de amanhã. |
| `Admin_ErrorCutoffRange` | The hour must be between 0 and 23. | A hora tem de estar entre 0 e 23. |

Removed from both files: `Product_BookingSoon`, `Product_BookingSoonNote`.

The public form is `<form method="get" asp-page="/Book/Index" asp-route-culture="@CultureLink.Current">`
— `asp-page=` and `asp-route-culture=` **on the same line**, because C10 of `public-site.tsv` is a
relation counted line by line, and so is every link the new page emits.

---

## 9. Tests

The ones that prove the invariants, not the ones that exercise the code. Every test of absence
asserts a presence first, in the same method. Effects: none in this leva (no e-mail, no Stripe) —
the neutralization step of the test section is therefore *"assert that no `IEmailSender` and no
Stripe type is registered"* against `SiteFactory.RegisteredServiceNames`, which is the presence
half of 03b's future neutralization.

### 9.1 `DomainTests` — pure, no host

- `HoldsInventory` is true for exactly the five statuses of §5.1 (assert the set, both ways).
- `CanTransition` — the table: every legal edge true, and at least these illegal ones false:
  `Cancelled → Confirmed`, `Completed → Active`, `Confirmed → Confirmed`.
- `Booking.Cancel` sets `Status`, `CancelledAtUtc` (the instant passed in), `CancelReason`; refuses
  from `Cancelled` with `InvalidOperationException`.
- Availability, the four scenes of §5.4 with their numbers; plus: a unit in `Maintenance` does not
  count; a battery `Retired` does not count; a scooter model with zero available batteries is
  unavailable at `q = 1`; a wheelchair ignores batteries and chargers; the charger bound bites when
  the two models together exceed 14 (e.g. 4 Scouts + 4 Spitfires each with a second = 16 > 14,
  refused at the charger while each pool alone, 8 > 6, is also refused — so also test 3 + 3 with
  seconds = 12 batteries, 12 chargers, accepted, then one more scooter without a second = 13 ≤ 14
  accepted at the charger and refused at the pool of its model when that pool is at 6).
- Quote: 5 days Scout = per-day 32 × 5 = 160; 2 days = flat 75; 7 days = 27 × 7 = 189; second
  battery 1 × 8.00 × 5 = 40; `sunshade` 3 × 1 × 5 = 15 (PerDay) and `cup-holder` 5 × 1 = 5
  (PerRental); zone `vacation-homes` fee 25; tax rate 0.0650 on 245 → 15.925 → **15.93**
  (AwayFromZero); an invalid tier set (`Gap`) fails closed with the problem named and no amount;
  `Days` outside 1–60 refused.
- `EarliestPublicStart`: the DST twin of §5.4 scene 4; `EarliestStaffStart` = today.
- Number format: `OU-000007` for id 7; `OU-123456` for id 123456.
- `DeliveryWindows.Hours` returns the four pairs of §4.6.

### 9.2 `AdminCrudTests` additions — host, SQLite, `CreateStaffClient`

Fixture: `SeedAsync()` **plus** the settings row (`ChargerCount = 14`, `SecondBatteryPerDay = 8`,
`LostChargerFee = 30`, `NextDayCutoffHour = 18`) **plus** `BatterySeeder.RunAsync` — the twelve
batteries — arranged in the test's own setup, since `SeedAsync` does neither. Turnaround: the seed
now writes 1 (§4.7), so the fixture carries it.

- Anonymous `GET /admin/bookings` → redirect to `/admin/login` (the existing pattern).
- Staff `GET /admin/bookings` on an empty table prints **"Nenhuma reserva ainda."** in pt-BR and
  the English twin in en-US (set the admin culture cookie the way the existing tests do).
- Create through the form (`FormPoster.ReadFormAsync` + `Set`) one Scout, 20–24 Dec, one second
  battery at 8.00, cup holder, Pop Century → 302 to details; the row exists, `Number` is `OU-` +
  six digits, `Status = Confirmed`, `Source = Staff`, `Days = 5`, `Subtotal = 160`,
  `ExtraBatteriesTotal = 40`, `AddOnsTotal = 5`, `DeliveryFee = 0`, `Tax = 0`, `Total = 205`; one
  `BookingEvents` row of type `Created` with the actor; **zero** `AuditEntries` rows for
  `nameof(Booking)` (presence: the event row; absence: the audit row).
- Create a fifth Scout over four existing → 200 with **"Não disponível: "** in the body and no new
  row; the same post with the overbook field set → 302, row with `IsOverbooked = true`, event
  summary contains the word *overbooked*, and the list page shows **"Acima da frota"**.
- Cancel → `Status = Cancelled`, event `Cancelled`, and a new booking for the same dates is accepted
  (release proved through the screen, not only in §9.1).
- Cancel twice → the second answers with **"Esta reserva não pode ser cancelada na situação atual."**
  and no second event.
- Details prints the stored `ProductName` **after** the product is renamed through the editor of
  leva 04 (snapshot proved through two screens: rename, then read the booking).
- Settings: the cut-off field round-trips; `24` is refused with the range message; the audit line
  of the existing handler still lands.
- `No_battery_column_declares_a_store_default`: cardinal **16**. The interpolated-key test: the
  three new patterns join the list, cardinal **32**.
- A type test in the spirit of EMENDA-04B-04: every `decimal` property of the four booking
  entities has precision `(10, 2)` in the model **except** `Booking.TaxRate`, which is `(5, 4)` —
  read from `IProperty.GetPrecision()/GetScale()`, and the loop asserts it walked **≥ 14** decimal
  columns.

### 9.3 `SiteBehaviourTests` / `RenderedTextTests` / `SeoTests`

- `/book` and `/pt/book` join `PublicPathList` (23 entries) and the `InlineData` of
  `No_page_prints_a_resource_key`.
- Anonymous `GET /book?product=drive-scout-4&start=…&end=…&quantity=1&extraBatteries=1&place=L{id}`
  (five days, fixture with 12 batteries and the settings row, Pop Century, no extra) → 200,
  **"Available for these dates"** and the total **`$200.00`** (160 rental + 40 second battery, fee
  0, tax 0). The plan restates the number from the fields actually posted before the test is written.
- The same with four Scouts already booked → **"Sold out for these dates"**; with three Scouts
  each with a second battery already booked → **"The second battery is not available for these
  dates"** and the breakdown without it.
- The Portuguese twins on `/pt/book`.
- `/rentals/drive-scout-4` prints **"Check availability and price"** and **not** *"Booking opens
  soon"*; `/rentals/single-stroller` still prints the coming-soon sentence.
- The sitemap lists `/book` and `/pt/book` with their alternates (the existing test's floors move
  up by one page, and a direct assertion on the two locs is added).
- Start before the earliest day → the message names the earliest day computed from the
  `FakeClock` the test set.

### 9.4 `SolutionWiringTests` / `ArchitectureTests`

- `AvailabilityQueries`, `QuoteBuilder`, `BookingWriter`, `BookingTimeline` are registered scoped
  (names, never types — the reflection rule).
- `Domain/` still references nothing (the existing test; it is what forbids a `DbContext` in
  `Availability.cs`).

### 9.5 Cardinals that move, in one place

| Test | From | To | Why |
|---|---|---|---|
| `AdminCrudTests.cs:647` | 15 | 32 | 11 statuses + 4 windows + 2 sources, keys by interpolation |
| `AdminCrudTests.cs:960` | 15 | 16 | `NextDayCutoffHour` |
| `SiteBehaviourTests.PublicPathList` | 21 | 23 | `/book`, `/pt/book` |
| `[Fact]`/`[Theory]` total | 107 | measured by the agent at P3 | reported, never asserted |

### 9.6 The clock in tests

`FakeClock` (D13/03): `UtcNow` settable, `TodayInOrlando()` and `NowInOrlando()` derived through
the **same** zone resolution as `SystemClock` — the agent moves `ResolveOrlandoZone()` into a
`Domain`- or `Application`-level helper both implement against **only if** that keeps `foundation.tsv`
C06 (the list of files reading `DateTime.UtcNow`) at exactly `SystemClock.cs`; otherwise the fake
resolves the zone itself, duplicated on purpose, with a comment saying which control the
duplication protects.

---

## 10. Visual check

The roteiro is **`Docs/conferencia-leva-03.md`**, already in the tree with an empty result column.
Its pre-condition block carries: recompile and run; the migration applied and the settings row
present; the twelve batteries seeded; **"Dias de intervalo" = 1 on the three bookable products**
(human step 3 of §0) with item 0b measuring it; the staff account; the instrument for the width
items (Chrome, `F12`, `Ctrl+Shift+M`, **375**, zoom **100 %**, never *Fit to window*); and **item 0,
the build-fresh proof: the admin navigation has seven destinations, not six.** Every line names
the screen and quotes the word the screen shows, in Portuguese for `/admin` and for `/pt/book`, in
English for `/book`.

What the roteiro does not reach: the LocalDB row counts (the reviewer's shell has no `dotnet`
and no client for the database), so items that end in a database fact are answered by the
operator with the number the screen shows, and the P3 report carries the count read by the agent.

---

## 11. Controls

### 11.1 Files the front alters

**New:** `Domain/Booking.cs`, `Domain/BookingLine.cs`, `Domain/BookingAddOn.cs`,
`Domain/BookingEvent.cs`, `Domain/BookingStatusRules.cs`, `Domain/Availability.cs`,
`Domain/Quote.cs`, `Domain/BookingRules.cs`, `Domain/DeliveryWindows.cs`;
`Infrastructure/Data/Configurations/{Booking,BookingLine,BookingAddOn,BookingEvent}Configuration.cs`;
`Infrastructure/Data/Migrations/<ts>_AddBookings.cs` and `.Designer.cs`;
`Infrastructure/Data/{AvailabilityQueries,QuoteBuilder,BookingWriter,BookingTimeline}.cs`;
`Pages/Book/Index.cshtml` and `.cshtml.cs`;
`Pages/Admin/Bookings/{Index,Create,Details}.cshtml` and `.cshtml.cs`;
`tests/OrlandoUp.Tests/FakeClock.cs`; `Docs/controles/booking-core.tsv`;
`Docs/relatorio-leva-03-etapa-N.md`.

**Modified, and only in this:** `Domain/Enums.cs` (the four enums of §4.6);
`Infrastructure/Data/AppDbContext.cs` (four `DbSet`s); `Domain/OperationalSettings.cs` and
`Infrastructure/Data/Configurations/OperationalSettingsConfiguration.cs` (`NextDayCutoffHour`);
`Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs` (generated);
`Infrastructure/Seeding/CatalogSeedData.cs` and `CatalogSeeder.cs` (turnaround, §4.7);
`Application/IClock.cs` and `Infrastructure/SystemClock.cs` (`NowInOrlando()`, §5.4, only if
needed); `Program.cs` (four registrations); `Pages/Rentals/Details.cshtml` (lines 177–183 only);
`Pages/Shared/_AdminLayout.cshtml` (one link); `Pages/Admin/Index.cshtml` and `.cshtml.cs` (one
count); `Pages/Admin/Settings/Index.cshtml` and `.cshtml.cs` (one field);
`Resources/SharedResource.resx` and `SharedResource.pt-BR.resx` (§8); `wwwroot/css/site.css`
(only if a new component needs a rule; the plan names it); `tests/OrlandoUp.Tests/SiteFactory.cs`
(the clock, D13/03); `tests/OrlandoUp.Tests/{DomainTests,AdminCrudTests,SiteBehaviourTests,
RenderedTextTests,SeoTests,SolutionWiringTests}.cs`; `Docs/controles/admin-catalog.tsv` (C06
reach floor only, if it moves) and `Docs/controles/fleet-batteries.tsv` (C06 rebased 5 → 6, label
untouched); `Docs/architecture.md` (§3: the booking number and the rounding sentence, as a dated
note, D6/03 and D7/03); `README.md` (the *Running locally* section, only if a step changes);
`Docs/conferencia-leva-03.md` (**results column only, by the operator — never by the agent**);
`Docs/fila-cc.md` (Estado and Commit of this line).

**Negative, by diff column:** `CLAUDE.md`, `Docs/decisions.md`, `Docs/roadmap.md`,
`Docs/open-questions.md`, `Docs/market-notes.md`, `Docs/backlog-conhecido.md`,
`Docs/protocolo-conversa.md`, `Docs/regras-de-controle.md`, `Docs/medir-controles.sh`, every
`resumo-*`/`atrito-*`, every earlier spec, every earlier `conferencia-*`, every earlier
`relatorio-*`, `.githooks/`, `.gitattributes`, `.github/`, `appsettings.json` (C16 of
`foundation.tsv` stays alive), `Domain/Product.cs`, `Domain/Unit.cs`, `Domain/Battery.cs`,
`Domain/PricingTier*.cs`, `Domain/Delivery*.cs`, `Domain/AddOn*.cs`, `Domain/AuditEntry.cs`,
`Infrastructure/Data/{CatalogQueries,CatalogWriter,AuditTrail}.cs`, every
`Pages/Admin/{Products,Units,Batteries,Audit}/`, `Pages/Shared/_Layout.cshtml`, every other public
page, `wwwroot/fonts/`, `wwwroot/img/`, `Docs/controles/{foundation,public-site}.tsv` appear in
**zero** lines of `git diff --stat` over the leva's commit range; `Docs/controles/admin-catalog.tsv`
in at most one line; this queue in two cells.

### 11.2 Controls for `Docs/controles/booking-core.tsv`

The exact commands are the agent's; every one obeys `Docs/regras-de-controle.md` (`-I`,
`--exclude-dir=bin --exclude-dir=obj`, `; true` and a default on every count, no `git grep`, no TAB
inside a command, presence sibling for every absence).

1. **C01 — D7/03: no booking page reads the catalog price.** `PricingTier`, `.PricingTiers`,
   `AddOn.Amount` and `DeliveryFee` from a `DeliveryZone` do not occur in
   `Pages/Admin/Bookings/Details*` — count 0. **C02 reach:** the same identifiers occur in
   `Infrastructure/Data/QuoteBuilder.cs` — `sim`.
2. **C03 — §5.1: no page assigns a booking status.** `Status = BookingStatus.` occurs 0 times under
   `Pages/` and `Infrastructure/`. **C04 reach:** it occurs ≥ 2 times under `Domain/`.
3. **C05 — D4/03: the public page never overbooks.** `IsOverbooked` occurs 0 times under
   `Pages/Book/`. **C06 reach:** ≥ 1 under `Pages/Admin/Bookings/`.
4. **C07 — §5.2 in one file:** the list of files under `src/` containing `HoldsInventory` is exactly
   `Domain/BookingStatusRules.cs,Infrastructure/Data/AvailabilityQueries.cs,Pages/Admin/Index.cshtml.cs`
   (sorted, `paste -sd,`), the C06-of-`foundation` shape. The dashboard is on the list because its
   count filters by it; a fourth reader is a stop.
5. **C08 — D12/03 / EMENDA-04B-04: no value-typed `FirstOrDefault` on the settings row.** In
   `Infrastructure/Data/AvailabilityQueries.cs`, `FirstOrDefaultAsync` and `FirstOrDefault(` occur
   0 times (the loader uses `SingleAsync` and lets absence throw). **C09 reach:** `SingleAsync`
   occurs ≥ 1 in that file.
6. **C10 — the disabled button is gone (front-owned prohibition):** `Product_BookingSoon` occurs in
   0 files under `src/`. **C11 reach:** `Product_CheckAvailability` occurs ≥ 1 under `Pages/Rentals/`.
7. **C12 — D2/03: the seeder no longer writes a literal turnaround.** `TurnaroundDays = 0` occurs 0
   times in `Infrastructure/Seeding/CatalogSeeder.cs`. **C13 reach:** `TurnaroundDays` occurs ≥ 1
   in `CatalogSeedData.cs`.
8. **C14 — no money aggregated in SQL (SQLite lesson, §6.1):** `SumAsync(` and `MaxAsync(` occur 0
   times under `Infrastructure/Data/` in files whose name starts with `Availability`, `Quote` or
   `Booking`. **C15 reach:** `.Sum(` or `.Max(` occurs ≥ 1 in `Domain/Availability.cs` (the
   in-memory computation exists).

**Permanent controls this leva REMEASURES in place, never duplicates:** `admin-catalog.tsv`
**C05** (stays **2** — three new `OnPost…Async` under `Pages/Admin/Bookings/` and one on Settings
already counted, each with its `.Record(`) and **C06** (the reach floor moves from 9 to the
measured count if the agent prefers; leaving 9 is also correct since it is a floor); `fleet-batteries.tsv`
**C06** (`public bool Is[A-Za-z]+` in `Domain/`: **5 → 6**, `IsOverbooked`; the label names
visibility flags and the operand counts every `Is…` — the plan states the class/member gap in
those words, as the leva 04b resumo §4 did, and rebases the line); `admin-catalog.tsv` **C09/C10**
(the typed public list: still 0 admin addresses, now 23 ≥ 20); `public-site.tsv` **C10** (the link
relation: the new page's links keep it at 0 — measured at P3); `foundation.tsv` **C17/C18** (no
`?? 0` on money — the quote must not introduce one; measured at P3).

### 11.3 What STEP 0 measures, before altering any file, all in the plan

1. `grep -rIwE --exclude-dir=bin --exclude-dir=obj "Booking|BookingLine|BookingAddOn|BookingEvent|Availability|Quote|DeliveryWindow|FakeClock" src tests` — expected **1** line, and that line is
   the `.resx` value of `Product_BookingSoon`; anything else is a stop, and the name changes
   before the type exists.
2. The value at the initial HEAD of every control named in §11.2's last paragraph, and of
   `admin-catalog.tsv` C05 with the operands `a` and `c` shown separately.
3. The proposal of `Docs/controles/booking-core.tsv` with `bash Docs/medir-controles.sh medir`
   run at the initial HEAD — C01, C03, C05, C08, C10, C12, C14 read **0 before the leva too** (the
   files do not exist), so their reach siblings are what proves the leva happened: each sibling
   reads `nao` at HEAD and `sim` at P3, and the plan says which siblings will move.
4. The form of the reflection tests, anchored in **names** (`AvailabilityQueries`, `QuoteBuilder`,
   `BookingWriter`, `BookingTimeline`, `FakeClock`).
5. `CatalogSeeder.cs` read for its re-run behaviour (§4.7), and the answer written in the plan.
6. The `tests/` files that read `UtcNow` or `TodayInOrlando()` directly, listed, so the fake clock's
   frozen default is checked against each (D13/03).
7. Every contradiction between this spec and the tree.

---

## 12. Out of scope, and why

- **Payment, the customer's booking form, the hold and its sweeper, the confirmation e-mail, the
  manage link, refunds** — leva 03b, waiting on Q6 and Q15 (D1/03).
- **Assigning units and batteries to a booking, today's deliveries, the calendar** — the booking
  half of phase 4 (D33); the counts of this leva are what it will assign against.
- **The address on the public page.** A visitor at a vacation home picks the zone and sees the
  price; the address is asked when the booking is made (03b). Recorded so it is not read as a
  forgotten field.
- **The XL battery** — an attribute, never an option (D37); this leva does not mention it anywhere
  a customer reads.
- **Tax** — the zone's rate stays what the seed wrote (0) and the page prints *Taxes included*
  (Q4); no rate is decided here.
- **Cancellation policy and the damage waiver's meaning** — Q5 and Q15; the `damage-waiver` add-on
  is sold as today, promising what its description says.
- **Coupons** — a table the architecture names and no answer has asked for yet.
- **A booking's `Culture` on the public page** — the quote has no booking; 03b writes the culture
  of the page the customer booked on.
- **The public nav** — no *Book* link; the product page is the entry, and `_Layout.cshtml` stays
  untouched (C10/C11 of `public-site.tsv` unmoved).
- **Charger tags** (`CHG-01…14`) — counted, not listed (D37/D38).

### 12.1 Statements re-checked and TRUE — do not "fix" them

- `Product.TurnaroundDays` exists and is editable on the product screen; nothing reads it today
  (`grep -rn TurnaroundDays src`). The leva makes it read, it does not create it.
- `DeliveryZone.SalesTaxRate` and `DeliveryFee` exist and are seeded (0/0/0/25; rate 0 everywhere).
  The quote reads them; nothing is added to the zone.
- `PricingTierRules.Validate` and `PricingTier.Covers` exist; the quote calls them and adds no
  second validator.
- The `OperationalSettings` row is absent on the test host by design (`AdminCrudTests.cs:999`
  asserts it); the fixture arranges it per test, and that test keeps passing.
- `PublicPages.Names()` derives the sitemap from the registered pages; the new page needs no
  registration anywhere.
- `foundation.tsv` C14/C15 answer 127 to the reviewer's shell and are green in the agent's — not a
  defect.

---

## 13. Closing

Two commits: the content commit (code, tests, resources, migration, `booking-core.tsv`, the
remeasured lines, the architecture note), then the closing commit that writes its hash into the
Commit cell of this leva's queue line. At the end of the session: `git status --short` whole and
`git diff --stat`; `bash Docs/medir-controles.sh verificar` on **all five** `Docs/controles/*.tsv`,
reported, never silenced. The push is the operator's.
