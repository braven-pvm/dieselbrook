# Discovery Layer — Index

**What this folder is:** Raw discovery outputs from the live Annique DB archaeology sessions. These documents capture what we found in the existing Annique system — DB structure, stored procedures, integration patterns, business logic, and source completeness. They are the evidence base for everything in `analysis/`.

**What this folder is not:** Decisions, design, or replacement scope. Those live in `analysis/`.

> For the project-wide navigation index, see [`../README.md`](../README.md).

---

## Ground-Truth Discovery Records

These are the primary session notes from direct SQL interrogation of the live Annique DB.

| Document | Description |
|---|---|
| [`annique-discovery.1.0.md`](annique-discovery.1.0.md) | **Primary discovery record** — full session notes: DB stats, schema archaeology, row counts, stored proc analysis, live system observations. Start here for ground truth. |
| [`annique-discovery-0.1.md`](annique-discovery-0.1.md) | First-pass discovery (v0.1) — earlier session, superseded by 1.0 but retained for reference. |
| [`integration_summary.md`](integration_summary.md) | Executive-level integration summary — how AccountMate, NopCommerce, FoxPro middleware, and SOPortal interact, derived from DB evidence. |
| [`Annique_Integration_Map.md`](Annique_Integration_Map.md) | Earlier integration map (parallel to `02_integration_map.md`) — AccountMate↔NopCommerce touchpoints. |
| [`CORRECTIONS.md`](CORRECTIONS.md) | QA errata on early discovery docs — flags incorrect claims and revises figures from the initial analysis pass. |
| [`INDEX.txt`](INDEX.txt) | Original index of the DB export CSV package (tables, procs, views, etc.). |

---

## Numbered Analysis Series (Early Discovery Phase)

These documents were produced sequentially during the discovery phase. They analyse specific aspects of what was found.

| # | Document | What it covers |
|---|---|---|
| 01 | [`01_table_modules.md`](01_table_modules.md) | All 913 tables classified by module (AM, NOP, custom) with row counts |
| 02 | [`02_integration_map.md`](02_integration_map.md) | Every integration touchpoint: NOP↔AM sync paths, direction, method |
| 03 | [`03_data_tiers.md`](03_data_tiers.md) | Data volume tiers — hot/warm/cold/archive classification for migration |
| 04 | [`04_business_logic.md`](04_business_logic.md) | Key business logic found in DB: pricing rules, consultant tiers, MLM patterns, order validation |
| 05 | [`05_delivery_architecture_dieselbrook.md`](05_delivery_architecture_dieselbrook.md) | Early delivery design brief — Shopify Plus + middleware architecture, scope and phasing |
| 06 | [`06_orders_deep_dive.md`](06_orders_deep_dive.md) | Order architecture end-to-end: AM, NopCommerce, SOPortal, stored procs, status sync |
| 07 | [`07_hosting_certainty_matrix.md`](07_hosting_certainty_matrix.md) | Hosting topology evidence matrix — confirmed facts vs. evidenced vs. unknown (**confirmed 2026-03-11**) |
| 08 | [`08_hosting_questions_email_draft.md`](08_hosting_questions_email_draft.md) | Draft email re hosting/network topology — superseded by Annique confirmation 2026-03-11 |
| 09 | [`09_environment_access_inventory.md`](09_environment_access_inventory.md) | What infrastructure, DB files, and workspace artifacts are currently accessible |
| 10 | [`10_accessible_estate_and_replacement_surface.md`](10_accessible_estate_and_replacement_surface.md) | Accessible legacy estate vs. the replacement surface Dieselbrook must understand |
| 11 | [`11_nisource_process_parity_matrix.md`](11_nisource_process_parity_matrix.md) | NISource FoxPro runtime files → replacement process parity (not replicating FoxPro, capturing intent) |
| 12 | [`12_nop_customization_domain_classification.md`](12_nop_customization_domain_classification.md) | NopCommerce custom plugins classified: replace in Shopify/Dieselbrook vs. retire |
| 13 | [`13_phase1_sql_contract.md`](13_phase1_sql_contract.md) | Minimum conservative SQL access contract for Phase 1 — what queries middleware can rely on |
| 14 | [`14_campaign_module_missing_dependencies.md`](14_campaign_module_missing_dependencies.md) | Campaign module gap analysis — what else must exist for the NISource campaign module to function |
| 15 | [`15_nisource_source_completeness_matrix.md`](15_nisource_source_completeness_matrix.md) | How complete is the NISource snapshot? Matrix of process areas vs. files present |
| 16 | [`16_annique_followup_request_missing_web_assets.md`](16_annique_followup_request_missing_web_assets.md) | Draft follow-up request to Annique for missing source assets identified by gap analysis |

---

## Relationship to Analysis

The numbered documents above are **inputs** to the `analysis/` synthesis layer. The mapping is:

- `05`, `06`, `09`–`15` → directly feed `analysis/` domain packs and cross-cuts
- `01`–`04` → provide the evidence base for `analysis/01_program_analysis_baseline.md`
- `07` → feeds `analysis/16_platform_runtime_and_hosting_crosscut.md` (now confirmed)
- `13` → feeds `analysis/17_data_access_and_sql_contract_crosscut.md`

When discovery and analysis diverge, **analysis takes precedence** — it is the curated, reviewed view.
