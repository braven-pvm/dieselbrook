# NISource Replacement Worklist

## Purpose

This document turns the consultant and MLM findings into a concrete replacement worklist for Dieselbrook.

It focuses on the current `NISource` touchpoints that must be replaced or deliberately retained behind new Dieselbrook-owned services.

## Scope Principle

Because Dieselbrook is replacing `NISource`, every current FoxPro dependency on:

- `amanniquelive`
- `compplanlive`
- `compsys`
- `NopIntegration`

must be assessed as either:

- replace in Dieselbrook middleware
- keep ERP-side but invoke from Dieselbrook middleware
- retire explicitly

## Replacement Worklist

| Current touchpoint | Current source | Current dependency | Replacement target | Phase 1 action |
|---|---|---|---|---|
| consultant sync from AM to storefront account | `syncconsultant.prg` | `arcust`, Nop customer records, plugin profile tables | `ConsultantSyncService` | replace |
| sponsor/downline validation | `apiprocess.prg` | `CompPlanLive.dbo.fn_Get_DownlineHist` | `ConsultantHierarchyAndMLMAccessService` | replace |
| order-triggered consultant reactivation | `syncorders.prg` -> `sp_ws_reactivate` | `compplanlive..deactivations` | `OrderWritebackService` + `ConsultantLifecycleService` | replace caller, preserve SQL proc initially |
| consultant welcome email queueing | `syncconsultant.prg` + `MailMessage` | `compsys.dbo.mailmessage` | `ConsultantOnboardingCommunicationService` | replace |
| consultant report data reads | `reports.prg` | `ctcomp`, `ctcomph`, related report procedures | `ConsultantReportingDataService` | replace |
| consultant discount refresh | SQL `sp_ct_updatedisc` | `CompPlanLive.dbo.ctConsv` | `PricingParityService` + ERP stored procedure invocation if still used | assess and preserve parity |
| consultant trade-group segmentation | SQL `sp_TradeGroupBuild` | `Ctstatement`, `Ctstatementh`, `CtRunData`, `CtConsv` | `ConsultantReportingDataService` or dedicated segmentation job | assess |
| Brevo campaign log processing | `brevoprocess.prg` | `NopIntegration..Brevolog` | `BrevoSyncService` | replace |
| direct mail queue model | `amdata.prg` `MailMessage` class | `compsys.dbo.mailmessage` | app-owned messaging adapter | replace |

## Detailed Work Items

### 1. Consultant master sync

Replace the logic currently in `syncconsultant.prg` that:

- matches consultant identity by `ccustno`
- creates or updates storefront customer accounts
- writes consultant profile metadata
- assigns consultant roles
- triggers new-consultant onboarding behavior

Target Dieselbrook component:

- `ConsultantSyncService`

### 2. Hierarchy and sponsor validation API

Replace the FoxPro API behavior that validates sponsor relationships using:

- `CompPlanLive.dbo.fn_Get_DownlineHist`

Target Dieselbrook component:

- `ConsultantHierarchyAndMLMAccessService`

Required outputs:

- sponsor validity
- in-downline result
- current status / active checks

### 3. Order lifecycle integration with MLM-side consultant state

Replace the order-import caller behavior in `syncorders.prg` that currently executes:

- `sp_ws_reactivate`

Target Dieselbrook components:

- `OrderWritebackService`
- `ConsultantLifecycleService`

Phase 1 design:

- Dieselbrook owns the call point and orchestration
- SQL procedure may remain authoritative until replaced deliberately

### 4. Consultant communication and onboarding emails

Replace the current FoxPro mail queue model that uses:

- `MailMessage` class in `amdata.prg`
- `compsys.dbo.mailmessage`

Target Dieselbrook component:

- `ConsultantOnboardingCommunicationService`

Decision required:

- continue queueing into `compsys`
- or migrate middleware-owned communications to Dieselbrook-owned delivery infrastructure

### 5. Consultant and MLM reporting surface

Replace the FoxPro report reads in `reports.prg` that access:

- `ctcomp`
- `ctcomph`
- report procedures around recruits / deactivations / consultant stats

Target Dieselbrook component:

- `ConsultantReportingDataService`

Output surface options:

- middleware APIs for custom app dashboards
- report endpoints for internal backoffice tools
- exported report datasets

### 6. Brevo-related marketing and communication logging

Replace the FoxPro process around:

- `NopIntegration..Brevolog`

Target Dieselbrook component:

- `BrevoSyncService`

Important note:

- `Brevolog` is a Brevo campaign log, not a general API audit trail

## Recommended Dieselbrook Service Set

The replacement worklist implies the following consultant/MLM-capable middleware services:

- `ConsultantSyncService`
- `ConsultantLifecycleService`
- `ConsultantHierarchyAndMLMAccessService`
- `ConsultantReportingDataService`
- `ConsultantOnboardingCommunicationService`
- `BrevoSyncService`
- `OrderWritebackService`
- `PricingParityService`

## Risk Notes

- The biggest hidden risk is not the storefront UI. It is missing FoxPro-era side effects around hierarchy checks, reactivation, and communications.
- Replacing `NISource` without replacing these touchpoints would create apparent consultant parity in Shopify while silently breaking operational flows.
- The commission engine itself does not need to be rewritten in phase 1, but the middleware call and read surface around it does need a Dieselbrook-owned replacement.