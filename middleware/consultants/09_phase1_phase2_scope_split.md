# Phase 1 And Phase 2 Scope Split

## Purpose

This document separates:

- what Dieselbrook must replace in phase 1 to retire `NISource`
- what Dieselbrook may optionally re-platform later as a phase 2 MLM modernization effort

## Core Rule

Phase 1 is about replacing the legacy middleware and preserving business continuity.

Phase 2 is where Dieselbrook may choose to replace deeper ERP-native commission and hierarchy engines if that becomes commercially justified.

## Phase 1 In Scope

Phase 1 should include all middleware-facing dependencies currently handled by FoxPro.

### Consultant and MLM integration surface

- consultant account and profile sync
- consultant role and lifecycle synchronization
- sponsor/downline validation API behavior
- order-triggered reactivation orchestration
- consultant-facing report data access
- middleware-owned communications now queued through `compsys`
- Brevo-related middleware behavior now handled through `NopIntegration..Brevolog`

### ERP-side invocations that remain authoritative

- calling `sp_ws_reactivate` if retained initially
- preserving invoice creation paths that commission logic depends on
- reading hierarchy / commission outputs from `compplanlive`
- consuming ERP-native consultant status and sponsor truth

### Phase 1 outcome

At the end of phase 1:

- `NISource` is replaced
- Shopify and Dieselbrook middleware own the consultant-facing experience
- ERP SQL still owns the commission and hierarchy truth where required

## Phase 1 Out Of Scope By Default

These items should not be assumed part of phase 1 unless explicitly commissioned:

- full rewrite of `sp_ct_Rebates`
- full rewrite of `sp_ct_downlinebuild`
- migration of MLM ledger ownership out of SQL Server
- replacement of all historical commission tables and report models
- complete redefinition of rank, payout, or compensation-plan formulas

## Phase 2 Candidate Scope

Phase 2 can evaluate deeper re-platforming such as:

- replacing SQL-native hierarchy functions with middleware-owned hierarchy services
- replacing SQL-native commission calculation with a Dieselbrook-owned commission engine
- creating an app-owned consultant statement and payout ledger
- modernizing report generation away from direct SQL/report procedure dependence
- replacing ERP-side queued mail infrastructure where it still supports middleware-owned flows

## Recommended Decision Matrix

| Capability | Phase 1 | Phase 2 |
|---|---|---|
| retire FoxPro middleware | yes | no |
| preserve consultant sync parity | yes | no |
| preserve reactivation parity | yes | no |
| preserve consultant report access | yes | no |
| preserve invoice-to-commission dependency | yes | no |
| rewrite `sp_ct_Rebates` | no | optional |
| rewrite `sp_ct_downlinebuild` | no | optional |
| move MLM ledger outside SQL | no | optional |
| redesign compensation model | no | optional |

## Delivery Risk If This Split Is Ignored

If Dieselbrook mixes phase 1 middleware replacement with an implicit MLM re-platforming effort, the likely outcomes are:

- hidden scope growth
- delayed parity delivery
- disputes over whether Dieselbrook was supposed to replace the commission engine
- fragile cutover because too many critical systems change at once

## Recommended Commercial Framing

The clean commercial and delivery framing is:

- Phase 1: replace `NISource`, preserve ERP-native MLM truth, expose required outcomes through Dieselbrook middleware
- Phase 2: optional modernization of the MLM engine, report stack, and communications infrastructure

That gives Dieselbrook a defensible scope boundary without understating the real middleware work.