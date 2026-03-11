# Phase 1 Staging Object Trace

## Purpose

This document traces the consultant/MLM objects confirmed on staging that matter for phase-1 Dieselbrook replacement.

It is focused on replacement planning, not full schema documentation.

## Core Rule

Phase 1 must replace the FoxPro middleware touchpoints while preserving required SQL-native authority where appropriate.

## Confirmed Staging Objects That Matter For Phase 1

| Object | Database | Current role | Phase-1 implication |
|---|---|---|---|
| `CTcomph` | `compplanLive` | commission source/history used by rebate logic and reports | read dependency must be understood and preserved |
| `CTcomp` | `compplanLive` | current-period commission data used by reports | report/API dependency |
| `CTconsv` | `compplanLive` | consultant sales / contribution data used by discount and grouping logic | pricing/report dependency |
| `CTRunData` | `compplanLive` | run-cycle / recruit-related consultant metrics | reporting and segmentation dependency |
| `CTstatement` | `compplanLive` | current statement data | report dependency |
| `CTstatementh` | `compplanLive` | historical statement data | report/history dependency |
| `deactivations` | `compplanLive` | consultant deactivation/reactivation data | order and lifecycle dependency |
| `CTdownlineh` | `compplanLive` | historical hierarchy/downline data | hierarchy/report dependency |
| `MAILMESSAGE` | `compsys` | queued mail storage | communication dependency |
| `sp_Sendmail` | `compsys` | mail dispatch procedure | communication/alert dependency |
| `BrevoLog` | `NopIntegration` | Brevo campaign log | marketing middleware dependency |

## Replacement Mapping

### Consultant hierarchy and sponsor validation

Confirmed staging dependency:

- `compplanLive` hierarchy-related data and functions

Dieselbrook replacement target:

- `ConsultantHierarchyAndMLMAccessService`

### Order-triggered consultant lifecycle handling

Confirmed staging dependency:

- `compplanLive..deactivations`

Dieselbrook replacement target:

- `OrderWritebackService`
- `ConsultantLifecycleService`

### Consultant report data access

Confirmed staging dependencies:

- `CTcomp`
- `CTcomph`
- `CTstatement`
- `CTstatementh`
- `CTRunData`
- `CTconsv`
- `CTdownlineh`

Dieselbrook replacement target:

- `ConsultantReportingDataService`

### Consultant communication flows

Confirmed staging dependencies:

- `compsys.dbo.MAILMESSAGE`
- `compsys.dbo.sp_Sendmail`

Dieselbrook replacement target:

- `ConsultantOnboardingCommunicationService`
- generic middleware messaging gateway

### Brevo-related middleware support

Confirmed staging dependency:

- `NopIntegration..BrevoLog`

Dieselbrook replacement target:

- `BrevoSyncService`

## Immediate Phase-1 Design Guidance

- treat `compplanLive` as a directly accessible staging dependency, not a theoretical side database
- design middleware interfaces around the confirmed tables above
- replace FoxPro call sites first
- leave deep commission-engine replacement out of phase 1 unless explicitly commissioned

## Remaining Discovery Still Worth Doing

- inspect table schemas for the phase-1 objects above
- identify which stored procedures or functions beyond `sp_ws_reactivate` and `sp_ct_Rebates` must be invoked directly
- determine whether Dieselbrook should continue writing middleware-owned communications into `compsys` or replace that path completely