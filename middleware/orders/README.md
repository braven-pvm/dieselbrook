# Orders Middleware Workstream

This folder breaks the order domain into the parts Dieselbrook will need to design, build, validate, and migrate.

## Document Set

| File | Purpose |
|---|---|
| `01_current_state_order_flow.md` | What the current source code and SQL procedures do today across NopCommerce, middleware, AccountMate, and `SOPortal` |
| `02_target_order_middleware_design.md` | How Dieselbrook should replicate and improve the current order pipeline in the new middleware |
| `03_order_data_models.md` | The key AccountMate tables, a proposed canonical middleware order model, and the minimum middleware persistence layer |
| `04_shopify_order_interface.md` | Shopify-side integration surface, event flows, field translation, and order-status synchronization |
| `05_order_business_rules.md` | The current business rules that materially affect order import, financial posting, fulfillment, and side effects |
| `06_staging_order_inventory.md` | Confirmed order-side and middleware-support objects directly verified on staging |
| `07_order_replacement_touchpoint_map.md` | Concrete mapping from current order touchpoints to Dieselbrook replacement responsibilities |

## Current Working View

- The existing ERP procedures are not the core order integration. They are doorbell calls that wake the middleware.
- The real order logic lives in `NISource/syncorders.prg` and `NISource/syncorderstatus.prg`.
- The middleware does direct SQL reads from NopCommerce and direct SQL writes into AccountMate.
- AccountMate order import is not just `sosord` and `sostrs`; it also creates `soskit`, `SOPortal`, and often `arcash` records.
- Dieselbrook should preserve behaviour, but replace the fragile direct-coupling model with explicit canonical contracts, idempotency, reconciliation, and auditability.

## Immediate Goals

1. Reconstruct the current order path exactly enough to avoid behavioural regressions.
2. Separate essential business rules from incidental implementation details.
3. Define a target middleware design that is deterministic, observable, and recoverable.
4. Build a clean Shopify-to-AccountMate translation model that can support future changes.

## Important Source Inputs

- `docs/06_orders_deep_dive.md`
- `tmp/sp_NOP_syncOrders.txt`
- `tmp/sp_NOP_syncOrderStatus.txt`
- `tmp/sp_ws_reactivate.txt`
- `NISource/syncorders.prg`
- `NISource/syncorderstatus.prg`

## Current Risks

- Order import mutates more than order tables; it can reactivate consultants and create receipts.
- `SOPortal` is a real operational dependency, not just an audit table.
- Financial, MLM, and side-feature processes depend on correct downstream AM invoice creation.
- Returns, cancellations, partial shipments, and manual warehouse exceptions still need deeper source mapping.