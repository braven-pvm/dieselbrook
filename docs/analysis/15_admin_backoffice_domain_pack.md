# Admin And Back-Office Tooling Domain Pack

## 1. Domain Summary

Business purpose:

- provide the internal operational surfaces needed to manage reports, campaign/product exposure, consultant/account support actions, and other business workflows not exposed purely through storefront UX

Why the domain matters to the migration:

- legacy operational tooling is split across Nop admin/plugin surfaces and `NISource` web-facing modules
- this is the domain most exposed to scope creep unless business-critical tools are separated from low-value legacy UI parity

Phase-1 importance:

- high but decision-dependent
- some internal tooling is likely essential for day-one operations, while other legacy surfaces should be deferred or retired

## 2. Current Systems And Actors

Systems involved:

- plugin admin controllers and settings surfaces
- report admin UI
- `NISource/campaign/` campaign admin module
- any additional missing web-facing nopintegration modules not yet supplied
- support tables/config in `NopIntegration`

Human roles involved:

- internal admins
- campaign/pricing operators
- report administrators
- support/operations staff

Source-of-truth boundaries:

- internal tools mostly orchestrate or expose underlying ERP/middleware truth rather than owning business truth themselves
- current repo snapshot is incomplete for some web-facing nopintegration UI shell/assets

## 3. Current Source Components

FoxPro files/modules:

- `NISource/campaign/campaign.prg`
- `NISource/campaign/Campaign_class.prg`
- `NISource/campaign/Campaign.html`
- `NISource/campaign/campaigns.js`
- `appprocess.prg` for legacy page/request handling

Plugin components:

- `Controllers/AdminAnniqueReportController.cs`
- plugin configuration/settings surfaces
- admin action filters and admin-facing controllers referenced in plugin analysis

SQL procedures/tables:

- `NopReports`
- support/config tables such as `NopSettings`, `ApiClients`, `wwrequestlog`

## 4. Current Workflows

### Workflow A: Campaign administration and product exposure management

Trigger:

- business user updates campaign structure, product assignment, or stage publication

Inputs:

- campaign metadata and campaign stored procedures

Major logic steps:

- web-facing module loads campaign summaries and SKU detail
- operator edits campaign/product state and publishes or copies as required

Outputs:

- updated operational campaign state

Side effects:

- downstream storefront pricing and product exposure are affected

Error/failure behavior:

- current source is incomplete for the UI shell, so precise runnable parity is not fully proven even though business intent is clear

### Workflow B: Report administration

Trigger:

- admin creates/edits report definitions and parameters

Inputs:

- report metadata, ACL rules, parameters, menu/display settings

Major logic steps:

- plugin admin controller and related services manage report records and parameters

Outputs:

- updated report admin configuration

Side effects:

- user-visible report availability and behaviour are changed

Error/failure behavior:

- admin mistakes can invalidate report usage without changing source data

### Workflow C: Operational settings and support surfaces

Trigger:

- internal operational support or configuration changes

Inputs:

- settings, logs, client configuration, and internal support data

Major logic steps:

- plugin and middleware support tables/controllers expose or update operational configuration

Outputs:

- internal operational control state

Side effects:

- supportability, diagnostics, and internal runbook behaviour depend on the existence of some minimal admin surface

Error/failure behavior:

- phase-1 can be functionally live but operationally brittle if no replacement operator surface exists for genuinely needed tasks

## 5. Operational Dependencies

Other domains that depend on this one:

- pricing/campaigns because campaign operations likely need internal tooling
- reporting because report metadata management is an admin surface
- identity because staff/admin access control governs who can use internal tools

Downstream side effects:

- ability to manage campaign and report behaviour
- ability to diagnose issues during transition

Upstream assumptions:

- only business-critical internal tools should survive phase 1

Timing or sequencing dependencies:

- decisions on required admin tooling directly affect scope and should be locked before requirement shells are written

## 6. Replacement Boundary

What must be replaced in phase 1:

- only the internal/admin tooling confirmed as operationally necessary
- minimum operator visibility needed for support, diagnostics, and parity-critical management tasks

What can remain ERP-side or legacy-side temporarily:

- low-value or rarely used admin surfaces
- internal pages without a clear phase-1 business case

What is candidate for later-phase modernization:

- full admin UX redesign
- richer internal back-office platform replacing scattered legacy tools

## 7. Risks And Fragility Points

- admin/back-office scope is the easiest place for the programme to overbuild
- incomplete `NISource` web-shell visibility means we should not assume we have seen every internal tool
- the absence of any operator tooling at all would create avoidable operational risk

## 8. Open Decisions

Decision IDs:

- `D-03` which legacy admin/back-office functions must survive in phase 1
- `D-08` whether additional browser-facing nopintegration modules are still to be supplied
- `D-09` whether phase 1 needs a new back-office/admin interface or only APIs and ops tooling
- `D-10` which reports must remain operational

Assumption and dependency IDs:

- `A-05` current `NISource` snapshot is useful but incomplete as a web app checkout
- `A-06` campaign module belongs to the legacy estate and matters to admin scope
- `X-DEP-02` Annique response on missing web shell/assets
- `X-DEP-05` confirmation of back-office tools required for phase 1

## 9. Recommended Phase-1 Capabilities

Named capabilities required:

- operator diagnostics and trace surface
- minimal campaign/report admin capability where confirmed necessary
- internal access-control layer for admin users

Recommended service ownership boundaries:

- middleware and platform services should own business logic
- admin UI should be thin, purpose-built, and limited to confirmed operational needs

## 10. Evidence Base

Source files:

- `NISource/campaign/campaign.prg`
- `NISource/campaign/Campaign_class.prg`
- `NISource/appprocess.prg`
- `NopCommerce - Annique/Plugins/Annique.Plugins.Nop.Customization/Controllers/AdminAnniqueReportController.cs`

Existing discovery docs:

- `docs/10_accessible_estate_and_replacement_surface.md`
- `docs/11_nisource_process_parity_matrix.md`
- `docs/14_campaign_module_missing_dependencies.md`
- `docs/15_nisource_source_completeness_matrix.md`
- `docs/16_annique_followup_request_missing_web_assets.md`
