# Spec — Leva 02: public site (the real fleet, the v1 palette, one page per question a visitor asks)

**Date:** 2026-09-05, conversation 3. **Executor:** Claude Code, strongest model — this leva
replaces the whole visible surface (palette, type, copy, catalog) and touches the schema once;
everything a visitor will ever read starts here.

**What this leva closes:** the site runs, but every number and every sentence on it is a declared
placeholder `[V, src/OrlandoUp.Web/Infrastructure/Seeding/CatalogSeedData.cs header]`, the palette
is direction A retired by D29, and the delivery areas and the machine-readable plumbing (sitemap,
structured data) do not exist.

**Language of this spec:** English (`Docs/decisions.md` D1). The control rules it relies on are in
`Docs/regras-de-controle.md`; the spec skeleton follows `Docs/spec-01-foundation.md`.

**What this leva is built on, and what it is not allowed to invent:** D26 (the real fleet), D27
(prices), D28 (company data), D29 (palette v1), D30 (images), D31 (content per audience), and the
new D32 (visible but not bookable). Two facts are still missing — the exact model names on the
labels with the count per model (Q13) and the WhatsApp number (Q12). **Neither blocks the leva**,
because §5 and §7 render a missing fact as a visible absence and never as an invented value; both
are named at every point where they touch the work.

---

## 0. Execution surface

**Launcher phrase:** this spec is executed by the line of `Docs/fila-cc.md` dated `2026-09-05`
whose description starts with *"LEVA 02 — PUBLIC SITE"*. Not "the `aguardando` line" — that line,
by name.

**Tree state at receipt** (measured 2026-09-05 by Claude Web, at `6ce1d10`): `git ls-files` = **136**
tracked files, 98 of them under `src/`; `git status --porcelain` **empty**; `main` equals
`origin/main`; `Docs/controles/foundation.tsv` carries **18** controls, all green at the close of
leva 01. On Windows, `warning: LF will be replaced by CRLF` and an index needing
`git update-index --refresh` are noise, never divergence. Any tracked file modified or any
untracked file at receipt outside `scratchpad/` is a stop with a report.

**Files the front ALTERS:** the closed list is §12.1. A file outside it is a stop with a report,
**no cardinal**.

**Files the front PRODUCES as record:** `scratchpad/leva02/plano.md` (the plan — not committed,
`scratchpad/` is ignored, and execution starts only after Rod reviews it);
`Docs/relatorio-leva-02-etapa-N.md` for every mandatory stop (committed before asking approval);
`Docs/controles/public-site.tsv` (proposed in step 0, committed with the content commit);
`Docs/conferencia-leva-02.md` (the visual-check results, §10).

**Files the TOOL generates coupled:** everything under
`src/OrlandoUp.Web/Infrastructure/Data/Migrations/` that `dotnet ef migrations add` writes,
including the updated `AppDbContextModelSnapshot.cs`; `bin/`/`obj/` (ignored). Authorized by this
declaration. **The BOM these tools write is stripped before staging** (`CLAUDE.md`, lesson of
leva 01).

**Four mandatory stops.** The agent commits the report and waits; it does not continue on its own.

| Stop | When | What it carries |
|---|---|---|
| **P0** | after step 0, before any file changes | `scratchpad/leva02/plano.md`: the measurements of §12.3, the answers to the open points of §11, and every contradiction found between this spec and the tree |
| **P1** | migration written, **not applied** | the generated SQL (`dotnet ef migrations script`), classified operation by operation with the `revisao-migration-efcore` skill; the row counts of `Products`, `Units`, `PricingTiers`, `DeliveryZones` measured before |
| **P2** | fonts fetched (or not) | the two font families, their exact source URLs, licence text, byte sizes and the subsets kept — or the reason the network refused them and the fallback in force |
| **P3** | everything written, tests green, before the content commit | the pre-commit sweep (BOM, secrets), the control measurements, `Docs/conferencia-leva-02.md` |

**Steps that need a human hand:**

1. **Rod, at P0:** the three open points of §11 (delivery fee on the public page, the wheelchair
   model, the price of the second scooter). Each has a recommendation and a fallback the agent can
   execute if Rod prefers not to decide now.
2. **The agent applies the migration to LocalDB only.** `OrlandoUpDb` on `(localdb)\MSSQLLocalDB`
   is still the only database that exists (D14: staging and production arrive in phases 3 and 5).
   The destructive part — deleting the placeholder catalog — is the block of §5.4 and runs only
   after P1 is approved.
3. **Visual check:** the agent runs §10 and writes `Docs/conferencia-leva-02.md`; Rod confirms in
   his own browser, and runs the one item the agent cannot (Lighthouse accessibility).
4. **Push:** Rod, after the closing commit — the remote and its credentials are his (`CLAUDE.md`).

---

## 1. What the leva delivers, in plain words

Rod opens the site and it no longer looks like a scaffold. The header is navy, the primary button
is sun yellow with navy text, the headings are Bricolage Grotesque; nothing orange is left. The
home page speaks to the three people who actually rent — the slower walker in a fast group, the
person with a temporary or permanent medical reason, and the adult child booking for a parent —
and the Portuguese home page says the thing that only matters to a Brazilian: there is a Brazilian
team in Orlando and the service is in Portuguese.

The catalog is the fleet Ronatrip owns. Two scooters that exist, with the published specification
of each real model and the price list of D27; one wheelchair; and the four strollers marked
**coming soon**, visible, priced at nothing, with no booking button — because they are not bought
yet and pretending otherwise is how a customer arrives at a hotel and finds no stroller.

A new page answers the question every one of these visitors asks second: *where do you deliver, and
how do I get it?* It lists the four delivery zones from the database with the hand-over rule of
each, so the Disney meet-and-greet is explained before anybody has to phone and ask.

Machines can read the site too: `/sitemap.xml` lists both language versions of every public page
and every product, and each page carries structured data describing the business and the product.
Indexing stays off (`robots.txt` still says `Disallow: /`, the pages still carry `noindex`) — that
is phase 5's switch, and this leva does not touch it.

What this leva is **not**: no booking, no dates picker, no availability, no payment, no e-mail, no
public form of any kind, no admin CRUD, no photographs, no deploy. Those are levas 03–05.

---

## 2. Decisions of this leva

Project-wide decisions are D1–D32 in `Docs/decisions.md`. These are local to the leva; all of them
are settled here so that the agent never has to invent one mid-execution.

**D1/02 — The placeholder catalog is deleted by a documented block of statements, not by a command
that ships.** A permanent `reset-catalog` command would outlive the single reason it exists and
would be a loaded gun pointed at the first real database. The rows are removed once, in the block
of §5.4, against LocalDB, after `SELECT DB_NAME()` is printed and matched (D12), and the report of
P1 records the counts before and after.

**D2/02 — `seed-catalog` keeps its guard: it inserts into an empty `Products` table or does
nothing.** The guard is what protects an administrator's edit from a second run; the leva empties
the table first instead of weakening it.

**D3/02 — No photograph and no generated image enters in this leva.** D30 says images are generated
outside the repository and pass through the `preparo-imagem-site` skill; the agent cannot produce
them, and a front must not stop on an artefact it cannot make. The site ships the geometric line
art of `Docs/architecture.md` §12 — flat SVG tiles on sun / navy / light blue, drawn by the agent,
no third-party artwork — and the `ImagePath` column keeps working exactly as it does today, so
dropping real files under `wwwroot/img/products/` and setting the column later is a reversible
adjustment, not a front.

**D4/02 — Both fonts are self-hosted woff2 under OFL, like Nunito was (D7/01), and Nunito is
deleted.** Bricolage Grotesque (headings) and Manrope (body), the two subsets of leva 01 (latin,
latin-ext) so Portuguese accents render from a file the site owns. If the network refuses the
download, the agent stops at P2 with the exact failing command; it does **not** silently ship the
fallback stack and it does **not** link to a font CDN — a third-party font request on every page is
a privacy and a performance decision nobody made.

**D5/02 — The delivery-areas page reads the zones from the database, never from a resource file.**
Same reason the how-it-works page already does: the hand-over sentence a visitor reads before
booking must be the same string the booking will show, and one source is the only way that holds.

**D6/02 — The difference between the two languages lives in the resource *values*, never in a
conditional block in the markup.** D31 allows the copy to differ; the `.resx` per culture already
carries that difference under one key, so no page is duplicated, the parity test keeps working, and
a translator can see the two versions side by side. Concretely: `Home_TeamTitle` reads *"A local
Orlando team"* in `en-US` and *"Uma equipe brasileira em Orlando"* in `pt-BR` — one key, two
messages. A culture `if` in a public page is a defect, and control C08 counts them.

**D7/02 — Structured data describes what exists and carries no `Offer`.** A `schema.org/Offer` with
a price and `InStock` availability is a promise the site cannot keep until leva 03 puts a checkout
behind it; Google is entitled to show that price and a visitor is entitled to try to buy it. So the
product pages emit a `Product` with name, description, category and brand, and the offer arrives
with the booking flow. Every company field still marked `TODO-` is **omitted** from the
`LocalBusiness` block rather than emitted as a marker.

**D8/02 — `/sitemap.xml` is served while indexing is off, and says nothing about it.** The file is
correct from the day the site is closed; phase 5 flips `Seo:AllowIndexing` and the sitemap is
already right. It lists only pages that answer 200 and only products with `IsActive = true`.

**D9/02 — The retired palette leaves no trace in `src/`.** No token, no hex, no comment explaining
the old orange — control C04 counts the retired hex over `src/` and expects 0, and rule 3 of
`Docs/regras-de-controle.md` forbids a comment that transcribes what a control searches for. The
history of the palette lives in `Docs/architecture.md` §12 and in this spec, which are not `src/`.

**D10/02 — Every internal link in a public page writes `asp-page` and `asp-route-culture` on the
same source line.** It is a formatting rule with a control behind it (C09, a relation in the sense
of rule 4): six Portuguese links escaped to English in leva 01 because the culture was left
implicit, and a line-wise subtraction is the cheapest permanent guard against the same defect.

---

## 3. Measured terrain

Measured at `6ce1d10` on 2026-09-05 by Claude Web. Where the agent's step 0 disagrees with a number
here, **the measurement wins** and the plan says so.

| Fact | Value | How |
|---|---|---|
| tracked files | 136 (98 under `src/`) | `git ls-files` |
| numbered decision lines | 33 (31 distinct; D25 and D29 each carry a closing line) | `grep -c '^\*\*D[0-9]'` |
| resource keys per culture | 145, identical sets | `grep -c '<data name='` on both `.resx` |
| controls of leva 01 | 18, all green | `Docs/controles/foundation.tsv` |
| stylesheet | 524 lines, direction-A tokens, Nunito in two subsets | `wwwroot/css/site.css` |
| public pages | home, rentals, product detail, how-it-works, faq, contact, terms, privacy | `Pages/` |
| seeded catalog | 7 products, 6 add-ons, 4 zones, 10 locations, 7 units | `CatalogSeedData.cs` |
| products with a real model behind them | **0** | the header comment of `CatalogSeedData.cs` |
| `TODO-` markers in `appsettings.json` | 7 | `grep -cF 'TODO-'` |

---

## 4. The one schema change

One migration, named **`AddIsBookableAndOptionalDimensions`**, three column operations, all
non-destructive. It is written at P1 and applied only after the review.

| Operation | SQL shape | Why |
|---|---|---|
| add `Products.IsBookable` | `bit NOT NULL DEFAULT 1` | D32. A product can be visible and not bookable. Existing rows become bookable, which is what they are. |
| alter `Products.WidthIn` | `decimal(5,1) NULL` (was `NOT NULL`) | A dimension we do not know is absent, not zero and not a typical value (the same reason D15 gives for price). Widening `NOT NULL` to `NULL` loses no row and no value. |
| alter `Products.LengthIn` | `decimal(5,1) NULL` (was `NOT NULL`) | idem |

**Domain consequences, all in `Domain/Product.cs` and its readers:**

- `WidthIn` and `LengthIn` become `decimal?`.
- `FitsDisneyTransport` becomes `bool?`: `true` when both dimensions are known and inside
  30 in × 48 in, `false` when both are known and outside, **`null` when either is unknown**. The
  badge renders only on `true`. Unknown must not read as "does not fit", and must not read as
  "fits" — both are claims about a real unit.
- `ProductCard.FitsDisneyTransport` and `ProductDetail.FitsDisneyTransport` become `bool?`; the
  three pages that read them (`_ProductCard.cshtml`, `Rentals/Details.cshtml`) compare to `true`.
- `ProductDetail.WidthIn` / `LengthIn` become `decimal?` and the specs list omits the row it cannot
  fill, exactly as it already does for seat width and range.
- `PricingTierRules.Validate` is **unchanged**. What changes is the seeder: it validates the price
  list of a product that is bookable, and requires a **non-bookable product to carry no tier at
  all** — a price nobody can pay is worse than no price. Both directions get a test (§9).

`builder.Ignore(p => p.FitsDisneyTransport)` stays: the badge is still a reading of the dimensions
and never a stored copy.

**Not in this migration, and why:** no index change, no rename, no drop, no `HasData`. The
`revisao-migration-efcore` skill reviews the generated script at P1 and the report classifies every
statement.

---

## 5. The real fleet

### 5.1 What the seed carries

Seven products, as D26 fixes them. The `IsBookable` column is what separates the two halves.

| # | Slug | Category | Bookable | Units | Dimensions (W × L in) | Seat | Range | Max rider |
|---|---|---|---|---|---|---|---|---|
| 1 | `drive-scout-4` | MobilityScooter | yes | 4 | 20.5 × 42.3 | *unknown → omitted* | 9 mi | 300 lb |
| 2 | `drive-spitfire-ex` | MobilityScooter | yes | 4 | 19.5 × 39 | 17 in | 9 mi | 300 lb |
| 3 | `drive-wheelchair` | Wheelchair | yes | 2 | *see §11 K2* | *see K2* | — | *see K2* |
| 4 | `single-stroller` | Stroller | **no** | 0 | *unknown → null* | — | — | — |
| 5 | `double-stroller` | Stroller | **no** | 0 | *unknown → null* | — | — | — |
| 6 | `triple-stroller` | Stroller | **no** | 0 | *unknown → null* | — | — | — |
| 7 | `infant-stroller` | Stroller | **no** | 0 | *unknown → null* | — | — | — |

Every number in rows 1 and 2 comes from D26, which recorded it `[V, web 2026-09-05]` from the
manufacturer's published specification. **The agent adds no number that is not in D26**; a
specification it would like to have and cannot cite is left out, and the row simply does not appear
on the page. The extended-battery ranges (14 mi for the Scout 4, 15 mi with the 21 Ah pack for the
Spitfire EX) belong in the highlights text, not in `RangeMiles`, which holds the range of the
battery actually delivered.

**Counts are Q13.** D26 says "≈ 4", "≈ 4", "2"; the labels have not been read. The public site
never shows a unit count — only the admin dashboard does — so a count that is off by one misleads
nobody outside `/admin`, and the assumption is recorded in `Docs/open-questions.md` Q13 rather than
hidden in the seed. Asset tags follow the existing convention: `DRIVE-SCOUT-4-001` … `-004`.

**Model names are Q13 too.** The seed writes exactly what D26 names — *Drive Scout 4* and *Drive
Spitfire EX* — and the agent invents no suffix, no year and no trim. The slug is separate from the
name: when Rod reads the labels, a corrected **name** is a content edit (one commit, no migration);
a corrected **slug** is also cheap today because the site is not indexed and no URL was ever
published, and it stops being cheap at phase 5.

### 5.2 Prices

D27: the tiers of the leva-01 seed go live as the list price. Mapped onto the real fleet:

| Slug | 1–2 days | 3–6 days | 7+ days |
|---|---|---|---|
| `drive-scout-4` | flat US$ 75 | US$ 32/day | US$ 27/day |
| `drive-spitfire-ex` | flat US$ 75 | US$ 32/day | US$ 27/day |
| `drive-wheelchair` | flat US$ 40 | US$ 12/day from day 3 | (same band) |
| the four strollers | **no tier at all** | | |

The heavy-duty tier set of leva 01 (95 / 38 / 33) is **retired**: it priced a 400 lb scooter, and
no unit in the fleet is one. Both scooters carry the same list because both are 300 lb machines of
the same class; §11 K3 asks Rod whether he wants the smaller Spitfire cheaper, and the fallback is
"same price", which is what this table says.

Add-ons keep the six codes of leva 01 and are linked to the three bookable products only. A
coming-soon product shows no add-on, for the same reason it shows no price.

### 5.3 What the coming-soon products say

They exist as rows so that the catalog is the truth about the fleet, `IsActive = true` so that they
are reachable, `IsBookable = false` so that nothing is offered. The card shows the name, the
tagline and a **Coming soon** pill instead of a price. The detail page shows name, tagline,
description, highlights and the same pill — no specs list, no price table, no add-ons, no booking
button. The copy says what is true: *these arrive for the winter season; tell us your dates and we
will hold one* — with no form and no promise of a price, because there is no form in this leva
(§13).

The stroller descriptions of leva 01 can be reused **only where the sentence is about the class of
equipment and the park rule** (the Disney 31 in × 52 in stroller limit, wagons prohibited — both in
`Docs/market-notes.md`, both still true). Every sentence that describes a unit we do not own — a
recline, a canopy, a basket, a dimension — is removed. The agent re-reads `Docs/market-notes.md`
before any park rule reaches a page, per the header of that file.

### 5.4 Replacing the rows (runs only after P1 is approved)

In this order, against LocalDB and nothing else:

1. `SELECT DB_NAME()` — printed into the report; it must read `OrlandoUpDb`. Anything else is a
   stop (D12).
2. row counts of `Products`, `Units`, `PricingTiers`, `ProductAddOns`, `ProductTranslations`,
   `DeliveryZones`, `DeliveryLocations`, `AddOns` — recorded before.
3. `dotnet ef database update` — the migration of §4.
4. delete the catalog rows in foreign-key order (`Units` before `Products`, `ProductAddOns` and
   `ProductTranslations` and `PricingTiers` before `Products`, `DeliveryLocations` before
   `DeliveryZones`, `AddOnTranslations` before `AddOns`). Identity tables are **not** touched — the
   admin account of leva 01 survives, and `seed-admin` is not re-run.
5. `dotnet run --project src/OrlandoUp.Web -- seed-catalog`.
6. the same counts, recorded after: expected 7 products, 10 units, 6 add-ons, 4 zones, 10
   locations.

---

## 6. The palette v1 and the type

`wwwroot/css/site.css` is rewritten around the tokens of `Docs/architecture.md` §12 (v1), approved
as D29. The tokens, their values and their measured contrast pairs are in that table and are not
repeated here — **the architecture file is the source, and a value that disagrees with it is a
defect in the stylesheet, not a variation.**

What the agent must get right, and what the visual check looks at:

- **Sun is a surface, never text on paper.** `--color-sun` carries navy text at 10.5:1. The accent
  word in a headline is sun **only over navy**. A link on paper is `--color-link` (#175A96, 6.6:1);
  the lighter blue of the canvas is allowed only at 18 px and above, and the stylesheet says which
  rule uses it.
- **The header, the hero and the "how it works" band are navy**; the page ground is paper; the
  footer is ink. Secondary text over navy is `--color-on-navy-muted`.
- **Shape:** pill buttons and tags (`border-radius: 999px`), 22 px cards, 28–32 px dark panels, hit
  targets ≥ 44 px, primary button 52–58 px tall.
- **Type:** Bricolage Grotesque 500–800 for headings at the display scale of §12
  (84 / 48 / 40 / 32 / 26 / 24 px desktop, `letter-spacing: -0.02em`, `line-height: 1.05`), Manrope
  400–700 for body at 18 px / 1.55. Both `font-display: swap`, both with the fallback
  `"Segoe UI", system-ui, sans-serif` declared on the same stack so a failed font never leaves a
  page unstyled.
- **Focus is visible everywhere** (D9): a `:focus-visible` ring with ≥ 3:1 against both paper and
  navy. `outline: none` without a replacement ring is a defect and control C11 counts it.
- **Nothing is meaningful by colour alone** (D9): the coming-soon pill carries the word, the
  "fits Disney buses" pill carries the word, the language switch marks the current language with
  `aria-current` and not only with a background.
- The header comment of the stylesheet carries the contrast table of the tokens **in use**, each
  number measured with the WCAG relative-luminance formula and not estimated — the leva-01 pattern.
  It does not name the retired colours (D9/02).

The two Nunito files, the `--color-action` / `--color-action-text` / `--color-trust` /
`--color-surface` tokens and every rule that reads them are deleted, not commented out.

---

## 7. Pages and content

### 7.1 The rule for every sentence on the site

Three readers, from `Docs/market-notes.md`: the slower walker in a fast group (the largest and
least served segment), the temporary or permanent medical reason, and the senior — usually booked
by an adult child. D31 adds the fourth axis: the English site sells to the American and
international tourist, the Portuguese site to the Brazilian, and **a claim that is not a
differentiator for the reader of that culture does not appear in that culture.**

Concretely, what belongs where:

| Claim | `en-US` | `pt-BR` |
|---|---|---|
| delivered to your hotel, tested and charged | yes | yes |
| fits the Disney buses and the Skyliner | yes — it is the purchase criterion | yes |
| the Disney meet-and-greet, explained | yes | yes |
| clear prices, no deposit conversation | yes | yes |
| a Brazilian team in Orlando, service in Portuguese | **no** — it is not a differentiator for an American reader | yes, prominently |
| WhatsApp | only once a number exists (Q12) | yes, once a number exists (Q12) |

Everything above lives in resource **values** under shared keys (D6/02). No page grows a culture
`if`.

**No claim about the business enters without a source.** Insurance, licensing, "family owned",
"since 20xx", review counts, response times, delivery radius in miles — none of these are in any
decision or in `Docs/market-notes.md`, so none of them appear. The pages say what the fleet is, what
the price is, where we deliver and how the hand-over works.

### 7.2 The pages

**Home** (`/`, `/pt`) — hero over navy with the headline, the sub-headline and one primary button
to the catalog; the three-reader band (the existing `Home_Audience*` keys, rewritten to speak to
the three profiles above); the product strip reading from the database — three bookable products
with "from US$ X/day" and four coming-soon cards; the four steps of how it works; and one band that
carries the culture difference of D31 (`Home_Team*`). The placeholder hero copy of leva 01 is
replaced, not adjusted.

**Rentals** (`/rentals`, `/pt/rentals`) — unchanged in structure: the intro, then one section per
category with its cards. Categories keep their order (scooters, wheelchair, strollers), and the
stroller section carries the coming-soon explanation once, above its cards, so four pills do not
have to each explain themselves.

**Product detail** (`/rentals/{slug}`) — unchanged in structure for a bookable product: name,
tagline, badge when `FitsDisneyTransport == true`, illustration, specs list (only the rows that
have a value), highlights, description, price table, add-ons, and the disabled booking button with
its note. For a coming-soon product: name, tagline, illustration, highlights, description, the
coming-soon pill, and nothing else.

**Delivery areas** (`/delivery-areas`, `/pt/delivery-areas`) — **new**. One section per active zone,
in `SortOrder`: the zone name, the hand-over instructions rendered from the database through
`RichText`, and the example locations as a plain list under a sentence that says they are examples
of hotels in that zone and not a closed list. The Disney zone leads, because its meet-and-greet is
the rule nobody expects (`Docs/market-notes.md`: ScooterBug holds the Bell Services exclusivity).
The delivery **fee** appears or does not according to §11 K1. The page gets a nav entry
(`Nav_DeliveryAreas`) and a footer link.

**How it works** (`/how-it-works`) — the four steps expanded with what actually happens, plus the
Disney meet-and-greet block it already renders from the zone. A link to the new delivery-areas page
replaces the temptation to repeat the zone list here.

**FAQ** (`/faq`) — rewritten for the real fleet and extended to ten questions. The candidates,
in order of what a visitor actually asks: how delivery works at a Disney resort; what happens at a
non-Disney hotel; whether the scooter fits the buses and the Skyliner; how long the battery lasts
and where to charge it; what happens if it breaks; who may drive one (age, licence); whether
strollers are available (the coming-soon answer); how to change or cancel (**answer: written from
the assumption in force for Q5 and marked as such, or the question is left out — the agent does not
invent a policy**); what is included in the price; how to reach a human. Any question whose honest
answer depends on an unanswered open question is **left out** rather than answered from
imagination. The page renders every `Faq_Q<n>` key present in the resource file — the count is not
hard-coded — and a test proves the page and the resource file agree.

**Contact** (`/contact`) — the company data of D28 now that it is real: legal name *Ronatrip Tours
& Travel*, address *7362 Futures Dr, Ste 2, Orlando, FL 32819*. Phone, WhatsApp, e-mail and hours
stay `TODO-` (Q12) and keep rendering through `_CompanyValue`, which already shows a marked value as
visibly unfinished. The `wa.me` button keeps its existing guard and therefore does not appear.

**Terms and Privacy** (`/terms`, `/privacy`) — the draft banner **stays**. Q5 is open, there is no
waiver text, and a legal page written from imagination is worse than a page that says it is a
draft. Their body copy is left alone except for the palette.

### 7.3 Resource keys

New keys follow the existing families (`Nav_`, `Home_`, `Rentals_`, `Product_`, `Faq_`, `Footer_`)
and are **stable identifiers, never the English text** — the leva-01 rule. The new families are
`Delivery_` (the new page) and `Product_ComingSoon*`. Both `.resx` files gain and lose exactly the
same keys; the parity test and control C12 of leva 01 prove it.

---

## 8. Machine-readable plumbing

**`/sitemap.xml`** — a minimal endpoint next to `robots.txt` in `Api/SitemapEndpoints.cs`, one file
(control C10 keeps it that way). For each public address it emits both culture variants and the
`xhtml:link rel="alternate" hreflang="…"` pair plus `x-default` pointing at the English one, which
is exactly what `_Layout.cshtml` already emits per page. Products come from `CatalogQueries` with
`IsActive` filtered — the same read the pages do, so a hidden product cannot leak through a second
code path. `/admin`, `/healthz`, `/error/*` are absent. `lastmod` is omitted rather than faked:
there is no per-page modification date in the schema, and a fabricated one is worse than none.

**Structured data** — one partial, `Pages/Shared/_StructuredData.cshtml`, rendered from the layout,
emitting `application/ld+json` from a typed model built in `Application/StructuredData.cs`
(control C10 asserts the single entry point). It carries:

- `LocalBusiness` on every page: `name` (trade name), `legalName`, `address` from `CompanyOptions`,
  `url`, `inLanguage`. **Every field whose value is empty or starts with `TODO-` is omitted**
  (D7/02) — a test asserts no rendered page contains the marker.
- `Product` on a product detail page: `name`, `description`, `category`, `brand`, and `width` /
  `length` as `QuantitativeValue` in inches **only when known**. No `Offer`, no `aggregateRating`,
  no `review` — the site has none of the three (D7/02).

**`robots.txt` and `noindex` are untouched.** `Seo:AllowIndexing` stays `false`; phase 5 flips it.
A test already asserts both and keeps asserting them.

---

## 9. Tests

The suite grows; nothing existing is deleted. Tests that must change because a type changed
(`FitsDisneyTransport` becoming `bool?`) are updated, and the change is named in the report.

**Domain (`DomainTests.cs`)**

1. the transport badge is `null` when either dimension is unknown, and the existing two cases keep
   passing;
2. a bookable product with no price list is refused by the seeder;
3. a non-bookable product with no price list is accepted;
4. a non-bookable product that carries a price list is refused — the guard has two sides.

**Seeding (`SeedingTests.cs`)**

5. the seeded catalog has 7 products, 3 of them bookable and 4 not, 10 units, and every unit belongs
   to a bookable product;
6. every seeded product carries both cultures (exists; must keep passing);
7. no coming-soon product carries a pricing tier or an add-on link.

**Site behaviour (`SiteBehaviourTests.cs`)**

8. a coming-soon product page answers 200 and its body contains no `US$`;
9. the rentals page shows a price for every bookable product and none for a coming-soon one;
10. the delivery-areas page names every active zone, in both cultures;
11. `/sitemap.xml` is well-formed XML, lists both cultures of every public page, lists every active
    product, and lists no inactive one (proved by deactivating one in the test database);
12. no public page — in either culture — contains the string `TODO-`;
13. every public page has exactly one `<h1>`;
14. `Nothing_in_this_release_can_send_a_message_to_anybody` keeps passing, with its reach assertion
    intact.

**Rendered text (`RenderedTextTests.cs`)**

15. the address list grows to cover the new page and the new slugs in both cultures:
    `/delivery-areas`, `/pt/delivery-areas`, `/rentals/drive-scout-4`,
    `/pt/rentals/drive-scout-4`, `/rentals/single-stroller`, `/pt/contact`, `/pt/terms`,
    `/pt/privacy`. The reach assertion (≥ 20 keys read) stays.

**Localization (`LocalizationParityTests.cs`)**

16. unchanged, and it must stay green — same key set, no empty value.

**FAQ**

17. the page renders every `Faq_Q<n>` key that exists in the resource file, and the resource file
    has no `Faq_Q<n>` the page skips — the count lives in one place.

---

## 10. Visual check

**Before any item, in one block:** (1) `dotnet build` clean, then
`dotnet run --project src/OrlandoUp.Web`; (2) database: LocalDB `OrlandoUpDb` after the block of
§5.4; (3) user: anonymous, except items 9–10; (4) the `<meta name="generator">` value is recorded
and `+dirty` **is** the proof of a fresh build (K5/01); (5) proof of a change is a screenshot or a
re-navigation, never a text extractor in the same round.

| # | Do | Expect |
|---|---|---|
| 1 | open `https://localhost:7420/` | navy header, sun primary button with navy text, Bricolage headings; no orange anywhere |
| 2 | scroll the home page | three-reader band, three bookable cards with "from US$", four coming-soon cards without a price |
| 3 | click PT | `/pt`, Portuguese, and the team band says the Brazilian thing the English one does not |
| 4 | open `/rentals/drive-scout-4` | specs list shows width, length, range, max rider; no seat-width row; badge present |
| 5 | open `/rentals/single-stroller` | coming-soon pill, no price table, no add-ons, no booking button |
| 6 | open `/delivery-areas` and `/pt/delivery-areas` | four zones in order, Disney first with the meet-and-greet text from the database |
| 7 | keyboard only: Tab from the address bar | skip link first; every nav item, card and pill gets a visible focus ring on both navy and paper |
| 8 | width 375 px | no horizontal scroll; nav usable; the price table scrolls inside its own container |
| 9 | `/admin` after login | counts 7 / 10 / 10; the placeholder banner is **gone** |
| 10 | view source of `/rentals/drive-scout-4` | one `application/ld+json` block, `Product` without `offers`, no `TODO-` anywhere in the source |
| 11 | `/sitemap.xml` | valid XML, both cultures per page, the four coming-soon products present, nothing under `/admin` |
| 12 | `/robots.txt` | still `Disallow: /` |
| 13 | **Rod:** Lighthouse (Chrome DevTools), accessibility category, on `/` and `/rentals` | ≥ 95, the phase-2 gate of `Docs/roadmap.md` |

The result is written to `Docs/conferencia-leva-02.md`, one line per item with what was seen; an
item the agent cannot reach is listed with the reason, never omitted.

---

## 11. Open points for Rod, answered at P0

Each has a recommendation and a fallback the agent executes if Rod does not want to decide now.
None of them blocks step 0.

**K1 — Does the delivery-areas page publish a fee?** D27 left the delivery fee open (it belongs to
Q3): the seed carries US$ 0 for the three resort zones and US$ 25 for vacation homes, and Ronatrip
charges a flat US$ 30 today. The three answers: **(a)** publish the seed's fees as the list price,
the way D27 did for rentals; **(b)** publish no fee — the page describes zones and hand-over only,
and says the delivery is quoted with the booking; **(c)** publish a single flat US$ 30 to match what
Ronatrip charges today. *Recommendation: (b).* Free delivery is the loudest claim on the page and
the one hardest to walk back, and the number that would be published is the only one of the three
that nobody has confirmed. **Fallback if unanswered: (b).**

**K2 — Which wheelchair is it?** D26 says "2 Drive Medical wheelchairs" and stops there. The
manufacturer's published dimensions cannot be looked up without a model. **(a)** Rod reads the
label, the agent cites the manufacturer's specification the way D26 did for the scooters; **(b)** the
wheelchair ships bookable with its price list and **no** dimensions — the specs list simply omits
those rows and the transport badge does not appear, which is exactly what §4 made possible.
*Recommendation: (a) if the label is at hand today, otherwise (b).* **Fallback: (b)** — and Q13
records it.

**K3 — Same price for both scooters?** Both are 300 lb machines; the Spitfire EX is the smaller,
lighter, more portable one. **(a)** identical list, as §5.2 writes it; **(b)** the Spitfire a few
dollars a day cheaper — Rod gives the three numbers. *Recommendation: (a)* — a price difference the
site cannot justify in one sentence is a question at checkout, not a feature. **Fallback: (a).**

---

## 12. Controls

### 12.1 Files the front alters

**New:** `src/OrlandoUp.Web/Pages/DeliveryAreas.cshtml` and its page model;
`src/OrlandoUp.Web/Pages/Shared/_StructuredData.cshtml`;
`src/OrlandoUp.Web/Application/StructuredData.cs`; `src/OrlandoUp.Web/Api/SitemapEndpoints.cs`;
the migration files under `Infrastructure/Data/Migrations/`; the four font files and the updated
`wwwroot/fonts/OFL.txt`; new SVG tiles under `wwwroot/img/`;
`Docs/controles/public-site.tsv`; `Docs/conferencia-leva-02.md`;
`Docs/relatorio-leva-02-etapa-N.md`.

**Modified:** everything under `src/OrlandoUp.Web/` and `tests/OrlandoUp.Tests/` that §§4–9 name —
`site.css`, the two `.resx`, the public pages and their models, `Domain/Product.cs`,
`ProductConfiguration.cs`, `CatalogViews.cs`, `CatalogQueries.cs`, `CatalogSeedData.cs`,
`CatalogSeeder.cs`, `Program.cs` (the sitemap endpoint only), `appsettings.json` (the two company
fields D28 closes), `Pages/Admin/Index.cshtml.cs` (the placeholder banner condition), and the test
files listed in §9. **Deleted:** the two Nunito woff2 files.

**Modified, and only in this:** `Docs/controles/foundation.tsv` — the **label** of C16 only,
replacing the open question it names (Q9, now closed) with Q12, which is what keeps the four
markers alive. Its type, target, pattern and expected value do not change, and the control must
still read `sim` (exactly 4 markers remain after §7.2 fills two).
`Docs/fila-cc.md` — the State and Commit columns of this leva's line, nothing else.

**Negatives, by column of the diff:** `CLAUDE.md`, `Docs/decisions.md`, `Docs/architecture.md`,
`Docs/roadmap.md`, `Docs/open-questions.md`, `Docs/market-notes.md`, `Docs/backlog-conhecido.md`,
`Docs/protocolo-conversa.md`, `Docs/regras-de-controle.md`, `Docs/medir-controles.sh`,
`Docs/resumo-conversa-1.md`, `Docs/resumo-conversa-2.md`, `Docs/atrito-conversa-*.md`,
`Docs/spec-01-foundation.md`, `Docs/conferencia-leva-01.md`, `Docs/relatorio-leva-01-*.md`, this
spec, `.githooks/pre-commit`, `.gitattributes`, `.github/workflows/ci.yml` — untouched. A
contradiction found in one of them is reported in the plan, not fixed by the agent.

### 12.2 Invariants for `Docs/controles/public-site.tsv`

The exact command form is the agent's — it has read the code. The invariants are these, and every
rule of `Docs/regras-de-controle.md` applies, in particular rule 5 (every subtraction and every
zero carries a sibling proving the scan reaches), rule 6 (a threshold, never a cardinal, for
anything a later front will legitimately grow) and rule 3 (no comment in `src/` transcribes a form
a control searches for).

1. **C01** `nome-exato`: `DeliveryAreas.cshtml` exists in `src/OrlandoUp.Web/Pages`.
2. **C02 / C03** `nome-exato`: the heading font file and the body font file exist in
   `wwwroot/fonts` (exact names, because Windows ignores case and the App Service Linux does not).
3. **C04** the retired action hex occurs **0** times over `src/`, with **C05** as its reach: the
   sun token value occurs ≥ 1 time over `src/` → `sim`.
4. **C06** the retired heading family name occurs **0** times over `src/`, with **C07** as its
   reach: the new heading family name occurs ≥ 1 time → `sim`.
5. **C08** the culture flag of the layout is reached from exactly one file — a `cmd` listing the
   files under `Pages/` that read it, expected to print the shared layout's path and nothing else
   (the discriminating form of leva 01's C11).
6. **C09** the relation of rule 4: over `Pages/` excluding `Admin/`, lines carrying `asp-page=`
   minus lines carrying `asp-route-culture=` = **0**, with a reach sibling asserting the first
   count is ≥ 8. This is the control that D10/02 exists for.
7. **C10** the structured-data content type is written in exactly one file, and the sitemap address
   in exactly one file — `cmd`s that print the paths, with the paths in the labels.
8. **C11** `outline: *none*` without a replacement ring occurs **0** times in `site.css`, reach:
   `:focus-visible` occurs ≥ 1 time → `sim` (D9).
9. **C12** `dotnet build` exit code and `dotnet test` exit code are **not** repeated here — C14 and
   C15 of `foundation.tsv` already measure them for the whole solution, and a second control that
   measures the same thing turns red twice for one defect.

### 12.3 What STEP 0 measures, before altering any file

1. `dotnet --list-sdks` contains a `10.0.` line; `dotnet ef --version` prints a `10.` version;
   `sqllocaldb info` lists `MSSQLLocalDB`; the database `OrlandoUpDb` exists and `SELECT DB_NAME()`
   through the configured connection reads it. Any miss is a stop with the line measured.
2. `bash Docs/medir-controles.sh verificar Docs/controles/foundation.tsv` at HEAD — the 18 controls
   of leva 01, **all green before anything is touched**. A control already red at receipt is a stop:
   the leva must not inherit a defect and then be blamed for it.
3. The current row counts of the catalog tables (§5.4 step 2), recorded in the plan.
4. Whether the network reaches the font source: one `curl -sI` per family, recorded. A refusal here
   is not a stop at step 0 — it is what P2 exists for — but the plan must say which of the two it
   expects.
5. The proposal of `Docs/controles/public-site.tsv` with `bash Docs/medir-controles.sh medir` run at
   the initial HEAD: most values read `0`/`nao`, and that is the point — they must move.
6. The exact list of files under `Pages/` that today read the culture flag and the exact
   `asp-page` / `asp-route-culture` line counts, so C08 and C09 are written against the tree and
   not from memory (rule 8).
7. Any contradiction between this spec and the tree is reported in the plan; the spec is never
   obeyed against the measurement.

**Closing is two commits:** the content commit (code, tests, resources, fonts, controls,
conference), then the closing commit that writes that commit's hash into the `Commit` column of the
leva's line in `Docs/fila-cc.md`. No push from the agent.

---

## 13. Out of scope, and why

- **Any form** — contact, quote, "notify me when strollers arrive". Every public form needs the
  e-mail outbox and the recipient allow-list (`Docs/architecture.md` §6), which arrive with leva 03;
  a form without them sends real mail from a machine that was never configured to.
- **Booking, availability, checkout, Stripe** — leva 03, and it needs Q3–Q6.
- **Admin CRUD** — leva 04. The admin stays read-only so the schema is still cheap to change.
- **Photographs and generated images** — D3/02; a reversible adjustment once the images exist.
- **Opening the site to crawlers** — phase 5. `AllowIndexing` stays `false` and this leva does not
  touch the flag.
- **Reviews, ratings, testimonials** — there are none, and structured data that claims them is the
  fastest way to lose a rich result and a reputation.
- **A second delivery-fee model, tax, coupons** — phases 3 and 4; Q3 and Q4 are open.
- **Spanish** — `Docs/backlog-conhecido.md`.
