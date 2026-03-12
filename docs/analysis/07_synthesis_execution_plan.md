# Synthesis Execution Plan

## Purpose

This document is the explicit working plan for completing the synthesis phase.

It turns the analysis framework into an ordered execution path with concrete outputs and exit criteria.

## Plan Rule

This is not an implementation delivery plan.

It is the plan for finishing current-state synthesis well enough that later requirements and solution design can proceed without falling back into broad discovery.

## Completion Condition

Synthesis is complete when all of the following are true:

- the major replacement domains each have a canonical analysis pack
- the open decisions materially affecting scope are isolated and referenced
- the assumptions and external dependencies are explicit and current
- the phase-1 replacement boundary is clear enough to support requirement drafting
- remaining unknowns are narrow follow-up items, not structural gaps

## Execution Stages

### Stage 1: Lock The Control Layer

Goal:

- treat the analysis folder as the controlling synthesis layer

Outputs:

- baseline, decisions, workstreams, assumptions, template, onboarding, and this plan remain current

Exit criteria:

- new analysis work references this folder instead of creating more unstructured top-level synthesis notes

### Stage 2: Build The First Canonical Domain Packs

Goal:

- document the three highest-risk domains first

Outputs:

- orders domain pack
- consultants and MLM domain pack
- pricing, campaigns, and exclusives domain pack

Required content in each pack:

- business purpose and operational importance
- current-state components and source anchors
- system interactions and data touchpoints
- replacement boundary
- phase-1 expectations versus later-phase items
- key risks and parity traps
- linked decision IDs and assumption IDs

Exit criteria:

- the three packs are complete enough to explain why each domain matters, what must be replaced, and what remains decision-dependent

### Stage 3: Build The Remaining Domain Packs

Goal:

- complete the full domain-level analysis coverage

Outputs:

- products and inventory domain pack
- communications and marketing domain pack
- reporting domain pack
- identity and SSO domain pack
- admin and back-office tooling domain pack

Exit criteria:

- every primary workstream in the decomposition document has either a canonical domain pack or an explicit reason for deferral

### Stage 4: Reconcile Cross-Cutting Concerns

Goal:

- prevent the domain packs from hiding shared platform risk

Outputs:

- cross-cutting synthesis notes or sections covering:
  - platform runtime and hosting
  - SQL contract and data access
  - auditability, retries, idempotency, and reconciliation

Exit criteria:

- cross-domain dependencies are explicit and not buried inside one workstream only

### Stage 5: Tighten The Decision And Dependency Registers

Goal:

- make sure unresolved business choices are isolated from proven current-state facts

Outputs:

- updated open decisions register
- updated assumptions and dependencies register
- explicit references from each domain pack to the decisions and assumptions it depends on

Exit criteria:

- no major domain pack contains silent assumptions that are not represented in the control registers

### Stage 6: Freeze The Phase-1 Replacement Boundary

Goal:

- convert synthesis into a stable pre-requirements baseline

Outputs:

- a concise phase-1 replacement boundary summary
- clear notes on defer, retire, preserve, or later-phase items
- list of items still waiting on Dieselbrook or Annique decisions

Exit criteria:

- the programme can move into requirement shells without re-opening broad current-state discovery

## Recommended Working Order

1. Orders
2. Consultants and MLM
3. Pricing, campaigns, and exclusives
4. Products and inventory
5. Communications and marketing
6. Reporting
7. Identity and SSO
8. Admin and back-office tooling
9. Cross-cutting reconciliation
10. Phase-1 boundary freeze

## Immediate Next Actions

1. Create the orders domain pack.
2. Create the consultants and MLM domain pack.
3. Create the pricing, campaigns, and exclusives domain pack.
4. Update the decision and assumptions registers with any new dependencies exposed by those packs.
5. Only then move to the remaining domain packs.

## Guardrails

- do not restart generic archaeology unless a pack exposes a blocking evidence gap
- do not write final Shopify requirements while high-impact Dieselbrook decisions remain open
- do not let low-value legacy UI parity expand the phase-1 boundary without explicit business confirmation
- do not treat source incompleteness as a reason to pause synthesis unless it changes scope materially