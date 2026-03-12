# Pricing, Campaigns, And Exclusives Domain Pack

## 1. Domain Summary

Business purpose:

- determine and surface the correct effective consultant price
- enforce campaign-driven pricing windows and promotional state
- preserve exclusive-item eligibility and related access control
- support any phase-1-critical campaign administration or operational visibility

Why the domain matters to the migration:

- pricing is not a simple storefront discount concern in the current estate
- current behaviour is split across ERP campaign data, SQL procedures, Nop plugin override services, cart/checkout rules, and exclusive-item synchronization
- pricing mistakes or access leaks will be commercially visible immediately and erode trust quickly

Phase-1 importance:

- critical
- consultant pricing and exclusive access are core business behaviour and cannot be deferred without explicit business simplification

## 2. Current Systems And Actors

Systems involved:

- `amanniquelive`
- `NISource` middleware and campaign module
- NopCommerce custom plugin pricing/cart/checkout estate
- existing storefront catalog and checkout surfaces

Human roles involved:

- consultants and retail customers
- marketing or campaign administrators
- support and operations staff who need to explain pricing or access outcomes

Source-of-truth boundaries:

- AccountMate campaign and item data currently anchor live effective price state
- the custom Nop plugin currently applies or shapes storefront pricing and promotion behaviour
- `soxitems` and related exclusive-item flows act as entitlement/access data rather than simple merchandising metadata
- the newly received `NISource/campaign` module shows a legacy admin surface exists, but the current source snapshot is incomplete for that web shell

## 3. Current Source Components

FoxPro files and module surfaces:

- `NISource/campaign/campaign.prg`
- `NISource/campaign/Campaign_class.prg`
- `NISource/campaign/campaigns.js`
- related but incomplete campaign UI layout/script dependencies documented separately

Nop plugin components:

- `Services/OverriddenServices/OverriddenPriceCalculationService.cs`
- `Services/OverriddenServices/OverrideDiscountService.cs`
- `Services/OverriddenServices/AnniqueShoppingCartService.cs`
- `Services/DiscountAllocation/`
- `Services/SpecialOffers/`
- consultant/staff checkout rules that affect pricing/access enforcement

SQL procedures/tables:

- `Campaign`
- `CampDetail`
- `CampSku`
- `icitem`
- `soxitems`
- `sp_camp_getSPprice`
- `sp_Camp_CatSummary`
- `sp_Camp_SkuByMonthvert`
- `sp_Camp_BrandByMonth`
- `sp_Camp_GetCamp`
- `sp_Camp_GetSponTypes`
- `sp_NOP_syncSoxitems`
- `NOP_UpdateExclusiveItemCode`

## 4. Current Workflows

### Workflow A: Effective consultant price resolution

Trigger:

- storefront product pricing display or checkout pricing for a consultant buyer

Inputs:

- consultant identity/customer state
- item code
- order or pricing date
- active campaign and item pricing rows

Major logic steps:

- determine whether a campaign is active for the relevant date
- resolve matching campaign item row if present
- return campaign price when active, otherwise fall back to base item price
- plugin/cart logic then applies pricing behaviour in the storefront runtime

Outputs:

- effective consultant price
- retail comparison price context where needed

Side effects:

- cart, checkout, and displayed pricing all depend on this state being represented correctly

Error/failure behavior:

- pricing drift is immediately customer-visible and commercially sensitive

### Workflow B: Campaign administration and product-exposure management

Trigger:

- operational or marketing changes to campaign catalogue, SKU allocation, or publication state

Inputs:

- campaign definitions, campaign item rows, sponsor type metadata, and brand/category summary inputs

Major logic steps:

- admin-facing campaign module reads campaign summary and SKU detail procedures
- operators can edit campaign data, assign SKUs, and publish to stage

Outputs:

- updated campaign product and exposure state in the legacy estate

Side effects:

- campaign timing and publication state influence future storefront pricing and product visibility

Error/failure behavior:

- current snapshot of the module is incomplete, so current-state business intent is clearer than exact runnable admin implementation details

### Workflow C: Exclusive-item eligibility sync

Trigger:

- exclusive-item entitlement changes for consultant-linked customers

Inputs:

- `soxitems`
- mapped consultant/customer identity
- mapped product identity/SKU

Major logic steps:

- ERP-side exclusive-item changes trigger sync procedures
- legacy integration updates exclusive-item state in the web-platform estate
- storefront/cart logic uses that entitlement state to allow or block access

Outputs:

- consultant-specific exclusive product access state in the storefront platform

Side effects:

- access control is coupled to commerce identity and pricing eligibility

Error/failure behavior:

- entitlement drift can leak protected products or incorrectly block valid consultants

### Workflow D: Promotion and discount application in storefront runtime

Trigger:

- cart or checkout price calculation

Inputs:

- product
- customer role/status
- attributes and special-offer state
- standard discounts and promotional rules

Major logic steps:

- overridden price and discount services apply custom order of operations
- special offers can be applied before standard discounts
- consultant/staff checkout rules further shape eligibility and behaviour

Outputs:

- final line price and applied discount state in the storefront

Side effects:

- pricing behaviour is woven into platform service overrides, not isolated in one pricing function

Error/failure behavior:

- pricing and promotion bugs can arise from incorrect stacking order, incorrect eligibility, or missing effective-price synchronization

## 5. Operational Dependencies

Other domains that depend on this one:

- consultants and MLM because consultant identity/status drive pricing and exclusive-access eligibility
- products and inventory because campaign publication and product state intersect with what is sellable and visible
- orders because checkout pricing and entitlement outcomes must align with ERP-side expectations
- admin and back-office tooling because campaign operations may require retained or replaced admin capabilities

Downstream side effects:

- customer-visible price
- checkout discount totals
- exclusive-item access control
- campaign-driven product exposure

Upstream assumptions:

- ERP remains pricing source of truth in phase 1
- middleware or sync services will own effective-state projection rather than runtime ERP lookups in checkout

Timing or sequencing dependencies:

- price sync must account for campaign start/end boundaries, not only changed rows
- exclusive-item sync must stay aligned with consultant identity state

## 6. Replacement Boundary

What must be replaced in phase 1:

- effective consultant price computation or projection owned by Dieselbrook middleware
- synchronization of current effective pricing state into Shopify-facing runtime data
- exclusive-item entitlement synchronization and enforcement
- deterministic checkout pricing application that does not recreate the full ERP pricing engine live inside checkout
- minimum operational visibility or tooling needed to support pricing/campaign operations

What can remain ERP-side or legacy-side temporarily:

- campaign master-data authority in AccountMate
- deeper pricing archaeology beyond what is needed for parity harnessing
- some low-value legacy admin UI behaviour if business confirms it is not required in phase 1

What is candidate for later-phase modernization:

- full campaign admin UI redesign
- richer promotion tooling separated from legacy plugin/service patterns
- consolidation of pricing, promotions, and access control into cleaner purpose-built services and admin experiences

## 7. Risks And Fragility Points

- pricing logic is split across SQL, middleware, plugin overrides, cart logic, and promotions
- `sp_camp_getSPprice` appears to be a strong pricing oracle, but hidden exceptions still need to be handled cautiously
- campaign timing boundaries can activate or expire price state without a recent row update
- exclusive-item logic is access control, not just merchandising, and is easy to trivialize incorrectly
- the campaign admin module is relevant but incomplete, so UI parity decisions must not assume the current source snapshot is self-sufficient

## 8. Open Decisions

Decision IDs:

- `D-02` how campaign pricing should be represented on Shopify
- `D-03` which admin/back-office functions must survive in phase 1
- `D-04` South Africa only versus South Africa plus Namibia parity
- `D-06` Shopify Plus/B2B strategy for consultant pricing patterns
- `D-07` which custom Nop pricing and promotion features are mandatory for day-one parity
- `D-08` whether additional browser-facing nopintegration modules still need to be supplied
- `D-09` whether a new admin/back-office interface is required in phase 1

Assumption and dependency IDs:

- `A-02` preserve ERP truth in phase 1
- `A-04` campaign pricing needs middleware-owned effective-state logic
- `A-05` `NISource` source is useful but incomplete as a full web app snapshot
- `A-06` campaign module belongs to the legacy estate and matters to pricing/admin scope
- `A-08` middleware will have private network connectivity to AccountMate SQL estate
- `X-DEP-01` Dieselbrook final Shopify solution intent
- `X-DEP-02` Annique response on missing web shell/assets
- `X-DEP-06` hosting and infrastructure topology confirmation

## 9. Recommended Phase-1 Capabilities

Named capabilities required:

- effective pricing projection service
- pricing parity harness against ERP pricing oracle
- exclusive-item sync service
- checkout discount application component
- exclusive-access validation component
- minimum campaign operations support service or admin tooling adapter

Recommended service ownership boundaries:

- ERP remains source for campaign and base price truth
- middleware owns computation/projection of current effective state and synchronization to Shopify runtime data
- checkout runtime should stay deterministic and small, applying precomputed state rather than reconstructing ERP logic live

## 10. Evidence Base

Source files:

- `NISource/campaign/campaign.prg`
- `NISource/campaign/Campaign_class.prg`
- `NISource/campaign/campaigns.js`
- `NopCommerce - Annique/Plugins/Annique.Plugins.Nop.Customization/Services/OverriddenServices/AnniqueShoppingCartService.cs`
- `NopCommerce - Annique/Plugins/Annique.Plugins.Nop.Customization/Services/SpecialOffers/SpecialOffersService.cs`

Existing discovery and design-framing docs:

- `docs/05_delivery_architecture_dieselbrook.md`
- `docs/10_accessible_estate_and_replacement_surface.md`
- `docs/12_nop_customization_domain_classification.md`
- `docs/13_phase1_sql_contract.md`
- `docs/14_campaign_module_missing_dependencies.md`
- `docs/15_nisource_source_completeness_matrix.md`
- `docs/16_annique_followup_request_missing_web_assets.md`
- `docs/notion-updates/07c_pricing_access_updated.md`
- `docs/notion-updates/07d_pricing_engine_deep_dive.md`

Repo memory:

- `/memories/repo/pricing-risk-notes.md`