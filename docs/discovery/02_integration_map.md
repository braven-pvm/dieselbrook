# Integration Touchpoints Analysis
## NopCommerce (NopCommerce) & MLM System Integration Map

**Analysis Date:** 2026-03-01
**Database:** Annique AccountMate ERP + NopCommerce

---

## Executive Summary

The Annique database contains **active integration points** between:
1. **NopCommerce** (NopCommerce-based online store) — 12+ sync procedures
2. **MLM System** — 5+ rebate and consultant management procedures
3. **Fulfillment Warehouse** (fw_*) — 3rd party logistics integration
4. **Point of Sale** (SOPortal) — Web-based ordering portal

**Key Finding:** The NOP integration is relatively recent and uses stored procedures for bidirectional synchronization (Order sync, Item availability, Inventory updates).

---

## NopCommerce (NopCommerce) Integration

### Overview
NopCommerce is a NopCommerce fork (open-source e-commerce platform). The AccountMate ERP maintains 11 dedicated stored procedures and 1 view for synchronizing data between the two systems.

### Active NOP Sync Procedures

#### 1. **sp_NOP_syncItemAll**
- **Type:** Stored Procedure
- **Created:** 2023-03-20
- **Last Modified:** 2024-09-03 (recent!)
- **Purpose:** Full product catalog synchronization to NopCommerce
- **Frequency:** Manual trigger via wsSetting table
- **Tables Involved:**
  - Source: `icitem` (product master), `icimlp` (pricing)
  - Destination: External NOP database/table reference
- **Logic:** Reads AccountMate item master and pushes to store
- **Status:** ACTIVE (last modified Sept 2024)

#### 2. **sp_NOP_syncItemAvailability**
- **Type:** Stored Procedure
- **Created:** 2023-03-15
- **Last Modified:** 2024-09-03
- **Purpose:** Synchronize inventory availability/stock status
- **Tables Involved:**
  - Source: `iciwhs` (warehouse inventory), `icibal` (item balances)
  - Target: NOP product availability fields
- **Frequency:** Triggered by inventory changes (likely scheduled)
- **Notes:** Critical for real-time stock visibility on web store

#### 3. **sp_NOP_syncItemChanges**
- **Type:** Stored Procedure
- **Created:** 2023-03-15
- **Last Modified:** 2024-09-03
- **Purpose:** Sync product changes (price, description, status)
- **Tracking Table:** `changes` (531,698 rows) — change log
- **Logic:**
  ```
  Changes detected → sp_NOP_syncItemChanges → NOP updates
  ```
- **Data Flow:**
  - Monitors `changes` table for item updates
  - Extracts price, description, category, images
  - Sends to NOP via API or direct DB write

#### 4. **sp_NOP_syncOrders**
- **Type:** Stored Procedure
- **Created:** 2023-03-15
- **Last Modified:** 2026-01-09 (VERY RECENT!)
- **Purpose:** Synchronize orders from NOP to AccountMate
- **Direction:** NopCommerce → AccountMate (inbound)
- **Tables Involved:**
  - Target (Create): Sales orders in `sosord`, `sosktr` (kit tracking)
  - Source: NOP order table (external reference)
- **Frequency:** Likely real-time or scheduled hourly
- **Criticality:** HIGH — e-commerce orders must flow into ERP
- **Recent Changes:** Modified Jan 2026 suggests ongoing maintenance

#### 5. **sp_NOP_syncOrderStatus**
- **Type:** Stored Procedure
- **Created:** 2023-03-16
- **Last Modified:** 2024-10-28
- **Purpose:** Sync order status changes back to NOP
- **Direction:** AccountMate → NopCommerce (outbound)
- **Data Flow:**
  - Fulfillment/shipping status in AccountMate
  - Updates NOP order status (awaiting shipment → shipped, etc.)
- **Tables Involved:** `sosordh` (order history), shipping status fields

#### 6. **sp_NOP_syncSoxitems**
- **Type:** Stored Procedure
- **Created:** 2023-08-07
- **Last Modified:** 2023-09-29
- **Purpose:** Sync special order items (Sox) with NOP
- **Tables Involved:**
  - `soxitems` (19,574) — special order SKUs
  - `soxitemsH` (43,266) — historical special orders
- **Use Case:** Possibly customer-customizable products or build-to-order items

#### 7. **sp_ws_UpdateImageNOP**
- **Type:** Stored Procedure
- **Created:** 2023-03-22
- **Purpose:** Update product images in NOP
- **Tables Involved:**
  - Source: `iciimg` (3,372 images), `iciimgNew`, `iciimgUpdate`, `iciimgUpdateNOP` (7,744)
- **Process:**
  - Detects new/updated product images
  - Syncs to NOP media library
- **Image Staging:** `iciimgUpdateNOP` appears to be a staging table

#### 8. **sp_NOP_DiscountINS**
- **Type:** Stored Procedure
- **Created:** 2024-06-13 (RECENT)
- **Purpose:** Insert/manage discounts in NOP
- **Tables Involved:** `NOP_Discount` (2 rows)
- **Logic:** Takes discount rules and pushes to NOP discount engine

#### 9. **sp_NOP_OfferINS**
- **Type:** Stored Procedure
- **Created:** 2024-06-13 (RECENT)
- **Purpose:** Manage promotional offers in NOP
- **Related to:** Campaign management system

#### 10. **sp_NOP_OfferListINS**
- **Type:** Stored Procedure
- **Created:** 2024-06-13
- **Purpose:** List/maintain offer catalog

#### 11. **NOP_UpdateExclusiveItemCode**
- **Type:** Stored Procedure
- **Created:** 2025-09-29 (LATEST!)
- **Purpose:** Update exclusive product codes/SKUs
- **Tables Involved:** `SoxItems` — special order items
- **Business Logic:** Manages exclusive product access (possibly consultant-exclusive items in MLM system)

### NOP Integration Views (Read-Only)

#### 1. **vm_mk_NoProdMoved**
- **Type:** View
- **Purpose:** Track products moved in/out of NOP
- **Created:** 2014-11-03
- **Status:** LEGACY (12 years old, but may still be active)

### Integration Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    ACCOUNTMATE ERP                           │
├──────────────────┬──────────────────┬──────────────────────┤
│   Item Master    │  Order Module    │   Inventory           │
│   (icitem)       │   (sosord)       │   (iciwhs)            │
│   Images (ici*)  │   Shipping       │   Balances (icibal)   │
└──────────────────┼──────────────────┼──────────────────────┘
                   │                  │
        ┌──────────┴──────────┬───────┴───────────┐
        │                     │                   │
   sp_NOP_sync*          sp_NOP_sync*         sp_NOP_sync*
   ItemAll/Changes      Orders/Status        ItemAvailability
        │                     │                   │
        └──────────────────┬──┴───────────────────┘
                           │
        ┌──────────────────▼──────────────────┐
        │    KNOCK COMMERCE                   │
        │   (NopCommerce Store)               │
        ├──────────────────────────────────┤
        │ Products | Orders | Inventory    │
        │ Pricing  | Offers | Discounts    │
        └──────────────────────────────────┘
```

---

## MLM System Integration

### Overview
The MLM (multi-level marketing) system is **deeply integrated** into the core database. It's not a separate system but rather a series of specialized tables and procedures for:
1. Consultant hierarchy management
2. Rebate/commission calculations
3. Downline tracking
4. Sales attribution

### Core MLM Tables

#### 1. **genreb** (19,855,973 rows)
- **Purpose:** General rebate calculation ledger
- **Structure:** Likely stores individual rebate transactions/calculations
- **Scale:** 19.8 MILLION rows — this is the MLM backbone
- **Related:** `genrebcomp` (138,911) — computation summaries

#### 2. **mlmcust** (2,999,503 rows)
- **Purpose:** MLM customer/consultant master
- **Structure:** Links customers to MLM hierarchy
- **Scope:** ~3M consultant records (large downline network)

#### 3. **CustSPNLVL** (144,754 rows)
- **Purpose:** Customer-Sponsor-Level mapping
- **Logic:** Associates each customer with:
  - Sponsor (upline consultant)
  - Level (depth in tree)
- **Use:** Commission allocation, downline tracking

#### 4. **Consultant Level Tables**
- `Level1` (4,072) — First-level consultants/sponsors
- `Level2` (31,081) — Second-level (under sponsors)
- `Level1a`, `Level2a` — Variants/alternative structures

#### 5. **rbtmlm** (39,779 rows)
- **Purpose:** Rebate template MLM
- **Content:** Rebate plan definitions
- **Related:** `rbtmlmMONTHLY` (2.8M rows) — monthly rebate ledger

#### 6. **rebatedtl** (44,279 rows)
- **Purpose:** Rebate detail records
- **Related:** `rebatedtlMONTHLY` (3.3M rows) — monthly aggregates

#### 7. **MoDownlinerV1** (1,077,665 rows)
- **Purpose:** Monthly downliner tracking
- **Logic:** Captures monthly snapshot of each consultant's downline
- **Use Case:** Bonus calculations based on team structure

### MLM Processing Procedures

#### 1. **sp_ct_downlinebuild**
- **Type:** Stored Procedure
- **Created:** 2016-10-26
- **Last Modified:** 2016-10-28
- **Purpose:** Build/recalculate downline structure
- **Process:**
  - Walks the sponsor tree from each consultant
  - Calculates total downline members
  - Updates level assignments
- **Related Functions:**
  - `fn_get_downlineCT` — Recursive function to get downline members
  - `fn_get_titleall` — Get consultant titles/ranks
- **Tables Updated:**
  - `CTDownline` — Recalculated downline
  - `CTDownlineh` — Downline history
  - `CTCons` — Consultant status
- **Frequency:** Likely monthly or on-demand
- **Business Logic:**
  ```
  For each consultant:
    Calculate total downline members
    Calculate commission rank based on downline
    Award bonuses if targets met
  ```

#### 2. **sp_ct_Rebates**
- **Type:** Stored Procedure
- **Created:** 2018-09-26
- **Last Modified:** 2022-06-02
- **Purpose:** Calculate rebates/commissions for consultants
- **Complexity:** 13,126 characters of code (SUBSTANTIAL)
- **Process:**
  - Takes invoice data from `ARINVC`
  - Looks up item rebate rates (likely in item master or `icitem`)
  - Applies MLM formula based on sponsor relationship
  - Writes to rebate ledger tables
- **Key Tables:**
  - **Input:** `ARINVC` (invoices), `icitem` (item rebates), `compplanlive` (compensation plan)
  - **Output:** `CurUpdateRebate` (current rebates), `ARITRS` (transaction results)
- **Formula Logic:**
  - Commission percentage from item
  - Multiplication factor from consultant level
  - Distribution to sponsor (level 1), grandparent (level 2), etc.
- **Frequency:** Likely daily or per-invoice
- **Criticality:** HIGH — core MLM business process

#### 3. **vsp_mlm_getnewdocno**
- **Type:** Stored Procedure
- **Created:** 2012-07-02
- **Purpose:** Generate new MLM document numbers
- **Logic:** Sequential numbering for MLM invoices/transactions
- **Criticality:** Medium (infrastructure)

#### 4. **vsp_mlm_getnewmlminvc**
- **Type:** Stored Procedure
- **Created:** 2013-01-30
- **Purpose:** Generate new MLM invoice numbers
- **Logic:** Similar to above, specific to MLM invoices

#### 5. **vsp_rpt_rbtmlm**
- **Type:** Stored Procedure
- **Created:** 2012-10-17
- **Purpose:** Generate MLM rebate reports
- **Use Case:** Consultant commission statements, management reports

### MLM Integration Flow

```
┌─────────────────────────────────────────────────┐
│           SALES INVOICE CREATED                  │
│               (arinvc)                           │
└────────────────────┬────────────────────────────┘
                     │
                     │ Trigger/Schedule
                     ▼
        ┌────────────────────────────┐
        │  sp_ct_Rebates             │
        │  (Rebate Calculation)      │
        └────────────┬───────────────┘
                     │
        ┌────────────┴────────────────────────┐
        │                                     │
        ▼                                     ▼
    Lookup Item Rebate      Look Up Consultant
    Rate (icitem)           Sponsor Path
        │                      (CustSPNLVL)
        │                             │
        └────────────┬────────────────┘
                     │
                     ▼
        ┌──────────────────────────────┐
        │ Apply MLM Formula:           │
        │ • Base commission %          │
        │ • Level multipliers          │
        │ • Distribute to upline       │
        └────────────┬─────────────────┘
                     │
                     ▼
        ┌──────────────────────────────┐
        │ Write to Ledger:             │
        │ • genreb                     │
        │ • rbtmlm                     │
        │ • rebatedtl                  │
        │ • ARITRS                     │
        └──────────────────────────────┘
```

### MLM Business Rules Detected

1. **Multi-Level Commission Structure**
   - Commissions paid to sponsor (Level 1)
   - Commissions to sponsor's sponsor (Level 2)
   - Possibly deeper (Level 3+)

2. **Volume-Based Bonuses**
   - Tables like `QuerterlyBonusesBasedon100ofTarget` suggest bonuses triggered at thresholds
   - Likely based on personal sales volume + team volume

3. **Monthly Rollups**
   - `*MONTHLY` tables (`rbtmlmMONTHLY`, `rebatedtlMONTHLY`) suggest monthly commission cycles
   - Monthly downline snapshot (`MoDownlinerV1`) for bonus calculations

4. **Rank/Title System**
   - `fn_get_titleall` function
   - Level tables (`Level1`, `Level2`, etc.)
   - Suggests consultant promotions based on downline size/sales

---

## Fulfillment Warehouse Integration (fw_*)

### Overview
The `fw_*` tables (674K–681K rows each) appear to represent integration with a **third-party logistics provider** (possibly FedEx, UPS, or Amazon FBA).

### Fulfillment Tables

| Table | Rows | Purpose |
|-------|------|---------|
| `fw_consignment` | 674,738 | Shipments sent to warehouse |
| `fw_items` | 681,759 | Items in warehouse inventory |
| `fw_Hubs` | 23,629 | Warehouse hub locations |
| `fw_labels` | 26,241 | Shipping labels generated |
| `fw_manifest` | 6,861 | Shipment manifests |

### Integration Pattern
- **AccountMate sends:** Items to warehouse (consignments)
- **Warehouse maintains:** Real-time inventory (`fw_items`)
- **Orders trigger:** Label generation, manifests
- **Returns flow:** Back through system

### Related Tables
- `be_waybill` (334,253) — Shipment tracking numbers
- `be_invoice` (3,334) — Invoice data for logistics
- `SkyTrack` (138,096) — Possibly another carrier (Sky Express?)

---

## Order Processing Integration

### Sales Portal (SOPortal)

**Table:** `SOPortal` (646,237 rows)

**Purpose:** Web-based self-service ordering portal for consultants

**Integration Points:**
- Consultants place orders online
- Orders sync to AccountMate SO module
- Fulfillment tracked back to portal
- Status visible to customers

**Related Tracking:**
- `WebOrderTrace` (620,464) — Web order tracking details
- `somanifest` (6,657) — Shipment manifests

---

## Change Tracking & Synchronization

### Change Log Tables

**`changes` (531,698 rows)**
- **Purpose:** Central change log for all data modifications
- **Used By:** sp_NOP_syncItemChanges (detects what to sync)
- **Tracks:**
  - Item price changes
  - Product descriptions
  - Category assignments
  - Image updates
  - Availability changes

**`changescam` (892,431 rows)**
- Campaign change tracking

**`changescamp` (46,800 rows)**
- Campaign detail changes

**`changesitem` (41,755 rows)**
- Item-specific changes

**`changesper` (4,479 rows)**
- Personnel/consultant changes

### Trigger-Based Sync

The database likely uses **UPDATE/INSERT/DELETE triggers** on key tables:
- `icitem` (item changes) → triggers `changes` log
- `icimlp` (price changes) → triggers sync
- `sosord` (new orders) → triggers order sync to NOP (return)

---

## Summary of Integration Touchpoints

### NopCommerce Integration (11 Procedures + 1 View)

| # | Procedure | Direction | Frequency | Criticality |
|---|-----------|-----------|-----------|-------------|
| 1 | sp_NOP_syncItemAll | ERP → NOP | Manual/Scheduled | HIGH |
| 2 | sp_NOP_syncItemAvailability | ERP → NOP | Real-time/Scheduled | HIGH |
| 3 | sp_NOP_syncItemChanges | ERP → NOP | Change-triggered | HIGH |
| 4 | sp_NOP_syncOrders | NOP → ERP | Real-time | CRITICAL |
| 5 | sp_NOP_syncOrderStatus | ERP → NOP | Status-change triggered | HIGH |
| 6 | sp_NOP_syncSoxitems | ERP → NOP | Periodic | MEDIUM |
| 7 | sp_ws_UpdateImageNOP | ERP → NOP | Image-triggered | MEDIUM |
| 8 | sp_NOP_DiscountINS | ERP → NOP | Manual | MEDIUM |
| 9 | sp_NOP_OfferINS | ERP → NOP | Manual | MEDIUM |
| 10 | sp_NOP_OfferListINS | ERP → NOP | Manual | MEDIUM |
| 11 | NOP_UpdateExclusiveItemCode | ERP → NOP | Manual | MEDIUM |
| 12 | vm_mk_NoProdMoved (View) | Read-only | — | LOW |

### MLM Integration (5 Procedures)

| # | Procedure | Purpose | Criticality |
|---|-----------|---------|-------------|
| 1 | sp_ct_downlinebuild | Recalculate downline structure | HIGH |
| 2 | sp_ct_Rebates | Calculate commissions | CRITICAL |
| 3 | vsp_mlm_getnewdocno | Generate doc numbers | MEDIUM |
| 4 | vsp_mlm_getnewmlminvc | Generate invoice numbers | MEDIUM |
| 5 | vsp_rpt_rbtmlm | Generate reports | LOW |

### Other Critical Integrations

- **Fulfillment:** FW_* tables (674K–681K rows) — 3rd party logistics
- **Order Portal:** SOPortal (646K rows) — Web-based ordering
- **Change Detection:** `changes` table (531K rows) — Triggers syncs

---

## Shopify Migration Impact

### What to Replicate
1. **NOP Sync Procedures** → **Shopify API integrations** or **Zapier/custom webhooks**
2. **Order Sync** → **Shopify order webhook** → AccountMate
3. **Inventory Sync** → **Shopify product API** + scheduled sync job
4. **MLM Integration** → **Custom Shopify app** or **external service**

### Custom Development Needed
1. **Shopify Product Sync App** (replaces sp_NOP_sync*)
2. **Shopify Order Importer** (replaces sp_NOP_syncOrders)
3. **MLM Commission Engine** (keep sp_ct_Rebates logic, interface with Shopify)
4. **Fulfillment Warehouse Integration** (keep fw_* sync)

### Data Migration Strategy
- **Preserve:** All NOP sync logic in custom Shopify app
- **Replicate:** Change tracking (`changes` table) → Shopify change hooks
- **Archive:** Legacy NOP tables after cutover

---

**End of Integration Map**
