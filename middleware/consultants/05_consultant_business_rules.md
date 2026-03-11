# Consultant Business Rules

## Purpose

This document captures the consultant-specific rules that materially affect parity.

## 1. Consultant Identity Rule

Current rule:

- consultant identity in Nop is tied to ERP consultant number via `username = arcust.ccustno`

Implication:

- consultant number is not incidental metadata; it is the cross-system identity anchor

## 2. Consultant Role Rule

Current rule:

- consultant-specific storefront behavior is controlled by the configured `ConsultantRoleId`

Implication:

- consultant experience is role-driven throughout the custom plugin

## 3. Profile Completeness Rule

Current rule:

- consultants can be blocked from checkout if consultant profile data is missing or `ProfileUpdated = false`

Implication:

- profile completion is a transaction gate, not just a data-quality nice-to-have

## 4. Activation Rule

Current rule:

- activation date is set on the first paid order in qualifying statuses if activation date is currently null

Implication:

- first-order logic affects consultant lifecycle state

## 5. Reactivation Rule

Current rule:

- order import can reactivate inactive consultants in AM using `sp_ws_reactivate`

Implication:

- consultant lifecycle changes can happen as part of commerce flows

## 6. Role-Specific Checkout Rule

Current rule:

- consultant checkout path differs from non-consultant path and can surface gifts, starter-kit exclusives, and special offers

Implication:

- consultant commerce is not just customer pricing with a role tag

## 7. Exclusive Item Rule

Current rule:

- consultant access to exclusive category/products depends on explicit allocation records
- some allocations are starter-linked
- some allocations are force-add / force-show style
- allocations can be restored on order cancellation

Implication:

- consultant-exclusive products are governed by stateful entitlement records

## 8. Award Rule

Current rule:

- awards are processed during consultant checkout completion and paid-order handling

Implication:

- consultant incentives are part of order lifecycle, not just separate marketing data

## 9. Address Change Audit Rule

Current rule:

- consultant billing/shipping changes are tracked in `ANQ_CustomerChanges`

Implication:

- consultant profile and address edits require an audit trail

## 10. Starter Kit Rule

Current rule:

- new consultants can receive starter allocations loaded from integration logic

Implication:

- onboarding and commerce entitlement state are linked

## 11. Registration Rule

Current rule:

- consultant registration is a dedicated workflow with sponsor and validation semantics, not a plain account registration

Implication:

- the onboarding flow must remain workflow-driven in the new stack

## 12. Reports Rule

Current rule:

- consultant users have a dedicated reports surface in the storefront plugin

Implication:

- consultant self-service includes business visibility, not only ordering

## 13. Commission Boundary Rule

Current rule:

- storefront consultant actions feed into ERP-driven commission and downline processes, but commission calculation itself remains in AccountMate

Implication:

- Dieselbrook must avoid pushing commission logic into Shopify by accident unless that becomes an explicit re-platforming project

## Recommendation

Each of these rules should become explicit, named middleware rules with:

- source inputs
- decision outputs
- audit trace
- operator-visible failure reasons

That is the only safe replacement for the current distributed FoxPro and plugin branching.