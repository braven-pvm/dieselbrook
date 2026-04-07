# 03 — Workspace Guide
## What is in Every Folder and When to Use It

**Last updated:** 2026-04-07

---

## Workspace Root Layout

```
F:\Repositories\dieselbrook\
├── AnqIntegrationApiSource/     ← Existing .NET 9 API (AnniqueAPI) — architectural reference
├── NopCommerce - Annique/       ← NopCommerce source — being replaced, read-only reference
├── NISource/                    ← Legacy VFP middleware — behavioural reference for DBM
├── database/                    ← DB assets (NopCommerce backup + SQL Server installer)
├── docs/                        ← All documentation (analysis, discovery, specs, delivery)
│   ├── onboarding/              ← START HERE (this folder)
│   ├── analysis/                ← 26 analysis documents
│   ├── delivery/                ← Commercial documents (management overview, costing)
│   ├── discovery/               ← Discovery phase source material
│   ├── reference/               ← Client artifacts (NDA, requirements, SQL job schedules)
│   ├── spec/                    ← Specification phase output (WHERE NEW SPECS GO)
│   ├── .agent-memory/           ← Portable agent memory files
│   └── DB Structure/            ← Database schema CSVs (tables, columns, procs, views)
└── middleware/                  ← Domain design docs for orders and consultants
    ├── orders/                  ← Orders domain design (8 docs)
    └── consultants/             ← Consultants/MLM domain design (13 docs)
```

---

## Folder-by-Folder Reference

### `docs/onboarding/` — Start Here
All agent onboarding documents. Read in order 00 → 01 → 02 → 03 → 04.

| File | Purpose |
|---|---|
| `00_START_HERE.md` | Primary briefing — read first, every time |
| `01_project_context.md` | Programme background, stakeholders, history |
| `02_technical_architecture.md` | Systems, stacks, APIs, databases, topology |
| `03_workspace_guide.md` | This file |
| `04_session_state_april_2026.md` | Current state, open decisions, next steps |

---

### `docs/analysis/` — Analysis Phase Output (26 documents)

These are the core analytical artefacts from discovery and analysis phases. Do not modify these without a specific reason; they represent confirmed findings.

| File | Purpose | When to read |
|---|---|---|
| `README.md` | Navigation map for analysis docs | When orienting |
| `01_program_analysis_baseline.md` | Baseline facts and programme view | Always — foundational |
| `02_open_decisions_register.md` | All open decisions (D-xx, TC-xx, A-xx, OI-xx) | Before writing any spec |
| `03_workstream_decomposition.md` | Work breakdown structure | When planning|
| `04_assumptions_and_dependencies_register.md` | Dependencies register | When planning |
| `05_domain_analysis_template.md` | Template for domain analysis | If creating a new domain pack |
| `06_future_agent_onboarding.md` | Previous onboarding doc (superseded by this folder) | Historical context only |
| `07_synthesis_execution_plan.md` | Synthesis phase plan | Historical context |
| `08_orders_domain_pack.md` | Orders domain — full analysis | When working on orders |
| `09_consultants_mlm_domain_pack.md` | Consultants/MLM domain | When working on consultants |
| `10_pricing_campaigns_exclusives_domain_pack.md` | Pricing and campaigns | When working on pricing |
| `11_products_inventory_domain_pack.md` | Products and inventory | When working on product sync |
| `12_communications_marketing_domain_pack.md` | Brevo, WhatsApp, email | When working on comms |
| `13_reporting_domain_pack.md` | Reporting (Dieselbrook scope) | Reference only |
| `14_identity_sso_domain_pack.md` | SSO, OTP, auth | When working on identity |
| `15_admin_backoffice_domain_pack.md` | Admin console, back-office | When working on admin |
| `16_platform_runtime_and_hosting_crosscut.md` | Platform, runtime, topology | When planning deployment |
| `17_data_access_and_sql_contract_crosscut.md` | SQL access contracts | When designing DB layer |
| `18_auditability_idempotency_and_reconciliation_crosscut.md` | Idempotency, retries, audit | When designing sync services |
| `19_phase1_replacement_boundary_summary.md` | What is in/out of phase 1 | When scoping work |
| `20_pricing_engine_deep_dive.md` | Campaign pricing deep dive | When working on pricing |
| `21_pricing_access_supplement.md` | Pricing access/security | When working on pricing |
| `22_decision_gates_supplement.md` | Decision gates supplement | When reviewing decisions |
| `23_shopify_plus_requirements_analysis.md` | Shopify Plus requirements | When working on Shopify integration |
| `24_dbm_am_interface_contract.md` | DBM ↔ AccountMate interface | When designing AM integration |
| `25_reporting_deep_dive.md` | Reporting deep dive | When working on reporting |
| `26_solution_overview_scope_cost.md` | Comprehensive technical reference | Frequent reference — covers all 25 services |

**Next analysis document:** `27_...` (if a new analysis doc is needed)

---

### `docs/delivery/` — Commercial Documents

| File | Audience | Purpose |
|---|---|---|
| `01_management_overview.md` | Dieselbrook + Annique management | Non-technical programme overview |
| `02_costing_timing.md` | Dieselbrook (architect's engagement quote) | Architect pricing to Dieselbrook |

Also published to Notion (parent page ID: `324c65e5-d3c2-81cb-8035-dcf5dc3c2f3a`).

---

### `docs/discovery/` — Discovery Phase Source Material (17 docs)

Read-only reference. These were the working documents from the discovery phase (Feb–Mar 2026). Do not add to this folder.

Key files if you need background:

| File | Key content |
|---|---|
| `01_table_modules.md` | AccountMate table → business module mapping |
| `02_integration_map.md` | Full integration surface map |
| `05_delivery_architecture_dieselbrook.md` | Target architecture sketch |
| `06_orders_deep_dive.md` | Orders workflow deep dive |
| `07_hosting_certainty_matrix.md` | Infrastructure certainty matrix |
| `10_accessible_estate_and_replacement_surface.md` | What can be replaced |
| `11_nisource_process_parity_matrix.md` | VFP process → replacement surface map |
| `13_phase1_sql_contract.md` | Phase 1 SQL access contract |
| `Annique_Integration_Map.md` | Client-supplied integration map |
| `annique-discovery.1.0.md` | Discovery session transcript/notes |

---

### `docs/reference/` — Client Artifacts

Read-only. Provided by Annique/Dieselbrook.

| File | Content |
|---|---|
| `Dieselbrook_NDA_Annique_Project.pdf` | NDA |
| `User Requirements - Annotated - 19 Feb.docx` | Annotated user requirements |
| `Accountmate -SQL Jobs for Dieselbrook.xlsx` | AccountMate SQL Agent job schedule |
| `NOP-SQL Jobs for Dieselbrook.xlsx` | NopCommerce SQL Agent job schedule |
| `WS1 - Campaigns.docx` | Campaign functional spec |
| `WS1 - Item Master.docx` | Item master functional spec |
| `campaign.zip` | Campaign module source (4 files, partial) |

---

### `docs/spec/` — Specification Phase Output

**This is where new specifications go.** Currently contains only conventions.

| File | Purpose |
|---|---|
| `00_spec_conventions.md` | Authoring conventions — **read before writing any spec** |
| `README.md` | Spec folder guide |

New spec documents go in `docs/spec/domains/<domain-slug>/` with these files per domain:
- `spec.md` — master specification
- `state_machine.md` — lifecycle states (where relevant)  
- `api_contract.md` — endpoint/interface definitions
- `reconciliation_rules.md` — idempotency, retry, dead-letter guidance
- `acceptance_criteria.md` — testable acceptance statements

---

### `docs/DB Structure/` — Database Schema CSVs

Schema exports from AccountMate and NopCommerce databases. Used during discovery.

| File | Contents |
|---|---|
| `01_tables.csv` | Table list with row counts |
| `01_stored_procedures.csv` | Stored procedure inventory |
| `02_columns.csv` | Column definitions |
| `02_view_definitions.csv` | View SQL definitions |
| `03_foreign_keys.csv` | Foreign key relationships |
| `04_indexes.csv` | Index definitions |
| `04_triggers.csv` | Trigger definitions |
| `05_stored_procs.csv` | Stored procedure details |
| `06_views.csv` | View object list |

---

### `docs/.agent-memory/` — Portable Agent Memory

These files are committed to the repo and survive restarts. Maintained alongside sessions.

| File | Purpose |
|---|---|
| `README.md` | How to use agent memory |
| `analysis-phase-notes.md` | Phase status and doc inventory |
| `environment-access-notes.md` | Staging SQL access facts, confirmed DB row counts |
| `hosting-topology-notes.md` | Confirmed Azure + on-prem topology |
| `notion-page-ids.md` | Notion page IDs |
| `order-flow-notes.md` | Confirmed order lifecycle and idempotency |
| `pricing-risk-notes.md` | Pricing archaeology results |
| `sessions/2026-03-13.md` | Session log for March 13 2026 |

---

### `middleware/` — Domain Design Documents

Purpose-built design work for the two most complex domains.

#### `middleware/orders/` (8 docs)
| File | Purpose |
|---|---|
| `README.md` | Orders domain guide |
| `01_current_state_order_flow.md` | How orders flow today (NopCommerce → AM) |
| `02_target_order_middleware_design.md` | How orders will flow via DBM |
| `03_order_data_models.md` | Order data model definitions |
| `04_shopify_order_interface.md` | Shopify order webhook contracts |
| `05_order_business_rules.md` | Order processing business rules |
| `06_staging_order_inventory.md` | Order estate confirmed on staging |
| `07_order_replacement_touchpoint_map.md` | Every touchpoint DBM must replace |

#### `middleware/consultants/` (13 docs)
| File | Purpose |
|---|---|
| `README.md` | Consultants domain guide |
| `01_current_state_consultant_domain.md` | How consultants work today |
| `02_target_consultant_middleware_design.md` | How DBM handles consultants |
| `03_consultant_data_models.md` | Consultant data model |
| `04_shopify_consultant_interface.md` | Shopify customer ↔ consultant interface |
| `05_consultant_business_rules.md` | Entitlement and access rules |
| `06_commissions_reports_boundary.md` | What stays in AM (commissions) |
| `07_mlm_data_dependency_map.md` | `compplanLive` dependencies |
| `08_nisource_replacement_worklist.md` | VFP→DBM replacement items |
| `09_phase1_phase2_scope_split.md` | Phase 1 vs phase 2 consultant scope |
| `10_compsys_dependency_map.md` | `compsys` database dependencies |
| `11_staging_mlm_inventory.md` | MLM objects confirmed on staging |
| `12_phase1_staging_object_trace.md` | Object trace for phase 1 |
| `13_phase1_implementation_backlog.md` | Phase 1 backlog items |

---

### `AnqIntegrationApiSource/` — Architectural Reference (.NET 9)

**Read this to understand the patterns DBM should follow.**

| Path | Purpose |
|---|---|
| `AnqIntegrationApi.csproj` | Project file — `net9.0`, EF Core 9.0.7, SQL Server |
| `Program.cs` | Startup — JWT, Serilog, EF Core, multi-DB registration |
| `appsettings.json` | Config structure (connection strings redacted in source) |
| `DbContexts/AccountMateDbContext.cs` | EF Core mapping for AccountMate tables |
| `DbContexts/NopDbContext.cs` | EF Core mapping for NopCommerce tables |
| `DbContexts/OutboxDbContext.cs` | Outbox pattern for reliable messaging |
| `DbContexts/ApiSettingsDbContext.cs` | Settings/configuration context |
| `Services/JwtService.cs` | JWT token generation pattern |
| `Services/ClientDbContextFactory.cs` | Multi-tenant DB context factory |
| `Services/ApiClientProvider.cs` | API client resolution |
| `Services/Outbox/` | Full outbox pattern implementation |
| `Services/Workers/` | Background workers (BrevoOutboxWorker, WhatsAppOptinEmailWorker) |
| `Controllers/AuthController.cs` | JWT auth endpoint pattern |
| `Models/AM/` | AccountMate entity models |
| `Models/Nop/` | NopCommerce entity models |
| `BrevoApiHelpers/` | Brevo CRM helper library (subproject) |
| `Middleware/ApiClientContextMiddleware.cs` | HTTP context middleware pattern |

Key patterns to reuse in DBM:
- Multi-DB EF Core context factory (`ClientDbContextFactory`)
- Outbox pattern for reliable external calls (`Services/Outbox/`)
- Background worker pattern (`Services/Workers/`)
- JWT auth (`Services/JwtService.cs` + `Controllers/AuthController.cs`)

---

### `NISource/` — Legacy VFP Middleware (Behavioural Reference)

**Read this to understand WHAT DBM must do, not HOW.**

| File | What it reveals |
|---|---|
| `nopintegrationmain.prg` | Application entry point and routing |
| `syncproducts.prg` | Product sync logic — live/staging config, AM connection strings |
| `syncorders.prg` | Order import to AM (sosord, sostrs, soskit tables) |
| `syncconsultant.prg` | Consultant sync from `arcust` |
| `syncorderstatus.prg` | Order status reverse-sync from AM to NopCommerce |
| `apiprocess.prg` | Main API handler — OTP, linked server calls, CompPlan functions |
| `appprocess.prg` | App-level request processing |
| `amdata.prg` | AccountMate data access class (wwSQL wrapper) |
| `basedata.prg` | Generic SQL ORM base class |
| `brevoprocess.prg` | Brevo CRM operations |
| `ssoclass.prg` | SSO session handling |
| `nopapi.prg` | NopCommerce API interface |
| `syncclass.prg` | Sync state machine base |
| `reports.prg` | Reporting operations |
| `campaign/Campaign_class.prg` | Campaign admin class |

---

### `NopCommerce - Annique/` — Legacy Storefront (Reference Only)

**Read-only.** Key items if needed:

| Path | Purpose |
|---|---|
| `Plugins/Annique.Plugins.Nop.Customization/` | Custom plugin with all Annique-specific logic |
| `Plugins/Annique.Plugins.Payments.AdumoOnline/` | PayU payment plugin |
| `Libraries/Nop.Core/` | NopCommerce core library |
| `Libraries/Nop.Data/` | NopCommerce data layer |

---

### `database/`

| Path | Purpose |
|---|---|
| `AnniqueNOP.bak` | NopCommerce production DB backup (7.5 GB, 06/03/2026) |
| `sqlmedia/Express_ENU/SETUP.EXE` | **SQL Server 2025 Express** installer (v17.0.1000.7) |
| `sqlmedia/Express_ENU/SqlServerInstallConfig.ini` | SQL Server install configuration |

---

## Task-to-Document Routing

| If you are working on... | Read first... | Then read... |
|---|---|---|
| Orders domain | `middleware/orders/README.md` | `docs/analysis/08_orders_domain_pack.md` |
| Consultant/MLM | `middleware/consultants/README.md` | `docs/analysis/09_consultants_mlm_domain_pack.md` |
| Pricing / campaigns | `docs/analysis/10_pricing_campaigns_exclusives_domain_pack.md` | `docs/analysis/20_pricing_engine_deep_dive.md` |
| Products / inventory | `docs/analysis/11_products_inventory_domain_pack.md` | `NISource/syncproducts.prg` |
| Identity / OTP / SSO | `docs/analysis/14_identity_sso_domain_pack.md` | `NISource/ssoclass.prg`, `apiprocess.prg` |
| Admin console | `docs/analysis/15_admin_backoffice_domain_pack.md` | `docs/analysis/26_solution_overview_scope_cost.md §4` |
| Shopify integration | `docs/analysis/23_shopify_plus_requirements_analysis.md` | `docs/analysis/24_dbm_am_interface_contract.md` |
| DBM deployment/hosting | `docs/analysis/16_platform_runtime_and_hosting_crosscut.md` | `docs/.agent-memory/hosting-topology-notes.md` |
| SQL access patterns | `docs/analysis/17_data_access_and_sql_contract_crosscut.md` | `AnqIntegrationApiSource/DbContexts/` |
| Idempotency / retry | `docs/analysis/18_auditability_idempotency_and_reconciliation_crosscut.md` | `AnqIntegrationApiSource/Services/Outbox/` |
| Writing a new spec | `docs/spec/00_spec_conventions.md` | `docs/analysis/02_open_decisions_register.md` |
| DBM architecture patterns | `AnqIntegrationApiSource/Program.cs` | `AnqIntegrationApiSource/Services/` |
| Brevo / CRM | `docs/analysis/12_communications_marketing_domain_pack.md` | `AnqIntegrationApiSource/BrevoApiHelpers/` |
| AccountMate tables | `AnqIntegrationApiSource/DbContexts/AccountMateDbContext.cs` | `docs/DB Structure/02_columns.csv` |

---

*Continue reading: [04_session_state_april_2026.md](04_session_state_april_2026.md)*
