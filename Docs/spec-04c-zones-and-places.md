# Spec — Leva 04c: zones and places, and the hotel that is not on the list

**Date:** 2026-09-15, conversation 7. **Pair:** conversation 7 ↔ leva 04c. **Executor:** Claude Code,
strongest model — the leva writes four administration screens, changes the one place that prices a
delivery, and reworks the markup of a form the operator already uses.

**Language of this spec:** English (`Docs/decisions.md` D1). The control rules it relies on are in
`Docs/regras-de-controle.md` (twelve rules, in the repository — read them before §11.2). The
skeleton follows `Docs/spec-04b-fleet-batteries.md`.

**What this leva closes.** The system can promise equipment and cannot say **where**. Measured on
2026-09-15: the four delivery zones and the ten places inside them exist only in
`Infrastructure/Seeding/CatalogSeedData.cs:204`, nothing in `src/OrlandoUp.Web/Pages/Admin/` reads
`DeliveryZones` for writing, and the seeder refuses to run twice — so **the list of hotels a
customer picks from can only be changed by editing C# and rebuilding**. The consequence the
operator met on 2026-09-14, in items 4 and 11 of `Docs/conferencia-leva-03.md`: a guest at a hotel
that is not among the ten cannot be quoted at all, on either screen, because the place select is
`required` and there is no option that means *somewhere else*.

**Numbering.** `Docs/decisions.md` D33 fixes leva N ↔ roadmap phase N, never the order of
execution. `Docs/roadmap.md` puts *"catalog/zones/coupons/units CRUD"* in phase 4, so this is the
third front of phase 4 — **04c**. Leva 03b (Stripe, public booking form, e-mail, hold, refund)
keeps its number and stays blocked on Q6 and Q15; nothing in this leva waits on either.

**What it does NOT get.** No migration. `DeliveryZones`, `DeliveryZoneTranslations` and
`DeliveryLocations` have existed since `20260904233355_InitialCreate`, with every column this leva
writes. If the agent finds himself running `dotnet ef migrations add`, the design went wrong and it
is a stop with a report.

---

## 0. Execution surface

**Launcher sentence:** this spec is executed by the line of `Docs/fila-cc.md` dated 2026-09-15
whose description begins with *"LEVA 04C — ZONES AND PLACES"*. **Not "the `aguardando` line"** —
that line, by name.

**State of the tree on receipt.** Measured on 2026-09-15 by reading `.git/logs/HEAD` through the
file bridge (the bridge's virtual machine still does not mount the folder — the Windows update of
2026-09-08 — so `git` was not run by the reviewer this conversation):

- HEAD is **`6be7a01`**, message `docs: resumo da conversa 6`;
- it descends from `698c035` (`chore: leva 03 closed — queue line records the content commit
  53676c2`) and from `53676c2`, the content commit of leva 03;
- the line *"LEVA 03 — BOOKING CORE"* of `Docs/fila-cc.md` reads `concluido` and `53676c2`;
- **the commits carrying this spec come after `6be7a01`**, so HEAD at execution time is a
  descendant of it and not `6be7a01` itself. That is not divergence.

**What was NOT measured, and is therefore the agent's step 0, not an assertion of this spec:**
`git status --porcelain`, `git ls-files`, and `origin/main...main`. The reviewer had no shell in
the repository. The agent reports all three in the plan. **Any modified or untracked file outside
`scratchpad/` is a stop with a report.** CRLF warnings and a stale index are possibilities to
ignore, not a state this spec claims you will find. **The push is Rod's**, and there are unpushed
commits: the agent never pushes.

**Files the front ALTERS:** the closed list is §11.1. A file changed outside it is a stop with a
report, **without a cardinal** — the list discriminates, never the count.

**Files the front PRODUCES as a record** (authorised here, not a scope escape): `scratchpad/leva04c/plano.md`
(not committed), `Docs/relatorio-leva-04c-etapa-N.md` (committed before each approval),
`Docs/controles/zones-places.tsv`.

**Files the TOOL generates coupled:** none. No migration, so no snapshot, no Designer.

**Steps that need a human hand, with the exact command. Rod's shell is PowerShell 5.1 on Windows;
the commands below are written for it.**

1. **Rebuild and run**, because four screens are new and two are reworked:
   ```
   dotnet run --project src/OrlandoUp.Web
   ```
2. **Create the `other-hotel` zone in the real database, through the new screen.** The seed carries
   it for a fresh install and for the test host, and `CatalogSeeder` refuses to run over a seeded
   database — so Rod's `OrlandoUpDb` will not get the row from the seed. Creating it by hand **is**
   items 5 to 8 of the conference roteiro: the values are written there, once, and the screen is
   exercised by the act of using it.
3. **The visual conference is Rod's**, after P2, into `Docs/conferencia-leva-04c.md`. The agent
   never writes a result in that file.
4. **`git push` is Rod's**, after the closing commits.

---

## 1. What the leva delivers, in plain Portuguese of the counter

Hoje a lista de hotéis mora no código. Se um cliente está hospedado num hotel que não está nela, o
atendente não consegue lançar a reserva — o campo **"Local de entrega"** é obrigatório e não existe
opção que queira dizer *outro lugar*. E o turista que abre o site e não acha o hotel dele
simplesmente vai embora.

Depois desta leva existem duas telas novas na administração: **Zonas** e **Locais**. Na primeira o
operador cria e edita as regiões de entrega — o nome que o cliente lê nas duas línguas, a taxa de
entrega, a alíquota de imposto, como o equipamento troca de mãos e se a zona está ativa. Na segunda
ele cria e edita os hotéis de cada zona. Nenhuma das duas apaga nada: zona e local saem de
circulação desmarcando **"Ativo"**, porque reserva antiga aponta para eles e um registro que some
leva a reserva junto.

Com as telas no ar, o operador cria a zona **"Outro hotel em Orlando ou Kissimmee"**, e ela nasce
sem lista de hotéis — que é exatamente o que o seletor já sabe tratar: opção de zona sem lista pede
**endereço digitado**. A partir daí o atendente lança a reserva de qualquer hotel, e o turista
escolhe essa opção em `/book` e **digita o endereço ali mesmo**, que viaja junto da consulta.

E a taxa de entrega deixa de ser um número fixo por zona: continua nascendo da zona, e o lançamento
da equipe pode **sobrescrevê-la naquela reserva** — em branco usa a da zona, um valor digitado vale
aquele valor, inclusive zero.

**O que isto NÃO é.** Não é regra de distância: ninguém mede milhas, e nenhuma taxa é calculada a
partir de endereço. Não é cupom nem promoção com data — a entrega gratuita de hoje é a taxa da zona
valendo zero, editável na tela, e nada no site anuncia que ela é temporária. Não é a atribuição de
unidade física à reserva, que é a outra metade da fase 4. E não é o formulário público de reserva,
que é a leva 03b: `/book` continua sendo consulta de disponibilidade e preço, sem pagamento e sem
gravar nada.

---

## 2. The decisions

The three business decisions were asked of Rod on 2026-09-15, each with its scene, before any block
of this spec existed. They go into `Docs/decisions.md` as **D43, D44 and D45**; the rest are
decisions of this spec and are numbered `Dn/04c`.

**D43/global — the delivery fee is a property of the zone, editable on the zone screen, and the
staff booking screen may override it for one booking.** [operador] The scene Rod gave: *"International
Drive, a entrega será gratuita (por tempo limitado); regiões com distância superior a 15 milhas
teriam um custo de $25"* — and, in the same breath, that he does **not** want a distance rule
because it would generate too much work. What he asked for is flexibility at two moments: the zone's
standing price, and the price of the delivery he is agreeing to right now on WhatsApp. Both are one
number in one place: the zone's, and the booking's copy of it.

**D44/global — the zone `other-hotel` is born with a fee of US$ 0, and it covers Orlando and
Kissimmee.** [operador] *"Se é em Orlando ou Kissimmee, qualquer hotel pode ser atendido."* Zero is
today's promise and not a permanent one: it is a row in a table with an edit screen. Nothing on the
public site says the free delivery is temporary — announcing a promotion is a content decision with
its own front (§12).

**D45/global — the customer types the hotel address on `/book` itself.** [operador] He chose this
over the cheaper alternative (offer the option, ask for the address only at booking time), so that
the booking form of leva 03b is born already filled. The page is a GET whose state is the query
string, so the address travels in the URL and nothing is stored — which is also why §12 says this
leva records no customer data.

**D1/04c — zones and places get two pairs of screens of their own** (`/admin/zones`,
`/admin/zones/create`, `/admin/zones/edit/{id}`, and the same three for `/admin/locations`),
mirroring `Pages/Admin/Units/` and `Pages/Admin/Batteries/` file for file. Not a nested editor
inside the zone page. Reason: the nested editor is the shape of `Pages/Admin/Products/Edit`, which
is the largest and most fragile screen in the tree (11 853 bytes of markup, price bands and two
translations in one post); places have a parent and five fields, which is exactly the Units shape.

**D2/04c — no screen deletes a zone or a place; the flag is `IsActive`.** A booking's
`DeliveryZoneId` is a required foreign key `[V, Domain/Booking.cs:57]` and
`DeliveryZoneConfiguration.cs:31` declares `OnDelete(DeleteBehavior.Restrict)` for locations, so a
delete would either be refused by the database or take a reservation's history with it. The screens
say *"Ativo"* and nothing offers a delete button. This is also a control (§11.2 C01).

**D3/04c — "other hotel" is NOT a new concept in code.** It is an active zone with no active
location, and `CreateModel.PlaceChoicesAsync` already turns exactly that into an option keyed
`Z{id}` with `NeedsAddress = true` `[V, Pages/Admin/Bookings/Create.cshtml.cs:323-327]`. No enum
member is added, no zone is looked up by its code, and no page branches on `ZoneKind.Other`. What
the leva adds is the ability to create such a zone, and the public page's half of the address rule.

**D4/04c — the address becomes required on `/book` whenever the chosen option asks for one, and
the field is always visible.** The public site has no JavaScript and this leva does not introduce
any `[V, Pages/Book/Index.cshtml.cs:22 — "There is no JavaScript and no payment"]`, so the field
cannot appear on selection. It is rendered under the select with a help line saying it is needed
only for the options that ask for an address. The refusal is the server's, in the same shape as the
admin screen's `Admin_ErrorAddressRequired`.

**D5/04c — the override is a nullable decimal all the way down: blank means the zone's fee, a typed
value means that value, and zero typed means zero.** Absence never coalesces to zero (D15, and
control C17 of `foundation.tsv` forbids `?? 0` in `src` outright). The single expression that
resolves it is `deliveryFeeOverride ?? zone.DeliveryFee`, inside `QuoteBuilder`, and it is the only
one.

**D6/04c — an overridden fee is written into the first line of the booking's history.** The booking
freezes the number `[V, Domain/Booking.cs:171]`, and a total nobody can explain later is a total the
operator will not trust. The `Created` event says so in words; `BookingWriter` already composes that
sentence `[V, Infrastructure/Data/BookingWriter.cs:168-174]`.

**D7/04c — the equipment table of the staff booking screen becomes a grid of cards, and the form
adopts the `.field` classes the stylesheet already carries.** `site.css` has **zero** `@media`
rules — the whole site is fluid — and this leva does not add the first one: the cards are a grid of
`repeat(auto-fit, minmax(…, 1fr))`, which is one column at 375 px and several on a desktop without
a breakpoint. The posted field names do not change, which is what keeps the existing tests honest
(§9).

**D8/04c — the seed gains the `other-hotel` zone, and the tests that count zones move with it.**
`SeedingTests` asserts four zones and ten locations `[V, tests/OrlandoUp.Tests/SeedingTests.cs:43-44]`;
after this leva it is five and ten. A fresh install and the test host get the zone; Rod's database
gets it by hand (§0, step 2), and that asymmetry is stated here so nobody reads the passing suite as
proof that his database has it.

---

## 3. The measured terrain

Everything `[V]` on 2026-09-15, read file by file through the file bridge (`device_stage_files` +
`grep`). One line per file; no plural subjects.

| File | The fact | How it was reached |
|---|---|---|
| `Domain/DeliveryZone.cs` | The zone carries `Code`, `Kind`, `DeliveryFee`, `HandoverMode`, `SalesTaxRate`, `IsActive`, `SortOrder`, `Translations`, `Locations`. `Code` is documented as *"Stable identifier used in code, never shown to the customer."* | whole file read |
| `Domain/DeliveryLocation.cs` | The place carries `ZoneId`, `Name`, `Address`, `Notes`, `IsActive`, `SortOrder`, and `Name` is documented as *"The name as the customer would recognise it; it is not translated."* | whole file read |
| `Domain/DeliveryZoneTranslation.cs` | One row per culture, with `Name` and a nullable `Instructions` documented as *"Markdown shown to the customer, rendered through the rich-text gate."* | whole file read |
| `Domain/Enums.cs` | `ZoneKind` has five members (`DisneyResort`, `UniversalResort`, `HotelOrResort`, `VacationHome`, `Other = 9`); `HandoverMode` has three (`MeetAndGreet`, `FrontDesk`, `Doorstep`). | whole file read |
| `Infrastructure/Data/Configurations/DeliveryZoneConfiguration.cs` | `Code` is `HasMaxLength(40)`, required, with a **unique index**; `SalesTaxRate` is `HasPrecision(6, 4)` **with `HasDefaultValue(0m)`**; locations cascade nothing — `OnDelete(DeleteBehavior.Restrict)`. | whole file read |
| `Infrastructure/Data/Configurations/DeliveryLocationConfiguration.cs` | `Name` is `HasMaxLength(160)`, required, and `(ZoneId, Name)` is a **unique index**; `Address` 300, `Notes` 400. | whole file read |
| `Infrastructure/Data/Configurations/DeliveryZoneTranslationConfiguration.cs` | `(ZoneId, Culture)` is a unique index; `Name` is `HasMaxLength(120)`, required. | whole file read |
| `Infrastructure/Seeding/CatalogSeedData.cs` | Four zones at line 204: `disney-resorts` (fee 0, MeetAndGreet, six places), `universal-resorts` (0, MeetAndGreet, two), `idrive-lbv-hotels` (0, FrontDesk, two), `vacation-homes` (**25**, Doorstep, **`[]` — no place at all**). | `grep -n "Zone\|Location"` then lines 200-252 read |
| `Infrastructure/Seeding/CatalogSeeder.cs` | Lines 171-208 write the zones, their translations and their places from that array, and `SaveChangesAsync` is called once for the whole catalog. | lines 160-220 read |
| `Pages/Admin/Bookings/Create.cshtml.cs` | `PlaceChoicesAsync` (line 300) is **static and shared with the public page**: a zone whose active places are zero becomes `new PlaceChoice($"Z{zone.Id}", zoneName, zoneName, true)` (line 325); otherwise every place becomes `$"L{location.Id}"` with `NeedsAddress` false (line 332). | whole file read |
| `Pages/Admin/Bookings/Create.cshtml.cs` | `TryReadPlace` (line 227) hands back `needsAddress`, and `OnPostAsync` refuses with `Admin_ErrorAddressRequired` both when the option is unknown and when the address is blank (lines 129-137). | whole file read |
| `Pages/Book/Index.cshtml.cs` | Line 297 calls the same `CreateModel.PlaceChoicesAsync`; its own `TryReadPlace` (line 235) returns **only the zone id and ignores `NeedsAddress`**, and the model has **no `Address` property**. | whole file read |
| `Pages/Book/Index.cshtml` | The place select (lines 74-88) is `required`, grouped by `place.Group`, with an empty first option; **there is no address input anywhere in the form**. | whole file read |
| `Pages/Admin/Bookings/Create.cshtml` | The screen's labels read `Admin_FieldPlace`, `Admin_FieldAddress`, `Admin_FieldDeliveryNotes`, `Admin_FieldStaffNotes`; all four are `<input>`, inside bare `<p>` tags, and **none of them uses the `.field` class**. | whole file read |
| `Pages/Admin/Bookings/Create.cshtml` | The equipment block (lines 105-157) is a `<table>` inside `<div class="table-scroll">`, with five columns and one row per bookable product; the posted names are `Quantity[id]`, `ExtraBatteries[id]`, `ExtraBatteryPerDay[id]`, `AddOns[id]`. | whole file read |
| `Infrastructure/Data/QuoteBuilder.cs` | Line 117 is the only construction of a priced request: `Quote.For(new QuoteRequest(start, end, zone.DeliveryFee, zone.SalesTaxRate, lines))`, and the class documents itself as *"the only place in the application allowed to read a price out of the catalog."* | whole file read |
| `Domain/Quote.cs` | `QuoteRequest` carries `DeliveryFee` and `TaxRate` as plain decimals; line 166 refuses a negative fee with `QuoteProblem.NegativeAmount`; line 277 makes the fee part of the taxable base. | whole file read |
| `Domain/Booking.cs` | `DeliveryZoneId` is required (line 57), `DeliveryLocationId` is nullable (line 62), `Address` is *"Required by validation when no curated location was chosen"* (line 67), and `DeliveryFee` is copied from the quote at line 171. | lines 40-200 read |
| `Domain/Booking.cs` | `StaffBookingDetails` (line 246) is a fourteen-member record and the parameter object every staff booking travels in. | lines 240-275 read |
| `Infrastructure/Data/BookingWriter.cs` | `CreateByStaffAsync` calls `_quotes.BuildAsync(asked, details.DeliveryZoneId, …)` at line 88, and writes the `Created` event at lines 168-174 with two possible sentences (plain, and overbooked). | whole file read |
| `Pages/DeliveryAreas.cshtml.cs` | The public page asks `CatalogQueries.ActiveZonesAsync` for the zones and `ActiveLocationsByZoneAsync` for their places. | whole file read |
| `Infrastructure/Data/CatalogQueries.cs` | `ActiveZonesAsync` (line 132) returns **every active zone that has a translation for the culture**, and skips silently (line 149) one that has none. | lines 132-162 read |
| `Pages/Shared/_AdminLayout.cshtml` | The navigation has **seven** destinations, written one `<a>` per line at lines 63-71, with a comment explaining that the block lives here and not in a partial (control C10 of `public-site.tsv`). | whole file read |
| `wwwroot/css/site.css` | `.field`, `.field select/textarea`, `.field--check`, `.field__help`, `.tier-row`, `.table-scroll` and `.card` all exist; **`grep -c "media" site.css` answers `0`** — the file has no media query at all. | `grep -c media`, then lines 740-880 read |
| `Pages/Admin/Batteries/Edit.cshtml.cs` | The CRUD shape to copy: `CatalogWriter` + `AuditTrail.Record(...)` + `IStringLocalizer<SharedResource>`, a `FindRefusalAsync` that returns a translated sentence, `Saved`/`Problem` flags, and `DbUpdateException` caught and turned into `Admin_ErrorBatteryTagTaken`. | whole file read |
| `Infrastructure/Data/CatalogWriter.cs` | It holds `FindBatteryAsync`, `ScooterModelsAsync`, `BatteryTagIsTakenAsync`, `AddBattery`, `SaveAsync` and `BeginAsync`, and documents `BeginAsync` as the transaction a creation needs so the audit line can name a key the database has not handed out yet. **It has no member that touches a zone or a place.** | whole file read |
| `tests/OrlandoUp.Tests/SeedingTests.cs` | Line 43 asserts `Assert.Equal(4, await db.DeliveryZones.CountAsync())` and line 44 `Assert.Equal(10, await db.DeliveryLocations.CountAsync())`. | lines 39-52 read |
| `tests/OrlandoUp.Tests/AdminCrudTests.cs` | `Every_enum_member_a_screen_names_has_a_key_in_both_cultures` builds its list from eight `Enum.GetNames<…>()` calls (lines 636-643) and closes with `Assert.Equal(32, keys.Count)` at line 650. | lines 600-660 read |
| `tests/OrlandoUp.Tests/AdminCrudTests.cs` | `No_visibility_flag_declares_a_store_default…` (line 115) already includes `typeof(DeliveryZone)` and `typeof(DeliveryLocation)` among four carriers and closes with `Assert.Equal(4, carriers.Length)`. | lines 105-175 read |
| `tests/OrlandoUp.Tests/AdminCrudTests.cs` | `BookingFormAsync` (line 2237) reads the rendered form and then `form.Set(...)` on `Place`, `Quantity[id]`, `ExtraBatteries[id]`, `ExtraBatteryPerDay[id]`, `AddOns[id]` — so the **names**, not the markup, are what it depends on. | lines 2230-2280 read |
| `tests/OrlandoUp.Tests/FormFields.cs` | `ReadFrom` gathers `input`, `textarea` and `select` of the **last** form in the document, because `_AdminLayout` puts a sign-out form in the header. | lines 1-60 read |
| `tests/OrlandoUp.Tests/SiteBehaviourTests.cs` | The `/book` cases pin `_place` to `db.DeliveryLocations.OrderBy(SortOrder).First()` (line 346) — an `L` option, which never asks for an address. | lines 335-380 read |
| `tests/OrlandoUp.Tests/LocalizationParityTests.cs` | It compares the two `.resx` files key by key and **cannot see a key built by interpolation**, which is why the `AdminCrudTests` list above exists. | whole file read |
| `Docs/controles/admin-catalog.tsv` | C05 is the relation *POST handlers minus audit/writer calls* over `Pages/Admin`, expected **2**; C06 is its reach, `-ge 9`. | whole file read |
| `Docs/controles/public-site.tsv` | C16 forbids **eleven** catalog identifiers anywhere in `src` outside `CatalogSeedData.cs`, expected `0`; the list includes `disney-resorts`, `universal-resorts`, `idrive-lbv-hotels`, `vacation-homes`. C17 is its reach, `-ge 7` inside the seed file. | whole file read |
| `Docs/controles/foundation.tsv` | C17 forbids `?? 0` and `GetValueOrDefault(` anywhere in `src`, expected `0`; C18 is its reach. | whole file read |
| `Docs/controles/booking-core.tsv` | C01 forbids `PricingTier`, `CatalogQueries`, `[Zz]one[.]DeliveryFee` and `db[.]AddOns` in the booking **detail** page, expected `0`; C02 is its reach, over `QuoteBuilder.cs`. | lines 1-6 read |

`[H]` **The suite never runs a migration** — `SiteFactory` builds the schema with
`EnsureCreatedAsync` — inherited from `Docs/spec-04b-fleet-batteries.md` EMENDA-04B-02 B1 and not
re-measured this conversation. It matters here only as a reason not to write a test that claims the
database has the `other-hotel` row.

### 3.1 The screen's vocabulary, next to the code's

The conference roteiro and this spec name what the **interface** shows. The two vocabularies differ
on purpose:

| The screen says (pt-BR) | The code calls it |
|---|---|
| Local de entrega | `Place`, resolved from `PlaceChoice.Value` |
| Endereço | `Address` |
| Zona | `DeliveryZone` |
| Ativo | `IsActive` |
| Ordem | `SortOrder` |
| Taxa de entrega | `DeliveryZone.DeliveryFee` |
| Taxa desta reserva | `DeliveryFeeOverride` |
| Como entregamos | `HandoverMode` |
| Tipo | `ZoneKind` |

---

## 4. The two new pairs of screens

### 4.1 Zones — `/admin/zones`, `/admin/zones/create`, `/admin/zones/edit/{id}`

The list shows, one row per zone, in `SortOrder`: **Nome** (the translation of the current UI
culture, falling back through `TranslationPicker`), **Tipo**, **Taxa de entrega**, **Como
entregamos**, **Locais** (the count of active places), **Ativo**, and the **Editar** link. An
inactive zone is marked the way an inactive battery is, with `class="todo"`.

Create and edit carry the same fields: `Code` (create only — it is the stable identifier, and
changing it later would break the seed's idempotence and C16's list), `ZoneKind`, `DeliveryFee`,
`HandoverMode`, `SalesTaxRate`, `SortOrder`, `IsActive`, and **two blocks of text**, English and
Portuguese, each with **Nome** and **Instruções**. The two blocks reuse the headings the product
editor already uses: `Admin_BlockEnglish` and `Admin_BlockPortuguese`.

Refusals, each a resource key and never a sentence in code:

| When | Key |
|---|---|
| Code empty or over 40 characters | `Admin_ErrorZoneCodeRequired` |
| Code already on another zone (checked before the save, and the `DbUpdateException` caught anyway) | `Admin_ErrorZoneCodeTaken` |
| English name empty | `Admin_ErrorEnglishNameRequired` (**exists**) |
| Fee below zero | `Admin_ErrorAmountNegative` (**exists**) |
| Tax rate below zero or at least 1 | `Admin_ErrorTaxRateRange` |
| A number that did not parse | `Admin_ErrorNumberFormat` (**exists**) |

A culture block whose name **and** instructions are blank removes that translation row, the way
`CatalogWriter.ApplyTranslation` does for a product — and for the same reason: a row of empty
strings makes the public page believe a translation exists. **The English block may not be emptied**;
`ActiveZonesAsync` skips a zone with no translation for the culture and falls back to English, so a
zone with no English row can vanish from the public page without anything failing.

### 4.2 Places — `/admin/locations`, `/admin/locations/create`, `/admin/locations/edit/{id}`

The list shows **Zona**, **Nome**, **Endereço**, **Ordem**, **Ativo**, **Editar**, ordered by the
zone's `SortOrder` then the place's. Create and edit carry `ZoneId` (a select of every zone, active
or not, labelled by its translated name), `Name`, `Address`, `Notes`, `SortOrder`, `IsActive`.

| When | Key |
|---|---|
| Name empty or over 160 characters | `Admin_ErrorLocationNameRequired` |
| The chosen zone does not exist | `Admin_ErrorZoneRequired` |
| `(ZoneId, Name)` already taken — checked, and the `DbUpdateException` caught anyway | `Admin_ErrorLocationNameTaken` |

### 4.3 What both screens do to the record

Every POST handler of the four new screens **records one audit line** through `AuditTrail.Record`,
with `AuditAction.Created`, `Updated`, `Deactivated` or `Reactivated` chosen the way
`Batteries/Edit.cshtml.cs:113-121` chooses it. This is not a matter of taste: C05 of
`admin-catalog.tsv` is the relation *POST handlers minus audit or booking-writer calls*, expected
**2**, and four new handlers without four new `Record` calls turns that control red (§11.3).

A creation needs the transaction `CatalogWriter.BeginAsync` already exists for — the audit line has
to name a key the database has not handed out yet.

### 4.4 The navigation

`_AdminLayout.cshtml` gains **two** destinations, one `<a>` per line, between Baterias and Reservas:
`Admin_ZonesTitle` and `Admin_LocationsTitle`. The navigation goes from seven to **nine**, and the
first item of the conference roteiro counts them — it is the proof of a fresh build.

---

## 5. The public page and the address

`Pages/Book/Index.cshtml.cs` gains `Address` as a bound GET property, and its `TryReadPlace` gains
the `needsAddress` out-parameter the admin twin already has. After the place is resolved and before
availability is asked:

- the option is unknown → `Book_ErrorPlaceRequired` (**exists**);
- the option asks for an address and `Address` is blank → **`Book_ErrorAddressRequired`** (new);
- otherwise the quote proceeds exactly as today.

`Pages/Book/Index.cshtml` gains, under the place select, one text input named `Address` with the
label `Book_FieldAddress` and a help line `Book_FieldAddressHelp` saying it is needed only for the
options that ask for one. It is **not** `required` in the markup: the browser cannot know which
option asks, and a `required` attribute on a field that is usually irrelevant is a wall.

**This rule reaches `vacation-homes` too**, which is an existing zone with no places and therefore
already a `Z` option. That is correct and intended: a delivery to a rental house without an address
was never answerable either.

Nothing about the address is stored. `/book` persists nothing and this leva does not change that.

---

## 6. The delivery fee, and the override

**Three files, one expression.**

1. `Infrastructure/Data/QuoteBuilder.cs` — `BuildAsync` gains a parameter
   `decimal? deliveryFeeOverride = null`, declared **last** and defaulted, so no existing call site
   changes. Line 117 becomes
   `Quote.For(new QuoteRequest(start, end, deliveryFeeOverride ?? zone.DeliveryFee, zone.SalesTaxRate, lines))`.
   **It is never written `?? 0`** — control C17 of `foundation.tsv` forbids that shape anywhere in
   `src`, and the shape is also wrong: a blank field is not a free delivery.
2. `Domain/Booking.cs` — `StaffBookingDetails` gains `decimal? DeliveryFeeOverride` as its **last**
   member. Every construction of the record is a compile error until it is passed, which is the
   point of the parameter object.
3. `Infrastructure/Data/BookingWriter.cs` — `CreateByStaffAsync` passes
   `details.DeliveryFeeOverride` through to `BuildAsync`, and the `Created` event's sentence gains a
   clause when it is not null: the fee that was charged, and that it was set by hand. Four sentences
   now exist (plain, overbooked, overridden, both), composed rather than branched four ways.

**The screen.** `/admin/bookings/create` gains one field in the delivery block, under **Endereço**:
label `Admin_FieldDeliveryFeeOverride` (*"Taxa desta reserva (US$)"*), `type="number"`,
`step="0.01"`, `min="0"`, **empty by default**, with the help line
`Admin_FieldDeliveryFeeOverrideHelp`: *"Deixe em branco para usar a taxa da zona."* Blank binds to
null. The public page never sends it and never could — `Book/Index` has no such property.

**What the quote already refuses, and this leva does not re-implement:** a negative fee is
`QuoteProblem.NegativeAmount` in the domain `[V, Domain/Quote.cs:166]`, translated by the page's
existing `KeyFor` switch to `Admin_ErrorNegativeAmount`. The screen does not validate the number a
second time.

---

## 7. A2, A3, A5 — the form the operator already uses

The three findings of the leva-03 conference that are not the address:

**A2 — the long fields.** `Address`, `DeliveryNotes` and `StaffNotes` become `<textarea rows="3">`.
`FormFields.ReadFrom` already gathers textarea content `[V, tests/OrlandoUp.Tests/FormFields.cs:22]`,
so the tests that read and repost the form keep working, and the posted names do not change.

**A3 — the alignment.** Every `<p><label>…<input></p>` of `Create.cshtml` becomes
`<div class="field">`, and the overbooking checkbox becomes `<div class="field--check">` with its
help line in `<p class="field__help">`. These classes exist and are what the settings and battery
screens already look like; the blocks **Cliente** and **Entrega** stop being two different widths
because `.field` carries `max-width: 24rem`.

**A5 — the equipment table at 375 px.** The `<table>` inside `.table-scroll` becomes
`<div class="line-cards">` holding one `<fieldset class="line-card">` per bookable product, the
product name as its `<legend>`, and the four controls as ordinary labelled fields inside. The
add-ons stay a list of checkboxes. One new rule in `site.css`:

```
.line-cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(17rem, 1fr)); gap: var(--space-5); }
```

No `@media`. At 375 px the grid is one column and nothing scrolls sideways; on a desktop it is two
or three cards per row.

**What must not change:** the `name` attribute of every posted field —
`Quantity[{id}]`, `ExtraBatteries[{id}]`, `ExtraBatteryPerDay[{id}]`, `AddOns[{id}]`, `Place`,
`Address`, `DeliveryNotes`, `StaffNotes`, `Overbook`. Each input gains a unique `id` and a real
`<label for=…>`, replacing the `aria-label` the table cells used. The checkbox-plus-hidden order
stays as it is: **the checkbox first** (inherited rule, `Docs/backlog-conhecido.md`).

---

## 8. The seed

`CatalogSeedData.Zones` gains a fifth entry, after `vacation-homes`:

- `Code` **`other-hotel`**, `Kind` `ZoneKind.Other`, `DeliveryFee` **`0m`**, `HandoverMode`
  `HandoverMode.FrontDesk`, `SortOrder` **`5`**, and **no place at all** (`[]`);
- English: name *"Any other hotel in Orlando or Kissimmee"*; instructions *"If your hotel is not on
  the list, choose this option and type the address. We deliver to any hotel in the Orlando and
  Kissimmee area, and we leave the equipment with the front desk under the name on the booking
  unless we agree otherwise."*
- Portuguese: name *"Outro hotel em Orlando ou Kissimmee"*; instructions *"Se o seu hotel não está
  na lista, escolha esta opção e digite o endereço. Atendemos qualquer hotel da região de Orlando e
  Kissimmee, e deixamos o equipamento na recepção no nome da reserva, salvo se combinarmos
  diferente."*

**Two consequences, both intended:**

1. `/delivery-areas` will list the new zone, in both languages, because `ActiveZonesAsync` returns
   every active zone with a translation. That is D44: a guest who does not find his hotel reads
   that we serve it.
2. `other-hotel` becomes the **twelfth** catalog identifier forbidden in `src` outside
   `CatalogSeedData.cs`. C16 of `public-site.tsv` is a permanent control this leva moves: the agent
   **adds the identifier to C16's pattern and to C17's, in the same commit**, and does not create a
   parallel line (§11.3).

---

## 9. Tests

The suite is `tests/OrlandoUp.Tests`, on SQLite in memory with `EnsureCreatedAsync`; the staff
client comes from `_factory.CreateStaffClient()` and the write helper from `FormPoster`. **Every
absence assertion states a presence in the same method**, and the fixture must hold the row the test
reads.

1. **A zone created through the screen is readable, and its audit line names it.** Post the create
   form; read the row back; assert the `AuditEntry` of the new key exists with `AuditAction.Created`.
2. **A place cannot repeat a name inside its zone, and can repeat it across zones.** Post the same
   name twice into one zone → the page answers with `Admin_ErrorLocationNameTaken` and the count did
   not move; post it into another zone → written. (The presence half is the second post.)
3. **Deactivating a zone removes it from both selects and from `/delivery-areas`, and does not
   delete it.** Assert the row still exists with `IsActive == false` — a "does not contain" alone
   would pass on a page that errored.
4. **A zone with no active place becomes an option that asks for an address.** Arrange a zone with
   no place; read `/book`; assert its translated name appears among the options; post the query with
   that option and a blank address → the page prints the text of `Book_ErrorAddressRequired`; repeat
   with an address → the page prints the availability answer. This is the test of D3/04c and D4/04c
   together, and without it the leva can be born right and rot on the next refactor.
5. **A curated place still needs no address.** The existing `/book` cases prove it and must stay
   green untouched; one new assertion states it explicitly, so that a stricter rule later cannot
   pass silently.
6. **The override changes the total and nothing else.** Two staff bookings on the same dates and
   zone, one with the field blank and one with `12.50`: the first freezes the zone's fee, the second
   freezes `12.50`, and `Subtotal`, `ExtraBatteriesTotal` and `AddOnsTotal` are equal in both.
7. **An override of zero is not an absence.** Post `0` against a zone whose fee is 25 → the booking
   freezes `0m` and the total is the rental alone. This is the test that distinguishes the two forms
   a coalescing bug cannot be distinguished by value otherwise.
8. **The overridden fee is in the first line of the history.** Assert the `BookingEvent` of type
   `Created` contains the amount; the presence half is the plain booking, whose event does not.
9. **The equipment fields survive the markup change.** `BookingFormAsync` reads the rendered form
   and posts it; one new assertion states that the form carries an input named `Quantity[{scoutId}]`
   before the post — otherwise a rendering mistake turns test 6 into a green nothing.
10. **The interpolated-key test grows.** Two `AddRange` lines (`ZoneKind` → `Admin_ZoneKind{name}`,
    `HandoverMode` → `Admin_Handover{name}`), and `Assert.Equal(32, keys.Count)` becomes
    **`Assert.Equal(40, keys.Count)`** — 32 + 5 + 3, and the agent reports the measured number
    rather than trusting this arithmetic.
11. **`SeedingTests` moves from four zones to five**, and the ten places stay ten.

**No external effect exists in this leva** — no e-mail, no Stripe, no blob, no webhook. Nothing to
neutralise, and the existing test that asserts neither is registered stays green.

---

## 10. Visual conference

**The roteiro is `Docs/conferencia-leva-04c.md`**, written with this spec and committed with it.
The result column is born blank and **Rod** fills it; the agent never writes a result there.

The preconditions live in one block before the first item and include the state of the instrument
(Chrome, `F12`, `Ctrl+Shift+M`, width **375**, zoom **100 %**, never *Fit to window* — it scales the
render and produces a horizontal bar by itself), the dates to use, and the proof of a fresh build:
**the administration navigation has nine destinations, not seven.**

The roteiro's items 5 to 8 are also **human step 2 of §0**: they are how the `other-hotel` zone gets
into Rod's database.

---

## 11. Controls

### 11.1 Files the front alters

**New (nine code files, six pages plus their models, and one control file):**

- `Pages/Admin/Zones/Index.cshtml` + `.cs`
- `Pages/Admin/Zones/Create.cshtml` + `.cs`
- `Pages/Admin/Zones/Edit.cshtml` + `.cs`
- `Pages/Admin/Locations/Index.cshtml` + `.cs`
- `Pages/Admin/Locations/Create.cshtml` + `.cs`
- `Pages/Admin/Locations/Edit.cshtml` + `.cs`
- `Docs/controles/zones-places.tsv`
- `Docs/relatorio-leva-04c-etapa-N.md` (one per stop)

**Modified, and only in this:**

- `Infrastructure/Data/CatalogWriter.cs` (the zone and place members: find, add, name-taken, the
  translation writer for a zone)
- `Infrastructure/Data/QuoteBuilder.cs` (the optional override parameter, and line 117)
- `Infrastructure/Data/BookingWriter.cs` (passes the override through; the `Created` sentence)
- `Domain/Booking.cs` (`StaffBookingDetails` gains its last member)
- `Infrastructure/Seeding/CatalogSeedData.cs` (the fifth zone)
- `Pages/Book/Index.cshtml` + `.cshtml.cs` (the address field and its refusal)
- `Pages/Admin/Bookings/Create.cshtml` + `.cshtml.cs` (the override field; `.field` classes; the
  cards)
- `Pages/Shared/_AdminLayout.cshtml` (two destinations)
- `wwwroot/css/site.css` (one rule, `.line-cards`)
- `Resources/SharedResource.resx` and `SharedResource.pt-BR.resx` (the new keys, both files)
- `tests/OrlandoUp.Tests/AdminCrudTests.cs`, `SiteBehaviourTests.cs`, `SeedingTests.cs`
- `Docs/controles/admin-catalog.tsv` (C05 re-measured, C06's threshold rebased)
- `Docs/controles/public-site.tsv` (C16 and C17 gain `other-hotel`)
- `Docs/architecture.md` (a dated note: the fee has a per-booking override)
- `Docs/fila-cc.md`, columns **Estado** and **Commit** of this leva's line only

**Negative, by column of the diff:** `Infrastructure/Data/Migrations/` untouched, every file of it,
including the snapshot. `Domain/DeliveryZone.cs`, `DeliveryLocation.cs`, `DeliveryZoneTranslation.cs`
and their three configurations untouched — the schema is already right. `Domain/Availability.cs`,
`Domain/Quote.cs`, `Domain/BookingRules.cs`, `Domain/BookingStatusRules.cs` and
`Infrastructure/Data/AvailabilityQueries.cs` untouched — no rule of availability changes.
`Pages/Admin/Products/`, `Pages/Admin/Units/`, `Pages/Admin/Batteries/`, `Pages/Admin/Settings/`
untouched. `CLAUDE.md` untouched. `Program.cs` untouched — Razor Pages discovers pages; if a
registration turns out to be needed, it is a stop with a report, not a quiet line.

### 11.2 Invariants for `Docs/controles/zones-places.tsv`

The exact form of each command is the agent's — he read the code. **Every rule of
`Docs/regras-de-controle.md` applies**, and three of them bite here: rule 8 (substring —
`DeliveryFeeOverride` contains `DeliveryFee`, so a pattern meant for one matches the other), rule 10
(`; true` and a default in the expansion), rule 11 (never `git grep`).

1. **No administration screen deletes a zone or a place.** Prohibition over `Pages/Admin`: the
   removal forms (`DeliveryZones.Remove(`, `DeliveryLocations.Remove(`, `.Remove(zone`,
   `.Remove(location`) count **0**. Its reach sibling asserts the two entity names are read by at
   least two files under `Pages/Admin`, so the zero is not a zero over nothing.
2. **One place builds a priced request.** `new QuoteRequest(` counts **1** across `src`, and its
   reach sibling asserts the identifier `QuoteRequest` is found in at least two files (the domain
   declares it).
3. **The override never coalesces to zero.** `DeliveryFeeOverride` followed by `??` and `0` counts
   **0** across `src`; the reach sibling asserts `DeliveryFeeOverride` is present in at least three
   files. (C17 of `foundation.tsv` forbids `?? 0` globally and stays at zero; this one names the
   member, so a rename cannot leave the invariant unmeasured.)
4. **The public page consults the flag rather than the zone's code.** `NeedsAddress` is present in
   `Pages/Book/Index.cshtml.cs` at least once — reach; and the prohibition half is C16 of
   `public-site.tsv`, extended with `other-hotel`, which is re-measured and not duplicated here.
5. **The booking form has no table.** `<table` counts **0** in
   `Pages/Admin/Bookings/Create.cshtml`; the reach sibling asserts the file still carries at least
   three of the posted field names, so an empty or renamed file cannot answer zero.
6. **The stylesheet has no media query.** `@media` counts **0** in `site.css`; reach: the file
   carries at least twenty class selectors. This states D7/04c as a rule rather than as a comment —
   a comment is read only by someone who already opened the file.

### 11.3 What step 0 measures, before any file changes

1. **The grep of the new radicals over `src/` and `tests/`, expected zero:** `Admin/Zones`,
   `Admin/Locations`, `DeliveryFeeOverride`, `line-cards`, `other-hotel`, `Book_FieldAddress`,
   `Admin_ZoneKind`, `Admin_Handover`. Any hit is a stop, and **the name changes before the type
   exists**.
2. **The value, at the starting HEAD, of every foreign control this leva moves**, each reported as a
   number with the command that produced it:
   - C05 of `admin-catalog.tsv` — expected **2** before and **2** after (four new POST handlers,
     four new `Record` calls); if it is not 2 after, the handler that did not record is the defect,
     not the control;
   - C06 of `admin-catalog.tsv` — the reach threshold, `-ge 9` today; rebased to the new floor, with
     the proof that the threshold can still answer `nao`;
   - C16 and C17 of `public-site.tsv` — **the identifier `other-hotel` is added to both patterns in
     the same commit as the seed entry**, and C16 stays at `0`;
   - C17 of `foundation.tsv` — `?? 0` stays at `0`;
   - C10 of `public-site.tsv` — the public-link relation; `_AdminLayout.cshtml` is excluded by name
     and the new admin pages live under `Pages/Admin/`, which is excluded by directory, so it should
     not move. **Measured before and after anyway**, because the comment in `_AdminLayout.cshtml`
     says this is exactly how a closed leva's control was broken before.
   - C01 of `booking-core.tsv` — the booking **detail** page still reads no catalog price.
3. **The proposal of `Docs/controles/zones-places.tsv`, with `medir` run at the starting HEAD**, so
   that no control is born already green.
4. **The shape of the reflection tests, anchored in NAME** — `OnPost…Async`, never a return type.
5. **The measured count of the interpolated-key list** (§9 test 10) before it is changed, so that
   40 is a measurement and not this spec's arithmetic.
6. **Any contradiction between this spec and the code is reported in the plan.** The spec is not
   obeyed against the measurement.

### 11.4 The stops

- **P0 — the plan**, in `scratchpad/leva04c/plano.md`, not committed. The review arrives as a dated
  `EMENDA-04C-NN` note at the top of this spec.
- **P1 — the four screens of §4, with the navigation, the resources, the tests of §9 items 1 to 3,
  and the five `.tsv` files verified.** Nothing of `/book` and nothing of the fee yet.
- **P2 — §5, §6, §7 and §8, with the rest of the tests and the six `.tsv` files verified.** The
  conference roteiro is committed before this stop is asked for.

**The closing is two commits**, as always: the content commit first, then the queue line recording
its short hash, then the conversation's summary commit. The front cannot record its own hash in one
commit.

---

## 12. Out of scope, and why

- **Any distance rule.** Rod raised it and set it aside in the same answer — *"Isso pode gerar muito
  trabalho"*. No address is geocoded, no radius is computed, and no fee is derived from a place.
- **Coupons and time-limited promotions.** `Docs/roadmap.md` puts coupons in phase 4; a promotion
  with a start and an end date is a second concept with its own screen. Today's free delivery is a
  zone fee of zero, editable.
- **Publishing a price on the public site.** `Docs/spec-02-public-site.md` K1 b records Rod's choice
  not to publish a delivery fee nobody had confirmed, and that stands: `/delivery-areas` describes
  how delivery works and names no amount. The `/book` quote shows the amount, because it is quoting
  a specific rental.
- **The unit and battery assignment to a booking, the calendar, and today's deliveries.** The other
  half of phase 4, after leva 03b.
- **Any change to availability.** Zones do not limit equipment; nothing in this leva touches a pool.
- **Storing the address typed on `/book`.** The page persists nothing; the field exists so the
  booking form of leva 03b is born filled (D45).
- **Q4 (the sales tax) stays open.** The zone screen exposes `SalesTaxRate` because the column
  exists and the operator should not need a developer to change it — not because the answer arrived.
  Every zone stays at `0` until Rod's accountant answers, and `/book` keeps printing *"Impostos
  incluídos"*.

**Confirmed true and NOT to be "fixed":**

- **`DeliveryZoneConfiguration` declares `HasDefaultValue(0m)` on `SalesTaxRate`, and that is
  correct.** D35 is about the sentinel a **boolean** default moves, and C01 of `admin-catalog.tsv`
  counts `HasDefaultValue(true|false)` only. A zone saved with rate zero is stored as zero either
  way.
- **`DeliveryLocation.Name` is not translated, and stays untranslated.** A hotel's name is the name
  on the building.
- **`vacation-homes` keeps its US$ 25 fee and its empty place list.** The empty list is what makes
  it an address-typing option, which is the same machinery this leva relies on.
- **`PlaceChoicesAsync` lives in `Pages/Admin/Bookings/Create.cshtml.cs` and is called by the public
  page.** It reads like a layering mistake and it is the one shared definition of what a place
  option is; moving it is a refactor with no invariant behind it, and it would touch two closed
  levas.
