# Consultants Middleware Workstream

This folder breaks the consultant domain into the parts Dieselbrook will need to preserve, redesign, and integrate for the Shopify migration.

## Core Finding

Consultants are not just storefront users.

In the current Annique stack, a consultant is a combined business entity spanning:

- an AccountMate consultant/customer record in `arcust`
- a NopCommerce customer with the `Annique Consultant` role
- consultant-specific profile and activation metadata
- consultant-only checkout and entitlement behavior
- exclusive items, starter kits, awards, bookings, and private account UI
- MLM hierarchy, downline, commission, and report dependencies that remain rooted in AccountMate

That means Shopify customer records alone are not sufficient as the full replacement model.

## Document Set

| File | Purpose |
|---|---|
| `01_current_state_consultant_domain.md` | What the current source code and plugin customizations do today for consultants |
| `02_target_consultant_middleware_design.md` | How Dieselbrook should model and replicate the consultant domain in the new middleware |
| `03_consultant_data_models.md` | ERP records, custom Nop plugin tables, and proposed middleware consultant model |
| `04_shopify_consultant_interface.md` | How consultant identity and entitlements should surface in Shopify |
| `05_consultant_business_rules.md` | The consultant-specific rules that must be preserved or deliberately replaced |
| `06_commissions_reports_boundary.md` | What should remain ERP-side vs what should surface in Shopify for commissions and reports |
| `07_mlm_data_dependency_map.md` | What currently touches the MLM-side SQL database surface and why it matters for scope |
| `08_nisource_replacement_worklist.md` | Concrete `NISource` replacement worklist for consultant, MLM, and report touchpoints |
| `09_phase1_phase2_scope_split.md` | Phase split between parity-critical middleware replacement and optional MLM re-platforming |
| `10_compsys_dependency_map.md` | What currently touches `compsys` and what Dieselbrook must replace vs leave ERP-side |
| `11_staging_mlm_inventory.md` | Confirmed staging inventory for `compplanLive`, `compsys`, and `NopIntegration` |
| `12_phase1_staging_object_trace.md` | Phase-1 consultant/MLM replacement objects confirmed directly on staging |
| `13_phase1_implementation_backlog.md` | Concrete phase-1 build backlog derived from staging-confirmed consultant and support objects |

## Primary Source Inputs

- `NISource/syncconsultant.prg`
- `NISource/syncorders.prg`
- `docs/04_business_logic.md`
- `docs/02_integration_map.md`
- `docs/integration_summary.md`
- `NopCommerce - Annique/Plugins/Annique.Plugins.Nop.Customization/**`

## Important Current-State Conclusions

- consultant sync currently runs from AccountMate into NopCommerce, not the other way around
- NopCommerce uses a custom plugin to add consultant-only profile, checkout, registration, awards, exclusive items, reports, and account behavior
- consultant activation is tied to first paid order logic in the storefront plugin and to `dstarter` in AccountMate
- order import can reactivate consultants in AM via `sp_ws_reactivate`
- MLM hierarchy, downline, and rebate calculation are still AccountMate-native processes

## Immediate Design Bias

For Dieselbrook, the safest design assumption is:

- Shopify customer = storefront identity
- middleware consultant profile = operational consultant integration model
- AccountMate = source of truth for consultant number, lifecycle state, sponsor/upline, and commission-affecting state

That is materially different from treating consultants as ordinary ecommerce customers.