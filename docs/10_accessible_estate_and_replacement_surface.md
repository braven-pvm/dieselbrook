# Accessible Estate And Replacement Surface

## Purpose

This document consolidates the currently accessible legacy estate and the concrete replacement surface that Dieselbrook must understand before building the new middleware.

It is meant to answer two questions at once:

1. What can we verify directly today?
2. What does NopCommerce plus `NISource` actually do and touch that Dieselbrook will need to replace?

## What Is Directly Accessible Today

### Local source and documentation estate

Confirmed accessible in the workspace:

- `NISource/`
- `NopCommerce - Annique/`
- `AnqIntegrationApiSource/`
- `docs/`
- `database/`
- `middleware/`
- `tmp/`

### Direct staging SQL estate

Confirmed by CLI inspection on `AMSERVER-V9`:

- `amanniquelive`
- `amanniquenam`
- `compplanLive`
- `compplanNam`
- `compsys`
- `NopIntegration`
- `Annique_Reports`
- `BackOffice`
- `BackOfficeNam`

### Key operational objects confirmed on staging

- order core: `sosord`, `sostrs`, `soskit`, `soship`, `arinvc`, `arcash`, `arcust`
- order operations: `SOPortal`, `be_waybill`, `changes`, `Campaign`, `CampDetail`, `soxitems`, `wsSetting`
- consultant and MLM: `CTcomph`, `CTcomp`, `CTconsv`, `CTRunData`, `CTstatement`, `CTstatementh`, `deactivations`, `CTdownlineh`
- support and communications: `MAILMESSAGE`, `sp_Sendmail`
- integration support: `BrevoLog`, `NopReports`, `ApiClients`, `NopSettings`, `NopSSO`, `wwrequestlog`

## Runtime Components That Must Be Replaced

## 1. FoxPro middleware runtime

The `NISource` folder is not one small sync script. It is the legacy middleware application.

Confirmed major entry points include:

- `nopintegrationmain.prg`
- `apiprocess.prg`
- `appprocess.prg`
- `brevoprocess.prg`
- `communicationsapi.prg`
- `syncproducts.prg`
- `syncorders.prg`
- `syncorderstatus.prg`
- `syncconsultant.prg`
- `syncstaff.prg`
- `nopnewregistrations.prg`
- `reports.prg`
- `amdata.prg`
- `nopdata.prg`
- `nopapi.prg`
- `basedata.prg`
- `syncclass.prg`

### What that means in practice

The current middleware owns or coordinates:

- API and process routing
- product and inventory synchronization
- order import and reverse order-status synchronization
- consultant synchronization and new consultant registration intake
- sponsor and downline lookups against MLM data
- consultant and operational reporting
- OTP, SMS, and communication-related flows
- Brevo-related synchronization and logging
- support logging and request tracing

Replacing `NISource` means replacing this full process estate, not only a few stored procedure calls.

## 2. Custom NopCommerce business layer

The custom Nop plugin under `Plugins/Annique.Plugins.Nop.Customization` is also a major business system, not a light extension.

The workspace contains a large plugin footprint with roughly 297 C# files across controllers, services, factories, domain objects, action filters, components, migrations, validators, and infrastructure.

Confirmed business domains in that plugin include:

- consultant registrations
- consultant awards
- gifts and gift card allocation
- exclusive items
- checkout customization and quick checkout
- custom order processing
- overridden discount and price calculation behavior
- custom shipping rules
- pickup collection
- user profile extensions
- OTP and identity-related flows
- private messages
- reports and report parameter management
- events and bookings
- category and manufacturer integration
- special offers
- configuration, routing, caching, and activity logging
- chatbot-related features

Replacing NopCommerce in its entirety means these behaviors must either move into Shopify plus Dieselbrook middleware or be intentionally retired.

## 3. SQL support estate behind the middleware

The replacement surface is not just the main ERP database.

Confirmed secondary databases with real production-like data on staging include:

- `compplanLive` for MLM hierarchy, statement, contribution, and lifecycle reads
- `compsys` for queued mail and SQL-driven dispatch support
- `NopIntegration` for request logging, report metadata, SSO support, settings, and marketing/integration support tables

This matters because the FoxPro layer currently crosses those database boundaries directly.

## Replacement Surface By Domain

| Domain | Legacy estate touched | Replacement expectation |
|---|---|---|
| Orders | NopCommerce, `syncorders.prg`, `syncorderstatus.prg`, `amanniquelive`, `SOPortal` | Dieselbrook middleware must own ingest, idempotency, ERP write-back, fulfillment/status propagation, and reconciliation |
| Consultants | plugin registration/profile logic, `syncconsultant.prg`, `nopnewregistrations.prg`, `arcust`, `compplanLive` | Dieselbrook must own consultant profile, lifecycle, sponsor validation, and storefront entitlement synchronization |
| Pricing and exclusives | plugin override services, `Campaign`, `CampDetail`, `soxitems`, ERP-derived rules | Dieselbrook must own effective pricing and entitlement sync into Shopify |
| Reporting | plugin report features, `reports.prg`, `NopIntegration..NopReports`, MLM report tables | explicit report APIs and reporting contracts are required |
| Communications | OTP, SMS, welcome mail, `MAILMESSAGE`, `sp_Sendmail`, Brevo support | replace with auditable messaging services and preserve critical business notifications |
| Marketing and SSO support | `BrevoLog`, `NopSSO`, `ApiClients`, settings/logging tables | re-home auth, config, and marketing support concerns into the new platform |

## Highest-Risk Replacement Zones

- order import side effects that also affect consultant lifecycle
- consultant pricing and exclusive-item behavior split across plugin code and ERP data
- assuming `NopIntegration` is optional when it currently supports reports, SSO, settings, and request logging
- under-scoping the custom Nop plugin because it contains real business workflows, not only UI tweaks
- treating MLM dependencies as out of scope when `NISource` currently reads them directly for live business behavior

## Dieselbrook Working Assumption

The safe working assumption now is:

- Shopify replaces storefront and checkout behavior
- Dieselbrook replaces `NISource` and the custom Nop business layer with explicit middleware and app services
- AccountMate and related SQL estates remain the operational source for core ERP and MLM truth until a later modernization decision is made

## Recommended Next Discovery Steps

1. Turn each legacy `NISource` process into a parity matrix of inputs, outputs, side effects, and replacement service ownership.
2. Break the custom Nop plugin into retire, replace, and preserve categories by domain.
3. Confirm which `NopIntegration` tables are still actively required at runtime versus historical support only.
4. Define the minimum read/write SQL contract Dieselbrook needs for phase 1 across `amanniquelive`, `compplanLive`, `compsys`, and `NopIntegration`.