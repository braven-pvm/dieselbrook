# Consultant Phase 1 Implementation Backlog

## Purpose

This document turns the staging-confirmed consultant, MLM, communication, and middleware support objects into a practical phase-1 build backlog for Dieselbrook.

It is intentionally biased toward parity replacement of the current FoxPro middleware surface, not a full redesign of the underlying commission engine.

## Working Rule

Phase 1 replaces `NISource` behavior and exposed integration contracts first.

Phase 1 does not automatically replace the full SQL-native MLM calculation engine unless a later decision expands scope.

## Confirmed Staging Evidence

Direct staging inspection confirmed the following phase-1 relevant objects are real, accessible, and populated:

| Database | Object | Row count | Why it matters |
|---|---|---:|---|
| `compplanLive` | `CTcomph` | 8,623,539 | commission and history reads |
| `compplanLive` | `CTcomp` | 11,574 | current commission data |
| `compplanLive` | `CTconsv` | 1,779,211 | consultant sales and grouping inputs |
| `compplanLive` | `CTRunData` | 3,702,636 | run-cycle and recruit metrics |
| `compplanLive` | `CTstatement` | 52,511 | current statement/report data |
| `compplanLive` | `CTstatementh` | 4,481,557 | historical statement/report data |
| `compplanLive` | `deactivations` | 35,702 | lifecycle and reactivation handling |
| `compplanLive` | `CTdownlineh` | 1,491,507 | historical sponsor/downline reads |
| `compsys` | `MAILMESSAGE` | 752,963 | queued outbound mail |
| `NopIntegration` | `BrevoLog` | 124,726 | Brevo audit/logging dependency |
| `NopIntegration` | `NopReports` | 52 | report catalog / middleware support |

## Backlog

| Priority | Work item | Current dependency | Dieselbrook target |
|---|---|---|---|
| P0 | Build consultant SQL access boundary | direct reads from `compplanLive`, `amanniquelive`, `compsys`, `NopIntegration` | `ConsultantDataAccessGateway` with least-privilege credentials and typed query surface |
| P0 | Replace sponsor and downline validation | `apiprocess.prg` plus `compplanLive` hierarchy reads/functions | `ConsultantHierarchyAndMLMAccessService` |
| P0 | Replace consultant sync orchestration | `syncconsultant.prg`, `nopnewregistrations.prg`, `arcust`, consultant role logic | `ConsultantSyncService` and `ConsultantLifecycleService` |
| P0 | Replace order-driven consultant reactivation | `syncorders.prg` calling `sp_ws_reactivate` and reading lifecycle state | `OrderWritebackService` plus `ConsultantLifecycleService` |
| P0 | Replace consultant report endpoints | `reports.prg`, `apiprocess.prg`, `NopIntegration..NopReports`, `compplanLive` report tables | `ConsultantReportingDataService` with explicit report contracts |
| P0 | Replace onboarding and welcome communication flow | `syncconsultant.prg`, `amdata.prg`, `compsys..MAILMESSAGE`, `sp_Sendmail` | `ConsultantOnboardingCommunicationService` with auditable outbound delivery |
| P1 | Replace Brevo consultant sync support | `brevoprocess.prg`, `BrevoLog` | `BrevoSyncService` plus middleware-owned delivery log |
| P1 | Preserve Namibia sibling-path design | `amanniquenam`, `compplanNam` sibling estate | tenant-aware environment abstraction rather than SA-only hard-coding |
| P1 | Add parity reconciliation tooling | today largely manual / implicit | consultant lifecycle, sponsor, and report parity dashboards |
| P1 | Add idempotent retry and failure handling | current FoxPro process routing and DB side effects | durable job execution, retry policy, and dead-letter handling |

## Recommended Delivery Slice

### Slice 1: access and read parity

- define typed read models for sponsor lookup, downline lookup, statement history, and current consultant status
- move those reads behind one middleware access layer
- validate against staging before building Shopify-facing flows

### Slice 2: lifecycle and registration parity

- replace new consultant registration intake
- replace consultant sync from ERP to commerce model
- replace first-order reactivation side effects and audit them explicitly

### Slice 3: report and communication parity

- replace consultant report APIs with explicit contracts
- replace `MAILMESSAGE` writes with a middleware-owned messaging path unless ERP-owned dispatch must be preserved for a specific reason
- preserve equivalent delivery auditing

### Slice 4: marketing and support parity

- replace Brevo sync and logging
- preserve operator visibility for failures, retries, and downstream delivery outcomes

## Design Constraints Dieselbrook Should Enforce

- no new service should depend on ad hoc cross-database SQL from arbitrary business code
- all consultant and MLM reads should be routed through a narrow access boundary
- lifecycle side effects triggered by orders must become explicit middleware steps, not hidden SQL side effects
- report contracts should be versioned and detached from FoxPro page/process routing

## Decisions Still Needed

1. Whether phase 1 will continue using `compsys..sp_Sendmail` or replace outbound communication entirely.
2. Whether consultant reports should be served live from SQL or cached in a middleware reporting projection.
3. Whether Namibia support is required in the first release or only South Africa parity.
4. Whether any `compplanLive` functions besides currently observed downline access must remain callable from middleware.