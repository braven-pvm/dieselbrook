# Program Analysis Baseline

## Purpose

This document freezes the current working view of the programme at the point where broad discovery gives way to structured analysis.

It is the baseline future documents in the analysis phase should assume unless explicitly superseded.

## Baseline Statement

Discovery is materially complete enough to move into synthesis and structured replacement analysis.

There are still gaps, but they are now narrow, dependency-oriented, and unlikely to change the overall shape of the migration problem.

## Current Programme View

### Core migration fact

This is not a storefront replacement project.

It is a platform and integration replacement programme with Shopify as the target commerce platform and AccountMate plus related SQL estates remaining operationally central.

### Core legacy estates in scope

- NopCommerce storefront and custom plugin estate
- `NISource` FoxPro middleware and web-application estate
- `amanniquelive` and related ERP tables/processes
- consultant and MLM support databases such as `compplanLive`
- support/integration databases such as `compsys` and `NopIntegration`

### Replacement reality

The real replacement surface includes:

- order import and reverse status flows
- consultant lifecycle and storefront entitlement flows
- pricing, campaigns, exclusives, and effective discount behavior
- communications, OTP, Brevo, and related support flows
- reporting and back-office support surfaces
- operational auditability, idempotency, retries, and reconciliation

## What We Now Believe With High Confidence

### Orders

- the real order logic lives in middleware, not only in AM-side stored procedure doorbells
- order import affects more than order tables and includes downstream operational side effects
- `SOPortal`, invoicing, and related status propagation are real operational dependencies

### Consultants and MLM

- consultants are a cross-system business entity, not ordinary ecommerce customers
- `compplanLive` is confirmed accessible on staging and matters to live consultant behavior
- replacing `NISource` brings consultant/MLM integration reads into scope even if the deep commission engine remains ERP-side in phase 1

### Pricing and campaigns

- pricing is not just a simple Shopify discount problem
- campaign data and effective pricing logic are ERP-rooted and materially affect the storefront experience
- the newly supplied campaign module confirms that campaign pricing also had a web/admin surface, not only background sync logic

### Source completeness

- we have substantial custom FoxPro source for the major sync and integration behaviors
- we do not appear to have a full self-sufficient snapshot of the entire nopintegration web application shell and assets

## What Is Still Open But No Longer Blocks Analysis

- the final Dieselbrook target-state decisions for some Shopify solution patterns
- whether certain legacy admin or back-office tools must survive in phase 1
- whether some support flows will continue to use legacy SQL-side queues temporarily
- whether additional web-facing nopintegration modules exist beyond the new campaign drop

These should now live in the open decisions register rather than keeping the whole programme in discovery mode.

## Analysis Boundaries

### In scope for analysis now

- current-state replacement surface
- workstream structure
- open decision set
- dependency and risk structure
- phase-1 vs later-phase boundary recommendations

### Not yet appropriate

- final detailed solution specs tightly tied to Shopify implementation mechanics
- frozen functional requirements that assume Dieselbrook's final target-state decisions
- detailed non-functional requirements by component before solution intent is confirmed

## Working Principle For Next Documents

Every new analysis document should do at least one of these things:

1. reduce uncertainty in a replacement decision
2. connect a domain to the programme-level scope
3. expose a dependency or risk that needs explicit handling
4. prepare a clean handoff into later specifications

If it does not do one of those things, it probably belongs in discovery/reference material instead.