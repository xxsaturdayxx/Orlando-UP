# Roadmap — Orlando Up

Each phase is one or more *levas* (fronts) executed by Claude Code from a `Docs/spec-*.md`.
"Done means" is the observable result, not the list of files. Order matters: nothing in a later
phase is started while an earlier phase has an open `aguardando` line in `Docs/fila-cc.md`.

| # | Phase | Done means | Depends on |
|---|---|---|---|
| 0 | **Foundation of the repository** (this conversation) | Decisions, architecture, roadmap, open questions, first spec and queue line committed; GitHub repository created and pushed by Rod. | — |
| 1 | **Foundation of the application** — `Docs/spec-01-foundation.md` | `dotnet run` serves the layout in `/` and `/pt/` from the same pages, the catalog seed renders from the database, `/admin` login works with a seeded admin, tests pass, CI is green on GitHub, pre-commit hook installed. | 0, .NET 10 SDK on Rod's machine |
| 2 | **Public site** | Home, rentals catalog, product pages, how it works, delivery areas, FAQ, contact, legal pages — in both languages, with real fleet photos and prices from the catalog; SEO plumbing (sitemap, hreflang, JSON-LD); Lighthouse accessibility ≥ 95. | 1, answers on fleet/prices/photos |
| 3 | **Booking and payment** | Dates + location → availability and price → checkout → Stripe (test mode) → confirmation e-mail with manage link; pending holds expire; webhook idempotent; refund from admin. Staging environment online with Stripe test keys. | 2, Stripe account, Brevo sender on the domain |
| 4 | **Operations back-office** | Today's deliveries/pickups, booking detail with timeline, unit assignment, calendar, catalog/zones/coupons/units CRUD, company settings, audit log. | 3 |
| 5 | **Go-live** | Production App Service + Azure SQL in `rg-orlandoup-prod`, custom domain `orlandoup.com` with certificate, DNS, SPF/DKIM, Application Insights, backups, Stripe live keys, Google Business Profile, `robots.txt` opened, `gate-de-deploy-azure` passed. Ronatrip's `/scooters` page links or redirects here. | 4, domain registrar access, Azure subscription |
| 6 | **PWA and reminders** | Installable PWA with offline shell and push; delivery-eve reminder; post-rental review request; WhatsApp click-to-chat everywhere. | 5 |
| 7 | **Content and social automation** | Social profiles (Instagram, Facebook, TikTok, YouTube, Pinterest, Google Business, TripAdvisor) created by Rod from a naming/bio kit; a content calendar and a "Content Studio" in the admin that drafts posts (both languages) from guides and fleet photos and schedules them through a posting API (candidates: Ayrshare, Meta Graph API directly). Guides section live and feeding posts. | 5 |
| 8 | **Native apps — decision gate** | A written decision, measured against demand and the API's stability, on whether to ship .NET MAUI apps for iOS and Android; if yes, a spec. | 6, 7 |

## Cost envelope (monthly, USD, to be validated in phase 5)

App Service Linux B1 ≈ 13 (staging) + B1/P0v3 ≈ 13–60 (production); Azure SQL Basic ≈ 5 each;
Storage + Application Insights ≈ 5; Stripe 2.9 % + 0.30 per transaction; Brevo free tier up to
300 e-mails/day; domain renewal as today; social posting API ≈ 0–50 depending on the tool.
Roughly **US$ 40–90/month** before payment fees.

## What is deliberately not on this roadmap

Multi-vendor marketplace, in-park kiosks, dynamic pricing, loyalty program, agency (B2B)
portal — Ronatrip's site already serves agencies; if Orlando Up ever needs a B2B channel it is a
new phase with its own decision, not a hidden requirement of the schema.
