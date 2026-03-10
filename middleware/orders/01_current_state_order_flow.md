# Current-State Order Flow

## Scope

This document explains how orders work today across:

- NopCommerce
- the FoxPro middleware hosted behind `nopintegration.annique.com`
- AccountMate sales-order tables
- `SOPortal`
- status synchronization back to the web layer

## Core Finding

The current order integration is a two-stage process:

1. AccountMate calls a middleware endpoint with no payload.
2. The middleware reads the order directly from NopCommerce SQL and writes the result directly into AccountMate SQL.

That means the real contract is behavioural, not HTTP-body based.

## Source Components Involved

| Component | Role |
|---|---|
| `sp_NOP_syncOrders` | Wakes the middleware order-import process |
| `sp_NOP_syncOrderStatus` | Wakes the reverse status synchronization process |
| `NISource/syncorders.prg` | Main import logic from NOP into AccountMate |
| `NISource/syncorderstatus.prg` | Reads AM shipping state and updates NOP shipment/order visibility |
| `sp_ws_reactivate` | Reactivates inactive consultants during import if needed |
| `sp_ws_getorderstatus` | AM-side status lookup used during reverse shipment sync |

## Doorbell Procedure Pattern

### `sp_NOP_syncOrders`

Confirmed behaviour:

- checks `wsSetting.ws.url` only as a kill-switch
- hardcodes the destination to `https://nopintegration.annique.com/syncorders.api`
- calls `sp_ws_HTTP`
- sends no order payload

Meaning:

- the ERP procedure does not serialize the order
- the middleware must fetch order data itself
- the endpoint behaves like a wake-up trigger, not an API contract

### `sp_NOP_syncOrderStatus`

Confirmed behaviour:

- same hardcoded wake-up pattern
- calls `https://nopintegration.annique.com/syncorderstatus.api?instancing=single`
- sends no status payload

Meaning:

- reverse sync logic must re-read current AM status after being triggered

## Import Flow in `syncorders.prg`

## 1. Order load and prerequisite checks

The middleware loads:

- order header
- billing address
- shipping address
- order notes
- order lines
- order discounts
- gift-card usage

If billing, shipping, or order lines are missing, processing fails.

## 2. Customer-role to AM-account mapping

The middleware maps NOP customer roles to AM `ccustno` values:

| NOP role | AM customer |
|---|---|
| `Annique Consultant` | consultant username as `ccustno` |
| `AnniqueStaff` | `STAFF1` |
| `AnniqueExco` | `STAFF1` |
| `Bounty` | `ASHOP1` |
| `Client` | `ASHOP2` |
| default | `ASHOP1` |

There is also a special `lInterCo` flag for the `Bounty` path.

## 3. Payment interpretation

The middleware inspects:

- gift-card usage
- `Atluz.PayUSouthAfrica` order-note data
- payment-method fragments and amounts

The result is an internal payment collection used later to create `arcash` entries.

This means the current order import is not purely logistical. It also writes payment-side records into AccountMate.

## 4. Duplicate prevention

The middleware checks for an existing SO using:

- `sosord.cPono = NOP Order ID`
- `sosord.ccustno = resolved AM customer`

If a match exists, the order is treated as already processed.

## 5. Customer reactivation side effect

If the target AM customer exists but is inactive, the middleware executes:

- `EXEC sp_ws_reactivate @ccustno = ...`

That can change consultant state before the order is imported.

## 6. Sales-order header construction

The middleware builds `sosord` with values including:

- `cSono = PADL(CustomOrderNumber, 10)`
- `cCustno = resolved AM customer`
- `cPono = NOP Order ID`
- `cShipVia` from order shipping logic
- `cOrderBy = CustomerID`
- `cPayCode = CWO`
- `lSource = 4`
- `dOrder = order created date`
- `cEnterBy = Shop Order` for shop accounts and `Web Order` for non-shop accounts

It also populates full billing and shipping address copies.

## 7. Shipping-method translation

The source code sets shipping/freight codes using branching logic:

| Condition | `cShipVia` / `cFrgtCode` |
|---|---|
| collect or staff order | `COLLECT` |
| test account | `BERCO` |
| pickup attributes present | `COURIER`, then possibly `POSTNET` or `SKYNET` |
| otherwise | `COURIER` |

The code also sets:

- `cIoNo`
- `cSAddrNo`
- pickup-company overrides

for specific pickup-point scenarios.

## 8. Order-line construction

For each order line, the middleware builds `sostrs` with:

- line identity from NOP order-item ID
- item number from SKU
- warehouse from middleware settings
- revenue code from item warehouse rules, except staff orders use `STAFF-01`
- quantity, price, discount, tax, extended values
- stock and kit flags
- campaign/pricing reference `csppruid`

Important logic:

- staff orders are priced at cost (`iciwhs.nCost`)
- standard orders use NOP line prices and discounts
- tax is recalculated into AM line fields
- weight rolls up to the header

## 9. Kit decomposition

If an item is a kit:

- middleware loads kit components from `icikit`
- creates `soskit` rows for each component

This means a Shopify replacement cannot assume an order is representable as header plus lines only.

## 10. Transactional write to AM

`PostToAM` opens a transaction and writes, in order:

1. `sosord`
2. each `sostrs` row
3. inventory booked updates via `iciwhs.UpdateBooked`
4. each `soskit` row
5. `arcash` receipts for non-staff orders
6. `SOPortal`

If any step fails, the transaction rolls back.

## 11. `arcash` payment writes

For each interpreted payment fragment, the middleware creates an `arcash` entry with:

- receipt number
- AM customer
- `cPayCode` based on payment type
- `cSono` linked to the sales order
- `nPaidAmt`
- `cPayRef`

Observed payment-code logic:

| Payment type | `arcash.cPayCode` |
|---|---|
| default card/payu | `PAYUC` |
| EFT | `PAYUE` |
| Payflex | `PAYFLEX` |
| gift card | `AFF` |

The middleware also updates `arcust.nbalance` and `arcust.nopencr`.

## 12. `SOPortal` creation

The middleware inserts a `SOPortal` row with:

- `cSono`
- `dCreate = DATETIME()`
- `cStatus = ''`
- `dPrinted = NULL`

This is the start of the warehouse/portal lifecycle.

## `SOPortal` Operational Status Flow

Confirmed current-state interpretation:

| Status | Meaning |
|---|---|
| blank / space | pending |
| `P` | picking |
| `S` | shipped |

`dPrinted` is also used as an operational marker.

## Reverse Status Sync in `syncorderstatus.prg`

The reverse flow is:

1. query NOP orders that are paid/eligible and have shipments
2. load matching AM sales order via padded `CustomOrderNumber`
3. execute `sp_ws_getorderstatus <NOP Order ID>`
4. interpret shipping method to derive tracking URL and customer note text
5. update NOP shipment tracking number and admin comments
6. mark shipment sent or delivered for collect/no-tracking paths where appropriate

Shipping-provider branches include:

- `BERCO` -> Aramex link
- `FASTWAY` -> Fastway link
- `POSTNET` -> Aramex link
- `SKYNET` -> Paxi link
- `COLLECT` -> no courier tracking, direct progression path

## Current-State Table Set

| Table | Role in flow |
|---|---|
| `sosord` | AM order header |
| `sostrs` | AM order lines |
| `soskit` | AM kit components |
| `SOPortal` | warehouse/process overlay |
| `arcash` | imported payment receipts |
| `arcust` | customer status and balance side effects |
| `arinvc` | downstream invoice header |
| `aritrs` | downstream invoice lines |

## Practical Implications for Dieselbrook

The new middleware must reproduce these behaviours deliberately:

1. order selection and readiness rules
2. customer/account resolution
3. duplicate prevention
4. consultant reactivation side effects
5. full sales-order construction
6. kit decomposition
7. payment-receipt creation or equivalent AM posting path
8. `SOPortal` lifecycle participation
9. reverse shipment/status synchronization back to Shopify

Anything less will produce partial parity and hidden regressions.