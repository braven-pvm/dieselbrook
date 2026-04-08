# DBM Architecture Specification
## Dieselbrook Middleware — Technical Architecture

**Document type:** Architecture Specification  
**Programme:** Annique → Shopify Plus migration and Dieselbrook Middleware build  
**Version:** 0.1 — initial  
**Date:** 2026-04-07  
**Status:** Draft — in review  
**Linear:** ANN-6  

---

## Decision Dependencies

| Decision ID | Topic | Status | Impact If Still Open |
|---|---|---|---|
| D-01 | Consultant/customer account model | ✅ Closed 2026-03-15 | Was: consultant identity model affects all auth and sync design |
| D-02 | Pricing architecture | ✅ Closed 2026-03-15 | Was: metafield vs. real-time pricing changes Function and sync design |
| D-06 | Shopify plan tier | ✅ Closed 2026-03-15 | Was: Shopify Plus required for private Functions and Container Apps |
| D-09 | Admin console requirement | ✅ Closed 2026-03-15 | Was: admin console is confirmed scope — Node.js/Remix surface confirmed |
| TC-04 | `shopapi.annique.com` ownership and source | 🟠 Open — Annique IT | OTP/SMS service-layer design partially specced; endpoint addresses TBD |
| TC-05 | `SyncCancelOrders.api` and `sp_ws_invoicedtickets` | 🟠 Open — Annique IT | Cancellation service SQL boundary TBD |

**This architecture specification is fully writeable.** TC-04 and TC-05 affect the detailed implementation of two specific services but do not change the platform architecture.

---

## 1. Architecture Principles

These principles govern all design decisions in DBM. They are the ground truth from which implementation decisions are derived.

### 1.1 Core principle

> **Event-driven on the Shopify side. Poll-and-reconcile on the AccountMate side.**

Shopify pushes events by webhook. AccountMate is a SQL-centric on-premises ERP with no outbound event capability. DBM polls AM on schedule, sweeps for time-boundary transitions (e.g. campaign `dFrom`/`dTo`), and runs nightly reconciliation to detect and correct drift.

### 1.2 System-of-record hierarchy

> **AccountMate owns all business data. DBM reads from AM and writes back to AM. Shopify displays and transacts. AM owns.**

Corollaries:
- DBM never becomes the authority for consultant number assignment, lifecycle state, commission computation, or ERP financial records.
- DBM is the authority for the Shopify-facing projection of AM state (consultant profiles, pricing metafields, exclusive-item access keys).
- DBM's own state (idempotency ledger, sync watermarks, audit log) is stored in a separate DBM-owned database. DBM does not write to AM's tables for middleware housekeeping.

### 1.3 Greenfield constraint

> **DBM is a clean greenfield implementation. It inherits no code from NISource, NopCommerce, AnniqueAPI, or any existing Annique middleware.**

Stack choices are made purely on fitness for the work, not for compatibility with the legacy estate.

### 1.4 Idempotency is not optional

Every integration action must name its idempotency key, define its retry behaviour, and expose its failure state to an operator before it can be considered designed. No domain spec may be considered complete without these.

---

## 2. System Topology

### 2.1 Physical topology

```
┌─────────────────────────────────────────────────────────────────────┐
│                         SHOPIFY PLUS                                │
│  Storefront · Checkout · Orders · Customers · Products              │
│  Metafields · Webhooks · Admin GraphQL API · Shopify Flow           │
│  Shopify Functions (Rust) · Checkout UI Extensions · Theme Ext.     │
└──────────────────────────────┬──────────────────────────────────────┘
                               │ HTTPS
                               │ Webhooks (orders, customers, fulfillment, cancel)
                               │ Admin GraphQL API calls
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│               AZURE  ·  Dieselbrook Middleware (DBM)                │
│                                                                     │
│  Azure App Service — DBM Core (.NET 9)                              │
│  ├─ API Layer (ASP.NET Core Web API)                                │
│  │   ├─ Webhook receivers (orders, customers, fulfillments)         │
│  │   ├─ Shopify OAuth app backend                                   │
│  │   ├─ DBM Data API (feeds admin console)                          │
│  │   └─ Legacy-compatible endpoints (TC-04, TC-05 scope TBD)        │
│  │                                                                  │
│  └─ Worker Services (IHostedService)                                │
│      ├─ Sync workers: Products, Inventory, Images                   │
│      ├─ Sync workers: Consultants, ExclusiveItems                   │
│      ├─ Sync workers: CampaignPricing, PricingParity                │
│      ├─ Write-back workers: OrderWriteback, OrderCancellation        │
│      ├─ Status workers: OrderStatusFulfillment                      │
│      ├─ Comms workers: Brevo, SMS, OTP, MetaCAPI, Vouchers          │
│      ├─ Platform workers: AuditLog, Reconciliation, JobOrchestration│
│      └─ Job scheduler: Quartz.NET                                   │
│                                                                     │
│  Azure App Service — DBM Admin Console (Node.js / Remix)            │
│  ├─ Campaign management UI                                          │
│  ├─ Consultant operations UI                                        │
│  ├─ Reporting dashboard                                             │
│  ├─ Sync status and diagnostics                                     │
│  └─ System configuration and monitoring                             │
│                                                                     │
│  Azure Service Bus                                                  │
│  ├─ order-ingest queue (webhook → write-back decoupling)            │
│  ├─ fulfillment-sync queue                                          │
│  └─ dead-letter queues (per integration class)                      │
│                                                                     │
│  Azure SQL Database — DBM State                                     │
│  └─ Separate from AM; DBM-owned schema only                         │
│      idempotency · outbox · watermarks · audit · dead-letter        │
│      reconciliation · consultant projections · settings             │
│                                                                     │
│  Azure Key Vault                                                    │
│  └─ All credentials, tokens, connection strings                     │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
                               │ Direct SQL — private connectivity
                               │ (existing Azure → on-prem path;
                               │  Dieselbrook joins same routing)
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│             ACCOUNTMATE SQL ESTATE  ·  On-premises (AMSERVER-v9)   │
│                                                                     │
│  amanniquelive   — orders, customers, products, inventory           │
│  compplanLive    — MLM hierarchy, downlines, commission              │
│  compsys         — AM internal comms (not touched by DBM)           │
│  amanniquenam    — Namibia (out of scope)                           │
│                                                                     │
│  Stored procedures called by DBM:                                   │
│  sp_camp_getSPprice · sp_ws_reactivate · sp_ws_gensoxitems          │
│  sp_ws_syncorder (+ related order write-back SPs)                   │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Network connectivity

| Connection | Mechanism | Notes |
|---|---|---|
| Shopify → DBM | HTTPS (public) | Webhook HMAC verified by DBM receiver |
| DBM → Shopify Admin API | HTTPS (public) | OAuth token stored in Key Vault |
| DBM → AMSERVER-v9 | Direct SQL / private routing | Join existing Azure → on-prem path; exact VPN/private-link config to confirm with Annique IT |
| DBM Admin Console → DBM API | HTTPS (Azure-internal or public) | JWT-authenticated |
| DBM → Azure Service Bus | Azure SDK (AMQP) | Managed Identity preferred |
| DBM → Azure Key Vault | Azure SDK | Managed Identity — no credentials in source |
| DBM → Brevo | HTTPS | API key from Key Vault |
| DBM → SMS provider | HTTPS | TC-03 resolved — provider TBD |

---

## 3. Technology Stack

> DBM is a clean greenfield build. Every stack choice was made for fitness for the specific workload. The AccountMate integration workload is SQL Server–heavy, SP-centric, and requires reliable background polling — .NET 9 is the correct choice.

### 3.1 Full stack table

| Layer | Technology | Rationale |
|---|---|---|
| Middleware runtime | **.NET 9 / ASP.NET Core** | Dominant choice for SQL Server–heavy ERP integration. `IHostedService` + Worker Services is the natural model for 25 sync services. Azure SDK, Service Bus client, Dapper, Serilog all first-class. No serious alternative matches it for this workload. |
| Data access | **Dapper** | Precise, lightweight control over SP calls and parameterised queries. Avoids ORM abstraction overhead that conflicts with AM's SP-centric design. No migrations — DBM never owns the AM schema. EF Core is explicitly ruled out for the AM access layer. |
| Background scheduling | **Quartz.NET** | Reliable recurring job scheduling with execution history, cron expressions, retry orchestration, and clustered execution. Core engine for all polling sync services. Hangfire is the fallback if Quartz.NET proves operationally harder to operate without a dashboard licence. |
| Async queue / dead-letter | **Azure Service Bus** | Durable async processing for webhook-triggered flows and dead-letter queues. Decouples webhook receiver from write-back processing. Standard tier minimum; Premium if VNet integration is required for private connectivity to Service Bus. |
| DBM state persistence | **Azure SQL / SQL Server** | DBM's own state only — idempotency keys, outbox, sync watermarks, job history, audit records, dead-letter items, reconciliation results, consultant projections. Physically separate from AccountMate. |
| Structured logging | **Serilog** | Structured, sink-composable logging. Azure Application Insights sink provides live metrics, distributed tracing, and alerting at low operational overhead. |
| APM / observability | **Azure Application Insights** | Log, metric, and trace aggregation. Native Azure SDK integration. Auto-correlation of HTTP and queue-driven traces. |
| Shopify Functions | **Rust** | Only viable language for Shopify Functions in a private/custom app. Deterministic, sandboxed, fast. Two Functions required: pricing discount delta and exclusive-item access gate. |
| Checkout UI Extensions | **React + Shopify UI Extensions SDK** | Shopify-mandated model for Checkout Extensibility. Consultant-specific pricing display and exclusive-access messaging at checkout. |
| Theme extensions | **Liquid + minimal JS** | Pre-checkout visibility logic — price preview, access indicator. Low surface area. |
| Admin console UI | **Node.js / Remix + Shopify Polaris** | Shopify CLI scaffolds Remix for embedded admin apps. Polaris is the Shopify-native component library. The Node.js Remix shell renders UI only; all data comes from the .NET 9 DBM Data API. |
| Shopify app shell | **Node.js / Remix** | Shopify app OAuth, session token management, embedded app frame. Shopify CLI's native scaffolded output. |
| CI/CD | **GitHub Actions** | Deployment automation for .NET middleware (dotnet publish → Azure App Service) and Node.js app (npm build → Azure App Service). |
| Secrets management | **Azure Key Vault** | All connection strings, API tokens, and credentials. No credentials in source code. Managed Identity for DBM process identity → Key Vault access. |
| Hosting | **Azure App Service** | Consistent with existing Annique Azure topology. Private connectivity to on-prem AM SQL estate via confirmed routing path. Container Apps is the upgrade path if horizontal scaling is needed. |

### 3.2 Stack boundary summary

```
Shopify platform layer    → Rust (Functions) + React (Checkout UI Ext.) + Liquid (theme)
Shopify admin app         → Node.js / Remix / Polaris  (UI shell — renders only)
DBM middleware core       → .NET 9 / ASP.NET Core / Dapper / Worker Services
DBM Web API               → .NET 9 ASP.NET Core Web API (feeds admin console + external consumers)
DBM background workers    → .NET 9 IHostedService + Quartz.NET scheduler
Async decoupling layer    → Azure Service Bus (webhooks → write-back, dead-letter)
DBM state layer           → Azure SQL (DBM-owned schema only)
AccountMate access        → Dapper → direct SQL SP calls (read + controlled write)
Secrets                   → Azure Key Vault (all credentials via Managed Identity)
```

---

## 4. Service Decomposition

All 25 DBM services are grouped below by layer and workstream.

### 4.1 API layer (ASP.NET Core Web API)

These services expose HTTP endpoints — either to Shopify or to the admin console.

| # | Service | Type | Consumes | Produces |
|---|---|---|---|---|
| 1 | `ShopifyAppBackend` | API | Shopify OAuth callbacks, session tokens | App installation state, OAuth token (→ Key Vault) |
| 2 | `WebhookReceiver` | API | Shopify webhooks (HMAC-verified HTTPS POST) | Messages on Azure Service Bus order-ingest queue |

### 4.2 Write-back workers (WS-01 — Orders)

| # | Service | Schedule / Trigger | AM interaction |
|---|---|---|---|
| 3 | `OrderWritebackService` | ASB queue consumer (order-ingest) | Calls `sp_ws_syncorder` + related order SPs |
| 4 | `OrderCancellationService` | ASB queue consumer + poll | TC-05 pending — SP boundary TBD |
| 5 | `OrderStatusFulfillmentService` | Poll `SOPortal` on schedule | AM `SOPortal` (` ` → `P` → `S`) → Shopify fulfillment |
| 16 | `PayUReconciliationService` | Scheduled | PayU gateway reconciliation against AM orders |

### 4.3 Consultant and identity workers (WS-02)

| # | Service | Schedule / Trigger | AM interaction |
|---|---|---|---|
| 9 | `CustomerSyncService` | Scheduled + lifecycle event | `arcust` read → Shopify customer metafields |
| 10 | `ConsultantSyncService` | Scheduled delta poll | `arcust` read → DBM consultant projection → Shopify |
| 11 | `ExclusiveItemsSyncService` | Scheduled | `soxitems` read → Shopify customer metafields |
| 14 | `ConsultantHierarchyAndMLMAccessService` | Scheduled read-only | `compplanLive` read → hierarchy cache in DBM state |
| 19 | `RegistrationValidationService` | API + worker | New consultant intake → AM write-back (`arcust`) → consultant number return |

### 4.4 Product and inventory workers (WS-03)

| # | Service | Schedule / Trigger | AM interaction |
|---|---|---|---|
| 6 | `ProductSyncService` | Scheduled (full + delta) | `icitem` read → Shopify products/variants |
| 7 | `InventorySyncService` | Scheduled | `icitem`/warehouse read → Shopify inventory levels |
| 8 | `ImageSyncService` | Scheduled | `iciimgUpdateNOP` → Shopify product images |

### 4.5 Pricing and campaign workers (WS-04)

| # | Service | Schedule / Trigger | AM interaction |
|---|---|---|---|
| 12 | `CampaignPriceSyncService` | Scheduled poll + boundary sweep (every 5–15 min) | `sp_camp_getSPprice` → precomputed prices → Shopify metafields |
| 13 | `PricingParityService` | Nightly reconciliation | DBM precomputed price vs. `sp_camp_getSPprice` oracle — parity harness |

### 4.6 Communications workers (WS-05)

| # | Service | Schedule / Trigger | AM interaction |
|---|---|---|---|
| 17 | `SmsService` | API (replaces `sendsms.api`) | None — SMS provider only |
| 18 | `OtpService` | API (replaces `otpGenerate.api`) | TC-04 scope TBD — read AM for validation if required |
| 20 | `BrevoSyncService` | Event-driven + scheduled | None — Brevo API only |
| 21 | `MetaCapiService` | Event-driven | None — Meta CAPI only |
| 22 | `VoucherNotificationService` | Event-driven (replaces `NotifyVouchers.api`) | AM voucher state read |

### 4.7 Reporting worker (WS-06)

| # | Service | Schedule / Trigger | AM interaction |
|---|---|---|---|
| 15 | `ConsultantReportingDataService` | Scheduled + on-demand | `amanniquelive` + `compplanLive` reads for consultant self-service reports |

### 4.8 Platform services (X-01, X-03)

| # | Service | Purpose |
|---|---|---|
| 23 | `AuditLogService` | Structured audit records for all ERP write-back and state-change events |
| 24 | `JobOrchestrationService` | Quartz.NET wrapper — schedules, monitors, and retries all polling workers |
| 25 | `ReconciliationDashboardService` | Aggregates parity check outcomes and exposes them to the admin console |

### 4.9 Custom Shopify app components

| Component | Runtime | Purpose |
|---|---|---|
| Shopify Function — pricing | Rust | Reads effective consultant price from product metafields; applies deterministic discount delta at checkout |
| Shopify Function — exclusive access | Rust | Validates consultant exclusive-item eligibility at checkout using `soxitems`-synced customer metafields |
| Checkout UI Extensions | React / UI Extensions SDK | Consultant-specific pricing display and exclusive-access messaging at checkout |
| Theme app extensions | Liquid + minimal JS | Pre-checkout price preview and access indicator |
| Metafield / metaobject schema | — | Registers DBM-owned product (`dbm.consultant_price`, `dbm.campaign_id`) and customer (`dbm.is_consultant`, `dbm.exclusive_items`) metafield namespaces |
| Embedded admin pages | Node.js / Remix | Sync status, diagnostic views, and override tooling within Shopify Admin |

---

## 5. DBM State Database Schema

DBM owns a dedicated Azure SQL database (`dbm_state`). It does not write to AccountMate for middleware housekeeping. All tables listed here are in the `dbo` schema unless a domain-specific schema is appropriate.

### 5.1 Idempotency ledger

Prevents duplicate processing of the same source event.

```sql
CREATE TABLE idempotency_keys (
    id              BIGINT IDENTITY PRIMARY KEY,
    idempotency_key NVARCHAR(256)  NOT NULL,  -- stable business key (e.g. shopify_order_id)
    action_type     NVARCHAR(100)  NOT NULL,  -- 'order_writeback', 'consultant_sync', etc.
    status          NVARCHAR(50)   NOT NULL,  -- 'processing' | 'complete' | 'failed'
    first_seen_at   DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    completed_at    DATETIME2      NULL,
    correlation_id  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    result_summary  NVARCHAR(MAX)  NULL,      -- JSON — outcome detail
    CONSTRAINT uq_idempotency_key_action UNIQUE (idempotency_key, action_type)
);
```

### 5.2 Transactional outbox

Guarantees at-least-once delivery for Shopify API writes initiated from within a database transaction.

```sql
CREATE TABLE outbox_events (
    id              BIGINT IDENTITY PRIMARY KEY,
    event_type      NVARCHAR(100)  NOT NULL,  -- 'shopify.product.update', 'shopify.customer.update', etc.
    payload         NVARCHAR(MAX)  NOT NULL,  -- JSON payload to deliver
    status          NVARCHAR(50)   NOT NULL DEFAULT 'pending',  -- 'pending' | 'delivered' | 'failed'
    retry_count     INT            NOT NULL DEFAULT 0,
    created_at      DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    next_attempt_at DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    delivered_at    DATETIME2      NULL,
    last_error      NVARCHAR(MAX)  NULL,
    correlation_id  UNIQUEIDENTIFIER NOT NULL
);
CREATE INDEX ix_outbox_pending ON outbox_events (status, next_attempt_at) WHERE status IN ('pending', 'failed');
```

### 5.3 Sync watermarks

Tracks the last-synced position per service to enable efficient delta sync.

```sql
CREATE TABLE sync_watermarks (
    service_name    NVARCHAR(100)  NOT NULL PRIMARY KEY,
    watermark_value NVARCHAR(256)  NULL,  -- timestamp or cursor (service-specific)
    last_run_at     DATETIME2      NULL,
    last_run_status NVARCHAR(50)   NULL,  -- 'success' | 'partial' | 'failed'
    records_synced  INT            NULL,
    notes           NVARCHAR(MAX)  NULL
);
```

### 5.4 Job execution history

Quartz.NET execution records; also written by the `JobOrchestrationService`.

```sql
CREATE TABLE job_executions (
    id              BIGINT IDENTITY PRIMARY KEY,
    job_name        NVARCHAR(200)  NOT NULL,
    job_group       NVARCHAR(200)  NOT NULL,
    fired_at        DATETIME2      NOT NULL,
    completed_at    DATETIME2      NULL,
    duration_ms     INT            NULL,
    status          NVARCHAR(50)   NOT NULL,  -- 'running' | 'success' | 'failed' | 'vetoed'
    error_message   NVARCHAR(MAX)  NULL,
    records_affected INT           NULL
);
CREATE INDEX ix_job_exec_name_fired ON job_executions (job_name, fired_at DESC);
```

### 5.5 Audit log

Structured audit record for every ERP write-back, consultant lifecycle change, and pricing sync event.

```sql
CREATE TABLE audit_entries (
    id              BIGINT IDENTITY PRIMARY KEY,
    event_time      DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    domain          NVARCHAR(100)  NOT NULL,  -- 'orders', 'consultants', 'pricing', etc.
    action          NVARCHAR(200)  NOT NULL,  -- 'order_writeback', 'consultant_deactivation', etc.
    actor           NVARCHAR(200)  NULL,      -- service name or admin user
    entity_type     NVARCHAR(100)  NULL,      -- 'order', 'consultant', 'product', etc.
    entity_id       NVARCHAR(256)  NULL,      -- e.g. Shopify order GID or AM order no.
    correlation_id  UNIQUEIDENTIFIER NOT NULL,
    before_state    NVARCHAR(MAX)  NULL,      -- JSON snapshot
    after_state     NVARCHAR(MAX)  NULL,      -- JSON snapshot
    outcome         NVARCHAR(50)   NOT NULL,  -- 'success' | 'failure' | 'skipped'
    detail          NVARCHAR(MAX)  NULL
);
CREATE INDEX ix_audit_domain_time ON audit_entries (domain, event_time DESC);
CREATE INDEX ix_audit_entity ON audit_entries (entity_type, entity_id);
```

### 5.6 Dead-letter items

Captures failed operations that have exhausted automatic retry for operator review and replay.

```sql
CREATE TABLE dead_letter_items (
    id              BIGINT IDENTITY PRIMARY KEY,
    integration_class NVARCHAR(100) NOT NULL,  -- 'order_writeback', 'product_sync', etc.
    idempotency_key NVARCHAR(256)  NOT NULL,
    payload         NVARCHAR(MAX)  NOT NULL,   -- original event payload
    failure_reason  NVARCHAR(MAX)  NOT NULL,
    retry_count     INT            NOT NULL,
    first_failed_at DATETIME2      NOT NULL,
    last_failed_at  DATETIME2      NOT NULL,
    status          NVARCHAR(50)   NOT NULL DEFAULT 'open',  -- 'open' | 'replayed' | 'dismissed'
    resolved_at     DATETIME2      NULL,
    resolved_by     NVARCHAR(200)  NULL,
    correlation_id  UNIQUEIDENTIFIER NOT NULL
);
CREATE INDEX ix_dead_letter_open ON dead_letter_items (status, last_failed_at DESC) WHERE status = 'open';
```

### 5.7 Reconciliation results

Stores parity check outcomes per run for operator dashboards.

```sql
CREATE TABLE reconciliation_runs (
    id              BIGINT IDENTITY PRIMARY KEY,
    run_at          DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    domain          NVARCHAR(100)  NOT NULL,  -- 'pricing', 'inventory', 'orders', etc.
    records_checked INT            NOT NULL,
    records_matched INT            NOT NULL,
    records_diverged INT           NOT NULL,
    divergence_details NVARCHAR(MAX) NULL,    -- JSON array of divergent items
    status          NVARCHAR(50)   NOT NULL   -- 'clean' | 'drift_detected' | 'error'
);
```

### 5.8 Consultant projections

DBM's projection of AM consultant state — the canonical DBM-side view of each consultant's Shopify-facing profile.

```sql
CREATE TABLE consultant_projections (
    dbm_consultant_id     BIGINT IDENTITY PRIMARY KEY,
    am_cust_no            NVARCHAR(20)   NOT NULL UNIQUE,  -- arcust.cCustNo
    shopify_customer_id   NVARCHAR(30)   NULL,
    is_active             BIT            NOT NULL DEFAULT 0,
    consultant_type       NVARCHAR(50)   NULL,
    exclusive_item_keys   NVARCHAR(MAX)  NULL,   -- JSON array from soxitems
    pricing_tier          NVARCHAR(50)   NULL,
    last_am_sync_at       DATETIME2      NULL,
    last_shopify_sync_at  DATETIME2      NULL,
    sync_hash             NVARCHAR(64)   NULL,   -- SHA-256 of last projected state for change detection
    created_at            DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at            DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);
```

### 5.9 Settings

Runtime configuration key-value store for non-secret settings (secret settings are in Key Vault).

```sql
CREATE TABLE settings (
    setting_key     NVARCHAR(200)  NOT NULL PRIMARY KEY,
    setting_value   NVARCHAR(MAX)  NOT NULL,
    description     NVARCHAR(500)  NULL,
    last_updated_at DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    last_updated_by NVARCHAR(200)  NULL
);
```

---

## 6. Security Architecture

### 6.1 Principles

- No credentials in source code. No `sa` account. No hardcoded connection strings. The NISource pattern of `sa`/`AnniQu3S@` in source is a confirmed anti-pattern and must not be reproduced.
- Azure Managed Identity is the primary identity for DBM's Azure service-to-service authentication (Key Vault, Service Bus, Application Insights).
- All secrets are stored in Azure Key Vault and injected via the Azure SDK at runtime.

### 6.2 Credential inventory

| Credential | Storage | Access method |
|---|---|---|
| AM SQL connection string (`amanniquelive`) | Azure Key Vault | Key Vault SDK via Managed Identity |
| AM SQL connection string (`compplanLive`) | Azure Key Vault | Key Vault SDK via Managed Identity |
| Shopify Admin API access token | Azure Key Vault | Key Vault SDK via Managed Identity |
| Shopify webhook signing secret | Azure Key Vault | Key Vault SDK via Managed Identity |
| Azure Service Bus connection string | Azure Key Vault (or Managed Identity) | Managed Identity preferred |
| DBM state DB connection string | Azure Key Vault | Key Vault SDK via Managed Identity |
| Brevo API key | Azure Key Vault | Key Vault SDK via Managed Identity |
| SMS provider API key | Azure Key Vault | Key Vault SDK via Managed Identity |
| Meta CAPI token | Azure Key Vault | Key Vault SDK via Managed Identity |

### 6.3 AccountMate SQL access

| Requirement | Implementation |
|---|---|
| No `sa` account | Dedicated least-privilege service account: `dbm_svc` |
| Scope | `amanniquelive` + `compplanLive` (read-heavy) + controlled write via named SPs only |
| Grant model | EXECUTE permission on named stored procedures; SELECT on required views/tables; no INSERT/UPDATE/DELETE on raw tables except via SP |
| Write boundary | `sp_ws_syncorder` (and related order SPs), `sp_ws_reactivate`, `sp_ws_gensoxitems`, consultant number registration call |
| Forbidden | Direct writes to `arcash`, `SOPortal` (unless confirmed), `compsys`, any `compplanLive` write |
| Verification required | TC-05 cancellation SP boundary — not yet confirmed |

### 6.4 DBM Web API authentication

The DBM Data API (feeding the admin console and any external consumers) uses JWT bearer tokens.

| Consumer | Auth mechanism |
|---|---|
| Admin console (Node.js / Remix) | Shopify session token (embedded app) → exchanged for DBM JWT |
| External integrations (if any) | Client credentials flow → DBM JWT |
| Health endpoints | No auth (public `/health`) or IP-restricted (`/health/detail`) |

### 6.5 Webhook security

All inbound Shopify webhooks are HMAC-verified using the Shopify webhook signing secret before any processing. Requests that fail HMAC verification return `401` immediately with no payload processing.

### 6.6 Shopify Function scoping

Shopify Functions run in a sandboxed WebAssembly environment — they do not have network access. They read only from the product/customer metafields already synced by DBM. No AM credentials are accessible from a Function context.

---

## 7. Deployment Specification

### 7.1 Environments

| Environment | Purpose | AM database |
|---|---|---|
| `development` | Local developer environment + unit tests | Mock / local SQL Server |
| `staging` | Integration and UAT; connected to live AM staging | `AMSERVER-v9` at `196.3.178.122:62111` |
| `production` | Live environment post-cutover | Production AMSERVER (to confirm with Annique IT) |

Staging is connected to the confirmed live staging SQL Server (`AMSERVER-v9`). This is the correct parity-testing environment. Development does not connect to AM directly.

### 7.2 Azure resource layout

| Resource | SKU / tier | Notes |
|---|---|---|
| Azure App Service — DBM Core | P2v3 or equivalent | Hosts .NET 9 API + workers in a single App Service; can be split if scale requires |
| Azure App Service — Admin Console | P1v3 or equivalent | Node.js / Remix embedded Shopify app |
| Azure SQL Database — DBM State | S3 or equivalent | DBM-owned schema; no AM data; sized for ~10K orders/month at steady state |
| Azure Service Bus | Standard tier | Queues + dead-letter; upgrade to Premium if VNet isolation required |
| Azure Key Vault | Standard tier | All credentials |
| Azure Application Insights | Pay-as-you-go | Logs, metrics, traces |
| Private connectivity to AMSERVER-v9 | Existing path (TBC with Annique IT) | Joins the Azure → on-prem routing used by current AnniqueAPI |

> **Note on hosting:** Azure App Service is the baseline. Azure Container Apps is the natural upgrade path if horizontal scaling, zero-downtime deploys, or sidecar patterns are needed. The architecture does not assume containers — but is designed to containerise cleanly (no filesystem state, secrets via SDK, 12-factor compliant).

### 7.3 Deployment pipeline

```
GitHub (main branch)
  │
  ├─ dotnet test → dotnet publish → Azure App Service (DBM Core)
  │     [GitHub Actions workflow: deploy-dbm-core.yml]
  │
  ├─ npm test → npm run build → Azure App Service (Admin Console)
  │     [GitHub Actions workflow: deploy-admin-console.yml]
  │
  └─ cargo build → shopify app deploy (Rust Functions + React extensions)
        [GitHub Actions workflow: deploy-shopify-app.yml]
```

### 7.4 Configuration management

| Config class | Mechanism |
|---|---|
| Secrets | Azure Key Vault references in App Service application settings |
| Non-secret runtime config | App Service application settings (environment variables) |
| Feature flags / operational settings | `settings` table in DBM State DB (live-editable via admin console) |
| Scheduled job definitions | Quartz.NET persisted scheduler (uses DBM State DB) |

### 7.5 Environment promotion

- Code changes merge to `main` and deploy automatically to `staging`.
- Production deploys are manual approval gates in GitHub Actions.
- AM staging is the integration gate — no production deploy without passing staging integration tests against `AMSERVER-v9`.

---

## 8. Observability

### 8.1 Structured logging (Serilog)

All DBM services log via Serilog. Every log event carries:

| Property | Value |
|---|---|
| `CorrelationId` | GUID — spans from webhook receipt through write-back to AM |
| `ServiceName` | e.g. `OrderWritebackService` |
| `Domain` | e.g. `orders`, `consultants`, `pricing` |
| `IdempotencyKey` | business key for the event being processed |
| `Outcome` | `success` / `failure` / `retry` / `dead-lettered` |

Serilog sinks:
- **Console** (dev/staging) — structured JSON
- **Azure Application Insights** (all environments) — primary operational sink
- **File sink** (staging only) — rolling daily log for local debugging

### 8.2 Health endpoints

| Endpoint | Auth | Behaviour |
|---|---|---|
| `GET /health` | None (public) | Returns `200 OK` if process is alive — used by Azure App Service probe |
| `GET /health/ready` | IP-restricted | Returns liveness + readiness: DB connectivity, ASB connectivity, Key Vault reachability |
| `GET /health/detail` | JWT bearer | Returns per-service health: last job run, sync watermark age, dead-letter counts |

### 8.3 Alerting

Configured in Azure Application Insights / Azure Monitor:

| Alert | Condition | Severity |
|---|---|---|
| Order write-back failure | Any order dead-lettered | High |
| Pricing sweep stale | `CampaignPriceSyncService` watermark age > 20 min | Medium |
| High dead-letter count | Dead-letter queue depth > 10 items (any class) | Medium |
| Sync worker down | `JobOrchestrationService` has not fired a scheduled job in > 30 min | High |
| SQL connectivity loss | Health check reports AM SQL unreachable | Critical |

### 8.4 Distributed tracing

Application Insights provides distributed tracing across the ASP.NET Core webhook receiver → Azure Service Bus message → Worker Service write-back chain. `CorrelationId` is propagated as a Service Bus message property to ensure trace continuity across the queue boundary.

---

## 9. Integration Patterns

### 9.1 Webhook ingestion (Shopify → DBM → AM)

```
Shopify webhooks
  → WebhookReceiver (HMAC verify → idempotency check → enqueue to ASB)
  → Azure Service Bus (order-ingest queue)
  → OrderWritebackService (dequeue → idempotency check → call AM SP → audit log)
  → AccountMate (sp_ws_syncorder)
  → outbox_events (update Shopify fulfillment/status if needed)
```

**Idempotency:** `shopify_order_id` is checked against `idempotency_keys` at both the receiver and the worker. Duplicate events produce `409` at receiver and are skipped at worker.

**Dead-letter:** After 3 failed SP call attempts (exponential backoff), the message moves to the ASB dead-letter queue and a `dead_letter_items` record is created. The admin console exposes the item for operator review and replay.

### 9.2 Delta sync polling (AM → DBM → Shopify)

```
Quartz.NET fires scheduled job (e.g. ProductSyncService)
  → Read sync_watermarks for last run position
  → Query AM with delta filter (e.g. modified_at > watermark)
  → Transform records
  → Write to outbox_events
  → Outbox relay → Shopify Admin API calls
  → Update sync_watermarks on success
  → Record job_executions entry
```

### 9.3 Pricing boundary sweep

```
CampaignPriceSyncService fires every 5–15 min (configurable in settings table)
  → Query sp_camp_getSPprice for all active SKUs
  → Compare against last precomputed price in DBM state
  → For changed prices: update Shopify product metafield via outbox
  → Nightly: PricingParityService runs full oracle comparison and writes reconciliation_runs record
```

### 9.4 Transactional outbox relay

A dedicated outbox relay worker reads `outbox_events` with `status = 'pending'` and delivers them to the Shopify Admin API. This ensures that even if a Shopify API call fails mid-transaction, the event is not lost and will be retried on the next relay cycle. The relay uses the `idempotency_key` header on the Shopify API call to prevent duplicate mutations.

---

## Evidence Base

| Artefact | Location |
|---|---|
| Solution overview, scope and cost | `docs/analysis/26_solution_overview_scope_cost.md` |
| Platform runtime and hosting cross-cut | `docs/analysis/16_platform_runtime_and_hosting_crosscut.md` |
| Data access and SQL contract cross-cut | `docs/analysis/17_data_access_and_sql_contract_crosscut.md` |
| Auditability, idempotency, reconciliation cross-cut | `docs/analysis/18_auditability_idempotency_and_reconciliation_crosscut.md` |
| DBM–AM interface contract | `docs/analysis/24_dbm_am_interface_contract.md` |
| Phase 1 replacement boundary | `docs/analysis/19_phase1_replacement_boundary_summary.md` |
| Pricing engine deep dive | `docs/analysis/20_pricing_engine_deep_dive.md` |
| Shopify Plus requirements analysis | `docs/analysis/23_shopify_plus_requirements_analysis.md` |
| Open decisions register | `docs/analysis/02_open_decisions_register.md` |
| PRD | `docs/spec/00_prd.md` |
