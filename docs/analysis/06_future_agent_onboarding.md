# Future Agent Onboarding

## Purpose

This document is the restart point for future agents or contributors if work is interrupted.

It explains where the programme stands, what documents matter most, and how to continue without restarting broad discovery.

## Current Phase

The project has moved from broad discovery into synthesis.

The goal now is to organize the evidence into a stable analysis baseline, domain packs, and decision-driven replacement framing.

Do not resume with open-ended archaeology unless a targeted gap is explicitly blocking analysis.

## What Is Already Strongly Established

- the migration is primarily an integration/middleware replacement programme, not just a storefront rebuild
- major source estates are `NISource`, the custom Nop plugin, and the staging SQL estate
- staging SQL access has been validated and used to confirm key databases and objects
- order, consultant/MLM, pricing/campaign, support DB, and integration support surfaces are all materially mapped
- the newly supplied `NISource/campaign` module is relevant but incomplete as a runnable web module

## First Documents To Read

### Programme baseline

1. `docs/analysis/README.md`
2. `docs/analysis/01_program_analysis_baseline.md`
3. `docs/analysis/02_open_decisions_register.md`
4. `docs/analysis/03_workstream_decomposition.md`
5. `docs/analysis/04_assumptions_and_dependencies_register.md`

### Core replacement framing

1. `docs/10_accessible_estate_and_replacement_surface.md`
2. `docs/11_nisource_process_parity_matrix.md`
3. `docs/12_nop_customization_domain_classification.md`
4. `docs/13_phase1_sql_contract.md`

### Campaign/source-completeness findings

1. `docs/14_campaign_module_missing_dependencies.md`
2. `docs/15_nisource_source_completeness_matrix.md`
3. `docs/16_annique_followup_request_missing_web_assets.md`

### Domain work already established

1. `middleware/orders/README.md`
2. `middleware/consultants/README.md`

## Current Working Assumptions

- discovery is approximately 95 percent complete and no longer the main task
- remaining gaps should be captured as open decisions, dependencies, or narrow follow-up requests
- Dieselbrook target-state intent on Shopify is still pending for some important decisions
- therefore analysis should continue, but detailed solution specs should wait until those decisions are clearer

## How To Continue Safely

### Preferred next actions

1. Use the completed domain packs and cross-cutting synthesis docs as the current baseline.
2. Review the open decisions and assumptions registers against the phase-1 boundary summary.
3. Promote any remaining structural uncertainty into explicit decisions or dependencies rather than new discovery notes.
4. Begin early requirements shells or specification scaffolds only for areas not blocked by unresolved Dieselbrook decisions.

### Avoid

- restarting generic codebase discovery
- scattering new analysis into random top-level docs without using the synthesis structure
- writing final requirements as if Dieselbrook target-state decisions are already known

## Environment Facts Worth Remembering

- staging SQL access is confirmed to `AMSERVER-V9`
- `amanniquelive`, `compplanLive`, `compsys`, and `NopIntegration` are verified and materially used
- the local workspace does not contain a full local backup/export of every side database
- the new campaign zip contains only four files and is not a full web application source drop

## Memory Pointers

Portable memory files are committed to the repo at `docs/.agent-memory/`. Read all files in that directory before beginning any new work. They cover:

- `analysis-phase-notes.md` — synthesis phase status, doc inventory, sequencing rules
- `environment-access-notes.md` — staging SQL access facts and confirmed row counts
- `hosting-topology-notes.md` — confirmed Azure+on-prem split topology
- `notion-page-ids.md` — Notion page IDs for all maintained programme pages
- `order-flow-notes.md` — confirmed order lifecycle and idempotency facts
- `pricing-risk-notes.md` — pricing archaeology findings and recommended architecture

Session logs (one per working session) are under `docs/.agent-memory/sessions/`. Read the most recent session log to understand what was done immediately before the current restart.

Equivalent notes also exist under `/memories/repo/` for agents that can read VS Code workspace memory directly — but the `docs/.agent-memory/` copies are the portable source of record and are always up to date.

## Restart Rule

If interrupted, restart from this document and the analysis baseline documents before taking any new discovery action.