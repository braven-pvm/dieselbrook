# Platform Runtime And Hosting Cross-Cut

## Purpose

This document consolidates the platform-runtime and hosting implications that cut across all domain packs.

## Current Working Position

- the legacy estate is a confirmed split architecture, not a single-host storefront application
- middleware and commerce tier is **confirmed Azure-hosted** — Annique topology diagram (2026-03-11) shows NopCommerce, Nop SQL Server, AnniqueAPI, and Nopintegration all running in an Azure cloud tier
- AccountMate ERP is **confirmed on-premises** — topology diagram shows `AMSERVER-v9` on the private/on-premises side, with `AccountMate DB`, `Related DB`, data warehouse, and a `Remote SQL Server (IP Restricted)` co-located
- existing private routing connects the Azure middleware tier to the on-premises ERP/data tier; this routing path is what current middleware uses to reach `172.19.16.100`
- private SQL reachability is a hard delivery constraint, not a deployment preference
- Dieselbrook middleware goes in Azure; joining the existing private routing to `AMSERVER-v9` is the connectivity action required before build

## What Is Already Strongly Established

- `nopintegration.annique.com` opens direct SQL connections to both the commerce-side SQL estate and live AccountMate
- the public integration tier and the ERP/data tier should be treated as separate trust and connectivity zones
- `NISource` currently combines application hosting, orchestration, data access, provider integration, and page rendering in one runtime family

## Cross-Domain Implications

### Orders, consultants, and pricing

- these domains depend on low-latency, reliable access to ERP SQL and support estates
- they should not be designed as browser-only or storefront-only functions

### Products and inventory

- queue-driven or scheduled synchronization depends on a runtime that can poll, transform, and publish safely across network boundaries

### Reporting, identity, and admin tooling

- some retained support/admin/report surfaces may need to sit closer to middleware APIs than to the storefront itself

## Runtime Design Consequences

- treat middleware as a first-class application runtime, not a collection of ad hoc jobs
- separate commerce-facing API concerns from ERP-facing integration concerns even if they deploy together initially
- keep private connectivity, secret management, and environment isolation on the critical path

## Preferred Phase-1 Runtime Shape

- hosted middleware runtime with private connectivity to AccountMate SQL
- service-owned persistence for workflow state, idempotency, and operator support data
- centralized secrets and structured observability
- scheduler/queue capability for sync and retry handling

## What Must Not Be Assumed Prematurely

- the exact Azure resource layout (VM count, App Services vs. VMs, NSG configuration) is not yet specified
- the exact private-routing mechanism between the Azure tier and `AMSERVER-v9` (site-to-site VPN, Azure Hybrid Connection, VNet peering, or another path) has not been technically verified — it is shown in the topology diagram but the configuration has not been independently confirmed
- same-SQL-instance assumptions for any live database other than what is confirmed on staging remain unsafe without further verification

## Phase-1 Recommendation

- deploy Dieselbrook middleware in Azure, consistent with the confirmed current architecture
- engage Annique IT to confirm the exact VPN/private-link configuration used by the current middleware, and ensure Dieselbrook middleware is added to the same routing path to reach `AMSERVER-v9`
- do not design for LAN-adjacent or on-premises middleware placement unless the private routing path proves non-joinable

## Primary Dependencies And Decisions

- `A-08` middleware will have private network connectivity to AccountMate SQL estate
- `A-09` **CONFIRMED** Azure-compatible middleware deployment — topology diagram (2026-03-11) confirms Azure is the current and target platform for the middleware and commerce tier
- `X-DEP-01` Dieselbrook final Shopify solution intent
- `X-DEP-02` Annique response on missing web shell/assets
- `X-DEP-06` **RESOLVED** hosting and infrastructure topology confirmed by Annique-supplied diagram (2026-03-11)

## Evidence Base

- `docs/07_hosting_certainty_matrix.md`
- `docs/05_delivery_architecture_dieselbrook.md`
- `docs/11_nisource_process_parity_matrix.md`
