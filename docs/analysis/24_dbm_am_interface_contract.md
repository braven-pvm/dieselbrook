# DBM–AM Interface Contract

*Closes decisions: D-01 (consultant model), D-02 (pricing model) — 2026-03-15*
*Source of confirmed answers: Dieselbrook Notion decisions page, 2026-03-15*

---

## 1. Purpose

This document defines the authoritative system-of-record boundaries and the minimum interface contract between Dieselbrook Middleware (DBM) and AccountMate (AM/ERP).

It answers:

- What does DBM own as the system of record?
- What does AM remain as the system of record for?
- What is the minimum data flow AM needs from DBM?
- What does DBM write back to AM?
- Why full AM source code access is recommended.

---

## 2. System-of-Record Boundaries — Confirmed

### 2.1 DBM is the system of record for

**Consultant and customer identity (Shopify-facing)**

DBM owns the consultant and customer data model as presented to Shopify and to ecommerce operations. Key confirmed facts (D-01):

- Consultant identity is managed in DBM. It is NOT managed natively in Shopify (no Shopify B2B companies, no native customer tag model as the primary mechanism).
- DBM is the system of record for Shopify-facing consultant profiles: account state, entitlement flags, exclusive access keys, campaign eligibility, and storefront identity.
- Consultant identity data originates in AccountMate (AccountMate is still the ERP authority for consultant number, lifecycle state, and commission-affecting attributes). DBM reads from AM and projects consultant state into Shopify via sync.
- New consultant onboarding is improved inside DBM. The legacy `nopnewregistrations.prg` / NopCommerce onboarding flow is replaced by a DBM-native improved intake flow.
- Consultant orders are tagged in DBM to distinguish consultant purchases from direct-to-consumer purchases, so that AM receives the correct classification on order write-back.

**Pricing model (ecommerce-facing)**

DBM owns pricing computation for the storefront. Key confirmed facts (D-02):

- `sp_camp_getSPprice` in AccountMate acts as the pricing oracle — it returns effective consultant prices per SKU per campaign.
- DBM reads `sp_camp_getSPprice` on a scheduled / campaign-boundary basis and precomputes effective prices for all relevant SKU × campaign combinations.
- Precomputed effective prices are synced to Shopify as product metafields.
- A deterministic Shopify Function in the custom private app reads these metafields at checkout and applies the discount delta (retail price minus precomputed consultant price) for authenticated consultant customers.
- DBM owns this pricing computation cycle end-to-end. AM's pricing oracle is read but not called in real time at checkout.

**Ecommerce communications**

DBM owns the ecommerce communication estate. Key confirmed facts (D-05):

- A new dedicated Brevo instance is provisioned for DBM ecommerce communications (order confirmations, consultant welcome emails, campaign notifications, marketing flows).
- This Brevo instance is separate from any Brevo configuration connected to the legacy NopCommerce/NISource estate.
- The legacy `compsys` / `sp_Sendmail` pathway remains active for AccountMate operational communications. DBM does not touch this pathway.

**Admin console**

DBM owns the back-office administration surface. Key confirmed facts (D-09):

- DBM requires a dedicated admin console.
- Scope includes: campaign management, consultant operations, reporting, system monitoring, configuration.
- This is a confirmed scope addition, not a deferred optional.

---

### 2.2 AccountMate remains the system of record for

- Consultant number assignment and lifecycle state (activation, deactivation, reactivation)
- Commission-affecting attributes and commission computation
- MLM hierarchy and downline relationships
- Accounting and financial records
- Inventory master data (item master, stock on hand, warehouse allocation)
- Invoice generation and accounts receivable
- Procurement and purchase orders
- ERP-side operational communications via `compsys`/`sp_Sendmail`

---

## 3. Minimum AM Interface Requirements

*What does AM need FROM DBM?*

DBM must supply the following to AM:

### 3.1 Orders — write-back with pricing resolved

- AM receives ecommerce orders written by DBM's `OrderWritebackService`.
- Orders arrive at AM with prices already computed by DBM. AM does not re-run pricing logic on ecommerce orders.
- Each order line must carry: SKU, quantity, effective consultant price applied (not retail price), order total, and consultant vs direct-to-consumer classification.
- Consultant flag on the order must be accurate. AM uses this for commission attribution and lifecycle events (e.g., reactivation trigger via `sp_ws_reactivate`).

### 3.2 Consultant status events — inbound to AM

- When a customer completes a registration flow in DBM that marks them as a new consultant, DBM notifies AM.
- AM creates the consultant record in `arcust` with the correct consultant type.
- DBM receives the AM-assigned consultant number on acknowledgment and stores it against the DBM-side consultant profile.

### 3.3 Consultant status change notifications — outbound from AM

- When AM changes a consultant's lifecycle state (activation, deactivation, suspension, reactivation), it must trigger a sync event.
- DBM's `ConsultantSyncService` (replacement for `syncconsultant.prg`) reads this state on a scheduled or event-driven basis and updates the Shopify-facing consultant profile accordingly.
- Timing tolerance: consultant entitlement and pricing access must reflect current AM state within a maximum of one sync cycle (schedule TBD during infrastructure configuration, see A-08).

### 3.4 No AM pricing re-computation

- AM must not re-price ecommerce orders. DBM resolves pricing before write-back.
- The `sp_camp_getSPprice` procedure is called by DBM during sync cycles. It is not called by AM or any AM-side process on behalf of ecommerce order processing.

---

## 4. What DBM Writes to AM

| Write | Detail | DBM Service |
|---|---|---|
| Ecommerce orders | Full order with consultant classification, effective prices applied, line-level detail | `OrderWritebackService` |
| New consultant registration | Consultant application data triggering AM record creation | `ConsultantRegistrationService` |
| Consultant reactivation trigger | Via `sp_ws_reactivate` when an order qualifies for reactivation | `OrderWritebackService` |
| Order status acknowledgment | Fulfillment and invoice events returned from AM back to Shopify | `OrderStatusFulfillmentService` |

---

## 5. What DBM Reads from AM

| Read | Source | DBM Service |
|---|---|---|
| Effective consultant prices | `sp_camp_getSPprice` | `PricingSyncService` |
| Consultant lifecycle state | `arcust` + related lifecycle tables | `ConsultantSyncService` |
| Inventory availability | `amanniquelive` inventory tables | `InventorySyncService` |
| Campaign definitions | `Campaign`, `CampDetail`, `CampSku` | `CampaignAdministrationService` |
| Sponsor/upline relationships | `compplanLive` hierarchy tables | `ConsultantSyncService` |
| MLM statement data (reporting) | `CTstatement`, `CTstatementh`, `CTdownlineh` | `ConsultantReportingDataService` |
| Order fulfillment and invoice state | `SOPortal`, `arinvc`, `soship` | `OrderStatusFulfillmentService` |

---

## 6. Consultant Model in DBM — Architecture Details

### 6.1 Shopify representation

- Consultants are represented as standard Shopify customer accounts.
- Consultant-specific state is stored in customer metafields managed by DBM:
  - `consultant.number` — AM-assigned consultant number
  - `consultant.tier` — pricing tier / campaign eligibility class
  - `consultant.status` — active / inactive / suspended
  - `consultant.exclusive_access` — whether consultant has access to exclusive SKUs in current campaign
- Consultant tags are used for Shopify storefront gating (exclusive collections, member-only access) where metafields alone are not sufficient.

### 6.2 Onboarding flow improvement

The legacy onboarding (NopCommerce plugin + `nopnewregistrations.prg`) is replaced by a DBM-native onboarding intake:

- Prospective consultant submits registration via DBM storefront or a dedicated registration flow.
- DBM validates sponsor reference, checks for duplicates, confirms eligibility criteria.
- On validation, DBM creates a provisional Shopify customer account and notifies AM.
- AM confirms record creation and returns consultant number.
- DBM updates the Shopify customer metafields with the AM-assigned consultant number and activates entitlements.
- Consultant receives onboarding welcome communication via the new Brevo instance.

### 6.3 Direct-to-consumer vs consultant classification

On every ecommerce order:
- DBM determines whether the placing customer is an active consultant (via stored consultant state in DBM).
- Order write-back to AM tags the order with the correct customer type.
- AM uses this classification for commission attribution (consultant orders may generate commission events; direct-to-consumer orders do not).

---

## 7. Pricing Model in DBM — Architecture Details

*See also `docs/analysis/20_pricing_engine_deep_dive.md` for full pricing archaeology.*

### 7.1 Precomputed effective prices

DBM runs a pricing sync cycle at campaign boundaries and on a scheduled refresh:

1. Read all active campaigns and eligible SKUs from `Campaign`, `CampDetail`, `CampSku`.
2. For each SKU × campaign combination, call `sp_camp_getSPprice` to get the effective consultant price.
3. Store precomputed prices in DBM's internal pricing store.
4. Push precomputed prices to Shopify product metafields (one metafield per relevant pricing tier, per product variant).

### 7.2 Checkout Function

A custom Shopify Function (deployed in the custom private app, requires Shopify Plus — see `docs/analysis/23_shopify_plus_requirements_analysis.md`) executes at checkout:

1. Read the placing customer's `consultant.tier` metafield.
2. For each line item, read the `variant.consultant_price` metafield.
3. Apply a discount adjustment: effective consultant price minus Shopify variant retail price.
4. The Function output is deterministic and stateless — it reads only what DBM has already precomputed.

### 7.3 AM does not re-price

AM receives order write-backs with line prices already determined by DBM. AM records these as received. There is no AM-side re-pricing of ecommerce orders.

---

## 8. AM Source Code — Recommendation

**Recommendation: Full source code access for AccountMate is required.**

**Reason:**

The interface contract defined in this document relies on deep knowledge of:
- `sp_camp_getSPprice` behaviour and all its logic branches (campaign types, tier resolution, exception handling)
- `sp_ws_reactivate` trigger conditions and side effects
- Order write-back stored procedure contracts (field-by-field mapping, validation behaviour)
- Consultant lifecycle event model (what triggers state changes, what AM validates on write)
- `compplanLive` schema for MLM reading (statement, hierarchy, contribution data for reporting)

Without source code access, DBM development relies on reverse-engineered contracts from observed behaviour alone. Any undocumented edge in the AM stored procedure estate becomes a regression risk on live data.

**Specific risks without source access:**
- Pricing edge cases in `sp_camp_getSPprice` that are not visible from test data alone
- Order write-back field contracts that are only fully defined in AM-side SP code
- Reactivation trigger logic that may have undocumented preconditions
- Commission calculation dependencies that affect how orders must be structured

**Decision:** Dieselbrook has confirmed "I say yes" — source code access should be obtained from Annique as a delivery prerequisite. This aligns with TC-02 (go-live target of end July 2026 is hard-dependent on full source access).

---

## 9. Decision References

| Decision | Status | Resolution |
|---|---|---|
| D-01 Consultant/customer model | Closed 2026-03-15 | DBM owns consultant identity model |
| D-02 Pricing architecture | Closed 2026-03-15 | DBM owns pricing; AM oracle read by DBM |
| D-05 Communications split | Closed 2026-03-15 | New Brevo instance for DBM; compsys untouched |
| D-09 Admin console | Closed 2026-03-15 | Dedicated admin console in scope |
| TC-02 Go-live and source dependency | Closed 2026-03-15 | End July 2026; source code is critical path |
