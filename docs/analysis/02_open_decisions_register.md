# Open Decisions Register

## Purpose

This document is the control register for unresolved decisions that materially affect scope, replacement design, work sequencing, or requirements.

Open questions should be promoted here once they are important enough to influence the programme.

## Usage Rule

- `Decision Owner` is who must ultimately resolve the item
- `Blocking Level` is how much it prevents downstream work
- `Current Working Assumption` is what analysis should assume until the decision is made

## Register — Open Items

| ID | Decision | Decision Owner | Why It Matters | Current Working Assumption | Blocking Level | Status |
|---|---|---|---|---|---|---|
| D-08 | Are there additional browser-facing nopintegration modules still to be supplied? | Annique | affects completeness of legacy current-state analysis; conditional on full source access being granted | assume campaign is not the only possible module, but do not block current analysis on more drops | low | open — awaiting full source |
| TC-04 | Does the `shopapi.annique.com` surface include live notification behaviour? Specifically: what notification flows are active and routed through that endpoint? | Annique IT | shapes the notification contract inside DBM and determines whether a notifications-specific service boundary must be named | assume notification-relevant endpoints exist; do not design around them until confirmed | medium | open — deep dive required |
| TC-05 | What is the current operational role of `SyncCancelOrders.api` and `sp_ws_invoicedtickets`? Are these order cancellation and invoice reconciliation flows still active in production? | Annique IT | if active, both represent order lifecycle side effects that must be replicated in DBM's order write-back service | treat as potentially active; do not remove from replacement scope until confirmed otherwise | medium | open — further investigation required |

## Decisions Expected From Annique

- D-08 remaining web-facing nopintegration assets/modules (blocked on full source access)
- TC-04 notifications deep dive on `shopapi.annique.com`
- TC-05 confirmation of `SyncCancelOrders.api` and `sp_ws_invoicedtickets` operational status

---

## Closed Decisions

### D-01 — Consultant/Customer Account Model
**Closed: 2026-03-15**
**Resolution:** Consultant and customer identity model is managed in Dieselbrook Middleware (DBM). It is NOT managed natively in Shopify (no Shopify B2B, no native customer tag model as primary driver). DBM is the system of record for Shopify-facing consultant identity. A bidirectional sync with AccountMate is maintained. New consultant onboarding is improved in DBM. Orders are tagged correctly to distinguish consultants from direct-to-consumer buyers so that AM receives correct classification on write-back.
**Impact:** Consultant identity architecture, pricing entitlement, exclusive access, and ERP write-back classification are all locked to the DBM-owned model. See `docs/analysis/24_dbm_am_interface_contract.md`.

### D-02 — Consultant Pricing Architecture
**Closed: 2026-03-15**
**Resolution:** Pricing model confirmed in DBM. Architecture approved: `sp_camp_getSPprice` acts as the oracle for effective consultant prices; DBM precomputes effective prices and syncs them to Shopify as product metafields; a deterministic Shopify Function applies a fixed discount delta (retail minus precomputed consultant price) at checkout. DBM owns pricing computation end-to-end. Minimum AM interface requirement: orders arrive at AM with prices already resolved by DBM; AM does not redo pricing. Assessment of minimum AM needs from DBM pricing context is captured in `docs/analysis/24_dbm_am_interface_contract.md`.
**Impact:** Pricing engine, sync design, Function build, and integration contracts are unblocked. See `docs/analysis/20_pricing_engine_deep_dive.md` and `docs/analysis/24_dbm_am_interface_contract.md`.

### D-03 — Admin/Back-Office Scope
**Closed: 2026-03-15**
**Resolution:** Phased replacement confirmed. Phase 1 covers operationally critical functions necessary for day-one commerce continuity. All necessary functions are eventually replaced across subsequent phases. D-09 confirms DBM requires a dedicated admin console.
**Impact:** Phase-1 back-office scope is limited to critical operational tooling. Later phases extend to full admin parity. Admin console scope formally enlarged by D-09 closure.

### D-04 — SA-only vs SA+Namibia
**Closed (prior session)**
**Resolution:** Namibia is out of scope entirely. All Namibia-specific considerations are removed from phase-1 analysis.

### D-05 — Outbound Communications / compsys
**Closed: 2026-03-15**
**Resolution:** Clean communications architectural split confirmed. Legacy `compsys`/`sp_Sendmail` pathway stays active for AccountMate operational interface — it must not be disrupted. A new dedicated Brevo instance is provisioned for DBM ecommerce communications. The two communication estates do not share infrastructure. A dedicated Brevo analysis workstream is required to scope the new instance.
**Impact:** DBM middleware must connect to a new Brevo instance. Legacy compsys write path from AM remains untouched. See `docs/analysis/12_communications_marketing_domain_pack.md`.

### D-06 — Shopify Plan Tier
**Closed: 2026-03-15**
**Resolution:** Shopify Plus is the confirmed direction. A lower-tier viability investigation is also noted — documenting what would be lost on a lower plan. The primary reason Plus is required extends well beyond Shopify Functions: Checkout Extensibility, advanced API rate limits, Shopify Flow automation, priority support, and unlimited staff accounts all justify Plus for this programme. See `docs/analysis/23_shopify_plus_requirements_analysis.md`.
**Impact:** Custom Shopify app with Shopify Functions is confirmed viable. Plus unlocks the full recommended architecture.

### D-07 — Plugin Feature Scope
**Closed: 2026-03-15**
**Resolution:** All plugin features are in scope for eventual replacement. Phased delivery: Phase 1 = core commerce, consultant identity, and pricing. Remaining features (awards, gifts, events, bookings, special offers, advanced reporting, chatbot) follow in subsequent phases.
**Impact:** No plugin features are permanently retired without an explicit future decision. Phase-1 scope is bounded; the programme remains accountable for full coverage across phases.

### D-09 — Admin Console Requirement
**Closed: 2026-03-15**
**Resolution:** DBM requires its own dedicated admin console. This is a confirmed scope addition covering: campaign management, consultant operations, reporting, system monitoring, and configuration. It is not a minimal ops-only tooling set.
**Impact:** Admin UI scope is significantly expanded. Phase-1 admin console must deliver enough surface to operate the business. Full admin parity follows in later phases.

### D-10 — Operationally Used Reports
**Closed: 2026-03-15 — with ordered action**
**Resolution:** A deep dive is required to classify all 52 `NopReports` entries. The classification determines which reports are AM/accounting-only (remain in AM, not migrated), which are ecommerce (migrate to DBM or Shopify analytics), and which are hybrid (partial migration). Consultant self-service reports are a distinct category requiring DBM portal delivery. See `docs/analysis/25_reporting_deep_dive.md`.
**Impact:** Reporting scope and BI replacement boundary unblocked pending deep dive output. Phase-1 reporting commitment deferred until classification is complete.

### TC-02 — Go-Live Timeline
**Closed: 2026-03-15**
**Resolution:** Target go-live is end of July 2026. This is a hard dependency on Annique supplying full source code access. NopCommerce stays live in parallel until actual handover and client signoff. The `[WEBSTORE]` linked server retires at handover.
**Impact:** Programme delivery target locked. All phase-1 scope must be achievable before end of July 2026. Source code dependency is on the critical path.

### TC-03 — SMS OTP Requirement
**Closed: 2026-03-15**
**Resolution:** SMS OTP is not required for the ecommerce platform. Shopify handles storefront authentication natively; no SMS OTP provider is needed for customer login. Marketing SMS is a separate analysis workstream and does not block phase-1 delivery.
**Impact:** SMS OTP provider removed from phase-1 scope. Communications domain scope reduced accordingly for phase 1.

### TC-06 — Guest Checkout
**Closed: 2026-03-15**
**Resolution:** Guest checkout does not exist in the current NopCommerce deployment. It will be available in DBM — this is a non-negotiable scope requirement. PayU (legacy payment gateway) is replaced by a Shopify-native payment gateway in DBM.
**Impact:** Guest checkout is a confirmed DBM capability. Payment gateway migration from PayU to Shopify-native is in scope.

### TC-07 — Skin Care Analysis Tool
**Closed: 2026-03-15**
**Resolution:** The Skin Care Analysis feature will be delivered as a native Shopify app or integration. It is out of DBM middleware scope.
**Impact:** Skin Care Analysis does not generate middleware integration work. A Shopify native app is selected or built separately.

### TC-08 — NopCommerce Parallel Run
**Closed: 2026-03-15**
**Resolution:** NopCommerce stays live in parallel until handover and client signoff. The `[WEBSTORE]` linked server and associated AutoPick and exclusive-item stored procedures remain active until that point.
**Impact:** No Big Bang cutover. Dual-running must be planned. `[WEBSTORE]` dependency does not need to be resolved before go-live — it retires cleanly at handover.

---

## Resolution Log

| Date | Items Closed | Resolved By |
|---|---|---|
| prior | D-04, A-07 (Namibia) | Dieselbrook |
| 2026-03-11 | X-DEP-06 (hosting topology), A-09 (Azure hosting) | Annique IT topology diagram |
| 2026-03-15 | D-01, D-02, D-03, D-05, D-06, D-07, D-09, D-10, TC-02, TC-03, TC-06, TC-07, TC-08 | Dieselbrook via Notion answers |

---

## Review Rule

Each future major analysis or requirements document should reference the relevant decision IDs it depends on.