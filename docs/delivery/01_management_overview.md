# Annique Cosmetics — Platform Modernisation
## Management Overview

**Prepared by:** Dieselbrook  
**Date:** 2026-03-15  
**Version:** 1.0  
**Audience:** Dieselbrook executive team · Annique Cosmetics management team

> **Note on delivery parties:** This project involves two distinct delivery workstreams. The **middleware and Shopify integration layer** (DBM) is developed by an independent architect engaged by Dieselbrook. The **Shopify store setup, consumer front-end, and reporting** are delivered by Dieselbrook directly. Both workstreams are required for go-live.

---

## 1. What is this project?

Annique Cosmetics currently operates its online store on **NopCommerce**, a self-hosted e-commerce platform. This platform is reaching the end of its effective commercial life for Annique's needs — it cannot support the consultant-driven sales model, campaign pricing complexity, and loyalty mechanics that define how Annique sells.

This project replaces the NopCommerce storefront with **Shopify Plus**, the industry-standard enterprise e-commerce platform, while keeping Annique's existing back-office systems — **AccountMate (AM)** for ERP/financials and the **Annique Middleware (AM/the existing integration layer)** — exactly as they are.

The result is a modern, maintainable, scalable consumer-facing store that is fully integrated with Annique's existing business operations from day one.

---

## 2. What is staying the same?

It is important to be clear about what this project does **not** change:

| System | Status | Notes |
|---|---|---|
| AccountMate (ERP / financials) | **Unchanged** | Orders, consultants, pricing, and inventory continue to be mastered in AM |
| Existing AM integration layer | **Unchanged** | The existing `shopapi.annique.com` and related endpoints are consumed as-is |
| Business rules and pricing logic | **Unchanged** | Campaign pricing, consultant tiers, and exclusive items are replicated to Shopify — not redesigned |
| Consultant hierarchy and classification | **Unchanged** | SP, consultant, and customer types are read from AM and reflected in Shopify |
| PayU (payment gateway) | **Unchanged** | PayU continues as the payment provider; Shopify Plus supports it natively |

The key principle: **AccountMate remains the single source of truth for all business data.** Shopify displays and processes; AM owns.

---

## 3. What is being built?

Three things are being built or replaced:

### 3.1 Shopify Plus storefront (replaces NopCommerce)

The consumer-facing store is migrated to Shopify Plus. This includes:

- Product catalogue, variant management, and search
- Cart, checkout, and order placement
- Customer accounts (consumers and consultants)
- Consultant-exclusive pricing reflected at checkout
- Campaign pricing applied automatically at the right time
- Access restrictions for exclusive items (consultant-only products)
- PayU payment processing at checkout

Shopify Plus also replaces several NopCommerce plugins that are being retired, including the Awards module, the OTP (one-time pin) registration flow, and the Meta CAPI marketing integration.

### 3.2 Dieselbrook Middleware (DBM) — the integration engine (new build)

DBM is the system that keeps Shopify and AccountMate in sync. It is a new, purpose-built integration layer developed by Dieselbrook.

**What DBM does:**

| Function | How often | Business purpose |
|---|---|---|
| Product sync | Scheduled (full + delta) | Keeps Shopify catalogue aligned with AM item master |
| Inventory sync | Near-real-time | Prevents overselling; reflects AM stock positions |
| Image sync | On-demand | Product images managed in AM, served to Shopify |
| Order write-back | On order placement | Lodges every Shopify order into AM for fulfilment and financials |
| Order status and cancellation | Event-driven | Keeps Shopify order state consistent with AM processing state |
| Customer/consultant sync | Scheduled | Reflects AM consultant changes (activations, deactivations, tier changes) in Shopify |
| Exclusive items sync | Scheduled | Computes consultant eligibility and gatekeeps exclusive-item access |
| Campaign pricing sync | Time-bounded | Applies AM campaign prices to Shopify at exact campaign open/close boundaries |
| PayU reconciliation | Scheduled | Cross-checks Shopify orders against PayU transaction records |
| OTP and registration | On-demand | Validates consultant OTP pins through existing AM endpoints |

DBM also includes an **admin console** — a web-based operational tool for the teams running this system day-to-day. It provides sync status dashboards, manual trigger controls, reconciliation views, and exception handling.

### 3.3 Shopify custom apps and extensions

Several pieces of Annique-specific commercial logic cannot be handled by off-the-shelf Shopify features. These are built as native Shopify extensions:

- **Discount Function:** Applies the correct consultant pricing tier at checkout in real time
- **Access Gate Function:** Restricts exclusive items to eligible consultants before any product is added to cart
- **Checkout UI extensions:** Surfaces consultant account information, pricing confirmation, and registration messaging during checkout

---

## 4. How it all fits together

```
          CONSUMER / CONSULTANT
                   |
           Shopify Plus storefront
           (Checkout · Catalogue · Accounts)
                   |
         Shopify custom apps / functions
         (Consultant pricing · Access gates)
                   |
    ┌──────── Dieselbrook Middleware (DBM) ────────┐
    │  Product sync  │  Order write-back            │
    │  Pricing sync  │  Customer sync               │
    │  Inventory     │  Reconciliation              │
    └──────────────────────────────────────────────┘
                   |
          ┌────────┴────────┐
          │                 │
    AccountMate (ERP)   Existing AM APIs
    (source of truth)   (shopapi.annique.com)
```

Shopify is the consumer-facing surface. DBM is the invisible integration engine. AccountMate continues to own all business data. Nothing in the back office changes.

---

## 5. Technology choices — in plain terms

| Layer | Technology chosen | Plain-language reason |
|---|---|---|
| Integration middleware (DBM) | .NET 9 / C# | Microsoft stack; reliable, well-supported, large talent pool, excellent Azure integration |
| Shopify admin app + DBM admin console | Node.js / Remix | Native to Shopify's development toolchain; fastest path to a working admin console |
| Shopify pricing and access functions | Rust | Shopify's performance-critical function layer; Rust is the Shopify-recommended language for this |
| Checkout UI extensions | React | Shopify's standard for checkout customisation |
| Hosting | Microsoft Azure | Co-located with existing Annique infrastructure; low latency to on-premise AccountMate via private link |
| Message queue | Azure Service Bus | Reliable asynchronous delivery for order events and sync jobs |
| Secrets management | Azure Key Vault | Secure, auditable storage for all credentials and connection strings |
| Structured logging | Serilog + Application Insights | Full operational visibility; alerts and dashboards available from day one |

---

## 6. What is in scope — and what is not

This project has two parallel delivery workstreams. The table below shows which party is responsible for each area.

### Architect engagement scope (middleware and Shopify integration)

The following is delivered by the architect as a separate development engagement:

- Full product catalogue sync (items, variants, pricing, images, inventory)
- Order placement, write-back, status, and cancellation
- Consultant and customer sync including activation/deactivation lifecycle
- Campaign pricing with timed application and parity verification
- Exclusive items and consultant access gating
- Shopify custom Functions (discount pricing, access restrictions — Rust)
- Shopify checkout UI extensions (consultant pricing display, registration messaging)
- PayU payment integration and reconciliation
- OTP/SMS consultant validation
- Email marketing integration (Brevo)
- Meta CAPI marketing events
- Voucher/coupon service
- DBM admin console (operational monitoring and control)

### Dieselbrook scope (Shopify store setup, front-end, reporting)

The following is delivered by Dieselbrook directly and is not part of the architect's engagement:

- Shopify Plus store creation and platform configuration (plan subscription, store settings, payment provider setup, shipping, tax)
- Consumer-facing storefront theme design and build (what customers see)
- Reporting (NopReports migration, Shopify reporting configuration)

### Explicitly out of scope (phase 1 — neither party)

- Changes to AccountMate, its stored procedures, or its data model
- Historical data migration beyond what is needed for cutover reconciliation
- The Chatbot plugin (currently disabled in NopCommerce — not migrated)
- The Events plugin (5 events configured; not confirmed as business-critical — to be confirmed or deferred)
- Consumer-facing mobile app or native app development

---

## 7. Decisions that have been made

All major architecture and integration decisions have been resolved prior to build. These are not outstanding questions — they are settled:

| # | Decision | Resolution |
|---|---|---|
| D-01 | Platform for new storefront | **Shopify Plus** — industry standard; supports all required features natively |
| D-02 | Integration strategy | **Dieselbrook Middleware (DBM)** — bespoke integration layer, not a generic iPaaS tool |
| D-03 | AccountMate connectivity | **Azure private link or equivalent VPN** — to be confirmed technically with Annique IT (see §8) |
| D-05 | Order write-back mechanism | **Direct SQL + stored procedure calls** — reads AM contract exactly; no translation layer |
| D-06 | Pricing model handling | **AM-computed prices pushed through DBM** — AM remains authoritative; no price logic in Shopify |
| D-07 | NopCommerce plugin scope | Awards, Bookings, OTP, Gifts, Meta CAPI confirmed. Events/Chatbot deferred |
| D-09 | Admin console included | **Yes** — operational dashboard is part of phase 1 deliverable |
| D-10 | Report migration scope | Active NopReports only — Annique must confirm which are operationally used before Phase 5 |

---

## 8. What Annique still needs to confirm

Two open items require input from Annique's technical team. These are the only remaining blockers before build begins:

| # | Item | What is needed | Why it matters |
|---|---|---|---|
| TC-04 | Source of `shopapi.annique.com` | Annique IT to confirm who owns and maintains this API, and provide documentation or access | DBM consumes these endpoints. If undocumented behaviour is found during build, it creates rework. |
| TC-05 | `SyncCancelOrders.api` and `sp_ws_invoicedtickets` | Confirm ownership, documentation status, and that these are available for DBM to consume | Order cancellation and status sync depend on these. |

Additionally, Annique management should:
- Confirm the list of operationally active reports from the current NopCommerce report set
- Confirm UAT resource availability from Week 6 of the project onward (approximately 6 weeks from project start)
- Engage Annique IT to verify the private-link or VPN connectivity path between Azure hosting and on-premise AccountMate

---

## 9. Go-live timeline

| Milestone | Timing |
|---|---|
| Project kick-off / Phase 0 | Week 1 |
| Middleware platform and Shopify app foundation | Weeks 1–3 |
| Core ERP integrations (products, orders, customers) | Weeks 2–5 |
| Consultant, access, and pricing logic | Weeks 4–6 |
| Supporting integrations and admin console | Weeks 5–7 |
| UAT, hardening, and cutover preparation | Weeks 6–8 |
| **Target go-live** | **Week 8–9** |
| **Formal deadline (Annique)** | **End of July 2026** |

The July deadline provides **11–13 weeks of buffer** beyond the expected delivery window. The schedule is not the risk. The main constraint on timeline is the speed at which Annique's team can resolve TC-04, TC-05, confirm the report set, and make UAT resources available.

---

## 10. How this project is being delivered

### Who delivers what

This project involves two separate delivery parties working in parallel:

| Workstream | Delivered by | What it covers |
|---|---|---|
| Middleware development and Shopify integration | Independent architect (engaged by Dieselbrook) | DBM middleware, all ERP sync services, Shopify partner app, custom Functions, checkout extensions, DBM admin console |
| Shopify store setup and front-end | Dieselbrook | Shopify Plus store configuration, consumer storefront theme, reporting |

Both workstreams must complete for a go-live. They run in parallel and coordinate around the integration contract (API endpoints, data shapes, webhook events) which the architect defines early in Phase 0.

### Architect delivery model

The architect's engagement is delivered by a **single senior software architect directing AI development agents**. This is not a traditional multi-person software team.

The architect owns:
- All middleware architecture and technical design decisions
- All integration specifications and field mappings
- All instruction-setting and review of AI-generated code
- All verification, testing direction, and UAT coordination for the middleware scope
- All cutover planning and hypercare preparation (middleware side)

AI tools accelerate routine code generation — boilerplate, scaffold, repetitive patterns — by approximately 60–80%. The architect's judgement and verification time is unchanged for anything that carries business risk (order write-back, pricing correctness, ERP data integrity).

The practical effect: the middleware and integration scope that a traditional team would take 3–5 months to deliver is completed in **7–9 calendar weeks**. See the costing document for full detail.

---

## 11. Summary

| Item | Status |
|---|---|
| Architecture decided | ✅ All major decisions resolved |
| Technology stack selected | ✅ Confirmed: .NET 9, Shopify Plus, Node.js, Azure |
| Integration scope defined | ✅ All 25 integration services mapped |
| Delivery scope split defined | ✅ Architect: middleware + integration · Dieselbrook: Shopify setup + front-end + reporting |
| Cost and timeline modelled | ✅ Architect engagement: R200K / 7–9 weeks — see costing document |
| Open client items | ⚠️ TC-04, TC-05, report list, UAT resource availability |
| Build ready | ✅ Pending TC-04/TC-05 resolution and connectivity verification |

This project is well-specified, technically de-risked, and ready to proceed. The primary remaining action is Annique confirming the two technical open items (TC-04, TC-05) so that build can commence without ambiguity in the integration contract.

---

*Document prepared by Dieselbrook. Internal reference: `docs/delivery/01_management_overview.md`*
