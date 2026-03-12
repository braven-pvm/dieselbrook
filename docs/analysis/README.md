# Analysis Framework

## Purpose

This folder is the start of the synthesis phase.

The earlier `docs/` and `middleware/` work established discovery, archaeology, and evidence.

This folder is for turning that evidence into a controlled analysis baseline that can support:

- replacement scope decisions
- Dieselbrook alignment once target-state intent is confirmed
- later specifications and requirements
- clean continuation by future agents or contributors

## Working Rule

Do not use this folder for raw discovery notes unless the note materially changes the current-state analysis baseline.

Discovery evidence should remain in the existing source-oriented docs.

This folder should answer: what do we now believe, what do we need to replace, what is still undecided, and what is waiting on Dieselbrook.

## Recommended Reading Order

1. `01_program_analysis_baseline.md`
2. `02_open_decisions_register.md`
3. `03_workstream_decomposition.md`
4. `04_assumptions_and_dependencies_register.md`
5. `05_domain_analysis_template.md`
6. `06_future_agent_onboarding.md`
7. `07_synthesis_execution_plan.md`

## What This Folder Controls

### Macro level

- current-state program baseline
- replacement scope framing
- cross-domain workstream structure
- open decisions and dependencies
- assumptions that must be revisited later

### Domain level

This folder will eventually contain or point to canonical domain packs for:

- orders
- consultants and MLM
- products and inventory
- pricing, campaigns, and exclusives
- communications and marketing
- reporting
- identity and SSO
- back-office/admin tooling

Stage 2 canonical packs now created:

- `08_orders_domain_pack.md`
- `09_consultants_mlm_domain_pack.md`
- `10_pricing_campaigns_exclusives_domain_pack.md`

Stage 3 domain-pack coverage now created:

- `11_products_inventory_domain_pack.md`
- `12_communications_marketing_domain_pack.md`
- `13_reporting_domain_pack.md`
- `14_identity_sso_domain_pack.md`
- `15_admin_backoffice_domain_pack.md`

Cross-cutting synthesis and boundary docs now created:

- `16_platform_runtime_and_hosting_crosscut.md`
- `17_data_access_and_sql_contract_crosscut.md`
- `18_auditability_idempotency_and_reconciliation_crosscut.md`
- `19_phase1_replacement_boundary_summary.md`

### Micro level

Detailed capability-level parity work should continue to live close to the relevant domain docs, but this folder should always be the place that explains why that capability matters and how it fits into the replacement picture.

## Current Source Inputs Feeding This Folder

- `docs/05_delivery_architecture_dieselbrook.md`
- `docs/06_orders_deep_dive.md`
- `docs/09_environment_access_inventory.md`
- `docs/10_accessible_estate_and_replacement_surface.md`
- `docs/11_nisource_process_parity_matrix.md`
- `docs/12_nop_customization_domain_classification.md`
- `docs/13_phase1_sql_contract.md`
- `docs/14_campaign_module_missing_dependencies.md`
- `docs/15_nisource_source_completeness_matrix.md`
- `middleware/orders/`
- `middleware/consultants/`

## Immediate Next Use

The near-term job is not to write final specifications.

The near-term job is to stabilize:

- the current-state analysis baseline
- the replacement boundary
- the waiting-on-Dieselbrook decision set
- the workstream model that later specs will attach to

The execution order for finishing synthesis is now captured explicitly in `07_synthesis_execution_plan.md`.

## Status

Discovery is materially complete enough to enter synthesis.

Remaining discovery gaps should now be treated as:

- targeted dependency closure
- explicit open decisions
- low-noise evidence updates

They should not reset the structure of the analysis phase.