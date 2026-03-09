# Dieselbrook Delivery Architecture
## Shopify Plus Middleware and Integration Implementation Brief

**Date:** March 9, 2026
**Prepared for:** Dieselbrook
**Prepared by:** Braven Lab
**Scope:** Discovery, architecture, middleware development, and supporting integration solutions for the Annique Shopify migration
**Assumption:** Shopify Plus is approved as the target commerce platform

---

## 1. Purpose

This document converts the discovery and research work into an implementation-ready delivery view for Dieselbrook.

It answers five practical questions:

1. What exactly needs to be built?
2. What is the target architecture?
3. What technology stack should be used?
4. What will the effort, impact, and risk look like?
5. What should be the main focus areas during delivery?

It also includes a less technical summary that can be used in stakeholder discussions.

---

## 2. Executive Summary

### Core conclusion

Even with Shopify Plus, the project is still primarily an **integration and middleware programme**, not a theme or storefront project.

The success of the migration depends on building a reliable middleware layer between Shopify and AccountMate that can:

- receive Shopify events in real time
- push orders and customer changes into AccountMate safely
- pull product, inventory, image, consultant, campaign, and pricing changes out of AccountMate on a schedule
- enforce consultant pricing and exclusive-item access in Shopify using synced data
- preserve operational dependencies such as PayU reconciliation, SMS, OTP, registration validation, Meta CAPI, Brevo, and voucher notifications

### What Shopify Plus changes

Shopify Plus is helpful because it removes plan-level friction around custom app deployment and checkout extensibility. It does **not** remove the need for custom integration logic.

The main benefits of the Plus assumption are:

- cleaner custom app deployment model
- fewer commercial constraints around checkout extensibility
- easier long-term governance for custom Functions and admin tooling
- optional use of B2B/Catalog tooling where it genuinely fits

### Delivery reality

The main build is:

- one production middleware platform
- one custom Shopify app
- a set of scheduled sync services
- a set of webhook handlers
- pricing and access-control logic backed by synced Shopify data
- operational tooling, logging, reconciliation, and support visibility

This is a serious integration programme with ERP risk, not a light storefront implementation.

---

## 3. Delivery Scope

### Included in delivery scope

- solution architecture and delivery design
- middleware platform design and implementation
- Shopify custom app design and implementation
- Shopify webhook ingestion layer
- AccountMate to Shopify sync services
- Shopify to AccountMate order and customer write-back flows
- consultant pricing synchronization and enforcement
- exclusive-item access synchronization and enforcement
- PayU, SMS, OTP, registration validation, voucher notification, and Brevo integration replacement
- operational logging, retry, health checks, alerting, and reconciliation tooling
- staging, UAT, cutover support, and hypercare planning

### Not automatically replaced by Shopify itself

- AccountMate pricing logic
- AccountMate consultant model
- `soxitems` exclusive access behaviour
- PayU reconciliation logic
- SQL-driven sync jobs
- custom Nop-era side integrations
- Meta CAPI queue behaviour
- Brevo attribute and lost-cart processes

### Important framing

Shopify Plus gives Dieselbrook a better platform surface, but the critical business logic still lives in the integrations and ERP-adjacent services.

---

## 4. Exact Target Architecture

### 4.1 Architecture principle

The target architecture should be **event-driven on the Shopify side, poll-and-reconcile on the AccountMate side**.

Why:

- Shopify can push events reliably by webhook
- AccountMate currently depends on SQL tables, triggers, stored procedures, and scheduled jobs rather than outbound events
- current business behaviour is already based on periodic synchronization and ERP-side source-of-truth decisions

### 4.2 Logical architecture

```text
┌──────────────────────────────────────────────────────────────┐
│                        SHOPIFY PLUS                         │
│                                                              │
│  Storefront / Theme / Checkout                              │
│  Customers / Products / Inventory / Orders                  │
│  Metafields / Metaobjects / B2B features                    │
│  Shopify Functions / Checkout UI Extensions                 │
│  Webhooks / Admin GraphQL API                               │
└─────────────────────────────┬────────────────────────────────┘
                              │
                              │ HTTPS
                              ▼
┌──────────────────────────────────────────────────────────────┐
│                 DIESELBROOK MIDDLEWARE PLATFORM             │
│                                                              │
│  API Layer                                                   │
│  - Shopify OAuth + app backend                               │
│  - Webhook receivers                                         │
│  - Legacy-compatible endpoints if required                   │
│                                                              │
│  Domain Services                                             │
│  - Order write-back service                                  │
│  - Product sync service                                      │
│  - Inventory sync service                                    │
│  - Customer sync service                                     │
│  - Consultant sync service                                   │
│  - Exclusive items sync service                              │
│  - Campaign pricing sync service                             │
│  - Fulfillment/status sync service                           │
│  - Cancellation sync service                                 │
│  - PayU reconciliation service                               │
│  - OTP / SMS / registration validation services              │
│  - Brevo / Meta CAPI / voucher services                      │
│                                                              │
│  Platform Services                                           │
│  - Job scheduler                                              │
│  - Retry / dead-letter / queue processing                    │
│  - Audit logging                                              │
│  - Health monitoring                                          │
│  - Reconciliation tooling                                     │
│                                                              │
│  Data / State                                                 │
│  - Sync watermarks                                            │
│  - Job execution history                                      │
│  - Idempotency keys                                           │
│  - Failure logs                                                │
│  - Reconciliation results                                     │
└─────────────────────────────┬────────────────────────────────┘
                              │
                              │ Direct SQL / secure private access
                              ▼
┌──────────────────────────────────────────────────────────────┐
│                    ACCOUNTMATE SQL SERVER                    │
│                                                              │
│  amanniquelive / amanniquenam / compplanlive / compsys      │
│  Stored procedures / triggers / current business logic       │
│  Campaign / CampDetail / icitem / arcust / soxitems / so*    │
└──────────────────────────────────────────────────────────────┘
```

### 4.3 Integration pattern by direction

#### Shopify to middleware

- webhooks for orders, customers, fulfillments, cancellations, and other near-real-time events
- Admin API for reading and writing structured commerce state

#### Middleware to AccountMate

- direct SQL access for reads and controlled procedure/table writes
- stored procedure calls where current operational logic already depends on them
- carefully scoped SQL login with least privilege

#### AccountMate to middleware

- scheduled polling against change indicators and source tables
- reconciliation passes for high-risk domains such as pricing and inventory
- boundary sweeps for time-based pricing activation and expiry

#### Middleware to Shopify

- Admin GraphQL for products, metafields, customers, orders, inventory, and app-owned data
- batched writes with retry and idempotency

---

## 5. Exact Functional Components To Build

### 5.1 Middleware platform

The middleware should be treated as the primary application deliverable.

Core components:

1. `ShopifyAppBackend`
2. `WebhookReceiver`
3. `OrderWritebackService`
4. `OrderCancellationService`
5. `OrderStatusFulfillmentService`
6. `ProductSyncService`
7. `InventorySyncService`
8. `ImageSyncService`
9. `CustomerSyncService`
10. `ConsultantSyncService`
11. `ExclusiveItemsSyncService`
12. `CampaignPriceSyncService`
13. `PricingParityService`
14. `PayUReconciliationService`
15. `SmsService`
16. `OtpService`
17. `RegistrationValidationService`
18. `BrevoSyncService`
19. `MetaCapiService`
20. `VoucherNotificationService`
21. `AuditLogService`
22. `JobOrchestrationService`
23. `ReconciliationDashboardService`

### 5.2 Shopify custom app

This should be a dedicated custom Shopify app owned and maintained by Dieselbrook for this merchant.

Core app components:

1. Admin app backend
2. Shopify Functions for pricing and access control
3. Checkout UI extensions for pricing/exclusive-access messaging
4. Theme app extensions where pre-checkout visibility logic is needed
5. Metafield and metaobject schema setup
6. Embedded admin pages for sync status, overrides, and diagnostics

### 5.3 Pricing and access-control layer

Assuming Plus, the safest architecture is still:

- middleware computes and syncs effective pricing state from AccountMate
- Shopify stores the effective state in metafields
- Shopify Functions apply deterministic logic at checkout

This is still preferred over trying to make Shopify the pricing source of truth.

### 5.4 Operational support layer

Non-negotiable production features:

- structured logs
- dead-letter handling
- job retry policy
- sync failure visibility
- replay tooling
- parity checks for pricing and inventory
- health endpoints
- alerting

---

## 6. Recommended Technology Stack

### 6.1 Primary recommendation

The strongest delivery stack for Dieselbrook is:

| Layer | Recommendation | Why |
|---|---|---|
| Middleware runtime | .NET 8 | Best fit for ERP-heavy integration work, SQL Server access, strong background services model, and alignment with the existing `AnqIntegrationApiSource` lineage |
| API framework | ASP.NET Core | Mature, fast, clean hosting model for webhooks, admin APIs, and background services |
| Data access | Dapper | Precise control over SQL, better fit than heavy ORM usage for SP/table-centric integration logic |
| Background jobs | Hosted services + Quartz.NET or Hangfire | Reliable recurring jobs, retry orchestration, execution history |
| Queue / async decoupling | Azure Service Bus | Strong operational model for retries and dead-lettering |
| Persistence for middleware state | SQL Server or Azure SQL | Good fit for sync watermarks, audit tables, reconciliation results |
| Logging | Serilog + Seq or Application Insights | Production-grade structured visibility |
| Shopify app backend | Node.js app shell only if required by specific Shopify tooling; otherwise keep in .NET-backed service boundary | Keep operational complexity low; avoid splitting platforms unless Shopify tooling forces it |
| Shopify Functions | Rust | Best performance and strongest fit for deterministic logic |
| Checkout UI extensions | React + Shopify UI extensions | Standard Shopify extension model |
| Theme extensions | Liquid + small JS layer | Low complexity for storefront messaging |
| CI/CD | GitHub Actions | Straightforward deployment automation |
| Secrets | Azure Key Vault | Required for production hygiene |
| Hosting | Azure App Service or containerized Azure-hosted runtime with private connectivity | Best balance of maintainability and managed operations |

### 6.2 Why .NET 8 is the best primary choice

It is the strongest fit for:

- SQL Server-heavy reads and writes
- stored procedure integration
- long-running recurring jobs
- operational reliability
- structured logging and health checks
- deterministic error handling
- maintainability under one engineering owner

### 6.3 When Node.js should be used

Node.js should be used only where Shopify tooling makes it materially easier, such as:

- Shopify app bootstrap tooling
- some admin UI / extension workflows

If used, Node.js should remain narrow in scope. The core middleware should not be split across two primary backend stacks unless absolutely necessary.

### 6.4 Hosting model

Recommended production topology:

1. Azure-hosted middleware runtime
2. secure private network connectivity to AccountMate SQL Server
3. App Insights / Seq for observability
4. Azure Service Bus for retry and decoupling
5. Azure Key Vault for secrets

If private Azure-to-SQL connectivity cannot be guaranteed with acceptable latency, Dieselbrook should prefer a LAN-adjacent Windows server or private VM close to AccountMate.

---

## 7. Data and Sync Design

### 7.1 Source-of-truth model

| Domain | Source of truth |
|---|---|
| Orders written into ERP | Shopify event + middleware write-back into AccountMate |
| Inventory and product master | AccountMate |
| Consultant identity and status | AccountMate |
| Exclusive access rights | AccountMate `soxitems` |
| Campaign pricing | AccountMate `Campaign` / `CampDetail` |
| Effective consultant price | Middleware-computed from AccountMate rules |
| Checkout application of pricing | Shopify Function using synced effective prices |

### 7.2 Pricing design under Plus

Assuming Shopify Plus, Dieselbrook should still implement pricing as:

- middleware-calculated effective consultant prices
- synced to Shopify product or variant metafields
- enforced by a small deterministic discount Function

Reason:

- it stays faithful to AccountMate
- it is testable against `sp_camp_getSPprice`
- it avoids re-creating a full ERP pricing engine inside Shopify
- it supports future changes better than hardcoding pricing logic in the storefront

### 7.3 Exclusive access design

Recommended model:

- sync consultant-specific allowed item codes from `soxitems`
- store access state in Shopify customer metafields
- mark protected products with item-code and access-required metafields
- enforce at checkout via validation Function
- optionally improve pre-checkout UX via theme extension visibility checks

### 7.4 Required sync patterns

| Pattern | Why it exists |
|---|---|
| Delta polling | Standard ERP-side change detection |
| Boundary sweeps | Required for timed campaign pricing changes |
| Nightly reconciliation | Required for pricing and inventory drift detection |
| Webhook-first inbound processing | Required for near-real-time order and customer events |
| Idempotent write-back | Required to avoid duplicate ERP writes |

---

## 8. Delivery Workstreams

### Workstream 1: Discovery closure and solution definition

- close remaining open technical decisions
- finalize field mappings
- finalize hosting topology
- finalize security design
- finalize cutover assumptions

### Workstream 2: Middleware platform foundation

- solution scaffolding
- auth, secrets, environments
- logging, health, monitoring
- scheduler and queue foundation
- sync-state persistence

### Workstream 3: Shopify custom app foundation

- app registration
- OAuth and token handling
- extension scaffolding
- metafield schema registration
- admin shell

### Workstream 4: Core ERP integrations

- order write-back
- cancellation handling
- fulfillment and order status
- product and inventory synchronization
- image synchronization

### Workstream 5: Consultant, access, and pricing model

- consultant sync
- exclusive item access sync
- campaign pricing sync
- pricing parity harness
- Shopify Functions for discount and validation

### Workstream 6: Supporting side integrations

- PayU reconciliation
- OTP and SMS
- registration validation
- Brevo sync
- Meta CAPI
- voucher notifications

### Workstream 7: Operational readiness

- replay tooling
- failure dashboards
- reconciliation reporting
- deployment automation
- UAT support
- cutover checklist and hypercare

---

## 9. Exact Effort Breakdown

### 9.1 High-level delivery estimate

Assuming one senior integration engineer leading architecture and implementation, with intermittent support for QA/UAT and Shopify front-end extension work:

| Phase | Estimated effort |
|---|---|
| Discovery closure and detailed technical design | 1.5-2.5 weeks |
| Middleware platform foundation | 1.5-2 weeks |
| Shopify app foundation | 1-1.5 weeks |
| Core product, inventory, and order integrations | 3-4 weeks |
| Consultant access and pricing engine | 2.5-3.5 weeks |
| Supporting side integrations | 2-3 weeks |
| UAT, hardening, cutover preparation, and hypercare setup | 2-3 weeks |
| **Total** | **14-19 weeks** |

### 9.2 Detailed effort by component

| Component | Effort | Notes |
|---|---|---|
| Architecture finalization and mapping pack | 4-6 days | Final contracts, flow design, deployment decisions |
| Middleware solution skeleton | 3-4 days | Environments, logging, health, auth, scheduler |
| Shopify custom app bootstrap | 2-4 days | OAuth, embedded shell, API plumbing |
| Product sync service | 4-6 days | Full + delta product update logic |
| Inventory sync service | 3-5 days | Availability, quantities, retry handling |
| Image sync service | 2-3 days | Lower risk but operationally useful |
| Order write-back service | 5-8 days | Highest business criticality |
| Cancellation + order status services | 3-5 days | Must be exact and auditable |
| Customer + consultant sync | 3-5 days | Includes identity and status mapping |
| Exclusive items sync | 3-4 days | `soxitems` to customer metafields |
| Campaign pricing sync | 4-6 days | Includes delta + boundary sweep design |
| Pricing parity harness | 2-4 days | Test protection against AM drift |
| Discount Function | 3-5 days | Small if data model is right |
| Validation Function | 2-3 days | Exclusive-item gate |
| Checkout/theme extensions | 3-5 days | Messaging and UX reinforcement |
| PayU reconciliation integration | 3-5 days | Important financial control |
| OTP / SMS / registration validation | 3-4 days | User account operations |
| Brevo / Meta CAPI / voucher services | 4-6 days | Business support integrations |
| Reconciliation dashboard + support tooling | 3-5 days | Needed for operational ownership |
| UAT support and hardening | 7-10 days | Defect fixing and production readiness |

### 9.3 Impact of team shape on timeline

| Team model | Expected timeline |
|---|---|
| 1 senior full-stack integration lead | 14-19 weeks |
| 1 lead + 1 support engineer | 10-14 weeks |
| 1 lead + dedicated Shopify extension/front-end support + QA support | 9-12 weeks |

### 9.4 What drives the effort most

- ERP write-back complexity
- pricing parity confidence
- hidden production edge cases in stored procedures and jobs
- external services with incomplete ownership or weak documentation
- test/UAT access and responsiveness from client stakeholders

---

## 10. Delivery Impact

### 10.1 Business impact

If delivered well, this architecture gives Dieselbrook and Annique:

- a stable Shopify commerce platform
- preservation of critical ERP-driven business logic
- reduced dependency on legacy VFP/Nop integration code
- much better operational visibility than the current system
- a cleaner long-term base for future improvements

### 10.2 Technical impact

- replaces undocumented legacy sync behaviour with explicit services
- centralizes operational control in one owned middleware platform
- separates source-of-truth decisions from presentation and checkout execution
- reduces hidden integration behaviour living inside SQL jobs and old web stacks

### 10.3 Operational impact

- introduces a real support surface with logs, health checks, and diagnostics
- makes failed syncs visible instead of silent
- enables replay and controlled recovery instead of ad hoc manual fixes

---

## 11. Risks

### 11.1 Primary delivery risks

| Risk | Severity | Why it matters | Mitigation |
|---|---|---|---|
| ERP write-back behaviour is more complex than documented | High | Orders, cancellations, and statuses are the most sensitive business flows | Build against staging first, document exact SP/table paths, add audit trails |
| Hidden pricing exceptions exist beyond current findings | High | Pricing mistakes are commercially visible immediately | Use `sp_camp_getSPprice` parity harness, expand SQL archaeology before final pricing sign-off |
| Hosting connectivity to AccountMate is unstable or slow | High | Middleware depends on reliable SQL access | Finalize topology early, prove latency and firewall path before build lock |
| Supporting integration endpoints are poorly owned | Medium-High | OTP, SMS, ShopAPI, Brevo, and notifications may fail for non-code reasons | Treat each as a separate integration contract and verify ownership early |
| Cutover introduces temporary drift between Shopify and ERP | Medium-High | Can affect orders, pricing, stock, and consultant access | Parallel reconciliation, pre-cutover validation pack, hypercare plan |
| Business rules are split across SQL, jobs, and custom apps | High | Missing one path can cause invisible regressions | Maintain a formal integration inventory and trace each replaced behaviour |

### 11.2 Secondary risks

| Risk | Severity | Mitigation |
|---|---|---|
| Scope creep from “nice-to-have” Shopify UX changes | Medium | Keep core integration scope separate from storefront enhancement scope |
| Merchant-side process ambiguity | Medium | Require explicit sign-off on each critical process |
| Production data quality issues | Medium | Add validation and reconciliation checks instead of trusting source blindly |
| Over-splitting the stack across too many technologies | Medium | Keep core middleware on one primary backend platform |

---

## 12. Key Focus Areas For Delivery

### Focus Area 1: Order integrity

The first priority is ensuring that Shopify orders become correct AccountMate orders every time.

This includes:

- successful order creation
- correct order status transitions
- correct cancellation behaviour
- correct payment-state handling
- correct auditability

### Focus Area 2: Pricing correctness

Pricing is one of the highest-risk areas because errors are immediately visible to the merchant and consultants.

Key actions:

- treat AccountMate as the pricing oracle
- precompute and sync effective pricing state
- validate parity against live SQL procedures
- separate pricing calculation from pricing display

### Focus Area 3: Consultant access control

The `soxitems` model is business critical. It must be treated as an access-control problem, not just a merchandising problem.

Key actions:

- sync access lists accurately
- enforce at checkout
- make pre-checkout behaviour clear to users

### Focus Area 4: Operational supportability

If the system cannot be diagnosed quickly, Dieselbrook becomes the support bottleneck.

Key actions:

- structured logging everywhere
- explicit job visibility
- replay support
- reconciliation reporting
- alerting on failures

### Focus Area 5: Controlled integration replacement

Do not replace the legacy ecosystem in one abstract jump. Replace it behaviour by behaviour.

Key actions:

- map every current job/service/procedure to its target replacement
- verify each path in staging
- track replacement coverage formally

### Focus Area 6: Production topology certainty

Hosting and network shape must be locked early. This is not an infrastructure detail; it is a delivery dependency.

---

## 13. Recommended Delivery Approach For Dieselbrook

### Recommendation

Dieselbrook should position this work as a **three-part engagement**:

1. Discovery closure and technical definition
2. Core middleware and custom app implementation
3. UAT, cutover, and hypercare stabilization

### Why this is the right commercial framing

- it reflects real technical complexity
- it gives Dieselbrook room to validate hidden ERP behaviour before hard implementation commitments
- it separates design risk from build effort cleanly
- it makes stakeholder sign-off easier at logical checkpoints

### Suggested implementation order

1. Finalize hosting and connectivity
2. Finalize order, pricing, and access-control contracts
3. Build middleware foundation
4. Build order/product/inventory core flows
5. Build consultant/pricing/access-control layer
6. Build side integrations
7. Add operational tooling and reconciliation
8. Run UAT and hardening
9. Plan cutover with hypercare

---

## 14. Less Technical Summary For Dieselbrook Discussion

### Simple explanation

This project is not mainly about building a Shopify store.

It is about building the software layer that allows Shopify to work properly with Annique's existing ERP and business rules.

The current business relies on a lot of hidden integration logic:

- SQL jobs
- stored procedures
- consultant rules
- special pricing
- exclusive-item access
- payment reconciliation
- messaging and registration flows

Shopify Plus helps, but it does not magically replace those moving parts.

### What Dieselbrook would actually be delivering

Dieselbrook would be delivering:

- the architecture for the new integration platform
- the middleware that connects Shopify and AccountMate
- the custom Shopify app needed for pricing and access control
- the scheduled sync services that keep Shopify aligned with ERP data
- the operational tooling needed to support the solution in production

### Why the middleware matters so much

Without a solid middleware layer:

- orders can fail to reach AccountMate correctly
- stock can drift
- consultant prices can be wrong
- exclusive products can become visible to the wrong users
- support becomes manual and expensive

### What Shopify Plus improves

Shopify Plus makes it easier to implement custom checkout and app behaviour, and it removes some platform constraints. That helps Dieselbrook deliver a cleaner solution.

But the difficult part is still the integration work, not the Shopify subscription.

### Practical expectation on effort

This should be treated as a multi-month integration delivery, not a quick theme build.

A realistic expectation is roughly:

- 3 to 5 months with one strong lead doing most of the work
- faster with support from an additional engineer and QA capacity

### Best way to discuss it with stakeholders

The cleanest message is:

> Shopify Plus gives us the right commerce platform, but the critical success factor is the custom middleware and integration layer that preserves Annique's ERP-driven business rules.

And then:

> If we get the integration layer right, the storefront becomes manageable. If we get it wrong, pricing, access, orders, and support all become unstable.

---

## 15. Final Recommendation

Dieselbrook should treat the implementation as a **mission-critical integration programme with Shopify as the commerce front end**, not as a simple replatforming exercise.

The recommended execution stance is:

- use Shopify Plus
- keep AccountMate as the operational source of truth where required
- build a strong .NET 8 middleware platform
- use a custom Shopify app with Functions and extensions where needed
- invest early in pricing correctness, order integrity, and operational visibility
- structure delivery in phases with clear sign-off checkpoints

If Dieselbrook wants a stable production outcome and a supportable long-term solution, this is the correct architectural direction.