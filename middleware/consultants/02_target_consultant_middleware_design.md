# Target Consultant Middleware Design

## Objective

Design a consultant architecture for Shopify that preserves the operational and commercial behavior currently spread across AccountMate, FoxPro sync, and the Nop custom plugin.

## Design Principles

1. Shopify customer accounts are necessary but not sufficient.
2. Consultant identity must remain anchored to a stable ERP consultant number.
3. Consultant entitlements must be explicit, synchronized, and auditable.
4. Commission and hierarchy calculations should remain ERP-native unless deliberately re-platformed.
5. Consultant onboarding, activation, and profile completeness are first-class domain processes.

## Recommended Domain Split

| Layer | Responsibility |
|---|---|
| Shopify | login, storefront identity, customer-visible account UX, self-service actions |
| Dieselbrook middleware | consultant sync, entitlement sync, onboarding orchestration, profile enforcement, cross-system mapping |
| AccountMate | consultant master, activation state, sponsor/upline, hierarchy, commission-affecting truth |

## Recommended Consultant Model

The target consultant model should be three-tiered:

1. Shopify customer record
2. middleware consultant profile record
3. ERP consultant master mapping

### Why

This prevents Shopify from being overloaded with ERP-only semantics while still allowing the storefront to react to consultant state quickly.

## Recommended Middleware Services

| Service | Responsibility |
|---|---|
| Consultant sync service | sync consultant master/profile data from AM into Shopify-facing model |
| Consultant entitlement service | sync starter kits, exclusive items, awards, and related entitlements |
| Consultant onboarding service | manage consultant registration intake, validation, and downstream creation flow |
| Consultant profile service | manage profile completeness, accepted terms, and additional data |
| Consultant report gateway | expose consultant-friendly reports sourced from ERP or report services |
| Consultant lifecycle service | activation, reactivation, status changes, and deactivation sync |

## Recommended Sync Directions

| Direction | Recommended rule |
|---|---|
| AM -> middleware | source of truth for consultant number, active state, sponsor/upline, activation-affecting fields |
| middleware -> Shopify | push consultant identity flags, profile metadata, and entitlements needed for storefront logic |
| Shopify -> middleware | collect self-service changes, onboarding requests, and storefront actions |
| middleware -> AM | push approved profile changes and onboarding outputs where the ERP must remain authoritative |

## Consultant Identity Strategy

Required stable keys:

- `am_ccustno` as the master consultant identifier
- Shopify customer ID as storefront identity
- middleware consultant ID as integration identity

Do not rely on Shopify email as the consultant primary key.

## Recommended Storefront Representation

In Shopify, a consultant should likely be represented as:

- a normal customer account
- plus consultant-specific tags or segments
- plus consultant-specific metafields for synchronized state

That is more realistic than trying to force the whole consultant model into native Shopify user fields alone.

## Recommended Entitlement Handling

The current consultant experience includes more than price:

- exclusive-item access
- starter-kit access
- awards availability
- gifts / special offers
- report access

These should be synchronized into explicit consultant entitlement records in middleware, then surfaced in Shopify via:

- customer metafields
- product or variant metafields
- app-managed gating rules
- app proxy or embedded app pages where necessary

## Recommended Activation Model

Activation should become an explicit lifecycle state with tracked transitions.

Suggested states:

- `pending_registration`
- `registered_not_activated`
- `active`
- `inactive`
- `reactivated`

The middleware should record why each transition happened and which source system initiated it.

## Recommended Registration Model

Consultant onboarding should be handled as a workflow, not a plain customer sign-up.

Suggested stages:

1. registration submitted
2. sponsor / validation checks complete
3. approved or rejected
4. ERP consultant created or linked
5. Shopify customer linked
6. starter entitlements loaded
7. welcome / activation communications sent

## Recommended Reports Boundary

Do not attempt to rebuild MLM reports directly inside Shopify first.

Safer initial approach:

- keep consultant reports sourced from middleware or ERP-backed report services
- surface them through a custom consultant portal or app surface
- decouple report presentation from core storefront catalog and checkout

## Recommended Persistence In Middleware

At minimum, store:

- consultant master mapping
- consultant profile snapshot
- sponsor/upline mapping snapshot
- entitlement allocations
- registration workflow records
- activation history
- sync history and errors

## High-Risk Areas To Preserve Carefully

- AM consultant number mapping
- activation-date logic
- exclusive-item allocations and restoration rules
- award redemption state
- starter-kit loading
- commission-sensitive identity changes such as sponsor/upline shifts

## Recommendation

For consultants, Dieselbrook should treat Shopify as the storefront face of the consultant domain, not the full consultant system of record.