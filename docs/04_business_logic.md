# Key Business Logic Summary
## Critical Processes for Shopify Migration

**Analysis Date:** 2026-03-01
**Database:** Annique AccountMate ERP + NopCommerce

---

## Executive Summary

The Annique database implements **complex business logic** across five critical domains:

1. **MLM Commission System** — Multi-level rebate calculations with downline tracking
2. **Order Processing** — E-commerce order intake, fulfillment, and tracking
3. **Inventory Synchronization** — Real-time stock updates to web store
4. **Accounts Receivable** — Customer billing, payment collection, refunds
5. **General Ledger** — Financial posting and reconciliation

Each domain requires custom Shopify app development or integration to preserve business rules during migration.

---

## Critical Process 1: MLM Commission Calculation

### Procedure: `sp_ct_Rebates`
- **Type:** Stored Procedure
- **Schema Name:** dbo
- **Size:** 13,126 characters (substantial logic)
- **Last Modified:** 2022-06-02
- **Input Tables:**
  - `ARINVC` — Customer invoices (source of sales)
  - `icitem` — Item master (contains rebate rates)
  - `compplanlive` — Active compensation plan configuration
- **Output Tables:**
  - `CurUpdateRebate` — Current rebate amounts
  - `ARITRS` — Transaction result tracking
  - `rebatedtl` — Detailed rebate records
  - `rbtmlm` — MLM rebate template
  - `genreb` — General rebate ledger (19.8M rows)

### Business Logic Flow

```
Sales Invoice Created
        ↓
Lookup Item Rebate Percentage
        ↓
Apply Consultant Commission Formula:
├─ Commission % from item master
├─ Level multiplier (Level 1, 2, 3...)
├─ Volume multiplier (if applicable)
└─ Territory/region adjustments (if any)
        ↓
Distribute to Upline:
├─ Pay sponsor (Level 1 up)
├─ Pay sponsor's sponsor (Level 2 up)
└─ Possibly deeper (Level 3+)
        ↓
Write to:
├─ genreb (ledger)
├─ rebatedtl (detail)
├─ CurUpdateRebate (current totals)
└─ ARITRS (transaction posting)
        ↓
Generate Commission Statement
```

### Key Business Rules Inferred

1. **Multi-Level Commission Distribution**
   - Commissions flow upward through sponsor chain
   - Each level receives a percentage of sale
   - Likely decreasing percentages at deeper levels

2. **Item-Level Rebate Rates**
   - Not all products have same commission percentage
   - Stored in `icitem` table (rebate_rate or similar field)
   - May vary by product category or supplier

3. **Consultant Levels/Tiers**
   - Stored in `Level1`, `Level2`, `Level1a`, `Level2a` tables
   - Different commission structures per level
   - Level advancement based on downline size/sales (inferred)

4. **Monthly Aggregation**
   - `rbtmlmMONTHLY` (2.8M rows) — Monthly rebate totals
   - `rebatedtlMONTHLY` (3.3M rows) — Monthly detail rollups
   - Suggests monthly commission cycles (common in MLM)

5. **Compensation Plan Flexibility**
   - `compplanlive` table indicates multiple plans can coexist
   - New plans can be activated without recalculating history
   - Supports plan changes mid-season

### For Shopify Migration

**Challenge:** MLM logic is **incompatible with Shopify's native order system**.

**Solution Options:**

1. **Shopify Plus Custom App** (Recommended)
   - Create custom Shopify app that hooks into order.paid webhook
   - Calls AccountMate API to trigger sp_ct_Rebates
   - Maintains consultant hierarchy in separate system
   - Writes rebates back to Shopify customer notes/metafields

2. **External MLM Engine**
   - Separate Node.js/Python service
   - Listens to Shopify webhooks
   - Calculates commissions independently
   - Syncs back to Shopify and AccountMate

3. **Hybrid Approach**
   - Keep AccountMate as MLM backend
   - Shopify feeds orders to AccountMate
   - AccountMate calculates rebates
   - Results sync back to Shopify customer records

**Recommended:** Option 3 (hybrid) — leverages existing AccountMate logic

---

## Critical Process 2: Downline Structure & Rank Advancement

### Procedure: `sp_ct_downlinebuild`
- **Type:** Stored Procedure
- **Schema Name:** dbo
- **Size:** 1,812 characters
- **Last Modified:** 2016-10-28
- **Input Tables:**
  - `CustSPNLVL` — Customer sponsor level mapping (144,754 rows)
  - Functions: `fn_get_downlineCT`, `fn_get_titleall`
- **Output Tables:**
  - `CTDownline` — Recalculated downline
  - `CTDownlineh` — Downline history
  - `CTCons` — Consultant status/titles

### Business Logic

```
For Each Consultant:
  ├─ Fetch sponsor path (upline tree)
  ├─ Calculate total downline members
  ├─ Get current rank/title
  ├─ Evaluate rank advancement criteria:
  │   ├─ Downline size threshold?
  │   ├─ Personal sales volume?
  │   └─ Team sales volume?
  ├─ Update rank if criteria met
  ├─ Calculate rank-based bonuses
  └─ Store in CTDownline + CTDownlineh
```

### Key Data Structure

**CustSPNLVL (144,754 records):**
- Likely columns: customer_id, sponsor_id, level (depth)
- Creates parent-child relationships
- Used by recursive downline functions

**Rank System:**
- `fn_get_titleall` suggests title/rank assignment
- Ranks like: Consultant → Manager → Director → etc.
- Rank advancement triggers bonus pools

### For Shopify Migration

**Challenge:** Shopify doesn't natively support MLM hierarchies.

**Solution:** Build Shopify app to:
1. Sync CustSPNLVL relationships
2. Store in Shopify metafields (customer._mlm_sponsor_id, _mlm_level, _rank)
3. Calculate rank advancement on monthly schedule
4. Assign rank-based pricing/discounts in Shopify

---

## Critical Process 3: Order Processing Flow

### Primary Tables

1. **sosord** (56,063 rows) — Sales order master
2. **soskit** (5.9M rows) — Kit/line-item details (MASSIVE!)
3. **sosppr** (260,147 rows) — Price per product
4. **soship** (55,516 rows) — Shipment master
5. **soshiph** (1.3M rows) — Shipment history

### Order Processing Steps

```
Customer Places Order (Shopify → AccountMate)
        ↓
sp_NOP_syncOrders (Proc #4 in integration map)
        ↓
Create SO Record in sosord
├─ sosord_id (new)
├─ customer_id (from Shopify)
├─ order_date (current)
├─ total_amount
├─ discount_applied
└─ shipping_method
        ↓
Create Line Items in soskit
├─ sosord_id (link)
├─ line_item_number
├─ item_code
├─ quantity
├─ unit_price
├─ line_total
└─ special_pricing (if any)
        ↓
Apply Pricing Rules (sosppr)
├─ Check for volume discounts
├─ Apply consultant pricing (if MLM)
├─ Apply campaign discounts
└─ Apply promotional codes
        ↓
Reserve Inventory
├─ Check iciwhs (warehouse balances)
├─ Hold stock for order
├─ Update icibal (item balance)
└─ Create icktrs (kit transaction)
        ↓
Process Fulfillment
├─ Allocate to warehouse (fw_*) or internal
├─ Generate picking list
├─ Create shipping label (be_waybill)
└─ Create manifest (somanifest)
        ↓
Update Order Status
├─ "Awaiting fulfillment" → sosord.status
├─ Sync back to Shopify (sp_NOP_syncOrderStatus)
├─ Update SOPortal (646K customer portal)
└─ Notify customer
        ↓
Ship Order
├─ Mark items shipped (soship)
├─ Add shipping tracking (be_waybill)
├─ Generate invoice (arinvc)
├─ Apply rebate calculation (sp_ct_Rebates)
└─ Close SO
```

### Order Types Detected

1. **Regular Web Orders** — From SOPortal (646K portal records)
2. **Consignment Orders** — soconsign (318K rows) — possibly bulk orders to vendors/distributors
3. **Kit/Bundle Orders** — soskit (5.9M rows) — pre-packaged combos
4. **Special Order Items** — soxitems (19K rows) — customizable/build-to-order

### Fulfillment Integration

**Two fulfillment paths:**

1. **Internal Warehouse** (AccountMate-managed)
   - Pick/pack/ship from ICW (inventory control warehouse)
   - Uses `iciwhs` for balances
   - Generates soship records

2. **3rd-Party Logistics** (fw_* integration)
   - Consign items to external warehouse (FedEx/Amazon/etc.)
   - fw_consignment (674K rows) tracks what's sent out
   - fw_items (681K rows) tracks FW inventory
   - Returns flow back through system

### For Shopify Migration

**Action Items:**

1. **Preserve Order Processing Logic**
   - Replicate sosord → soskit creation in Shopify order webhook
   - Custom app processes each Shopify order

2. **Pricing Engine**
   - Keep sosppr logic for volume/consultant discounts
   - Implement in Shopify app or external service

3. **Inventory Reservation**
   - Sync iciwhs updates from Shopify backorder/fulfillment
   - Maintain two-way sync with fulfillment

4. **Shipment Tracking**
   - Preserve be_waybill records for supply chain analytics
   - Map Shopify Fulfillment API to AccountMate shipping

---

## Critical Process 4: Inventory Synchronization

### Key Tables

1. **icitem** (10,676) — Product master
2. **icimlp** (772) — Item location pricing
3. **iciwhs** (101,970) — Warehouse inventory
4. **iciwhs_daily** (130M rows!) — Daily snapshots (MASSIVE)
5. **icibal** (325,206) — Item balance by location
6. **iciimg** (3,372) — Product images

### Real-Time Sync Process

```
Inventory Change Detected (Sales/Purchase/Adjustment)
        ↓
Update iciwhs (warehouse balance)
Update icibal (item balance)
        ↓
Trigger: changes table entry
        ↓
sp_NOP_syncItemAvailability (fires automatically or scheduled)
        ↓
Read updated inventory levels
        ↓
Push to Shopify Product API
├─ Update available quantity
├─ Update reserved quantity
├─ Set back-in-stock notifications
└─ Trigger "Item Available" webhook (if was out of stock)
```

### Pricing Sync

```
Price Changed in AccountMate (icitem or icimlp)
        ↓
Entry logged in changes table
        ↓
sp_NOP_syncItemChanges triggers
        ↓
Push to Shopify Product API
├─ Update base price
├─ Update compare-at price (MSRP)
├─ Update variant prices (if kits)
└─ Trigger Shopify webhook
```

### Image Sync

```
Image Added/Updated (iciimg table)
        ↓
Staged in iciimgUpdate or iciimgUpdateNOP
        ↓
sp_ws_UpdateImageNOP fires
        ↓
Upload to Shopify CDN
├─ Create Shopify Image record
├─ Assign to Product
└─ Set alt text from description
```

### The 130M Row Problem

**Table:** `iciwhs_daily` (129.98M rows) — Daily warehouse snapshots

**Questions:**
- Do you need **every day** of inventory history?
- Or just current + rolling 12-month?

**Recommendation:** Archive historical snapshots to cold storage. Shopify only needs current stock levels.

**Data Reduction:** Delete pre-2023 snapshots → **120M rows eliminated** (30% of DB)

### For Shopify Migration

**Technical Challenge:** Real-time sync at scale

**Solution Architecture:**

1. **Event-Driven Sync**
   - Shopify Product Inventory API
   - AccountMate triggers → Shopify API calls
   - Queue system (SQS/RabbitMQ) to handle volume

2. **Scheduled Sync Job**
   - Run every 4 hours
   - Compare AccountMate iciwhs to Shopify inventory
   - Reconcile discrepancies
   - Log in changes table

3. **Inventory Visibility**
   - Shopify shows accountable inventory from iciwhs
   - Reduce available quantity by pending/processing orders
   - Handle backorders with Shopify's backorder app

---

## Critical Process 5: Accounts Receivable & Refunds

### Invoice Processing

```
Order Shipped
        ↓
Generate Invoice (arinvc table)
├─ Create AR record
├─ Line items from soskit
├─ Apply taxes (costax table)
├─ Calculate freight (arfrgt)
└─ Total amount due
        ↓
Account Posting (arinvc + arrevn for revenue)
├─ Post to GL (gltrsn)
├─ Debit: Accounts Receivable (arinvc)
├─ Credit: Sales Revenue (arrevn)
└─ Credit: Tax Payable (costax)
        ↓
Customer Receives Invoice
├─ Email/portal notification
├─ Payment terms applied (arpycd = payment code)
└─ Due date calculated
        ↓
Payment Received (arcash records)
├─ Create cash receipt (arcash: 81K rows)
├─ Allocate to invoice (arcapp: 201K rows)
├─ Post to GL (gltrsn)
└─ Update customer balance (arcust)
```

### Refund Processing

```
Customer Requests Refund (arrefund: 827 rows)
        ↓
Create Refund Record (arrfnd: 303K rows)
├─ Link to original invoice
├─ Reason code (arfrgt or similar)
├─ Amount & authorization
└─ GL accounts affected
        ↓
Approve Refund
        ↓
Process Refund
├─ Reverse invoice posting (gltrsn)
├─ Credit: Accounts Receivable
├─ Debit: Sales Revenue (reversal)
└─ Create AR credit memo
        ↓
Payment Method Refund
├─ Credit card refund
├─ Bank transfer
├─ Store credit (if applicable)
        ↓
Post to GL (final posting)
```

### Statement Generation

```
Monthly: sp_statement processes (arstmt: 18K rows)
├─ Summarize outstanding invoices per customer
├─ Calculate aging (0-30, 31-60, 61-90, 90+)
├─ Apply finance charges if overdue (arfchg)
├─ Generate PDF statement
└─ Email to customer
```

### Key AR Tables

| Table | Rows | Purpose |
|-------|------|---------|
| `arinvc` | 275,883 | Active invoices |
| `arinvch` | 5.8M | Invoice history (archive to 5 years) |
| `arcash` | 81,667 | Cash receipts |
| `arcashh` | 1.4M | Receipt history (archive) |
| `arcapp` | 201,173 | Invoice applications (payment allocation) |
| `arrfnd` | 303,752 | Refund master |
| `arstmt` | 18,815 | Customer statements |
| `arcust` | 119,616 | Customer master (CRITICAL) |

### For Shopify Migration

**Action Items:**

1. **Preserve Invoice Numbering**
   - Keep arinvc sequential (don't reset)
   - Map Shopify orders to AccountMate invoices

2. **Customer Master Sync**
   - Migrate arcust to Shopify customer records
   - Preserve credit limit, payment terms
   - Link MLM rank/sponsor info (metafields)

3. **Payment Processing**
   - Shopify Payments → AccountMate arcash
   - Payment status webhook → arcapp (allocation)
   - GL posting remains in AccountMate

4. **Refund Workflow**
   - Shopify refund → AccountMate arrfnd
   - GL reversal automatic
   - Customer statement reflects refund

---

## Critical Process 6: General Ledger & Financial Posting

### GL Architecture

```
Transaction Created (anywhere in system)
├─ Sales order → arinvc (AR)
├─ Payment received → arcash (AR)
├─ Invoice posted → AP (apinvc)
├─ Inventory adjustment → icibal (IC)
└─ Rebate calculated → rebatedtl (MLM)
        ↓
GL Batch Job (runs nightly/hourly)
        ↓
Create gltrsn Record (118M rows!)
├─ Source: Each transaction module
├─ Account code: From master (glacct)
├─ Amount: Debit or credit
├─ Date: Transaction date
├─ Reference: Module + doc ID
└─ Period: Accounting period (coacpd)
        ↓
Posting Summary (glbatc: 8,915 rows)
├─ Batch ID
├─ Total debits = total credits (balanced)
├─ Posted flag
└─ Posted date
        ↓
Period Closing (monthly)
├─ Lock coacpd (accounting period)
├─ Post closing entries (if fiscal month-end)
├─ Generate trial balance
└─ GL report ready
```

### Key GL Tables

| Table | Rows | Purpose |
|-------|------|---------|
| `gltrsn` | 118.3M | ALL GL transactions (core ledger) |
| `glacct` | 9,439 | Chart of accounts |
| `coacpd` | 351 | Accounting periods (12/year × history) |
| `glbatc` | 8,915 | GL batch posting control |
| `gljeid` | 18,226 | Journal entry ID tracking |
| `glbugt` | 17,825 | Budget master |

### For Shopify Migration

**Challenge:** Shopify doesn't have GL integration. Account must remain in AccountMate.

**Solution:**

1. **Keep AccountMate GL**
   - Shopify orders → API call to AccountMate
   - AccountMate triggers GL posting (arinvc → gltrsn)
   - GL remains source of truth for accounting

2. **Archive Old Transactions**
   - Keep only 3-5 years in active DB
   - Archive pre-2021 gltrsn to cold storage
   - Reduces gltrsn from 118M to ~50M rows

3. **Audit Trail**
   - coaudt (89K rows) provides audit
   - Preserve for compliance/SOX requirements

---

## Batch Processes & Automation

### Likely Scheduled Jobs (Inferred)

| Process | Frequency | Procedure | Source |
|---------|-----------|-----------|--------|
| **Rebate Calculation** | Daily or per-invoice | `sp_ct_Rebates` | Sales invoice posting |
| **Downline Rebuild** | Monthly | `sp_ct_downlinebuild` | Month-end |
| **Inventory Snapshot** | Daily | (implicit) | Nightly job |
| **NOP Sync - Items** | 4 hours | `sp_NOP_syncItemAll` | Scheduled job |
| **NOP Sync - Orders** | Real-time or hourly | `sp_NOP_syncOrders` | Order received |
| **NOP Sync - Availability** | 4 hours | `sp_NOP_syncItemAvailability` | Scheduled job |
| **NOP Sync - Changes** | Real-time | `sp_NOP_syncItemChanges` | Change detected |
| **AR Statement Gen** | Monthly | (implicit) | Month-end |
| **GL Batch Posting** | Nightly | (implicit) | EOD job |
| **Bank Reconciliation** | Weekly/Monthly | (Bank import) | Manual upload |

### For Shopify Migration

**Build Automated Workflows:**

1. **Daily/Hourly Inventory Sync** → Shopify Product API
2. **Order Receipt Webhook** → AccountMate SO creation
3. **Shipment Webhook** → AccountMate fulfillment update
4. **Monthly Rebate Batch** → Keep in AccountMate, report to Shopify
5. **Payment Reconciliation** → Shopify Payments API

---

## Summary: Critical Processes to Preserve

### Must-Have in Shopify Integration

1. **MLM Commission Calculation** (sp_ct_Rebates)
   - Keep logic in AccountMate
   - Trigger from Shopify order webhook
   - Status back to Shopify customer records

2. **Downline Management** (sp_ct_downlinebuild)
   - Sync CustSPNLVL to Shopify metafields
   - Calculate rank advancement monthly
   - Apply rank-based pricing/discounts

3. **Order-to-Invoice Flow**
   - Shopify order → AccountMate SO → Invoice → Rebate
   - Preserve sequential invoice numbering
   - Maintain full audit trail

4. **Inventory Real-Time Sync**
   - AccountMate iciwhs ↔ Shopify inventory API
   - 4-hour sync frequency minimum
   - Handle warehouse allocation

5. **GL Integration**
   - Keep AccountMate as GL backend
   - Shopify orders post debits/credits
   - AR aging/reporting from AccountMate

### Can Improve/Simplify

1. **Daily Inventory Snapshots** (iciwhs_daily)
   - Archive historical; keep 12 months for reporting

2. **Backup Table Cleanup**
   - Delete 49 backup tables (3.5M rows)

3. **Test/Debug Tables**
   - Clean up 9 debug tables before migration

4. **Empty Schema Tables**
   - Drop 293 unused tables

---

**End of Business Logic Summary**

---

## Appendix: Stored Procedure Cross-Reference

| Procedure | Module | Purpose | Modified |
|-----------|--------|---------|----------|
| sp_NOP_syncItemAll | ECOM | Sync product catalog | 2024-09-03 |
| sp_NOP_syncItemAvailability | ECOM | Sync inventory | 2024-09-03 |
| sp_NOP_syncItemChanges | ECOM | Sync product changes | 2024-09-03 |
| sp_NOP_syncOrders | ECOM | Import web orders | 2026-01-09 (LATEST!) |
| sp_NOP_syncOrderStatus | ECOM | Sync order status | 2024-10-28 |
| sp_NOP_syncSoxitems | ECOM | Sync special orders | 2023-09-29 |
| sp_ws_UpdateImageNOP | ECOM | Sync product images | 2023-03-22 |
| sp_NOP_DiscountINS | ECOM | Manage discounts | 2024-06-13 |
| sp_NOP_OfferINS | ECOM | Manage offers | 2024-06-13 |
| sp_NOP_OfferListINS | ECOM | List offers | 2024-06-13 |
| NOP_UpdateExclusiveItemCode | ECOM | Update SKUs | 2025-09-29 (NEWEST!) |
| sp_ct_downlinebuild | MLM | Recalc downline | 2016-10-28 |
| sp_ct_Rebates | MLM | Calculate rebates | 2022-06-02 |
| vsp_mlm_getnewdocno | MLM | Generate doc IDs | 2013-08-20 |
| vsp_mlm_getnewmlminvc | MLM | Generate invoice IDs | 2013-08-20 |
| vsp_rpt_rbtmlm | MLM | Generate rebate report | 2013-08-20 |
| vsp_brtfer_transfer_auto | Bank | Auto bank transfer | 2009-09-14 (LEGACY!) |

**Key Insight:** Most NOP sync procedures updated in 2024, showing active development. MLM procedures are older (2012-2018), suggesting stable legacy code.

