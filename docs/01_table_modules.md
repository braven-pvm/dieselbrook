# SQL Database Analysis: Table Module Classification
## Annique AccountMate ERP + NopCommerce (NopCommerce)

**Analysis Date:** 2026-03-01
**Total Tables:** 913
**Total Rows:** ~399.6 million
**Database System:** Microsoft SQL Server (AccountMate ERP)

---

## Executive Summary

This is a comprehensive cosmetics MLM (multi-level marketing) enterprise database combining:
- **AccountMate ERP** (core financial and operational system)
- **NopCommerce** (NopCommerce-based online store integration)
- **MLM/Rebate System** (consultant levels, downline tracking, commission calculations)

The database is heavily weighted toward transactional data in Sales Orders (SO), Accounts Receivable (AR), Inventory (IC), and General Ledger (GL) modules.

---

## Module Classification by Functional Area

### AR - Accounts Receivable/Customer
**65 tables | ~57.1 million rows**

Core customer invoice, payment, and receivables management.

**Major Tables:**
- `arinvc` (275,883) - Active invoices
- `arinvch` (5,830,255) - Invoice history (massive)
- `aritrsh` (21,607,444) - Item transaction history
- `arcapp` (201,173) - Customer applications/registrations
- `arcust` (119,616) - Customer master
- `arcash` (81,667) - Cash receipts
- `arstmt` (18,815) - Customer statements
- `arcadr` (109,594) - Customer addresses
- `arrfnd` (303,752) - Refunds
- `arcnot` (110,716) - Customer notes
- `arpycd` (66) - Payment codes
- `arcact` (2,248) - Account activity
- `arcapp$` (1,680) - Backup/staging
- `arcpos` (138,437) - Point of sale/POS integration
- `arikit` (4,851,844) - Item kits
- `aritsp` (157,402) - Item transaction specifics
- `aritrs` (769,633) - Item transaction results

**Flags:** Multiple backup tables detected:
- `arcapp_bu11Nov22` (17 rows) - November 2022 backup
- `arcapp_bu11Oct18` (2,629 rows) - October 2018 backup
- `arcappBU10Dec12` (35,413 rows) - December 2012 backup
- `arcappNov12` (1,062 rows) - November 2012 backup
- `arinvc_bu11Oct18` (2,629 rows) - Invoice backup

**Key Relationships:** Links to GL accounts, item master (IC), and MLM customer levels

---

### AP - Accounts Payable/Vendor
**22 tables | ~294.9 thousand rows**

Vendor management, purchase invoice tracking, and payments.

**Major Tables:**
- `apcapp` (46,398) - Vendor invoice applications
- `apinvc` (4,259) - Vendor invoices
- `apinvch` (45,083) - Invoice history
- `apdist` (16,804) - AP distribution
- `apdisth` (128,118) - Distribution history
- `apchck` (19,175) - Checks issued
- `apvend` (872) - Vendor master
- `apracq` (5,206) - Receivable inquiries
- `apdapp` (3,600) - Distribution applications

**Flags:** Some empty tables:
- `apcont` (0) - Vendor contacts
- `apamrt` (0) - Amortization
- `apfinc` (0) - Finance charges

---

### GL - General Ledger
**21 tables | ~125.6 million rows**

Core accounting ledger, journal entries, budgets.

**Major Tables:**
- `gltrsn` (118,274,883) - GL transactions (MASSIVE)
- `glbugtprep` (587,354) - Budget prep
- `glacct` (9,439) - Chart of accounts
- `gljeid` (18,226) - Journal entry IDs
- `gltfer` (3,262,263) - GL transfers
- `glbugt` (17,825) - Budget master
- `glunje` (113,796) - Unjournal entries
- `glabal` (1,025) - GL balances

**Flags:** Temporary/backup tables:
- `gltrsnbu18Dec23` (2,386,881 rows) - December 2023 backup (LARGE!)
- `gltfererror` (6) - Error tracking
- `gltfertemp` (3,262,263) - Transfer temp table

---

### IC - Inventory Control
**67 tables | ~174.1 million rows**

Item master, warehouse, stock movements, pricing.

**Major Tables:**
- `icimlp` (772) - Item location pricing
- `icimlpdat` (3,625) - Item pricing dates
- `icimlp_040717` (38,918) - Historical pricing (dated)
- `icitem` (10,676) - Item master
- `icibal` (325,206) - Item balances
- `iciwhs` (101,970) - Inventory by warehouse
- `iciwhs_daily` (129,980,593) - Daily inventory snapshots (MASSIVE!)
- `iciwhs_monthly` (15,686,562) - Monthly inventory (LARGE)
- `icitrsnh` (17,549,397) - Item transaction history
- `icktrs` (802,131) - Kit transactions
- `icuprc` (19,171) - Unit pricing
- `iciimg` (3,372) - Item images
- `iciimgUpdateNOP` (7,744) - Images updated for NOP

**Flags:** Backups and test tables:
- `icitem_bak_1111` (6,853)
- `icitem_bak1009` (6,316)
- `icitembu26Oct11` (5,332)
- `icitembak04022014` (6,513)
- `iciwhsbackupfeb` (75,394)
- `icitem_test1` (6,765)
- `icuprc_bak1411` (28,874)
- `icimlp_100818` (0) - Empty
- `icuprc_newP` (0) - Empty

---

### SO - Sales Order
**47 tables | ~48.0 million rows**

Sales order processing, shipments, fulfillment tracking.

**Major Tables:**
- `sosord` (56,063) - Sales orders
- `sosordh` (1,624,995) - Order history
- `sosptr` (292,107) - Special pricing
- `sosptrh` (11,885,664) - Special pricing history
- `sospsph` (4,536,160) - Special pricing SPA history
- `sosktr` (104,613) - Kits tracked
- `sosktrh` (7,546,225) - Kit history
- `soship` (55,516) - Shipments
- `soshiph` (1,299,228) - Shipment history
- `soskit` (5,903,804) - Kit details (MASSIVE)
- `sosppr` (260,147) - Special pricing per product
- `soconsign` (318,949) - Consignment orders
- `soconitems` (327,244) - Consignment items
- `sospsp` (73,017) - Price/ship details
- `sostsph` (11,885,664) - Price history

**Flags:** Portal integration and tracking:
- `SOPortal` (646,237) - Sales portal interface
- `WebOrderTrace` (620,464) - Web order tracking

---

### BR - Bank Reconciliation
**11 tables | ~1.1 million rows**

Bank statement processing and reconciliation.

**Major Tables:**
- `brctrl` (25,840) - Bank control
- `brctrlh` (979,275) - Control history
- `brtrsn` (31,715) - Transaction numbers
- `brdist` (35,373) - Distribution
- `brtran` (4,548) - Transactions
- `brsyst` (1) - System settings
- `brstmt` (1,301) - Statements

---

### MLM - MLM/Rebate/Consultant System
**18 tables | ~30.9 million rows**

Consultant management, rebate calculations, downline tracking, commission processing.

**Major Tables:**
- `genreb` (19,855,973) - General rebate (MASSIVE!)
- `genrebcomp` (138,911) - Rebate computations
- `rbtmlm` (39,779) - Template MLM
- `rbtmlmMONTHLY` (2,844,015) - Monthly MLM rebates (LARGE)
- `rebatedtl` (44,279) - Rebate details
- `rebatedtlMONTHLY` (3,324,956) - Monthly details (LARGE)
- `mlmcust` (2,999,503) - MLM customers (LARGE)
- `CustSPNLVL` (144,754) - Sponsor/customer level mapping
- `Level1` (4,072) - Consultant level 1
- `Level2` (31,081) - Consultant level 2
- `MLMHist` (11,466) - MLM history
- `MoDownlinerV1` (1,077,665) - Monthly downliner tracking

**Flags:** Test and backup tables:
- `genrebcompTEST` (64,860) - Test computations
- `genrebcompFEB` (9,757) - February backup
- `custspnlvl_bu02222016` (135,051) - February 2016 backup
- `rbtmlmbu21Feb14` (36,355) - February 2014 backup
- `rebatedtlbu25Aug14` (45,963) - August 2014 backup
- `AG_GENREBCOMP_DATE` (0) - Empty

**Critical Note:** `genreb` table is 19.8M rows — likely needs sampling for migration.

---

### NOP/ECOM - NopCommerce/Web Integration
**8 tables | ~1.4 million rows**

Online store integration with NopCommerce (NopCommerce-based).

**Major Tables:**
- `fw_consignment` (674,738) - Fulfillment warehouse consignments (LARGE)
- `fw_items` (681,759) - FW item tracking (LARGE)
- `fw_Hubs` (23,629) - Fulfillment hubs
- `fw_labels` (26,241) - Shipment labels
- `fw_manifest` (6,861) - Shipment manifests
- `NOP_Discount` (2) - NOP discount rules
- `soxitems` (19,574) - Special order items (Sox)
- `soxitemsH` (43,266) - Sox history

**Key Integration:** Fulfillment warehouse (fw_*) appears to be third-party logistics provider integration.

---

### CAMP - Campaign Management
**14 tables | ~1.0 million rows**

Marketing campaigns, promotions, discounts.

**Major Tables:**
- `Campaign` (77) - Campaign master
- `CampCat` (451) - Campaign categories
- `CampDetail` (18,371) - Campaign details
- `CampSponSum` (308) - Sponsor summaries
- `CampSku` (14,197) - SKU assignments
- `CampComp` (4,613) - Competitor tracking
- `CampKit` (2,094) - Kit campaigns
- `CampChanges` (936,905) - Change tracking (LARGE)
- `CampSpon` (3,122) - Sponsor campaigns
- `CampType` (20) - Campaign types

---

### CO - Company/Configuration
**20 tables | ~239.6 thousand rows**

System configuration, accounting periods, currencies.

**Major Tables:**
- `coacpd` (351) - Accounting periods
- `coaudt` (89,105) - Audit trail
- `coactr` (7,455) - Accounting transactions
- `coschd` (1) - Schedule master
- `cocurr` (12) - Currencies
- `cobank` (40) - Bank accounts
- `costax` (28) - Tax codes
- `costen` (17) - Standard entries
- `cosmsg` (5) - System messages
- `comisc` (4,037) - Miscellaneous

---

### PO - Purchase Order
**27 tables | ~166.8 thousand rows**

Purchase order processing and receiving.

**Major Tables:**
- `popord` (3,340) - PO master
- `popordh` (6,999) - PO history
- `podist` (11,250) - PO distribution
- `porecg` (4,110) - Receiving
- `pocnlg` (979) - Cancellations
- `poachg` (52) - Account charges
- `poctrs` (1,283) - Transactions

---

### RA - Returns/RMA
**23 tables | ~26.9 thousand rows**

Returns management, RMA processing, credit memos.

**Major Tables:**
- `radist` (6,910) - Return distributions
- `racord` (1,152) - Return orders
- `raiwrn` (4,625) - Item warnings
- `rarecg` (1,098) - Receiving
- `rashpg` (1,140) - Return shipping
- `rastrs` (2,629) - Transactions

---

### MI - Manufacturing/WIP
**13 tables | ~38.3 thousand rows**

Work-in-process, bill of materials, production.

**Major Tables:**
- `miwips` (778) - WIP status
- `miwipsh` (15,188) - WIP history
- `miboml` (581) - BOM links
- `miword` (72) - Work orders
- `miwtrk` (44) - Work tracking
- `miwtrs` (397) - Work transactions
- `miwtrsh` (7,848) - Transaction history

---

### AD - Address/Postal
**7 tables | ~368.9 thousand rows**

Address validation, postal codes, geographic data.

**Major Tables:**
- `ad_AddressIssues` (312,069) - Address problems flagged
- `ad_PostalCodes` (33,265) - Postal code master
- `ad_Area` (42) - Geographic areas
- `ad_Regions` (5) - Regions
- `PostalCodes` (6,621) - Postal codes (duplicate?)
- `suburbs` (28,183) - Suburb master
- `portaddfix` (117) - Portal address fixes

---

### BE - Business Events/Logistics
**4 tables | ~338.2 thousand rows**

Business event tracking, logistics providers (SkyTrack, Waybill).

**Major Tables:**
- `be_waybill` (334,253) - Shipment waybills
- `be_invoice` (3,334) - Invoice tracking
- `be_recon` (448) - Reconciliation
- `bercoevent` (162) - Event log

---

### PR - Payroll
**5 tables | ~3.2 thousand rows**

Employee payroll (mostly empty in this schema).

**Major Tables:**
- `prhire` (0)
- `prempd` (0)
- `prstat` (0)

**Note:** Payroll appears to be integrated with external system or handled separately.

---

### JR - Journal/Reporting
**2 tables | 167 rows**

Journal and reporting configuration.

---

### OTHER - Miscellaneous
**188 tables | ~7.5 million rows**

Test tables, ad-hoc queries, specialized features, and unclassified tables.

**Notable Subsets:**
- **Forecast/Planning:** `FJan11`, `FMar11`, `FCastOct11`, etc. (monthly forecasts)
- **Reports:** `TREPORT` (8,369), `Aging` (39,083), `Reg` (11,081), `RevCode` (71,094)
- **Dropship:** `changedrp` (60,960), `drpval` (5,257), `drpvalues` (70)
- **Testing/Staging:** `new` (140), `dynamic` (525), `test111` (73)
- **Single-letter tables (DEBUG):** `a`, `i`, `g1`, `t2`, `t3`, `for`, `cnt`, `age`, `Aug`, `Dec`, `Jan`, etc.

---

## Data Quality Flags

### Backup/Temporary Tables (49 tables)
These should be skipped in migration:
- `arcapp_bu11Nov22`, `arcapp_bu11Oct18`, `arcappBU10Dec12`, `arcappNov12`
- `acinvc_bu11Oct18`, `apinvcbu7Feb13`, `apcappbu7Feb13`
- `dpibdgbu29Oct10`, `dpibdgbu2Dec10`, `dpibdgbu5Sep11`, `dpibdgbu6July2013`, `dpibdgbu9July13`
- `custspnlvl_bu02222016`, `rbtmlmbu21Feb14`, `rbtmlmbu25Aug14`, `rebatedtlbu25Aug14`
- `gltrsnbu18Dec23`, `icitembu6Jun14`
- `soshiph_bu`, `podisth_bu`
- And 30+ others with date patterns

### Test/Debug Tables (9 tables)
Single-letter and obvious test names:
- `a` (17,345 rows) - DEBUG
- `i` (17,345 rows) - DEBUG
- `g1` (29 rows)
- `test111` (73)
- `test2` (6)
- `TEST30` (196), `TEST31` (195), `TEST32` (35), `TEST33` (66), `TEST34` (1)
- `fintest` (57)
- `testss` (10)

### Empty Tables (293 tables)
Zero row count — structure only, can be DDL-only in migration.

**Examples:**
- `apamrt`, `apcont`, `apfinc`, `apfinch`, `apdsta`, `apraca`, `aprchg`, `aprcri`, `apvacb`, `apvact`
- `aragng`, `aramrth`, `arccnt`, `aretrs`, `arfchg`, `aritci`, `aritrs_T` (actually has rows)
- `AG_GENREBCOMP_DATE`, `DashboardInfo`, `DateTbl`, `log_events`, `sysdiagrams`, `usrtbl`, `ws_Setting`
- All `pr*` (payroll) tables
- Many `pr*` (payroll-related) tables

---

## Summary Statistics

| Module | Tables | Rows | Avg Rows/Table |
|--------|--------|------|----------------|
| GL (General Ledger) | 21 | 125.6M | 5.98M |
| IC (Inventory) | 67 | 174.1M | 2.60M |
| AR (A/R) | 65 | 57.1M | 878K |
| SO (Sales Order) | 47 | 48.0M | 1.02M |
| MLM/Rebate | 18 | 30.9M | 1.72M |
| **TOTAL (Core)** | **220** | **435.7M** | **1.98M** |
| Backup/Temp Tables | 49 | 3.5M | (skip) |
| Empty Tables | 293 | 0 | (structure only) |
| Other/Test | 351 | 3.7M | (varies) |
| **GRAND TOTAL** | **913** | **~399.6M** | — |

---

## Migration Impact Assessment

### High Priority (Transactional Core)
1. **AR module** (57.1M rows) — customer invoices, payments, history
2. **SO module** (48.0M rows) — orders, shipments, fulfillment
3. **IC module** (174.1M rows) — inventory balances (especially `iciwhs_daily`)
4. **GL module** (125.6M rows) — accounting ledger
5. **MLM module** (30.9M rows) — consultant rebates and commissions

### Medium Priority (Supporting)
6. **AP module** (295K rows) — vendor management
7. **PO module** (167K rows) — purchase orders
8. **NOP/ECOM** (1.4M rows) — web store integration
9. **Campaign** (1.0M rows) — marketing campaigns

### Low Priority (Reference/Archive)
10. **CO module** (240K rows) — configuration
11. **AD module** (369K rows) — address data
12. **BE module** (338K rows) — logistics events
13. **RA module** (27K rows) — returns
14. **MI module** (38K rows) — manufacturing

### Skip (Cleanup Required)
- All backup tables (49 tables, 3.5M rows)
- All empty tables (293 tables, 0 rows)
- All test/debug tables (9 tables)

---

## Key Observations for Shopify Migration

1. **Massive Inventory Tracking:** The `iciwhs_daily` (130M rows) and `iciwhs_monthly` (15.7M rows) tables suggest daily/monthly snapshots of inventory. This may not be necessary in Shopify — sampling recommended.

2. **Multi-Warehouse Setup:** Multiple IC tables with warehouse-specific data indicate a complex fulfillment model (possibly using external warehouses like fw_* tables).

3. **Extensive History:** AR and SO modules have 10-100x more history rows than active records. Archive older history for reference-only purposes.

4. **MLM Complexity:** The `genreb` table (19.8M rows) represents the core rebate calculation engine. This is business-critical and may require custom Shopify app development.

5. **Existing NOP Integration:** NopCommerce is already integrated — understand current sync points before replacing with Shopify.

6. **Data Redundancy:** Multiple copies of tables with date suffixes (e.g., `_bu7Feb13`, `_020717`) suggest ad-hoc backups and testing. Clean these up.

---

**End of Report**
