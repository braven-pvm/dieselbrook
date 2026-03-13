# 🚦 03 · Decision Gates & Open Questions

*Source: annique-discovery.1.0 §§ 7.5, 9 · v0.2 · 2026-03-06*

---

> Decision gates must be resolved before the migration proposal can be finalised and delivery scoped accurately. Open questions feed directly into PRD and FRS sections.

---

## Decision Gates

| Gate | Question | Impact if YES | Impact if NO | Owner | Status |
|---|---|---|---|---|---|
| **G1** | Is `amanniquenam` (Namibia ERP) actively used by Namibia staff? | All product changes must flow through AM trigger chain — no direct Shopify product API writes | Can simplify product management approach | Annique Management | 🔴 Open |
| **G2** | Shopify Plus (B2B) in budget? | Exclusive items + consultant pricing solved natively via B2B Catalogs + Price Lists | Custom app required — adds ~6–10 weeks and significant complexity | Annique Management | 🔴 Open — **Blocks FRS §4, §6** |
| **G3** | Will `anniquestore.co.za` stay live during Shopify transition? | `[WEBSTORE]` linked server stays; AutoPick and event exclusive item SPs unchanged | AutoPick + `sp_ws_gensoxitems` must be updated before old store retires | Annique IT | 🟠 Partially confirmed on staging |
| **G4** | Can production SQL Agent jobs be accessed? | Exact sync frequencies confirmed — middleware can match precisely | Design for near-real-time worst case | Annique IT | ✅ Resolved — schedules obtained from IT team |
| **G5** | Is middleware server on same LAN as AccountMate SQL Server? | Direct SQL connection, low latency | VPN tunnel or Azure Hybrid Connection required | Annique IT / Braven | 🟠 Open |
| **G6** | Does Namibia need its own Shopify store? | Separate scoped project | Share SA Shopify store — needs multi-region variant logic | Annique Management | 🔴 Open |

---

## Discovery Actions Required

### 🔴 Critical Path — Blocks Migration Proposal

| # | Action | Owner | Status |
|---|---|---|---|
| **D1** | ~~Identify who built/owns `nopintegration.annique.com` and obtain source code~~ | | ✅ Resolved — VFP Web Connection, `NISource/` fully documented |
| **D2** | ~~Get production SQL Agent job schedules~~ | | ✅ Resolved — full schedules obtained from Annique IT |
| **D3** | Confirm whether `amanniquenam` is actively used by Namibia staff | Annique Management | 🔴 Blocker |
| **D4** | Decide: Shopify Plus (B2B) or Shopify Advanced / Standard | Business Stakeholder | 🔴 Blocker |
| **D5** | Identify source / owner of `shopapi.annique.com` | Annique IT | 🟠 High — function partially known: staff sync + voucher notifications + OTP generation |
| **D6** | ~~Confirm `[WEBSTORE]` linked server on production~~ | | 🟡 Partially resolved — staging NopCommerce instance (`20.87.212.38,63000`) confirmed `[WEBSTORE]` linked server → `stage.anniquestore.co.za,61023` (SQLNCLI). Production instance-level config not accessible via DB backup (instance-level objects only). |
| **D7** | Obtain source of `SyncCancelOrders.api` and `sp_ws_invoicedtickets` | Annique IT | 🟠 High |
| **D8** | Confirm exact AM→NOP mechanism for `NOP - Sync Affiliates` job | Annique IT | 🟡 Partially resolved — `ANQ_SyncAffiliates` (plural) confirmed in production NopCommerce DB. `ANQ_SyncAffiliate` (singular, as named in AM SQL Agent job description) does NOT exist — SP name mismatch logged. Exact call mechanism still unconfirmed. |

---

## Open Technical Questions

| # | Question | Impacts | Status |
|---|---|---|---|
| **Q1–Q4** | Middleware data access, sync state, internal jobs, field mappings | FRS §3 | ✅ Answered in discovery |
| **Q5** | How are campaign discounts presented to consultants in NopCommerce today? | PRD §5, FRS §4 | ❓ Open |
| **Q6** | How many consultants have active exclusive items at any one time? | FRS §4, effort estimate | ✅ Confirmed — 21,682 rows in `ANQ_ExclusiveItems` production DB (from `AnniqueNOP.bak`) |
| **Q7** | Are there active event registrations through the webstore generating `soxitems`? | FRS §4 | ❓ Open |
| **Q8** | Does Namibia need its own Shopify store, or share SA's? | Project scope | ❓ Open (see G6) |
| **Q9** | Target go-live date? Parallel-run of NopCommerce + Shopify required? | Migration plan | ❓ Open |
| **Q10** | Which NopCommerce features do consultants actively use? (Awards, Events, Gifts, Bookings) | PRD §4 | ✅ Confirmed from production DB (2026-03-06): **Awards** ✅ 843 issued — FS1 R800 (290), FS2 R1,600 (310), FS3 R2,300 (243). **Bookings** ✅ 940 records. **Gifts** ✅ periodic — 12 configured, 0 active at backup date. **Events** ⚠️ minimal — 5 configured only, not a primary feature. **Chatbot** ❌ disabled. **Stripe** ❌ disabled. **OTP** ✅ live. **Meta CAPI** ✅ 81,228 queued events. |
| **Q11** | Which SMS provider handles OTP and password reset delivery? | FRS §7 | ❓ Open — OTP endpoint: `shopapi.annique.com/otpGenerate.api`; password reset: `nopintegration.annique.com/sendsms.api` — underlying SMS provider still unconfirmed |
| **Q12** | Is guest checkout live, or PayU-without-redirect in use? | FRS §5 | ❓ Open |
| **Q13** | Is Skin Care Analysis tool a custom build or third-party? | Scope | ❓ Open |

---

## Resolution Log

| Date | Gate / Question | Decision | Decided By |
|---|---|---|---|
| 2026-03-02 | D1 | `nopintegration.annique.com` is VFP Web Connection — source obtained | Braven Lab |
| 2026-03-02 | D2 | Full SQL Agent job schedules obtained from Annique IT | Annique IT |
| 2026-03-06 | — | Production NopCommerce backup (`AnniqueNOP.bak`, 7.5 GB) restored to local SQL Server and fully analysed. 135 published SKUs · 71,097 customers (29,285 active in 2025) · ~10,000 orders/month · 81,228 Meta CAPI events queued · 73 custom SPs · 40 custom tables. New endpoints confirmed: `shopapi.annique.com/otpGenerate.api`, `nopintegration.annique.com/api/api/ValidateNewRegistration/`, `nopintegration.annique.com/sendsms.api` also handles password reset. | Braven Lab |
| 2026-03-06 | D6 | `[WEBSTORE]` linked server confirmed on staging NopCommerce (`20.87.212.38,63000`) → `stage.anniquestore.co.za,61023` (SQLNCLI). Production instance-level objects not included in DB backup. | Braven Lab |
| 2026-03-06 | D8 | `ANQ_SyncAffiliates` (plural) confirmed in production NopCommerce DB. `ANQ_SyncAffiliate` (singular, as named in AM SQL Agent job) does NOT exist — SP name mismatch logged. | Braven Lab |
| 2026-03-06 | Q6 | `ANQ_ExclusiveItems` = 21,682 rows in production NopCommerce DB — exclusive item count confirmed. | Braven Lab |
| 2026-03-06 | Q10 | Awards ✅ 843 (FS1 R800×290, FS2 R1,600×310, FS3 R2,300×243) · Bookings ✅ 940 · Gifts ✅ 12 periodic / 0 active · Events ⚠️ 5 only · OTP ✅ live · Meta CAPI ✅ 81,228 queued · Chatbot ❌ · Stripe ❌ | Braven Lab |
