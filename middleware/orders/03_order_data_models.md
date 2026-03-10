# Order Data Models

## Purpose

This document separates:

- current AccountMate order and invoice structures
- the canonical order model Dieselbrook should use inside middleware
- the minimum middleware persistence tables needed for safe replication

## Current AccountMate Data Model

## Core operational tables

| Table | Role | Notes |
|---|---|---|
| `sosord` | sales-order header | main operational order record |
| `sostrs` | sales-order lines | pricing, tax, quantity, revenue, campaign refs |
| `soskit` | kit components | separate decomposition for bundled items |
| `SOPortal` | warehouse/process status overlay | minimal but operationally important |
| `arcash` | receipt/payment artefacts | created during legacy import for non-staff orders |
| `arcust` | customer master | can be updated during import via reactivation and balance changes |

## Downstream financial tables

| Table | Role | Notes |
|---|---|---|
| `arinvc` | invoice header | financial realization of order |
| `aritrs` | invoice lines | line-level invoice realization |

## `sosord` header fields that matter most

| Field | Meaning |
|---|---|
| `cSono` | AM sales-order number |
| `cCustno` | AM customer account |
| `cPono` | external order reference, currently NOP Order ID |
| `cOrderBy` | source customer reference |
| `cEnterBy` | source/channel marker such as `Web Order` |
| `cShipVia` | shipping method / carrier |
| `cFrgtCode` | freight code |
| `cPayCode` | payment code |
| `dOrder` | order date |
| `cb*` fields | billing address copy |
| `cs*` fields | shipping address copy |
| `nSalesAmt`, `nDiscAmt`, `nFrtAmt`, `nTaxAmt*` | commercial totals |
| `lHold`, `lVoid`, `lSource` | operational flags |

## `sostrs` line fields that matter most

| Field | Meaning |
|---|---|
| `cSono` | parent order |
| `cLineItem` | line identifier |
| `cItemNo` | AM SKU |
| `cWarehouse` | warehouse |
| `cRevnCode` | revenue code |
| `nOrdQty` | ordered quantity |
| `nShipQty` | shipped quantity |
| `nPrice` | unit price excl tax |
| `nPrcInCtx` | unit price incl tax |
| `nDiscAmt` | line discount amount |
| `nSalesAmt` | line sales amount |
| `cSpprUid` | campaign/pricing link |
| `lKitItem` | kit marker |
| `lFree`, `lStock`, `lUpsell` | behavioural flags |

## `SOPortal` fields

| Field | Meaning |
|---|---|
| `cSono` | parent order |
| `dCreate` | portal row creation |
| `dPrinted` | warehouse print marker |
| `cStatus` | operational state |

## Canonical Middleware Order Model

The new middleware should not use Shopify payloads directly as its working model.

Recommended canonical order root:

| Field | Description |
|---|---|
| `source_system` | `shopify` |
| `shopify_order_id` | immutable external identity |
| `shopify_order_name` | human-readable order reference |
| `shopify_customer_id` | Shopify customer reference |
| `resolved_am_customer` | final AM `ccustno` |
| `resolved_salesperson` | AM `cSlpnNo` if applicable |
| `financial_status` | canonical payment state |
| `fulfillment_status` | canonical fulfillment state |
| `shipping_method_code` | normalized shipping method |
| `order_date_utc` | order creation timestamp |
| `currency_code` | order currency |
| `billing_address` | normalized billing block |
| `shipping_address` | normalized shipping block |
| `order_lines` | canonical order-line array |
| `payments` | canonical payment fragment array |
| `discounts` | canonical discount array |
| `notes` | canonical order-note array |
| `source_payload_hash` | change detection |

## Canonical order line model

| Field | Description |
|---|---|
| `shopify_line_id` | immutable external line identity |
| `sku` | source SKU |
| `resolved_am_itemno` | final AM item number |
| `description` | normalized line description |
| `quantity` | ordered quantity |
| `price_excl_tax` | unit price excluding tax |
| `price_incl_tax` | unit price including tax |
| `discount_rate` | applied line discount rate |
| `discount_amount` | applied line discount amount |
| `tax_rate` | line tax rate |
| `tax_amount` | line tax amount |
| `is_kit` | whether decomposition is required |
| `kit_components` | resolved AM kit component array |
| `campaign_ref` | campaign/pricing reference if used |
| `attributes` | raw and normalized option/attribute data |

## Canonical payment model

| Field | Description |
|---|---|
| `payment_type` | `card`, `eft`, `giftcard`, `payflex`, etc. |
| `payment_reference` | transaction reference |
| `amount` | amount attributed to this payment fragment |
| `status` | authorized / paid / settled / unknown |
| `source_evidence` | webhook, transaction, note, or API source |

## Middleware Persistence Tables

At minimum, Dieselbrook should create its own order integration tables.

## `order_sync`

| Column | Purpose |
|---|---|
| `id` | internal row id |
| `shopify_order_id` | unique business key |
| `shopify_order_name` | support lookup |
| `am_csono` | AM order reference |
| `am_cpono` | AM external reference |
| `sync_status` | pending / processing / synced / failed / quarantined |
| `last_attempt_at` | retry tracking |
| `last_success_at` | success tracking |
| `last_error_code` | failure categorization |
| `last_error_message` | support detail |
| `payload_hash` | mutation detection |

## `order_sync_attempt`

| Column | Purpose |
|---|---|
| `id` | attempt id |
| `order_sync_id` | parent sync row |
| `started_at` | start timestamp |
| `completed_at` | completion timestamp |
| `result` | success / retry / failed |
| `raw_payload_ref` | raw event reference |
| `canonical_snapshot` | canonical contract snapshot |
| `am_write_summary` | written tables and keys |

## `order_status_sync`

| Column | Purpose |
|---|---|
| `shopify_order_id` | business key |
| `am_csono` | AM order reference |
| `am_status` | latest observed AM status |
| `shopify_fulfillment_status` | latest pushed Shopify state |
| `tracking_number` | pushed tracking number |
| `tracking_url` | pushed tracking link |
| `last_synced_at` | status sync timestamp |

## Mapping Philosophy

Do not let AM table structures become the public middleware contract.

The correct layering is:

1. Shopify payload
2. canonical middleware order model
3. AM write model

That separation is what makes replay, testing, and future platform change viable.