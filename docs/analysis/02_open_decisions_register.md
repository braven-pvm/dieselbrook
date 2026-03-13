# Open Decisions Register

## Purpose

This document is the control register for unresolved decisions that materially affect scope, replacement design, work sequencing, or requirements.

Open questions should be promoted here once they are important enough to influence the programme.

## Usage Rule

- `Decision Owner` is who must ultimately resolve the item
- `Blocking Level` is how much it prevents downstream work
- `Current Working Assumption` is what analysis should assume until the decision is made

## Register

| ID | Decision | Decision Owner | Why It Matters | Current Working Assumption | Blocking Level | Status |
|---|---|---|---|---|---|---|
| D-01 | What consultant/customer account model does Dieselbrook intend on Shopify? Are consultants standard Shopify customers with tags and metafields, or a distinct custom account class managed by middleware? (Shopify B2B native tooling is not the recommended path based on pricing analysis.) | Dieselbrook | affects identity, pricing, account structure, and entitlement design; consultants are a distinct operational class in the legacy estate with their own ERP lifecycle, pricing eligibility, exclusive access, and MLM hierarchy access — none of this can be designed without knowing the target account model | consultants remain a distinct operational model; assumed to be standard Shopify customer accounts extended with consultant-specific tags and metafields, synced from AccountMate by middleware | high | open |
| D-02 | Is the recommended Shopify consultant pricing architecture approved? Analysis recommends: middleware precomputes effective consultant prices using `sp_camp_getSPprice` as the AM oracle; effective prices sync as product metafields; a deterministic Shopify Function applies a fixed discount delta (retail minus synced consultant price) at checkout; exclusive item access enforced via customer metafields. If not approved, what alternative representation is preferred? | Dieselbrook | the recommended architecture is fully designed; Dieselbrook must confirm it or nominate an alternative — the sync design, Function build, and integration effort all depend on this | middleware will precompute effective prices from `sp_camp_getSPprice` and sync as product metafields; Function applies fixed discount delta (retail − consultant price) at checkout; this is the assumed architecture pending explicit confirmation | high | open |
| D-03 | Which legacy admin/back-office functions must survive in phase 1? | Dieselbrook + Annique | determines whether campaign/admin/report UIs are replaced immediately | parity-critical operational tooling survives if business-critical; low-value admin tooling may defer | high | open |
| D-04 | Does phase 1 need South Africa only, or South Africa plus Namibia parity? | Dieselbrook | affects tenancy, campaign publishing, product sync, and operational scope | **CLOSED: Namibia is out of scope entirely. All Namibia-specific considerations are removed from phase-1 analysis.** | medium | closed |
| D-05 | Will outbound communications still use `compsys` temporarily? | Dieselbrook | affects integration boundary and SQL permissions | prefer replacing legacy queue usage unless a transitional compatibility need is proven | medium | open |
| D-06 | What is the intended Shopify plan tier and app distribution model? Specifically: is Shopify Plus in budget? This determines whether a custom Shopify app with Shopify Functions is viable for the consultant checkout pricing implementation. | Dieselbrook | custom apps containing Shopify Functions require Shopify Plus; on Advanced or lower plans, only public App Store apps can use Functions. The recommended consultant pricing architecture (precomputed effective prices + deterministic checkout Function) depends on Function availability. B2B native tooling is NOT the recommended path — pricing archaeology shows a middleware-owned precomputed model is correct and B2B is irrelevant. The decision is purely about plan tier and app distribution model (custom app vs public app). | analysis proceeds assuming a custom app with Shopify Functions is the target; if Plus is not approved, the fallback is a public-app distribution model which is viable on any plan but has operational trade-offs to assess | high | open |
| D-07 | Which Nop custom plugin features are mandatory for day-one parity versus retire/defer? | Dieselbrook + Annique | prevents overbuilding or under-scoping | core commerce and consultant behavior are mandatory; niche programmes require explicit confirmation | high | open |
| D-08 | Are there additional browser-facing nopintegration modules still to be supplied? | Annique | affects completeness of legacy current-state analysis | assume campaign is not the only possible module, but do not block current analysis on more drops | low | open |
| D-09 | Does the phase-1 solution need a new back-office/admin interface, or only APIs and ops tooling? | Dieselbrook | shapes UI scope, admin capability scope, and effort | assume minimal admin/tooling necessary for operational control only | medium | open |
| D-10 | Which reports are actually used operationally and must survive? | Dieselbrook + Annique | affects reporting scope and BI replacement boundary | only materially used reports should drive phase-1 scope | medium | open |

## Decisions Expected From Dieselbrook

- D-01 consultant/customer operating model
- D-02 campaign pricing representation
- D-06 Shopify plan tier and app distribution model (is Plus in budget?)
- D-09 admin/back-office UX expectation

## Closed Decisions

- D-04 SA-only versus SA+NAM — **closed: Namibia is out of scope**

## Decisions Expected From Annique

- D-08 remaining web-facing nopintegration assets/modules
- D-10 operationally used report set confirmation

## Review Rule

Each future major analysis or requirements document should reference the relevant decision IDs it depends on.