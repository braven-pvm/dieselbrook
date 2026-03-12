# Reporting Domain Pack

## 1. Domain Summary

Business purpose:

- expose consultant and operational reports through metadata-driven report definitions, parameters, and rendered outputs

Why the domain matters to the migration:

- reporting is not just raw SQL access; it includes report metadata, parameter models, ACL/menu behavior, and both public and admin report surfaces
- some reports are likely operationally critical even if not all should survive phase 1

Phase-1 importance:

- medium to high, depending on confirmed business usage
- reporting must be triaged carefully to avoid both overbuilding and loss of critical operational visibility

## 2. Current Systems And Actors

Systems involved:

- `reports.prg`
- `apiprocess.prg`
- `NopIntegration..NopReports`
- plugin report domain/services/controllers
- ERP and MLM report source tables

Human roles involved:

- consultants consuming report outputs
- operations/support staff
- admins maintaining report metadata and parameters

Source-of-truth boundaries:

- report definitions and parameter metadata are partly managed in the plugin and/or `NopReports`
- underlying business data remains in ERP/MLM estates

## 3. Current Source Components

FoxPro files:

- `NISource/reports.prg`
- report-routing logic in `NISource/apiprocess.prg`

Plugin components:

- `Controllers/AdminAnniqueReportController.cs`
- report services, factories, and models under `Services/AnniqueReport/`, `Factories/AnniqueReport/`, and report domain/models
- public report views and parameter rendering components

SQL procedures/tables/databases:

- `NopReports`
- MLM and ERP report source tables referenced dynamically by report logic

## 4. Current Workflows

### Workflow A: Report metadata lookup and execution

Trigger:

- user requests a report or drill-down report output

Inputs:

- report name/system name
- report parameters and filters
- metadata from `NopReports`

Major logic steps:

- report metadata is loaded by name or drill identifier
- report-specific logic is invoked dynamically
- field metadata and output formatting are assembled

Outputs:

- rendered report payload or export

Side effects:

- consultant and operational visibility depend on correct parameter and ACL handling

Error/failure behavior:

- dynamic report execution is brittle if metadata and code drift apart

### Workflow B: Report administration

Trigger:

- admin creates, edits, deletes, or parameterizes report definitions

Inputs:

- report definition fields
- ACL and menu settings
- parameter definitions and values

Major logic steps:

- admin controller and model factory manage report metadata lifecycle
- parameters and ACL/menu settings are stored and surfaced in admin UI

Outputs:

- updated report definitions and parameter structures

Side effects:

- report discoverability and user access depend on these admin settings

Error/failure behavior:

- metadata errors can break reports without changing underlying business data

## 5. Operational Dependencies

Other domains that depend on this one:

- consultants and MLM because consultant statements/downline outputs are reporting-sensitive
- admin and back-office tooling because report management is partly an admin surface

Downstream side effects:

- operational visibility
- consultant self-service insight
- support troubleshooting capability

Upstream assumptions:

- only materially used reports should drive phase-1 scope

Timing or sequencing dependencies:

- report metadata and ACL must stay aligned with whichever identity model survives phase 1

## 6. Replacement Boundary

What must be replaced in phase 1:

- access to materially used consultant/operational reports
- enough metadata/parameter behavior to support retained reports

What can remain ERP-side or legacy-side temporarily:

- historical/rarely used reports
- report designs that are not operationally necessary for day one

What is candidate for later-phase modernization:

- full reporting portal redesign
- BI-oriented replacement beyond parity-critical operational reports

## 7. Risks And Fragility Points

- report usage is not yet fully validated, so scope can easily bloat
- dynamic metadata-driven report execution is brittle and opaque
- admin maintenance behaviour is part of the reporting surface, not just the report output itself

## 8. Open Decisions

Decision IDs:

- `D-03` which admin/back-office functions must survive in phase 1
- `D-10` which reports are actually used operationally and must survive

Assumption and dependency IDs:

- `A-02` preserve ERP/MLM truth in phase 1
- `X-DEP-04` confirmation of operationally used reports

## 9. Recommended Phase-1 Capabilities

Named capabilities required:

- reporting data service
- report metadata service
- report access-control service
- minimal report admin capability if required for live operations

Recommended service ownership boundaries:

- middleware/API layer should own report retrieval and parameter execution
- any retained admin UI should manage report metadata only where operationally justified

## 10. Evidence Base

Source files:

- `NISource/reports.prg`
- `NISource/apiprocess.prg`
- `NopCommerce - Annique/Plugins/Annique.Plugins.Nop.Customization/Controllers/AdminAnniqueReportController.cs`

Existing discovery docs:

- `docs/10_accessible_estate_and_replacement_surface.md`
- `docs/11_nisource_process_parity_matrix.md`
- `docs/13_phase1_sql_contract.md`
