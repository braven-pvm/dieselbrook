# Specification Phase — Document Hierarchy and Control Layer

## Purpose

This document defines the planned structure of the specification phase.

The specification phase translates the analysis baseline (synthesis docs in `docs/analysis/`) into actionable, decision-grounded requirements: field maps, API contracts, state machines, reconciliation rules, and testable acceptance criteria per domain.

Nothing in this directory is final until the relevant open decisions in `docs/analysis/02_open_decisions_register.md` are resolved. Each spec document records which decision IDs it depends on and whether those are still open.

---

## Sequencing Rule

1. **Decision closure first** — resolve open decisions before writing specs for blocked domains.
2. **Orders first** — least blocked by open decisions, highest evidence density.
3. **Products/inventory and identity/SSO** — largely unblocked, proceed in parallel with orders.
4. **Consultants/MLM and pricing/campaigns** — gated on D-01 (consultant operating model) and D-06 (Shopify B2B strategy).
5. **Backlog decomposition last** — only after the domain spec it depends on is stable.

---

## Document Hierarchy

```
docs/spec/
│
├── README.md                              ← this file; index, status per doc, blocked/unblocked flag
├── 00_spec_conventions.md                 ← field naming, API contract format, state machine notation, acceptance criteria template
│
├── decisions/
│   └── 01_decision_closure_log.md         ← cross-ref to analysis/02_open_decisions_register.md;
│                                             records each decision received + downstream impact
│
├── domains/
│   │
│   ├── orders/
│   │   ├── spec.md                        ← master spec: scope, bounded context, field maps (AM → Shopify), error handling rules
│   │   ├── state_machine.md               ← order lifecycle states and transitions (trigger / guard / action notation)
│   │   ├── api_contract.md                ← endpoint definitions, request/response shapes, auth, versioning
│   │   ├── reconciliation_rules.md        ← idempotency keys, retry policy, dead-letter handling, operator playbook
│   │   └── acceptance_criteria.md         ← testable acceptance statements per capability
│   │
│   ├── consultants_mlm/                   ← BLOCKED: depends on D-01, D-07
│   │   ├── spec.md
│   │   ├── state_machine.md               ← consultant lifecycle (prospect → active → suspended → lapsed)
│   │   ├── api_contract.md
│   │   └── acceptance_criteria.md
│   │
│   ├── pricing_campaigns/                 ← BLOCKED: depends on D-01, D-02, D-06
│   │   ├── spec.md                        ← effective price resolution, campaign eligibility, soxitems contract
│   │   ├── api_contract.md
│   │   └── acceptance_criteria.md
│   │
│   ├── products_inventory/                ← UNBLOCKED
│   │   ├── spec.md                        ← full/delta/availability/image sync — schedules, triggers, field maps
│   │   ├── api_contract.md
│   │   └── acceptance_criteria.md
│   │
│   ├── communications_marketing/          ← PARTIALLY BLOCKED: OTP unblocked; Brevo/marketing depends on D-05
│   │   ├── spec.md                        ← OTP, onboarding comms, Brevo contact sync
│   │   ├── api_contract.md
│   │   └── acceptance_criteria.md
│   │
│   ├── reporting/                         ← PARTIALLY BLOCKED: depends on D-03, D-10
│   │   ├── spec.md                        ← metadata model, parameter binding, ACL, public vs admin surface
│   │   └── acceptance_criteria.md
│   │
│   ├── identity_sso/                      ← UNBLOCKED (core provisioning); SSO destinations depend on D-01
│   │   ├── spec.md                        ← consultant/staff provisioning, OTP verification, token issuance
│   │   ├── state_machine.md
│   │   ├── api_contract.md
│   │   └── acceptance_criteria.md
│   │
│   └── admin_backoffice/                  ← BLOCKED: depends on D-03, D-09
│       ├── spec.md                        ← campaign admin, report admin, operational settings
│       └── acceptance_criteria.md
│
└── backlog/
    ├── ws1_product_sync.md                ← stories decomposed from products_inventory spec
    ├── ws2_order_flow.md                  ← stories decomposed from orders spec
    ├── ws3_consultant_identity.md         ← stories decomposed from consultants + identity specs
    ├── ws4_pricing_campaigns.md           ← stories from pricing/campaigns spec
    ├── ws5_communications.md              ← stories from communications spec
    ├── ws6_reporting.md                   ← stories from reporting spec
    └── ws7_admin_ops.md                   ← stories from admin/back-office + cross-cuts
```

---

## Document Status

| Document | Status | Blocking Decisions |
|---|---|---|
| `00_spec_conventions.md` | not started | none |
| `decisions/01_decision_closure_log.md` | not started | none |
| `domains/orders/spec.md` | not started | none |
| `domains/orders/state_machine.md` | not started | none |
| `domains/orders/api_contract.md` | not started | none |
| `domains/orders/reconciliation_rules.md` | not started | none |
| `domains/orders/acceptance_criteria.md` | not started | none |
| `domains/products_inventory/spec.md` | not started | none |
| `domains/products_inventory/api_contract.md` | not started | none |
| `domains/products_inventory/acceptance_criteria.md` | not started | none |
| `domains/identity_sso/spec.md` | not started | none (core); D-01 for SSO destinations |
| `domains/identity_sso/state_machine.md` | not started | none |
| `domains/identity_sso/api_contract.md` | not started | none |
| `domains/identity_sso/acceptance_criteria.md` | not started | none |
| `domains/communications_marketing/spec.md` | not started | D-05 (Brevo boundary) |
| `domains/communications_marketing/api_contract.md` | not started | D-05 |
| `domains/communications_marketing/acceptance_criteria.md` | not started | D-05 |
| `domains/consultants_mlm/spec.md` | blocked | D-01, D-07 |
| `domains/consultants_mlm/state_machine.md` | blocked | D-01 |
| `domains/consultants_mlm/api_contract.md` | blocked | D-01 |
| `domains/consultants_mlm/acceptance_criteria.md` | blocked | D-01, D-07 |
| `domains/pricing_campaigns/spec.md` | blocked | D-01, D-02, D-06 |
| `domains/pricing_campaigns/api_contract.md` | blocked | D-01, D-02, D-06 |
| `domains/pricing_campaigns/acceptance_criteria.md` | blocked | D-01, D-02, D-06 |
| `domains/reporting/spec.md` | blocked | D-03, D-10 |
| `domains/reporting/acceptance_criteria.md` | blocked | D-10 |
| `domains/admin_backoffice/spec.md` | blocked | D-03, D-09 |
| `domains/admin_backoffice/acceptance_criteria.md` | blocked | D-03, D-09 |
| `backlog/ws1_product_sync.md` | blocked | products_inventory spec must stabilise first |
| `backlog/ws2_order_flow.md` | blocked | orders spec must stabilise first |
| `backlog/ws3_consultant_identity.md` | blocked | D-01; identity spec must stabilise first |
| `backlog/ws4_pricing_campaigns.md` | blocked | D-01, D-02, D-06 |
| `backlog/ws5_communications.md` | blocked | D-05 |
| `backlog/ws6_reporting.md` | blocked | D-10 |
| `backlog/ws7_admin_ops.md` | blocked | D-03, D-09 |

---

## Relationship to Analysis Layer

All spec documents trace back to the synthesis layer in `docs/analysis/`:

- Domain scope → `docs/analysis/08_*` through `docs/analysis/15_*` (domain packs)
- Cross-cutting requirements → `docs/analysis/16_*` through `docs/analysis/18_*`
- Replacement boundary → `docs/analysis/19_phase1_replacement_boundary_summary.md`
- Open decisions → `docs/analysis/02_open_decisions_register.md`
- Workstream decomposition → `docs/analysis/03_workstream_decomposition.md`
