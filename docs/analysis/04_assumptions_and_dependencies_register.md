# Assumptions And Dependencies Register

## Purpose

This register captures the current working assumptions and external dependencies that analysis is relying on.

It keeps temporary beliefs explicit so they do not silently harden into unchallenged requirements.

## Assumptions Register

| ID | Assumption | Why We Are Assuming It | Risk If Wrong | Review Trigger |
|---|---|---|---|---|
| A-01 | Shopify is the target storefront/platform direction | existing delivery architecture and programme framing assume this | medium | Dieselbrook changes target platform direction |
| A-02 | Phase 1 should preserve ERP/MLM truth rather than re-platforming it | current evidence supports parity-first replacement | high | Dieselbrook asks for deeper ERP/MLM modernization in phase 1 |
| A-03 | Consultants remain a distinct operating model | current business and source evidence strongly support this | high | Dieselbrook confirms a materially simpler customer model |
| A-04 | Campaign pricing will need middleware-owned effective-state logic | current ERP and plugin evidence show high pricing complexity | high | Dieselbrook confirms a different pricing strategy |
| A-05 | The current `NISource` snapshot is useful but not a full web-app source checkout | campaign module gap analysis strongly supports this | medium | Annique supplies the missing web shell and proves completeness |
| A-06 | `NISource/campaign` belongs to the legacy estate and matters to pricing/admin scope | code style and database/procedure usage strongly support this | low | evidence shows it was an abandoned or unrelated module |
| A-07 | ~~Namibia awareness should remain in the analysis~~ | **SUPERSEDED** — Dieselbrook has confirmed Namibia is out of scope entirely. All A-07 references in domain packs are retired. | — | — |
| A-08 | Phase-1 middleware will have private network connectivity to AccountMate SQL estate | all high-risk domains (orders, consultants, pricing, products, inventory) require direct SQL access to ERP; this is a hard delivery constraint exposed by the platform hosting cross-cut | high | infrastructure verification shows no reliable private path to ERP SQL, forcing a different architecture |
| A-09 | ~~Azure-compatible middleware deployment is the working default hosting assumption~~ | **CONFIRMED 2026-03-11** — Annique-supplied topology diagram confirms the current middleware and commerce tier is Azure-hosted; AccountMate ERP (`AMSERVER-v9`) is on-premises with private routing from the Azure tier. Assumption promoted to confirmed fact. | — | superseded by topology confirmation |

## External Dependencies Register

| ID | Dependency | Type | Why It Matters | Owner |
|---|---|---|---|---|
| X-DEP-01 | Dieselbrook final Shopify solution intent | programme decision dependency | needed before locking detailed solution specs | Dieselbrook |
| X-DEP-02 | Annique response on missing web shell/assets | source completeness dependency | affects confidence in admin/back-office UI analysis | Annique |
| X-DEP-03 | Continued staging SQL access | verification dependency | needed for targeted schema/object validation | Current project team |
| X-DEP-04 | Confirmation of operationally used reports | business validation dependency | affects reporting replacement scope | Annique + Dieselbrook |
| X-DEP-05 | Confirmation of back-office tools required for phase 1 | scope dependency | determines whether admin UI must be rebuilt | Dieselbrook |
| X-DEP-06 | ~~Hosting and infrastructure topology confirmation~~ | **RESOLVED 2026-03-11** — Annique supplied a topology diagram confirming: (1) middleware and NopCommerce tier is Azure-hosted; (2) AccountMate (`AMSERVER-v9`) and related DBs are on-premises; (3) existing private routing connects the Azure tier to the on-premises ERP tier. Architecture decision is unblocked. Remaining follow-up: confirm exact VPN/private-link mechanism so Dieselbrook middleware can join same routing path. | infrastructure verification dependency | resolved |

## Usage Rule

When a future document relies on one of these assumptions or dependencies, reference the relevant ID rather than restating the entire rationale.