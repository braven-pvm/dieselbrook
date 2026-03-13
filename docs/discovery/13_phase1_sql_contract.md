# Phase 1 SQL Contract

## Purpose

This document defines the minimum SQL access contract Dieselbrook is likely to need for phase 1.

It is intentionally conservative.

Where current behavior is confirmed, the contract says so.

Where a write path is still plausible but not fully proven, the contract marks it as validation-required instead of asserting it as fact.

## Scope Rule

Phase 1 should minimize direct SQL write scope and keep writes constrained to the smallest set of objects and procedures needed for business parity.

## Database-Level Contract

| Database | Phase-1 access stance | Why |
|---|---|---|
| `amanniquelive` | read plus controlled write | core ERP write-back and operational reads for orders, customer state, status, and pricing support |
| `compplanLive` | read-first, procedure-call only if unavoidable | MLM hierarchy, report, statement, and lifecycle reads are required; broad writes are not justified for phase 1 |
| `compsys` | prefer no direct writes unless preserving legacy mail queue temporarily | mail queue support exists, but Dieselbrook should ideally own outbound messaging rather than write into legacy queue tables |
| `NopIntegration` | read and narrow write | report metadata and integration support reads are required; some transitional support writes may be needed for Brevo-style logging or compatibility |

## Object-Level Contract

### `amanniquelive`

| Object | Read | Write | Reason |
|---|---|---|---|
| `arcust` | yes | validation-required | consultant/customer lookup is required; direct writes depend on whether lifecycle updates stay in SQL or move behind procedures |
| `sosord` | yes | yes | order header write-back is phase-1 core behavior |
| `sostrs` | yes | yes | order transaction detail is part of order import |
| `soskit` | yes | yes | line/kit detail is part of order import |
| `soship` | yes | validation-required | reads are required for fulfillment/status; direct writes need confirmation |
| `arinvc` | yes | no for phase 1 middleware | invoices are operationally critical reads; creation is likely downstream ERP behavior rather than middleware direct insert |
| `arcash` | yes | validation-required | payment-related reads matter; direct write depends on current import path and payment mode |
| `SOPortal` | yes | validation-required | parity may require downstream operational updates, but direct write path should be verified before assuming it |
| `be_waybill` | yes | no | tracking/waybill reads support fulfillment visibility |
| `changes` | yes | no | change tracking and reconciliation support |
| `soxitems` | yes | no | exclusive-item eligibility support |
| `Campaign` | yes | no | campaign pricing read support |
| `CampDetail` | yes | no | campaign pricing read support |
| `wsSetting` | yes | no | sync/config support |
| `sp_ws_reactivate` | call | yes, controlled | confirmed order-side lifecycle side effect for consultant reactivation |

### `compplanLive`

| Object | Read | Write | Reason |
|---|---|---|---|
| `CTcomph` | yes | no | commission and history reporting |
| `CTcomp` | yes | no | current-period commission/report reads |
| `CTconsv` | yes | no | consultant sales/grouping and pricing support |
| `CTRunData` | yes | no | consultant run metrics and segmentation |
| `CTstatement` | yes | no | current statement/report access |
| `CTstatementh` | yes | no | historical statement/report access |
| `CTdownlineh` | yes | no | sponsor/downline and hierarchy history reads |
| `deactivations` | yes | no direct write | lifecycle/reactivation logic reads are required; direct writes are not justified in phase 1 |
| hierarchy functions used by middleware | yes | no | source analysis indicates direct function reads for downline/history validation |

### `compsys`

| Object | Read | Write | Reason |
|---|---|---|---|
| `MAILMESSAGE` | validation-required | validation-required | only needed if temporary compatibility with legacy queued-mail path is retained |
| `sp_Sendmail` | validation-required | validation-required | same as above; preferred direction is to replace rather than depend on it |

### `NopIntegration`

| Object | Read | Write | Reason |
|---|---|---|---|
| `NopReports` | yes | validation-required | report metadata read is likely required; direct write only if report admin parity is preserved |
| `BrevoLog` | yes | validation-required | current middleware logs Brevo activity; Dieselbrook may replace this with its own audit store instead |
| `ApiClients` | yes | no preferred | useful for understanding current API client estate; new platform should own its own client/auth model |
| `NopSettings` | yes | no preferred | useful migration reference, but new platform should own settings separately |
| `NopSSO` | yes | no preferred | transitional read support only if SSO behavior must be carried forward |
| `wwrequestlog` | yes | no preferred | legacy request trace reference; new middleware should log elsewhere |

## Access Principles

### Principle 1: read-first on legacy support databases

`compplanLive`, `compsys`, and most of `NopIntegration` should be treated primarily as read/reference estates during phase 1.

### Principle 2: writes must be explicit and audited

Every allowed SQL write should map to a named business action such as:

- create ERP order header
- create ERP order detail
- invoke consultant reactivation
- write temporary compatibility log

### Principle 3: avoid generic table-write permissions

Grant rights to a narrow SQL principal with only the objects and procedures Dieselbrook must use.

### Principle 4: prefer service-owned state for new behavior

Do not extend legacy tables for new middleware concerns such as:

- idempotency
- retry state
- dead-letter tracking
- webhook receipts
- operator reconciliation notes

Those concerns belong in Dieselbrook-owned persistence.

## Recommended First-Cut SQL Permission Shape

1. `amanniquelive`: object-level read on order, customer, shipment, invoice, campaign, and support tables plus controlled write only to the confirmed order-import path and `sp_ws_reactivate`.
2. `compplanLive`: read-only access on phase-1 consultant and reporting tables plus execute only for any hierarchy function or procedure proven necessary.
3. `compsys`: no access by default unless a temporary mail-queue compatibility decision is made.
4. `NopIntegration`: read-only by default, then add narrow write only if a transitional compatibility need is proven.

## Decisions Dieselbrook Should Force Early

1. Whether outbound mail will still flow through `compsys` during transition.
2. Whether any `NopIntegration` writes are truly required or whether that database can be treated as read-only reference plus historical audit.
3. Whether `SOPortal` parity requires direct middleware writes or only downstream ERP process parity.
4. Whether any consultant lifecycle updates besides `sp_ws_reactivate` must remain SQL-side in phase 1.