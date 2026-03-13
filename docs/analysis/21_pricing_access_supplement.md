# 07c · Shopify Pricing & Access — Pricing Logic Update

*Source: live AM SQL audit + Shopify docs review · v0.3 · 2026-03-09*

---

> This update supersedes the earlier assumption that the Shopify pricing problem is primarily about reconstructing a complex AM rule tree inside checkout. Current evidence suggests the safer model is to treat AM as the pricing oracle, sync effective prices into Shopify ahead of checkout, and keep the Shopify Function intentionally small and deterministic.

---

## Executive Update

### What is now known

1. `sp_camp_getSPprice` already implements the key consultant pricing lookup:
   - active `CampDetail.nPrice` if a campaign row exists for the item/date
   - fallback to `icitem.nprice` if no active campaign row exists
2. `sp_ct_updatedisc` indicates the current consultant discount rate is effectively flat at 20%.
3. `Campaign` and `CampDetail` are the current pricing model; `socamp` and `sosppr` should be treated as legacy lineage, not the primary live architecture.
4. `CampDetail` includes short-lived intra-month windows, so monthly campaigns are not a single price block.
5. `CampDetail.dLastUpdate` is active and usable for delta sync.

### What this changes architecturally

The pricing risk has shifted from "can we code AM pricing logic in Wasm?" to:
- can we keep Shopify's precomputed effective prices in sync with AM reliably?
- can we choose an app distribution model that permits Shopify Functions on the target plan?

---

## 1. Revised Risk Statement

The highest-risk pricing concern is not arithmetic. It is **source-of-truth fidelity under platform constraints**.

### Risks now ranked

| Risk | Level | Why |
|---|---|---|
| AM and Shopify price drift | High | Campaign rows change over time and can activate/deactivate by date window without a manual edit at the exact transition moment |
| Shopify Function availability by app type / plan | High | Current Shopify docs distinguish between public-app Functions and custom-app Functions |
| Reconstructing AM pricing at checkout from scratch | Medium | No longer appears necessary if effective prices are precomputed |
| Metafield size / payload limits | Low | Current pricing metadata can stay small if only effective current state is synced |
| Per-consultant price overrides | Low | No evidence of a live `soxpric`-style override layer |

---

## 2. Current AM Pricing Model

### 2.1 Effective consultant price

The clearest live pricing oracle found so far is:

```sql
sp_camp_getSPprice(@ccustno, @citemno, @dorder)
```

Observed logic:

```text
1. Find active Campaign for @dorder
2. Join CampSku / CampDetail for @citemno
3. If an active CampDetail row exists for that date:
     return CampDetail.nPrice / CampDetail.nRsp
4. Else:
     return icitem.nprice / icitem.nprcinctx
```

### 2.2 Consultant discount base

`sp_ct_updatedisc` currently resolves consultant `arcust.ndiscrate` to 20 in all observed branches.

Implication:
- the present-state model is functionally a **flat consultant pricing model** with campaign overrides
- there is no evidence yet of a live, complex consultant-specific ladder that must be reproduced line-by-line in Shopify checkout

### 2.3 Campaign structure

Current live tables:
- `Campaign`
- `CampDetail`
- `CampSku`
- `icitem`

Key fields already confirmed:
- `Campaign.dFrom`, `Campaign.dTo`, `Campaign.dLastUpdate`
- `CampDetail.dFrom`, `CampDetail.dTo`, `CampDetail.nPrice`, `CampDetail.nRsp`, `CampDetail.nDiscRate`, `CampDetail.dLastUpdate`
- `icitem.nprice`, `icitem.nprcinctx`

### 2.4 Important nuance: short windows inside the month

Campaign detail rows can have sub-period windows such as:
- full-month specials
- one-day or short-duration EO / SMS offers
- multiple pricing slices for the same SKU within the same calendar month

Implication:
- a pricing sync cannot rely only on "new month = new prices"
- it needs both update-driven sync and time-boundary sweeps

---

## 3. Recommended Pricing Architecture

### 3.1 Principle

Do **not** make the Shopify Function a full pricing engine.

Instead:
- AM remains source of truth
- middleware computes the effective consultant price ahead of checkout
- Shopify stores only the effective current state needed at runtime
- the Function applies deterministic fixed discounts based on precomputed values

### 3.2 Preferred runtime model

For each product / variant:
- sync current retail price context
- sync current consultant effective price
- sync price source metadata (`campaign` or `base`)
- sync campaign expiry boundary if relevant

For each consultant customer:
- sync consultant identity flag
- sync exclusive access item list
- optionally sync consultant rate / status metadata for diagnostics

At checkout:
- if customer is not a consultant: no consultant discount
- if customer is consultant and line is eligible: apply fixed discount delta
- if product is exclusive and customer lacks entitlement: validation Function blocks checkout

### 3.3 Why fixed-amount discount deltas are better than percentage rules

Campaign prices are not always reducible to a clean percentage from retail.

For example:
- starter kit pricing is a strong fixed override versus underlying component value
- intra-month rows can produce discontinuous step changes
- voucher / delivery rows use negative prices and should not be treated as ordinary line-item pricing

Therefore the safest Function behavior is:

```text
line_discount = retail_price_in_shopify - synced_effective_consultant_price
```

not:

```text
line_discount = retail_price * consultant_percentage
```

---

## 4. Sync Design

### 4.1 Delta-driven sync

Use `CampDetail.dLastUpdate` and `Campaign.dLastUpdate` as the primary delta source.

Recommended jobs:

| Job | Frequency | Purpose |
|---|---|---|
| CampDetail delta sync | Every 5 min | Pull changed rows by `dLastUpdate`, recompute effective prices for affected items |
| Campaign delta sync | Every 15 min | Detect campaign metadata changes, status shifts, and new monthly campaign headers |
| Time boundary sweep | Every 5-15 min | Catch rows whose `dFrom` / `dTo` become active or expire without a fresh update |
| Nightly reconciliation | Nightly | Compare AM oracle results with Shopify-synced effective prices |

### 4.2 Why the boundary sweep is mandatory

A campaign detail row can become active because the clock moved past `dFrom`, not because a user edited it.

If middleware only watches `dLastUpdate`, price transitions can be missed.

### 4.3 Recommended sync watermark design

Maintain middleware-side sync state such as:
- last successful `CampDetail.dLastUpdate` processed
- last successful `Campaign.dLastUpdate` processed
- last boundary-sweep execution timestamp
- last reconciliation timestamp

---

## 5. Shopify Function Design

### 5.1 Function responsibility

The `discountFunction` should be small and deterministic.

It should:
- identify consultant buyer
- read effective pricing metafields already stored on product / variant
- apply a fixed discount candidate for eligible lines
- avoid complex AM interpretation at runtime

It should not:
- query AM directly
- reconstruct campaign schedules from historical tables
- derive VAT using hardcoded formulas at runtime
- carry large pricing calendars in the input query

### 5.2 Suggested runtime fields

| Resource | Namespace.key | Purpose |
|---|---|---|
| Product or Variant | `pricing.effective_consultant_price` | Current consultant target price |
| Product or Variant | `pricing.effective_price_source` | `campaign` / `base` |
| Product or Variant | `pricing.effective_price_valid_to` | Next known boundary for observability |
| Product or Variant | `pricing.retail_price_sync` | Optional diagnostic mirror of retail source price |
| Product or Variant | `am.item_code` | Stable AM item code |
| Customer | `consultant.is_consultant` | Buyer eligibility |
| Customer | `consultant.exclusive_item_codes` | Access validation list |

### 5.3 Recommended implementation pattern

```text
if !customer.is_consultant:
  return no pricing operations

for each cart line:
  effective = synced product consultant price
  current = current Shopify price context

  if effective exists and effective < current:
    apply fixed discount = current - effective
```

---

## 6. App / Plan Constraint Update

### 6.1 Important Shopify docs correction

Current Shopify docs indicate:
- public apps distributed through the App Store can use Functions on any plan unless otherwise restricted
- custom apps that contain Shopify Function APIs are limited to Shopify Plus
- network access for discount functions is enterprise-gated

### 6.2 Practical implication

The pricing solution is now gated by **app distribution model**, not just technical feasibility.

The likely option set is:

| Option | Plan | App Type | Pricing viability |
|---|---|---|---|
| A | Plus | Custom app | Strong |
| B | Advanced | Public app | Potentially strong, must validate operating model |
| C | Advanced | No Functions | Weak |
| D | Enterprise | Network-access Function | Technically strong but overkill unless truly needed |

### 6.3 Recommendation

Do not assume "Advanced + custom app + Functions" is valid until confirmed against current Shopify policy.

---

## 7. Migration Recommendation For 07c

### Recommended direction

1. Treat `sp_camp_getSPprice` as the pricing oracle for parity testing.
2. Use middleware to compute effective consultant prices outside checkout.
3. Sync only current effective prices into Shopify.
4. Keep `discountFunction` focused on deterministic fixed discount application.
5. Keep exclusive-access blocking in a separate validation function.
6. Resolve the app distribution / plan constraint before committing the build path.

---

## 8. Open Items Added / Clarified

| ID | Item | Why it matters |
|---|---|---|
| OI-8 | Confirm whether public-app Functions on Advanced are acceptable for this merchant operating model | Determines whether non-Plus pricing architecture is viable |
| OI-9 | Confirm whether Shopify storefront prices should be synced VAT-inclusive or VAT-exclusive | Prevents runtime tax conversion logic in Functions |
| OI-10 | Identify how negative-price voucher rows in `CampDetail` should map to Shopify order/shipping discounts | Needed to avoid misclassifying non-product promotions |
| OI-11 | Build parity harness: middleware result vs `sp_camp_getSPprice` | Core protection against pricing drift |

---

## 9. Changelog

| Version | Date | Author | Notes |
|---|---|---|---|
| v0.3 | 2026-03-09 | Braven Lab | Reframed pricing architecture around precomputed effective prices. Added AM oracle findings (`sp_camp_getSPprice`, `sp_ct_updatedisc`), delta-sync design, boundary sweeps, Function runtime constraints, and app-type / plan availability risk. |
