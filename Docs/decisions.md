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
