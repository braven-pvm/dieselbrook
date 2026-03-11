# MLM Data Dependency Map

## Purpose

This document identifies what currently touches the MLM-side SQL database surface, primarily `compplanlive`, and what that means for Dieselbrook scope.

## Bottom Line

If Dieselbrook is replacing `NISource`, then the `compplanlive` integration surface is in scope.

That does not automatically mean Dieselbrook must reimplement the full SQL commission engine in phase 1. It does mean Dieselbrook must account for all middleware, API, reporting, and order-flow dependencies that currently rely on `compplanlive`.

As of 2026-03-11, `compplanLive` is also confirmed directly on the staging SQL server rather than only inferred from procedure references.

## Primary Database In Scope

The strongest current candidate for the topology diagram's `Related DB / MLM Data` box is:

- `compplanlive`

Adjacent support database:

- `compsys` for queued email / messaging support

## Staging Confirmation

Direct staging SQL inspection confirmed:

- SQL Server identity: `AMSERVER-V9`
- database present: `compplanLive`
- populated tables present: `CTcomph`, `CTcomp`, `CTconsv`, `CTRunData`, `CTstatement`, `CTstatementh`, `deactivations`, `CTdownline`, `CTdownlineh`

Selected staging row counts captured during verification:

- `CTcomph`: 8,623,539
- `CTconsv`: 1,779,211
- `CTRunData`: 3,702,636
- `CTstatementh`: 4,481,557
- `deactivations`: 35,702
- `CTdownlineh`: 1,491,507
- `CTdownline`: 0

This moves `compplanLive` from "strongly evidenced by references" to "confirmed present and populated on staging".

## Systems That Touch `compplanlive`

| System / layer | Touch type | Evidence | What it does |
|---|---|---|---|
| AccountMate SQL stored procedures | read + update | `sp_ct_Rebates`, `sp_ws_reactivate`, `sp_ct_updatedisc`, `sp_TradeGroupBuild` | commissions, reactivation, consultant discount support, trade-group segmentation |
| FoxPro middleware API | read | `NISource/apiprocess.prg` | sponsor/downline validation via `fn_Get_DownlineHist` |
| FoxPro order import middleware | execute indirect dependency | `NISource/syncorders.prg` | order flow triggers `sp_ws_reactivate`, which reads `compplanlive..deactivations` |
| FoxPro reporting layer | read | `NISource/reports.prg` | consultant/MLM reporting reads `ctcomp`, `ctcomph`, and related report procedures |
| NopCommerce custom plugin | no direct evidence found | search found no direct `compplanlive` references | depends on derived consultant outcomes, not direct DB calls |

## Confirmed Touchpoints

### 1. Commission generation

- `sp_ct_Rebates` reads `compplanlive..ctcomph`
- it aggregates consultant commission amounts
- it creates AR invoices and related posting rows in AccountMate
- it marks processed MLM rows using `UPDATE compplanlive..ctcomph SET cCompStatus='P'`

This is the clearest proof that `compplanlive` is an active operational database, not just a reporting store.

### 2. Consultant reactivation during order import

- `NISource/syncorders.prg` calls `EXEC sp_ws_reactivate @ccustno=...`
- `sp_ws_reactivate` reads `compplanlive..deactivations`
- it also uses `compplanlive.dbo.fn_splitstring_to_table`

This means web order intake already depends on the MLM-side database for consultant lifecycle handling.

### 3. Sponsor/downline validation

- `NISource/apiprocess.prg` calls `CompPlanLive.dbo.fn_Get_DownlineHist(...)`
- this is used to verify whether a sponsor belongs in the requester's downline

This is a middleware/API dependency, not only a month-end finance dependency.

### 4. Consultant reports

- `NISource/reports.prg` reads from `ctcomp` and `ctcomph`
- related SQL procedures also read `Ctstatement`, `Ctstatementh`, `CtRunData`, and `CtConsv`

This means any replacement of the legacy consultant report/API layer must account for MLM-side report reads.

### 5. Consultant discount and grouping logic

- `sp_ct_updatedisc` reads `CompPlanLive.dbo.ctConsv`
- `sp_TradeGroupBuild` reads `compplanlive..Ctstatement`, `Ctstatementh`, `CtRunData`, and `CtConsv`

These are not storefront concerns, but they are still part of the consultant operating model.

## Scope Implication For Dieselbrook

Replacing `NISource` means the following `compplanlive`-related capabilities are in scope:

- consultant hierarchy and sponsor validation interfaces
- consultant reactivation integration during order import
- consultant-facing reporting interfaces that currently read MLM-side data
- any middleware APIs that expose downline or commission-adjacent views
- operational awareness of the commission run dependencies on invoice creation

## What Is In Scope vs Not Automatically In Scope

| Capability | Scope status | Reason |
|---|---|---|
| replacing FoxPro reads/executions against `compplanlive` | in scope | this is part of replacing `NISource` |
| replacing consultant/downline API queries | in scope | current FoxPro API uses `compplanlive` directly |
| replacing consultant report data access | in scope | current report layer uses MLM-side data |
| preserving order-triggered reactivation behaviour | in scope | current order import depends on `sp_ws_reactivate` |
| reimplementing `sp_ct_Rebates` commission algorithm outside SQL | not automatic for phase 1 | that is a separate MLM engine replacement project unless explicitly commissioned |
| moving MLM ledger ownership into Shopify | not recommended for phase 1 | current logic is SQL-native and invoice-driven |

## Practical Design Consequence

Dieselbrook should treat `compplanlive` as a first-class integration dependency of the replacement middleware.

The correct phase-1 design bias is:

- replace FoxPro access patterns
- preserve SQL-native commission and hierarchy engines where they remain authoritative
- expose the required consultant/report outcomes through Dieselbrook-controlled middleware APIs and jobs

That keeps `NISource` replacement in scope without accidentally turning the project into a full MLM engine rewrite.