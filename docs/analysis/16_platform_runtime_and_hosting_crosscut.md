# Platform Runtime And Hosting Cross-Cut

## Purpose

This document consolidates the platform-runtime and hosting implications that cut across all domain packs.

## Current Working Position

- the legacy estate is a split architecture, not a single-host storefront application
- public middleware and commerce components are strongly evidenced as Microsoft-hosted and likely Azure-hosted
- ERP/data systems remain on a privately reachable SQL boundary
- private SQL reachability is a hard delivery constraint, not a deployment preference

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

- exact live AccountMate hosting class is still not proven
- exact Azure resource layout is still not proven
- same-server or same-SQL-instance assumptions are unsafe without infrastructure confirmation

## Phase-1 Recommendation

- proceed assuming an Azure-compatible middleware deployment with private SQL reachability to ERP/data systems
- keep a fallback option for LAN-adjacent or privately hosted runtime placement if latency or connectivity evidence contradicts the default assumption

## Primary Dependencies And Decisions

- `A-08` middleware will have private network connectivity to AccountMate SQL estate
- `A-09` Azure-compatible middleware deployment is the working default hosting assumption
- `X-DEP-01` Dieselbrook final Shopify solution intent
- `X-DEP-06` hosting and infrastructure topology confirmation
- hosting verification items already documented in `docs/07_hosting_certainty_matrix.md`

## Evidence Base

- `docs/07_hosting_certainty_matrix.md`
- `docs/05_delivery_architecture_dieselbrook.md`
- `docs/11_nisource_process_parity_matrix.md`
