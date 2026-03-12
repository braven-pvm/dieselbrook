# Auditability, Idempotency, And Reconciliation Cross-Cut

## Purpose

This document consolidates the resilience and operational-control concerns that appear repeatedly across the domain packs.

## Current Working Position

- the legacy estate relies heavily on duplicate checks, direct side effects, and weak operator visibility
- high-risk phase-1 parity depends on making auditability, idempotency, retries, and reconciliation explicit first-class concerns

## Why This Is Cross-Cutting

- orders require durable duplicate protection and post-write reconciliation
- pricing and inventory require drift detection and boundary sweeps
- communications require visible delivery and failure state
- reporting and admin tooling need operator-facing diagnostics rather than opaque failure modes

## Confirmed Legacy Weaknesses

- current order duplicate protection is necessary but not sufficient as a future-state idempotency model
- retry behaviour in the legacy runtime is limited and tightly coupled to side effects
- `wwrequestlog` and related support tables are historical references, not a complete operational observability model
- Brevo logging is not a general-purpose API audit system

## Required Future-State Controls

### Idempotency

- middleware-owned idempotency ledger for inbound and outbound workflows
- named idempotency keys by business action, not only by technical request

### Auditability

- structured logs with correlation IDs per workflow
- clear business-event and integration-event audit records
- traceability from storefront event to middleware action to ERP outcome

### Retry and failure isolation

- explicit retry policies per integration class
- dead-letter handling where replay is possible
- operator-visible failure state rather than silent queue accumulation

### Reconciliation

- nightly or scheduled reconciliation for orders, pricing, inventory, and high-risk support flows
- domain-specific comparison outputs rather than generic success/failure counts only

## Phase-1 Recommendation

- treat auditability, idempotency, and reconciliation as required platform capabilities, not optional operational polish
- do not sign off any high-risk domain design unless it names its idempotency key, audit trail, retry behaviour, and reconciliation method

## Primary Dependencies And Decisions

- no new business decision is needed to justify these controls; they are already implied by the legacy fragility and the target architecture

## Evidence Base

- `docs/06_orders_deep_dive.md`
- `docs/11_nisource_process_parity_matrix.md`
- `docs/05_delivery_architecture_dieselbrook.md`
- `docs/annique-discovery.1.0.md`
