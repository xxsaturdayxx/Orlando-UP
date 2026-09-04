# Market notes — Orlando mobility rentals

Research done on 2026-09-04 (conversation 1) from public web pages. Everything here is
`[V, web 2026-09-04]` unless marked otherwise, and **prices and park rules change every season**:
re-check before any number reaches a page, a price table or an e-mail (the `varredura-de-veracidade`
skill exists for that).

## What the customer is buying

Three profiles, all on the site's home page in some form:

1. **The slower walker in a fast group** — no medical condition, just a family that walks 8–12 miles
   a day. The scooter keeps the group together. This is the largest and least served segment; most
   competitors only speak to "mobility needs".
2. **Temporary or permanent medical condition** — recovering from surgery, arthritis, heart or lung
   conditions, pregnancy. Needs reassurance about reliability, battery range and the meet-and-greet.
3. **Seniors** — often booked by an adult child. Needs plain language, a phone/WhatsApp number, and a
   simple rebooking path.

Plus families with small children (single / double / triple strollers, infant stroller) and manual
wheelchairs for those who have a pusher.

## Park rules that shape the product

| Rule | Value | Source |
|---|---|---|
| Disney World in-park ECV rental | US$ 65/day + refundable deposit (US$ 20 parks, US$ 100 Disney Springs / water parks); first-come, no reservation; park-use only, cannot leave with it | Disney ECV page |
| Disney minimum age to rent/drive an ECV | 18, photo ID | Disney ECV page |
| Disney ECV max rider weight (in-park units) | 450 lb, one rider | Disney ECV page |
| Disney buses / Skyliner fit | ≤ 30 in wide × 48 in long — the number that matters for third-party scooters | Ziggy Knows Disney guide |
| Disney parks general max | 36 in × 52 in | Ziggy Knows Disney guide |
| Disney stroller max | 31 in × 52 in; **wagons and stroller-wagons prohibited** | Disney Park Nerds policy page |
| Disney in-park stroller rental | single US$ 15/day (US$ 13 multi-day), double US$ 31/day (US$ 27 multi-day); park-use only | Disney Park Nerds policy page |
| Universal Orlando ECV limits | ≤ 30 in × 48 in; no official rider weight limit; in-park rentals first-come | Scootz rules page |
| **ScooterBug is Disney's "Featured Provider"** — the only third party allowed to leave equipment with Bell Services and collect it there; every other vendor must hand over in person (meet-and-greet) at the resort | since 2020 | Disney ECV page; There's a Girl in the Castle |

Consequences for the site: (a) the meet-and-greet is a scheduled **delivery window**, not a
drop-off, and the booking flow must collect a phone/WhatsApp and a time window; (b) "fits Disney
buses" is a product attribute worth a badge; (c) the stroller catalog must state dimensions and
must not contain wagons.

## Competitor snapshot (Orlando, 7-day scooter base rate unless noted)

| Vendor | Positioning | Price signals |
|---|---|---|
| ScooterBug | Disney Featured Provider, drop at Bell Services, no need to be present | ≈ US$ 61 for 1 day, US$ 185–210 for 7 days (before tax) |
| Buena Vista Scooters | Long-standing local, Disney-area | US$ 245 / 7 days |
| Apple Scooter | Local, hotel delivery | US$ 225 / 7 days |
| Florida Mobility Rentals | Price leader | US$ 177.95 / 7 days |
| BP Mobility | Price leader | US$ 150–210 / 7 days |
| Scooter King Orlando | Tiered by rider weight; free delivery within 25 mi of Magic Kingdom; optional insurance ≈ US$ 20/trip; no deposit | 3+ days: US$ 27/day (200 lb), 33 (300 lb), 40 (400 lb), 50 (500 lb); 1–2 days flat US$ 75–120 |
| Cloud of Goods | Marketplace, cart + checkout, many reviews | models by capacity 200 / 350 / 400 lb, ranges 6–15 mi |
| Scootaround, Scootz, Scootarama, Gold Mobility, Walker Mobility | National or local players, similar model | — |

Patterns worth copying: tiers by **rider weight capacity**, a **cheaper per-day rate from 3 days**,
**free delivery inside a radius** and a fee outside, optional **damage waiver** as an add-on,
**accessories** (cup holder, cane holder, sunshade, basket, phone mount) as add-ons, a **battery
range** figure per model, and **reviews** displayed near the price. Nobody serves Brazilians in
Portuguese with a local Orlando team — that is Orlando Up's opening.

## Ronatrip today

`[V, ronatrip.com/scooters, 2026-09-04]` The current page is a lead form (name, e-mail, WhatsApp,
dates, quantity, hotel, notes) that creates a quote in Ronatrip's admin; no prices, no models, no
online payment, Portuguese only, "approximately 150 kg" capacity, delivery to the hotel at an
agreed time. Everything Orlando Up adds — catalog, prices, availability, payment, two languages,
operations — is new.

## Business facts still needed from Rod

See `Docs/open-questions.md`: fleet inventory, current price list, delivery radius and fee, hours,
sales-tax treatment of medical scooter rentals in Orange/Osceola counties, waiver text, insurance.

## Sources

- Disney ECV rentals: https://disneyworld.disney.go.com/guest-services/ecv-rentals/
- Ziggy Knows Disney scooter guide: https://ziggyknowsdisney.com/disney-world-scooter-rentals/
- There's a Girl in the Castle (ScooterBug exclusivity): https://theresagirlinthecastle.com/walt-disney-world/using-a-scooter-at-disney-world-part-3-updates/
- Disney Park Nerds stroller policy: https://disneyparknerds.com/disney-world-stroller-policy/
- Scootz — Universal ECV rules: https://www.scootzrentals.com/blog/2025/universal-studios-ecv-rules-explained.html
- Scooter King 2026 prices: https://scooterkingorlando.com/blogs/visiting-orlando/disney-world-scooter-rental-prices-in-2026-what-visitors-should-budget
- Cloud of Goods Disney ECV page: https://www.cloudofgoods.com/disney-world/ecv-rentals
- Ronatrip scooters page: https://www.ronatrip.com/scooters
- NerdWallet Stripe vs Square: https://www.nerdwallet.com/business/software/learn/stripe-vs-square
- .NET 10 overview: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview
