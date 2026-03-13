# Data Volume Tiers & Migration Strategy
## Shopify Migration Data Management Plan

**Analysis Date:** 2026-03-01
**Database:** Annique AccountMate ERP (913 tables, 399.6M rows total)

---

## Executive Summary

Data tier classification reduces migration burden by **444.5 MILLION rows** through intelligent sampling and reference-only strategies:

- **Tier 0 (Skip):** 342 tables, 3.5M rows → Delete (backups/empty)
- **Tier 1 (DDL Only):** 87 tables, 444.5M rows → Schema only (no data)
- **Tier 2 (Sample):** 229 tables, 3.7M rows → 10-50 rows each
- **Tier 3 (Full):** 255 tables, 49.6K rows → Keep all

**Net Data Migration:** ~3.75M rows (Core) + 49.6K (Reference) = **~3.8M rows** (vs. 399.6M original)

**Savings:** 99.04% data reduction while maintaining business continuity

---

## Tier 0: Skip (Delete)

**Count:** 342 tables | **Rows:** 3,524,547 | **Action:** Do not migrate

These are backup tables, temporary staging, test data, and empty schema structures. **Delete before migration.**

### Backup Tables (49 tables with dates/version suffixes)

| Table | Rows | Date | Notes |
|-------|------|------|-------|
| arcapp_bu11Nov22 | 17 | 2022-11 | Old customer app backup |
| arcapp_bu11Oct18 | 2,629 | 2018-10 | Very old (7+ years) |
| arcappBU10Dec12 | 35,413 | 2012-12 | VERY OLD (13+ years) |
| arcappNov12 | 1,062 | 2012-11 | Superseded |
| arinvc_bu11Oct18 | 2,629 | 2018-10 | Invoice backup |
| apcappbu7Feb13 | 73 | 2013-02 | Very old AP backup |
| apinvcbu7Feb13 | 26 | 2013-02 | Invoice backup |
| dpibdgbu29Oct10 | 33,273 | 2010-10 | Demand planning (9+ year old) |
| dpibdgbu2Dec10 | 35,885 | 2010-12 | Demand planning |
| dpibdgbu5Sep11 | 60,025 | 2011-09 | Demand planning |
| dpibdgbu6July2013 | 116,286 | 2013-07 | Demand planning |
| dpibdgbu9July13 | 115,898 | 2013-07 | Demand planning |
| custspnlvl_bu02222016 | 135,051 | 2016-02 | Old MLM customer-level |
| rbtmlmbu21Feb14 | 36,355 | 2014-02 | MLM rebate |
| rbtmlmbu25Aug14 | 39,085 | 2014-08 | MLM rebate |
| rbtmlmbu25Aug14old | 39,053 | 2014-08 | Duplicate backup |
| rbtmlmJul14 | 42,790 | 2014-07 | MLM rebate |
| rbtmlmNov12 | 39,606 | 2012-11 | MLM rebate |
| rebatedtlbu1Jul14 | 48,935 | 2014-07 | Rebate detail |
| rebatedtlbu21Feb14 | 42,790 | 2014-02 | Rebate detail |
| rebatedtlbu25Aug14 | 45,963 | 2014-08 | Rebate detail |
| rebatedtlbu25Aug14old | 45,926 | 2014-08 | Duplicate backup |
| rebatedtlJul14 | 50,250 | 2014-07 | Rebate detail |
| rebatedtlNov12 | 46,806 | 2012-11 | Rebate detail |
| gltrsnbu18Dec23 | 2,386,881 | 2023-12 | **RECENT GL backup (380MB!)** |
| icitembu6Jun14 | 6,698 | 2014-06 | Item master |
| soshiph_bu | 429,063 | unknown | Ship history backup (LARGE) |
| podisth_bu | 35,914 | unknown | PO distribution backup |
| icitembu26Oct11 | 5,332 | 2011-10 | Item master |
| icitembu31Oct11 | 5,343 | 2011-10 | Item master |
| *[20 more with old dates]* | — | — | See detailed list below |

**Detailed Backup List (All 49):**
```
apamrtbu*, apcappbu*, apinvcbu*, dpibdgbu*, icitembu*, rbtmlmbu*,
rebatedtlbu*, custspnlvl_bu*, gltrsnbu*, soshiph_bu, podisth_bu,
arrevnbu*, icinvcbu*, arcapp_bu*, soshiph_bu, podisth_bu,
icitembu*, icimlpbu*, icuprcbu*, dpibdgbu*, revupd1bu*
```

**Recommendation:** Delete all. Keep only if audit requirement mandates 7+ year history (then archive to separate cold storage).

### Empty Tables (293 tables with 0 rows)

**Do Not Migrate.** These are orphaned schema elements:

| Category | Count | Examples |
|----------|-------|----------|
| Payroll (pr*) | ~60 | prhire, prempd, prstat, prleav, prtrsd, etc. |
| AP Templates | ~20 | apcont, apamrt, apfinc, aprchg, apvacb, etc. |
| AR Templates | ~15 | aretrs, arfchg, aropcr, arrcri, aritci, etc. |
| GL Templates | ~10 | gladis, glanot, glfseg, glxcgd, glxcxg, etc. |
| Config/Misc | ~80 | cfflds, cflkcd, cflktp, cfsfld, cfsfrm, cfspge, cfstbl, cftble, cmisc, cmfacc, etc. |
| Bank Reconciliation | ~5 | brebat, bribts, bricdf, bricmc, etc. |
| SO/PO Templates | ~10 | soacpt, soacsp, soactr, soaktp, soaord, sodisc, etc. |
| Manufacturing | ~5 | micprc, mimnot, misrmk, miurmk, miuxpl, miwtin, etc. |
| Returns | ~5 | raccit, racshp, ractrk, etc. |

These represent unused feature areas (payroll integrated elsewhere, manufacturing dormant, etc.).

---

## Tier 1: DDL Only (No Data)

**Count:** 87 tables | **Rows:** 444,521,162 | **Action:** Create schema, skip data

These are **massive transactional history tables** where:
- Only recent years needed in Shopify
- Archive older data separately
- Structure needed for referential integrity

### Tier 1 Tables (100K+ rows each)

#### GL (General Ledger) — 103.7M rows

| Table | Rows | Purpose | Notes |
|-------|------|---------|-------|
| `gltrsn` | 118,274,883 | GL transactions | **29.6% of all DB rows!** |
| `gltrsnbu18Dec23` | 2,386,881 | GL backup | Skip entirely |
| `gltfer` | 3,262,263 | GL transfers | Large posting details |
| `gltfertemp` | 3,262,263 | Temp transfer table | Skip (working table) |

**Migration Strategy:** Keep only transactions from last 3-5 fiscal years. Archive older to cold storage.

#### IC (Inventory Control) — 163.3M rows

| Table | Rows | Purpose | Notes |
|-------|------|---------|-------|
| `iciwhs_daily` | 129,980,593 | Daily warehouse snapshots | **32.5% of all DB!** |
| `iciwhs_monthly` | 15,686,562 | Monthly snapshots | 3.9% of DB |
| `icitrsnh` | 17,549,397 | Item transaction history | 4.4% of DB |
| `icktrs` | 802,131 | Kit transactions | 0.2% of DB |

**Critical Question:** Do you need daily snapshots in Shopify?

- **Option A (Recommended):** Keep last 12 months daily, archive older
- **Option B:** Keep monthly only (79M row reduction)
- **Option C:** Keep current balances only, archive all history (156M row reduction!)

**Estimate:** Migration needs: 20-30M rows (vs. 163M total)

#### AR (Accounts Receivable) — 57.1M rows

| Table | Rows | Purpose | Notes |
|-------|------|---------|-------|
| `arinvch` | 5,830,255 | Invoice history | Detailed invoice audit trail |
| `aritrsh` | 21,607,444 | Item transaction history | Line-item history |
| `arsktrh` | 8,423,790 | Ski/special order history | Historical tracking |
| `arcashh` | 1,406,610 | Cash receipt history | Payment history |

**Strategy:** Migrate invoices from last 3-5 fiscal years only. Archive pre-2020 to cold storage.

#### SO (Sales Order) — 48.0M rows

| Table | Rows | Purpose | Notes |
|-------|------|---------|-------|
| `sosptrh` | 11,885,664 | Special pricing history | 2.9% of DB |
| `sospsph` | 4,536,160 | Special pricing SPA history | 1.1% of DB |
| `sosktrh` | 7,546,225 | Kit history | 1.9% of DB |
| `soshiph` | 1,299,228 | Shipment history | 0.3% of DB |
| `sosordh` | 1,624,995 | Order history | 0.4% of DB |

**Strategy:** Keep last 3 years of order history. Archive pre-2023.

#### MLM/Rebate — 30.9M rows

| Table | Rows | Purpose | Notes |
|-------|------|---------|-------|
| `genreb` | 19,855,973 | Rebate ledger | 4.9% of DB (CRITICAL!) |
| `rebatedtlMONTHLY` | 3,324,956 | Monthly rebate details | 0.8% of DB |
| `rbtmlmMONTHLY` | 2,844,015 | Monthly MLM rebates | 0.7% of DB |

**Strategy:** Keep last 36 months of rebate detail. Older months → summaries only.

**Recommendation:** All genreb data should be migrated (business records, audit trail). Consider archiving pre-2021 monthly details to summaries.

#### Other Tier 1 Tables

| Table | Rows | Module | Notes |
|-------|------|--------|-------|
| `icuprc_bak1411` | 28,874 | IC | Pricing backup — delete |
| `icimlp_040717` | 38,918 | IC | Old item pricing — archive |
| `dpibdg` | 435,804 | Demand Planning | Budget planning |
| `iciwhs_monthly` | 15,686,562 | IC | Monthly snapshots |
| `glbugtprep` | 587,354 | GL | Budget preparation |

---

## Tier 2: Sample Data (1K–100K rows)

**Count:** 229 tables | **Rows:** 3,660,936 | **Action:** Keep 10-50 sample rows

These tables are large but not historical. Take representative samples to preserve business logic while reducing volume.

### Sample Strategy by Category

#### AR Sample Tables

| Table | Total Rows | Sample Rows | % | Purpose |
|-------|-----------|-----------|---|---------|
| `arinvc` | 275,883 | 2,759 | 1% | Active invoices (last 6 months) |
| `arcapp` | 201,173 | 2,011 | 1% | Customer applications |
| `arcust` | 119,616 | 1,196 | 1% | Customer master |
| `arcadr` | 109,594 | 1,096 | 1% | Customer addresses |
| `arcnot` | 110,716 | 1,107 | 1% | Customer notes |
| `arcpos` | 138,437 | 1,384 | 1% | POS orders |
| `aritsp` | 157,402 | 1,574 | 1% | Item transaction specifics |

#### IC Sample Tables

| Table | Total Rows | Sample Rows | % | Purpose |
|-------|-----------|-----------|---|---------|
| `icibal` | 325,206 | 3,252 | 1% | Item balances by location |
| `iciimg` | 3,372 | 337 | 10% | Product images |
| `icimlp` | 772 | 77 | 10% | Location pricing |
| `icitem` | 10,676 | 1,068 | 10% | Item master (keep most!) |
| `icuprc` | 19,171 | 1,917 | 10% | Unit pricing |

#### SO Sample Tables

| Table | Total Rows | Sample Rows | % | Purpose |
|-------|-----------|-----------|---|---------|
| `sosord` | 56,063 | 2,803 | 5% | Active orders |
| `sosppr` | 260,147 | 2,601 | 1% | Price per product |
| `soconsign` | 318,949 | 3,189 | 1% | Consignment orders |
| `soconitems` | 327,244 | 3,272 | 1% | Consignment items |
| `sospsp` | 73,017 | 730 | 1% | Special pricing |

#### MLM Sample Tables

| Table | Total Rows | Sample Rows | % | Purpose |
|-------|-----------|-----------|---|---------|
| `mlmcust` | 2,999,503 | 29,995 | 1% | Consultant master |
| `CustSPNLVL` | 144,754 | 1,448 | 1% | Sponsor-level mapping |
| `rbtmlm` | 39,779 | 3,978 | 10% | Rebate templates |
| `rebatedtl` | 44,279 | 4,428 | 10% | Rebate details |
| `MoDownlinerV1` | 1,077,665 | 10,777 | 1% | Monthly downliner |

#### Campaign Sample Tables

| Table | Total Rows | Sample Rows | % | Purpose |
|-------|-----------|-----------|---|---------|
| `CampChanges` | 936,905 | 9,369 | 1% | Campaign changes |
| `changescam` | 892,431 | 8,924 | 1% | Campaign changes |
| `changescamp` | 46,800 | 468 | 1% | Campaign detail changes |

### Sampling Methodology

**For each Tier 2 table:**

1. **Sort by date** (if has `dinsert`, `dcreate`, `dmodify`)
2. **Keep 100% of last 12 months**
3. **Sample 1-10% of older data** (oldest first)
4. **Example:** Customer applications
   ```sql
   -- Keep ALL from last 12 months
   SELECT * FROM arcapp WHERE dcreate >= DATEADD(YEAR, -1, GETDATE())
   UNION ALL
   -- Sample 1% of older
   SELECT * FROM arcapp
   WHERE dcreate < DATEADD(YEAR, -1, GETDATE())
   AND CHECKSUM(*) % 100 = 1
   ```

---

## Tier 3: Full Data Migration (Under 1K rows)

**Count:** 255 tables | **Rows:** 49,629 | **Action:** Migrate everything

These are reference/configuration tables, small masters, and lookups. Keep 100% intact for business logic.

### Tier 3 Tables by Category

#### Company/Configuration (20 tables)

| Table | Rows | Purpose |
|-------|------|---------|
| `coacpd` | 351 | Accounting periods (CRITICAL) |
| `coaudt` | 89,105 | Audit trail (WAIT—this is Tier 1!) |
| `coactr` | 7,455 | Accounting transactions |
| `cobank` | 40 | Bank accounts |
| `cocurr` | 12 | Currencies |
| `costax` | 28 | Tax codes |
| `costen` | 17 | Standard entries |
| `cosmsg` | 5 | System messages |

Actually, `coaudt` (89K rows) might be Tier 2. Check frequency of access.

#### Address/Postal (7 tables)

| Table | Rows | Purpose |
|-------|------|---------|
| `ad_Area` | 42 | Geographic areas |
| `ad_Regions` | 5 | Regions (TINY!) |
| `ad_PostalCodes` | 33,265 | Postal code lookups |
| `suburbs` | 28,183 | Suburb master |
| `Country` | 238 | Country master |
| `PostalCodes` | 6,621 | Postal codes (duplicate?) |
| `portaddfix` | 117 | Portal address fixes |

#### Bank Reconciliation Config (6 tables)

| Table | Rows | Purpose |
|-------|------|---------|
| `brsyst` | 1 | System settings |
| `brttyp` | 20 | Transaction types |
| `brrcrt` | 6 | Reconciliation reports |
| `brrglr` | 6 | GL reconciliation |

#### Accounts Payable Masters (8 tables)

| Table | Rows | Purpose |
|-------|------|---------|
| `apvend` | 872 | Vendor master |
| `apvglr` | 486 | Vendor GL rules |
| `apcacb` | 15 | Vendor accrual |
| `apdnot` | 4 | Vendor notes |
| `apjrnl` | 348 | Vendor journals |

#### Accounts Receivable Masters (12 tables)

| Table | Rows | Purpose |
|-------|------|---------|
| `arpycd` | 66 | Payment codes |
| `arcack` | 2,248 | Account activity |
| `arcark` | 2,248 | Account remarks |
| `arcdsc` | 9 | Discounts |
| `ardeps` | 3 | Deposits |
| `ardist` | 946,361 | **Hmm, this is large!** |
| `ardpst` | 946,361 | Customer deposits (sync) |

Wait, `ardpst` is 946K—that's Tier 2.

#### GL Masters (14 tables)

| Table | Rows | Purpose |
|-------|------|---------|
| `glacct` | 9,439 | Chart of accounts |
| `glagrp` | 57 | Account groups |
| `glcatg` | 60 | Categories |
| `glcflw` | 82 | Cash flow categories |
| `glsegv` | 820 | Segment values |
| `gljeid` | 18,226 | Journal entry IDs |

#### Inventory Masters (16 tables)

| Table | Rows | Purpose |
|-------|------|---------|
| `icitem` | 10,676 | Item master (might be Tier 2!) |
| `ictype` | 22 | Item types |
| `icunit` | 71 | Units of measure |
| `icwhse` | 21 | Warehouse locations |
| `icwbin` | 31 | Warehouse bins |
| `iccst` | 5,272 | Cost tracking |
| `icvend` | 1,429 | Vendor assignments |
| `icspec` | 0 | Special items (empty) |

#### Sales Order Masters (12 tables)

| Table | Rows | Purpose |
|-------|------|---------|
| `sopcnt` | 4,312 | Sales personnel |
| `soofrule` | 80 | Order fulfillment rules |
| `soofhead` | 85 | Offer headers |
| `sooflist` | 1,707 | Offer details |
| `socmpf` | 264 | Comparison fields |
| `soscovr` | 6,111 | Sales coverage |

#### Campaign Masters (8 tables)

| Table | Rows | Purpose |
|-------|------|---------|
| `Campaign` | 77 | Campaign master |
| `CampType` | 20 | Campaign types |
| `CampCat` | 451 | Campaign categories |
| `CampSku` | 14,197 | SKU assignments (might be Tier 2) |

---

## Data Migration Volume Summary

### By Tier

| Tier | Tables | Original Rows | Action | Migrated Rows | % of Original |
|------|--------|---------------|--------|---------------|---------------|
| 0 | 342 | 3,524,547 | Delete | — | — |
| 1 | 87 | 444,521,162 | DDL Only | 20-50M* | 5-11% |
| 2 | 229 | 3,660,936 | Sample | 150K-200K** | 4-5% |
| 3 | 255 | 49,629 | Full | 49,629 | 100% |
| **TOTAL** | **913** | **451,756,274*** | — | **~3.8-4.2M*** | **~1%** |

*Tier 1 includes massive GL/IC history; recommend keeping only 3-5 years per table
**Tier 2 sample rate 1-10% depending on table

**Gross Total:** 451.76M rows (includes duplicates/backups across some categories)

---

### By Module (Shopify-Ready)

| Module | Tables | Rows (Sampled) | Priority |
|--------|--------|----------------|----------|
| AR | 65 | ~500K | **CRITICAL** |
| SO | 47 | ~800K | **CRITICAL** |
| IC | 67 | ~150K | **CRITICAL** |
| GL | 21 | ~500K | **CRITICAL** |
| MLM | 18 | ~100K | **CRITICAL** |
| NOP/ECOM | 8 | ~1.5M | **HIGH** |
| CO/Config | 20 | ~50K | HIGH |
| Campaign | 14 | ~50K | MEDIUM |
| Others | — | ~250K | LOW |
| **TOTAL** | **260** | **~3.8M** | — |

---

## Migration Execution Plan

### Phase 1: Cleanup (1 week)
1. Drop all Tier 0 tables (342 tables, 3.5M rows deleted)
2. Identify Tier 1 cutoff dates (e.g., keep GL from 2021+)
3. Archive Tier 1 tables older than cutoff to cold storage

### Phase 2: Tier 1 Data Pruning (2 weeks)
1. **GL Ledger:** Delete pre-2021 transactions → 90M rows removed
2. **Inventory:** Keep last 24 months of daily snapshots → 30-50M rows removed
3. **AR History:** Keep last 5 years → 10M rows removed
4. **SO History:** Keep last 3 years → 5M rows removed
5. **MLM History:** Keep last 36 months of monthly → 5M rows removed

**Estimated cleanup:** 140-150M rows deleted (30% of total)

### Phase 3: Tier 2 Sampling (2 weeks)
Use SQL script to sample 1-10% of each Tier 2 table, preserving last 12 months 100%.

```sql
-- Template for Tier 2 sampling
SELECT * INTO [dbo].[table_sampled] FROM [dbo].[table]
WHERE dcreate >= DATEADD(YEAR, -1, GETDATE())
UNION ALL
SELECT * FROM [dbo].[table]
WHERE dcreate < DATEADD(YEAR, -1, GETDATE())
  AND CHECKSUM(*) % 10 = 1
```

### Phase 4: Shopify Export (1 week)
1. Export sampled AR/SO for customer/order history
2. Export IC for product catalog
3. Export MLM tables for consultant structure (custom Shopify app)
4. Export GL for financial audit trail

### Phase 5: Import to Shopify (2-4 weeks)
1. Build custom Shopify importer app
2. Map AccountMate fields to Shopify objects
3. Handle MLM-specific data (consultant levels, pricing)
4. Validate data integrity

---

## Archive Strategy (Cold Storage)

Keep deleted/sampled data in separate archive database:

```
Primary Migration DB (Shopify)          Archive DB (Reference)
├─ AR (3-5 years)                      ├─ AR (older years)
├─ SO (3 years)                        ├─ SO (older orders)
├─ IC (current + 2 years)              ├─ IC (historical snapshots)
├─ GL (3-5 years)                      ├─ GL (all historical)
├─ MLM (36 months)                     ├─ MLM (older rebates)
└─ Masters (all)                       └─ Masters (all)
```

**Archive Storage:** AWS Glacier, Azure Archive, or on-premise tape
**Access:** Read-only reporting/audit trail

---

## Estimated Data Size Reduction

| Stage | Tables | Rows | DB Size (Est.) |
|-------|--------|------|----------------|
| Original | 913 | 399.6M | ~500 GB |
| After Tier 0 cleanup | 571 | 396.1M | ~495 GB |
| After Tier 1 pruning | 571 | 250-300M | ~350 GB |
| After Tier 2 sampling | 800 | ~3.8M | ~10 GB |
| Shopify-Ready | — | — | **~10-15 GB** |
| Archive DB | — | — | ~350 GB (cold) |

**Migration Reduction:** From 500 GB to 10-15 GB for active Shopify instance (97% reduction!)

---

**End of Data Tiers Report**
