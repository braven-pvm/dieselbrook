# Orders Deep Dive
## AccountMate, NopCommerce, Portal Flow, Stored Procedures, Data Models, and Validation Logic

**Date:** March 10, 2026
**Prepared by:** Braven Lab
**Scope:** Current-state order architecture across AccountMate, NopCommerce integration middleware, SOPortal, invoices, status sync, and related validation rules
**Goal:** Produce a full working model of how orders are represented, moved, validated, stored, and synchronized today so Dieselbrook can design the Shopify replacement correctly

---

## 1. Executive Summary

### Core finding

The current order architecture is **not** a direct API-to-ERP write model.

It is a layered process built from:

- NopCommerce order records
- a middleware service (`nopintegration.annique.com`) that reads NopCommerce data and writes into AccountMate
- AccountMate sales-order and invoice tables
- `SOPortal` as an internal order staging and warehouse-status control table
- AccountMate-side shipping and invoicing processes
- a status sync loop that pushes shipped state back to NopCommerce

### Most important implementation fact

The ERP-side stored procedures `sp_NOP_syncOrders` and `sp_NOP_syncOrderStatus` do **not** carry order payloads. They are only HTTP wake-up calls.

They POST to `nopintegration.annique.com`, and the middleware then performs the real work by:

- reading NopCommerce orders directly
- mapping those orders to AccountMate customers and payment structures
- writing `sosord`, `sostrs`, `soskit`, and `SOPortal`
- later reading `SOPortal` status changes to update NopCommerce order/shipment visibility

### Practical implication for Dieselbrook

To replace orders safely, Dieselbrook must understand and reproduce **all five layers**:

1. NopCommerce order intake rules
2. middleware transformation and validation rules
3. AccountMate sales-order and invoice structures
4. `SOPortal` operational status workflow
5. reverse status and fulfillment synchronization back to the web platform

This is the single most sensitive integration area in the project.

---

## 2. High-Level Architecture

```text
Customer places order in NopCommerce
        ↓
ERP-side SQL job / SP posts empty request to /syncorders.api
        ↓
nopintegration middleware wakes up
        ↓
Middleware reads unprocessed NOP orders directly from Nop DB
        ↓
Middleware maps customer, payment, items, pricing, shipping
        ↓
Middleware writes AccountMate order records
  - sosord   (order header)
  - sostrs   (order lines)
  - soskit   (kit components)
  - SOPortal (portal / warehouse workflow state)
        ↓
AccountMate warehouse / AutoPick / fulfillment process runs
        ↓
Shipping / invoice creation in AM
  - arinvc   (invoice header)
  - aritrs   (invoice lines)
        ↓
SOPortal status changes to shipped
        ↓
ERP-side SP posts empty request to /syncorderstatus.api
        ↓
nopintegration middleware reads AM shipped state and updates NOP order status / shipment visibility
```

---

## 3. System Responsibilities By Layer

### 3.1 NopCommerce responsibility

NopCommerce is responsible for:

- customer-facing cart and checkout
- order capture
- payment state in the web layer
- shipment/customer visibility in the web layer
- customer role assignment used by the middleware to choose the AM billing account

### 3.2 Middleware responsibility

The middleware is responsible for:

- identifying which NOP orders are ready to import
- mapping order/customer/payment data into AM-compatible structures
- preventing duplicate imports
- reactivating inactive consultants if required
- creating AM order records and the `SOPortal` record
- pushing shipment/status visibility back to NOP

### 3.3 AccountMate responsibility

AccountMate is responsible for:

- the sales-order operational record
- warehouse and picking process
- shipping/invoicing lifecycle
- AR posting and invoice balances
- MLM/rebate downstream impact once invoice data exists

### 3.4 SOPortal responsibility

`SOPortal` appears to act as an internal order workflow table for web-originating orders.

It is responsible for:

- staging orders into operational picking flow
- driving warehouse status visibility (`pending`, `picking`, `shipped`)
- acting as the source read by the reverse status sync logic

---

## 4. The Doorbell Procedure Pattern

### 4.1 `sp_NOP_syncOrders`

Confirmed live definition:

- reads `wsSetting` only as a null/kill-switch check
- then hard-overrides the URL to `https://nopintegration.annique.com/syncorders.api`
- calls `sp_ws_HTTP`
- sends no order payload

Meaning:

- the SP itself does not transform orders
- the SP does not send JSON or line data
- the real integration logic lives outside the ERP procedure in the middleware

### 4.2 `sp_NOP_syncOrderStatus`

Confirmed live definition:

- same pattern as above
- hard-overrides to `https://nopintegration.annique.com/syncorderstatus.api?instancing=single`
- calls `sp_ws_HTTP`
- sends no order status payload

Meaning:

- shipped/picked status is not sent in the POST body
- middleware must read current state from AM after waking up

### 4.3 Why this matters

For Dieselbrook, these procedures should be viewed as:

- compatibility triggers
- not business logic
- not payload contracts

The replacement must preserve the **behavioural contract**, not just the URL.

---

## 5. End-to-End Current-State Order Flow

## 5.1 Order intake from NopCommerce into AccountMate

Based on source-derived discovery already documented:

1. ERP schedule or process calls `sp_NOP_syncOrders`
2. `sp_NOP_syncOrders` POSTs to `/syncorders.api`
3. middleware connects to NOP SQL and finds unprocessed orders
4. middleware loads:
   - order header
   - customer
   - billing address
   - shipping address
   - order items
   - order notes
5. middleware determines the AM account to charge based on NOP customer role
6. middleware interprets payment method / state
7. middleware checks whether customer must be reactivated in AM
8. middleware checks whether the order already exists in AM
9. middleware writes AM order structures
10. middleware creates `SOPortal` record
11. middleware creates/updates NOP shipment visibility through the web API side

### 5.2 Warehouse / portal flow inside AM

Current confirmed status progression in `SOPortal.cStatus`:

- `' '` = pending
- `'P'` = picking
- `'S'` = shipped

Confirmed by prior procedure review and corrections note.

Operational markers:

- `dcreate` = portal/staging creation time
- `dprinted` = order sent/printed into warehouse flow
- `cStatus` = warehouse progression state

### 5.3 Invoicing flow

Once shipped/processed, AM creates invoice records:

- `arinvc` header
- `aritrs` lines

These are the financially meaningful sales records and the basis for downstream AR and MLM processes.

### 5.4 Reverse status synchronization back to NopCommerce

1. AM-side procedure posts to `/syncorderstatus.api`
2. middleware reads `SOPortal` where shipped state has been reached
3. middleware updates NOP order / shipment status for customer visibility

### 5.5 Invoice-to-side-feature flow

Additional confirmed downstream usage:

- `sp_ws_invoicedtickets` links event booking/ticket records to invoice numbers by reconciling `aritrs` and `sostrs`
- MLM/rebate logic depends on invoices existing in AM, not just sales orders

---

## 6. Order Identity and Idempotency

### 6.1 Confirmed idempotency rule

Previously documented from `syncorders.prg`:

- duplicate check is done via `sosord.cPono = NOP OrderID`
- plus customer match (`ccustno`)

Meaning:

- the NOP order ID is persisted into the AccountMate sales order header as the purchase/order reference
- this is the current primary anti-duplicate control

### 6.2 Why this matters

Dieselbrook must keep a strong external-order identity strategy.

For the Shopify replacement, the equivalent should be:

- store Shopify Order ID and/or order name in the AM-side order record or middleware ledger
- maintain a separate middleware idempotency table as a second protection layer

---

## 7. Customer and Account Mapping Rules

### 7.1 Confirmed role mapping from NOP to AM account

From source-derived discovery:

| NOP Role | AM `ccustno` used | Meaning |
|---|---|---|
| `Annique Consultant` | consultant's own `ccustno` | Standard consultant order |
| `AnniqueStaff` | `STAFF1` | Staff purchase |
| `AnniqueExco` | `STAFF1` | Executive/staff channel |
| `Bounty` | `ASHOP1` | Special/inter-company channel |
| `Client` | `ASHOP2` | Retail customer |
| default/no clear match | `ASHOP1` | fallback mapping |

### 7.2 Reactivation rule

If consultant customer is inactive in AM:

- middleware calls `sp_ws_reactivate @ccustno`

Confirmed live `sp_ws_reactivate` behaviour:

- validates a deactivation record exists in `compplanlive..deactivations`
- derives sponsor/upline from generation data
- updates `arcust` to set active status and sponsor
- adjusts `dstarter` logic
- prepends a reactivation note to `arcnot`
- logs an action to `NopIntegration..Brevolog`

### 7.3 Practical meaning

An order import is not just a commercial transaction. It can also mutate consultant lifecycle state.

This is a critical business rule and must not be lost during migration.

---

## 8. Payment and Shipping Mapping

### 8.1 Sales order header fields used for order/payment/shipping representation

From `sosord` schema and live samples, important header fields include:

- `csono` — sales order number
- `ccustno` — AM customer account
- `corderby` — order-by / external reference field
- `centerby` — live samples show `Web Order`
- `cpaycode` — payment code
- `cshipvia` — shipping method/carrier
- `csource` — source tag
- `cb*` / `cs*` address fields — billing/shipping addresses
- `nsalesamt`, `ndiscamt`, `nfrtamt`, `ntaxamt*` — financial totals
- `lhold`, `lvoid`, `lsource`, `ctranref` — operational controls

### 8.2 Live sample evidence

Recent `sosord` sample rows show:

- `centerby = 'Web Order'`
- `cpaycode = 'CWO'`
- `cshipvia` populated with carrier values such as `SKYNET`, `COURIER`, `FASTWAY`, `BERCO`
- `lsource = 4` on recent web-order rows

### 8.3 Invoice-side equivalents

Important `arinvc` fields include:

- `cinvno` — invoice number
- `ctype` — invoice type / credit-type indicator
- `ccustno`
- `dorder`, `dinvoice`, `dcreate`
- `cpaycode`, `cshipvia`
- `nsalesamt`, `ndiscamt`, `nfrtamt`, `ntaxamt1`
- `ntotpaid`, `nbalance`
- `lmlm`, `lautoreb`, `cmlmlink`

### 8.4 What this tells us

AM order and invoice headers are carrying:

- customer identity
- source/channel identity
- shipping method
- payment code
- full billing/shipping address copy
- tax and freight totals
- downstream AR/MLM controls

This is more than a simple order mirror.

---

## 9. Data Model: Core Tables

## 9.1 `sosord` — Sales order header

Role:

- operational sales-order master in AccountMate

Key fields confirmed from schema:

- identifiers: `csono`, `crevision`, `ccustno`, `cpono`, `cbsono`, `ciono`
- parties: `corderby`, `centerby`, `ccommiss`, `csource`
- billing address: `cbaddrno`, `cbcompany`, `cbaddr1`, `cbaddr2`, `cbcity`, `cbstate`, `cbzip`, `cbcountry`, `cbphone`, `cbcontact`, `cbemail`
- shipping address: `csaddrno`, `cscompany`, `csaddr1`, `csaddr2`, `cscity`, `csstate`, `cszip`, `cscountry`, `csphone`, `cscontact`, `csemail`
- logistics/payment: `cshipvia`, `cfob`, `cpaycode`, `cbankno`, `cchkno`, `ccardno`, `cexpdate`, `ccardname`, `cpayref`, `ccurrcode`, `ctranref`
- dates: `dcreate`, `dorder`, `dexpire`, `dquote`
- flags: `lquote`, `lhold`, `lvoid`, `lnobo`, `lusecusitm`, `lapplytax`, `lprcinctax`, `lsavecard`, `lcrhold`, `lautoacpt`, `lautoinvk`
- totals: `nsalesamt`, `ndiscamt`, `nfrtamt`, `ntaxamt1`, `ntaxamt2`, `ntaxamt3`, `nadjamt`, `nweight`, `nxchgrate`

Interpretation:

- this is a full commercial order header, not just a lightweight staging record
- many values are copied forward to the invoice lifecycle

## 9.2 `sostrs` — Sales order lines

Role:

- operational order transaction/line table tied to `sosord`

Key fields confirmed from schema:

- linkage: `cuid`, `csono`, `ccustno`, `clineitem`, `cinvno`
- item identity: `citemno`, `cdescript`, `cspeccode1`, `cspeccode2`, `cwarehouse`, `cmeasure`
- financials: `nordqty`, `nshipqty`, `nprice`, `nprcinctx`, `ndiscamt`, `nsalesamt`, `nbprice`, `nextnddprc`
- behaviour flags: `lkititem`, `lfree`, `lstock`, `lmodikit`, `lupsell`, `ldropship`, `lautoacpt`
- tax/revenue: `ctaxcode`, `crevncode`, `ndiscrate`, `ntaxamt*`
- campaign/pricing references: `csppruid`, `ccampuid`
- cross-reference fields: `cSORefUId`, `cSORef`
- inventory hooks: `ixitmID`, `lcusxitm`

Interpretation:

- `sostrs` is where item-level pricing, shipping quantity, kit behaviour, and campaign linkage really live
- this is the most important line table for order import correctness

## 9.3 `soskit` — kit decomposition table

Role:

- records kit/component detail associated with an SO line

Key fields confirmed:

- `csono`, `clineitem`, `citemno`, `cdescript`
- `nqty`, `ncost`
- `lstock`, `nseq`

Interpretation:

- orders containing bundles/kits are not represented by header + lines only
- there is a separate decomposition structure that must be respected

## 9.4 `SOPortal` — web/portal workflow state

Role:

- internal order portal/staging table that appears to drive warehouse process state for web-originating orders

Confirmed schema fields:

- `csono`
- `dcreate`
- `dprinted`
- `cStatus`

Interpretation:

- intentionally minimal
- likely references `sosord` as the main order detail source
- functions as a process-state overlay rather than a full order record

## 9.5 `WebOrderTrace`

Role:

- likely historical audit/trace table for web order linkage

Confirmed fields from export sample:

- `crcptno`
- `ccustno`
- `csono`
- `centerby`

Interpretation:

- likely used for traceability, portal reconciliation, or web-origin audit rather than order fulfillment logic
- still relevant for migration audit considerations

## 9.6 `arinvc` — invoice header

Role:

- financial invoice record
- downstream AR and MLM base document

Key fields confirmed from schema:

- identifiers: `cuid`, `cinvno`, `crevision`, `ctype`, `ccustno`
- lineage/source: `coriginvno`, `corderby`, `centerby`, `csource`, `cmlmlink`
- address block: same full billing/shipping copy model as `sosord`
- logistics/payment: `cshipvia`, `cfob`, `cpaycode`, `cbankno`, `cchkno`, `ccardno`, `ccardname`, `cpayref`
- dates: `dcreate`, `dorder`, `dinvoice`, `ddiscount`, `ddue`, `dlastpaid`, `dcharge`, `dclosed`, `ddate`, `dbill`
- totals: `nsalesamt`, `ndiscamt`, `nfrtamt`, `ntaxamt*`, `ntotpaid`, `nbalance`
- flags: `lvoid`, `lfinchg`, `lapplytax`, `lprcinctax`, `lmlm`, `lgenrebate`, `linvoicing`, `lautoreb`, `lblock`
- digital contact: `cbemail`, `csemail`

Interpretation:

- this is the document that matters for finance, AR, and commissions
- any Shopify replacement must preserve accurate invoice creation downstream in AM

## 9.7 `aritrs` — invoice lines

Role:

- line-level realization of the invoice
- links back to the source sales order and items

Key fields confirmed from schema:

- linkage: `cinvno`, `csono`, `cshipno`, `clineitem`, `csolineitm`, `cshlineitm`
- item identity: `citemno`, `cdescript`, `cmeasure`, `ccustno`, `cwarehouse`
- quantities: `nordqty`, `nshipqty`
- pricing: `nprice`, `nprcinctx`, `ndiscamt`, `nsalesamt`, `nbprice`
- campaign/pricing refs: `csppruid`, `ccampuid`
- status/flags: `cstatus`, `lkititem`, `lstock`, `lfree`
- user trace: `cusername`
- shipping/additional: `nshipcost`
- MLM-related fields: `nQv`, `nsp`

Interpretation:

- `aritrs` is the realized invoice-side equivalent of `sostrs`
- it preserves sales-order linkage needed for downstream reconciliation

---

## 10. Live Data Observations

### 10.1 Recent `sosord` rows

Observed characteristics from live sample:

- recent order numbers look like `A1092075`
- `centerby` is `Web Order`
- `cpaycode` is populated with `CWO`
- `cshipvia` contains operational carrier values such as `SKYNET`, `COURIER`, `FASTWAY`, `BERCO`
- freight is explicitly stored on the header
- `lsource = 4` on sampled recent web rows

### 10.2 Recent `SOPortal` rows

Observed characteristics:

- newly created rows often have `dprinted = NULL`
- shipped/advanced rows have `dprinted` populated
- `cstatus` may be blank/space in early/pending state

### 10.3 Recent `arinvc` rows

Observed characteristics:

- invoice types include blank/default and `R` for returns/credits
- `nbalance` and `ntotpaid` drive financial settlement state
- `cshipvia` and `cpaycode` survive into invoices
- `lmlm` and `lautoreb` are explicit invoice-level flags

### 10.4 Recent `sostrs` and `aritrs` rows

Observed characteristics:

- pricing is carried at the line level
- `csppruid` is often populated on invoice lines
- `ccampuid` can be null or populated depending on campaign involvement
- invoice lines retain `csono`, preserving order-to-invoice traceability

---

## 11. Validation and Business Rules

## 11.1 Intake validation in middleware

Confirmed or strongly evidenced rules:

- order must be in a processable NOP state before import
- duplicate import prevented by checking existing AM SO using external order reference
- customer role determines which AM customer account gets charged
- inactive consultant can be reactivated before order import
- payment method is interpreted and mapped into AM payment fields

## 11.2 Sales-order integrity rules in AM

Observed from trigger inventory and schema:

- `sosord` has insert, update, and delete triggers
- `sostrs` has insert/update/delete and quantity triggers
- `sostrs` trigger updates parent `sosord` state such as `lnobo`
- credit-hold and quote state affect downstream behaviour

Implication:

- direct inserts into AM order tables are not passive
- writes can trigger cascading behaviour and data normalization

## 11.3 Invoice integrity rules in AM

Observed from trigger inventory:

- `arinvc` has insert and update triggers plus additional triggers
- `aritrs` has insert/update triggers and quantity-related triggers
- invoice triggers update balances, payment artefacts, and history behaviour

Implication:

- invoice creation/modification is part of a wider AR engine, not just a data write

## 11.4 Portal status validation

Confirmed:

- `SOPortal.cStatus` drives warehouse/portal state
- `dprinted IS NOT NULL` plus `cStatus=' '` transitions to `P`
- shipped state `S` is used by reverse sync to update the web platform

## 11.5 Ticket invoice reconciliation

Confirmed from `sp_ws_invoicedtickets`:

- event booking rows in NOP without invoice number are scanned
- procedure reconciles invoice number by joining `aritrs` and `sostrs`
- it uses `csono`, `cSORef`, line references, and item matching
- then updates `NOP..ANQ_Booking.cinvno`

Implication:

- order/invoice linkage is consumed by side features beyond standard fulfillment

---

## 12. Triggers and Structural Side Effects

### 12.1 `sosord` triggers

Confirmed trigger presence:

- `sosord_insert`
- `sosord_update`
- `sosord_delete`

Observed effects from export snippets:

- sanitization of check/card fields by payment mode
- adjustments to term discount and due-day values
- cascade delete/cleanup of lines in certain conditions

### 12.2 `sostrs` triggers

Confirmed trigger presence:

- `sostrs_insert_update_delete`
- `sostrs_qtyUD`

Observed effects from export snippets:

- updates parent `sosord` no-backorder state (`lnobo`)
- checks quote and credit-hold conditions
- quantity updates interact with item/inventory references (`ixitmID`)

### 12.3 `arinvc` triggers

Confirmed trigger presence:

- `arinvc_insert`
- `arinvc_update`
- `dateupdate`
- `trig_it_arinvc`

Observed effects from export snippets:

- numbering/history movement behaviour
- balance and finance-charge logic
- clearing of payment fields based on payment mode

### 12.4 `aritrs` triggers

Confirmed trigger presence:

- `aritrs_ins_upd`
- `aritrs_qtyUD`
- `trig_it_aritrs`

Observed effects from export snippets:

- audit/history tracking into auxiliary structures
- quantity change handling
- user modification trace capture

### 12.5 Key conclusion on triggers

The AM order model is **behavioural**, not just structural.

Any Dieselbrook design that bypasses or incompletely reproduces current insert/update assumptions can cause silent downstream differences.

---

## 13. Current Endpoints, Jobs, and Stored Procedure Roles

### 13.1 Order intake side

| Component | Current role |
|---|---|
| `sp_NOP_syncOrders` | Doorbell POST from AM to middleware |
| `/syncorders.api` | Middleware order-import endpoint |
| `syncorders.prg` | Real order import logic in middleware |
| `ANQ_UnprocessedOrders` | Middleware-side selector for processable NOP orders |
| `sp_ws_reactivate` | Consultant reactivation if needed during import |

### 13.2 Fulfillment/status side

| Component | Current role |
|---|---|
| `SOPortal` | internal status/staging table |
| AutoPick / warehouse procedures | move orders through picking and shipping lifecycle |
| `sp_NOP_syncOrderStatus` | Doorbell POST from AM to middleware |
| `/syncorderstatus.api` | Middleware status sync endpoint |
| `ANQ_syncOrderStatus` | NOP-side job polling/pushing web status visibility |

### 13.3 Invoice/linkage side

| Component | Current role |
|---|---|
| `arinvc` / `aritrs` | financial invoice realization |
| `sp_ws_invoicedtickets` | attaches invoice numbers to event bookings |
| `sp_ct_Rebates` | downstream MLM commission logic depends on invoice records |

---

## 14. What Is Stored Where Today

## 14.1 Stored in NopCommerce / web layer

- customer-facing order
- payment status in web commerce context
- shipment/customer status visibility
- customer role used for AM-account mapping
- event booking records that later need invoice linkage

## 14.2 Stored in middleware (behaviourally, if not as business tables)

- import logic
- selection criteria for unprocessed orders
- field mapping and role mapping logic
- duplicate detection logic
- API-to-API side effects back to NOP

## 14.3 Stored in AccountMate sales-order layer

- operational order header (`sosord`)
- operational order lines (`sostrs`)
- kit decomposition (`soskit`)
- portal process state (`SOPortal`)

## 14.4 Stored in AccountMate invoice/AR layer

- invoice header (`arinvc`)
- invoice lines (`aritrs`)
- balances and payment settlement state
- MLM-related invoice flags and linkage

---

## 15. Key Inferred Lifecycle Relationships

### Confirmed or strongly evidenced relationships

| Relationship | Evidence |
|---|---|
| `SOPortal.csono` → `sosord.csono` | procedure snippets and joins in discovery exports |
| `sostrs.csono` → `sosord.csono` | schema and trigger logic |
| `aritrs.csono` → source sales order number | invoice sample and schema |
| `aritrs.cinvno` → `arinvc.cinvno` | invoice model and schema |
| `sostrs.cinvno` may later reference invoice linkage | schema field presence |
| `sp_ws_invoicedtickets` reconciles bookings using `sostrs` + `aritrs` joins | live SP text |

### Lifecycle summary

```text
web order
  → sales order header/lines
  → portal workflow state
  → shipped/processed
  → invoice header/lines
  → downstream finance / MLM / booking linkage
```

---

## 16. What AM Expects From An Imported Order

At a minimum, the current import path expects enough data to populate:

### Header-level expectations

- customer account (`ccustno`)
- order reference / external order identity
- order date / create date
- billing and shipping addresses
- shipping method (`cshipvia`)
- payment code (`cpaycode`)
- totals: sales, discount, freight, tax
- source/channel markers such as `centerby='Web Order'`

### Line-level expectations

- item code
- quantity ordered
- line pricing
- line discount amount
- whether line is kit/free/stock-affecting
- campaign/pricing references where applicable

### Operational expectations

- duplicate-safe import
- valid AM customer account mapping
- inactive consultant handled appropriately
- `SOPortal` record created to let warehouse flow continue

### Downstream expectations

- invoice process must remain possible
- shipment status must still reconcile back to the web platform
- invoice-level features like booking-ticket linkage must still work

---

## 17. Migration Implications For Dieselbrook

### 17.1 What must be preserved exactly

- external-order idempotency
- customer-role-to-AM-account mapping
- inactive consultant reactivation behaviour
- `SOPortal` status lifecycle
- sales-order to invoice traceability
- side features that depend on invoice linkage

### 17.2 What can change safely

- the transport mechanism can move from NOP polling to Shopify webhooks
- the middleware implementation language/platform can change
- NOP-specific order selectors can be replaced by Shopify-compatible logic
- customer-facing status updates can use Shopify Fulfillment APIs instead of NOP order status plumbing

### 17.3 Highest-risk replacement areas

| Area | Risk |
|---|---|
| Order import mapping | High |
| Account mapping by role | High |
| `SOPortal` operational staging | High |
| invoice linkage and downstream features | High |
| status sync timing | Medium-High |
| ticket/event invoice reconciliation | Medium |

---

## 18. Recommended Target-State Design Notes

For the Shopify replacement, Dieselbrook should aim for:

1. Shopify webhook as the new inbound event source.
2. Middleware-owned idempotency ledger in addition to AM reference matching.
3. Explicit AM order-write service that reproduces current order header/line/portal behaviour.
4. `SOPortal` retained as internal staging if AutoPick and warehouse processes still depend on it.
5. Reverse fulfillment sync driven by AM shipped state, but writing to Shopify rather than NOP.
6. Reconciliation tooling to compare Shopify orders, AM sales orders, `SOPortal`, and invoices.

---

## 19. Open Questions Still Worth Closing

Even with this analysis, a few order areas still need targeted confirmation:

1. Exact field set written by `syncorders.prg` into `sosord`, `sostrs`, `soskit`, and `SOPortal`.
2. Exact selector logic inside `ANQ_UnprocessedOrders` and how cancellations/partial payments affect eligibility.
3. Which AM procedures or jobs advance `SOPortal` from pending to picking to shipped.
4. Whether Shopify replacement should write directly into `SOPortal` or into `sosord` + `sostrs` + `SOPortal` as a single transaction.
5. Whether any special handling exists for returns, COD, PayFlex, gift-card, or mixed-payment orders beyond the currently documented mapping.
6. Whether there are additional NOP-side shipment creation assumptions not yet surfaced from `syncorders.prg` / API backend.

---

## 20. Bottom Line

The current order model is a **hybrid operational-financial workflow**:

- NOP captures the commercial order
- middleware transforms and imports it
- AccountMate owns operational order execution
- `SOPortal` controls warehouse visibility/state
- AM invoices become the financial truth
- reverse sync returns shipment visibility to the web layer

That means Dieselbrook is not replacing a simple order API. It is replacing a coordinated order-processing ecosystem with dependencies across:

- customer/account mapping
- ERP order structures
- warehouse portal workflow
- invoicing
- downstream booking and MLM logic

The Shopify solution will only be safe if it reproduces that lifecycle deliberately rather than just “creating an order in ERP”.