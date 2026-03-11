# Shopify Consultant Interface

## Purpose

This document defines how the consultant domain should surface in Shopify without pretending Shopify is the full consultant system of record.

## Short Answer

Consultants in Shopify should most likely be represented as:

- customer accounts
- plus consultant-specific tags or segments
- plus consultant-specific metafields and app-managed experience

They should not be modeled as ordinary customers with no additional integration layer.

## 1. Minimum Shopify Representation

Recommended Shopify-side representation:

| Shopify object | Role |
|---|---|
| Customer | login and customer-facing identity |
| Customer tags / segments | quick eligibility and segmentation |
| Customer metafields | consultant sync state and entitlement summaries |
| Product / variant metafields | consultant pricing / entitlement / exclusivity support |
| Custom app UI / app proxy | reports, onboarding, advanced consultant workflows |

## 2. Recommended Consultant Metafields

Suggested customer metafields:

| Metafield | Purpose |
|---|---|
| `consultant.is_consultant` | primary storefront gate |
| `consultant.am_ccustno` | ERP mapping |
| `consultant.status` | active / inactive / pending |
| `consultant.activation_date` | lifecycle visibility |
| `consultant.profile_completed` | checkout/profile gate |
| `consultant.sponsor_ref` | optional support or UI context |
| `consultant.has_exclusive_items` | category/product gating shortcut |
| `consultant.has_starter_kit_allocations` | starter flow shortcut |
| `consultant.has_awards` | award UI shortcut |

Keep the payload compact. Full consultant state should stay in middleware.

## 3. Consultant UX That Shopify Alone Will Not Replace

The current Nop customization includes:

- consultant-specific my-account view
- consultant reports route
- consultant registration workflow
- consultant-only checkout popups and entitlement handling
- awards and exclusive-item flows

These will require either:

- custom app pages
- app proxy pages
- customer-account extensions where suitable
- or a middleware-backed consultant portal adjacent to Shopify

## 4. Consultant Checkout Interface

The current storefront distinguishes consultant vs non-consultant during checkout.

For Shopify, the new consultant checkout experience will need to support:

- consultant pricing eligibility
- starter-kit / exclusive-item entitlements
- gift and special-offer logic where still required
- profile completeness gating before checkout finalization

That will likely require a mix of:

- synchronized consultant/customer metadata
- cart or checkout validation in custom app logic
- Shopify Functions only where they fit the narrow pricing/rule use case

## 5. Consultant Registration Interface

The current Nop store has a dedicated consultant registration route and workflow.

For Shopify, recommended options are:

1. custom app registration form backed by middleware
2. external registration page backed by middleware, then link into Shopify account creation

Do not model consultant registration as a plain storefront account signup if sponsor validation and downstream ERP creation remain required.

## 6. Consultant Reports Interface

Current source shows a consultant-facing reports area under customer routes.

Recommended Shopify-era approach:

- keep report generation in middleware or ERP-backed report services
- surface links, report lists, and report views through app UI
- do not attempt to store downline or commission report history in Shopify customer records

## 7. Exclusive Item And Starter Interface

The current plugin uses consultant identity to:

- show or hide an exclusive category
- limit access to allocated exclusive products
- force-add some starter or exclusive items
- restore allocations on cancellation

In Shopify, this means consultant interface design must support both:

- visibility gating
- allocation-aware entitlement behavior

Those rules are too stateful to treat as a simple product tag problem.

## 8. Consultant As Customer vs Consultant As Business Actor

Shopify is good at representing the customer-facing aspect of a consultant.

Shopify is not the right native home for:

- sponsor/upline tree maintenance
- commission calculations
- rebate ledger history
- monthly downline rebuild logic

So the interface should be designed around what the consultant needs to see and do, while middleware and ERP preserve the business engine behind it.