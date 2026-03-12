# Identity And SSO Domain Pack

## 1. Domain Summary

Business purpose:

- manage storefront and adjacent-system identity, OTP verification, role assignment, staff access, and SSO/session propagation where legacy support portals or academy systems are involved

Why the domain matters to the migration:

- identity behaviour is split across plugin auth/registration overrides, OTP services, staff sync, and `ssoclass.prg`
- consultant and staff access models shape pricing, reporting, account pages, and operational support flows

Phase-1 importance:

- high
- not every legacy SSO integration may survive, but core identity and any required federation behaviour must be made explicit early

## 2. Current Systems And Actors

Systems involved:

- NopCommerce identity and custom plugin auth overrides
- `NISource/syncstaff.prg`
- `NISource/ssoclass.prg`
- `NopIntegration..NopSSO`
- external support/academy or WordPress-like systems referenced by SSO logic

Human roles involved:

- consultants
- staff and EXCO users
- support/admin users

Source-of-truth boundaries:

- storefront account identities are synchronized from business systems rather than purely self-managed in the web platform
- `NopSSO` and SSO helper logic currently bridge identity into adjacent external platforms where required

## 3. Current Source Components

FoxPro files:

- `NISource/syncstaff.prg`
- `NISource/ssoclass.prg`
- consultant identity coupling in `syncconsultant.prg`

Plugin components:

- overridden registration/auth services
- OTP services/components/controllers
- user profile and account controllers

SQL procedures/tables/databases:

- `NopSSO`
- plugin OTP tables/entities
- `arcust` for consultant/staff source identities

## 4. Current Workflows

### Workflow A: Consultant/staff identity provisioning

Trigger:

- ERP-side user state changes or initial sync

Inputs:

- consultant/staff records from `arcust`
- classification rules for consultant, staff, EXCO, and related roles

Major logic steps:

- middleware syncs users into storefront account model
- role assignment and password bootstrap rules are applied

Outputs:

- updated storefront user accounts and roles

Side effects:

- downstream access to pricing, account pages, and admin/report features depends on correct roles

Error/failure behavior:

- bad provisioning can create access leaks or lock out operational users

### Workflow B: OTP verification and protected account actions

Trigger:

- protected account flow requiring OTP verification

Inputs:

- customer identity
- OTP API configuration

Major logic steps:

- OTP is sent through configured API path
- plugin services verify entered OTP and verified state

Outputs:

- verified identity state for the relevant action

Side effects:

- strengthens account access flow where required

Error/failure behavior:

- failed or weak OTP flow degrades security and user experience simultaneously

### Workflow C: External SSO/session propagation

Trigger:

- user access into linked support/academy systems

Inputs:

- storefront user identity
- external site credentials or application password
- `NopSSO` token/session state

Major logic steps:

- SSO helper checks or creates external linkage
- token/session values are issued or reused until expiry

Outputs:

- linked-session/token behaviour across systems

Side effects:

- support or academy access continuity depends on this if those external systems remain in scope

Error/failure behavior:

- SSO issues can appear as support-site outages or user-access inconsistencies even when storefront login works

## 5. Operational Dependencies

Other domains that depend on this one:

- consultants and MLM because consultant identity is a domain core
- reporting because ACL/report visibility depends on identity and roles
- admin and back-office tooling because staff/admin access requires clear role boundaries

Downstream side effects:

- role-based pricing and entitlement behaviour
- report access
- external linked-system access

Upstream assumptions:

- not all legacy federated destinations necessarily need phase-1 parity

Timing or sequencing dependencies:

- identity provisioning must precede entitlement and reporting availability

## 6. Replacement Boundary

What must be replaced in phase 1:

- consultant/staff identity provisioning into Shopify or equivalent identity model
- necessary role and entitlement mapping
- OTP capability if still required
- any truly required SSO/federation path for live operations

What can remain ERP-side or legacy-side temporarily:

- non-critical linked-system SSO destinations if business retires or defers them

What is candidate for later-phase modernization:

- broader identity consolidation and external federation redesign
- password/bootstrap patterns inherited from legacy sync logic

## 7. Risks And Fragility Points

- role logic is tightly coupled to business behaviour and easy to oversimplify
- legacy password/bootstrap patterns are operational evidence but not good future-state design
- SSO may look peripheral until a support or academy workflow fails in production

## 8. Open Decisions

Decision IDs:

- `D-01` consultant/customer operating model on Shopify
- `D-06` Shopify Plus/B2B strategy where identity affects account structures
- `D-07` day-one mandatory auth/account features

Assumption and dependency IDs:

- `A-01` Shopify direction remains valid
- `A-03` consultants remain a distinct operating model
- `X-DEP-01` Dieselbrook final Shopify solution intent

## 9. Recommended Phase-1 Capabilities

Named capabilities required:

- identity provisioning service
- role and entitlement mapping service
- OTP verification service
- federation adapter for any retained external SSO

Recommended service ownership boundaries:

- storefront platform should own user session and presentation-layer auth
- middleware should own cross-system identity synchronization and any retained federation logic

## 10. Evidence Base

Source files:

- `NISource/syncstaff.prg`
- `NISource/ssoclass.prg`
- `NISource/syncconsultant.prg`
- `NopCommerce - Annique/Plugins/Annique.Plugins.Nop.Customization/Services/OTP/OtpService.cs`

Existing discovery docs:

- `docs/10_accessible_estate_and_replacement_surface.md`
- `docs/11_nisource_process_parity_matrix.md`
- `docs/12_nop_customization_domain_classification.md`
- `docs/13_phase1_sql_contract.md`
