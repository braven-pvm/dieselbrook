# Phase-1 Replacement Boundary Summary

## Purpose

This document freezes the current best synthesis view of what phase 1 must replace, what can remain temporarily, and what should not be assumed mandatory yet.

## Phase-1 Must Replace

- storefront order ingest into ERP and reverse status propagation
- consultant identity and entitlement synchronization required for live operations
- effective consultant pricing projection and exclusive-access enforcement
- product, inventory, and image synchronization into Shopify
- parity-critical OTP, onboarding, and operational messaging flows
- materially used consultant and operational reporting access
- minimum operator visibility, logging, retry, and reconciliation capability

## Phase-1 May Remain Legacy-Side Or ERP-Side Temporarily

- ERP invoice generation and deeper financial posting
- MLM commission engine and deep compensation logic
- campaign master-data authority in AccountMate
- broad historical reporting catalogue not proven operationally necessary
- low-value or niche legacy admin UI surfaces
- transitional SQL-based support integrations only where explicitly justified

## Explicit Defer Or Challenge Candidates

- full back-office/admin UX parity
- low-value promotional widgets and niche programme mechanics
- full consultant portal redesign
- deep reporting-platform modernization beyond materially used reports
- full re-platforming of support/academy-linked SSO destinations without a business case

## Boundary Rules

- if a feature is parity-critical to live operations, keep it in phase 1
- if a feature only preserves a legacy UI pattern without clear business value, challenge it
- if a feature depends on unresolved Dieselbrook operating-model decisions, keep it analysis-linked but do not freeze it as a detailed requirement yet

## Main Decision Pressure Points

- `D-01` consultant/customer operating model
- `D-02` campaign pricing representation on Shopify
- `D-03` required admin/back-office parity
- `D-06` Shopify Plus/B2B availability and intent
- `D-09` new admin/interface expectation in phase 1
- `D-10` operational report set confirmation

## Current Safe Working Assumption

- phase 1 is a parity-first middleware and platform replacement, not a full business-process redesign
- Shopify replaces storefront behaviour
- Dieselbrook middleware replaces `NISource` and the custom Nop business layer where required for live operations
- AccountMate and related SQL estates remain core operational truth until later modernization decisions are made

## Exit Condition For Moving Into Requirement Shells

- open decision register has been reviewed against this boundary
- no remaining domain pack introduces a structural new scope area
- unresolved items are isolated as decisions or dependencies rather than hidden assumptions

## Evidence Base

- all Stage 2 and Stage 3 domain packs under `docs/analysis/`
- `docs/analysis/02_open_decisions_register.md`
- `docs/analysis/04_assumptions_and_dependencies_register.md`
- `docs/analysis/07_synthesis_execution_plan.md`
