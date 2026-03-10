# Order Business Rules

## Purpose

This document captures the order rules that materially affect behavioural parity.

These are the rules Dieselbrook must either:

- replicate exactly
- replace deliberately with signed-off changes
- or quarantine as unsupported until the business decides

## 1. Account Resolution Rules

Current source-derived mapping:

| Condition | Result |
|---|---|
| customer role `Annique Consultant` | use consultant username as `ccustno` |
| customer role `AnniqueStaff` | use `STAFF1` |
| customer role `AnniqueExco` | use `STAFF1` |
| customer role `Bounty` | use `ASHOP1`, mark inter-company path |
| customer role `Client` | use `ASHOP2` |
| no explicit match | fall back to `ASHOP1` |

Implication:

- the same Shopify/NOP order structure can post into different AM accounts depending on role context

## 2. Reactivation Rule

If the resolved AM customer is inactive:

- the legacy middleware attempts `sp_ws_reactivate`

Observed effects include:

- changing customer status to active
- resetting sponsor/upline context
- adjusting starter date logic
- prepending notes
- logging to `NopIntegration..Brevolog`

Implication:

- order import can modify consultant lifecycle state
- this is a business operation, not just an integration detail

## 3. Duplicate Rule

Current duplicate detection:

- existing `sosord` where `cPono = source order ID`
- and `ccustno = resolved AM customer`

Target rule:

- preserve this AM-side check
- add middleware-side idempotency so duplicate prevention does not depend on AM lookup alone

## 4. Order Number Rule

Current behaviour:

- `cSono` is built from the web order number
- `cPono` is populated with the source order ID

This distinction must survive the Shopify migration.

## 5. Shipping Rules

Current shipping logic is rule-based, not a direct pass-through.

| Condition | Result |
|---|---|
| collect or staff order | `COLLECT` |
| test account | `BERCO` |
| pickup-point attribute branch | may become `POSTNET` or `SKYNET` |
| default | `COURIER` |

Implication:

- Shopify shipping lines alone are not enough; the middleware may need extra metadata to derive AM shipping behaviour correctly

## 6. Payment Rules

Current order import creates `arcash` entries for non-staff orders.

Observed payment-code mapping:

| Payment type | AM payment code |
|---|---|
| default card / PayU | `PAYUC` |
| EFT | `PAYUE` |
| Payflex | `PAYFLEX` |
| gift card | `AFF` |

Implication:

- payment-state mapping is part of order import scope
- Dieselbrook must confirm whether this remains the correct target process in the new architecture

## 7. Staff Order Pricing Rule

If `ccustno = STAFF1`:

- line prices are set from item cost
- revenue code becomes `STAFF-01`
- some downstream logic paths differ from normal customer orders

Implication:

- staff/executive order handling is not just an account mapping; it also changes commercial treatment

## 8. Kit Rule

If an AM item is marked as a kit:

- middleware must resolve components
- `soskit` rows must be created

Implication:

- one Shopify line item may map to both one `sostrs` line and multiple `soskit` component rows

## 9. Hold Rule

Current source logic sets `lHold = 1` when:

- the AM order number begins with `Z`
- or the customer is `TEST01`

Implication:

- some orders intentionally enter AM in a held state

## 10. Warehouse / Portal Rule

Every successfully imported order is inserted into `SOPortal` with:

- blank initial status
- no printed timestamp

Status progression is then interpreted as:

- blank -> pending
- `P` -> picking
- `S` -> shipped

Implication:

- `SOPortal` is required for behavioural parity with current fulfillment visibility

## 11. Side-Feature Dependency Rule

Orders and invoices are consumed by downstream features beyond normal fulfillment.

Confirmed examples:

- MLM/rebate processes depend on invoice records
- ticket / booking reconciliation uses `aritrs` and `sostrs`

Implication:

- any replacement that only gets the order into Shopify and ignores AM invoice lineage will break adjacent processes

## 12. Rules Still Requiring Deeper Discovery

These areas still need more source and live-behaviour analysis before final implementation:

- order cancellation before import
- order cancellation after import
- refund to credit-note flow
- partial shipment behaviour
- partial invoice behaviour
- backorders and insufficient stock logic
- manual warehouse exceptions
- order edits after initial import

## Recommendation

For the Dieselbrook build, business rules should be implemented as explicit, testable rule modules with:

- named rule identifiers
- inputs and outputs captured in logs
- rule-decision traces stored per order
- operator-visible failure reasons

That is the only sustainable way to replace the current hidden FoxPro branching logic.