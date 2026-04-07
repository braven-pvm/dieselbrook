# 01 — Project and Programme Context
## Annique Cosmetics Platform Modernisation

**Last updated:** 2026-04-07

---

## 1. About Annique Cosmetics

Annique Cosmetics is a South African direct-sales cosmetics company. Its business model is built on a network of **consultants** — independent salespeople who buy products at a discounted price and sell to end-consumers. This is an MLM (multi-level marketing) hierarchy: consultants have upline sponsors and downline recruits, and their purchasing entitlements, pricing tiers, and exclusive product access are all determined by their classification in AccountMate.

The existing storefront (`annique.co.za`) runs NopCommerce, a self-hosted open-source e-commerce platform. This has been heavily customised to support Annique's consultant model, campaign pricing, awards system, and OTP-based login.

---

## 2. Why This Project Exists

NopCommerce is reaching the end of its effective commercial life for Annique's needs:

- The codebase is aged, heavily customised, and difficult to maintain
- The consultant-driven pricing model (campaign pricing, discount tiers, exclusive items) is fragile and requires ongoing VFP middleware maintenance
- The integration layer (NISource, a VFP Web Connection server) is not maintainable at scale
- Shopify Plus is the industry standard for their size and model and is significantly cheaper to run

The programme replaces NopCommerce + NISource with **Shopify Plus** + **DBM (Dieselbrook Middleware)**.

---

## 3. The Delivery Parties

### Annique Cosmetics (client)
- Owns AccountMate (ERP, financials, consultant records, pricing, inventory)
- Owns the existing NopCommerce site and databases
- Owns `shopapi.annique.com` (the existing integration API)
- AccountMate **stays exactly as-is** — this is a non-negotiable constraint

### Dieselbrook (system integrator)
- Prime contractor for the programme delivery
- Responsible for: Shopify Plus store setup, storefront theme/UX, and reporting
- Engaged the architect to deliver DBM
- Contacts: Dieselbrook executive team (programme owners)

### Architect (you, working in this workspace)
- Engaged by Dieselbrook under an NDA (`docs/reference/Dieselbrook_NDA_Annique_Project.pdf`)
- Responsible for: DBM middleware build + Shopify integration layer (custom apps, Shopify Functions)
- Architect's engagement rate: R850/hr
- Architect's estimated scope: R170K–R230K, recommended budget R200K (7–9 calendar weeks)
- Quotes go TO Dieselbrook (Dieselbrook then prices to Annique)

---

## 4. Programme History

### Phase 0 — Discovery (Feb–Mar 2026)
Systematic source code and database archaeology to understand the full replacement surface:
- Mapped NISource VFP codebase (20+ `.prg` files)
- Reverse-engineered NopCommerce custom plugin
- Accessed staging SQL estate (`AMSERVER-v9`) to confirm table structures and stored procedures
- Identified all databases: `amanniquelive`, `compplanLive`, `compsys`, `NopIntegration`
- Mapped 25+ sync and integration surfaces
- Output: `docs/discovery/` (16 discovery docs + source material)

### Phase 1 — Analysis (Mar 2026)
Structured synthesis of discovery into actionable analysis:
- 8 domain packs (orders, consultants/MLM, pricing/campaigns, products, communications, reporting, identity, admin)
- 3 cross-cutting synthesis docs (platform/hosting, data access/SQL, auditability/idempotency)
- Phase-1 boundary summary (what is in scope for first delivery)
- Open decisions register (active tracking)
- Pricing deep-dives (campaign pricing architecture, effective price oracle)
- Shopify Plus requirements analysis
- DBM/AM interface contract
- Output: `docs/analysis/` (26 analysis docs)

### Phase 2 — Commercial Documents (Mar 2026)
- Solution overview, scope and cost doc (`docs/analysis/26_solution_overview_scope_cost.md`)
- Management overview (`docs/delivery/01_management_overview.md`)
- Costing and timing model (`docs/delivery/02_costing_timing.md`)
- Published all three to Notion

### Phase 3 — Infrastructure Advisory (Apr 2026)
- Investigated AccountMate cloud migration (can it move to Azure?)
- Source code archaeology confirmed: AM uses plain SQL Server tables + stored procs
- Key finding: Azure IaaS VM lift-and-shift is viable. Azure SQL PaaS is not needed.
- Identified SQL Server 2025 installer already in workspace (`database/sqlmedia/`)
- See `docs/analysis/` for any formal migration plan doc (to be created if needed)

### Phase 4 — BUILD (April 2026 onwards) ← **CURRENT PHASE**
- PRD (Product Requirements Document) — starting now
- Architecture and technical specifications
- DBM implementation

---

## 5. The DBM Scope Boundary

This is precise. The architect builds exactly this:

### Architect scope (DBM)
- DBM sync engine (.NET 9 background service + API)
- All sync functions (product, inventory, order, consultant, pricing, exclusive items, OTP)
- DBM admin console (Node.js web app for operational visibility)
- Shopify custom app (Shopify Functions, webhooks, extensions)
- Deployment infrastructure on Azure

### Dieselbrook scope (NOT architect scope)
- Shopify Plus account setup and configuration
- Shopify storefront theme and consumer UX
- Reporting and BI layer
- Any business intelligence or dashboards for Annique management

### Out of scope for this programme (unchanged)
- AccountMate ERP platform and database
- `shopapi.annique.com` existing endpoints (consumed as-is)
- PayU payment gateway (Shopify Plus supports it natively)
- Consultant commission calculations (ERP-side, phase 2+)

---

## 6. Key Decisions Still Open (as of April 2026)

These decisions were outstanding after analysis. Do not resolve them unilaterally — check with Dieselbrook/Annique before writing specs that depend on them.

| ID | Decision | Blocker impact |
|---|---|---|
| D-01 | Consultant account model on Shopify (tags+metafields vs B2B) | Blocks account, entitlement, and pricing architecture |
| D-02 | Confirm consultant pricing architecture (metafields + Shopify Function) | Blocks pricing spec |
| D-06 | Shopify plan tier + app distribution model (Plus confirmed?) | Blocks Shopify Function viability |
| D-03 | OTP delivery for registration (existing `shopapi.annique.com` vs new) | Blocks identity spec |
| D-07 | Awards/loyalty module — migrate to DBM or retire? | Blocks communications scope |
| TC-02 | Go-live date and cutover model | Blocks delivery plan |
| TC-03 | SMS provider for OTP delivery | Blocks OTP spec |
| TC-04 | `shopapi.annique.com` source code and ownership | Blocks OTP replacement plan |

Full register: `docs/analysis/02_open_decisions_register.md`

---

## 7. Key Source Material Available

### Client reference documents (`docs/reference/`)
- `Dieselbrook_NDA_Annique_Project.pdf` — NDA between Dieselbrook and Annique
- `User Requirements - Annotated - 19 Feb.docx` — annotated requirements document
- `Accountmate -SQL Jobs for Dieselbrook.xlsx` — AccountMate SQL Agent job schedule
- `NOP-SQL Jobs for Dieselbrook.xlsx` — NopCommerce SQL Agent job schedule
- `WS1 - Campaigns.docx` — Annique campaign functional spec
- `WS1 - Item Master.docx` — Item master management spec

### Pricing confirmed facts
- Effective consultant price formula: `CampDetail.nPrice` if active campaign row exists, else `icitem.nprice` (flat 20% discount)
- Pricing oracle stored procedure: `sp_camp_getSPprice`
- Delta sync trigger: `CampDetail.dLastUpdate` + `Campaign.dLastUpdate` + mandatory boundary sweep every 5–15 min
- Shopify implementation: middleware precomputes effective prices → metafields → Shopify Function at checkout

---

## 8. Notion Workspace

All documents are also published to Notion for stakeholder visibility.

| Notion page | Purpose | Page ID |
|---|---|---|
| Programme root | Annique → Shopify Requirements Delivery | `0a6c65e5d3c2831b9457016b7bac2ce7` |
| Decision Gates | Open decisions, tech confirmations, resolution log | `5b0c65e5d3c283a08569017e4e536399` |
| Solution Overview parent | Contains all 3 delivery docs | `324c65e5-d3c2-81cb-8035-dcf5dc3c2f3a` |
| Pricing access page | Pricing archaeology supplement | `31ec65e5d3c281b58984e44bb2328c11` |

Full ID registry: `docs/.agent-memory/notion-page-ids.md`

---

*Continue reading: [02_technical_architecture.md](02_technical_architecture.md)*
