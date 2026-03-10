# Target Order Middleware Design

## Objective

Replace the current FoxPro order integration with a Dieselbrook-owned middleware service that preserves business behaviour while improving:

- determinism
- observability
- recoverability
- auditability
- maintainability

## Design Principles

1. Shopify is event-driven. AccountMate is stateful and side-effect-heavy.
2. Canonical order contracts must exist inside middleware before any AM write occurs.
3. AM writes must be idempotent and fully auditable.
4. Financial and warehouse side effects must be explicit, not hidden in ad hoc script branches.
5. Reconciliation must be a first-class process, not a support-only activity.

## Recommended High-Level Flow

```text
Shopify order event
    -> webhook ingestion
    -> raw event log
    -> canonical order builder
    -> validation + rule engine
    -> customer/account resolution
    -> idempotency check
    -> AM transaction writer
    -> post-write verification
    -> status/fulfillment sync loop
    -> reconciliation and alerting
```

## Recommended Services

| Service | Responsibility |
|---|---|
| Webhook ingress | Receive Shopify order, fulfillment, cancellation, refund, and customer events |
| Raw event store | Persist original webhook payloads for replay and audit |
| Canonical order builder | Convert Shopify payloads into a stable internal order contract |
| Rule engine | Apply account mapping, shipping, payment, and exception rules |
| AM writer | Write `sosord`, `sostrs`, `soskit`, `SOPortal`, and payment-side artefacts in the correct sequence |
| Status synchronizer | Read AM shipment/order state and update Shopify status and tracking |
| Reconciliation service | Detect drift between Shopify, middleware, and AM |
| Operator tooling | Show retries, failures, duplicates, and manual actions |

## Required Middleware Persistence

At minimum, the new middleware should keep its own ledger for:

- inbound Shopify events
- canonical orders
- idempotency keys
- AccountMate write attempts
- per-order status transitions
- reconciliation results
- manual override or replay actions

Without this, supportability will collapse back into database forensics.

## Proposed Write Pattern into AccountMate

The new middleware should preserve the broad write sequence already proven in the legacy implementation:

1. resolve target AM customer and account context
2. verify order eligibility
3. verify idempotency
4. reactivate consultant if rules require it
5. create AM order header
6. create AM order lines
7. create kit rows where applicable
8. create payment/receipt artefacts if still required in target process
9. create `SOPortal` row
10. commit transaction
11. verify result and store cross-reference data

## Idempotency Design

Current legacy idempotency is too thin.

The target middleware should use both:

- business key: Shopify Order ID
- AM key: resulting `cSono` and `cPono`

Recommended middleware keys:

| Key | Use |
|---|---|
| `shopify_order_id` | primary business identity |
| `shopify_order_name` | human trace and support lookup |
| `am_csono` | ERP order identity |
| `am_cpono` | external-order identity in AM |
| `sync_attempt_id` | per-attempt execution trace |
| `payload_hash` | duplicate and mutation detection |

## Canonical Validation Stages

The middleware should reject or quarantine orders before AM write when:

- customer/account mapping cannot be resolved
- line items have no valid AM SKU mapping
- required shipping information is incomplete
- payment state is not eligible for write-back
- pricing totals fail integrity checks
- order is a duplicate or conflicting replay

## Error Handling Model

Use explicit categories:

| Error class | Example | Action |
|---|---|---|
| Retryable infrastructure | AM connection timeout | retry with backoff |
| Retryable data-race | order not fully materialized yet in Shopify | short delayed retry |
| Non-retryable mapping | no AM SKU or no customer mapping | operator queue |
| Non-retryable business rule | inactive customer cannot be reactivated | manual review |
| Drift / reconciliation | Shopify says paid, AM not created | alert and reconcile |

## Status Synchronization Design

The target design should not depend on opaque wake-up procedures.

Recommended approach:

1. middleware polls or listens for AM shipment-status changes
2. middleware maps AM operational state to Shopify fulfillment/customer-visible state
3. middleware updates tracking details, status, and notes in Shopify
4. middleware stores the last synchronized state and timestamp

## Recommended Operational Controls

The new order middleware should support:

- replay one order
- replay a date range
- force status refresh
- quarantine and release
- view raw source payloads
- view AM SQL writes by order
- compare Shopify totals to AM totals

## Open Design Questions

These still need deeper source confirmation:

- exact cancellation handling path
- exact refund/credit-note path
- partial shipment handling rules
- backorder and stock-shortage behaviour
- event-ticket / side-feature downstream requirements
- whether `arcash` creation must remain in middleware or move to another AM-compatible process

## Recommended Implementation Bias

For orders specifically, favour:

- explicit state machines over procedural branching
- background job processing over synchronous long-running webhook logic
- strong persistence and replay controls over speed-first shortcuts
- AM-adjacent deployment if connectivity or latency is uncertain