# Order Flow Notes

- sp_NOP_syncOrders and sp_NOP_syncOrderStatus are AM-side doorbell POSTs only; real logic lives in middleware.
- Current order lifecycle is NOP order -> middleware import -> sosord/sostrs/soskit + SOPortal -> AM invoicing in arinvc/aritrs -> reverse status sync from SOPortal.
- SOPortal status flow is confirmed as ' ' pending -> 'P' picking -> 'S' shipped.
- Existing idempotency is source-derived via sosord.cPono = NOP OrderID plus ccustno match; Shopify replacement should add a middleware idempotency ledger.
- Inactive consultants may be auto-reactivated during order import via sp_ws_reactivate.
- Invoice creation matters beyond finance: MLM and booking/ticket linkage consume arinvc/aritrs outputs.
- Staging confirms the order estate extends beyond core order tables: sostrs 333607; SOPortal 653961; be_waybill 339774; arcash 89321; Campaign 77; CampDetail 18571; wsSetting 2.
- NopIntegration is part of the order replacement surface, not just marketing support: NopReports 52; ApiClients 8; NopSettings 19; NopSSO 10260; wwrequestlog 1640737; BrevoLog 124726.
- Orders workstream staging docs created: middleware/orders/06_staging_order_inventory.md and middleware/orders/07_order_replacement_touchpoint_map.md.
