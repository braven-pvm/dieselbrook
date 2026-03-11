# Order Replacement Touchpoint Map

## Purpose

This document maps the current order-related legacy touchpoints to the replacement responsibilities Dieselbrook will need to own.

The goal is to replace behavior, not just individual stored procedure calls.

## Core Finding

The current order integration spans four layers at once:

1. NopCommerce storefront and custom plugin behavior
2. FoxPro `NISource` middleware routing and orchestration
3. direct SQL writes and reads in `amanniquelive`
4. downstream operational dependencies such as `SOPortal`, invoicing, tracking, and consultant lifecycle side effects

## Touchpoint Map

| Current touchpoint | Current role | Confirmed data surface | Dieselbrook replacement |
|---|---|---|---|
| `NISource/syncorders.prg` | imports web orders into ERP and triggers side effects | `sosord`, `sostrs`, `soskit`, `arcust`, `SOPortal`, `arcash`, `sp_ws_reactivate` | `OrderWritebackService` |
| `NISource/syncorderstatus.prg` | returns fulfillment / order status back toward commerce | `soship`, `arinvc`, `SOPortal`, tracking-related data | `OrderStatusFulfillmentService` |
| `NISource/apiprocess.prg` | process routing and API entry point for sync actions | `NopIntegration`, ERP reads, consultant/order dispatch | `ShopifyAppBackend` plus explicit webhook and job endpoints |
| `NISource/nopintegrationmain.prg` | West Wind server bootstrap and process host | `wwrequestlog`, request routing, app/api/brevo processes | middleware API host and job orchestrator |
| AccountMate order tables | source-of-truth order, shipment, invoice, cash, customer state | `sosord`, `soship`, `sostrs`, `soskit`, `arinvc`, `arcash`, `arcust` | typed SQL gateway plus canonical order model |
| Operational order portal | warehouse / order status dependency | `SOPortal` | explicit downstream status integration and parity checks |
| Campaign and exclusive-item support | order eligibility and campaign pricing side rules | `Campaign`, `CampDetail`, `soxitems` | `CampaignPriceSyncService` and `ExclusiveItemsSyncService` |
| Legacy middleware support DB | reports, settings, SSO, logs, marketing support | `NopReports`, `NopSettings`, `NopSSO`, `BrevoLog`, `wwrequestlog` | middleware-owned configuration, auth, audit, and reporting support |

## Phase-1 Replacement Scope

Phase 1 should replace the following order behaviors completely:

- web order ingestion from the commerce platform into ERP
- idempotent creation of ERP order records and all required dependent records
- reverse status synchronization from ERP fulfillment state back to commerce
- order-triggered consultant reactivation handling where still required
- visibility and reconciliation for failures, retries, and partial downstream success

## Behaviors That Need Explicit Validation

- whether every imported order must result in `SOPortal` parity
- whether payment-side postings in `arcash` are synchronous, deferred, or conditional by payment method
- which exact shipment and invoice states should drive commerce fulfillment updates
- how exclusive items and campaign pricing interact when an order is imported

## Highest-Risk Zones

- hidden side effects during order import, especially consultant lifecycle changes
- assuming invoice creation is a finance-only concern when MLM and reporting also depend on it
- underestimating `NopIntegration` support tables because they look secondary to the main ERP schema
- replacing only stored procedure doorbells while leaving FoxPro orchestration behavior unmapped

## Dieselbrook Design Guidance

- treat order import as a multi-step workflow with durable checkpoints
- add an idempotency ledger independent of current `cPono` matching rules
- separate business events from SQL write mechanics so order replay is safe
- preserve downstream parity observability for `SOPortal`, invoices, and status propagation