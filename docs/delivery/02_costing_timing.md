# Annique Cosmetics — Platform Modernisation
## Costing & Timing Model

**Engagement quote for:** Dieselbrook  
**Date:** 2026-03-15  
**Version:** 1.0  
**Scope:** DBM middleware development and Shopify integration layer — *not* Shopify store setup, front-end, or reporting

---

## 0. Scope split — who delivers what

This document is a **development engagement quote from the architect to Dieselbrook** for the middleware and Shopify integration scope only. It does not cover, and is not priced to include, the Shopify store configuration, consumer front-end/theme, or reporting work that Dieselbrook manages directly.

| Scope area | Delivered by | Included in this quote? |
|---|---|---|
| DBM middleware (.NET 9 backend, sync services, job scheduler, Azure hosting) | Architect | **Yes** |
| Shopify partner app (OAuth, webhook scaffold, Remix app shell) | Architect | **Yes** |
| Shopify custom Functions — discount and access gate (Rust) | Architect | **Yes** |
| Shopify checkout UI extensions (React) | Architect | **Yes** |
| DBM admin console (Remix + .NET API — operational dashboard) | Architect | **Yes** |
| All ERP integration services (product, inventory, order, customer, pricing, PayU, OTP, Brevo, Meta CAPI) | Architect | **Yes** |
| Pricing parity harness and reconciliation tooling | Architect | **Yes** |
| UAT support, defect resolution (middleware/integration scope) | Architect | **Yes** |
| Cutover preparation and hypercare planning (middleware scope) | Architect | **Yes** |
| **Shopify Plus store setup and configuration** | **Dieselbrook** | **No** |
| **Consumer storefront theme and front-end development** | **Dieselbrook** | **No** |
| **Reporting (NopReports migration / Shopify reporting)** | **Dieselbrook** | **No** |
| All platform and subscription costs (Shopify Plus, Azure, Brevo, etc.) | Annique / Dieselbrook | No — see §7 |

> **Reading this document:** All hours, costs, and timelines below refer exclusively to the architect's middleware and integration development scope. Dieselbrook's front-end, configuration, and reporting effort is a separate workstream with separate costing.

---

## 1. Delivery model

This project is delivered by a **single senior software architect directing AI development agents** — not a traditional multi-person development team.

### How the model works

The architect is the single billable resource. AI agents (large language model coding tools) are directed by the architect to generate code, scaffold services, and implement well-defined patterns. The architect's role is:

1. **Specify** — translate business and integration requirements into precise technical instructions
2. **Instruct** — direct AI agents to produce code, tests, and configuration
3. **Verify** — review every output against the integration contract; test that it behaves correctly against real data
4. **Iterate** — catch gaps or edge cases, refine, re-verify
5. **Own** — all architecture decisions, all sign-offs, all UAT coordination

The AI agents perform no business reasoning. They generate code from instructions. The architect's judgement is applied to everything that touches Annique's data, money, or operations.

### What AI accelerates — and what it does not

| Work type | AI compression | Reason |
|---|---|---|
| Boilerplate and scaffold (middleware core, API shell, test structure) | High (~80%) | Well-defined patterns with no business ambiguity |
| Repetitive sync services (product, inventory, image, customer) | High (~70–80%) | Each follows the same delta-sync pattern; verified once, replicated |
| Order write-back and ERP integration | Low (~20–30%) | AccountMate stored procedure contract must be verified against live system; architect-intensive |
| Campaign pricing sync + boundary sweeps | Medium (~40–50%) | Commercially sensitive; pricing parity harness must be verified exhaustively |
| DBM admin console UI | Medium (~50–60%) | UI scaffold is fast; data correctness and edge-case handling are verified manually |
| UAT, integration testing, cutover | Low (~20–30%) | Client-dependent; real-world testing time does not compress significantly |
| Architecture design and field mapping | Low (~20%) | Judgement work; AI assists but architect decides |

The model's efficiency comes from removing the overhead of coordination, context-switching, code review cycles between team members, and ramp-up time for junior or mid-level engineers. The architect applies full attention to every component from day one.

---

## 2. Architect time breakdown

**Billing rate: R850 / hour**

All times below are **architect billable hours** — the time spent specifying, instructing, verifying, and iterating. Not clock time on a calendar; actual billable work.

### 2.1 By component

| Component | Architect hours | Risk level | Notes |
|---|---|---|---|
| Architecture finalisation and field mapping | 10–14h | Low | Mostly complete; remaining mapping from DB Structure and CSV archaeology |
| Middleware solution skeleton + CI/CD + environments | 4–6h | Low | Scaffold from instructions; architect reviews structure and conventions |
| Shopify app bootstrap + OAuth + webhook scaffold | 4–6h | Low | Shopify CLI scaffolds this; architect verifies app config and secrets |
| Product sync service (full + delta) | 4–6h | Medium | Instruct + verify against `icitem` field mapping |
| Inventory sync service | 3–5h | Medium | Standard delta pattern; verify reconciliation edge cases |
| Image sync service | 2–3h | Low | Straightforward; verify URL handling and idempotency |
| **Order write-back service** | **14–18h** | **High** | Highest business risk — SP contract, consultant classification, audit trail all verified manually |
| Order cancellation + status services | 8–12h | High | Audit-critical; every state transition verified independently |
| Customer + consultant sync | 6–8h | Medium | Identity and lifecycle mapping verified against `arcust` |
| Exclusive items sync | 4–6h | Medium | `soxitems` → customer metafields; verify access gate behaviour |
| Campaign pricing sync + boundary sweeps | 12–16h | High | Oracle correctness and timed transitions verified exhaustively |
| Pricing parity harness | 6–8h | High | Test harness must catch AM price drift — verification is the bulk of this work |
| Shopify Function — discount (Rust) | 6–8h | Medium | Logic is deterministic but Rust output needs review and Function deploy testing |
| Shopify Function — access gate (Rust) | 4–6h | Medium | Same pattern; smaller scope |
| Checkout UI + theme extensions | 4–6h | Low | Verify consultant messaging and pricing display |
| PayU reconciliation | 6–8h | Medium | Financial control — verify against PayU data format |
| OTP + SMS + registration validation | 4–6h | Medium | Verify endpoint parity with current `shopapi.annique.com` behaviour |
| Brevo, Meta CAPI, voucher services | 5–7h | Medium | Three separate integrations; each verified independently |
| DBM admin console (Remix + .NET API) | 16–22h | Medium | UI scaffold fast; data APIs and business logic views take time to verify |
| Reconciliation dashboard + support tooling | 4–6h | Medium | Verify data accuracy and alert trigger logic |
| UAT support, defect fixing, and hardening | 24–32h | High | Client-driven testing; defect resolution is real work regardless of AI tooling |
| Cutover preparation + hypercare planning | 8–12h | High | Checklist, parallel-run plan, rollback triggers — architect-owned |
| **Subtotal (base estimate)** | **~168–225h** | | |

### 2.2 Contingency

A **20% contingency** is applied to the base estimate. This covers:

- Rework cycles when ERP edge cases are discovered during verification
- Undocumented AccountMate stored procedure branches that require re-mapping
- Client-driven scope refinements identified during UAT
- TC-04 / TC-05 resolution if the endpoints contain undocumented business logic

| Scenario | Base hours | With 20% contingency |
|---|---|---|
| Optimistic | 168h | **~200h** |
| Midpoint | 188h | **~235h** |
| Pessimistic | 225h | **~270h** |

---

## 3. Cost scenarios

**Billing rate: R850 / hour (single architect resource)**

| Scenario | Architect hours | Total delivery cost |
|---|---|---|
| Optimistic | 200h | **R170,000** |
| Midpoint | 235h | **R199,750** |
| Pessimistic | 270h | **R229,500** |

### Recommended budget figure

> **R200,000** delivery fee (midpoint scenario, inclusive of all contingency)
>
> This is the number to budget. Hours beyond 270h would require a formal change order with documented cause. Hours below 235h would be billed as actuals — the client benefits from unused contingency.

---

## 4. Calendar timeline

The architect's effective working day is approximately 6 hours of billable architect time (the remainder goes to administrative, communication, and review time not charged to the project).

| Scenario | Architect hours | Working days (at 6h/day) | Calendar weeks | Buffer to July deadline |
|---|---|---|---|---|
| Optimistic | 200h | ~33 days | **~7 weeks** | **13 weeks buffer** |
| Midpoint | 235h | ~39 days | **~8 weeks** | **12 weeks buffer** |
| Pessimistic | 270h | ~45 days | **~9 weeks** | **11 weeks buffer** |

The July 2026 deadline is extremely comfortable. Even under the pessimistic scenario, delivery completes with **11 weeks to spare**.

The critical constraint is not build velocity — it is **client-dependency items**. The items most likely to affect the timeline are:

- Annique IT confirming TC-04 (`shopapi.annique.com` source and documentation)
- Annique IT confirming TC-05 (`SyncCancelOrders.api` and `sp_ws_invoicedtickets`)
- Annique IT confirming the private-link / VPN connectivity path to AccountMate
- Annique confirming the operationally active report set (report migration scope)
- Annique making UAT resources available from Week 6 onward

None of these block the start of build. They do become critical-path items in Phase 4–5 if unresolved.

---

## 5. Phased delivery timeline (midpoint — ~8 weeks)

| Phase | Description | Architect hours | Calendar weeks |
|---|---|---|---|
| Phase 0 | Discovery closure, connectivity verification, solution definition | 16–20h | Week 1 |
| Phase 1 | Middleware platform foundation (DBM skeleton, CI/CD, environments) | 12–16h | Weeks 1–2 |
| Phase 2 | Shopify app foundation (OAuth, webhooks, Remix scaffold) | 10–14h | Weeks 2–3 (parallel with Phase 1 tail) |
| Phase 3 | Core ERP integrations (products, inventory, images, orders, customers) | 40–54h | Weeks 2–5 |
| Phase 4 | Consultant, access gate, and campaign pricing | 36–48h | Weeks 4–6 (parallel with Phase 3 tail) |
| Phase 5 | Supporting integrations + DBM admin console | 32–44h | Weeks 5–7 (parallel with Phase 4 tail) |
| Phase 6 | UAT, hardening, cutover preparation, and hypercare planning | 32–44h | Weeks 6–8 |

Phases 3 through 6 overlap by design. The high-risk integrations (order write-back, pricing) begin early and run in parallel with lower-risk work. UAT begins before all development is complete — a standard parallel-track approach.

---

## 6. Comparison: AI-assisted vs. traditional delivery

For reference, the same scope delivered under a traditional multi-person model:

| Delivery model | Effort | Rate basis | Estimated cost |
|---|---|---|---|
| Traditional — 1 senior developer | 76–115 developer days | R900–R1,200/hr market rate | R3,000,000 – R5,500,000 |
| Traditional — 2-person team (1 senior + 1 mid) | ~100 combined days | Blended ~R850/hr | R2,500,000 – R4,000,000 |
| **AI-assisted architect-led (this model)** | **200–270 architect hours** | **R850/hr** | **R170,000 – R230,000** |

The AI-assisted model delivers the same scope at **approximately 10–15% of traditional cost**. The primary driver is compression of low-risk boilerplate and scaffolding work. High-risk, verification-intensive components (ERP write-back, pricing correctness, UAT) take similar architect time regardless of model.

This comparison is the commercial case for the delivery approach.

---

## 7. Platform and ongoing costs

These costs are **not** part of this engagement quote. They are recurring operational costs that are Dieselbrook's and Annique's responsibility to procure and budget for. The architect has no billing relationship with any of these platforms.

| Platform / service | Monthly cost (indicative) | Notes |
|---|---|---|
| **Shopify Plus** | **~USD 2,300/month** | Enterprise plan; billed monthly or annually. Dominates the platform budget. |
| Azure App Service (DBM middleware hosting) | USD 200–600/month | Depends on tier; includes Service Bus queue |
| Azure SQL (DBM state database) | USD 50–150/month | Basic to Standard tier depending on load |
| Azure Key Vault (secrets management) | USD 5–20/month | Negligible |
| Brevo (email/SMS marketing) | USD 25–300/month | Depends on contact volume and usage tier |
| Structured logging (Serilog + Application Insights) | USD 0–150/month | Azure consumption model or Seq on-premise |
| **Total platform (indicative)** | **~USD 2,600–3,500/month** | Dominated by Shopify Plus |

At current exchange rates, total platform cost is approximately **R47,000–R63,000/month**. This is a recurring operating cost, not a project cost.

> Shopify Plus pricing: Shopify's enterprise plan pricing should be confirmed directly with Shopify for Annique's specific requirements. The USD 2,300 figure is the standard published rate for plans up to USD 800K monthly GMV.

---

## 8. What is excluded from this quote

The following are explicitly outside the architect's engagement scope. Items marked **[Dieselbrook]** are expected to be handled by Dieselbrook as part of their own project workstream:

- **Shopify Plus store setup and configuration** **[Dieselbrook]** — Plan subscription, store settings, payment provider configuration (PayU), shipping zones, tax settings, and Shopify admin configuration
- **Consumer storefront theme and front-end development** **[Dieselbrook]** — Visual design, brand implementation, Shopify theme build, and all UX on the consumer-facing storefront
- **Reporting** **[Dieselbrook]** — NopReports migration analysis, Shopify reporting configuration, and any custom report build
- **Content migration** — Blog posts, static content pages, and media not covered by product catalogue sync
- **Historical order data migration** — Archive of NopCommerce historical orders
- **Staff training and change management** — Internal training for Annique or Dieselbrook teams on Shopify Plus operations
- **Post-launch feature development** — Any feature added after Phase 1 go-live requires a new engagement

> The architect's deliverable is a **fully functional middleware and integration layer** that, when combined with Dieselbrook's Shopify store setup and front-end work, produces a live and integrated Shopify Plus store. Both workstreams are required for go-live.

---

## 9. Budget summary

> **This is the architect's quote to Dieselbrook for the middleware and Shopify integration development scope only.** Shopify Plus setup, consumer front-end, and reporting are separate Dieselbrook workstreams not included below.

| Item | Amount | Type |
|---|---|---|
| Architect engagement — optimistic | R170,000 | Once-off (actuals at R850/hr) |
| **Architect engagement — recommended budget** | **R200,000** | **Once-off (actuals at R850/hr)** |
| Architect engagement — pessimistic ceiling | R230,000 | Once-off (actuals at R850/hr) |
| Platform costs (Shopify Plus, Azure, etc.) | ~R47,000–R63,000/month | Recurring — Dieselbrook / Annique |
| Traditional model comparison (full middleware scope) | R2,500,000–R5,500,000 | For reference only |

### Payment / billing structure

The engagement is billed at **R850/hour as actuals**, invoiced against recorded architect hours. The R200,000 figure represents the budgeted ceiling at midpoint hours. A formal change order would be required for any work beyond 270 hours.

Billing cadence should be agreed at project kick-off. Typical structures for this model: weekly actuals invoice, or milestone-gated invoice at phase completions (Phase 2, Phase 4, Phase 6).

---

*Architect engagement quote — middleware and Shopify integration scope. Prepared for Dieselbrook. Internal reference: `docs/delivery/02_costing_timing.md`*
