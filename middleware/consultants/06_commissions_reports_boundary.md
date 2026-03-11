# Commissions And Reports Boundary

## Purpose

This document answers the critical consultant-domain question:

What belongs in Shopify, what belongs in middleware, and what should remain in AccountMate for commissions, hierarchy, and reports?

## Short Answer

Commissions and hierarchy should remain ERP-native unless Dieselbrook is explicitly commissioned to replace the MLM engine.

However, if Dieselbrook is replacing `NISource`, then the middleware-facing MLM integration surface is in scope.

Shopify should expose consultant-facing outcomes, not become the rebate engine.

## 1. What The Current System Tells Us

Confirmed current-state signals:

- `sp_ct_Rebates` calculates multi-level rebates
- `sp_ct_downlinebuild` rebuilds consultant hierarchy/downline structures
- `genreb` is the large rebate ledger
- `CTDownline` / `CTDownlineh` and related views support downline logic and reporting
- consultant-facing report routes exist in the Nop customization layer

This means:

- the commission engine is not in NopCommerce
- NopCommerce provides consultant-facing access and presentation, not the core rebate algorithm
- the FoxPro middleware and reporting layer do directly touch the MLM-side database surface

## 1A. Scope Correction For `NISource` Replacement

There is an important distinction between:

- replacing the legacy FoxPro middleware and report layer
- replacing the underlying SQL commission engine itself

If Dieselbrook is replacing `NISource`, then these MLM-adjacent capabilities are in scope:

- sponsor/downline validation interfaces
- consultant reactivation integration
- consultant-facing reporting reads backed by MLM-side data
- any middleware API endpoints that currently query `compplanlive`

What is not automatically in scope for phase 1 is a full rewrite of:

- `sp_ct_Rebates`
- `sp_ct_downlinebuild`
- the underlying rebate ledger model

## 2. Recommended Boundary

| Concern | Best home |
|---|---|
| consultant login and account UX | Shopify |
| consultant pricing and product access | Shopify + middleware |
| consultant registration intake | middleware |
| consultant activation and identity sync | middleware + AccountMate |
| sponsor/upline source of truth | AccountMate |
| downline rebuild logic | AccountMate |
| rebate calculation | AccountMate |
| rebate ledger history | AccountMate |
| consultant-facing reports | middleware-backed app or report surface |

## 3. What Shopify Should Show

Shopify can and should show consultant-facing information such as:

- whether the customer is a consultant
- basic consultant status and activation state
- exclusive-item or starter entitlement availability
- links to consultant reports or dashboards
- simplified KPI views if exposed from middleware

Shopify should not be the primary calculation layer for:

- upline/downline rebate distribution
- payout ledger generation
- monthly hierarchy rebuilds
- historical commission audit data

## 4. Recommended Report Strategy

Safer initial strategy:

- leave report definitions and heavy data sourcing outside Shopify
- expose consultant report list and details through custom app or middleware-backed UI
- use Shopify only as a launch point into the consultant report experience where useful

## 5. Recommended Commission Strategy

Safer initial strategy:

- preserve current ERP commission generation processes
- make sure Shopify-originated orders still land in AM in a way that preserves invoice lineage and consultant attribution
- let middleware bridge the order/customer identity needed by the ERP commission engine
- replace FoxPro middleware/report access to MLM-side data with Dieselbrook-owned APIs, jobs, and integration services

## 6. Critical Risk If This Boundary Is Ignored

If Dieselbrook treats consultants as only Shopify customers and ignores ERP commission boundaries, the likely failures are:

- broken sponsor/upline attribution
- incorrect rebate calculations
- missing or inconsistent consultant reports
- loss of commission audit trail
- disagreement between storefront status and ERP consultant state

## 7. Recommended First-Phase Outcome

Phase 1 should aim for:

- consultant experience in Shopify
- consultant state and entitlement sync via middleware
- continued commission and hierarchy truth in AccountMate
- consultant-facing reporting re-exposed through a controlled middleware/report layer
- replacement of `NISource` touchpoints into `compplanlive` without forcing a premature MLM engine rewrite

That gives Dieselbrook a migration path without taking on a stealth MLM re-platforming project.