# Shopify Plus Requirements Analysis

*Closed decision: D-06 confirmed Shopify Plus as the target plan tier — 2026-03-15*
*Triggered by: Annique programme review and Dieselbrook decision confirmed via Notion*

---

## 1. Purpose

This document records the full requirements basis for the Shopify Plus selection across all relevant factors — not only Shopify Functions eligibility.

It also documents what a lower-tier plan would sacrifice, as instructed by the D-06 resolution note ("Plus, investigate lower tier").

**Conclusion up front:** Shopify Plus is required. No lower tier satisfies the combined requirements of this programme.

---

## 2. Confirmed Direction

**D-06 Resolution (2026-03-15):** Shopify Plus is the confirmed Shopify plan tier for the Annique DBM platform.

---

## 3. Requirements Analysis By Plus Feature

### 3.1 Shopify Functions — Checkout Pricing (Critical)

**Requirement:** The approved pricing architecture (D-02) requires a deterministic Shopify Function deployed in a custom private app to apply the consultant price delta at checkout. The Function reads precomputed effective prices from product metafields and applies the correct discount for authenticated consultant customers.

**Plus dependency:** Custom apps containing Shopify Functions are only installable on Shopify Plus stores. On Advanced or lower plans, Shopify Functions are available only through apps published on the Shopify App Store. A custom private app deploying a bespoke consultant pricing Function requires Plus.

**Lower-tier alternative:** A public app with Functions capability could be published on the App Store, but this introduces significant trade-offs: public distribution of proprietary pricing logic, dependency on App Store review and listing maintenance, and inability to lock the app to a single merchant without custom partner arrangements. These trade-offs are not compatible with the programme intent.

**Verdict:** Plus required for custom Function deployment.

---

### 3.2 Checkout Extensibility — Custom Checkout UI (High)

**Requirement:** The Annique ecommerce flow includes consultant-specific checkout state: pricing display, exclusive item validation, and potentially consultant number entry or display at checkout. Checkout Extensibility (Checkout UI Extensions) is the supported mechanism for customising the Shopify checkout UI.

**Plus dependency:** Checkout Extensibility via UI Extensions is available on Plus. On lower plans, checkout customisation is severely limited — merchants on Shopify (non-Plus) cannot install checkout UI extensions from custom apps without Plus.

**Lower-tier alternative:** None that preserves the consultant checkout experience. A non-Plus checkout is a locked, uncustomisable surface.

**Verdict:** Plus required for checkout UI extensions.

---

### 3.3 Shopify Flow — Operational Automation (High)

**Requirement:** DBM will require operational automation for: consultant lifecycle events triggering Shopify actions, order tagging for ERP write-back routing, fraud/risk rule automation, and campaign-linked product state management. Shopify Flow is the native automation engine for Shopify and runs server-side without custom middleware code.

**Plus dependency:** Shopify Flow is included with Shopify Plus. It is not available on lower-tier plans.

**Lower-tier alternative:** All automation would need to be implemented inside DBM middleware via webhook consumers and additional orchestration logic, increasing build scope and operational complexity significantly.

**Verdict:** Plus strongly preferred; exclusion would increase middleware scope materially.

---

### 3.4 Advanced Admin API Rate Limits (High)

**Requirement:** DBM middleware performs high-volume synchronisation: product sync (full catalogue), inventory sync, price metafield sync per product per campaign cycle, consultant customer syncs, and order ingest. All of these hit the Shopify Admin API heavily during campaign boundary events and scheduled sync windows.

**Plus dependency:** Shopify Plus provides significantly higher API rate limits (leaky bucket capacity and restore rate) compared to lower plans. Plus merchants benefit from enhanced API call allowances that support high-volume middleware patterns.

**Lower-tier alternative:** Lower plans impose API rate limits that would require aggressive throttling, slower sync windows, and more complex backoff handling in DBM middleware. During campaign launches (which are high-frequency pricing change events for Annique), lower API limits would create operational risk.

**Verdict:** Plus required for reliable high-volume sync behaviour.

---

### 3.5 Unlimited Staff Accounts (Medium)

**Requirement:** The DBM admin console will be accessed by Dieselbrook staff (operations, support, campaign management) and Annique operational staff. Shopify admin access may also be required for product management, order administration, and reporting by multiple users across both organisations.

**Plus dependency:** Shopify Plus provides unlimited staff accounts. Lower plans cap staff accounts (Basic: 2, Shopify: 5, Advanced: 15).

**Lower-tier alternative:** Advanced plan allows 15 staff accounts, which may be sufficient for the initial deployment. However, as operations scale and both Dieselbrook and Annique teams require access, the cap becomes a risk.

**Verdict:** Not a hard blocker on its own, but Plus removes a future constraint cleanly.

---

### 3.6 Custom Checkout Branding and `checkout.liquid` Access (Medium)

**Requirement:** The Annique storefront is a branded consumer-facing product. Storefront and checkout branding must reflect Annique's brand identity. Theme customisation is available broadly, but `checkout.liquid` access (for deeper checkout HTML/CSS branding control) is Plus-only.

**Plus dependency:** `checkout.liquid` access is restricted to Plus. Lower plans receive a themed checkout but without direct template control.

**Lower-tier alternative:** Checkout UI Extensions can partially bridge this gap on Plus (building UI blocks), but `checkout.liquid` depth is only available on Plus.

**Verdict:** Plus preferred for full brand control; not a hard technical blocker given Checkout Extensibility.

---

### 3.7 Shopify Audiences (Low / Future)

**Requirement:** If Annique's marketing capability expands to use Shopify Audiences for retargeting and marketing optimisation (relevant to consultant recruitment and consumer acquisition), Shopify Audiences is a Plus-only feature.

**Application to this programme:** Not a phase-1 requirement but relevant to future marketing capability.

**Verdict:** Not a phase-1 driver; Plus inclusion is a future-proofing benefit.

---

### 3.8 Priority Support (Medium)

**Requirement:** The programme has a hard go-live deadline of end July 2026 (TC-02). Any platform-side issues — API incidents, checkout problems, billing/plan configuration — need fast resolution support.

**Plus dependency:** Shopify Plus merchants get priority support with dedicated account management and faster response SLAs.

**Lower-tier alternative:** Standard support only, with slower response paths during critical delivery phases.

**Verdict:** Priority support reduces delivery risk during the critical pre-launch and go-live window.

---

### 3.9 Native Shopify B2B — Explicitly Ruled Out

The Shopify B2B native tooling (B2B Catalogs, B2B Price Lists, B2B Companies) is available on Plus but is explicitly NOT the recommended path for this programme.

**Reason:** Pricing archaeology shows that Annique's consultant pricing model is too complex and ERP-dependent for Shopify's native B2B price list model. The approved architecture (D-02) uses DBM-precomputed prices and a custom Function — this does not rely on B2B native tooling at all. B2B native features are irrelevant to the approved consultant pricing design.

This point is documented so that it is not revisited: the Plus selection is not driven by B2B native tooling availability.

---

## 4. Lower-Tier Viability Assessment

*D-06 resolution note: "investigate lower tier".*

| Feature | Required? | Available on Advanced | Available on Shopify (Basic+) |
|---|---|---|---|
| Custom Functions (private app) | Yes — critical | No | No |
| Checkout UI Extensions (custom app) | Yes — high | No | No |
| Shopify Flow | Yes — high | No | No |
| Advanced API rate limits | Yes — high | Partial | No |
| Unlimited staff accounts | Medium | No (15 cap) | No (5 or 2 cap) |
| `checkout.liquid` access | Medium | No | No |
| Priority support | Medium | No | No |
| Shopify Audiences | No (future) | No | No |

**Conclusion:** The Advanced plan fails on the three most critical requirements: custom private app Functions, Checkout Extensibility, and Shopify Flow. No lower plan tier satisfies the programme requirements. Shopify Plus is the minimum viable tier.

---

## 5. Cost Context

Shopify Plus pricing is at a higher monthly cost than lower plans (typically USD 2,000/month base for revenue below threshold, scaling above that). This cost must be incorporated into the Dieselbrook commercial model for the platform.

The cost is justified by: unlocking the consultant pricing Function (which is the core commercial differentiator of the platform), checkout extensibility, Flow automation (which reduces middleware build scope), and priority support during a time-critical delivery window.

---

## 6. Decision References

| Decision | Status | Link |
|---|---|---|
| D-06 Shopify plan tier | Closed 2026-03-15 | `docs/analysis/02_open_decisions_register.md` |
| D-02 Pricing architecture | Closed 2026-03-15 | `docs/analysis/02_open_decisions_register.md` |
| D-01 Consultant model | Closed 2026-03-15 | `docs/analysis/02_open_decisions_register.md` |
| Pricing engine deep dive | Reference | `docs/analysis/20_pricing_engine_deep_dive.md` |
| Pricing access supplement | Reference | `docs/analysis/21_pricing_access_supplement.md` |
