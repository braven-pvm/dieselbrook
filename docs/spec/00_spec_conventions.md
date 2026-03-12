# Specification Phase Conventions

## Purpose

This document defines the authoring conventions, formats, and templates used across all specification documents in `docs/spec/`.

All spec documents must follow these conventions so that future agents, developers, and reviewers can navigate and compare content consistently.

---

## 1. Document Naming and Location

- Spec documents live under `docs/spec/domains/<domain-slug>/`
- Each domain folder contains a fixed set of documents:
  - `spec.md` — master specification
  - `state_machine.md` — lifecycle state model (where relevant)
  - `api_contract.md` — endpoint/interface definitions
  - `reconciliation_rules.md` — idempotency, retry, dead-letter, and operator guidance (where relevant)
  - `acceptance_criteria.md` — testable acceptance statements

---

## 2. Decision Dependency Notation

Every spec document must open with a decision dependency header:

```markdown
## Decision Dependencies

| Decision ID | Topic | Status | Impact If Still Open |
|---|---|---|---|
| D-01 | consultant operating model | open | blocks account model, entitlement design |
```

If a document is fully unblocked, the table should include one row stating "none — this spec is unblocked".

---

## 3. Field Mapping Format

Field maps follow this format in `spec.md`. Each source field is mapped to its target equivalent with any transformation rule noted.

```markdown
### Field Map: <Source System> → <Target System>

| Source Field | Source Table/Object | Target Field | Target Object | Transformation |
|---|---|---|---|---|
| `cCustNo` | `arcust` | `customer.metafield["cCustNo"]` | Shopify Customer | copy as string |
| `cName` | `arcust` | `customer.first_name` + `customer.last_name` | Shopify Customer | split on first space |
```

Rules:
- Use backtick-quoted identifiers for SQL columns and object paths.
- Transformation is "copy as-is" unless a rule applies.
- If the mapping is uncertain or decision-blocked, mark the row with `[BLOCKED: D-xx]`.
- If a legacy field has no target equivalent yet, note it as `[DEFER]` with a brief reason.

---

## 4. State Machine Format

State machines are written in `state_machine.md` using the following textual notation, with a table and a plaintext transition list.

### State table

```markdown
| State | Meaning | Entry Condition | Exit Conditions |
|---|---|---|---|
| `pending` | Order received from storefront, not yet sent to ERP | New order arrives in middleware | → `writing` on ERP write attempt |
```

### Transition list

```markdown
### Transitions

- `pending` → `writing` : middleware begins ERP write-back; guard: order passes validation
- `writing` → `complete` : ERP write confirmed; action: emit order-imported event
- `writing` → `failed` : ERP write rejected or timeout; action: place in retry queue
- `failed` → `writing` : operator or scheduler retries; guard: within retry window
- `failed` → `dead-letter` : retry limit exceeded; action: alert operator, halt automatic processing
```

---

## 5. API Contract Format

API contracts are written in `api_contract.md` using the following format.

```markdown
### <Method> <Path>

**Purpose**: one-sentence description

**Auth**: bearer token / internal / webhook secret

**Request**

| Field | Type | Required | Description |
|---|---|---|---|
| `shopify_order_id` | string | yes | Shopify order GID or numeric ID |

**Response — 200 OK**

| Field | Type | Description |
|---|---|---|
| `am_order_no` | string | AccountMate sales order number |

**Response — Error**

| Code | Condition |
|---|---|
| 400 | Missing required field |
| 409 | Duplicate — order already imported |
| 502 | ERP write failure |

**Idempotency key**: `shopify_order_id`
```

---

## 6. Idempotency Key Rules

Every integration action must name its idempotency key explicitly. Rules:

- The key must be stable across retries (same source event → same key every time).
- The key must be scoped to one logical business action (not a technical request ID).
- If a business event has no natural stable key from the source, middleware must assign one durably before the first write attempt.

Preferred key patterns by type:

| Integration class | Preferred key pattern |
|---|---|
| Order import | `shopify_order_id` |
| Order status sync | `am_order_no` + `status_event_timestamp` |
| Product sync | `icitem.cItemNo` |
| Inventory update | `icitem.cItemNo` + `warehouse_id` + `as_of_date` |
| Image sync | `iciimgUpdateNOP.id` or equivalent queue row identity |
| Consultant provisioning | `arcust.cCustNo` |
| Staff provisioning | `arcust.cCustNo` |
| OTP verification | `session_id` + `action_type` |

---

## 7. Acceptance Criteria Format

Acceptance criteria are written in `acceptance_criteria.md` as numbered, present-tense capability statements. Each criterion must be independently testable.

```markdown
### Capability: Order Intake

AC-ORD-001: Given a valid Shopify order, the system writes a corresponding sales order into AccountMate within [SLA].
AC-ORD-002: Given the same Shopify order is submitted twice, the system imports it exactly once and returns a 409 on the second attempt.
AC-ORD-003: Given an ERP write failure, the order is placed in the retry queue and the failure is visible to an operator.
```

Rules:
- Criteria IDs use `AC-<DOMAIN>-<SEQ>` format.
- Domain codes: `ORD`, `PROD`, `INV`, `IDN`, `COMM`, `RPT`, `ADMIN`.
- Each criterion references only one testable outcome.
- Criteria must not depend on unmade design decisions (do not write criteria for blocked capabilities).

---

## 8. Risk and Fragility Annotation

Where a spec section carries a known fragility from the analysis layer, annotate it:

```markdown
> **Risk**: current legacy behaviour X has fragility Y. Replacement must explicitly address Z.
```

---

## 9. Traceability

Every spec document must include an evidence base section at the end:

```markdown
## Evidence Base

| Artefact | Location |
|---|---|
| Legacy process | `NISource/syncorders.prg` |
| Domain pack | `docs/analysis/08_orders_domain_pack.md` |
| SQL contract | `docs/13_phase1_sql_contract.md` |
| Parity matrix row | `docs/11_nisource_process_parity_matrix.md` — `syncorders.prg` row |
```
