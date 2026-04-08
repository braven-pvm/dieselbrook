# Product Requirements Document
## Dieselbrook Middleware (DBM) — Annique Cosmetics Platform Modernisation

**Document type:** PRD — Product Requirements Document  
**Programme:** Annique → Shopify Plus migration and Dieselbrook Middleware build  
**Version:** 0.1 — initial  
**Date:** 2026-04-07  
**Status:** Draft — in review  
**Linear:** ANN-5  

---

## Decision Dependencies

| Decision ID | Topic | Status | Impact If Still Open |
|---|---|---|---|
| D-01 | Consultant/customer account model | ✅ Closed 2026-03-15 | Was: blocks account model, entitlement, pricing |
| D-02 | Consultant pricing architecture | ✅ Closed 2026-03-15 | Was: blocks pricing and campaign sync spec |
| D-03 | Admin/back-office scope | ✅ Closed 2026-03-15 | Was: blocks admin console scope |
| D-05 | Outbound communications architecture | ✅ Closed 2026-03-15 | Was: blocks comms domain |
| D-06 | Shopify plan tier | ✅ Closed 2026-03-15 | Was: blocks Shopify Function viability |
| D-07 | Plugin feature scope | ✅ Closed 2026-03-15 | Was: blocks awards, events, gifts, bookings scope |
| D-09 | Admin console requirement | ✅ Closed 2026-03-15 | Was: blocks admin console spec |
| D-10 | Operationally used reports | ✅ Closed 2026-03-15 — deep dive ordered | Blocks final report scope per domain |
| TC-04 | `shopapi.annique.com` ownership and source | 🟠 Open — Annique IT | Blocks OTP/SMS spec; identity domain cannot be fully specced until resolved |
| TC-05 | `SyncCancelOrders.api` and `sp_ws_invoicedtickets` | 🟠 Open — Annique IT | Blocks order cancellation spec detail |

**This PRD is fully writeable.** The two open technical confirmations (TC-04, TC-05) affect the detailed domain specs for identity and order cancellation, not the programme-level requirements captured here.

---

## 1. Purpose

This document defines the product requirements for the **Dieselbrook Middleware (DBM)** — a purpose-built integration and middleware platform that connects Shopify Plus (the new Annique Cosmetics storefront) to AccountMate (the existing on-premises ERP).

DBM replaces the current three-tier legacy stack:
- **NopCommerce** — the existing ASP.NET storefront (297 custom plugin files, 40 custom tables, 73 custom stored procedures)
- **NISource** — the Visual FoxPro Web Connection middleware layer (`nopintegration.annique.com`, `shopapi.annique.com`)
- **SQL Agent jobs** — the scheduled AM ↔ NopCommerce synchronisation jobs

This is **not a storefront project.** The storefront surface (Shopify Plus) is commercially established and requires relatively limited custom build. The critical and complex deliverable is the middleware layer that bridges Shopify and AccountMate — preserving 15+ years of MLM business logic, consultant pricing, exclusive-item access, order flow, and operational integrations that currently live in the legacy stack.

**The single most important architectural principle:**  
AccountMate remains the single source of truth for all business data. DBM reads from AM and writes back to AM. Shopify displays and processes. AM owns.

---

## 2. Programme Context

### 2.1 Parties

| Party | Role | Delivers |
|---|---|---|
| **Annique Cosmetics** | Client and ERP owner | AccountMate (unchanged), Shopify Plus account, source access |
| **Dieselbrook** | System integrator and prime contractor | Shopify store setup, storefront theme/UX, reporting/BI |
| **Architect (Braven Lab)** | DBM builder, engaged by Dieselbrook | DBM middleware platform, custom Shopify app, deployment |

### 2.2 Business model

Annique operates a **direct-sales MLM cosmetics business**. The business model is built on a network of independent consultants who purchase products at a discounted price. Their pricing tier, exclusive product access, and purchasing entitlements are all determined by their classification in AccountMate. This MLM model is the core complexity of the programme — it cannot be simplified away and must be faithfully replicated in DBM.

### 2.3 Key scale facts

| Metric | Value |
|---|---|
| Orders/month | ~10,000 (~128,000 in 2024) |
| Active consultants | 10,308 (of 119,616 total) |
| Customers in NopCommerce | 71,097 (29,285 active in 2025) |
| Published SKUs on store | 135 (of ~800 in AM `icitem`) |
| Exclusive item rows (`soxitems`) | 21,682 |
| Campaign pricing rows (`CampDetail`) | 18,571 |
| Active campaigns | 77 |
| Meta CAPI events queued | 81,228 |

### 2.4 Delivery constraints

| Constraint | Value |
|---|---|
| Target go-live | End of July 2026 |
| Delivery model | AI-assisted, architect-led |
| Architect rate | R850/hr |
| Architect budget | R200K (~168–225 architect hours) |
| AccountMate | Must not be modified — no schema changes, no new stored procedures without explicit approval |
| Credentials | `sa`/`AnniQu3S@` hardcoded in NISource VFP source — must never appear in DBM; Azure Key Vault required |

---

## 3. System Context

### 3.1 What DBM is

DBM is a **.NET 9 / ASP.NET Core middleware platform** deployed to Azure, with private SQL connectivity to the on-premises AccountMate SQL estate. It contains:

- **25 domain and platform services** handling all AM ↔ Shopify data flows
- **A custom private Shopify app** — Shopify Functions (pricing, access control), Checkout UI Extensions, admin shell
- **A dedicated admin console** — campaign management, consultant operations, reporting, monitoring (Node.js/Remix)
- **A DBM state database** — Azure SQL for sync watermarks, idempotency keys, job history, audit records, reconciliation results

### 3.2 What DBM is not

- DBM does not replace AccountMate or any part of the AM ERP
- DBM does not replace the AM commission engine (`compplanLive`) — read-access only in phase 1
- DBM does not replace AM internal communications (`compsys`/`sp_Sendmail`)
- DBM is not a fork or extension of NISource, NopCommerce, or AnniqueAPI — it is a clean greenfield build
- DBM does not own the Shopify storefront theme or consumer UX — Dieselbrook owns those

### 3.3 Architecture principle

> **Event-driven on the Shopify side. Poll-and-reconcile on the AccountMate side.**

Shopify pushes events by webhook. AccountMate is a SQL-centric ERP with no outbound event capability — DBM polls against change indicators, executes boundary sweeps for time-based pricing transitions, and runs nightly reconciliation for drift detection.

### 3.4 System-of-record boundaries

| Domain | System of record |
|---|---|
| Consultant identity (ecommerce-facing) | DBM (projected from AM; owns Shopify-facing state) |
| Consultant identity (ERP lifecycle) | AccountMate |
| Effective consultant pricing | DBM (computed from AM oracle, synced to Shopify) |
| Campaign pricing definition | AccountMate (`Campaign`/`CampDetail` tables) |
| Exclusive item eligibility | AccountMate (`soxitems`) |
| Inventory master | AccountMate |
| Product master | AccountMate (`icitem`) |
| Orders (written to ERP) | DBM write-back service |
| Ecommerce communications | DBM (new Brevo instance) |
| AM internal communications | AccountMate (`compsys`) — DBM does not touch |
| MLM commission computation | AccountMate (`compplanLive`) — read-only from DBM |

---

## 4. Functional Requirements

Requirements are grouped by workstream. Each requirement is tagged with an ID and workstream reference.

---

### WS-01 · Orders and Fulfilment

**Context:** ~10,000 orders/month. Orders originate in Shopify and must be written back to AccountMate with prices pre-resolved. The AM write-back is the single highest-risk operation in the programme — errors are immediately visible and financially material.

#### FR-ORD-001 — Webhook ingestion
The system shall receive and acknowledge Shopify order webhooks (`orders/create`, `orders/cancelled`, `orders/fulfillment_updated`) within 5 seconds of delivery.

#### FR-ORD-002 — Idempotent order write-back
The system shall write each incoming Shopify order to AccountMate exactly once, regardless of how many times the webhook is delivered or replayed. The idempotency key is `shopify_order_id`.

#### FR-ORD-003 — Order data mapping
The system shall map all required Shopify order fields to the AccountMate sales order schema (`sosord`, `soxitem`, `sostr`, `soskit`) using the confirmed field mappings from `docs/analysis/24_dbm_am_interface_contract.md`.

#### FR-ORD-004 — Consultant vs. consumer classification
The system shall classify each order as consultant-originated or direct-to-consumer at AM write-back time, and set the appropriate AM fields to ensure correct commission attribution.

#### FR-ORD-005 — Pre-resolved pricing on write-back
The system shall write orders to AM with prices already resolved by DBM. AccountMate does not re-price ecommerce orders.

#### FR-ORD-006 — Order cancellation
The system shall handle Shopify order cancellations and propagate the cancellation to AccountMate. Behaviour of `SyncCancelOrders.api` and `sp_ws_invoicedtickets` must be confirmed and replicated (TC-05 open).

#### FR-ORD-007 — Order status and fulfilment reverse-sync
The system shall poll AccountMate order status (SOPortal flow: `' '` → `'P'` → `'S'`) and propagate status changes to Shopify as fulfillment events.

#### FR-ORD-008 — Kit order handling
The system shall correctly handle kit items in orders, mapping kit line items to `soskit` in AccountMate.

#### FR-ORD-009 — Dead-letter and retry
The system shall place failed order write-back attempts into a dead-letter queue after [N] retries and surface them in the admin console for operator review and replay.

#### FR-ORD-010 — Audit log
The system shall write a structured audit record for every order write-back attempt, including outcome, AM order number, and any error detail.

#### FR-ORD-011 — PayU reconciliation
The system shall provide a PayU reconciliation service that validates payment state against AM invoice records (`arinvc`).

---

### WS-02 · Consultants and MLM

**Context:** 10,308 active consultants with MLM hierarchy, pricing tiers, and exclusive product access. Consultant identity in Shopify is owned by DBM, which projects from AccountMate `arcust`.

#### FR-CON-001 — Consultant identity sync
The system shall sync consultant records from AccountMate `arcust` to Shopify customers, populating DBM-owned customer metafields with consultant classification, status, and entitlement data.

#### FR-CON-002 — Sync cadence
Consultant sync shall run on the same schedule as the legacy `/syncstaff.api` (1st of each month, with delta events triggered on lifecycle changes).

#### FR-CON-003 — Consultant lifecycle — activation
The system shall handle consultant activation events in AccountMate and provision the corresponding Shopify customer account with correct access entitlements.

#### FR-CON-004 — Consultant lifecycle — deactivation
The system shall handle consultant deactivation in AccountMate and revoke Shopify-side pricing and exclusive-item entitlements accordingly.

#### FR-CON-005 — Consultant lifecycle — reactivation
The system shall handle consultant reactivation via `sp_ws_reactivate` and reinstate Shopify-side access.

#### FR-CON-006 — New consultant onboarding
The system shall provide a new consultant onboarding intake flow that improves on the legacy `nopnewregistrations.prg` process: consultant number assignment (DBM initiates → AM assigns → DBM stores), initial Shopify account provisioning, and welcome communications trigger.

#### FR-CON-007 — Exclusive item access sync
The system shall sync per-consultant exclusive item eligibility from AccountMate `soxitems` (21,682 rows) to Shopify customer metafields. Sync shall run on a configurable schedule with delta detection.

#### FR-CON-008 — MLM hierarchy read-access
The system shall read consultant hierarchy and downline data from `compplanLive` for reporting and entitlement validation purposes. The commission engine itself is not replaced in phase 1.

#### FR-CON-009 — Consultant number as stable key
The AM `arcust.cCustNo` consultant number shall be the stable identity key across all DBM ↔ AM operations. It must be stored durably in DBM before any Shopify write is attempted.

---

### WS-03 · Products and Inventory

**Context:** 135 published SKUs (of ~800 in AM `icitem`). Products and inventory levels are AM-authoritative. DBM is a read-and-push layer only — it never creates or modifies AM product records.

#### FR-PROD-001 — Full product sync
The system shall perform a full product catalogue sync from AccountMate `icitem` to Shopify products/variants on a configurable schedule, creating or updating Shopify product records to match the AM master.

#### FR-PROD-002 — Delta product sync
The system shall perform delta product sync by polling AM change indicators (last-modified timestamps on `icitem`) at the configured interval, pushing only changed records to Shopify.

#### FR-PROD-003 — Product sync cadence
Full product sync shall run at least 3 times per day. Delta sync shall run every 15 minutes between 07:00 and 17:00, matching the confirmed legacy SQL Agent job schedule.

#### FR-PROD-004 — Inventory sync
The system shall sync inventory availability levels from AccountMate to Shopify inventory levels at a minimum 5-minute interval.

#### FR-PROD-005 — Image sync
The system shall sync product images from AccountMate to Shopify, handling image create, update, and removal idempotently.

#### FR-PROD-006 — Category and manufacturer mapping
The system shall map AccountMate product categories and manufacturer records to the corresponding Shopify taxonomy (collections, metafields).

#### FR-PROD-007 — Inventory reconciliation
The system shall run a nightly inventory reconciliation comparing DBM-synced values against AM stock levels and flagging discrepancies in the admin console.

---

### WS-04 · Pricing, Campaigns, and Exclusive Access

**Context:** This is the most complex and commercially sensitive domain. 77 active campaigns, 18,571 pricing rows, consultant-specific effective prices computed from `sp_camp_getSPprice`. Pricing errors are immediately visible to consultants and financially material.

#### FR-PRC-001 — Pricing oracle polling
The system shall poll AccountMate `sp_camp_getSPprice` (the authoritative pricing oracle) on a configurable schedule to determine effective consultant prices per SKU.

#### FR-PRC-002 — Effective price formula
The effective consultant price per SKU shall be computed as:
- `CampDetail.nPrice` if an active campaign row exists for the SKU and the current date falls within `dFrom`/`dTo`
- else `icitem.nprice` × 0.80 (flat 20% consultant discount)

This formula is confirmed from SQL archaeology of `sp_camp_getSPprice` and `sp_ct_updatedisc`.

#### FR-PRC-003 — Price metafield sync
The system shall sync precomputed effective consultant prices to Shopify product metafields (DBM-owned namespace) for all active SKUs.

#### FR-PRC-004 — Shopify Function — pricing discount
The custom Shopify Function shall read the effective price metafield at checkout and apply a deterministic discount delta (`retail_price - effective_consultant_price`) for authenticated consultants.

#### FR-PRC-005 — Campaign boundary sweep
The system shall run a mandatory pricing boundary sweep every 5–15 minutes to catch `dFrom`/`dTo` campaign activation and expiry transitions that occur without an explicit edit event on `CampDetail`. This is non-negotiable — the legacy system missed these transitions and it caused visible pricing errors.

#### FR-PRC-006 — Delta pricing sync
The system shall detect pricing changes via `CampDetail.dLastUpdate` and `Campaign.dLastUpdate` and push changed prices to Shopify metafields without requiring a full pricing sweep.

#### FR-PRC-007 — Pricing parity harness
The system shall maintain a pricing parity harness that compares DBM-precomputed effective prices against live `sp_camp_getSPprice` oracle calls for a representative sample of SKUs on each sync cycle. Discrepancies shall be surfaced in the admin console and trigger an alert.

#### FR-PRC-008 — Shopify Function — exclusive-access gate
The custom Shopify Function shall validate at checkout that a consultant attempting to purchase an exclusive item has the item in their `soxitems`-derived eligibility metafield. Ineligible items shall be blocked at checkout with a user-visible message.

#### FR-PRC-009 — Pricing reconciliation
The system shall run a nightly full-catalogue pricing reconciliation comparing all synced Shopify metafield prices against the AM oracle. Mismatches shall be corrected automatically and logged.

---

### WS-05 · Communications, Marketing, and OTP

**Context:** Multiple active communication channels currently owned by NISource VFP. OTP (`/otpGenerate.api`) is the live consultant login gate — the single highest-risk endpoint to replace. A new dedicated Brevo instance handles ecommerce communications; the legacy `compsys`/`sp_Sendmail` pathway for AM internal operations must not be disrupted.

#### FR-COM-001 — OTP dispatch
The system shall provide an OTP generation and dispatch service that replaces the behaviour of `shopapi.annique.com/otpGenerate.api`. The replacement must be functionally equivalent for all active consultant login flows before cutover. (TC-04 open — full behaviour profile pending Annique IT confirmation.)

#### FR-COM-002 — SMS dispatch
The system shall provide an SMS dispatch service replacing `nopintegration.annique.com/sendsms.api` (password reset and OTP SMS). SMS OTP is not required for ecommerce authentication (TC-03 closed) but SMS dispatch capability is required for other flows.

#### FR-COM-003 — Registration validation
The system shall provide a registration validation service replacing `nopintegration.annique.com/api/ValidateNewRegistration/`.

#### FR-COM-004 — Brevo — new dedicated instance
The system shall integrate with a new dedicated Brevo instance for all DBM ecommerce communications (order confirmations, consultant onboarding, campaign notifications). This Brevo instance is separate from any legacy Brevo configuration. The legacy `compsys`/`sp_Sendmail` pathway for AM internal operations must not be touched.

#### FR-COM-005 — Brevo — transactional email
The system shall send order confirmation and consultant lifecycle emails via the dedicated Brevo instance, using Brevo templates configurable without a code deployment.

#### FR-COM-006 — Meta CAPI event forwarding
The system shall forward ecommerce conversion events to Meta CAPI. The current queue holds 81,228 events — the forwarding service must handle backlog processing and ongoing real-time forwarding.

#### FR-COM-007 — Voucher notification dispatch
The system shall replace `shopapi.annique.com/NotifyVouchers.api` — the voucher notification dispatch service that runs every 4 hours between 08:00 and midnight.

#### FR-COM-008 — Communications isolation
The system shall never write to or modify `compsys.MAILMESSAGE` or invoke `sp_Sendmail`. AM internal communications are not DBM's responsibility.

---

### WS-06 · Reporting and Operational Visibility

**Context:** 52 `NopReports` entries require classification (AM-only / ecommerce / consultant self-service / hybrid — deep dive in `docs/analysis/25_reporting_deep_dive.md`). Phase 1 scope is limited to materially used ecommerce and consultant-facing reports.

#### FR-RPT-001 — Report classification
The system shall support the classified ecommerce and consultant self-service reports from the `NopReports` deep dive. AM-only reports are excluded from DBM scope.

#### FR-RPT-002 — Consultant self-service reports
The system shall expose a consultant self-service report data service (accessible via the DBM admin console or a consultant-facing portal endpoint) covering the confirmed materially used consultant reports.

#### FR-RPT-003 — Operational reports
The system shall expose operational reports in the DBM admin console covering sync health, order volumes, pricing anomalies, and consultant activity metrics.

#### FR-RPT-004 — Report data API
Consultant and operational report data shall be served via a DBM API endpoint — not by direct AM SQL queries from the admin console UI.

---

### WS-07 · Identity, SSO, and Access Control

**Context:** Consultants are a distinct class of storefront user. Their access is gated by AM classification. Shopify handles storefront authentication; DBM owns the consultant identity layer.

#### FR-IDN-001 — Consultant vs. consumer identity model
The system shall maintain a clear identity boundary between consultants (AM-originated, DBM-synced) and direct consumers (Shopify-native). Consultants shall be identifiable at checkout for pricing and access-gate purposes.

#### FR-IDN-002 — OTP login replacement
The system shall replace the current OTP-based consultant login mechanism. The Shopify storefront handles authentication natively; DBM provides the OTP generation and validation service for consultant identity operations that require it.

#### FR-IDN-003 — Role and access mapping
The system shall maintain the Shopify-side role mapping (consultant vs. consumer) via customer metafields, enabling Shopify Functions and theme extensions to apply the correct pricing and access logic without additional API calls at checkout.

#### FR-IDN-004 — Staff access to Shopify Admin
The system shall support staff (Annique and Dieselbrook operational users) accessing the Shopify Admin and the DBM admin console with appropriate role boundaries.

#### FR-IDN-005 — Guest checkout
Guest checkout shall be available in the new platform. It is not available in the current NopCommerce store and is a non-negotiable scope requirement (TC-06 closed).

---

### WS-08 · Admin Console (DBM)

**Context:** Confirmed scope addition (D-09). DBM requires a dedicated admin console. This is not a minimal ops-only tooling set — it is the primary operational interface for Annique and Dieselbrook staff to manage the platform.

#### FR-ADM-001 — Campaign management
The admin console shall provide a campaign management interface allowing authorised users to view, and where appropriate administer, campaign definitions sourced from AccountMate.

#### FR-ADM-002 — Consultant operations
The admin console shall provide a consultant operations interface covering consultant lifecycle management, support tooling, and entitlement visibility.

#### FR-ADM-003 — Sync status dashboard
The admin console shall display the current sync status for each DBM domain service — last successful run time, next scheduled run, failure count, and any items in dead-letter queues.

#### FR-ADM-004 — Reconciliation dashboard
The admin console shall display the results of the most recent pricing, inventory, and consultant entitlement reconciliation runs, including any detected discrepancies.

#### FR-ADM-005 — Dead-letter management
The admin console shall allow operators to view, inspect, replay, and dismiss dead-lettered operations across all domain services.

#### FR-ADM-006 — System configuration
The admin console shall allow authorised users to manage configurable system parameters (sync intervals, threshold values, feature flags) without a code deployment.

#### FR-ADM-007 — Audit log access
The admin console shall provide searchable access to the structured audit log for all ERP write-back operations, filterable by order, consultant, and time range.

---

### X-01 · Platform Runtime and Hosting (Cross-cut)

#### FR-PLT-001 — Azure deployment
DBM shall be deployed to Azure App Service (or Azure Container Apps) within the existing Annique Azure virtual network, using the confirmed private routing path to the on-premises AccountMate SQL estate on `AMSERVER-v9`.

#### FR-PLT-002 — Environment provisioning
The system shall have three environments: development, staging (connected to staging AM at `196.3.178.122:62111`), and production (connected to live AM at `172.19.16.100:1433`).

#### FR-PLT-003 — Secrets management
All credentials — database connection strings, Shopify API tokens, Brevo API keys — shall be stored in Azure Key Vault and injected at runtime. No credentials shall appear in source code or version control.

#### FR-PLT-004 — Job scheduler
Background sync jobs shall be managed by Quartz.NET or Hangfire with configurable cron schedules, execution history, and distributed clustering support.

#### FR-PLT-005 — Async queue
The webhook-to-write-back processing pipeline shall use Azure Service Bus for durable async processing, retry policies, and dead-letter queuing.

---

### X-02 · Data Access and SQL Contract (Cross-cut)

#### FR-SQL-001 — Least-privilege SQL login
DBM shall connect to AccountMate using a dedicated least-privilege SQL login — not the `sa` account. The login shall have scoped read access and controlled write access only via the approved stored procedure boundary.

#### FR-SQL-002 — No schema modification
DBM shall never issue DDL statements against AccountMate databases. No tables, columns, indexes, or stored procedures shall be created or modified in AM.

#### FR-SQL-003 — Stored procedure boundary
All AM writes from DBM shall be via the approved stored procedure contract documented in `docs/analysis/24_dbm_am_interface_contract.md`. Direct table writes require explicit approval.

#### FR-SQL-004 — Sync watermarks
DBM shall maintain per-domain sync watermarks in its own state database, used to drive delta queries against AM change indicators.

#### FR-SQL-005 — Database coverage
DBM requires access to: `amanniquelive` (primary), `compplanLive` (read-only for hierarchy and reporting), `compsys` (read-only for boundary confirmation, no writes).

---

### X-03 · Auditability, Idempotency, and Reconciliation (Cross-cut)

#### FR-AUD-001 — Idempotency keys on all inbound operations
Every inbound operation (webhook, scheduled sync, OTP request) shall be assigned a stable idempotency key before any external write is attempted. The key shall be stored durably. See `docs/spec/00_spec_conventions.md §6` for key patterns by operation type.

#### FR-AUD-002 — Exactly-once ERP write semantics
No operation shall write to AccountMate more than once for the same logical event, regardless of retries, replays, or duplicate webhook deliveries.

#### FR-AUD-003 — Structured audit log
Every ERP write-back operation shall produce a structured audit record containing: operation type, idempotency key, source event reference, AM entity affected, outcome, timestamp, and any error detail.

#### FR-AUD-004 — Dead-letter capture
Operations that fail after exhausting their retry policy shall be captured in a dead-letter store with full context for operator replay.

#### FR-AUD-005 — Health endpoints
The system shall expose health check endpoints (`/health`, `/health/ready`, `/health/live`) compatible with Azure App Service health probes and Application Insights.

#### FR-AUD-006 — Alerting
The system shall trigger alerts (configurable via Application Insights) on: dead-letter queue depth exceeding threshold, pricing parity check failure, consecutive sync failures per domain, and health check degradation.

---

## 5. Non-Functional Requirements

### 5.1 Performance

| Metric | Requirement | Rationale |
|---|---|---|
| Webhook acknowledgement | < 5 seconds from delivery | Shopify retries after 5s |
| Order write-back (end-to-end) | < 30 seconds from webhook receipt to AM confirmation | Operational visibility expectation |
| Inventory sync latency | ≤ 5 minutes from AM change to Shopify update | Legacy sync cadence (5 min confirmed) |
| Product delta sync | ≤ 15 minutes from AM change to Shopify update | Matches legacy SQL Agent cadence |
| Pricing boundary sweep | ≤ 15 minutes from campaign transition to Shopify update | Non-negotiable — legacy missed transitions caused errors |
| Admin console page load | < 3 seconds for dashboard views | Operational usability |
| Concurrent order throughput | 50 concurrent orders without degradation | ~10K/month = ~14/hour peak capacity headroom |

### 5.2 Reliability

| Metric | Requirement |
|---|---|
| Webhook receiver uptime | 99.5% monthly |
| Order write-back success rate | 99.9% (with retry) |
| Sync service availability | 99.0% monthly |
| Data loss tolerance | Zero — all failed operations must be dead-lettered, not dropped |

### 5.3 Security

| Requirement | Detail |
|---|---|
| No credentials in source | All secrets in Azure Key Vault — enforced via CI/CD policy |
| Least-privilege AM access | Dedicated SQL login with scoped permissions |
| Shopify authentication | Bearer token (OAuth) — same pattern as AnniqueAPI |
| Admin console access | Authenticated — role-based access for Annique staff and Dieselbrook operators |
| Transport security | HTTPS/TLS for all external-facing endpoints |
| Audit trail | All AM write-back operations logged with operator identity where applicable |

### 5.4 Observability

| Requirement | Detail |
|---|---|
| Structured logging | Serilog with Application Insights sink — all services |
| Distributed tracing | Application Insights correlation IDs across service boundaries |
| Metrics | Sync job execution counts, durations, failure rates per domain |
| Health probes | `/health` endpoints on all services — integrated with Azure App Service |
| Alerting | Application Insights alert rules on key failure conditions |

### 5.5 Maintainability

| Requirement | Detail |
|---|---|
| Pattern reference | All DBM services follow the patterns established in `AnqIntegrationApiSource/` |
| Greenfield codebase | No inheritance from NISource or NopCommerce codebases |
| Configuration-driven sync intervals | All sync cadences configurable without code deployment |
| Replay tooling | All dead-lettered operations replayable from admin console |

---

## 6. Phase 1 vs. Phase 2 Scope

### Phase 1 — Must deliver by end of July 2026

All workstreams defined in Section 4 are Phase 1 scope, subject to the boundaries below.

### Phase 1 — In scope

| Workstream | Scope |
|---|---|
| WS-01 Orders | Full — order write-back, cancellation, status sync, PayU reconciliation |
| WS-02 Consultants | Full — identity sync, lifecycle, onboarding, exclusive items, MLM hierarchy read |
| WS-03 Products/Inventory | Full — full + delta sync, inventory, images |
| WS-04 Pricing/Campaigns | Full — oracle polling, metafield sync, boundary sweeps, parity harness, Shopify Functions |
| WS-05 Communications | Full — OTP (pending TC-04), SMS, registration validation, Brevo, Meta CAPI, vouchers |
| WS-06 Reporting | Partial — materially used operational and consultant self-service reports only |
| WS-07 Identity/SSO | Full — consultant vs. consumer model, OTP login, guest checkout |
| WS-08 Admin Console | Full — campaign management, consultant ops, sync status, reconciliation, dead-letter management |
| X-01 Platform | Full — Azure hosting, environments, secrets, scheduler, queue |
| X-02 SQL Contract | Full — least-privilege login, SP boundary, watermarks |
| X-03 Auditability | Full — idempotency, audit log, dead-letter, health, alerting |
| Awards (843 issued) | Phase 1 — awards are actively used |
| Bookings (940 records) | Phase 1 — bookings are actively used |
| Gifts (12 configured) | Phase 1 — basic gift functionality |
| Meta CAPI (81,228 queued) | Phase 1 — queue backlog processing + ongoing forwarding |

### Phase 2 and beyond — explicitly deferred

| Item | Reason |
|---|---|
| AM commission engine replacement | AM retains authority — phase 1 is read-only access to `compplanLive` |
| AM accounting and AR modernisation | ERP-side financial records stay in AM |
| Full consultant portal redesign | Parity-first in phase 1; portal redesign is a separate future programme |
| Deep BI/reporting platform modernisation | Only materially used reports gate phase 1 |
| Full admin UI parity | DBM admin console covers operational need; legacy admin UI surfaces challenged and deferred |
| Events feature | Only 5 events configured — lowest priority; phase 2 candidate |
| Chatbot | Disabled in current NopCommerce — phase 2 or permanent deferral |
| Multi-store / Namibia | Out of scope entirely |
| Shopify B2B native tooling | Ruled out — DBM-owned model is confirmed architecture |

### Parallel run

NopCommerce stays live in parallel until actual handover and client sign-off. The `[WEBSTORE]` linked server and dependent stored procedures (`sp_ws_autopickverify`, exclusive-item SPs) remain active until the handover point. DBM and NopCommerce will operate simultaneously during the transition period.

---

## 7. Definition of Done

A workstream is done when:

1. **Specification complete** — domain spec (`spec.md`, `state_machine.md`, `api_contract.md`, `reconciliation_rules.md`, `acceptance_criteria.md`) written and reviewed
2. **Implementation complete** — all services in the workstream implemented and passing unit tests
3. **Integration tests pass** — all field mappings verified against staging AM (`196.3.178.122:62111`)
4. **Parity verified** — for pricing and inventory: DBM output matches AM oracle output for a representative SKU sample
5. **Acceptance criteria met** — all `AC-<DOMAIN>-xxx` criteria in `acceptance_criteria.md` verified
6. **Dead-letter tested** — failure and retry paths verified; dead-letter capture confirmed working
7. **Audit log verified** — all write-back operations produce correct audit records
8. **Admin console wired** — domain-relevant admin console views showing live data
9. **Cutover checklist item** — workstream has a named item in the cutover checklist

### Per-domain DoD additions

| Domain | Additional DoD requirement |
|---|---|
| Orders | ERP write verified on staging AM; consultant classification confirmed in AM record |
| Pricing | Parity harness green for 100% of active campaign SKUs |
| OTP/Identity | OTP flow tested end-to-end with live consultant credentials (staging) |
| Exclusive items | Checkout gate confirmed blocking ineligible purchase |
| Admin console | Campaign management, sync status, and dead-letter views all operational |

---

## 8. Service Inventory

The 25 DBM services that must be built:

| # | Service | Workstream | Phase |
|---|---|---|---|
| 1 | `ShopifyAppBackend` | X-01 | 1 |
| 2 | `WebhookReceiver` | WS-01 | 1 |
| 3 | `OrderWritebackService` | WS-01 | 1 |
| 4 | `OrderCancellationService` | WS-01 | 1 |
| 5 | `OrderStatusFulfillmentService` | WS-01 | 1 |
| 6 | `ProductSyncService` | WS-03 | 1 |
| 7 | `InventorySyncService` | WS-03 | 1 |
| 8 | `ImageSyncService` | WS-03 | 1 |
| 9 | `CustomerSyncService` | WS-02 | 1 |
| 10 | `ConsultantSyncService` | WS-02 | 1 |
| 11 | `ExclusiveItemsSyncService` | WS-02 / WS-04 | 1 |
| 12 | `CampaignPriceSyncService` | WS-04 | 1 |
| 13 | `PricingParityService` | WS-04 | 1 |
| 14 | `ConsultantHierarchyAndMLMAccessService` | WS-02 | 1 |
| 15 | `ConsultantReportingDataService` | WS-06 | 1 |
| 16 | `PayUReconciliationService` | WS-01 | 1 |
| 17 | `SmsService` | WS-05 | 1 |
| 18 | `OtpService` | WS-05 | 1 |
| 19 | `RegistrationValidationService` | WS-02 | 1 |
| 20 | `BrevoSyncService` | WS-05 | 1 |
| 21 | `MetaCapiService` | WS-05 | 1 |
| 22 | `VoucherNotificationService` | WS-05 | 1 |
| 23 | `AuditLogService` | X-03 | 1 |
| 24 | `JobOrchestrationService` | X-01 | 1 |
| 25 | `ReconciliationDashboardService` | X-03 | 1 |

Custom Shopify app components:

| # | Component | Phase |
|---|---|---|
| 1 | App backend (OAuth, webhook registration, API plumbing) | 1 |
| 2 | Shopify Function — pricing discount (Rust) | 1 |
| 3 | Shopify Function — exclusive-access gate (Rust) | 1 |
| 4 | Checkout UI Extensions | 1 |
| 5 | Theme app extensions | 1 |
| 6 | Metafield/metaobject schema registration | 1 |
| 7 | Embedded admin pages (Remix) | 1 |

---

## 9. Open Items

These items are tracked in the decisions register (`docs/analysis/02_open_decisions_register.md`) and Linear:

| ID | Item | Owner | Impact |
|---|---|---|---|
| TC-04 | `shopapi.annique.com` source, stack, and owner — 3 confirmed endpoints including the live OTP login gate | Annique IT | Blocks detailed identity/OTP spec |
| TC-05 | `SyncCancelOrders.api` and `sp_ws_invoicedtickets` operational status | Annique IT | Blocks order cancellation spec detail |
| D-08 | Additional `nopintegration.annique.com` browser-facing modules beyond campaign | Annique | Low priority — current analysis not blocked |
| X-DEP-04 | Which of 52 `NopReports` entries are actively used in production | Annique + Dieselbrook | Blocks final reporting scope |

---

## 10. Evidence Base

| Artefact | Location |
|---|---|
| Open decisions register | `docs/analysis/02_open_decisions_register.md` |
| Phase-1 boundary summary | `docs/analysis/19_phase1_replacement_boundary_summary.md` |
| Solution overview and scope | `docs/analysis/26_solution_overview_scope_cost.md` |
| DBM ↔ AM interface contract | `docs/analysis/24_dbm_am_interface_contract.md` |
| Pricing engine deep dive | `docs/analysis/20_pricing_engine_deep_dive.md` |
| Orders domain pack | `docs/analysis/08_orders_domain_pack.md` |
| Consultants/MLM domain pack | `docs/analysis/09_consultants_mlm_domain_pack.md` |
| Pricing/campaigns domain pack | `docs/analysis/10_pricing_campaigns_exclusives_domain_pack.md` |
| Products/inventory domain pack | `docs/analysis/11_products_inventory_domain_pack.md` |
| Communications domain pack | `docs/analysis/12_communications_marketing_domain_pack.md` |
| Identity/SSO domain pack | `docs/analysis/14_identity_sso_domain_pack.md` |
| Admin/back-office domain pack | `docs/analysis/15_admin_backoffice_domain_pack.md` |
| Reporting deep dive | `docs/analysis/25_reporting_deep_dive.md` |
| Orders middleware design | `middleware/orders/` |
| Consultants middleware design | `middleware/consultants/` |
| NISource VFP source (behaviour reference) | `NISource/` |
| AnniqueAPI (.NET 9 pattern reference) | `AnqIntegrationApiSource/` |
| Spec conventions | `docs/spec/00_spec_conventions.md` |
