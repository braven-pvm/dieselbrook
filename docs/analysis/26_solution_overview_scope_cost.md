# Annique → DBM · Solution Overview, Scope & Cost

**Document type:** High-level technical overview — scope and commercial model  
**Programme:** Annique Cosmetics MLM platform → Shopify Plus migration and Dieselbrook Middleware (DBM)  
**Build model:** DBM is a clean greenfield implementation — no inheritance from existing Annique middleware codebases  
**Client:** Annique / Delivered by: Dieselbrook  
**Prepared by:** Braven Lab  
**Date:** 2026-03-15  
**Version:** 1.0 — all programme decisions closed  
**Status:** Ready for internal review and client presentation

---

## 1. Executive Summary

This programme replaces the Annique Cosmetics ecommerce and integration estate — currently a legacy NopCommerce storefront, VFP Web Connection (`NISource`) middleware layer, and a suite of AccountMate SQL jobs — with a modern Shopify Plus storefront backed by a purpose-built Dieselbrook Middleware (DBM) platform.

**This is primarily an integration and middleware engineering programme, not a storefront project.**

The storefront surface (Shopify Plus) is commercially established and needs relatively little custom build. The critical and complex work is the middleware layer that bridges Shopify and AccountMate, preserving 15+ years of MLM business logic, consultant pricing, exclusive-item access, order flow, and operational integrations that currently live in the legacy stack.

### Key programme facts at decision-close (2026-03-15)

| Fact | Value |
|---|---|
| Target platform | Shopify Plus |
| Target middleware | Dieselbrook Middleware (DBM) — custom .NET 9 platform (greenfield) |
| ERP | AccountMate (on-premises, Azure-connected) — retained as-is |
| All programme decisions closed | ✅ D-01 through D-10, TC-02/03/06/07/08 — 2026-03-15 |
| Confirmed go-live | End July 2026 |
| Weeks remaining from today | ~20 weeks |
| Production scale | ~10,000 orders/month · 71,097 customers · 135 published SKUs · 21,682 exclusive item rows |
| Namibia | Out of scope |

### What Dieselbrook is building

1. **DBM middleware platform** — 25 domain and platform services handling all AM↔Shopify data flows
2. **Custom Shopify app** — Shopify Functions (pricing, access control), Checkout UI Extensions, admin shell
3. **Dedicated DBM admin console** — campaign management, consultant operations, reporting, monitoring
4. **Migration, UAT, and cutover** — data validation, parallel-run planning, hypercare

### What is not being changed

- AccountMate ERP (retained as system of record for ERP, finance, MLM, consulting)
- AM pricing engine (`sp_camp_getSPprice` — read by DBM, not replaced)
- AM commission engine (`compplanLive` — read-access only in phase 1)
- AM's `compsys`/`sp_Sendmail` internal communications pathway

---

## 2. Programme Context

### 2.1 What the legacy estate is

The current Annique platform is a three-tier legacy stack:

```
┌───────────────────────────────────────┐
│  NopCommerce storefront               │   - Custom build (297 C# files, 40 tables,
│  + 297 C# custom plugin files         │     73 custom stored procedures)
│  + 40 custom DB tables                │   - Azure-hosted
│  + 73 custom stored procedures        │
└───────────────────┬───────────────────┘
                    │
┌───────────────────▼───────────────────┐
│  NISource (VFP Web Connection)        │   - Visual FoxPro middleware layer
│  nopintegration.annique.com           │   - Handles sync, OTP, registration,
│  shopapi.annique.com                  │     SMS, affiliate sync, vouchers
└───────────────────┬───────────────────┘
                    │
┌───────────────────▼───────────────────┐
│  AccountMate SQL Server               │   - On-premises (AMSERVER-v9)
│  amanniquelive / compplanLive         │   - Private routing from Azure tier
│  compsys / amanniquenam               │   - ERP, MLM, commission, AM SQL jobs
└───────────────────────────────────────┘
```

NISource (`nopintegration.annique.com`) is VFP Web Connection — a 1990s-era technology with no practical maintainability path. It must be fully replaced. It currently owns:

- Order write-back (`syncorders.prg`)
- Order status sync (`syncorderstatus.prg`)
- Product/inventory sync (`syncproducts.prg`)
- Consultant sync (`syncconsultant.prg`)
- Campaign pricing sync (`brevoprocess.prg`, `communicationsapi.prg`)
- OTP generation and SMS dispatch
- Registration validation (`ValidateNewRegistration`)
- Voucher notification dispatch (`shopapi.annique.com/NotifyVouchers.api`)

### 2.2 Decision baseline — all gates closed

All programme decision gates are closed. The table below records the confirmed position of each.

| Decision | Topic | Resolution |
|---|---|---|
| D-01 | Consultant/customer model | DBM owns consultant identity (not native Shopify). Consultants are a distinct class: ERP lifecycle, MLM hierarchy, exclusive access, and pricing eligibility all managed in DBM and projected into Shopify via sync. |
| D-02 | Pricing architecture | DBM reads `sp_camp_getSPprice` (AM pricing oracle), precomputes effective consultant prices, syncs to Shopify product metafields. Shopify Function applies deterministic discount delta at checkout. DBM owns pricing end-to-end. |
| D-03 | Legacy admin scope | Phased approach: replace all operationally necessary functions. DBM admin console is the replacement surface (D-09). |
| D-05 | Comms routing | New dedicated Brevo instance for DBM ecommerce. Legacy `compsys`/`sp_Sendmail` stays for AM internal ops. Clean split. |
| D-06 | Shopify plan tier | Shopify Plus confirmed. Required for: custom Function deployment, Checkout Extensibility, Shopify Flow, advanced API limits, unlimited staff. |
| D-07 | NopCommerce plugin scope | All plugins in scope for phase 1 — Awards (843 issued), Bookings (940 records), Gifts (12 configured), OTP (live), Meta CAPI (81,228 queued). Events (5 configured) and Chatbot (disabled) lower priority. |
| D-08 | NISource browser modules | Full source access pending from Annique — deep dive after access granted. |
| D-09 | Admin console | Confirmed: DBM requires a dedicated admin console. Scope: campaign management, consultant operations, reporting, system monitoring, configuration. |
| D-10 | Report scope | Reporting deep dive commissioned. 52 NopReports definitions to be classified (AM-only / ecommerce / consultant self-service / hybrid). Annique confirmation of actively used reports pending. |

---

## 3. Target Architecture

### 3.1 Architecture principle

> **Event-driven on the Shopify side. Poll-and-reconcile on the AccountMate side.**

Shopify pushes events by webhook. AccountMate is a SQL-centric ERP with no outbound event capability — DBM polls against change indicators, boundary sweeps for time-based pricing, and nightly reconciliation for drift detection.

### 3.2 Logical architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                         SHOPIFY PLUS                             │
│                                                                  │
│  Storefront · Theme · Checkout · Customers · Products            │
│  Orders · Inventory · Metafields · Metaobjects                   │
│  Shopify Functions · Checkout UI Extensions · Theme Extensions   │
│  Shopify Flow · Admin GraphQL API · Webhooks                     │
└─────────────────────────────┬────────────────────────────────────┘
                              │ HTTPS (webhooks + Admin GraphQL)
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│             DIESELBROOK MIDDLEWARE (DBM)  ·  Azure               │
│                                                                  │
│  API Layer                                                        │
│  ├─ Shopify OAuth + app backend                                  │
│  ├─ Webhook receivers (orders, customers, fulfillments, cancel)  │
│  └─ Legacy-compatible endpoints where required                   │
│                                                                  │
│  Domain Services (25 services)                                   │
│  ├─ Orders: write-back · cancellation · status/fulfillment       │
│  ├─ Products: full sync · delta sync · image sync                │
│  ├─ Inventory: availability sync · reconciliation                │
│  ├─ Consultants: identity sync · lifecycle events · hierarchy    │
│  ├─ Pricing/campaigns: oracle polling · metafield sync           │
│  │   · parity harness · boundary sweeps                          │
│  ├─ Exclusive items: soxitems sync → customer metafields         │
│  ├─ Communications: OTP · SMS · Brevo · Meta CAPI · Vouchers     │
│  ├─ Registration: new consultant intake · AM write-back          │
│  ├─ PayU: reconciliation service                                 │
│  └─ Reporting: consultant/operational report data service        │
│                                                                  │
│  Platform Services                                               │
│  ├─ Job scheduler (Quartz.NET / Hangfire)                        │
│  ├─ Queue + dead-letter (Azure Service Bus)                      │
│  ├─ Audit logger                                                 │
│  ├─ Health monitoring + alerting                                 │
│  └─ Reconciliation and replay tooling                            │
│                                                                  │
│  DBM Admin Console                                               │
│  ├─ Campaign management                                          │
│  ├─ Consultant operations                                        │
│  ├─ Reporting dashboard                                          │
│  ├─ Sync status and diagnostics                                  │
│  └─ Configuration and monitoring                                 │
│                                                                  │
│  Middleware State (Azure SQL / SQL Server)                       │
│  └─ Sync watermarks · job history · idempotency keys            │
│      · failure logs · reconciliation results                     │
└─────────────────────────────┬────────────────────────────────────┘
                              │
                              │ Direct SQL — secure private connectivity
                              │ (existing Azure → on-prem routing,
                              │  Dieselbrook joins same path)
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│                   ACCOUNTMATE SQL ESTATE  ·  On-premises         │
│                                                                  │
│  amanniquelive   — orders, customers, products, inventory       │
│  compplanLive    — MLM hierarchy, downlines, commission data     │
│  compsys         — AM internal comms (sp_Sendmail — not touched) │
│  amanniquenam    — Namibia ERP (out of scope)                    │
│                                                                  │
│  Key stored procedures called by DBM:                            │
│  sp_camp_getSPprice · sp_ws_reactivate · sp_ws_gensoxitems       │
│  sp_ws_syncorder + related order write-back procedures           │
└──────────────────────────────────────────────────────────────────┘
```

### 3.3 Custom Shopify app components

The custom private Shopify app (Plus-only deployment) contains:

| Component | Purpose |
|---|---|
| Shopify Function — pricing | Reads effective consultant price from product metafields, applies deterministic discount delta at checkout for authenticated consultants |
| Shopify Function — exclusive access | Validates consultant exclusive-item eligibility at checkout using customer metafields synced from `soxitems` |
| Checkout UI Extensions | Consultant-specific pricing display, exclusive-access messaging at checkout |
| Theme app extensions | Pre-checkout visibility logic (price preview, access indicator) |
| Metafield/metaobject schema | Registers DBM-owned product and customer metafield namespaces |
| Embedded admin pages | Sync status, diagnostic views, override tooling within Shopify Admin |

### 3.4 System-of-record boundaries (confirmed — D-01, D-02)

| Domain | System of record | Notes |
|---|---|---|
| Consultant identity (ecommerce-facing) | DBM | DBM projects from AM; owns Shopify-facing state |
| Consultant identity (ERP lifecycle) | AccountMate | Number assignment, activation, commission |
| Effective consultant pricing | DBM | Computed from AM oracle, synced to Shopify |
| Campaign pricing definition | AccountMate | AM authority; DBM reads on schedule |
| Exclusive item eligibility | AccountMate (`soxitems`) | DBM reads and syncs to customer metafields |
| Inventory master | AccountMate | DBM reads and syncs to Shopify |
| Product master | AccountMate | DBM reads and syncs to Shopify |
| Orders (inbound to ERP) | DBM write-back service | Orders written to AM with pricing resolved |
| Ecommerce communications | DBM (new Brevo instance) | Clean split from AM/compsys pathway |
| AM internal communications | AccountMate (`compsys`) | DBM does not touch this pathway |
| MLM commission computation | AccountMate (`compplanLive`) | Read-only access from DBM; not replaced |

### 3.5 Technology stack

> **DBM is a clean greenfield implementation.** It consumes AccountMate via SQL/stored procedures and exposes Shopify-facing endpoints. It does not extend or inherit from any existing Annique codebase. Stack choices are made purely on fit for the work.

| Layer | Technology | Rationale |
|---|---|---|
| Middleware runtime | **.NET 9 / ASP.NET Core** | The dominant choice for SQL Server–heavy ERP integration. `IHostedService` + Worker Services is the natural model for DBM's 25 sync services. Azure SDK, Service Bus client, Dapper, and Serilog are all first-class in this ecosystem. Alternatives considered: Node.js (weak SQL Server/SP tooling, immature background job model), Python (no serious hosted services framework for this pattern), Go (thin SQL Server support, no Azure Service Bus SDK maturity). None match .NET 9 for this specific workload. |
| Data access | **Dapper** | Precise, lightweight control over stored procedure calls and parameterised queries. Avoids ORM abstraction overhead that would conflict with AccountMate's SP-centric design. No migrations — DBM never owns the AM schema. |
| Background jobs | **Quartz.NET or Hangfire** | Reliable recurring job scheduling with execution history, cron expressions, retry orchestration, and clustered execution. Core engine for all sync polling services. |
| Queue / async decoupling | **Azure Service Bus** | Durable async processing for webhook-triggered flows, dead-letter queues, and retry policies. Decouples webhook receiver from write-back processing. |
| Middleware state persistence | **Azure SQL / SQL Server** | DBM's own state — sync watermarks, idempotency keys, job history, failure logs, audit records, reconciliation results. Separate schema from AccountMate. |
| Logging | **Serilog + Application Insights** | Structured logs with Azure-native sink. Application Insights provides live metrics, alerting, and distributed tracing at low operational overhead. |
| Shopify Functions | **Rust** | The only viable language for custom Shopify Functions in private apps. Deterministic, fast, sandboxed — correct tool for pricing delta and access-gate logic. |
| Checkout UI extensions | **React + Shopify UI Extensions SDK** | Standard Shopify-mandated model for Checkout Extensibility. |
| Theme extensions | **Liquid + minimal JS** | Pre-checkout visibility logic. Low surface area. |
| Shopify app shell + admin console | **Node.js / Remix + Shopify Polaris** | Shopify CLI scaffolds Remix for embedded admin apps natively. Polaris component library is JS-first. The admin console (D-09) is a Shopify-embedded surface — Node.js owns this boundary cleanly. DBM's .NET 9 API provides the data endpoints; the Remix shell renders the UI. |
| CI/CD | **GitHub Actions** | Straightforward deployment automation for both the .NET middleware and the Node.js Shopify app. |
| Secrets management | **Azure Key Vault** | Required for production hygiene — connection strings, API keys, Shopify tokens. |
| Hosting | **Azure App Service (or containerised)** | Consistent with the existing Annique Azure hosting topology. Private connectivity to on-prem AccountMate SQL estate via the confirmed routing path. |

**Stack boundary summary:**

```
Shopify platform     →  Rust (Functions) + React (extensions) + Liquid (theme)
Shopify admin app    →  Node.js / Remix / Polaris  (UI shell only)
DBM middleware core  →  .NET 9 / ASP.NET Core / Dapper / Worker Services
DBM data API         →  .NET 9 Web API  (feeds admin console and external consumers)
AccountMate access   →  Direct SQL via Dapper — stored procedures + parameterised reads
```

---

## 4. Phase 1 Scope

### 4.1 In scope

#### WS-01 · Orders and Fulfillment

- Shopify order ingest via webhook
- Order write-back to AccountMate (`sp_ws_syncorder` and related SPs) with prices pre-resolved
- Consultant vs. direct-to-consumer order classification on AM write-back
- Order cancellation handling
- Fulfillment status propagation (AM → Shopify)
- Invoice and operational dependencies (`SOPortal` connectivity)
- Idempotency, retry, and dead-letter handling for all order flows

#### WS-02 · Consultants and MLM

- Consultant identity and entitlement sync (AM `arcust` → DBM → Shopify customer metafields)
- Consultant lifecycle events (activation, deactivation, reactivation via `sp_ws_reactivate`)
- MLM hierarchy read-access for reporting and entitlement validation
- New consultant onboarding flow (replacement for `nopnewregistrations.prg`) — improved intake
- Consultant number assignment: DBM initiates → AM assigns → DBM stores
- Exclusive item access sync (`soxitems` → customer metafields)
- Exclusive item validation at checkout (Shopify Function)

#### WS-03 · Products and Inventory

- Full product catalogue sync (AM `icitem` → Shopify products/variants)
- Delta product sync driven by change indicators
- Inventory availability sync with retry and parity checks
- Image synchronisation
- Category and manufacturer mapping

#### WS-04 · Pricing, Campaigns, and Exclusives

- Campaign pricing oracle polling (`sp_camp_getSPprice`) on schedule
- Effective consultant price precomputation per SKU × campaign combination
- Price sync to Shopify product metafields
- timed boundary sweep (every 5–15 min) for `dFrom`/`dTo` campaign transitions
- Nightly pricing parity reconciliation harness (DBM price vs. AM oracle)
- Shopify Function: deterministic discount delta application at checkout for consultants
- Exclusive-item Shopify Function for checkout validation

#### WS-05 · Communications, Marketing, and OTP

- OTP dispatch replacement (current: `shopapi.annique.com/otpGenerate.api`)
- SMS dispatch replacement (current: `nopintegration.annique.com/sendsms.api`)
- Registration validation replacement (`ValidateNewRegistration`)
- New dedicated Brevo instance for ecommerce communications (order confirmations, consultant onboarding, campaign notifications)
- Meta CAPI event forwarding (81,228 queued events currently — confirmed active)
- Voucher notification dispatch replacement (`shopapi.annique.com/NotifyVouchers.api`)

#### WS-06 · Reporting and Operational Visibility

- Consultant self-service report data service
- Operational report access for the DBM admin console (confirmed scope — D-10)
- Classification of 52 `NopReports` definitions into: AM-only / ecommerce / consultant self-service / hybrid (deep dive in `analysis/25_reporting_deep_dive.md`)
- Phase 1 scope: materially used ecommerce and consultant-facing reports only
- AM-only reports excluded from phase 1

#### WS-07 · Identity, SSO, and Access Control

- Storefront identity model for consultants and direct consumers
- OTP-based login (replacing NopCommerce OTP plugin)
- Role and access mapping in Shopify (consultant vs. consumer)
- Staff SSO to Shopify Admin for Annique and Dieselbrook operational users

#### WS-08 · Admin Console (DBM)

- **Confirmed scope addition (D-09):** DBM requires a dedicated admin console
- Campaign management (campaign definition and administration surface)
- Consultant operations (lifecycle management, support tooling)
- Reporting dashboard (operational report access)
- Sync status and diagnostics (middleware health, job state, last-sync indicators)
- System configuration and monitoring

#### X-01 · Platform Runtime and Hosting (Cross-cut)

- Azure App Service hosting for DBM middleware
- Secure private connectivity to AccountMate SQL estate (joining existing Azure → on-prem private routing)
- Secrets management (Azure Key Vault)
- Environment provisioning (dev / staging / production)

#### X-02 · Data Access and SQL Contract (Cross-cut)

- Least-privilege SQL login scoped to required DBs and objects
- Stored procedure boundary: read and controlled write via the established SP contracts
- SQL watermarks for delta sync
- Coverage: `amanniquelive`, `compplanLive`, `compsys` (read-only for OTP/SMS boundary), `AnniqueNOP`-compatible interfaces

#### X-03 · Auditability, Idempotency, and Reconciliation (Cross-cut)

- Idempotency keys on all inbound event processing
- Dead-letter capture and replay tooling for failed operations
- Structured audit log for all ERP write-back operations
- Parity checks for pricing, inventory, and consultant access state
- Health endpoints and alerting
- Reconciliation dashboard in DBM admin console

### 4.2 Explicitly out of scope (phase 1)

| Item | Reason | When |
|---|---|---|
| AM commission engine replacement | AM remains authority for commission computation and MLM finance | Future phase |
| AM accounting and AR modernisation | ERP-side financial records stay in AM | Future phase |
| Full consultant portal redesign | Parity-first approach in phase 1; redesign is a later programme | Future phase |
| Deep BI/reporting platform modernisation | Only materially used operational reports gate phase 1 | Post-phase 1 |
| Namibia (`amanniquenam`) and multi-store | Out of scope — confirmed | — |
| Legacy admin UI full parity | Challenge and defer low-value surfaces; DBM admin console covers operational need | Post-phase 1 |
| Shopify B2B native tooling | Ruled out — DBM-owned model is the confirmed architecture (D-02) | Not applicable |
| AM invoice generation and procurement | ERP responsibility — not replaced | — |

### 4.3 Scope items requiring further confirmation

| ID | Item | Blocker |
|---|---|---|
| X-DEP-02 | NISource browser modules beyond campaign — full source needed | Annique to supply missing web shell |
| X-DEP-04 | Which of 52 `NopReports` entries are actively used in production | Annique + Dieselbrook business confirmation |
| TC-04 | `shopapi.annique.com` full source and ownership | Annique IT — source unknown, endpoints confirmed |
| TC-05 | `SyncCancelOrders.api` and `sp_ws_invoicedtickets` ownership | Annique IT |

---

## 5. Full Component Inventory

### 5.1 DBM middleware services — complete list

| # | Service | Workstream |
|---|---|---|
| 1 | `ShopifyAppBackend` | X-01 |
| 2 | `WebhookReceiver` | WS-01 |
| 3 | `OrderWritebackService` | WS-01 |
| 4 | `OrderCancellationService` | WS-01 |
| 5 | `OrderStatusFulfillmentService` | WS-01 |
| 6 | `ProductSyncService` | WS-03 |
| 7 | `InventorySyncService` | WS-03 |
| 8 | `ImageSyncService` | WS-03 |
| 9 | `CustomerSyncService` | WS-02 |
| 10 | `ConsultantSyncService` | WS-02 |
| 11 | `ExclusiveItemsSyncService` | WS-02 / WS-04 |
| 12 | `CampaignPriceSyncService` | WS-04 |
| 13 | `PricingParityService` | WS-04 |
| 14 | `ConsultantHierarchyAndMLMAccessService` | WS-02 |
| 15 | `ConsultantReportingDataService` | WS-06 |
| 16 | `PayUReconciliationService` | WS-01 |
| 17 | `SmsService` | WS-05 |
| 18 | `OtpService` | WS-05 |
| 19 | `RegistrationValidationService` | WS-02 |
| 20 | `BrevoSyncService` | WS-05 |
| 21 | `MetaCapiService` | WS-05 |
| 22 | `VoucherNotificationService` | WS-05 |
| 23 | `AuditLogService` | X-03 |
| 24 | `JobOrchestrationService` | X-01 |
| 25 | `ReconciliationDashboardService` | X-03 |

### 5.2 Shopify custom app — complete list

| # | Component | Purpose |
|---|---|---|
| 1 | App backend | OAuth, token handling, webhook registration, Shopify API plumbing |
| 2 | Shopify Function — pricing discount | Reads metafields, applies consultant discount delta at checkout |
| 3 | Shopify Function — exclusive-access gate | Validates consultant eligibility for protected items at checkout |
| 4 | Checkout UI Extensions | Consultant-specific pricing display and access messaging |
| 5 | Theme app extensions | Pre-checkout price visibility and access indicator |
| 6 | Metafield schema definitions | Registers DBM product and customer metafield namespaces |
| 7 | Embedded admin pages | Sync status, diagnostics, and override tooling in Shopify Admin |

---

## 6. Delivery Model — AI-Assisted Architect-Led

> This programme is delivered using an **AI-assisted, architect-led model**. A software architect defines scope, writes component-level instructions, directs AI agents for implementation, and verifies all output against acceptance criteria. Billing is architect time only — AI generation is not a separate line item.
>
> This model compresses implementation effort significantly versus a traditional engineering team. The architect's billable time is: **specification · instruction · verification · integration testing · UAT coordination**. Code generation itself is handled by AI agents under architect direction and is not billed.

### 6.1 What compresses vs. what does not

| Activity | Compression vs. traditional | Reason |
|---|---|---|
| Boilerplate, scaffolding, DTO mapping, CRUD services | **High** (~80%) | AI generates correctly from well-scoped instructions |
| Standard sync service patterns (poll, delta, watermark, retry) | **High** (~75%) | Repeatable pattern; architect writes once, AI applies per service |
| Webhook receivers and API controllers | **High** (~75%) | Well-understood HTTP patterns |
| Shopify Function logic (Rust) | **Medium** (~50%) | Deterministic logic but Rust + Shopify tooling needs careful verification |
| ERP write-back services (order, cancellation, status) | **Medium** (~40–50%) | High business risk; verification and edge-case testing takes real time regardless |
| Campaign pricing sync + boundary sweeps | **Medium** (~40–50%) | Commercially sensitive; parity harness must be verified exhaustively |
| DBM admin console UI | **Medium** (~50–60%) | UI generation is fast; business logic behind it is verified manually |
| UAT, integration testing, cutover | **Low** (~20–30%) | Client-dependent; real-world testing time does not compress significantly |
| Architecture design and field mapping | **Low** (~20%)  | Thinking work; AI assists but architect decides |

### 6.2 Architect hours by component

All times are **architect billable hours** (specification + instruction + verification + iteration). Rate: R850/hour.

| Component | Architect hours | Risk | Notes |
|---|---|---|---|
| Architecture finalisation and field mapping | 10–14h | Low | Mostly done; remaining mapping work from DB Structure CSVs |
| Middleware solution skeleton + CI/CD + environments | 4–6h | Low | Scaffold from instructions; architect reviews structure and conventions |
| Shopify app bootstrap + OAuth + webhook scaffold | 4–6h | Low | Remix/Shopify CLI scaffolds this; architect verifies app config |
| Product sync service (full + delta) | 4–6h | Medium | Instruct + verify against `icitem` field mapping |
| Inventory sync service | 3–5h | Medium | Standard delta pattern; verify reconciliation edge cases |
| Image sync service | 2–3h | Low | Straightforward; verify URL handling and idempotency |
| **Order write-back service** | **14–18h** | **🔴 High** | Highest business risk — SP contract, consultant classification, audit trail all verified manually |
| Order cancellation + status services | 8–12h | 🔴 High | Audit-critical; every state transition verified |
| Customer + consultant sync | 6–8h | Medium | Identity and lifecycle mapping verified against `arcust` |
| Exclusive items sync | 4–6h | Medium | `soxitems` → customer metafields; verify access gate behaviour |
| Campaign pricing sync + boundary sweeps | 12–16h | 🔴 High | Oracle correctness and timed transitions verified exhaustively |
| Pricing parity harness | 6–8h | 🔴 High | Test harness must catch AM drift — verification is the bulk of this work |
| Shopify Function — discount (Rust) | 6–8h | Medium | Logic is deterministic but Rust output needs review + Function deploy testing |
| Shopify Function — access gate (Rust) | 4–6h | Medium | Same pattern; shorter |
| Checkout UI + theme extensions | 4–6h | Low | Verify consultant messaging and pricing display |
| PayU reconciliation | 6–8h | Medium | Financial control — verify against PayU data format |
| OTP + SMS + registration validation | 4–6h | Medium | Verify endpoint parity with current `shopapi.annique.com` behaviour |
| Brevo, Meta CAPI, voucher services | 5–7h | Medium | Three separate integrations; verify each independently |
| DBM admin console (Remix + .NET API) | 16–22h | Medium | UI scaffold quick; data APIs and business logic views take time to verify |
| Reconciliation dashboard + support tooling | 4–6h | Medium | Verify data accuracy and alert trigger logic |
| UAT support, defect fixing, hardening | 24–32h | 🔴 High | Client-driven testing; defect resolution is real work regardless of AI tooling |
| Cutover preparation + hypercare planning | 8–12h | High | Checklist, parallel-run plan, rollback triggers — architect-owned |
| **Total** | **~168–225h** | | |

> **Contingency added:** The above reflects optimistic-to-median verification time. A **20% contingency** for rework cycles, unexpected ERP edge cases, and client-driven pivots brings the working budget to:
>
> **Optimistic: ~200h · Pessimistic: ~270h · Midpoint: ~235h**

### 6.3 Calendar timeline

| Scenario | Architect hours | At 6h/day effective | Calendar weeks | Buffer to July deadline |
|---|---|---|---|---|
| Optimistic | 200h | ~33 working days | **~7 weeks** | 13 weeks buffer |
| Midpoint | 235h | ~39 working days | **~8 weeks** | 12 weeks buffer |
| Pessimistic | 270h | ~45 working days | **~9 weeks** | 11 weeks buffer |

> The July deadline is extremely comfortable under this model. The critical constraint shifts from **timeline** to **client-dependency items** (TC-04, TC-05, connectivity verification, UAT responsiveness from Annique stakeholders).

### 6.4 Phase timeline (midpoint scenario — ~8 weeks)

| Phase | Architect hours | Calendar weeks |
|---|---|---|
| Phase 0: Discovery closure and solution definition | 16–20h | Week 1 |
| Phase 1: Middleware platform foundation | 12–16h | Weeks 1–2 |
| Phase 2: Shopify app foundation | 10–14h | Weeks 2–3 (parallel) |
| Phase 3: Core ERP integrations | 40–54h | Weeks 2–5 |
| Phase 4: Consultant, access, and pricing | 36–48h | Weeks 4–6 (parallel with late Phase 3) |
| Phase 5: Supporting integrations + admin console | 32–44h | Weeks 5–7 (parallel with late Phase 4) |
| Phase 6: UAT, hardening, cutover | 32–44h | Weeks 6–8 |

---

## 7. Cost Model

**Delivery rate:** R850/hour (software architect — single resource)

### 7.1 Delivery cost

| Scenario | Architect hours | Total cost (R850/hr) |
|---|---|---|
| Optimistic | 200h | **R170,000** |
| Midpoint (recommended budget) | 235h | **R199,750** |
| Pessimistic | 270h | **R229,500** |

> **Recommended client budget: R200,000 delivery fee** (midpoint, inclusive of 20% contingency). This is the number to communicate. Additional hours beyond 270h would require a change order.

### 7.2 Cost comparison — traditional vs. AI-assisted model

| Model | Effort | Rate basis | Estimated cost |
|---|---|---|---|
| Traditional (1 senior dev) | 76–115 days | ~R900–R1,200/hr market rate | R3.0M – R5.5M |
| Traditional (Option B — 2 resources) | 59 SR + 42 MID days | blended ~R850/hr | R2.5M – R4.0M |
| **AI-assisted architect-led (this model)** | **200–270h architect** | **R850/hr** | **R170,000 – R230,000** |

> The AI-assisted model delivers the same scope at **~10–15% of traditional cost**. This is the commercial case for the delivery model.

### 7.3 Platform and third-party costs (monthly, ongoing)

These are not included in the delivery cost above and must be budgeted separately by Annique/Dieselbrook.

| Item | Estimated monthly cost | Notes |
|---|---|---|
| Shopify Plus | ~USD 2,300/month | Billed annually or monthly |
| Azure App Service (DBM middleware hosting) | USD 200–600/month | Depends on tier and scaling; includes Service Bus |
| Azure SQL (middleware state) | USD 50–150/month | Basic to standard tier |
| Azure Key Vault | USD 5–20/month | Negligible |
| Brevo (ecommerce Brevo instance) | USD 25–300/month | Depends on contact volume and email/SMS usage |
| Seq (structured logging, on-prem option) | Free–USD 150/month | Or Application Insights on Azure consumption model |
| **Total platform (indicative)** | **~USD 2,600–3,500/month** | Dominated by Shopify Plus fee |

---

## 8. Risk Register

### 8.1 Delivery risks

| # | Risk | Severity | Impact | Mitigation |
|---|---|---|---|---|
| R-01 | ERP write-back behaviour more complex than documented | 🔴 High | Orders, cancellations, and statuses are the most commercially sensitive flows. A hidden SP branch or trigger can corrupt order data in AM. | Build against staging first. Document every SP/table path with explicit test cases. Full audit trail on all writes. |
| R-02 | Hidden pricing exceptions beyond current `sp_camp_getSPprice` findings | 🔴 High | Pricing errors are commercially visible immediately — wrong consultant prices or missed campaign transitions cause direct revenue and trust damage. | Parity harness from the start. Expand SQL archaeology before pricing sign-off. Run delta tests against live campaign boundary events during UAT. |
| R-03 | Hosting connectivity to AccountMate is unstable or introduces latency | 🔴 High | DBM depends on reliable, fast SQL access to on-prem AM. If connectivity is flaky or slow, sync services, pricing sweeps, and order write-back all degrade in correlated ways. | Confirm exact private-link or VPN mechanism with Annique IT before build lock. Prove latency and firewall path early in Phase 0. |
| R-04 | Client-dependency items block progress more than architect capacity does | 🟠 Medium | Under the AI-assisted model, the July deadline is not at risk from build velocity. The real blockers are client-dependent: TC-04, TC-05, connectivity verification, and UAT stakeholder availability from Annique. | Escalate all client-dependent open items immediately (see §10). UAT must be scheduled and resourced on Annique's side from Week 6 onward. |
| R-05 | Supporting integration endpoints are weakly owned or undocumented | 🟠 Medium-High | `shopapi.annique.com` source is unconfirmed (TC-04). `SyncCancelOrders.api` and `sp_ws_invoicedtickets` are unconfirmed (TC-05). These could contain business logic DBM must replicate. | Treat each as a separate integration contract. Escalate TC-04 and TC-05 to Annique IT immediately. Do not let these become critical-path blockers in Phase 4–5. |
| R-06 | Cutover introduces drift between Shopify and AccountMate | 🟠 Medium-High | Data state in Shopify and AM will diverge if the cutover window is not tightly controlled. Consultant access, pricing, inventory, and order state are all at risk. | Parallel reconciliation run before cutover. Pre-cutover validation pack. Hypercare plan with defined rollback triggers. |

### 8.2 Scope risks

| # | Risk | Severity | Mitigation |
|---|---|---|---|
| RS-01 | Admin console (D-09) scope expands beyond operational minimum | 🟠 Medium | Fix admin console scope definition early in Phase 0. Create a backlog boundary document separating phase-1 operational minimum from post-launch enhancements. |
| RS-02 | Reporting scope (D-10) expands as Annique reviews the 52 NopReports definitions | 🟠 Medium | Annique must confirm the operationally used report set before Phase 5. Only confirmed-active reports gate phase 1. All others are post-phase-1 backlog. |
| RS-03 | NopCommerce plugin scope (D-07) adds complexity beyond confirmed active features | 🟡 Low-Medium | Awards, Bookings, Gifts, OTP, and Meta CAPI are confirmed mandatory. Events (5 configured) and Chatbot (disabled) are out unless Annique explicitly escalates them. Keep formal scope boundary. |

---

## 9. Delivery Plan — Phase Structure

### Phase 0 · Discovery closure and solution definition (Weeks 1–2)

- Confirm private-link / VPN mechanism with Annique IT (A-08 verification)
- Finalise field mappings for all sync services from DB Structure CSVs and domain packs
- Resolve TC-04 (`shopapi.annique.com` source/owner) and TC-05 (`SyncCancelOrders.api`)
- Establish dev and staging environments
- Obtain Annique IT confirmation of operationally used reports (X-DEP-04)
- DB access verification — confirm SQL login and permissions to all required databases
- Security design sign-off (least-privilege SQL login, Azure Key Vault, secrets model)

### Phase 1 · Middleware platform foundation (Weeks 2–4)

- Solution skeleton: Auth, secrets, configuration, environments
- Logging, health endpoints, monitoring (Serilog + Application Insights)
- Job scheduler and orchestration framework
- Azure Service Bus queue configuration
- Sync-state persistence schema
- CI/CD pipeline (GitHub Actions)

### Phase 2 · Shopify app foundation (Weeks 3–5, parallel with Phase 1)

- Shopify custom app registration and OAuth
- Metafield and metaobject namespace schema registration
- Webhook receiver scaffold
- Embedded admin shell
- Extension scaffold for Functions and Checkout UI

### Phase 3 · Core ERP integrations (Weeks 4–9)

- Product sync service (full + delta)
- Inventory sync service
- Image sync service
- Order write-back service ← highest risk, most testing required
- Order cancellation service
- Order status and fulfillment service

### Phase 4 · Consultant, access, and pricing (Weeks 6–11, parallel with late Phase 3)

- Consultant sync service
- Exclusive items sync service
- Campaign pricing sync service + boundary sweeps
- Pricing parity harness
- Shopify Function — pricing discount
- Shopify Function — exclusive-access gate
- Checkout UI and theme extensions

### Phase 5 · Supporting integrations and admin console (Weeks 8–12, parallel with late Phase 4)

- OTP and SMS services
- Registration validation service
- Brevo sync service (new DBM ecommerce instance)
- Meta CAPI service
- Voucher notification service
- PayU reconciliation service
- DBM admin console (campaign, consultant, reporting, monitoring views)
- Reconciliation dashboard and support tooling

### Phase 6 · UAT, hardening, cutover, and hypercare (Weeks 11–14)

- Full end-to-end UAT with Annique and Dieselbrook stakeholders
- Staging-to-production promotion
- Pricing parity final sign-off
- Cutover checklist execution
- Parallel-run period (if agreed — TC-08 confirmed NopCommerce stays live until handover/signoff)
- Hypercare: real-time monitoring, alert response, escalation paths
- Go-live: end July 2026

---

## 10. Open Items at Document Date

| ID | Item | Priority | Owner |
|---|---|---|---|
| TC-04 | Confirm `shopapi.annique.com` source, stack, and owner | 🔴 Critical | Annique IT |
| TC-05 | Confirm `SyncCancelOrders.api` and `sp_ws_invoicedtickets` ownership and logic | 🔴 High | Annique IT |
| X-DEP-02 | Obtain NISource web shell and missing browser-facing module source | 🟠 High | Annique |
| X-DEP-04 | Confirm which of 52 `NopReports` entries are actively used in production | 🟠 High | Annique + Dieselbrook |
| A-08 verification | Confirm exact private-link / VPN mechanism for DBM → AM SQL connectivity | 🔴 Critical | Annique IT / Dieselbrook |
| D-10 deep dive | Complete report classification and Annique sign-off (see `25_reporting_deep_dive.md`) | 🟠 High | Braven Lab + Annique |

---

## 11. Evidence Base

This document synthesises findings from the full discovery and analysis corpus. Key source documents:

| Document | Purpose |
|---|---|
| `discovery/annique-discovery.1.0.md` | Full DB archaeology and current-state ground truth |
| `discovery/05_delivery_architecture_dieselbrook.md` | Implementation-ready delivery view, tech stack, effort breakdown |
| `discovery/07_hosting_certainty_matrix.md` | Confirmed hosting topology |
| `analysis/01_program_analysis_baseline.md` | Programme anchor point |
| `analysis/03_workstream_decomposition.md` | Workstream structure |
| `analysis/04_assumptions_and_dependencies_register.md` | Assumptions and external dependencies |
| `analysis/08–15_*_domain_pack.md` | All 8 domain packs — boundary and component detail |
| `analysis/16–18_*_crosscut.md` | Platform, SQL, and auditability cross-cuts |
| `analysis/19_phase1_replacement_boundary_summary.md` | Frozen phase 1 boundary |
| `analysis/20_pricing_engine_deep_dive.md` | AM pricing model and Shopify paths |
| `analysis/21_pricing_access_supplement.md` | Access control supplement |
| `analysis/23_shopify_plus_requirements_analysis.md` | Shopify Plus justification and feature analysis |
| `analysis/24_dbm_am_interface_contract.md` | DBM–AM system-of-record boundary contract |
| `analysis/25_reporting_deep_dive.md` | Report classification for D-10 |
| `DB Structure/*.csv` | Raw SQL schema export — primary archaeological evidence |

---

*Document version 1.0 — 2026-03-15 — All programme decisions closed. Ready for internal review.*
