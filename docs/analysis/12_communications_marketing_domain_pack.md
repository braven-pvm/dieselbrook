# Communications And Marketing Domain Pack

## 1. Domain Summary

Business purpose:

- send and track business-critical messages such as OTP, welcome mail, SMS, and marketing/contact events
- preserve customer and consultant communication paths that support onboarding, support, and campaign-related operations

Why the domain matters to the migration:

- communications are distributed across middleware provider wrappers, plugin OTP flows, SQL support tables, and Brevo support logging
- silent message failures create support problems and can break onboarding, verification, and operational follow-up

Phase-1 importance:

- high
- not every marketing feature is day-one critical, but OTP, welcome, and operational messaging flows are parity-relevant

## 2. Current Systems And Actors

Systems involved:

- `communicationsapi.prg`
- `brevoprocess.prg`
- plugin OTP services and UI components
- `compsys`
- `NopIntegration`

Human roles involved:

- customers and consultants receiving OTP or operational messages
- support and onboarding staff
- marketing/contact-management operators where Brevo-linked processes remain in use

Source-of-truth boundaries:

- messaging events originate from business workflows across consultant registration, login/verification, and operations
- legacy support databases hold queued mail and communication logs, but these should not automatically be treated as future-state system-of-records

## 3. Current Source Components

FoxPro files:

- `NISource/communicationsapi.prg`
- `NISource/brevoprocess.prg`
- welcome-mail behaviour in `NISource/syncconsultant.prg`

Plugin components:

- `Services/OTP/OtpService.cs`
- `Services/OTP/IOtpService.cs`
- `Components/OTPFormViewComponent.cs`
- customer/account controllers with OTP paths

SQL procedures/tables/databases:

- `MAILMESSAGE`
- `sp_Sendmail`
- `BrevoLog`
- `NopSettings`
- `ApiClients`

## 4. Current Workflows

### Workflow A: OTP generation and verification

Trigger:

- login, account action, or verification flow requiring OTP

Inputs:

- customer identity
- configured OTP API URL and environment flags
- contact channels such as email or SMS

Major logic steps:

- plugin-side service determines whether OTP is enabled
- OTP is generated/sent through middleware/API path
- verification checks stored OTP state and expiry

Outputs:

- delivered OTP and verified/unverified state for the customer

Side effects:

- account access and sensitive workflow completion can depend on OTP success

Error/failure behavior:

- stale or failed OTP delivery blocks user flows immediately

### Workflow B: Consultant onboarding and welcome communications

Trigger:

- consultant creation or onboarding progression

Inputs:

- consultant/sponsor data
- configured mail or message templates

Major logic steps:

- consultant sync/onboarding logic composes welcome/support communications
- downstream message send path uses legacy mail or provider integrations

Outputs:

- welcome/support message delivery attempts

Side effects:

- onboarding perception and support readiness depend on these communications

Error/failure behavior:

- onboarding succeeds operationally but appears broken or incomplete to consultants if messages fail

### Workflow C: Brevo/contact synchronization and logging

Trigger:

- Brevo webhooks, sync events, or campaign/contact changes

Inputs:

- webhook payloads or Brevo API data
- `NopIntegration` support tables such as `BrevoLog`

Major logic steps:

- middleware entry point receives Brevo-related events
- state/logging updates are written and side effects applied

Outputs:

- contact or campaign sync state and communication audit records

Side effects:

- marketing/contact-management processes retain continuity during transition if still required

Error/failure behavior:

- communications can appear sent from a business perspective without reliable auditability if logging is weak

## 5. Operational Dependencies

Other domains that depend on this one:

- consultants and MLM because onboarding and support flows are communication-heavy
- identity and SSO because OTP intersects with access and verification
- reporting and admin because communication logs may be needed operationally

Downstream side effects:

- customer verification state
- onboarding/support experience
- marketing/contact audit trail

Upstream assumptions:

- not all legacy queues or providers should survive into phase 1 unless a compatibility need is proven

Timing or sequencing dependencies:

- OTP and onboarding messages must happen close to the triggering business event

## 6. Replacement Boundary

What must be replaced in phase 1:

- OTP send/verify capability if still required by business/security policy
- consultant welcome/onboarding communications critical to live operations
- auditable messaging gateway behaviour for parity-critical messages
- any materially used Brevo/contact synchronization still required at go-live

What can remain ERP-side or legacy-side temporarily:

- `compsys` queued-mail path only if Dieselbrook explicitly chooses a transition dependency
- non-critical marketing automation not needed for day-one parity

What is candidate for later-phase modernization:

- deeper marketing automation redesign
- migration off transitional SQL mail/log support entirely
- richer customer communications orchestration and preferences model

## 7. Risks And Fragility Points

- communications are split across SQL queues, middleware providers, and plugin-side account flows
- OTP failure causes visible user-facing breakage immediately
- reliance on legacy queue tables without strong observability creates silent operational risk
- Brevo-related processes are easy to under-scope because they look like marketing-only concerns even when they carry support/audit value

## 8. Open Decisions

Decision IDs:

- `D-05` whether outbound communications still use `compsys` temporarily
- `D-07` which custom Nop communication features are mandatory for day-one parity

Assumption and dependency IDs:

- `A-01` Shopify target direction remains valid
- `X-DEP-01` Dieselbrook final Shopify solution intent

## 9. Recommended Phase-1 Capabilities

Named capabilities required:

- messaging gateway service
- OTP service
- communication audit/log service
- Brevo/contact sync adapter if still required

Recommended service ownership boundaries:

- middleware should own provider integration and auditability
- storefront should only invoke communication intents, not own delivery logic

## 10. Evidence Base

Source files:

- `NISource/communicationsapi.prg`
- `NISource/brevoprocess.prg`
- `NISource/syncconsultant.prg`
- `NopCommerce - Annique/Plugins/Annique.Plugins.Nop.Customization/Services/OTP/OtpService.cs`

Existing discovery docs:

- `docs/10_accessible_estate_and_replacement_surface.md`
- `docs/11_nisource_process_parity_matrix.md`
- `docs/13_phase1_sql_contract.md`
- `docs/annique-discovery.1.0.md`
