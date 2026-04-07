# START HERE — Agent Onboarding Briefing
## Dieselbrook Middleware (DBM) — Annique Cosmetics Programme

**Last updated:** 2026-04-07  
**Current phase:** BUILD — PRD and architecture specifications starting  
**Read this first.** Then continue with documents 01–04 in this folder.

---

## 1. What You Are Working On

You are working on the **Dieselbrook Middleware (DBM)** — a new, purpose-built integration engine that connects **Shopify Plus** (replacing the existing NopCommerce storefront) to **AccountMate (AM)**, Annique Cosmetics' existing ERP system.

This is not a storefront project. It is a **platform and integration replacement programme**. The storefront is a consequence; the hard work is the integration.

**The single most important architectural principle:**  
AccountMate remains the single source of truth for all business data. DBM reads from AM and writes back to AM. Shopify displays and processes. AM owns.

---

## 2. The Three Parties

| Party | Role | Builds |
|---|---|---|
| **Annique Cosmetics** | Client and ERP owner | Nothing (AM stays unchanged) |
| **Dieselbrook** | System integrator, project manager | Shopify store setup, theme/UX, reporting |
| **Architect (you)** | Engaged by Dieselbrook | DBM middleware + Shopify integration layer |

**Important:** Your quotes and scope go TO Dieselbrook, not directly to Annique. Dieselbrook is your client.

---

## 3. What DBM Does (in 10 lines)

| Sync function | Direction | Frequency |
|---|---|---|
| Product catalogue + pricing | AM → Shopify | Scheduled (full + delta) |
| Inventory levels | AM → Shopify | Near-real-time |
| Consultant pricing (campaigns) | AM → Shopify | Time-bounded delta |
| Exclusive item eligibility | AM → Shopify | Scheduled |
| Customer/consultant sync | AM → Shopify | Scheduled |
| Order write-back | Shopify → AM | On order placement |
| Order status + cancellation | AM → Shopify | Event-driven |
| OTP / registration | Shopify → AM endpoints | On-demand |
| Admin console | Internal | Web UI for operators |

---

## 4. Current Phase Status

| Phase | Status |
|---|---|
| Discovery (source archaeology, SQL, topology) | ✅ Complete |
| Analysis (domain packs, decisions, cross-cutting) | ✅ Complete |
| Delivery documents + Notion publishing | ✅ Complete |
| AccountMate cloud migration advisory | ✅ Complete |
| **PRD — Product Requirements Document** | 🔜 Starting now |
| **Architecture + technical specifications** | 🔜 Starting now |
| DBM build | ⬜ Not started |

---

## 5. Five Things You Must Know Before Starting Any Work

1. **Discovery is closed.** Do not restart broad codebase exploration. All major facts are documented.
2. **`NISource/` is the behavioural reference**, not the target. It is the legacy VFP middleware that DBM replaces. Read it to understand WHAT to build, not HOW.
3. **`AnqIntegrationApiSource/` is the architectural reference** — the existing .NET 9 / EF Core API (AnniqueAPI). DBM follows the same patterns (JWT, Serilog, EF Core, SQL Server).
4. **AccountMate must not be modified.** No schema changes, no new stored procedures unless explicitly approved.
5. **Spec documents go in `docs/spec/domains/<domain>/`** following the conventions in `docs/spec/00_spec_conventions.md`.

---

## 6. Essential Reading (in order)

### Immediate context
1. `docs/onboarding/01_project_context.md` — Programme background, stakeholders, history, delivery model
2. `docs/onboarding/02_technical_architecture.md` — All systems, stacks, APIs, databases, topology
3. `docs/onboarding/03_workspace_guide.md` — What is in every folder and when to use it
4. `docs/onboarding/04_session_state_april_2026.md` — Current state, open decisions, immediate next steps

### Analysis baseline (read if working on analysis-level questions)
5. `docs/analysis/01_program_analysis_baseline.md`
6. `docs/analysis/02_open_decisions_register.md` ← always check before beginning spec work
7. `docs/analysis/19_phase1_replacement_boundary_summary.md`
8. `docs/analysis/26_solution_overview_scope_cost.md` ← comprehensive technical reference

### Domain packs (read the one relevant to your task)
- Orders: `docs/analysis/08_orders_domain_pack.md` + `middleware/orders/`
- Consultants/MLM: `docs/analysis/09_consultants_mlm_domain_pack.md` + `middleware/consultants/`
- Pricing/Campaigns: `docs/analysis/10_pricing_campaigns_exclusives_domain_pack.md`
- Products/Inventory: `docs/analysis/11_products_inventory_domain_pack.md`
- Communications: `docs/analysis/12_communications_marketing_domain_pack.md`
- Identity/SSO/OTP: `docs/analysis/14_identity_sso_domain_pack.md`
- Admin/Backoffice: `docs/analysis/15_admin_backoffice_domain_pack.md`

### Specification phase
- Conventions: `docs/spec/00_spec_conventions.md` ← read before writing any spec

---

## 7. Key Facts Quick Reference

| Fact | Value |
|---|---|
| Target storefront | Shopify Plus |
| ERP | AccountMate (SQL Server) — unchanged |
| DBM target stack | .NET 9 / ASP.NET Core / EF Core 9 / SQL Server |
| Live AM server | `172.19.16.100:1433` · `amanniquelive` |
| Staging AM server | `196.3.178.122:62111` · `amanniquelive` |
| AM server named instance | `AMSERVER-v9` |
| NopCommerce Azure | `20.87.212.38:63000` |
| Architect rate | R850/hr |
| Architect budget | R200K (7–9 calendar weeks) |
| Deployment target | Azure (same VNet as existing AnniqueAPI) |

---

## 8. Workspace Navigation (30-second map)

```
docs/onboarding/        ← YOU ARE HERE. Read all 5 files first.
docs/analysis/          ← 26 analysis docs. Numbered. Start with 01, 02, 19, 26.
docs/delivery/          ← Commercial docs (management overview, costing/timing)
docs/discovery/         ← Source material from discovery phase (read-only reference)
docs/spec/              ← Specification phase output (where new specs go)
middleware/             ← Domain design docs for orders and consultants
NISource/               ← Legacy VFP source (behaviour reference)
AnqIntegrationApiSource/ ← Existing .NET 9 API (architectural pattern reference)
NopCommerce - Annique/  ← NopCommerce source (being replaced)
database/               ← AnniqueNOP.bak (7.5 GB) + SQL Server 2025 installer
docs/reference/         ← Client artifacts (NDA, user requirements, SQL jobs)
docs/.agent-memory/     ← Portable agent memory (Notion IDs, session logs, etc.)
```

---

## 9. Rules

- **Do not modify AccountMate** (no schema changes, no new stored procs without explicit approval)
- **Discovery is complete** — do not restart broad codebase archaeology
- **New analysis docs** must be numbered sequentially in `docs/analysis/` (next = `27_...`)
- **Spec documents** must follow the conventions in `docs/spec/00_spec_conventions.md`
- **Check `docs/analysis/02_open_decisions_register.md`** before writing any spec — blocked items must not be resolved arbitrarily
- **Security:** `sa` credentials are hardcoded in NISource VFP files — do NOT reproduce these in new code; use environment-based secrets management
- **NopCommerce source** is provided for reference only; do not build on it

---

*Continue reading: [01_project_context.md](01_project_context.md)*
