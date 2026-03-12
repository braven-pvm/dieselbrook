# Products And Inventory Domain Pack

## 1. Domain Summary

Business purpose:

- publish and maintain sellable product state from ERP into the commerce channel
- keep catalog, stock availability, images, category mappings, and manufacturer mappings aligned with operational truth

Why the domain matters to the migration:

- product and inventory synchronization is one of the main ongoing runtime responsibilities of `NISource`
- catalog and stock state are foundational inputs for pricing, orders, fulfillment, and customer trust
- the current estate combines full syncs, delta syncs, stock syncs, and image syncs with ERP-driven queues and mappings

Phase-1 importance:

- critical
- the storefront cannot be replaced safely without equivalent product publication and inventory accuracy behaviour

## 2. Current Systems And Actors

Systems involved:

- `amanniquelive`
- `NISource/syncproducts.prg`
- NopCommerce API/database estate
- ERP-triggered queues such as `changes`, `icWsUpdate`, and `iciimgUpdateNOP`
Human roles involved:

- catalog administrators
- operations staff monitoring stock and publish state
- marketing/merchandising staff depending on correct category and brand mapping

Source-of-truth boundaries:

- ERP item and stock data remain the operational source of truth in phase 1
- middleware owns publication, transformation, and channel sync orchestration
- commerce channel owns presentation once synchronized, but not master truth

## 3. Current Source Components

FoxPro files:

- `NISource/syncproducts.prg`
- related runtime wiring through `apiprocess.prg`, `syncclass.prg`, `nopapi.prg`, and `nopdata.prg`

SQL procedures/tables/queues:

- `sp_NOP_syncItemAll`
- `sp_NOP_syncItemChanges`
- `sp_NOP_syncItemAvailability`
- `sp_ws_UpdateImageNOP`
- `icitem`
- `iciwhs`
- `icibal`
- `changes`
- `icWsUpdate`
- `iciimgUpdateNOP`
- `ANQ_CategoryIntegration`
- `ANQ_ManufacturerIntegration`

External integrations:

- NopCommerce API/backend used by `nopapi.prg`
- product image sync path into NOP-side image process
## 4. Current Workflows

### Workflow A: Full product synchronization

Trigger:

- `sp_NOP_syncItemAll` schedule or manual full run

Inputs:

- ERP product master data from `icitem`
- category/manufacturer integration mappings
- storefront-oriented read model such as `sp_ws_getactiveNEW`

Major logic steps:

- doorbell procedure wakes middleware
- middleware authenticates against NOP-side API
- active items are loaded and transformed into product payloads
- products are created or updated in the commerce channel
- publication state is normalized after sync completion

Outputs:

- complete product upsert in the commerce platform

Side effects:

- categories, manufacturers, SEO/image hooks, and published state are updated in the channel estate

Error/failure behavior:

- full sync is operationally heavy and any mapping or API failure can leave catalog state partially updated

### Workflow B: Delta product synchronization

Trigger:

- `sp_NOP_syncItemChanges`

Inputs:

- `changes` table rows identifying item field changes

Major logic steps:

- middleware reads delta candidates from change-tracking data
- only impacted items are re-read and republished

Outputs:

- incremental catalog alignment for changed products

Side effects:

- limits full-catalog churn while preserving near-current storefront state

Error/failure behavior:

- if delta logic misses a material field or queue state is stale, catalog drift emerges quietly

### Workflow C: Inventory availability synchronization

Trigger:

- `sp_NOP_syncItemAvailability` plus ERP stock queues/triggers

Inputs:

- warehouse stock from `iciwhs` and related balance logic

Major logic steps:

- ERP-side trigger/queue captures inventory changes
- middleware consumes availability updates and projects stock into the commerce channel

Outputs:

- storefront stock quantity and availability state

Side effects:

- order acceptance and customer trust depend on this being timely and accurate

Error/failure behavior:

- stale stock causes oversell, false-out-of-stock, or support load

### Workflow D: Image and merchandising asset synchronization

Trigger:

- image update queue and `sp_ws_UpdateImageNOP`

Inputs:

- `iciimg*`-family image staging/update records

Major logic steps:

- pending image items are read from the queue
- channel image sync is invoked per item

Outputs:

- product image refresh in the storefront estate

Side effects:

- catalog presentation quality and campaign/product exposure depend on image freshness

Error/failure behavior:

- image queues can accumulate silently if the downstream sync path fails

## 5. Operational Dependencies

Other domains that depend on this one:

- pricing and campaigns because effective pricing and publish windows assume correct product identity and visibility
- orders because accurate catalog and stock state are prerequisites for valid checkout and ERP order import
- admin and back-office tooling because product/category/brand mappings and publication behaviour are operational concerns

Downstream side effects:

- storefront product visibility
- stock visibility
- image quality
- category and brand correctness

Upstream assumptions:

- ERP remains source of truth for item and stock state in phase 1
- queue-driven detection mechanisms are still useful even if the consumer changes

Timing or sequencing dependencies:

- price and publish state should not be synchronized independently of product identity and active dates
- image and inventory updates need near-current propagation to avoid commercial drift

## 6. Replacement Boundary

What must be replaced in phase 1:

- full product publication pipeline
- delta product synchronization
- inventory synchronization
- image synchronization
- category/manufacturer mapping behaviour needed for storefront parity

What can remain ERP-side or legacy-side temporarily:

- ERP item master truth
- queue-generation triggers in ERP where they are already reliable and business-safe

What is candidate for later-phase modernization:

- redesign of queueing/event architecture
- deeper catalog enrichment and merchandising tooling
- more modern product/media orchestration outside direct ERP-driven polling

## 7. Risks And Fragility Points

- product, stock, and image synchronization are separate paths and can drift independently
- hidden mapping logic in category/manufacturer integration can break catalog discoverability without obvious runtime failures
- stock accuracy at scale is a high-volume operational problem, not a minor sync concern

## 8. Open Decisions

Decision IDs:

- `D-03` legacy admin/back-office functions required in phase 1
- `D-07` which custom Nop catalog features are mandatory for day-one parity

Assumption and dependency IDs:

- `A-01` Shopify remains the target platform
- `A-02` preserve ERP truth in phase 1
- `A-08` middleware will have private network connectivity to AccountMate SQL estate
- `X-DEP-01` Dieselbrook final Shopify solution intent
- `X-DEP-03` continued staging SQL access
- `X-DEP-06` hosting and infrastructure topology confirmation

## 9. Recommended Phase-1 Capabilities

Named capabilities required:

- product sync service
- inventory sync service
- image sync service
- catalog mapping service
- sync queue consumer/reconciliation service

Recommended service ownership boundaries:

- middleware owns transformation and publication from ERP to Shopify
- ERP remains owner of item and stock truth
- Shopify owns catalog presentation after sync only

## 10. Evidence Base

Source files:

- `NISource/syncproducts.prg`
- `NISource/apiprocess.prg`
- `NISource/syncclass.prg`
- `NISource/nopapi.prg`

Existing discovery docs:

- `docs/02_integration_map.md`
- `docs/04_business_logic.md`
- `docs/10_accessible_estate_and_replacement_surface.md`
- `docs/11_nisource_process_parity_matrix.md`
- `docs/annique-discovery-0.1.md`
- `docs/annique-discovery.1.0.md`
