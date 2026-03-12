# Orders Domain Pack

## 1. Domain Summary

Business purpose:

- move customer orders from the commerce platform into AccountMate
- preserve operational warehouse flow, shipment visibility, invoicing, and downstream financial/MLM side effects

Why the domain matters to the migration:

- orders are the most operationally sensitive replacement surface in the programme
- the current order flow crosses storefront, middleware, ERP, warehouse workflow, and reverse-status synchronization
- behavioural regressions here will be immediately visible to customers, finance, fulfillment, and consultant operations

Phase-1 importance:

- critical
- no viable phase-1 storefront cutover exists without safe parity for order ingest, ERP write-back, and fulfillment/status visibility

## 2. Current Systems And Actors

Systems involved:

- NopCommerce order store
- `NISource` middleware runtime
- `amanniquelive`
- `SOPortal`
- downstream invoice, payment, and shipping tables in AccountMate

Human roles involved:

- customers placing storefront orders
- warehouse/fulfillment operators
- finance and reconciliation staff
- consultant support staff where consultant lifecycle is affected by ordering

Source-of-truth boundaries:

- storefront order capture originates in NopCommerce today and will originate in Shopify later
- middleware owns translation, validation, idempotency, and write-back orchestration
- AccountMate remains operational truth for sales orders, invoices, shipping progression, and downstream ERP effects
- `SOPortal` is a real operational workflow dependency, not just a log table

## 3. Current Source Components

FoxPro files:

- `NISource/syncorders.prg`
- `NISource/syncorderstatus.prg`

SQL procedures/tables:

- `sp_NOP_syncOrders`
- `sp_NOP_syncOrderStatus`
- `sp_ws_reactivate`
- `sosord`
- `sostrs`
- `soskit`
- `soship`
- `SOPortal`
- `arinvc`
- `aritrs`
- `arcash`
- `be_waybill`
- `changes`

External or adjacent integrations:

- middleware wake-up via ERP-side doorbell procedures
- downstream ticket/event linkage through invoice reconciliation logic already identified in discovery
- consultant lifecycle side effect through reactivation during order import

## 4. Current Workflows

### Workflow A: Order intake into AccountMate

Trigger:

- ERP-side scheduler or process calls `sp_NOP_syncOrders`

Inputs:

- unprocessed Nop order header, customer, billing/shipping data, line items, notes, role context, and payment state

Major logic steps:

- doorbell procedure posts to middleware without carrying the order payload
- middleware reads ready orders directly from Nop SQL
- middleware resolves customer/account mapping and payment interpretation
- middleware checks duplicate-import state using Nop order identity persisted in AM
- middleware optionally reactivates inactive consultants via `sp_ws_reactivate`
- middleware writes ERP order structures and portal workflow row

Outputs:

- `sosord`
- `sostrs`
- `soskit`
- `SOPortal`
- sometimes `arcash`, depending on payment path and operational rules already observed

Side effects:

- consultant lifecycle mutation can occur during import
- warehouse workflow becomes active through `SOPortal`

Error/failure behavior:

- current estate appears fragile and directly coupled; reconciliation and deterministic retry behavior are not first-class concerns in the legacy model

### Workflow B: Warehouse and shipping progression

Trigger:

- order exists in ERP and enters operational warehouse flow

Inputs:

- ERP order records and `SOPortal` state

Major logic steps:

- order progresses through portal workflow markers
- warehouse/picking/shipping actions update operational state
- shipping and invoicing proceed in ERP-native flow

Outputs:

- `SOPortal.cStatus` moves through pending, picking, and shipped states
- invoices are created in `arinvc` and `aritrs`
- shipping and waybill visibility is available via `soship` and `be_waybill`

Side effects:

- financial posting and consultant/MLM downstream behaviour depend on invoice creation, not just order creation

Error/failure behavior:

- warehouse exceptions, partial shipments, and manual handling remain known risk areas that require parity-aware design even where current source is not exhaustively mapped

### Workflow C: Reverse order-status synchronization

Trigger:

- ERP-side process calls `sp_NOP_syncOrderStatus`

Inputs:

- current shipped/picked state from ERP and `SOPortal`

Major logic steps:

- doorbell procedure wakes middleware
- middleware reads operational state from AM-side tables
- middleware updates storefront-facing order/shipment visibility in the web platform

Outputs:

- customer-visible order status and shipment visibility in the storefront

Side effects:

- customer support and self-service visibility depend on correct reverse synchronization

Error/failure behavior:

- if reverse sync fails, the ERP may be correct while storefront status is stale

## 5. Operational Dependencies

Other domains that depend on this one:

- consultants and MLM because order import can reactivate consultants and invoice creation affects MLM downstream processes
- reporting and operational visibility because reconciliation depends on order, shipment, and invoice state
- communications because operational notifications often hinge on order and shipment lifecycle

Downstream side effects:

- invoice creation
- warehouse workflow activation
- consultant reactivation
- ticket/event linkage to invoice records

Upstream assumptions:

- commerce platform order capture remains authoritative before ERP import
- customer/account role resolution is available at import time

Timing or sequencing dependencies:

- order import must be idempotent before ERP write-back
- reverse status sync must run after ERP shipping state becomes operationally true

## 6. Replacement Boundary

What must be replaced in phase 1:

- storefront-to-middleware order ingest
- deterministic order validation and transformation
- ERP order write-back equivalent to current `syncorders.prg` behaviour
- explicit idempotency, retry, and reconciliation controls
- reverse status/shipment propagation to the storefront

What can remain ERP-side or legacy-side temporarily:

- invoice generation and deeper ERP financial posting
- warehouse-native fulfilment execution
- any ERP-native order-entry side behaviour not proven to require middleware ownership

What is candidate for later-phase modernization:

- deeper warehouse orchestration redesign
- replacement of legacy operational portal patterns if business confirms they are not required as-is
- advanced exception-handling/operator tooling beyond phase-1 minimum reconciliation capability

## 7. Risks And Fragility Points

- hidden side effects extend beyond `sosord` and `sostrs`
- current duplicate protection depends heavily on persisted external order identity and is not sufficient as the only protection in the target state
- `SOPortal` is parity-critical and easy to under-scope
- consultant reactivation during order import creates cross-domain coupling
- invoice-dependent downstream logic means a superficially successful order import can still fail the business process if invoicing or status progression breaks later

## 8. Open Decisions

Decision IDs:

- `D-03` legacy admin/back-office functions required in phase 1
- `D-05` transitional use of `compsys` for outbound communications where order notifications are involved
- `D-07` mandatory custom Nop order features for day-one parity
- `D-09` whether phase 1 requires new back-office/admin surfaces or only APIs and ops tooling

Assumption and dependency IDs:

- `A-01` Shopify remains target commerce platform
- `A-02` phase 1 preserves ERP truth
- `A-08` middleware will have private network connectivity to AccountMate SQL estate
- `X-DEP-01` Dieselbrook final Shopify solution intent
- `X-DEP-03` continued staging SQL access for targeted validation
- `X-DEP-06` hosting and infrastructure topology confirmation

## 9. Recommended Phase-1 Capabilities

Named capabilities required:

- order intake service
- ERP order write-back service
- order idempotency ledger
- shipment and status sync service
- reconciliation and operator trace service
- consultant-reactivation adapter for the proven legacy side effect

Recommended service ownership boundaries:

- storefront adapters should own inbound commerce event capture only
- middleware should own transformation, validation, idempotency, and ERP integration orchestration
- ERP remains owner of operational order, invoice, and shipping truth once import succeeds

## 10. Evidence Base

Source files:

- `NISource/syncorders.prg`
- `NISource/syncorderstatus.prg`

Existing discovery docs:

- `docs/06_orders_deep_dive.md`
- `docs/10_accessible_estate_and_replacement_surface.md`
- `docs/11_nisource_process_parity_matrix.md`
- `docs/13_phase1_sql_contract.md`
- `middleware/orders/01_current_state_order_flow.md`
- `middleware/orders/05_order_business_rules.md`
- `middleware/orders/06_staging_order_inventory.md`
- `middleware/orders/07_order_replacement_touchpoint_map.md`

Repo memory:

- `/memories/repo/order-flow-notes.md`