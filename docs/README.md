# Annique → Shopify | Discovery & Analysis — Navigation Index

**Project:** Annique Cosmetics MLM platform → Shopify Plus migration  
**Client:** Annique / Delivered by: Dieselbrook  
**Doc layer:** Navigation index — start here  
**Last updated:** 2026-04-07

---

## ⭐ New Agents: Start Here

The `docs/onboarding/` folder contains a complete 5-document onboarding sequence created April 2026. Read it before anything else:

1. [`onboarding/00_START_HERE.md`](onboarding/00_START_HERE.md) — Primary briefing
2. [`onboarding/01_project_context.md`](onboarding/01_project_context.md) — Programme background
3. [`onboarding/02_technical_architecture.md`](onboarding/02_technical_architecture.md) — Systems map
4. [`onboarding/03_workspace_guide.md`](onboarding/03_workspace_guide.md) — Doc/folder guide
5. [`onboarding/04_session_state_april_2026.md`](onboarding/04_session_state_april_2026.md) — Current state + next steps

---

## How This Folder Is Organised

```
docs/
  onboarding/  ← START HERE — agent briefing (April 2026)
  discovery/   ← What we found (DB archaeology, current-state analysis)
  analysis/    ← What it means (synthesis, domain packs, decisions, registers)
  delivery/    ← Commercial documents (management overview, costing/timing)
  spec/        ← What we'll build (spec phase — in progress)
  reference/   ← Raw client artifacts (requirements docs, workshop notes, SQL jobs)
  DB Structure/ ← Raw SQL schema export CSVs (~12 MB, primary evidence base)
  middleware/  ← (workspace root) Implementation design docs for the replacement
```

Use the sections below to find the right document for your need.

---

## Layer 0 — Onboarding: Start Here

**Location:** `onboarding/`

Created April 2026. Complete programme context for any agent or contributor starting fresh.

---

## Layer 1 — Discovery: What We Found

**Location:** `discovery/`

Raw outputs from the live Annique DB archaeology sessions. Ground-truth current-state evidence — DB structure, stored procedures, integration touchpoints, business logic extracted from the SQL schema.

> **See [`discovery/README.md`](discovery/README.md) for the full annotated index.**

Quick-find:

| I need to understand… | Document |
|---|---|
| The full DB — stats, structure, what's in there | [`annique-discovery.1.0.md`](discovery/annique-discovery.1.0.md) |
| All 913 tables by module | [`01_table_modules.md`](discovery/01_table_modules.md) |
| How AM, NopCommerce, SOPortal are connected | [`02_integration_map.md`](discovery/02_integration_map.md) |
| Data volumes and migration tiers | [`03_data_tiers.md`](discovery/03_data_tiers.md) |
| Business logic found in the DB | [`04_business_logic.md`](discovery/04_business_logic.md) |
| Order flow end-to-end | [`06_orders_deep_dive.md`](discovery/06_orders_deep_dive.md) |
| Hosting topology (confirmed) | [`07_hosting_certainty_matrix.md`](discovery/07_hosting_certainty_matrix.md) |
| NISource FoxPro replacement parity | [`11_nisource_process_parity_matrix.md`](discovery/11_nisource_process_parity_matrix.md) |
| NopCommerce plugins — keep vs. retire | [`12_nop_customization_domain_classification.md`](discovery/12_nop_customization_domain_classification.md) |
| Phase 1 SQL access contract | [`13_phase1_sql_contract.md`](discovery/13_phase1_sql_contract.md) |
| Campaign module gap analysis | [`14_campaign_module_missing_dependencies.md`](discovery/14_campaign_module_missing_dependencies.md) |
| What source files we have access to | [`09_environment_access_inventory.md`](discovery/09_environment_access_inventory.md) |

---

## Layer 2 — Analysis: What It Means

**Location:** `analysis/`

Structured synthesis built on top of discovery. This is the curated, decision-ready layer — domain packs, registers, cross-cuts, and the Phase 1 replacement boundary.

> **See [`analysis/README.md`](analysis/README.md) for the recommended reading order and full index.**

Quick-find:

| I need to understand… | Document |
|---|---|
| Current programme baseline (anchor point) | [`analysis/01_program_analysis_baseline.md`](analysis/01_program_analysis_baseline.md) |
| Open decisions blocking scope/design | [`analysis/02_open_decisions_register.md`](analysis/02_open_decisions_register.md) |
| Working assumptions and external dependencies | [`analysis/04_assumptions_and_dependencies_register.md`](analysis/04_assumptions_and_dependencies_register.md) |
| **Orders domain** — data model, replacement boundary | [`analysis/08_orders_domain_pack.md`](analysis/08_orders_domain_pack.md) |
| **Consultants/MLM domain** — identity, tiers, sponsor | [`analysis/09_consultants_mlm_domain_pack.md`](analysis/09_consultants_mlm_domain_pack.md) |
| **Pricing/campaigns domain** — how prices are determined | [`analysis/10_pricing_campaigns_exclusives_domain_pack.md`](analysis/10_pricing_campaigns_exclusives_domain_pack.md) |
| **Products/inventory domain** — catalog sync, stock | [`analysis/11_products_inventory_domain_pack.md`](analysis/11_products_inventory_domain_pack.md) |
| **Communications/marketing domain** — OTP, SMS, email | [`analysis/12_communications_marketing_domain_pack.md`](analysis/12_communications_marketing_domain_pack.md) |
| **Reporting domain** — metadata-driven report defs | [`analysis/13_reporting_domain_pack.md`](analysis/13_reporting_domain_pack.md) |
| **Identity/SSO domain** — storefront identity, OTP, roles | [`analysis/14_identity_sso_domain_pack.md`](analysis/14_identity_sso_domain_pack.md) |
| **Admin/backoffice domain** — internal operational surfaces | [`analysis/15_admin_backoffice_domain_pack.md`](analysis/15_admin_backoffice_domain_pack.md) |
| Hosting/infra cross-cut (confirmed Azure + on-prem) | [`analysis/16_platform_runtime_and_hosting_crosscut.md`](analysis/16_platform_runtime_and_hosting_crosscut.md) |
| SQL access contract cross-cut | [`analysis/17_data_access_and_sql_contract_crosscut.md`](analysis/17_data_access_and_sql_contract_crosscut.md) |
| Auditability/idempotency/reconciliation cross-cut | [`analysis/18_auditability_idempotency_and_reconciliation_crosscut.md`](analysis/18_auditability_idempotency_and_reconciliation_crosscut.md) |
| Phase 1 replacement boundary (frozen) | [`analysis/19_phase1_replacement_boundary_summary.md`](analysis/19_phase1_replacement_boundary_summary.md) |
| Pricing engine deep dive (AM pricing model, Shopify paths) | [`analysis/20_pricing_engine_deep_dive.md`](analysis/20_pricing_engine_deep_dive.md) |
| Pricing access update (AM-as-oracle model) | [`analysis/21_pricing_access_supplement.md`](analysis/21_pricing_access_supplement.md) |
| Decision gates and open questions | [`analysis/22_decision_gates_supplement.md`](analysis/22_decision_gates_supplement.md) |

---

## Layer 3 — Spec: What We'll Build

**Location:** `spec/`

Specification phase — translates analysis into field maps, API contracts, and state machines. The hierarchy is scaffolded but **not yet written**. See [`spec/README.md`](spec/README.md) for the planned structure.

---

## Layer 4 — Implementation Design

**Location:** `middleware/` (workspace root, outside `docs/`)

The most implementation-ready layer. Design docs for the replacement middleware across two active workstreams:

- `middleware/orders/` — order flow, data model, Shopify interface, staging inventory
- `middleware/consultants/` — consultant domain, commission/MLM, phase/scope split, implementation backlog

---

## Raw Source Material

| Location | Contents |
|---|---|
| [`reference/`](reference/) | Client-provided artifacts: user requirements doc, workshop notes, SQL Agent jobs, NDA |
| [`DB Structure/`](DB%20Structure/) | Raw SQL schema CSV exports (~12 MB) — primary evidence base for all discovery |

---

## Status Snapshot (as of 2026-03-13)

| Layer | Status |
|---|---|
| Discovery | Complete — all major DB areas covered |
| Analysis — domain packs | Complete (8 domains) |
| Analysis — cross-cuts | Complete (hosting confirmed, SQL contract set, auditability defined) |
| Analysis — registers | Live (decisions and assumptions actively maintained) |
| Spec | Not started — scaffolded only |
| Implementation (middleware) | In progress — orders and consultants workstreams active |

---