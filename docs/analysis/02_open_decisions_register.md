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
| D-01 | What consultant/customer model does Dieselbrook want on Shopify? | Dieselbrook | affects identity, pricing, account structure, and entitlement design | consultants remain a distinct operational model, not ordinary customers | high | open |
| D-02 | How should campaign pricing be represented on Shopify? | Dieselbrook | affects pricing architecture, sync shape, and admin tooling | effective pricing will be precomputed/synced, not calculated live at checkout from ERP | high | open |
| D-03 | Which legacy admin/back-office functions must survive in phase 1? | Dieselbrook + Annique | determines whether campaign/admin/report UIs are replaced immediately | parity-critical operational tooling survives if business-critical; low-value admin tooling may defer | high | open |
| D-04 | Does phase 1 need South Africa only, or South Africa plus Namibia parity? | Dieselbrook | affects tenancy, campaign publishing, product sync, and operational scope | **CLOSED: Namibia is out of scope entirely. All Namibia-specific considerations are removed from phase-1 analysis.** | medium | closed |
| D-05 | Will outbound communications still use `compsys` temporarily? | Dieselbrook | affects integration boundary and SQL permissions | prefer replacing legacy queue usage unless a transitional compatibility need is proven | medium | open |
| D-06 | Is Shopify Plus/B2B tooling available and intended for consultant pricing patterns? | Dieselbrook | materially affects final pricing and account strategy | analysis continues without assuming final B2B mechanism is approved | high | open |
| D-07 | Which Nop custom plugin features are mandatory for day-one parity versus retire/defer? | Dieselbrook + Annique | prevents overbuilding or under-scoping | core commerce and consultant behavior are mandatory; niche programmes require explicit confirmation | high | open |
| D-08 | Are there additional browser-facing nopintegration modules still to be supplied? | Annique | affects completeness of legacy current-state analysis | assume campaign is not the only possible module, but do not block current analysis on more drops | low | open |
| D-09 | Does the phase-1 solution need a new back-office/admin interface, or only APIs and ops tooling? | Dieselbrook | shapes UI scope, admin capability scope, and effort | assume minimal admin/tooling necessary for operational control only | medium | open |
| D-10 | Which reports are actually used operationally and must survive? | Dieselbrook + Annique | affects reporting scope and BI replacement boundary | only materially used reports should drive phase-1 scope | medium | open |

## Decisions Expected From Dieselbrook

- D-01 consultant/customer operating model
- D-02 campaign pricing representation
- D-06 Shopify Plus/B2B strategy
- D-09 admin/back-office UX expectation

## Closed Decisions

- D-04 SA-only versus SA+NAM — **closed: Namibia is out of scope**

## Decisions Expected From Annique

- D-08 remaining web-facing nopintegration assets/modules
- D-10 operationally used report set confirmation

## Review Rule

Each future major analysis or requirements document should reference the relevant decision IDs it depends on.