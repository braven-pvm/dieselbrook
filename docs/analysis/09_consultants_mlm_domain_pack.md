# Consultants And MLM Domain Pack

## 1. Domain Summary

Business purpose:

- represent consultants as a real operating model spanning storefront identity, ERP customer state, sponsor/upline relationships, and MLM-dependent behaviour
- preserve onboarding, lifecycle, entitlement, and reporting-critical consultant behaviour during platform replacement

Why the domain matters to the migration:

- consultants are not ordinary ecommerce customers in the legacy estate
- identity, pricing, exclusive access, registration, activation, and reporting all depend on consultant-specific rules and data
- under-scoping this domain would break both commerce behaviour and field-force operations

Phase-1 importance:

- critical
- consultant identity and lifecycle parity are necessary for day-one business continuity even if the deep commission engine stays ERP-side

## 2. Current Systems And Actors

Systems involved:

- NopCommerce customer and custom plugin estate
- `NISource` consultant and registration processes
- `amanniquelive`
- `compplanLive`
- `compsys`
- `NopIntegration`

Human roles involved:

- consultants
- sponsors/upline structures
- consultant support and onboarding staff
- reporting/operations staff

Source-of-truth boundaries:

- AccountMate remains the operational source for consultant number, lifecycle state, and commission-affecting attributes
- the custom Nop plugin currently owns a large amount of consultant storefront behaviour and profile extension logic
- `compplanLive` remains the MLM/reporting read estate for hierarchy, statements, and downline-related behaviour

## 3. Current Source Components

FoxPro files:

- `NISource/syncconsultant.prg`
- `NISource/nopnewregistrations.prg`
- order-side coupling through `NISource/syncorders.prg`

Nop plugin components:

- consultant registration services and controllers
- overridden customer registration and authentication services
- user profile services
- consultant reports and account-area extensions
- consultant-exclusive checkout and entitlement behaviour

SQL procedures/tables/databases:

- `arcust`
- `sp_ws_reactivate`
- `compplanLive` tables such as `CTcomph`, `CTcomp`, `CTconsv`, `CTRunData`, `CTstatement`, `CTstatementh`, `CTdownlineh`, and `deactivations`
- support surfaces in `NopIntegration` and `compsys` where consultant-facing reporting, SSO, or communications behaviour intersects

## 4. Current Workflows

### Workflow A: Consultant synchronization into storefront estate

Trigger:

- ERP-side consultant changes requiring storefront alignment

Inputs:

- consultant master and lifecycle data from AccountMate
- sponsor and hierarchy-related data used by middleware/plugin logic

Major logic steps:

- middleware reads consultant state from ERP-side sources
- middleware maps consultant into storefront identity/profile shape
- plugin-side roles, profile metadata, and consultant behaviours are applied in NopCommerce

Outputs:

- consultant-capable storefront account
- consultant role and profile state synchronized to the web platform

Side effects:

- consultant-only pricing, access, and checkout behaviour become available once the account is aligned

Error/failure behavior:

- mismatched consultant state can create broken entitlement, pricing, or reporting access even when the storefront account technically exists

### Workflow B: New consultant registration intake

Trigger:

- prospective consultant registration from the storefront side

Inputs:

- captured registration/profile data
- sponsor/upline validation inputs

Major logic steps:

- plugin and middleware process consultant-specific registration rules
- validation occurs against ERP/MLM expectations rather than generic ecommerce registration only
- approved registration data is aligned back to operational consultant records

Outputs:

- new consultant onboarding state that can be recognized across storefront and ERP-side systems

Side effects:

- onboarding communications, consultant profile activation, and downstream lifecycle processing may all depend on this path

Error/failure behavior:

- silent divergence between storefront sign-up and ERP/MLM registration state is a major business risk

### Workflow C: Consultant reactivation through ordering

Trigger:

- order import for an inactive consultant account

Inputs:

- imported order with consultant-linked customer/account context

Major logic steps:

- order import path determines whether consultant must be reactivated
- middleware calls `sp_ws_reactivate` when current rules demand it

Outputs:

- consultant lifecycle state is mutated in ERP-side systems as part of order processing

Side effects:

- order processing and consultant lifecycle are coupled and cannot be designed in isolation

Error/failure behavior:

- order succeeds but consultant state remains wrong, or consultant state changes unexpectedly, if parity is not preserved carefully

### Workflow D: MLM hierarchy and reporting reads

Trigger:

- consultant account, sponsor, statement, or reporting flows require live hierarchy/report data

Inputs:

- `compplanLive` hierarchy, statement, sales, and history tables

Major logic steps:

- middleware or plugin logic reads downline, statement, or consultant status information from MLM-side tables
- consultant-facing reports and support flows surface this data in the web estate

Outputs:

- downline visibility
- statement/report visibility
- consultant support information

Side effects:

- reporting scope and consultant UX both depend on continued MLM read access unless intentionally redesigned

Error/failure behavior:

- broken hierarchy/report reads degrade consultant trust quickly even if basic ordering still works

## 5. Operational Dependencies

Other domains that depend on this one:

- pricing, campaigns, and exclusives because consultant status drives eligibility and effective pricing behaviour
- orders because ordering can reactivate consultants and depends on consultant/account mapping
- reporting because consultant statements and hierarchy views are operationally important
- communications because onboarding and support messages are often consultant-lifecycle driven

Downstream side effects:

- storefront entitlements
- consultant-only checkout policies
- sponsor validation
- MLM statement/report visibility

Upstream assumptions:

- consultant records continue to exist as ERP-rooted business entities in phase 1
- not every consultant-facing behaviour belongs inside Shopify alone

Timing or sequencing dependencies:

- consultant identity and eligibility must be established before pricing and exclusive access are enforced correctly
- registration and reactivation flows must align with order and account-state transitions

## 6. Replacement Boundary

What must be replaced in phase 1:

- consultant profile synchronization and storefront identity alignment
- consultant onboarding/registration integration behaviour required for live operations
- sponsor/upline validation needed for current registration or lifecycle flows
- consultant entitlement synchronization for pricing, exclusive items, and account-area behaviour
- minimum consultant reporting and support data access required for continuity

What can remain ERP-side or legacy-side temporarily:

- deep commission calculation engine
- primary MLM financial calculation logic
- historical statement generation where read access is sufficient and re-platforming is not justified in phase 1

What is candidate for later-phase modernization:

- full consultant portal redesign
- deeper re-platforming of MLM reporting and hierarchy experience
- consolidation of consultant identity across storefront, middleware, and support systems into a more modern domain model

## 7. Risks And Fragility Points

- treating consultants as ordinary customers would erase critical operating rules
- consultant lifecycle logic is split across ERP, middleware, plugin overrides, and order-side effects
- MLM reads are easy to dismiss as reporting-only, but they materially affect live consultant experience and support operations
- onboarding, identity, pricing eligibility, and exclusive access are coupled even if implemented in different parts of the legacy estate
- phase-1 scope can expand uncontrollably if every consultant-facing portal feature is assumed mandatory without business triage

## 8. Open Decisions

Decision IDs:

- `D-01` consultant versus customer operating model on Shopify
- `D-03` which admin/back-office functions must survive in phase 1
- `D-06` Shopify Plus/B2B strategy for consultant pricing patterns
- `D-07` which custom Nop consultant features are mandatory for day-one parity
- `D-09` whether a new back-office/admin experience is required in phase 1
- `D-10` which reports are truly operationally required

Assumption and dependency IDs:

- `A-02` preserve ERP/MLM truth in phase 1
- `A-03` consultants remain a distinct operating model
- `A-08` middleware will have private network connectivity to AccountMate SQL estate
- `X-DEP-01` Dieselbrook final Shopify solution intent
- `X-DEP-03` continued staging SQL access
- `X-DEP-04` confirmation of operationally used reports
- `X-DEP-06` hosting and infrastructure topology confirmation

## 9. Recommended Phase-1 Capabilities

Named capabilities required:

- consultant profile sync service
- consultant registration intake service
- sponsor and hierarchy validation adapter
- consultant entitlement projection service
- consultant lifecycle/reactivation adapter
- consultant reporting read service

Recommended service ownership boundaries:

- Shopify should own consultant storefront identity and presentation surfaces only where appropriate
- middleware should own consultant operational model, cross-system synchronization, and ERP/MLM integration logic
- ERP and MLM databases remain source of truth for commission-affecting and hierarchy-sensitive state in phase 1

## 10. Evidence Base

Source files:

- `NISource/syncconsultant.prg`
- `NISource/nopnewregistrations.prg`
- `NISource/syncorders.prg`

Existing discovery docs:

- `docs/10_accessible_estate_and_replacement_surface.md`
- `docs/11_nisource_process_parity_matrix.md`
- `docs/12_nop_customization_domain_classification.md`
- `docs/13_phase1_sql_contract.md`
- `middleware/consultants/01_current_state_consultant_domain.md`
- `middleware/consultants/06_commissions_reports_boundary.md`
- `middleware/consultants/07_mlm_data_dependency_map.md`
- `middleware/consultants/09_phase1_phase2_scope_split.md`
- `middleware/consultants/11_staging_mlm_inventory.md`
- `middleware/consultants/12_phase1_staging_object_trace.md`

Repo memory:

- `/memories/repo/environment-access-notes.md`
