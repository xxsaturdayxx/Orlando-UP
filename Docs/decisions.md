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
