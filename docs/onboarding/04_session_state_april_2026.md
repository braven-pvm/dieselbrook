# 04 — Session State, April 2026
## Where We Are, What Is Done, What Comes Next

**Last updated:** 2026-04-07

---

## 1. Programme State Summary

As of April 7, 2026, the programme has completed all discovery and analysis phases and is transitioning into active development. The next major deliverables are:

1. **PRD** — Product Requirements Document for DBM
2. **Architecture specification** — Full technical architecture for DBM
3. **Domain specifications** — Per-domain technical specs in `docs/spec/domains/`
4. **DBM build** — Implementation of the Dieselbrook Middleware

---

## 2. Completed Work

### Discovery phase (Feb–Mar 2026) ✅
- Full source code archaeology across NISource (VFP), AnqIntegrationApiSource (.NET 9), NopCommerce custom plugin
- Staging SQL access confirmed (`196.3.178.122:62111` → `AMSERVER-v9`)
- All databases confirmed and row-counted: `amanniquelive`, `compplanLive`, `compsys`, `NopIntegration`
- 16 discovery documents created (`docs/discovery/`)

### Analysis phase (Mar 2026) ✅
- 26 analysis documents covering all 8 domains + 3 cross-cutting concerns
- Open decisions register (`docs/analysis/02_open_decisions_register.md`)
- Phase-1 boundary summary (`docs/analysis/19_phase1_replacement_boundary_summary.md`)
- Pricing engine deep dive — confirmed architecture (metafields + Shopify Function)
- Shopify Plus requirements analysis
- DBM/AM interface contract
- Consultants and orders domain packs + detailed middleware design docs

### Commercial documents (Mar 2026) ✅
- Solution overview, scope and cost (`docs/analysis/26_solution_overview_scope_cost.md`) — published to Notion
- Management overview (`docs/delivery/01_management_overview.md`) — published to Notion
- Costing and timing model (`docs/delivery/02_costing_timing.md`) — published to Notion
- All three docs published as child pages under Notion parent `324c65e5-d3c2-81cb-8035-dcf5dc3c2f3a`

### Infrastructure investigation (Apr 2026) ✅
- AccountMate cloud migration advisory completed
- Source code confirms AM = plain SQL Server + stored procs, no proprietary engine
- Azure IaaS VM lift-and-shift confirmed viable for AM migration
- SQL Server 2025 Express installer present in workspace (`database/sqlmedia/`)
- Finding: `sa` credentials hardcoded in VFP source — must not be reproduced in DBM

### Onboarding documents (Apr 2026) ✅ — just created
- `docs/onboarding/` folder created with 5 documents (00–04)
- Repo memory created at `/memories/repo/dieselbrook_project.md`

---

## 3. Open Decisions

**These decisions must be resolved before writing the specs that depend on them.**  
Check with Dieselbrook before proceeding with blocked items.

### Shopify + consultant model decisions
| ID | Decision | Blocks |
|---|---|---|
| D-01 | Consultant account model on Shopify (tags+metafields vs B2B) | Account spec, entitlement spec, pricing spec |
| D-02 | Confirm recommended pricing architecture (precomputed metafields + Shopify Function) | Pricing spec, campaign sync spec |
| D-06 | Shopify plan tier + app distribution model (Plus requires confirmation) | Shopify Function viability, custom app distribution |

### Scope and feature decisions
| ID | Decision | Blocks |
|---|---|---|
| D-03 | OTP/registration — replace via DBM or continue using existing `shopapi.annique.com`? | Identity spec |
| D-07 | Awards/loyalty module — migrate, rebuild, or retire? | Communications scope |
| D-09 | Which legacy admin/back-office tools must survive in phase 1? | Admin console spec |
| D-10 | Other feature scope items (see decisions register) | Various |

### Infrastructure and timeline
| ID | Decision | Blocks |
|---|---|---|
| TC-02 | Go-live date and cutover model | Delivery plan, phasing |
| TC-03 | SMS provider for OTP delivery | OTP spec |
| TC-04 | `shopapi.annique.com` source code and ownership | OTP replacement plan |
| TC-05–TC-08 | Various infrastructure confirmations | Deployment spec |

### Pricing open items (unresolved after archaeology)
| ID | Item | Blocks |
|---|---|---|
| OI-8 | Public-app Shopify Functions viability on Advanced plan (vs Plus only) | Distribution model decision |
| OI-9 | VAT-inclusive vs VAT-exclusive price sync | Pricing spec |
| OI-10 | Negative-price voucher rows — how to map to Shopify discount objects | Pricing spec |
| OI-11 | Parity harness: middleware pre-computed price vs `sp_camp_getSPprice` oracle | Integration testing |

Full register: `docs/analysis/02_open_decisions_register.md`

---

## 4. What Is NOT Blocked — Start Here

These domains can proceed to specification now without waiting for open decisions:

### ✅ Can spec now
- **Product and inventory sync** — No decision dependencies; AM to Shopify, well-understood
- **Order write-back** — Core order table mapping confirmed; idempotency pattern confirmed
- **Order status reverse-sync** — `SOPortal` flow confirmed (`' '` → `'P'` → `'S'` → Shopify)
- **Brevo/communications** — Pattern exists in AnniqueAPI; Brevo lib available in workspace
- **Admin console** — Can define operational requirements independently of Shopify decisions
- **Platform/hosting** — Azure deployment spec, topology confirmed
- **Idempotency and outbox** — Cross-cutting; pattern exists in AnniqueAPI

### ⚠️ Blocked until decisions resolved
- **Consultant checkout pricing** — Blocked on D-01, D-02, D-06
- **OTP and registration** — Blocked on D-03, TC-03, TC-04
- **Awards module** — Blocked on D-07
- **Exclusive items access gate** — Blocked on D-01 (depends on customer model)

---

## 5. Confirmed Technical Facts (as of April 2026)

These are facts, not assumptions. They must be treated as ground truth.

| Fact | Source |
|---|---|
| Azure + on-prem split topology confirmed | Annique diagram (2026-03-11) |
| DBM deploys in Azure, uses existing private routing to AM | Topology confirmation |
| AccountMate = standard SQL Server tables + stored procs | Source code + EF Core mapping |
| `sp_camp_getSPprice` is the authoritative pricing oracle | SQL archaeology |
| Pricing: effective consultant price = CampDetail.nPrice (campaign) or icitem.nprice (20% fallback) | SQL archaeology |
| `AMSERVER-v9` is the staging SQL server name | `@@SERVERNAME` confirmed via sqlcmd |
| Staging is accessible at `196.3.178.122:62111` | Direct access confirmed |
| Shopify Function availability = Shopify Plus only (custom app) | Shopify documentation |
| B2B native tooling is NOT the correct architecture | Analysis conclusion |
| SQL Server 2025 installer is present in workspace | `database/sqlmedia/Express_ENU/SETUP.EXE` v17.0.1000.7 |
| `sa`/`AnniQu3S@` is hardcoded in VFP source — must NOT be used in DBM | Source code grep |
| AntiqueAPI (`AnqIntegrationApiSource`) is a separate service from DBM | Architecture confirmed |
| `shopapi.annique.com` = NISource VFP server (TC-04 — ownership to confirm) | Best evidence |
| OTP endpoint (`/otpGenerate.api`) is HIGH RISK to replace | SQL Agent job evidence |

---

## 6. Immediate Next Steps (Agent Task Queue)

Work to do now, in recommended order:

### Step 1 — PRD (Product Requirements Document)
Create `docs/spec/00_prd.md` (or similar) covering:
- Programme purpose and goals
- DBM functional requirements (all sync functions)
- Non-functional requirements (reliability, latency, throughput)
- Phase 1 vs phase 2 scope split
- Definition of done per domain
- Decision dependency mapping

### Step 2 — DBM Architecture Specification
Create `docs/spec/01_dbm_architecture.md` covering:
- System architecture diagram
- Technology choices and rationale (.NET 9, EF Core, Azure)
- Service decomposition (sync engine, API, workers, admin console)
- Database schema for DBM's own DB (idempotency ledger, outbox, settings)
- Security architecture (secrets management, JWT, scopes)
- Deployment specification (Azure Container Apps vs Azure App Service vs VM)
- Observability (Serilog, health checks, metrics)

### Step 3 — Domain specifications (unblocked ones first)
Follow `docs/spec/00_spec_conventions.md`. Create per-domain folders:
- `docs/spec/domains/products/`
- `docs/spec/domains/inventory/`
- `docs/spec/domains/orders/`
- `docs/spec/domains/order-status/`
- `docs/spec/domains/communications/`
- `docs/spec/domains/admin/`

### Step 4 — Blocked domains (after decisions)
Once D-01/D-02/D-06 resolved:
- `docs/spec/domains/consultant-pricing/`
- `docs/spec/domains/consultant-accounts/`
- `docs/spec/domains/exclusive-items/`

---

## 7. AccountMate Migration Status

This is a separate advisory track (not the same as DBM build). Current state:

| Question | Answer |
|---|---|
| Can AM move to Azure IaaS VM? | Yes — plain SQL Server, no proprietary engine |
| Can AM use Azure SQL PaaS? | No — vendor confirmed not supported |
| Recommended path | Azure IaaS: Windows Server 2022 VM + SQL Server 2019/2022 |
| Compatibility validation needed | Ask vendor: does AM work on SQL Server 2019 compat level 150? |
| Credentials in current source | `sa`/`AnniQu3S@` — must be replaced with least-privilege service account |
| SQL Server 2025 installer | Available in workspace — someone has already prepared it |

No formal migration plan document has been created yet. If needed, create `docs/analysis/27_accountmate_cloud_migration.md`.

---

## 8. Delivery Budget and Timeline Tracking

| Parameter | Value |
|---|---|
| Architect rate | R850/hr |
| Architect budget (recommended) | R200K |
| Estimated hours | 160–270 hours (7–9 calendar weeks) |
| Hours consumed to date | Pre-build analysis/documentation (not yet tracked formally) |
| Go-live target | TC-02 open — date not confirmed |

---

## 9. Session Log Pointer

A detailed session log covering work up to April 7, 2026, is available in the conversation summary maintained by the agent. Key session logs:
- `docs/.agent-memory/sessions/2026-03-13.md` — March 13 session (topology confirmation, pricing reframe, D-06 rewrite)
- Previous sessions documented in conversation history

When starting any new session, this document (`04_session_state_april_2026.md`) is your entry point for current state. If significant new work is done, create `docs/.agent-memory/sessions/<date>.md` to log it.

---

*End of onboarding sequence. You are ready to begin PRD and architecture work.*
