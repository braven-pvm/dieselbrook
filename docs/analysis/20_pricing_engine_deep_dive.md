# 07d · Pricing Engine Deep Dive

*Source: live AM SQL archaeology + Shopify platform review · v0.1 · 2026-03-09*

---

> This page isolates the pricing problem from the broader 07c pricing-and-access discussion. It documents the current AM pricing model, the recommended Shopify implementation paths, synchronization design, testing strategy, and delivery options for a production-grade consultant pricing engine.

---

## 1. Scope

This document focuses on:
- consultant product pricing
- campaign pricing and date windows
- how AM pricing should be represented in Shopify
- how price changes should be synchronized automatically
- how the checkout `discountFunction` should behave
- how to verify parity between AM and Shopify over time

This document does **not** cover in detail:
- exclusive access gating mechanics beyond where they affect price eligibility
- broader order processing / ERP write-back
- shipping logic other than voucher / discount classification implications

---

## 2. Current-State Pricing Model In AccountMate

### 2.1 Canonical live sources

Current evidence indicates the live pricing model is centered on:
- `Campaign`
- `CampDetail`
- `CampSku`
- `icitem`
- supporting customer metadata in `arcust`

Legacy structures such as `socamp` and `sosppr` are important for archaeology, but they should not be treated as the primary current-state pricing model for implementation.

### 2.2 Effective-price oracle

`sp_camp_getSPprice` is currently the best discovered single pricing oracle.

Observed behavior:

```text
Input:
- customer number
- item code
- order date

Resolution:
- resolve active campaign for order date
- find item's active CampDetail row within that campaign and date window
- return CampDetail.nPrice / nRsp if found
- otherwise return icitem.nprice / nprcinctx
```

### 2.3 Base pricing procedure stack

Other relevant procedures discovered:
- `vsp_ic_getprice`
- `vsp_ic_getcurrprice`
- `sp_ws_getactive`
- `sp_ws_getactiveNew20250512`
- `sp_ct_updatedisc`
- `sp_ct_updateportaldisc`

### 2.4 What this suggests about the live rule set

The live rule set currently looks much more like:

```text
campaign price override
  else base item price
```

than:

```text
deep per-customer negotiated pricing tree with multiple override layers
```

There is no live `soxpric` table in the AM environment audited.

---

## 3. Proven Findings From SQL Inspection

### 3.1 `sp_camp_getSPprice`

Key significance:
- provides a compact, testable source of truth for effective consultant pricing
- already encapsulates the campaign-or-fallback decision
- should be used as the parity oracle during implementation

### 3.2 `sp_ct_updatedisc`

Key significance:
- the current script sets consultant `nDiscRate` to 20 in all observed branches
- this strongly suggests a flat consultant discount policy in the current system
- if other exceptions exist, they have not yet surfaced in the inspected procedure body

### 3.3 `sp_ws_getactiveNew20250512`

Key significance:
- this procedure already generates storefront-oriented product rows
- it overlays campaign pricing, stock, store visibility, quantity limits, promo type, and current product state
- it may be the best existing AM-side read model for current storefront pricing behavior

### 3.4 `CampDetail`

Confirmed characteristics:
- current active campaign item set is manageable in size
- rows can be updated ahead of future months
- rows use `dFrom` / `dTo` for time slicing inside a month
- `dLastUpdate` is available and active

### 3.5 `Campaign` and `CampDetail` delta viability

Observed live behavior:
- `Campaign` rows are updated infrequently
- `CampDetail` rows are updated actively and regularly
- future campaigns are edited before they become current

Implication:
- middleware can automate pricing updates using delta polling plus boundary sweeps

---

## 4. The Real Engineering Problem

The engineering problem is not "how to compute a 20% discount".

It is:
- how to represent AM effective pricing in Shopify without drift
- how to handle timed campaign boundaries reliably
- how to keep checkout logic deterministic and low-risk
- how to fit the solution inside current Shopify plan/app constraints

### 4.1 Risk decomposition

| Risk area | Description | Mitigation |
|---|---|---|
| Rule parity | Shopify result diverges from AM effective price | Use AM oracle-based regression harness |
| Timed transitions | Price activates/expires without explicit edit event | Add boundary sweeper |
| Checkout performance | Function becomes too heavy or too smart | Precompute prices outside checkout |
| Platform availability | Function path blocked by app type / plan mismatch | Validate app distribution strategy early |
| Admin drift | Manual Shopify edits diverge from AM | Treat Shopify price metafields as cache, not authoring surface |

---

## 5. Shopify Design Options

### Option A — Precomputed effective prices + Discount Function

**Description**
- Middleware computes effective consultant price from AM
- Writes current effective prices to Shopify metafields
- Discount Function applies fixed discount deltas at checkout

**Pros**
- deterministic checkout
- fast runtime
- small function input
- no enterprise-gated network dependency
- easiest parity testing

**Cons**
- requires robust sync pipeline
- requires reliable app/plan path for Functions

**Verdict**
- Recommended

### Option B — Rebuild AM pricing rules directly in Wasm

**Description**
- Function contains the pricing logic itself
- it evaluates date windows, source precedence, and perhaps tax transformations at runtime

**Pros**
- fewer upstream writes if feasible

**Cons**
- unnecessary complexity
- harder to test and debug
- higher risk of drift
- poor fit for Function resource constraints

**Verdict**
- Not recommended

### Option C — Discount Function with network access

**Description**
- Function uses fetch target to call middleware for live pricing

**Pros**
- freshest possible data
- central pricing engine can be reused directly

**Cons**
- enterprise-gated
- introduces network dependency into checkout path
- extra operational complexity

**Verdict**
- Only for enterprise scenarios or if precomputed sync proves insufficient

### Option D — No Functions, storefront-only approximation

**Description**
- use storefront logic, tags, or theme scripts to present consultant prices
- rely on non-authoritative frontend logic

**Pros**
- avoids Function dependency

**Cons**
- weak checkout authority
- high mismatch risk
- poor long-term maintainability

**Verdict**
- Avoid

---

## 6. Recommended Runtime Data Model

### 6.1 Product / Variant pricing state

The function should receive only what it needs to determine the current effective target price.

Suggested fields:

| Namespace.key | Type | Notes |
|---|---|---|
| `pricing.effective_consultant_price` | money or decimal-aligned field | Current effective consultant price |
| `pricing.effective_retail_price` | money or decimal-aligned field | Optional diagnostic mirror |
| `pricing.effective_price_source` | single-line text | `campaign` / `base` |
| `pricing.effective_price_valid_to` | date_time | Next known expiry / transition boundary |
| `pricing.campaign_id` | single-line text | Audit / support |
| `pricing.last_am_sync_at` | date_time | Debugging |
| `am.item_code` | single-line text | Stable AM lookup key |

### 6.2 Customer pricing state

| Namespace.key | Type | Notes |
|---|---|---|
| `consultant.is_consultant` | boolean | Primary eligibility gate |
| `consultant.am_custno` | single-line text | ERP mapping |
| `consultant.discount_rate` | number or text | Optional diagnostic only |
| `consultant.exclusive_item_codes` | list.single_line_text_field | Access control, not core pricing |

### 6.3 Price representation choice

Do not store only percentages.

Store the actual effective target prices that the Function should honor.

This keeps campaign-specific and kit-specific pricing accurate even when it is not a uniform percentage off retail.

---

## 7. Checkout Function Design

### 7.1 Responsibility split

| Concern | Component |
|---|---|
| Compute effective AM price | Middleware |
| Persist current effective state in Shopify | Sync jobs |
| Apply line discount | `discountFunction` |
| Enforce exclusive access | Validation Function |
| Explain price / discount in UI | Theme app extension or checkout UI extension |

### 7.2 Function logic

Recommended logic:

```text
for each line:
  if customer is not consultant:
    continue

  read effective consultant price
  read current Shopify line price context

  if no effective consultant price:
    continue

  if effective consultant price >= current line price:
    continue

  discount = current line price - effective consultant price
  apply fixed product discount candidate
```

### 7.3 Why fixed deltas are safest

Because AM campaign pricing can represent:
- full-month special pricing
- one-day or short-lived promotions
- kit-specific steep overrides
- voucher-like negative rows that need classification outside normal item pricing

A fixed target-price model stays faithful where percentage logic may not.

### 7.4 What the Function should not do

- read historical price schedules
- infer tax mode from store settings at runtime
- calculate AM rules from raw source fields
- call AM directly
- depend on large JSON payloads for product calendars

---

## 8. Synchronization Strategy

### 8.1 Core principle

Shopify is a **read-optimized cache** of the AM effective pricing state.

AM remains the only authoring system.

### 8.2 Jobs

#### Job A — CampDetail delta sync
- every 5 minutes
- query `CampDetail` where `dLastUpdate > watermark`
- collect affected item codes
- recompute current effective price
- update Shopify metafields

#### Job B — Campaign delta sync
- every 15 minutes
- query `Campaign` where `dLastUpdate > watermark`
- detect header/status/month transitions
- trigger downstream affected item recalculations if needed

#### Job C — Time boundary sweep
- every 5 to 15 minutes
- detect rows where `dFrom` or `dTo` crossed current time since last execution
- recompute affected items even if no `dLastUpdate` changed

#### Job D — Reconciliation
- nightly
- compare AM effective price oracle against Shopify effective price state
- log mismatches and optionally auto-heal

### 8.3 Write strategy to Shopify

Use Admin GraphQL with batched writes.

Relevant operational constraints:
- `metafieldsSet` max 25 metafields per request
- request is atomic
- compare-and-set support can help protect concurrent updates

### 8.4 Idempotency

Each sync write should record:
- AM source version / last update timestamp
- Shopify write timestamp
- payload hash
- success / failure state

This enables safe retries and easier diagnostics.

---

## 9. Classification Layer For Non-Standard Campaign Rows

Not every `CampDetail` row should become a product consultant price.

Observed row families include:
- normal positive-price line items
- starter kits
- negative-price voucher / delivery / order-value rows

Recommended classification:

| Row type | Shopify handling |
|---|---|
| Normal item with positive consultant price | Product discount logic |
| Starter kit with positive consultant price | Product discount logic |
| Shipping-related negative row | Delivery discount path |
| Order total / voucher negative row | Order discount path |
| Gift / GWP row | Separate gift/promo handling, not core consultant pricing |

This classification should happen in middleware before price metafields are written.

---

## 10. Tax / Price Context

A pricing engine should choose a single canonical price context for Shopify sync.

### Recommended rule

If Shopify storefront pricing is VAT-inclusive, sync VAT-inclusive effective prices.
If Shopify storefront pricing is VAT-exclusive, sync VAT-exclusive effective prices.

Do not rely on the Function to convert using `1.15` or any hardcoded VAT formula.

Reason:
- tax transforms belong in middleware
- checkout Wasm should not own tax policy
- avoids hidden mismatches across markets or future tax changes

---

## 11. Legacy NopCommerce Lessons

The legacy NOP integration model pushed discount structures into the old webstore database using procedures such as:
- `sp_NOP_DiscountINS`
- `sp_NOP_OfferINS`

These procedures show that the old ecosystem already relied on **synchronized discount state**, not only live AM calculations.

That is a useful migration lesson:
- syncing structured effective pricing / offer state outward is a known pattern in this ecosystem
- the Shopify version should improve that approach by making the synced state simpler and more deterministic

---

## 12. Testing Strategy

### 12.1 Oracle parity tests

For a curated SKU/date/customer matrix, compare:
- middleware computed effective price
- `sp_camp_getSPprice` result
- expected Shopify synced effective price

### 12.2 Boundary tests

Required scenarios:
- no campaign active
- full-month campaign active
- mid-month split pricing
- one-day EO / SMS promo
- expiry boundary minute / hour
- starter kit override
- negative-price row classification

### 12.3 Checkout tests

Validate:
- consultant receives correct line-level discount
- non-consultant does not
- exclusive item blocked without access
- discount amounts match precomputed effective prices
- multiple eligible lines behave correctly
- discount stacking does not create unintended totals

### 12.4 Reconciliation dashboard

Add middleware support screens showing:
- last AM delta processed
- count of products with active campaign pricing
- count of products changed in last 24h
- count of mismatch rows from last reconciliation
- next upcoming boundary transitions

---

## 13. Delivery Paths

### Path 1 — Plus + custom app + Functions
- clean operational model
- simplest merchant-specific deployment
- best if Plus budget is accepted

### Path 2 — Advanced + public app + Functions
- strongest non-Plus path if Shopify policy and operating model are confirmed
- needs partner/app distribution clarity

### Path 3 — Enterprise + network access Function
- only if live fetch becomes necessary
- likely unnecessary if sync jobs are robust

### Path 4 — no Function fallback
- only as a last resort
- materially weaker pricing authority

### Recommended path
- build for Path 1 or Path 2
- architect middleware so Path 3 remains possible without redesign if policy changes

---

## 14. Recommended Build Sequence

1. Build middleware pricing service and parity harness first.
2. Validate its outputs against `sp_camp_getSPprice`.
3. Implement CampDetail delta sync and boundary sweep.
4. Write Shopify metafield sync layer.
5. Build small Rust `discountFunction` consuming effective prices only.
6. Add diagnostics and nightly reconciliation.
7. Only then finalize advanced pricing UX and merchant tooling.

---

## 15. Open Questions For Further SQL Digging

1. Are there any hidden customer-specific exceptions beyond the flat consultant model?
2. How should negative-price `CampDetail` rows be categorized in all cases?
3. Is `sp_ws_getactiveNew20250512` the best AM-facing read model to mirror, or should middleware use its own SQL query set?
4. Are there order-entry stored procedures beyond `sp_camp_getSPprice` that alter price during invoice / SO creation?
5. Are there consultant eligibility gates for campaigns beyond `cclass='CONSULTANT'` and status?

---

## 16. Changelog

| Version | Date | Author | Notes |
|---|---|---|---|
| v0.1 | 2026-03-09 | Braven Lab | Initial pricing-engine deep dive. Documents current AM pricing model, Shopify implementation options, sync strategy, data model, test plan, and delivery paths. |
