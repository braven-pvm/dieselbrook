# NISource Process Parity Matrix

## Purpose

This document turns the major `NISource` runtime files into a parity-oriented replacement matrix for Dieselbrook.

The goal is not to preserve FoxPro implementation details.

The goal is to preserve the observable behavior, integration contracts, and side effects that matter to the business.

## Core Rule

Each legacy process should be replaced as an explicit service boundary with clear inputs, outputs, and failure handling.

## Process Matrix

| Legacy file | Current responsibility | Key inputs and dependencies | Key outputs and side effects | Dieselbrook replacement owner |
|---|---|---|---|---|
| `nopintegrationmain.prg` | application bootstrap, Web Connection host, process startup | IIS/Web Connection runtime, environment flags, process registration | starts runtime host, routes incoming work to process classes | `AppHostService` |
| `apiprocess.prg` | API routing for sync and report endpoints | HTTP requests, CORS rules, request encoding, SQL access, sync classes | JSON responses, sync dispatch, report dispatch, consultant validation flows | `MiddlewareApiGateway` |
| `appprocess.prg` | legacy page/request processing | HTTP requests, session state, page scripts | rendered page responses, legacy process dispatch | retire as legacy page host, preserve only any required endpoint behavior in `MiddlewareApiGateway` |
| `brevoprocess.prg` | Brevo webhook/process entry point | Brevo webhooks, Brevo data/API classes, `NopIntegration` support tables | Brevo event handling, log updates, campaign/contact side effects | `BrevoSyncService` |
| `communicationsapi.prg` | SMS/communications provider wrapper | outbound message payloads, provider credentials, token management | SMS sends, provider API calls, delivery/error results | `MessagingGatewayService` |
| `syncproducts.prg` | product and inventory synchronization from ERP toward commerce | ERP item and mapping data, Nop API/client, product/category/brand mappings | creates or updates product state in commerce channel | `ProductSyncService` and `InventorySyncService` |
| `syncorders.prg` | order import from commerce into ERP | Nop order/customer/address data, ERP tables, `sp_ws_reactivate`, consultant/customer state | writes ERP order records and dependent records, can reactivate consultants, contributes to invoice/portal path | `OrderWritebackService` |
| `syncorderstatus.prg` | reverse fulfillment and order-status sync | ERP shipment, invoice, and status tables; tracking sources | pushes status and tracking updates back toward commerce | `OrderStatusFulfillmentService` |
| `syncconsultant.prg` | consultant sync and onboarding support | `arcust`, address data, Nop customer API, consultant role/lifecycle data, messaging support | creates or updates consultant storefront identity, onboarding side effects, welcome comms | `ConsultantSyncService` and `ConsultantLifecycleService` |
| `syncstaff.prg` | staff sync into storefront identity model | ERP customer/staff classification, Nop customer API | creates or updates staff identities and roles | `StaffProvisioningService` |
| `nopnewregistrations.prg` | new consultant registration intake and validation | registration payloads, duplicate checks, sponsor/referral checks | creates registration records, triggers consultant onboarding path | `ConsultantRegistrationService` |
| `reports.prg` | dynamic report metadata and report rendering support | `NopReports`, report parameter metadata, MLM/ERP report tables | report definitions, field metadata, public/admin report outputs | `ConsultantReportingDataService` and `OperationalReportingService` |
| `amdata.prg` | ERP and support database data-access wrapper | `amanniquelive`, `compplanLive`, `compsys`, procedures, table models | reads/writes SQL-backed business entities, document numbers, queued communications | `LegacySqlAccessGateway` |
| `nopdata.prg` | Nop data-access wrapper | Nop DB structures and related stored procedures | reads/writes Nop-side entities used by middleware | replace with Shopify/Admin API integration plus app data store |
| `nopapi.prg` | Nop API client wrapper | Nop API endpoints, auth credentials, JSON payload templates | remote commerce API calls, serialized responses, error capture | replace with `ShopifyAdminClient` and middleware API adapters |
| `basedata.prg` | shared CRUD/data abstraction | field mappings, SQL cursors, validation helpers | reused DAL patterns, object persistence plumbing | fold into typed data access layer, do not port directly |
| `syncclass.prg` | shared sync base class | environment config, auth bootstrap, JSON serialization, Nop credentials | authenticated sync runtime context | `SyncRuntimeBase` |
| `ssoclass.prg` | SSO integration support | JWT/auth secrets, WordPress/support portal references, identity mappings | SSO token/session behavior, user linkage | `IdentityFederationService` if still required |
| `brevodata.prg` | Brevo data object wrappers | `NopIntegration` Brevo tables and SQL access | Brevo log persistence and campaign state tracking | `BrevoDataAccessService` |
| `brevoapi.prg` | Brevo API client wrapper | Brevo REST endpoints, auth token, campaign/list payloads | Brevo create/update/send actions and responses | `BrevoClient` |

## Shared Runtime Concerns That Must Also Be Replaced

### SQL access pattern

- direct cross-database SQL is embedded in runtime business logic
- plaintext credentials are present in source
- FoxPro data access is mixed with orchestration and transport concerns

Dieselbrook replacement:

- a narrow SQL access gateway per domain
- secret-managed credentials
- typed read/write contracts and audited procedure use

### Logging and observability

- current runtime uses file-based logs and class-level error flags
- request tracing appears in support tables such as `wwrequestlog`

Dieselbrook replacement:

- structured centralized logs
- correlation IDs per sync/workflow
- operator-facing reconciliation and retry visibility

### Authentication and external integrations

- API credentials and provider keys are handled in-process
- token caching and refresh behavior is weak or ad hoc

Dieselbrook replacement:

- secret store
- provider-specific adapters
- explicit retry/backoff and failure policies

### Idempotency and resilience

- most sync flows rely on duplicate checks rather than durable workflow state
- retry behavior is limited and side effects are tightly coupled

Dieselbrook replacement:

- idempotency ledger
- workflow checkpoints
- retry and dead-letter handling per integration domain

## Highest-Priority Parity Targets

1. `syncorders.prg`
2. `syncorderstatus.prg`
3. `syncconsultant.prg`
4. `nopnewregistrations.prg`
5. `reports.prg`
6. `communicationsapi.prg`
7. `brevoprocess.prg`

These are the files most likely to create business regressions if their behavior is only partially replaced.