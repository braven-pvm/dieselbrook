# Data Access And SQL Contract Cross-Cut

## Purpose

This document consolidates the SQL-access stance that cuts across all synthesis domains.

## Core Principle

Phase 1 should preserve business parity while minimizing direct SQL write scope.

New middleware concerns must live in Dieselbrook-owned state, not in legacy support tables.

## Current Working SQL Boundary

- `amanniquelive`: read plus controlled write for parity-critical flows
- `compplanLive`: read-first, procedure-call only if unavoidable
- `compsys`: avoid direct dependency unless transitional messaging compatibility is explicitly chosen
- `NopIntegration`: read-first, narrow write only if compatibility needs are proven

## Cross-Domain Implications

### Orders

- direct writes are justified to the proven order-import path
- invoice generation remains an ERP-side downstream concern

### Consultants and identity

- lookup and lifecycle reads are required
- lifecycle mutations should stay tightly constrained and explicit

### Pricing and products

- campaign, item, stock, and exclusive-access data are primarily read-side dependencies in phase 1

### Reporting and admin

- `NopReports`, `NopSSO`, and support settings/log tables should be treated as reference estates unless a concrete operational write need is proven

## Contract Rules

- object-level access over broad database rights
- audited, named write actions only
- no reuse of legacy tables for idempotency, retry, dead-letter, webhook receipt, or operator note state
- preserve least privilege as a design constraint, not a later hardening task

## What Should Be Treated As Validation-Required

- direct writes to `SOPortal`
- direct writes to `arcash`
- any consultant lifecycle writes beyond `sp_ws_reactivate`
- any continued writes into `NopIntegration` or `compsys`

## Phase-1 Recommendation

- carry forward the current SQL contract as the working minimum until a domain proves otherwise
- require every new proposed SQL write to be justified by a named business action and trace requirement

## Primary Dependencies And Decisions

- `D-05` transitional use of `compsys`
- `D-10` any reporting/admin parity that would force `NopIntegration` writes
- `X-DEP-03` continued staging SQL access

## Evidence Base

- `docs/13_phase1_sql_contract.md`
- `docs/10_accessible_estate_and_replacement_surface.md`
- `docs/11_nisource_process_parity_matrix.md`
