# Workstream Decomposition

## Purpose

This document breaks the programme into analysis and replacement workstreams that are stable enough to organize the documentation effort.

It is not a delivery schedule.

It is a structure map.

## Workstream Principles

- organize by business capability and replacement boundary, not by raw source folder alone
- keep cross-cutting platform concerns visible
- allow Dieselbrook target-state decisions to reshape implementation later without collapsing the analysis model

## Primary Workstreams

### WS-01 Orders And Fulfillment

Scope:

- order intake
- ERP write-back
- status propagation
- shipment/tracking behavior
- invoice and operational dependencies such as `SOPortal`

Main source anchors:

- `syncorders.prg`
- `syncorderstatus.prg`
- order staging inventory docs

### WS-02 Consultants And MLM

Scope:

- consultant identity and lifecycle
- sponsor and downline validation
- MLM read dependencies
- consultant registration and onboarding
- consultant support communications

Main source anchors:

- `syncconsultant.prg`
- `nopnewregistrations.prg`
- `compplanLive` and `compsys` docs

### WS-03 Products And Inventory

Scope:

- ERP item data
- product sync
- inventory sync
- image and category/manufacturer mapping concerns

Main source anchors:

- `syncproducts.prg`
- product sync findings
- Nop plugin product overrides

### WS-04 Pricing, Campaigns, And Exclusives

Scope:

- campaign pricing
- effective consultant pricing
- exclusives and eligibility
- campaign admin/back-office behavior
- timed pricing activation and expiry

Main source anchors:

- campaign data docs
- campaign module drop
- Nop price/discount override services

### WS-05 Communications, Marketing, And OTP

Scope:

- welcome mail
- SMS/OTP
- Brevo
- related operational notifications and support logging

Main source anchors:

- `communicationsapi.prg`
- `brevoprocess.prg`
- `BrevoLog`
- `MAILMESSAGE`

### WS-06 Reporting And Operational Visibility

Scope:

- consultant and operational reports
- report metadata
- reporting boundaries versus BI replacement
- operator visibility and reconciliation

Main source anchors:

- `reports.prg`
- `NopReports`
- reporting docs in consultant/plugin analysis

### WS-07 Identity, SSO, And Access Control

Scope:

- SSO behavior
- login/session behavior
- role and identity mapping
- consultant/staff storefront access behavior

Main source anchors:

- `ssoclass.prg`
- Nop auth/session overrides

### WS-08 Admin And Back-Office Tooling

Scope:

- legacy internal pages and admin surfaces
- campaign management UI
- any other web-facing operational tools
- decisions on replace versus retire versus defer

Main source anchors:

- `NISource/campaign/`
- admin-oriented Nop plugin surfaces

## Cross-Cutting Workstreams

### X-01 Platform Runtime And Hosting

- middleware host/runtime
- connectivity model
- secrets/config management
- operational deployment boundary

### X-02 Data Access And SQL Contract

- SQL permissions
- read/write boundaries
- procedure use
- legacy support database access

### X-03 Auditability, Idempotency, And Reconciliation

- operator tooling
- retries
- error handling
- parity verification
- change tracking and replay safety

## Documentation Use

Every later domain pack or spec should clearly identify which workstream it belongs to.

If a document spans multiple workstreams, it should name one primary owner workstream and list the cross-workstream dependencies explicitly.