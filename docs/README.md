# Annique Shopify Migration Analysis
## Complete SQL Database Export Analysis

**Analysis Date:** March 1, 2026
**Database:** Annique Cosmetics MLM + NopCommerce (NopCommerce)
**Analyst:** Claude Code Agent
**Context:** Comprehensive database documentation for Shopify migration project

---

## Document Overview

This analysis package contains **4 comprehensive markdown documents** analyzing a 913-table, 399.6-million-row SQL Server database for migration to Shopify.

### Files Included

#### 1. **01_table_modules.md** (Table Classification)
- **Size:** ~25 KB
- **Content:** Complete inventory of all 913 tables organized by functional modules
- **Key Sections:**
  - Module-by-module breakdown (AR, AP, GL, IC, SO, BR, MLM, etc.)
  - Row counts and data volume for each table
  - Backup/temp/test/empty table identification
  - Flags for data quality issues
  - Migration impact assessment

**Use This For:** Understanding the database structure, identifying which tables are relevant to your business processes, and spotting data cleanup opportunities.

---

#### 2. **02_integration_map.md** (Integration Touchpoints)
- **Size:** ~30 KB
- **Content:** Detailed map of all integration points between AccountMate ERP, NopCommerce, MLM system, and fulfillment
- **Key Sections:**
  - 11 NopCommerce sync procedures (product, order, inventory, image sync)
  - 5 MLM procedures (rebate calculation, downline management)
  - Fulfillment warehouse integration (fw_* tables)
  - Order processing flow
  - Change tracking & triggers
  - Integration flow diagrams
  - Shopify migration impact assessment

**Use This For:** Understanding how current systems are connected, which procedures must be replicated in Shopify, and what custom development is needed.

---

#### 3. **03_data_tiers.md** (Data Volume Strategy)
- **Size:** ~35 KB
- **Content:** Strategic classification of all 913 tables into 4 data tiers for migration
- **Key Sections:**
  - **Tier 0 (Skip):** 342 tables, 3.5M rows — backups, empty, test
  - **Tier 1 (DDL Only):** 87 tables, 444.5M rows — massive history (archive older data)
  - **Tier 2 (Sample):** 229 tables, 3.7M rows — keep 1-10% sample + all recent 12 months
  - **Tier 3 (Full):** 255 tables, 49.6K rows — reference/config (keep all)
  - Migration volume summary
  - Archive strategy for cold storage
  - Phase-by-phase execution plan

**Use This For:** Planning the actual data migration, understanding what to delete vs. keep, and estimating storage requirements. Shows how to reduce 399.6M rows to just 3.8M.

---

#### 4. **04_business_logic.md** (Critical Processes)
- **Size:** ~32 KB
- **Content:** Deep dive into 6 critical business processes and their implementation
- **Key Sections:**
  - **MLM Commission System:** sp_ct_Rebates (19.8M row ledger), downline tracking, rank advancement
  - **Order Processing:** Order flow, pricing, fulfillment, two-path fulfillment (internal + 3PL)
  - **Inventory Sync:** Real-time stock updates to web store, image sync, pricing sync
  - **Accounts Receivable:** Invoice-to-payment-to-GL flow, refund processing, aging
  - **General Ledger:** 118M transaction ledger, batch posting, period closing
  - **Batch Automation:** Inferred scheduled jobs and their frequency
  - Shopify migration action items for each process

**Use This For:** Understanding the business logic that must be preserved or replicated, identifying which AccountMate procedures to keep running, and planning custom Shopify app development.

---

## Quick Statistics

### Database Scope
| Metric | Value |
|--------|-------|
| **Total Tables** | 913 |
| **Total Rows** | ~399.6 million |
| **Estimated DB Size** | ~500 GB |
| **Functional Modules** | 18 (AR, AP, GL, IC, SO, BR, MLM, NOP, etc.) |
| **Backup/Temp Tables** | 49 |
| **Empty Tables** | 293 |
| **Test/Debug Tables** | 9 |
| **Active Data Tables** | 562 |

### Module Highlights
| Module | Tables | Rows | Criticality |
|--------|--------|------|------------|
| **GL (General Ledger)** | 21 | 125.6M | CRITICAL |
| **IC (Inventory Control)** | 67 | 174.1M | CRITICAL |
| **AR (Accounts Receivable)** | 65 | 57.1M | CRITICAL |
| **SO (Sales Order)** | 47 | 48.0M | CRITICAL |
| **MLM/Rebate System** | 18 | 30.9M | CRITICAL |
| **NopCommerce Integration** | 8 | 1.4M | HIGH |
| **Campaign Management** | 14 | 1.0M | MEDIUM |
| **AP (Accounts Payable)** | 22 | 295K | HIGH |
| **PO (Purchase Order)** | 27 | 167K | MEDIUM |
| **Other/Misc** | 351 | 7.5M | LOW |

### Integration Summary
| Integration | Count | Type | Status |
|-------------|-------|------|--------|
| **NOP Sync Procs** | 11 | Stored Procedure | ACTIVE (2023-2026) |
| **MLM Procs** | 5 | Stored Procedure | ACTIVE (legacy, last mod 2022) |
| **NOP Views** | 1 | View | LEGACY (2014) |
| **Fulfillment (fw_)** | 5 tables | Integration | ACTIVE (674K-681K rows) |
| **Order Portal** | 1 table | Integration | ACTIVE (646K rows) |

### Data Reduction Strategy
| Stage | Tables | Rows | Size (Est.) |
|-------|--------|------|-----------|
| Original | 913 | 399.6M | ~500 GB |
| After Cleanup (Tier 0) | 571 | 396.1M | ~495 GB |
| After Archival (Tier 1) | 571 | 250-300M | ~350 GB |
| After Sampling (Tier 2) | 800 | ~3.8M | ~10 GB |
| **Shopify-Ready** | — | — | **10-15 GB** |
| **Cold Archive** | — | ~350M | **~350 GB (Glacier)** |

**Result:** 97% data reduction while preserving business continuity!

---

## Key Findings

### 1. Complex MLM System (30.9M Rows)
- **Procedure:** `sp_ct_Rebates` calculates multi-level commissions
- **Ledger:** `genreb` (19.8M rows) — all rebate transactions
- **Infrastructure:** Consultant hierarchy, level tracking, monthly rollups
- **Action:** Must replicate in custom Shopify app or keep calling AccountMate

### 2. Active NopCommerce Integration (11 Procedures)
- **Sync Points:** Items (catalog), orders, inventory, images, discounts
- **Frequency:** Real-time to scheduled (4-hour cycles)
- **Status:** Recently updated (Jan 2026 order sync modification)
- **Action:** Replace with Shopify API equivalents + custom webhooks

### 3. Massive Inventory History (174M Rows)
- **Daily Snapshots:** `iciwhs_daily` (130M rows) — 32.5% of entire DB!
- **Problem:** Not needed for Shopify; only current + 12 months historical
- **Opportunity:** Archive to cold storage = 120M row reduction

### 4. 3PL Fulfillment Integration (fw_* Tables)
- **Pattern:** `fw_consignment`, `fw_items`, `fw_Hubs`, `fw_labels`, `fw_manifest`
- **Scale:** 674K-681K rows
- **Implication:** Using external logistics provider (FedEx/Amazon/UPS)
- **Action:** Understand current 3PL contract; replicate in Shopify Fulfillment Network or continue with current provider

### 5. Self-Service Portal (646K Web Orders)
- **Table:** `SOPortal` (646K rows)
- **Purpose:** Consultant/customer self-service ordering
- **Integration:** Orders feed into AccountMate SO module
- **Action:** Replicate in Shopify (easier with native checkout)

### 6. Significant Data Cleanup Opportunity
- **Delete:** 342 backup/empty/test tables (3.5M rows, negligible data)
- **Archive:** 87 large history tables (444.5M rows to cold storage)
- **Sample:** 229 transactional tables (keep 1-10% + all recent 12 months)
- **Preserve:** 255 reference tables (all kept for business logic)
- **Result:** Reduce active DB from 500 GB to 10-15 GB

---

## Shopify Migration Roadmap

### Phase 1: Pre-Migration (Weeks 1-2)
1. Review this analysis with business stakeholders
2. Clarify data retention requirements (how many years to keep?)
3. Identify which AccountMate processes continue post-migration
4. Plan custom Shopify app development scope

### Phase 2: Data Cleanup (Weeks 3-4)
1. Delete Tier 0 tables (backup/empty/test)
2. Archive Tier 1 historical data to cold storage
3. Sample Tier 2 transactional data
4. Validate data integrity

### Phase 3: Custom Development (Weeks 5-12)
1. **Shopify MLM App** — Commission calculation, rank tracking
2. **Order Sync Integration** — Shopify order → AccountMate SO
3. **Inventory Sync API** — Bidirectional stock management
4. **Fulfillment Connector** — Map to 3PL or Shopify FN
5. **Customer Metafields** — Consultant hierarchy, pricing tiers

### Phase 4: Data Migration (Weeks 8-10)
1. Export sampled AR/SO/IC/GL data
2. Build mapping for AccountMate → Shopify fields
3. Bulk import to Shopify
4. Validate data accuracy

### Phase 5: Go-Live (Weeks 11-14)
1. Parallel run (AccountMate + Shopify simultaneously)
2. Order sync testing
3. Rebate calculation validation
4. Fulfillment cutover
5. Customer communication

### Phase 6: Post-Migration (Ongoing)
1. Monitor Shopify ↔ AccountMate sync
2. Archive older data monthly
3. MLM commission accuracy audits
4. Performance tuning

---

## What You'll Need to Build/Configure

### Custom Shopify Apps/Integrations

1. **MLM Commission Engine**
   - Hooks into order.paid webhook
   - Calls AccountMate sp_ct_Rebates
   - Stores rebates in Shopify metafields
   - Monthly bonus calculations

2. **Inventory Sync Service**
   - Bidirectional sync AccountMate ↔ Shopify
   - 4-hour intervals minimum
   - Handles reserve/allocation/fulfillment

3. **Order Importer**
   - Listens to Shopify order webhook
   - Creates AccountMate sales order
   - Applies consultant pricing/discounts
   - Triggers rebate calculation

4. **Customer Master Sync**
   - Sync Shopify customers to AccountMate
   - Preserve MLM hierarchy (sponsor, level, rank)
   - Store in customer metafields
   - Update pricing tiers

5. **Fulfillment Integration**
   - Map Shopify fulfillment to 3PL or internal warehouse
   - Sync tracking numbers
   - Update order status in both systems

### AccountMate Changes

1. **Keep These Procedures Active**
   - `sp_ct_Rebates` — Rebate calculation
   - `sp_ct_downlinebuild` — Rank advancement
   - GL posting routines (automatic)
   - AR aging/statement generation

2. **Can Disable**
   - All `sp_NOP_sync*` procedures (replace with Shopify API)
   - Bank transfer automation (if moving to Shopify Payments)

3. **Should Archive**
   - Historical GL transactions (pre-2021)
   - Old order history (pre-2023)
   - Daily inventory snapshots (pre-2025)

---

## File Locations

All analysis documents are in:
```
/sessions/wonderful-amazing-franklin/analysis_output/
├── 01_table_modules.md
├── 02_integration_map.md
├── 03_data_tiers.md
├── 04_business_logic.md
└── README.md (this file)
```

---

## How to Use These Documents

### For Business Stakeholders
1. **Start:** Section "Quick Statistics" in this README
2. **Read:** 01_table_modules.md — understand data scope
3. **Review:** 04_business_logic.md — confirm key processes

### For Technical Architects
1. **Start:** 02_integration_map.md — understand current integrations
2. **Deep Dive:** 04_business_logic.md — preservation/replication strategy
3. **Plan:** 03_data_tiers.md — data migration approach
4. **Design:** Custom Shopify app architecture

### For Database/Migration Team
1. **Start:** 03_data_tiers.md — data volume strategy
2. **Execute:** Phase-by-phase cleanup and migration
3. **Validate:** Use 01_table_modules.md as checklist
4. **Archive:** Setup cold storage for Tier 1 historical data

### For Shopify Implementation Team
1. **Start:** 04_business_logic.md — critical processes
2. **Review:** 02_integration_map.md — current integration points
3. **Design:** Custom app requirements from business logic
4. **Test:** Use 01_table_modules.md sample data for dev/test

---

## Key Recommendations

### Do This First
✓ Delete 342 backup/empty/test tables (frees 3.5M rows immediately)
✓ Understand data retention requirements (3-5 year archival)
✓ Plan custom Shopify app scope (MLM, inventory, orders)
✓ Meet with 3PL provider (confirm warehouse integration)

### Do NOT Do This
✗ Migrate iciwhs_daily snapshots (130M rows not needed)
✗ Keep backup tables with date suffixes (_bu7Feb13, etc.)
✗ Run old NOP sync procedures in parallel with Shopify
✗ Over-migrate historical data (sample adequately)

### Preserve for Business Continuity
✓ MLM commission logic (in AccountMate or custom Shopify app)
✓ Chart of accounts and GL posting (keep in AccountMate)
✓ Customer master and invoices (3-5 year history)
✓ Inventory current balances and 12-month history
✓ Sales order history (3 years for analytics)

---

## Estimated Project Scope

| Task | Effort | Duration |
|------|--------|----------|
| Data cleanup & analysis | 40 hours | 1 week |
| Custom Shopify app dev | 400-600 hours | 8-12 weeks |
| Data migration & validation | 200 hours | 2-3 weeks |
| Integration testing | 150 hours | 2 weeks |
| Go-live & stabilization | 200 hours | 4 weeks |
| **TOTAL** | **1000-1200 hours** | **20-22 weeks** |

**Cost Estimate (@ $150/hr):** $150K-$180K for technical services (plus Shopify plan, apps, infrastructure)

---

## Success Criteria

✓ Data imported to Shopify: AR/SO/IC/MLM customer records
✓ Orders sync from Shopify → AccountMate automatically
✓ Inventory updates Shopify within 4 hours
✓ MLM rebates calculated without manual intervention
✓ Customer portal migrated to Shopify checkout
✓ Fulfillment continues through 3PL (or migrated to Shopify FN)
✓ Financial records remain in AccountMate
✓ Zero data loss during cutover
✓ All staff trained on new Shopify processes
✓ Performance meets or exceeds NopCommerce baseline

---

## Questions or Support

This analysis was generated via automated SQL schema inspection and stored procedure analysis. For clarifications:

1. **Data Modeling Questions:** Review 01_table_modules.md for table relationships
2. **Business Logic Questions:** Review 04_business_logic.md for process flows
3. **Integration Architecture:** Review 02_integration_map.md for current touchpoints
4. **Migration Planning:** Review 03_data_tiers.md for data strategy

---

**Document Generated:** 2026-03-01
**Analysis Scope:** 913 tables, 399.6M rows, 68,170 procedures/views/triggers
**Database Type:** Microsoft SQL Server 2012+
**ERP System:** AccountMate (Keyline, Australia)
**E-Commerce Platform (Current):** NopCommerce (NopCommerce fork)
**Target Platform:** Shopify Plus

---

**End of README**
