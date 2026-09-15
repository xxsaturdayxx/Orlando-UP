# Decisions — Orlando Up

Numbered decisions with the reason attached. A decision without a reason gets reversed by
accident in a later front; a decision with the reason can be reversed on purpose.

Provenance marks: `[V]` verified in the conversation that wrote the line (how, in parentheses);
`[H]` inherited and not re-checked — a pending check, not a fact. Mark **[operator]** when the
decision was Rod's, **[assistant]** when it was Claude's recommendation accepted by default.

Conversation 1 — 2026-09-04.

---

## Product and business

**D1 — Language follows the reader.** Code, identifiers, UI default, commit messages, and
engineering docs (`README.md`, `CLAUDE.md`, `Docs/architecture.md`, `Docs/decisions.md`,
`Docs/roadmap.md`, `Docs/spec-*.md`) are in **English**. Process artifacts that only Rod,
Claude Web and Claude Code read keep the names and the language the existing skills expect:
`Docs/fila-cc.md`, `Docs/resumo-conversa-N.md`, `Docs/atrito-conversa-N.md`,
`Docs/backlog-conhecido.md`, `Docs/controles/*.tsv`, `Docs/medir-controles.sh` — Portuguese.
Reason: the site must be transferable or sellable to a US buyer without translation work, but
renaming the process files would break the skills that trigger on their names. **[assistant]**,
Rod said "toda a estrutura pode ser em inglês".

**D2 — Own fleet, operated by Ronatrip.** The site manages a real inventory (per-unit assets,
availability, delivery schedule), not a lead form forwarded to a partner. **[operator]**, 2026-09-04.

**D3 — Legal entity is Ronatrip for now; a dedicated LLC is the intent.** Everything that names
the company (legal name, trade name, address, phone, support e-mail, tax id display, Stripe
account, sender domain) lives in **configuration** (`Company` options + secrets), never in code
or seed text, so the switch to the LLC is a config change plus a Stripe account swap, and the
whole repository can be handed over with the domain. **[operator]** intent, **[assistant]** mechanism.

**D4 — Brand: "Orlando Up", domain `orlandoup.com` (owned by Rod).** Canonical host is the apex
`orlandoup.com`; `www` 301-redirects to it. The old "platform for Orlando visitors" idea survives
as the content/guides section that feeds SEO and social posts. `[V]` on 2026-09-04 the domain
served a default WordPress "Hello world" page — where it is hosted is an open question
(`Docs/open-questions.md`). **[assistant]**

**D5 — Catalog v1: mobility scooters (two capacities), manual wheelchair, single / double /
triple stroller, infant stroller.** Exact models, weight capacities, dimensions and photos come
from Rod's fleet (`Docs/open-questions.md`). Scooters that fit Disney transportation
(30 in × 48 in) are flagged as such because it is a real purchase criterion (`Docs/market-notes.md`). **[assistant]**

**D6 — Delivery model: hotel / vacation-home delivery with scheduled hand-over; Disney resorts
require an in-person meet-and-greet** because ScooterBug holds the exclusive right to leave
equipment with Bell Services (`Docs/market-notes.md`, `[V]` web 2026-09-04). Delivery fee and
hand-over rules are attributes of a **delivery zone**, never hard-coded. **[assistant]**

**D7 — Payment: Stripe, full prepayment at booking, hosted Stripe Checkout.** Card, Apple Pay,
Google Pay and Link out of the box; no monthly fee; PCI scope stays SAQ-A because card data
never touches our server; refunds and disputes from the admin via the Stripe API. Damage
deposit, if ever wanted, is a separate authorization hold (manual-capture PaymentIntent) and is
**out of v1**. **[operator]** chose full prepayment, 2026-09-04.

**D8 — Two languages from day one: `en-US` (default) and `pt-BR`, with proper localization,
not browser auto-translate.** Reason: (1) SEO — Google indexes the served language; Brazilian
searches for *"aluguel de scooter em Orlando"* never find an English-only site, and Ronatrip's
whole channel is Brazilian; (2) legal texts (terms, waiver) must be exact, not machine-translated
on the fly; (3) e-mails, receipts and the admin must follow the customer's language, which the
browser cannot do. Spanish is the obvious third language later and costs only a resource file.
**[assistant]**, answering Rod's question.

**D9 — Accessibility is a product requirement, not a polish item: WCAG 2.2 AA.** The audience
includes seniors and people with temporary or permanent disabilities, and ADA website lawsuits
against Florida businesses are common. Large type, real contrast, keyboard navigation, form
labels and error messages read by screen readers — from the first layout. **[assistant]**

## Architecture

**D10 — .NET 10 LTS, ASP.NET Core Razor Pages for the site, Minimal APIs for the app/API surface.**
Razor Pages is what the operator's whole toolchain (rules, skills, agent conventions from
`ronatrip-website`) already knows; server-rendered pages are the SEO-safe default; the API is
there so a future mobile app or partner integration consumes the same application services.
Blazor was considered for component reuse with a .NET MAUI Blazor Hybrid app and rejected for v1:
it adds render-mode complexity to a marketing-plus-checkout site whose app is a later phase and
may never need shared components (see D17). `.NET 10` is LTS (supported until November 2028)
`[V]` learn.microsoft.com 2026-09-04. **[assistant]** — reversible only before leva 1 starts.

**D11 — One web project plus one test project, with layering by folder and an architecture test.**
`src/OrlandoUp.Web` holds `Domain/`, `Application/`, `Infrastructure/`, `Pages/`, `Api/`;
`tests/OrlandoUp.Tests` holds the tests, including a NetArchTest rule that `Domain` references no
other layer and `Application` never references `Infrastructure` or `Pages`. Reason: a
four-project Clean Architecture split doubles the friction for a single-developer codebase; the
folder discipline plus the test gives the same guarantee and can be split into projects later
without renaming namespaces. **[assistant]**

**D12 — Azure SQL Database + EF Core 10; SQL Server LocalDB in development.** Same family as
Ronatrip, so SSMS, backups and the operator's habits transfer. Tier starts at the cheapest
(Basic / serverless auto-pause) and grows with load. Migrations are additive-first and are
applied to production **before** the code that needs them is published — the rule inherited
from `ronatrip-website/CLAUDE.md` `[H]`. **[assistant]**

**D13 — Azure App Service (Linux) with GitHub Actions deploy, in its own resource group
`rg-orlandoup-prod`, ideally its own subscription.** Reason: the resource group is the unit that
gets transferred with the business; GitHub Actions makes deploys repeatable and reviewable by a
buyer, unlike a publish profile on one laptop. Windows App Service + Web Deploy from Visual
Studio stays as the documented fallback (it is what Ronatrip does today `[V]` csproj comment). **[assistant]**

**D14 — Three environments: local (LocalDB, Stripe test), staging (Azure, Stripe test), production
(Azure, Stripe live).** Staging must exist **before** the payments leva goes live — never later.
Reason: Ronatrip has no staging and its `CLAUDE.md` spends a whole section on the accidents
that causes (`[V]` "⚠️ Ambiente: não existe staging"). A B1 app plus a Basic database is
≈ US$ 20/month; one wrong write in production costs more. **[assistant]**

**D15 — Money is `decimal(10,2)` USD in the domain and database; cents only at the Stripe
boundary.** Reason: SQL sums and admin queries stay readable; conversion to cents happens in one
adapter. **Nullable price never coalesces to zero** — a missing price is an error, not a free
item (rule inherited from `ronatrip-website`, `[H]`). **[assistant]**

**D16 — Two kinds of time, two types.** Rental start/end and delivery day are **calendar dates
in Orlando** (`DateOnly`, plus a `TimeOnly` window); audit fields are **UTC instants**
(`DateTime` with `Kind=Utc`, suffix `Utc` in the name). All "today" defaults go through one
`IClock` that knows `America/New_York`. Never `DateTime.Now`. Inherited from Ronatrip's
`DataExibicao` lesson `[H]`. **[assistant]**

**D17 — Mobile: responsive site + PWA first; store apps are a gated later phase.** A customer
rents once per trip; the PWA (installable, push notifications, offline shell) covers the day-of
needs. Native store apps are decided only after the API exists and demand shows up; the
candidate stack then is .NET MAUI consuming the Minimal API. **[assistant]**, Rod asked for apps;
this is the cheapest path that keeps them possible.

**D18 — E-mail through an `IEmailSender` abstraction; first provider is Brevo SMTP (Ronatrip
already has an account) from `hello@orlandoup.com` with SPF/DKIM on the new domain.** Switching
to Azure Communication Services or Resend is one class plus config. **[assistant]**

**D19 — Customers check out as guests; no forced account.** Booking management uses a signed
"manage my booking" link in the confirmation e-mail. ASP.NET Core Identity is for staff (`Admin`,
`Staff` roles) only. Reason: every extra step before payment costs conversions; the audience is
not young. **[assistant]**

**D20 — Formatting culture is pinned to `en-US`; only the UI culture switches.** Request
localization supports `en-US` and `pt-BR` as **UI** cultures and `en-US` alone as **formatting**
culture, so `decimal` model binding never meets a comma. Dates shown to Brazilians are formatted
explicitly by a display helper. Inherited from Ronatrip's InvariantCulture rule `[H]`. **[assistant]**

**D21 — URL scheme: `/…` is English, `/pt/…` is Portuguese; every page emits `hreflang`
alternates and `<html lang>`.** Path prefix (not subdomain, not cookie-only) because it is what
Google indexes reliably and what people share. **[assistant]**

## Process

**D22 — Same Cowork ↔ Claude Code ritual as Ronatrip, lighter.** Instructions enter
`Docs/fila-cc.md` as `aguardando`; a front is executed from a `Docs/spec-*.md`; the conversation
closes with `Docs/resumo-conversa-N.md` committed; measurable invariants live in
`Docs/controles/*.tsv` measured by `Docs/medir-controles.sh` (copied verbatim from
`ronatrip-website` on 2026-09-04 — it has no Ronatrip-specific reference, `[V]` grep). Lighter
means: **a reversible change is an adjustment, not a front** — CSS, markup, text, refactor and
docs are executed directly with one commit; anything touching database, migration, real e-mail,
deploy, authentication or secrets is a front with a spec. **[assistant]**, mirrors
`ronatrip-website/CLAUDE.md`.

**D23 — The first admin account is created by a one-time command, never by a config-only seed
in production.** `dotnet run -- seed-admin` reads e-mail and password from user-secrets and
refuses if any admin exists. Reason: Ronatrip removed a config-driven admin seed because it was a
back door in a shared environment `[V]` `ronatrip-website/CLAUDE.md`. **[assistant]**

**D24 — Secrets never enter the repository.** User-secrets in development, App Service settings
in Azure. `appsettings.json` carries only shape and non-secret defaults. A pre-commit hook greps
the staged content for the usual secret shapes. **[assistant]**

## Amendments

**D25 — 2026-09-04 (after the closing of conversation 1) — Hosting starts on Ronatrip's existing
Windows App Service plan (Basic), as a separate app; own plan and resource group only when demand
or the LLC justifies it. Supersedes the "Linux, own resource group" part of D13 and the "B1 app"
cost lines of D14; everything else in D13/D14 stands (GitHub Actions deploy, staging before live
payments, separate databases).** **[operator]**, on the question "could it stay on the plan we
already have?". Reasons: (1) an App Service plan is billed per plan, not per app, so a second app
on the existing Basic plan costs nothing extra in compute — only the Azure SQL Basic database
(≈ US$ 5/month) is new; (2) a plan is tied to one OS, and Ronatrip's is Windows, so sharing it
means Windows — ASP.NET Core 10 runs identically there, and the two things that differ (Windows
time-zone ids and a case-insensitive file system) are already handled by D9/01 (`IClock` tries
the IANA id first, then `Eastern Standard Time`) and by the `nome-exato` control type in
`Docs/medir-controles.sh`; (3) Linux was suggested only because a **new** plan is far cheaper on
Linux and the case-sensitive file system catches path bugs early — neither reason applies to a
plan that already exists. Staging (D14) becomes a second app on the same plan
(`app-orlandoup-stg`, its own Basic database), still ≈ US$ 5/month. **Trigger to leave the shared
plan:** sustained CPU above ~50 % or memory above ~70 % on the plan (Application Insights), or the
LLC — at that point Orlando Up moves to its own plan in `rg-orlandoup-prod`; moving an app
between plans needs the same resource group, region and OS, and a move to another subscription is
a redeploy from GitHub plus a database restore, which is why the deploy stays in Actions and the
schema in migrations. Open question Q8 is closed by this decision.

**D25 — note of 2026-09-04 (later the same day).** The existing plan already hosts three apps
(Ronatrip, RonaMagic, MathWithLucas) `[operator]`; Orlando Up adds two (production and staging).
Scaling the plan **up** (B1 → B2/B3, or Basic → Standard/Premium v3) is a portal action on the
plan ("Scale up") that restarts the apps for a moment and needs **no change and no redeploy on
our side** — code, settings, domains and certificates stay; every app on the plan gets the extra
capacity. The "redeploy" mentioned in D25 applies only to moving Orlando Up to a **different
subscription** (the LLC scenario). Gate for phase 5, before creating the two apps: read the plan's
*Memory Percentage* and *CPU Percentage* for the last 30 days; above ~60 % memory, scale up to B2
(2 cores, 3.5 GB) first — B1 is 1 core and 1.75 GB shared by everything on the plan. Decide the
tier by metrics at that moment, never in advance; scaling down is the same click.

**D26 — 2026-09-05 (conversation 2, after the closing of leva 01) — The real fleet replaces the
generic catalog: two scooter models, one wheelchair, strollers "coming soon".** **[operator]**.
Ronatrip owns ≈ 4 Drive Medical scooters of one model and ≈ 4 Drive Medical **Spitfire** scooters,
2 Drive Medical wheelchairs, and two strollers in no condition to rent; the stroller fleet is to be
bought in the coming weeks (Thanksgiving sales). Consequences: the seed of leva 02 carries
`drive-scout-4` and `drive-spitfire-ex` (exact model names and counts to be read from the labels —
Rod confirms before the seed), one wheelchair product, and the four stroller products as
`IsActive = false` until units exist, shown on the public site as "coming soon" without price.
Published specs `[V, web 2026-09-05]`: Scout 4 — 300 lb, 42.3 × 20.5 in, 9 mi (14 mi extended);
Spitfire EX — 300 lb, 39 × 19.5 in, seat 17 in, 9 mi (15 mi with 21 Ah). Both fit the Disney
30 × 48 in limit. Closes Q1 (counts remain to confirm).

**D27 — 2026-09-05 — Pricing starts on the market-median tiers of the leva 01 seed; Ronatrip's
current practice is recorded, not adopted.** **[operator]**. Today Ronatrip charges US$ 175 for the
first week, US$ 20 per additional day and a US$ 30 delivery-and-pickup fee. The tiers of
`Docs/spec-01-foundation.md` §5 (standard scooter: 1–2 d flat 75; 3–6 d 32/d; 7+ d 27/d, i.e.
US$ 189 for 7 days) are close to that and match the competitors, so they go live as the list
price; occasional promotions become **coupons** (phase 4, roadmap) rather than lower list prices.
The **delivery fee** stays an open point of Q3: the seed has US$ 0 for resort zones and US$ 25
for vacation homes, Ronatrip charges a flat US$ 30 today — decided with the delivery areas.
Closes Q2.

**D28 — 2026-09-05 — Company data is Ronatrip's, as on ronatrip.com.** **[operator]**. Trade name
on the site stays "Orlando Up"; legal name shown: **Ronatrip Tours & Travel**; address: 7362
Futures Dr, Ste 2, Orlando, FL 32819; phone and WhatsApp: **a dedicated WhatsApp line is being
set up — number to confirm** (the `TODO-phone` / `TODO-whatsapp` placeholders stay until then,
so control C16 keeps at least those two markers); support e-mail and hours: to confirm with the
number. Closes Q9 except the number, which is Q12's.

**D29 — 2026-09-05 — Design direction: C "park energy" as the starting point, reworked into a
modern direction with the design skill before leva 02.** **[operator]** ("modern, beautiful,
functional; if I had to choose now, C"). The tokens of direction A implemented in leva 01
(`Docs/architecture.md` §12) stay until the new canvas is approved; leva 02 then replaces
`site.css` tokens in one place. Rule kept from D9: every text/background pair ≥ 4.5:1, measured
in the plan. Closes Q10.

**D30 — 2026-09-05 — Images are AI-generated (Google), as on ronatrip.com, and pass through
the `preparo-imagem-site` skill before entering the repository.** **[operator]**. Consequence:
generated images are **illustrative** — hero, lifestyle, category art — and must not depict a
branded model as if it were the unit delivered; product pages name the real model in text and use
a generated image of a generic scooter of the same class, or a real photo when Rod takes one.
No park logos, characters or trade dress in any generated image (`Docs/market-notes.md`). Closes
Q11.

**D29 — closed 2026-09-05 (later the same day).** Rod approved **Option A "Navy + Sun"** of the
canvas "Orlando Up — Direção C moderna" (https://claude.ai/code/artifact/39c6540f-80b0-4248-9f44-e90fe32fe43f,
page "Opção A"); Option B "grafite + vermelho" stays on the canvas as the rejected alternative
(reason to reject, recorded: red is also the web's error colour, so checkout error states would
need a second signal). **[operator]** ("ficou bom"). The approved tokens are in
`Docs/architecture.md` §12 (v1) and replace the direction-A tokens of leva 01 in `site.css` in
leva 02. Typography: Bricolage Grotesque (headings, weights 500–800) + Manrope (body), both
self-hosted as woff2 like Nunito was (D7/01), Nunito retired.

**D31 — 2026-09-05 — The two language versions sell to different audiences, and the copy is
allowed to differ.** **[operator]**. The English site speaks to the American and international
tourist: hotel delivery, the Disney bus/Skyliner fit, clear prices. The Portuguese site adds what
only matters to Brazilians: Brazilian team, service in Portuguese, WhatsApp in Portuguese. The
`.resx` per culture already permits different content under the same key, so no page is
duplicated; the parity test (same key set, no empty value) still holds. Rule for copy: a claim
that is not a differentiator for the reader of that culture does not appear in that culture.

**D32 — 2026-09-05 (conversation 3, writing the leva 02 spec) — A product can be visible without
being bookable, and a dimension nobody has measured is absent, not zero.** **[assistant]**,
amending the mechanism of D26 without changing its intent. D26 asked for the four stroller
products to be seeded with `IsActive = false` and still shown as "coming soon"; that contradicts
the rule closed in conversation 2 — public pages filter `IsActive`, and a hidden product must 404
exactly like one that never existed. So the mechanism becomes a second column: `Products.IsBookable`
(`bit NOT NULL DEFAULT 1`). `IsActive` keeps meaning *visible on the site*; `IsBookable` means
*units and a price list exist and leva 03 may offer it*. A coming-soon product is
`IsActive = true, IsBookable = false`, carries no pricing tier and no add-on link, and shows no
price and no booking button. In the same migration `Products.WidthIn` and `Products.LengthIn`
become nullable, and `FitsDisneyTransport` becomes `bool?`: a product we own but have not measured,
and one we have not bought, must not publish an invented dimension or a badge that claims a fit —
the same reason D15 gives for a missing price. Consequence for later phases: availability and
checkout read `IsBookable`, never `IsActive` alone. Spec: `Docs/spec-02-public-site.md` §4.

**D33 — 2026-09-07 (conversation 4, choosing the front) — Leva 04 runs before leva 03: the
booking-independent half of roadmap phase 4 is executed first.** **[operator]**, on the reviewer's
recommendation. `Docs/roadmap.md` declares phase 4 as depending on phase 3, and the half of it that
reads bookings does. The half specified in `Docs/spec-04-admin-catalog.md` — catalog editor, fleet
CRUD, audit trail — reads nothing that does not already exist, and running it now buys three things
leva 03 would otherwise pay for: the operator stops needing a commit to change what the site says,
the `IsActive` store-default defect is closed before a screen exercises it, and the authenticated
test client that leva 03 needs for its own admin screens exists already. Leva 03 keeps its number
and its scope; phase 3 is the next front. Consequence: the leva numbers follow the roadmap phases,
not the order of execution, and `Docs/spec-03-*.md` will be written after `spec-04`. What stays out
of leva 04 for lack of bookings, and must not be read as forgotten: today's deliveries and pickups,
the booking detail with its timeline, unit assignment and the calendar.

**D34 — 2026-09-07 (conversation 4, writing the leva 04 spec) — No boolean column carries a store
default; the four that do are repaired in one migration.** **[operator]**, on the reviewer's
recommendation, extending the rule stated for `IsBookable` in D32 to the whole schema. Measured on
2026-09-07 with `grep -rn HasDefaultValue` over
`src/OrlandoUp.Web/Infrastructure/Data/Configurations`: six occurrences, four of them `bool` —
`ProductConfiguration.cs:26`, `AddOnConfiguration.cs:20`, `DeliveryZoneConfiguration.cs:22`,
`DeliveryLocationConfiguration.cs:18`. The backlog entry of 2026-09-06 had recorded only the first.
The reason is the one written in `ProductConfiguration.cs:28-34`: a store default on a non-nullable
`bool` makes the provider unable to tell *the caller said false* from *the caller said nothing*, so
the explicit `false` is dropped and the row is inserted with the default. It bites on INSERT, which
is exactly what an administration screen does, and it cannot be caught by any value assertion —
the row is written, the count is right, and the meaning is wrong. The C# initializer `= true` stays
on all four domain classes; only the store default goes. `TurnaroundDays` (default `0`) and
`SalesTaxRate` (default `0m`) share the mechanism but are not booleans and are not written by any
screen yet: they are backlog, with the front that will bite named. Rule from here on: a boolean
column is declared `IsRequired()` and nothing else, and the rows that already exist are filled by an
explicit statement in the migration, where a reviewer can read it.

**D35 — 2026-09-08 (conversation 4, review of the leva 04 P1) — The mechanism stated in D32 and D34
is backwards: the store default that swallows an explicit `false` is the one declared by SQL, not
the one declared by value.** **[assistant]**, correcting the reason of a decision without changing
what it does. Measured twice by the agent on EF Core 10.0.11, the second time re-run rather than
quoted, in a throwaway project outside this repository, and recorded in full in
`Docs/relatorio-leva-04-etapa-1.md` §6. EF decides whether to send a column by comparing the
property's value against its **sentinel**. `HasDefaultValue(true)` moves the sentinel to `true`, so
an explicit `false` differs from it, is sent, and reads back `false`. `HasDefaultValueSql(...)`
leaves the sentinel at the language default, so an explicit `false` coincides with it, the column is
omitted, and the database default writes `true` over the caller's intent. This repository declares
no default by SQL, so the defect described in D32, in D34 and in the comment of
`ProductConfiguration.cs` never bit here. The finding is about EF's update pipeline and not about
the provider: the include-or-omit decision is taken before any provider sees the statement, and the
SQLite in-memory database was only the cheap place to run it. **What does not change:** D34 stands
and the migration is applied. Removing the store defaults is schema hygiene that is worth doing on
its own — the schema comes to say what the model means — and the rule it installs, *a boolean column
is `IsRequired()` and nothing else*, is what closes the door on the by-SQL form, which is the one
that does bite. The behavioural neutrality of the change is proved in that same §6, not asserted.
**And a second trap, measured in `sys.default_constraints` of `OrlandoUpDb` and recorded here
because nothing in the code would ever reveal it:** a column added by `AddColumn<bool>(…
defaultValue: x)` leaves a **permanent** default constraint in SQL Server that the model snapshot
never carries, so no later migration removes it and no model-level control can see it. That is how
`Products.IsBookable` — the column D32 deliberately gave no model default — ended up with
`DEFAULT ((0))` in the database. It is inert, because a property whose model declares no default has
`valueGenerated=Never` and EF names it in every `INSERT`; it is removed anyway, in the same
migration, by `EMENDA-04-03` C1. Rule from here on: after a migration that adds a column with a
`defaultValue`, read `sys.default_constraints` — the model will not tell you.

**D36 — 2026-09-09 (conversation 5, answering Q14) — Batteries are inventory of their own, tied to
the scooter MODEL and not to a unit; the first goes out with the package and a second is a paid
extra; every battery carries a charger, and both are named on the reservation.** **[operator]**,
answering four of the five points of Q14. What Rod stated, verbatim in substance:

- **There are four battery types, two per scooter model.** *Drive Scout Normal* — **9 miles**;
  *Drive Scout XL* — **14 miles**; *Drive Spitfire Normal* — **9 miles**; *Drive Spitfire XL* —
  **14 miles**. A battery therefore belongs to a **model**, not to a scooter: any Drive Scout
  battery fits any Drive Scout. That is the fact `Unit` as it stands cannot express, because a unit
  points at one product row.
- **Stock: 6 Drive Scout batteries and 6 Drive Spitfire**, twelve in total. **Each battery carries
  an identification tag**, and at the moment of booking **the administrator records which battery
  ids go out with the scooter** — the count alone is not enough.
- **The first battery is part of the package. A second costs about US$ 8 per day**, and may be
  given as a courtesy in some situations. A courtesy battery still leaves the shelf, so it is
  recorded on the reservation even when it is not charged for.
- **Every battery goes out with a charger**, and the charger is not priced separately — it is part
  of the battery. The administrator also records **how many chargers** the reservation carries.
  **A charger that is not returned carries a US$ 30 penalty.**
- **The rental shape, in Rod's own scene:** ten days with a second battery means the customer
  receives the scooter, **two batteries and two chargers**; he is instructed to charge both every
  night even if he barely rode; at the end all five pieces come back.

**Consequence computed by the reviewer, not stated by Rod, and it is the reason this decision exists
before the leva 03 spec:** with **4 scooters and 6 batteries per model**, four scooters on rent
commit four batteries and leave two — so **at most two of those four customers can take a second
battery**. The battery, not the scooter, is what runs out first, and a system that counts only
scooters would sell a third one. Availability counts batteries per model, per day.

**What the public site says today, and what it means now:** `RangeMiles` is seeded at **9** for both
scooter models, which is the **Normal** battery — the package as sold. The XL exists in the fleet
and is advertised nowhere. Whether it becomes a published option is a content decision that rides
with leva 03, not a defect of leva 02.

**Two facts that are content and not schema, recorded so they are not lost:** the customer only has
to take **the battery** out of the car at the end of the day to charge it, never the whole scooter —
which is a genuine selling point for a rental in a hotel garage; and the reminder to return the
chargers belongs in the FAQ and in the terms, alongside the penalty. Both go to the copy of leva 03,
not to its schema.

**What Q14 still does not answer, and what leva 03 cannot invent:** how the six per model split
between Normal and XL; which of the two the package includes; whether the second battery may be an
XL and at what price; what makes the US$ 8 an average rather than a number; whether the charger has
a tag of its own or only a count; whether the US$ 30 is per charger or per reservation; and how long
a battery is unavailable after coming back. Q14 stays open with exactly those points.

**D37 — 2026-09-09 (conversation 5, closing Q14) — The XL battery is an ATTRIBUTE of a battery, never
a product option; the second battery is priced per reservation by the administrator; chargers are
fungible and counted, not identified.** **[operator]**, answering the seven remaining points of Q14
and correcting two things D36 left open. Where this and D36 disagree, **this one wins**.

**The XL is not for sale, and must not become a catalog option.** Ronatrip owned two Drive Scout XL;
a customer broke one, so **one remains**. Rod raised the XL so the model would be able to hold it,
not because it is offered: **today he charges nothing extra for it and the site says nothing about
it**, and a customer who happens to receive it simply gets more range. He does not expect to own
many — an XL costs more than two Normals and is hard to find. **And even with plenty of them he
would not charge extra.** So the XL is a property of a physical battery, like a serial number, and
**the catalog gets no XL product, no XL variant and no XL add-on.** The site keeps publishing the
Normal figure, which is what the package promises.

**The consequence that simplifies the whole model:** since an XL satisfies every promise a Normal
satisfies and costs the customer nothing more, **availability treats all batteries of a model as one
pool**. The type is a label for the operation, not an allocation constraint — nothing has to reserve
"a Normal" rather than "a battery". A model that made the type an allocation dimension would be
inventing scarcity the business does not have.

**The second battery is an amount typed per reservation, not a price band.** Rod sometimes gives it
as a courtesy and sometimes charges **between US$ 5 and US$ 10 per day** — that is what "about US$ 8"
meant. He wants to **edit these values in the administration, the way the package prices are edited
now**. So the reservation carries the amount it was actually sold for, and zero is a legitimate
value that still consumes a battery from the pool.

**Chargers are fungible and counted; they are not identified units.** They carry no tag today, and
**any charger fits any battery — including across both scooter models**. That settles the shape by
itself: a piece with no identity of its own needs a **count**, not a row per item. Rod has
considered tagging them for tidiness; the model does not depend on it either way.

**The lost-charger penalty is US$ 30 PER CHARGER, and the amount is administration-editable.** It is
the cost of buying a replacement, so it changes when that cost changes. It joins the second-battery
amount as the second value that wants an editable settings surface — the same need that is already
waiting for the company data currently sitting in `appsettings.json`.

**There is no recovery interval to model for the customer's sake.** The customer is never asked to
return a charged battery: **Ronatrip charges every battery before the next rental.** Whether that
window has to be represented at all, or is absorbed by the delivery schedule and by the
`Products.TurnaroundDays` buffer that already exists, is a decision for the leva 03 spec rather than
a fact Rod has to supply.

**Two facts D36 recorded that this decision narrows:** the four battery *types* are real, but only
the Drive Scout XL is confirmed to exist in the fleet, and there is exactly one of it; whether a
Drive Spitfire XL was ever owned is not stated. And **a customer broke a battery**, which is the
first damage event this project has recorded — there is no stated policy for a battery that comes
back broken or does not come back at all, unlike the charger, which now has one.

**D38 — 2026-09-09 (conversation 5) — Every physical piece of the fleet carries a short speaking tag
of the shape `LLL-NN`, printed as text and as a QR code; the scanner that reads it is a front of its
own, after leva 03.** **[assistant]**, designing the format Rod asked for and said he would apply by
replacing the existing labels. Three separable questions were folded into one, and they separate
cleanly.

**The symbology: QR, not a linear barcode.** A QR reads at any angle, carries error correction so a
scratched or partly peeled label still resolves, and fits a curved surface the size of a battery. A
1D barcode needs alignment and a flat run of width, which a battery does not offer. Every phone
camera already reads QR with no application at all.

**What the code carries: the bare tag, never a URL.** A URL bakes a domain into a physical object —
it stops working the day the domain changes, it needs the network to mean anything, and a customer
who scans a label out of curiosity learns an internal identifier and lands on a page nobody designed
for him. The code carries the six characters and the application decides what they mean. If the
camera-app behaviour is ever wanted, a printed line beside the code does it without touching the
label's payload.

**The format: three letters, a dash, two digits.** Six characters, which is a QR of the smallest
version — it scans from far away and from an angle, and it is short enough to read aloud over the
phone when a label is unreadable, which is the case the tag exists for.

| Piece | Prefix | Range today |
|---|---|---|
| Drive Scout scooter | `SCT` | `SCT-01` … `SCT-04` |
| Drive Spitfire scooter | `SPT` | `SPT-01` … `SPT-04` |
| Wheelchair | `WCH` | `WCH-01`, `WCH-02` |
| Stroller | `STR` | when they are bought |
| Drive Scout battery | `BSC` | `BSC-01` … `BSC-06` |
| Drive Spitfire battery | `BSP` | `BSP-01` … `BSP-06` |
| Charger | `CHG` | `CHG-01` … `CHG-14` |

**Three properties of that table are deliberate.** No prefix contains `O`, `I` or `Q`, so nothing is
confused with a digit by a human reading a worn label. **The distinct prefixes make the tag unique
across the whole fleet for free**, even though batteries and units live in different tables with
separate indexes — a collision is impossible by construction rather than by a check. And **the
charger prefix names no model**, because D37 says any charger fits any battery of either model: a
tag that named a model would be a lie the day someone moved one.

**The grade is NOT in the tag.** `BSC-06` does not say it is the Extended Range one; the `Kind`
column says that. A tag identifies an object; a column describes it. Rod sticks `BSC-06` on the XL
that survived, and if he ever buys more the numbering simply continues.

**The scooters get the new tags by hand, through the screen leva 04 built.** They carry
`DRIVE-SCOUT-4-001` and friends today, generated by `CatalogSeeder.cs:146` from the slug. Ten rows,
ten minutes, no migration and no code — which is the administration proving what it was built for,
the same way *"Carrinho simples"* was corrected. **It is explicitly out of scope for leva 04b**, so
that a front about batteries does not quietly become a front about relabelling scooters.

**The scanner is a front of its own, and it comes after leva 03, for two reasons that are not
preference.** First, check-in and check-out are operations **on a rental**, and there are no rentals
until leva 03 exists — building the scanner first would produce a screen that scans a tag and has
nowhere to put the answer. Second, and this is the architectural cost: **this project contains no
JavaScript at all today.** The only `<script>` in the tree is the JSON-LD block of
`StructuredData.cs`, which is data and not code, and which is protected by an encoder that forbids
`<`, `>` and `&` precisely because it sits inside a script element. A camera scanner introduces the
first real script, a library, a camera permission and a policy for what a page may load — and every
one of those wants a control. That is a front with its own spec, not a corner of another one.

**Recorded as backlog rather than as an open question**, because nothing is waiting on Rod: the
decision is taken, the sequencing is fixed, and the work is described.

**D39 — 2026-09-12 (conversation 6, choosing the front) — Phase 3 is split: leva 03 is the booking
CORE without payment; leva 03b is Stripe, the public booking form, the e-mail and the hold.**
**[operator]**, on the reviewer's recommendation, choosing over "the whole phase in one leva" and
"the aesthetics round first". Leva 03 writes the four booking tables, the availability rule that
counts units, batteries and chargers per day with the turnaround, the frozen quote, the public
*"check availability and price"* page, and the booking **entered by staff** — which is how a WhatsApp
reservation lands in the system today. Nothing in it waits on Q6 (Stripe account) or Q15 (battery
liability text); everything in 03b does. Same shape as the 04/04b split: schema and rule first, the
surface that depends on an external account second. Both keep the phase number (D33).
`Docs/spec-03-booking-core.md`.

**D40 — 2026-09-12 (conversation 6) — Turnaround is ONE day, for the machine and for its batteries.**
**[operator]**, on the scene *SCT-02 comes back Tuesday 20:00 at Pop Century; Wednesday 09:00 another
customer wants a Scout at Art of Animation*: **no — it needs a day off** for cleaning, charge and
check. `Products.TurnaroundDays`, seeded at 0 for every model since 2026-09-05 and read by nothing,
moves to **1** for the three bookable products (the strollers stay 0: no fact). The battery pool of a
model uses the model's number — one value, because the answer covered both pieces. **The running
database is a copy the seed does not refresh:** Rod sets *"Dias de intervalo"* to 1 on the three
products through the product editor (human step 3 of the spec's §0); until then the site is one day
too generous. The extension to the wheelchair is the assistant's (the scene was a scooter; the reason
given — cleaning and check — applies), editable on the screen.

**D41 — 2026-09-12 (conversation 6) — Fixed delivery and pickup windows (8–10, 10–12, 14–16, 18–20)
and a next-day cut-off at 18:00 Orlando time, both as Q3 assumed since 2026-09-04.** **[operator]**,
on the scene *a customer books Friday 22:00 for Saturday 09:00 at Caribbean Beach* — refused for
Saturday; Sunday is the earliest. The windows are an enum with explicit numbers (a change is a
one-line adjustment); the cut-off hour is a column of `OperationalSettings`, editable on the
settings screen, because it moves in December. The cut-off binds the public page only — staff enter
what they have decided to deliver and are refused only the past. **Q3 stays open** for the zones,
their fees and the meet-and-greet routine, which this does not answer.

**D42 — 2026-09-12 (conversation 6) — Availability is a BARRIER on the public site and a WARNING for
staff.** **[operator]**, on the scene *four Scouts booked 20–24 December, a fifth customer asks for
22–23*: the site says sold out; **staff may enter it above the fleet, marked**, for the cases Rod
solves by hand (a Spitfire instead, a purchase). The staff form computes the same availability the
visitor sees, refuses by default, and accepts when the operator ticks the box that says so; the
booking carries `IsOverbooked = true`, its first event names it, the list shows the badge.
Deliberate over-selling is a business decision and this is it, written down — a system that
forbade it by accident and one that allowed it by accident would be indistinguishable in code.

**D43 — 2026-09-15 (conversation 7) — The delivery fee belongs to the ZONE, is edited on the zone
screen, and the staff booking screen may override it for one booking.** **[operator]**, on the scene
*a family books Westgate Lakes, which is not on the list of hotels; the driver leaves it at the front
desk exactly as at an I-Drive hotel*. Rod's answer named both moments he needs and refused the third:
*"International Dr, a entrega será gratuita (por tempo limitado); regiões com distância superior a 15
milhas teria um custo de $25"* — and, in the same breath, *"Não precisamos fazer isso necessariamente.
Isso pode gerar muito trabalho"* about the distance rule. So: **the zone carries the standing price**
(a column that already exists, now with a screen), and **the booking carries the price agreed in that
conversation** (a nullable override, blank meaning the zone's fee, zero meaning zero). No address is
geocoded and no radius is computed. The booking freezes the number it charged, as it already freezes
every other amount (D7/03), and the first line of its history says when the number was set by hand —
otherwise a total nobody can explain is a total the operator stops trusting.
`Docs/spec-04c-zones-and-places.md` §6.

**D44 — 2026-09-15 (conversation 7) — A zone `other-hotel` covers any hotel in Orlando or Kissimmee,
and it is born with a fee of US$ 0.** **[operator]**: *"Se é em Orlando ou Kissimmee, qualquer hotel
pode ser atendido"*, and free delivery kept **as an advertising instrument, temporarily**. Zero is
today's promise, not a permanent one: it is a row with an edit screen, and changing it is a number in
a field. **Nothing on the public site announces that it is temporary** — a promotion with a start and
an end date is a second concept (coupons, phase 4) and would be a claim the site would have to keep.
In code the zone is not a special case: it is an active zone with **no place list**, which the place
selector already turns into an option that asks for a typed address (D3/04c). It appears on
`/delivery-areas` in both languages, so a guest who does not find his hotel reads that we serve it.

**D45 — 2026-09-15 (conversation 7) — The customer types the hotel address on `/book` itself.**
**[operator]**, choosing it over the cheaper alternative the reviewer offered (show the option, ask
for the address only when the booking form exists). Reason he gave: the booking form of leva 03b
should be born already filled. `/book` is a GET whose state is the query string and persists nothing,
so the address travels in the URL and is stored nowhere until 03b writes a booking. The field is
always visible, because the public site has no JavaScript and this does not introduce any; the server
refuses a blank address only for the options that ask for one — the same rule the staff screen has
had since leva 03.
