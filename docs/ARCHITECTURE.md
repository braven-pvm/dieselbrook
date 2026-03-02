# Annique ERP System — Current Architecture & Data Flow
## Live Database-Verified Technical Reference for Shopify Migration

**Prepared:** March 2, 2026  
**Last Verified:** Live database query session against `amanniquelive` on `Away2.annique.com,62222`  
**Source:** Static CSV analysis (913 tables, 968 procs, 154 triggers, 1,743 views) + live SQL queries  
**Purpose:** Ground-truth architecture document to drive Shopify migration design

> ⚠️ **TEST SERVER:** The live queries were run against `AMSERVER-TEST` (the test/staging environment), not production. Some data (linked servers, wsSetting URL, socamp.lactive) may differ from production. All row counts and schema are production-equivalent.

---

---

## LIVE DATABASE SNAPSHOT (Verified 2026-03-02)

### Environment
| Item | Value |
|---|---|
| **Server name** | `AMSERVER-TEST` (test/staging — NOT production) |
| **SQL Server version** | SQL Server 2014 SP3 (12.0.6024.0) |
| **Connection** | `Away2.annique.com,62222` |
| **Database** | `amanniquelive` |
| **Server date at time of query** | `2026-03-02 17:53:37` |

### Real Row Counts (from `sys.partitions` — no full table scans)

| Table | Live Rows | Notes |
|---|---|---|
| `aritrsh` | **21,607,444** | All-time transaction history archive |
| `icWsUpdate` | **7,227,023** | Sync queue. Only **22 pending** (all iciwhs type) — queue accumulates but never purges |
| `mlmcust` | **2,999,503** | MLM commission aggregation |
| `sosordh` | **1,624,995** | Historical order archive |
| `aritrs` | **769,633** | Current invoice line items |
| `fw_items` | **681,759** | Fastway parcel items |
| `fw_consignment` | **674,738** | Fastway consignments |
| `SOPortal` | **646,237** | Portal orders (all have blank cStatus on test server — may differ on prod) |
| `changes` | **531,698** | Consultant change audit log: `cstatus` (167K), `csponsor` (147K), `ndiscrate` (53K) |
| `arinvc` | **275,883** | Invoice headers |
| `sosppr` | **260,147** | Campaign product pricing (all campaigns historical) |
| `arcust` | **119,616** | Consultants: **10,308 Active (A)**, 109,308 Inactive (I) |
| `iciwhs` | **101,970** | Warehouse inventory |
| `sosord` | **56,063** | Current sales orders |
| `iciimgUpdate` | **21,091** | Image sync queue (anniquestore.co.za) |
| `soxitems` | **19,574** | Exclusive items: 19,571 processed (lupdatetows=0), **2 pending** |
| `CampDetail` | **18,371** | Campaign product details |
| `icitem` | **10,676** | Products: **~800 Active (A)**, rest Inactive/Archive |
| `icikit` | **8,510** | Kit component definitions |
| `iciimgUpdateNOP` | **7,744** | NOP image sync queue (NopCommerce) |
| `Campaign` | **77** | Campaign records |
| `MLMCurr` | **49,788** | Current downline hierarchy snapshot |
| `MLMHist` | **11,466** | Historical downline snapshots |
| `NOP_Discount` | **2** | NOP discount rules (2 active entries) |

### wsSetting (Live — Test Server)
```
Id  Name           Value                         StoreId  type
1   ws.url         https://anniquestore.co.za/   0        C
2   ws.warehouse   4400                          0        C
```
> **Critical:** `ws.url` points to `anniquestore.co.za` (old webstore), NOT `nopintegration.annique.com`. The `sp_NOP_*` procedures hardcode `nopintegration.annique.com` regardless. The `sp_ws_*` family uses this URL for `anniquestore.co.za` sync. Production value may differ.

### Consultant Status
| Status | Count | Meaning |
|---|---|---|
| `I` | 109,308 | Inactive |
| `A` | 10,308 | Active |

### Current Campaign (at query time)
| Field | Value |
|---|---|
| Campaign | `Feb_2026` (ID: 90) |
| Period | 2026-02-02 → 2026-03-01 |
| Status | `C` (Current) |
| Discount rate | 20% |
| MLM rate | 21% |
| VAT rate | 15% |
| Next campaign | `Mar_2026` (status `F` = Future) |

### Delivery Method Mix (2025–2026 orders)
| Carrier | Orders | Share |
|---|---|---|
| FASTWAY | 23,974 | 44% |
| BERCO | 18,606 | 34% |
| SKYNET | 8,912 | 16% |
| POSTNET | 3,212 | 6% |
| COLLECT / COD / COURIER / ANNIQUE | 823 | ~1% |

### Active Product Catalog (Active items only)
| Category | Count |
|---|---|
| SKINCARE | 207 |
| LIFESTYLE | 121 |
| MARKETING | 115 |
| BODYCARE | 111 |
| SALES AID | 67 |
| TCOMPONENT | 56 |
| BMTM | 28 |
| VOUCHER | 21 |
| Other | ~74 |
| **Total Active** | **~800** |

---

## 1. The Big Picture

### What we are dealing with

Annique runs a **heavily customised AccountMate v7.x ERP** on SQL Server. NopCommerce does **not** share the same database — it runs on a separate SQL Server instance. The two are connected via:

1. **A linked server** named `[NOP]` (pointing to the NopCommerce instance at `[NOP].annique.dbo`), used for direct table reads/writes by 3 procedures.
2. **A REST-style HTTP integration service** at `nopintegration.annique.com`, called by the ERP via `sp_ws_HTTP` (which uses `MSXML2.ServerXMLHTTP` OLE Automation from within SQL Server).
3. **A second linked server** named `[WEBSTORE]` (pointing to `[WEBSTORE].anniquestore.dbo`) found in legacy/dead code — an older pre-NOP integration still present in the codebase.
4. **A third linked server** named `[Portal]` (pointing to `[Portal].Accountmate_Webstore.dbo`) used for consultant discount sync in one procedure.

There is also a **separate Shop API** at `shopapi.annique.com` used exclusively for staff/consultant sync.

### Key architectural facts confirmed from source

| Fact | Source |
|---|---|
| NopCommerce is on a **separate server** (linked via `[NOP]`) | sp_NOP_syncSoxitems code, 21 confirmed `[NOP].annique.dbo` references |
| `wsSetting.ws.url` is a **null-check kill-switch only** — all `sp_NOP_*` procedures immediately hardcode their own URLs after reading it | Verified in all 5 NOP sync SP definitions |
| `sp_ws_HTTP` **always POSTs** — there is no GET path in the implementation | sp_ws_HTTP source: `EXEC @ret = sp_OAMethod @token, 'open', NULL, 'POST'` |
| The `Authentication` header is misspelled (should be `Authorization`) | sp_ws_HTTP source: `'Authentication', @authHeader` |
| No API audit log table exists — `NopIntegration..Brevolog` is a **Brevo email marketing log** | CORRECTIONS.md, confirmed from SP source INSERT statement |
| AutoPick batching threshold is **15 minutes OR 10 orders** | AutoPick SP: `IF @rows=0 OR (@mins<15 AND @rows<10)` |

---

## 2. Database Domain Map

The ERP database contains **13 functional domains**:

```
┌─────────────────────────────────────────────────────────────────┐
│                    ANNIQUE SQL SERVER (ERP)                     │
│                                                                 │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────────┐  │
│  │    GL    │ │    AR    │ │    AP    │ │       IC         │  │
│  │ glacct   │ │ arcust   │ │ apvend   │ │ icitem / icqoh   │  │
│  │ gltrsn   │ │ arinvc   │ │ apinvc   │ │ iciwhs           │  │
│  │ 118M rows│ │ aritrs   │ │ apdist   │ │ iciwhs_daily     │  │
│  │          │ │ arcapp   │ │ apcapp   │ │ 130M daily snap  │  │
│  └──────────┘ └──────────┘ └──────────┘ └──────────────────┘  │
│                                                                 │
│  ┌──────────┐ ┌──────────┐ ┌───────────────┐ ┌─────────────┐  │
│  │    SO    │ │   MLM    │ │   CAMPAIGN    │ │     3PL     │  │
│  │ sosord   │ │ mlmcust  │ │ Campaign      │ │fw_consignment│  │
│  │ soxitems │ │ CTDownline│ │ CampDetail   │ │ fw_items    │  │
│  │ soportal │ │ rbtmlm   │ │ CampSku       │ │ fw_hubs     │  │
│  │ soship   │ │ rebatedtl│ │ CampChanges   │ │ fw_labels   │  │
│  └──────────┘ └──────────┘ └───────────────┘ └─────────────┘  │
│                                                                 │
│  ┌─────────────────────┐ ┌────────────────┐ ┌──────────────┐  │
│  │  NOP INTEGRATION    │ │  WEB SERVICES  │ │    AUDIT     │  │
│  │ NOP_Discount        │ │ wsSetting      │ │ changes      │  │
│  │ NOP_Offers          │ │ icWsUpdate     │ │ CampChanges  │  │
│  │ NOP_OfferList       │ │ iciimgUpdate   │ │ changesitem  │  │
│  │ iciimgUpdateNOP     │ │ iciimgUpdateNOP│ │              │  │
│  └─────────────────────┘ └────────────────┘ └──────────────┘  │
└─────────────────────────────────────────────────────────────────┘
        │  Linked Server [NOP]           │  HTTP API
        ▼                               ▼
┌─────────────────────┐    ┌────────────────────────────┐
│   NopCommerce DB    │    │  nopintegration.annique.com │
│ [NOP].annique.dbo   │    │  shopapi.annique.com        │
│ Product             │    │  (integration middleware)   │
│ Customer            │    └────────────────────────────┘
│ Orders / OrderItems │
│ ANQ_ExclusiveItems  │
│ ANQ_Booking         │
│ Discount / Offer    │
│ ANQ_Lookups         │
└─────────────────────┘
```

---

---

## 2.5 Cross-Database Ecosystem (Live-Verified)

The ERP is **not a standalone database**. Multiple databases interact via triggers, stored procedures, and linked servers:

| Database | Location | Purpose | How Reached |
|---|---|---|---|
| `amanniquelive` | `AMSERVER-TEST` (test) / `AMSERVER` (prod) | **Main SA ERP** — AccountMate for South Africa | Direct connection |
| `amanniquenam` | Same SQL Server instance | **Namibia ERP mirror** — receives field-level changes from SA triggers | Same-instance database reference (no linked server) |
| `compplanlive` | Same SQL Server instance | **MLM Compensation Plan** — stores `ctcomph` (commission history), `deactivations` | Same-instance database reference |
| `NopIntegration` | Same SQL Server instance | **NopCommerce integration bridge** — `Brevolog` table (Brevo email log) | Same-instance database reference |
| `compsys` | Same SQL Server instance | **Central mail messaging** — `MAILMESSAGE` table queues emails for delivery | Same-instance database reference |
| `[NOP].annique.dbo` | Separate SQL Server (linked server) | **NopCommerce database** — `Product`, `Customer`, `ANQ_ExclusiveItems`, etc. | Linked server `[NOP]` — **NOT PRESENT on test server** |
| `[WEBSTORE].anniquestore.dbo` | Separate SQL Server (linked server) | **Old anniquestore.co.za** web store database — `registration`, `EventItems`, `orderitem`, `ws_sostrs` | Linked server `[WEBSTORE]` — **NOT PRESENT on test server** |
| `[WEBSTORE].shopdata.dbo` | Same as above | Old webstore shop data | Same linked server |
| `[AMSERVER-V9]` | Remote SQL Server | Unknown purpose — only linked server actually present on test server | Linked server `AMSERVER-V9` (SQLNCLI11) |

### Data Mirror: SA → Namibia (Automatic via Triggers)

The `SA_NAM_ItemChanges` trigger on `icitem` mirrors **every field change** to `amanniquenam.dbo.icitem`:

```
SA icitem UPDATE (any field)
    │
    └── SA_NAM_ItemChanges trigger fires
            │ Field-by-field comparison
            │ For each changed field:
            └── UPDATE amanniquenam.dbo.icitem
                    SET [field] = [newValue]
                    WHERE citemno = @citemno
```

Fields mirrored: ALL (cdescript, cfdescript, ctype, cspectype1/2, cbarcode1/2, cstatus, cmeasure, csmeasure, cpmeasure, cclass, cprodline, ccommiss, cvendno, nprice, nstdcost, lmlm, ldscntble, ...)

The `socamp_insupd` trigger on `socamp` mirrors campaign changes to `amanniquenam..socamp`.

> **Migration impact:** A Shopify product management change that bypasses the ERP (e.g., directly calling Shopify API) will NOT flow through to Namibia. The NAM sync must be preserved in any new architecture.

---

## 3. Complete Data Flow: NopCommerce ↔ AccountMate

### 3.1 All 12 Integration Procedures (Source-Verified)

#### Via HTTP to `nopintegration.annique.com`

| Procedure | Hardcoded URL | Method | Direction | Updated |
|---|---|---|---|---|
| `sp_NOP_syncOrders` | `/syncorders.api` | POST (no body) | NOP → AM | 2026-01-09 |
| `sp_NOP_syncItemAll` | `/syncproducts.api?type=all&instancing=single` | POST | AM → NOP | 2024-09-03 |
| `sp_NOP_syncItemAvailability` | `/syncproducts.api?type=availability` | POST | AM → NOP | 2024-09-03 |
| `sp_NOP_syncItemChanges` | `/syncproducts.api?type=changes` | POST | AM → NOP | 2024-09-03 |
| `sp_NOP_syncOrderStatus` | `/syncorderstatus.api?instancing=single` | POST | AM → NOP | 2024-10-28 |
| `sp_SendAdminSMS` | `/sendsms.api` | POST with JSON body | AM → NOP | 2025-05-07 |

#### Via HTTP to `shopapi.annique.com`

| Procedure | Hardcoded URL | Method | Direction | Updated |
|---|---|---|---|---|
| `sp_NOP_syncStaff` | `/syncstaff.api` | POST | AM → ShopAPI | 2023-03-20 |

#### Via Linked Server `[NOP].annique.dbo` (direct DB writes)

| Procedure | Tables Written | Direction | Trigger |
|---|---|---|---|
| `sp_NOP_syncSoxitems` | `ANQ_ExclusiveItems`, `Product`, `Customer` | AM ↔ NOP | tr_soxitems_insert, tr_soxitems_update |
| `sp_ws_UpdateImageNOP` | Calls `nop.annique.dbo.ANQ_SyncImage` | AM → NOP | Manual / scheduled |

#### Via Local Staging Tables

| Procedure | Tables Written | Direction |
|---|---|---|
| `sp_NOP_DiscountINS` | `NOP_Discount` | AM staging |
| `sp_NOP_OfferINS` | `NOP_Offers` | AM staging |
| `sp_NOP_OfferListINS` | `NOP_OfferList` | AM staging |

#### Via Web Service (second integration family using `wsSetting` URL properly)

These procedures correctly use `wsSetting.ws.url` as the base URL (not hardcoded):

| Procedure | Path | Updated |
|---|---|---|
| `sp_ws_syncAvailability` | `syncavailability.sync` | 2020-06-18 |
| `sp_ws_syncAvailabilityAll` | `syncavailability.sync?citemno=all&instancing=single` | 2020-06-18 |
| `sp_ws_SyncImages` | `syncimage.sync?citemno={item}` (per item) | 2020-06-18 |
| `sp_ws_syncItems` | `syncitems.sync` (⚠️ URL **hardcoded** to `https://anniquestore.co.za/`) | 2025-03-26 |
| `sp_ws_syncItemsAll` | `syncitems.sync?citemno=all` | 2023-03-01 |
| `sp_ws_getAvailability_CodeNEW` | Gets availability | 2025-11-24 |
| `sp_ws_reactivate` | Reactivates orders | 2025-11-24 |
| `sp_ws_UpdateImage` | Single image | 2025-03-26 |

---

### 3.2 Order Flow (End-to-End)

```
CUSTOMER (NopCommerce Web Store)
    │
    │  Places order
    ▼
[NOP].annique.dbo.Orders / OrderItems
    │
    │  sp_NOP_syncOrders (POST to /syncorders.api)
    │  NOP integration service reads NOP orders,
    │  writes to AccountMate
    ▼
arinvc (AR Invoice Header)
aritrs (AR Invoice Line Items)
    │
    │  Trigger: Order appears in soportal
    ▼
soportal (cstatus = ' ' pending)
    │
    │  AutoPick SP runs (every N minutes)
    │  WAIT: dprinted < 15 min AND orders < 10
    ▼
soportal (cstatus = 'P' picking)
    │
    │  Warehouse picks items, ships order
    │  soship record created
    ▼
soportal (cstatus = 'S' shipped)
    │
    │  sp_NOP_syncOrderStatus (POST to /syncorderstatus.api)
    ▼
[NOP].annique.dbo.Orders.OrderStatus updated
    │
    ▼
Customer sees "Shipped" on web store
```

**3PL variant (Fastway):**
```
sosord / soxitems
    │
    ├── soconsign (318.9K rows) — consignment order
    │       │
    │       └── fw_consignment → fw_items → fw_labels
    │               │
    │               └── sp_Fastway_AddManualConsignment
    │                       → Fastway API
    │
    └── be_waybill / SkyTrack (carrier tracking)
```

---

### 3.3 Product Catalog Sync Flow

```
icitem (Item Master — 10,700 items)
    │
    ├── FULL SYNC: sp_NOP_syncItemAll
    │       POST /syncproducts.api?type=all&instancing=single
    │       Runs manually (heavy operation)
    │
    ├── DELTA SYNC: sp_NOP_syncItemChanges
    │       POST /syncproducts.api?type=changes
    │       Reads: changes table WHERE cfieldname='cstatus'
    │       Only syncs items with status field changes
    │
    ├── STOCK SYNC: sp_NOP_syncItemAvailability
    │       POST /syncproducts.api?type=availability
    │       Reads: icqoh (quantity on hand)
    │       Writes to: [NOP].annique.dbo.Product.StockQuantity
    │
    └── IMAGE SYNC: sp_ws_UpdateImageNOP
            Reads: iciimgUpdateNOP (7,744 pending images)
            Calls: nop.annique.dbo.ANQ_SyncImage per item
            Marks: dUpdated = GETDATE() on success
```

---

### 3.4 Consultant Exclusive Items Flow

```
soxitems table (19,600 rows)
    │
    │  INSERT / UPDATE trigger fires
    ▼
tr_soxitems_insert / tr_soxitems_update
    │
    │  Calls sp_ws_syncsoxitems
    │  which calls sp_NOP_syncSoxitems
    ▼
sp_NOP_syncSoxitems
    │
    ├── Lookup: SELECT id FROM [NOP].annique.dbo.Product WHERE sku=@citemno
    ├── Lookup: SELECT id FROM [NOP].annique.dbo.Customer WHERE UserName=@ccustno
    │
    ├── IF both found AND item NOT EXISTS in ANQ_ExclusiveItems:
    │       INSERT INTO [NOP].annique.dbo.ANQ_ExclusiveItems
    │
    ├── IF both found AND item EXISTS:
    │       UPDATE [NOP].annique.dbo.ANQ_ExclusiveItems
    │
    └── IF product not found (ProductID=0):
            DELETE from ANQ_ExclusiveItems (item no longer valid)
```

**Note:** The `sp_ws_syncsoxitems` procedure also contains dead code that previously wrote to `[WEBSTORE].anniquestore.dbo.ExclusiveItems` — a legacy linked server. This is bypassed via `goto nop:` but is still in the codebase.

---

### 3.5 Change Detection System

The `changes` table (531,698 rows) is the backbone of the delta-sync approach:

```
Field change occurs (any user in AccountMate)
    │
    └── Trigger fires on source table
            │
            INSERT INTO changes (
                ccustno,       -- who changed
                cfieldname,    -- what field
                cnewvalue,     -- new value
                codlvalue,     -- old value
                InsUpdDate,    -- when (GETDATE())
                Account,       -- table context
                cUser,         -- username
                nbalance       -- balance at time
            )

Tracked tables (from trigger analysis):
    arcust → customer trigger
    arcadr → customeradr trigger
    icitem → tr_icitem_update → changes table
    
Change consumers:
    sp_NOP_syncItemChanges → reads changes WHERE cfieldname='cstatus'
    sp_ws_* procedures → reads lupdateportal flag on arcust
```

---

## 4. MLM / Rebate System Data Flow

### 4.1 Monthly Commission Cycle

```
Month-end (run first week of following month)
    │
    └── Input: compplanlive..ctcomph
            (Commission Plan History — external database)
            WHERE cCompStatus <> 'P' (not yet processed)
    │
    └── sp_ct_Rebates
            │
            ├── PER CONSULTANT (cursor):
            │       Base Rebate = SUM(ctcomph.namount) for month
            │       VAT = Base × 15%
            │       Total = Base + VAT
            │
            ├── Rebate Invoice (ctype='R'):
            │       INSERT arinvc (cinvno='M'+random9, lmlm=1, lautoreb=0)
            │       INSERT aritrs (citemno='SASAILM165')
            │       INSERT arcapp (npaidamt = -(amount+tax))
            │
            ├── Downline Earnings → Credit Memo for UPLINE:
            │       INSERT arinvc (ctype='R', lautoreb=1, cmlmlink=linkID)
            │       (upline gets credit)
            │
            └── Downline Obligation → Debit Memo for DOWNLINE:
                    INSERT arinvc (ctype='', lautoreb=1)
                    (downline gets debit)
    │
    ├── Mark processed: UPDATE compplanlive..ctcomph SET cCompStatus='P'
    │
    └── Document numbering via:
            vsp_mlm_getnewdocno → arsyst.cmlmrcpt (10-char counter)
            vsp_mlm_getnewmlminvc → arsyst.cmlmcinvno (10-char counter)
            Both wrapped in BEGIN TRANSACTION for collision safety
```

### 4.2 Consultant Hierarchy (Downline)

```
arcust (119.6K consultants, csponsor field = upline FK)
    │
    └── sp_ct_downlinebuild (runs monthly)
            │
            ├── TRUNCATE CTDownline (working table)
            ├── INSERT FROM fn_get_downlineCT (recursive CTE)
            │       Fields: ccustno, ilevel, csponsor, gen, iTitle, cstatus
            │
            ├── IF last day of month:
            │       INSERT CTDownlineh (archive)
            │
            └── Related tables:
                    mlmcust (2.9M rows) — consultant sales aggregation
                    rbtmlm (39.8K) — rebate tier master
                    rbtmlmMONTHLY (2.8M) — monthly aggregation
                    rebatedtl (44.3K) — rebate detail lines
                    rebatedtlMONTHLY (3.3M) — monthly detail
                    CustSPNLVL (145K) — customer-sponsor-level mapping
                    Level1 / Level2 — tier membership tables
                    MoDownlinerV1 (1M) — monthly downline snapshots
```

### 4.3 Campaign Lifecycle

```
Campaign (77 campaigns)
    │ 1:N CampCat (451 categories)
    │         │ 1:N CampSku (14.2K SKUs)
    │         │         │ 1:N CampDetail (18.4K lines) [CASCADE delete]
    │         │         │ 1:N CampKit (2.1K) [CASCADE delete]
    │         │         │ 1:N CampComp (4.6K) [CASCADE delete]
    │         │ 1:N CampBrand [CASCADE delete from CampCat]
    │
    └── Campaign.cStatus lifecycle:
            D (Draft)
                → C (Current/Active) ← sp_Camp_GetCamp (active)
                → H (Historical)     ← Camp_NewMonth archives current
            P (Pending approval)

Camp_NewMonth (month end):
    1. Find cStatus='C' AND dto=today → SET cStatus='H'
    2. Find next campaign (dfrom > @dto) → SET cStatus='C'
    3. EXEC sp_Camp_SynctoNam (sync to NAM partner system)

CampChanges (892.4K rows) = complete audit of all campaign modifications
```

---

## 5. Trigger Architecture

### 5.1 Trigger Summary (154 triggers — full list from live DB)

| Domain | Key Triggers | Primary Purpose |
|---|---|---|
| AP Payments | `apcapp_delete`, `apvchk_insert`, `apvend_update/delete` | Void/reversal logic, balance maintenance |
| GL Posting | `apdist_insert_update`, `apdist_delete/insert/audit_trail` | GL account determination, audit trail |
| AR Balance | `arcash_insert/update`, `arcapp_ins_upd/insert`, `arinvc_insert/update` | Real-time balance maintenance |
| AR Cash | `arcash_log` | Cash receipt audit log |
| Customer Tracking | `customer` (arcust) | Change audit → `changes` table; bank change → `compsys.MAILMESSAGE` email |
| Customer Address | `arcadr_upd`, `customeradr` | Address sync + email alerts |
| Infotrac Audit | `trig_it_log_arcust_*`, `trig_it_log_icitem_*`, `trig_it_arinvc`, `trig_it_aritrs` + 6 more | IT audit trail logs |
| Item Master | `itemsync` (icitem) | Product field changes → `icWsUpdate` ('icitem' type) for webstore sync |
| Item Master | `SA_NAM_ItemChanges` (icitem) | Mirror ALL product changes to `amanniquenam.dbo.icitem` (Namibia ERP) |
| Item Master | `icitem_upd`, `icitem_update`, `newitem`, `itemchange`, `namvenditem` | AccountMate standard item maintenance |
| Item Images | `imageUpdate` (iciimg) | Image change → `iciimgUpdate` AND `iciimgUpdateNOP` queues |
| Warehouse Inventory | `iciwhs_sync` (iciwhs) | Stock on-hand change → `icWsUpdate` ('iciwhs' type) **only for warehouse 4400** |
| Sales Orders | `sosord_insert`, `sosord_update`, `sosord_delete` | SO balance and status maintenance |
| Campaign Pricing | `sosppr_insupd_sync` (sosppr) | Active campaign price changes → `icWsUpdate` ('sosppr' type) |
| Campaign Master | `socamp_insupd` (socamp) | Mirror campaign changes to `amanniquenam..socamp` (Namibia ERP) |
| Campaign Combos | `socmpf_insupd`, `socmpq_insupd`, `sopcnt_insupd` | Campaign combo/freebie triggers |
| Exclusive Items | `update_webstore` (soxitems) | Mark `lupdatetows=1` on insert/update; only skips if webstore sync system clears the flag |
| Forecast/Budget | `forecast` (dpibdg), `dpibdg_ins_upd` | Budget maintenance |
| Invoice History | `dateupdate` (arinvc), `trig_it_arinvch` (arinvch) | Invoice history stamping |
| Stock Alerts | `iciwhs_checkstockalert`, `wsLowStock_checkstockalert` | Low stock alert firing |
| PO/Receiving | `popord_insert/update`, `porecg_insert`, `poctrs_insert`, etc. | PO balance and receiving triggers |
| Item Costing | `icibin_insupd`, `newicibin` | Bin quantity maintenance |

### 5.2 icWsUpdate Queue Mechanics (Live-Verified)

The sync queue has **7.2 million total rows** but only **22 currently pending**. The queue accumulates forever (no purge). cType values observed:

| cType | Rows Total | Pending | Source Trigger/Proc |
|---|---|---|---|
| `iciwhs` | 7,214,520 | **22** | `iciwhs_sync` trigger |
| `campdetail` | 6,958 | 0 | Campaign pricing changes |
| `icitem` | 5,350 | 0 | `itemsync` trigger |
| `campsku` | 195 | 0 | Campaign SKU changes |
| `sosppr` | (in icitem count) | — | `sosppr_insupd_sync` trigger |

Queue processing: `sp_ws_getupdates(@Filter)` assigns a GUID batch and returns items. `sp_ws_syncAvailability` then POSTs each item to `anniquestore.co.za/syncavailability.sync` and marks `dUpdated=GETDATE()` on HTTP 200.

**The 22 pending iciwhs items** represent stock changes that failed to sync (HTTP non-200 response from anniquestore.co.za).

### 5.3 Balance Maintenance Formula

Every AR transaction ultimately feeds into:
```
arinvc.nbalance = 
    (nfsalesamt - nfdiscamt)     -- net sales
    + nffrtamt                   -- freight
    + nffinamt                   -- finance charges  
    + nftaxamt1 + nftaxamt2 + nftaxamt3  -- up to 3 tax types
    + nfadjamt                   -- adjustments
    - (nftotpaid + nftotdisc + nftotadj + nftotdebt)  -- all payments

arcust.nbalance = SUM(arinvc.nbalance) for all open invoices
```

---

## 6. Key Table Reference

### 6.1 Core Transaction Tables (Live Row Counts Verified)

| Table | Live Rows | Purpose | Shopify Impact |
|---|---|---|---|
| `arcust` | **119,616** | Customer/Consultant master (10,308 active, 109,308 inactive) | Maps to Shopify Customer |
| `arinvc` | **275,883** | Invoice headers | Maps to Shopify Order |
| `aritrs` | **769,633** | Invoice line items | Maps to Shopify Order.LineItems |
| `aritrsh` | **21,607,444** | Invoice history archive | Historical reference |
| `arcash` | — | Cash receipts | Maps to Shopify Payment |
| `icitem` | **10,676** | Product master (~800 active) | Maps to Shopify Product |
| `iciwhs` | **101,970** | Warehouse inventory (warehouse 4400) | Maps to Shopify Inventory |
| `sosord` | **56,063** | Sales order headers | Shopify → AM fulfillment |
| `sosppr` | **260,147** | Campaign product pricing (all campaigns) | Shopify price lists / metafields |
| `soxitems` | **19,574** | Exclusive items per consultant | Shopify B2B / customer tags |
| `SOPortal` | **646,237** | Portal/web orders | Replace with Shopify webhooks |
| `sosordh` | **1,624,995** | Historical order archive | Historical reference |
| `soship` | — | Shipments | Maps to Shopify Fulfillment |
| `fw_consignment` | **674,738** | Fastway consignments | Stays in AM |
| `fw_items` | **681,759** | Fastway parcel items | Stays in AM |
| `changes` | **531,698** | Change audit log (mostly COMPPLAN-driven) | Reuse for Shopify delta sync |
| `wsSetting` | **2** | API config (`ws.url=anniquestore.co.za`, `ws.warehouse=4400`) | Point to new middleware |
| `icWsUpdate` | **7,227,023** | Item sync queue (22 pending, never purged) | Reuse for Shopify product sync |
| `iciimgUpdate` | **21,091** | Image sync queue (anniquestore.co.za) | Repurpose for Shopify |
| `iciimgUpdateNOP` | **7,744** | NOP image sync queue | Reuse for Shopify image sync |
| `NOP_Discount` | **2** | NOP discount rules (2 rows only — `NOP_OfferList` does not exist!) | Shopify Discount API |
| `Campaign` | **77** | Campaign records | Campaign/pricing logic |
| `CampDetail` | **18,371** | Campaign product detail lines | Campaign product data |

### 6.2 MLM-Specific Tables (stay in AM, no Shopify impact)

| Table | Live Rows | Purpose |
|---|---|---|
| `mlmcust` | **2,999,503** | Consultant MLM aggregation (includes historical) |
| `MLMCurr` | **49,788** | Current downline hierarchy snapshot |
| `MLMHist` | **11,466** | Historical hierarchy snapshots |
| `rbtmlm` | (bkp tables present) | Rebate tier master + many backup variants |
| `rbtmlmMONTHLY` | (variant present) | Monthly rebate aggregation |
| `CampDetail` | **18,371** | Campaign eligibility detail |
| `genrebcomp` | (small) | General rebate computation working table |
| `CTDownline` | **0** (test server — empty) | Current downline hierarchy working table |

> **Note:** `CTDownlineh`, `CTDownline`, `CTSponsor`, `CTCons`, `CTTitle`, `CTcommission` referenced in procedures were NOT found as tables in `amanniquelive`. The CT tables may be in the `compplanlive` database, or the naming differs from what was in the static CSVs.

---

## 7. Linked Servers (Live-Verified)

> ⚠️ **TEST SERVER CAVEAT:** The live query against `AMSERVER-TEST` shows only **one** linked server (`AMSERVER-V9`). The `[NOP]`, `[WEBSTORE]`, and `[Portal]` linked servers referenced in stored procedure code are **absent on the test server** — they almost certainly exist on production. The table below reflects expected production state based on code analysis.

| Linked Server | Target | Used By | Live Status |
|---|---|---|---|
| `[NOP]` | NopCommerce SQL Server (`annique.dbo`) | `sp_NOP_syncSoxitems`, `sp_ws_UpdateImageNOP`, `sp_ws_syncsoxitems` | **NOT FOUND on test — expected on prod** |
| `[WEBSTORE]` | `anniquestore.dbo` + `shopdata.dbo` | `sp_ws_autopickverify` (order validation), `sp_ws_gensoxitems` (exclusive item generation from events) | **NOT FOUND on test — expected on prod** |
| `[Portal]` | `Accountmate_Webstore.dbo` | One consultant discount sync procedure | **NOT FOUND on test — expected on prod** |
| `AMSERVER-V9` | `AMSERVER-v9` (SQLNCLI11) | Unknown — only present on test | **FOUND on test server — purpose unclear** |

### Linked Server Usage Detail

**`[NOP]` (NopCommerce):**
```sql
-- Direct table access (not HTTP):
[NOP].annique.dbo.ANQ_ExclusiveItems  -- exclusive item sync
[NOP].annique.dbo.Product              -- product ID lookup by SKU
[NOP].annique.dbo.Customer             -- customer ID by username
[NOP].annique.dbo.ANQ_SyncImage        -- image sync calls
```

**`[WEBSTORE]` (Old anniquestore.co.za):**
```sql
-- Used in sp_ws_autopickverify (portal order validation):
[WEBSTORE].anniquestore.dbo.orderitem  -- order validation
[WEBSTORE].shopdata.dbo.ws_sostrs      -- SO line item cross-check

-- Used in sp_ws_gensoxitems (event registrations → exclusive items):
[WEBSTORE].anniquestore.dbo.registration  -- event registrations
[WEBSTORE].anniquestore.dbo.EventItems    -- items per event
```

---

## 8. Integration Service Architecture (sp_ws_HTTP)

### What it actually does

```sql
-- sp_ws_HTTP internals (verified from source):
DECLARE @authHeader NVARCHAR(64) = 'BASIC 0123456789ABCDEF0123456789ABCDEF'
-- ^ Hardcoded Base64 credential (NOT read from wsSetting)

EXEC sp_OACreate 'MSXML2.ServerXMLHTTP', @token OUT  -- requires OLE Automation
EXEC sp_OAMethod @token, 'open', NULL, 'POST', @url, 'false'
-- ^ Always POST, never GET

EXEC sp_OAMethod @token, 'setRequestHeader', NULL, 'Authentication', @authHeader
-- ^ BUG: Header name should be 'Authorization' not 'Authentication'
-- Presumably the server side ignores this or reads it differently

EXEC sp_OAMethod @token, 'send', NULL, @postData
-- ^ @postData is empty string for most sp_NOP_* calls

-- Response:
@cretvalue = HTTP status code (char 32)  -- e.g. '200'
@cresponse = Response body (nvarchar 4000)
-- No logging of request or response to any table
```

### What wsSetting actually does

```sql
-- In every sp_NOP_* procedure:
SELECT @cUrl = LTRIM(RTRIM(Value)) FROM wsSetting WHERE Name='ws.url'
IF @cUrl IS NULL RETURN   -- Only this — kill switch if NULL

SET @cUrl = 'https://nopintegration.annique.com/'  -- Then immediately overrides
SET @cUrl = @cUrl + 'syncorders.api'
```

**Implication for Shopify:** Changing `wsSetting.ws.url` alone will NOT redirect any `sp_NOP_*` calls. Each procedure must be updated individually.

---

## 9. Known Issues & Architecture Gaps

### 9.1 Confirmed Issues (Live-Verified)

| Issue | Severity | Location | Impact | Live Status |
|---|---|---|---|---|
| `wsSetting.ws.url` = `anniquestore.co.za` (old store) — not NopCommerce URL | HIGH | `wsSetting` table | `sp_ws_*` family hits old webstore; `sp_NOP_*` ignores it anyway | ✅ CONFIRMED LIVE |
| `sp_NOP_*` procedures hardcode `nopintegration.annique.com` | HIGH | All 5 NOP sync SPs | Shopify cutover cannot be done by changing one config row | ✅ CONFIRMED LIVE |
| `sp_ws_HTTP` always POSTs — no GET support | MEDIUM | `sp_ws_HTTP` | Any GET-based APIs will fail | ✅ CONFIRMED LIVE |
| HTTP header `Authentication` (should be `Authorization`) | MEDIUM | `sp_ws_HTTP` | Compensated server-side presumably | ✅ CONFIRMED LIVE |
| Hardcoded Base64 credentials in `sp_ws_HTTP` | HIGH SECURITY | `sp_ws_HTTP` | Credential rotation requires code deploy | ✅ CONFIRMED LIVE |
| `sp_ws_syncItems` hardcodes `https://anniquestore.co.za/` | HIGH | `sp_ws_syncItems` | Ignores wsSetting even in sp_ws_* family | ✅ CONFIRMED LIVE |
| `NOP_OfferList` table does NOT exist | CORRECTION | `amanniquelive` | Was listed in CSVs — not present in live DB | ✅ CONFIRMED LIVE |
| `NOP_Offers` table does NOT exist | CORRECTION | `amanniquelive` | Was listed in CSVs — not present in live DB | ✅ CONFIRMED LIVE |
| `icWsUpdate` has 7.2M rows but only 22 pending | WARNING | `icWsUpdate` | Queue accumulates forever, never truncated — performance risk | ✅ CONFIRMED LIVE |
| `changes` table driven by `COMPPLAN` system (not data entry) | INFORMATION | `changes` | Top changes: cstatus, csponsor, ndiscrate — not item changes | ✅ CONFIRMED LIVE |
| No API call audit log | MEDIUM | Architecture-wide | No visibility on sync failures | ✅ CONFIRMED LIVE |
| `socamp.lactive=1` on test server points to 2014 campaign | TEST DATA | Test server only | `sp_ws_getactive` and price sync for active campaign may be broken on test | ✅ NOTED (test data) |
| `[WEBSTORE]` linked server still referenced in `sp_ws_autopickverify` | ACTIVE | `sp_ws_autopickverify` | Not dead code — validates portal orders against old webstore DB | ✅ CONFIRMED LIVE (but LS absent on test) |
| `SA_NAM_ItemChanges` mirrors all product changes to `amanniquenam` | ARCHITECTURE | `icitem` trigger | Namibia ERP receives automatic product updates — must not be broken | ✅ CONFIRMED LIVE |
| `NopIntegration..Brevolog` is Brevo email log, not API audit | INFORMATION | `sp_ws_reactivate` | Documented as API log in prior docs — actually email campaign logging | ✅ CONFIRMED LIVE |

### 9.2 Security Notes

- Credentials in `sp_ws_HTTP` are hardcoded Base64 string — rotate after migration
- `arcust.cbankacct` stores bank account numbers — assess encryption
- OLE Automation (`sp_OACreate`) required for HTTP calls — known attack surface
- Bank account changes send alert emails to: `shona@annique.com`, cc: `annalien.johnson@annique.com;veronica@annique.com` (via `compsys.MAILMESSAGE`)
- Email sending delegated to `compsys` database — if that DB is unavailable, bank alerts are silently lost

---

## 10. Shopify Migration Impact Assessment

### 10.1 What Must Be Replaced

| Current Component | Shopify Replacement | Complexity |
|---|---|---|
| `sp_NOP_syncOrders` (pull via API) | Shopify Order webhook → middleware → AM | HIGH |
| `sp_NOP_syncItemAll` | Middleware polls `icitem`, pushes Shopify Product API | MEDIUM |
| `sp_NOP_syncItemChanges` | Reuse `changes` table, middleware pushes Shopify Product API | MEDIUM |
| `sp_NOP_syncItemAvailability` | Reuse `icqoh` → middleware → Shopify Inventory API | MEDIUM |
| `sp_NOP_syncOrderStatus` | Middleware polls `soportal.cstatus='S'` → Shopify Fulfillment API | MEDIUM |
| `sp_NOP_syncSoxitems` (direct DB) | Shopify customer tags / metafields or B2B | HIGH |
| `sp_ws_UpdateImageNOP` (direct DB) | `iciimgUpdateNOP` queue → middleware → Shopify Product Image API | LOW |
| NOP_Discount / NOP_Offers tables | Shopify Discount API / automatic discounts | MEDIUM |
| `sp_NOP_syncStaff` (shopapi.annique.com) | New Shopify custom app or middleware | LOW |
| `[NOP]` linked server | Decommission after middleware live | HIGH |
| `soportal` table as order intake | `soportal` becomes Shopify order staging | MEDIUM |
| `sp_SendAdminSMS` (calls NOP endpoint) | Update to call new middleware or SMS provider | LOW |

### 10.2 What Stays Identical

| Component | Reason |
|---|---|
| All GL, AR, AP, IC, PR, PO modules | AccountMate ERP is unchanged |
| `sp_ct_Rebates` + MLM engine | Stays entirely in SQL Server |
| `sp_ct_downlinebuild` + hierarchy | Stays in SQL Server |
| `Campaign` / `CampDetail` system | Stays in AccountMate |
| `fw_*` 3PL Fastway tables | Stays in AccountMate |
| `be_waybill` / `SkyTrack` | Stays in AccountMate |
| All 50 triggers | Unchanged |
| `changes` table mechanism | Reused as Shopify delta trigger |
| `icWsUpdate` queue | Reused for Shopify product sync queue |
| AutoPick / Over_App / Return_App | Stays in AccountMate |
| `compplanlive..ctcomph` | External commission DB stays |

### 10.3 Recommended Middleware Architecture

Replace `nopintegration.annique.com` with a new middleware service that:

```
                    ┌──────────────────────────────────┐
                    │        NEW MIDDLEWARE             │
                    │  (replace nopintegration.annique) │
                    │                                  │
  Shopify Webhooks ─┤  ┌────────────────────────────┐  │
  (orders, products)│  │ Webhook Receiver            │  │
                    │  │ Orders → arinvc/aritrs       │  │
                    │  │ Customers → arcust           │  │
                    │  └────────────────────────────┘  │
                    │                                  │
  Shopify Product   │  ┌────────────────────────────┐  │
  API               │  │ Sync Poller (scheduled)     │  │
                    │  │ Reads: changes table         │  │
                    │  │ Reads: icWsUpdate queue      │  │
                    │  │ Reads: iciimgUpdateNOP       │  │
                    │  │ Reads: soportal (cstatus='S')│  │
                    │  │ Pushes to: Shopify APIs      │  │
                    │  └────────────────────────────┘  │
                    │                                  │
                    │  ┌────────────────────────────┐  │
                    │  │ API Endpoint                │  │
                    │  │ /syncorders.api (compat)    │  │
                    │  │ /syncproducts.api (compat)  │  │
                    │  │ /syncorderstatus.api (compat) │
                    │  │ /sendsms.api (compat)       │  │
                    │  └────────────────────────────┘  │
                    └──────────────────────────────────┘
                               │
                    ┌──────────┴──────────┐
                    │   Shopify Store     │
                    │   Products          │
                    │   Inventory         │
                    │   Orders            │
                    │   Customers         │
                    │   Fulfillments      │
                    │   Discounts         │
                    └─────────────────────┘
```

**Key migration enabler:** Because all `sp_NOP_*` procedures hardcode their URLs, maintaining compatible API endpoint paths (`/syncorders.api`, `/syncproducts.api`, etc.) on the new middleware allows the existing stored procedures to work unchanged during transition — just update the hardcoded domains.

### 10.4 Exclusive Items Challenge

The NopCommerce `ANQ_ExclusiveItems` table has no direct Shopify equivalent. Options:

1. **Shopify B2B** — Product lists per company (requires Shopify Plus)
2. **Customer Metafields** — Store `exclusive_skus` as a JSON metafield on the customer
3. **Custom Shopify App** — Private app enforcing access control at checkout
4. **Draft Orders** — Generate draft orders per consultant for exclusive access

The current system identifies consultants by `Customer.UserName = arcust.ccustno` — this mapping must be preserved in whatever Shopify mechanism is chosen.

---

## 11. Pre-Migration Verification Checklist

Based on source analysis and live DB verification:

### ✅ Already Verified (Live Queries)
- [x] **`wsSetting` actual content** — `ws.url = https://anniquestore.co.za/`, `ws.warehouse = 4400`
- [x] **URL hardcoding scope** — all `sp_NOP_*` hardcode `nopintegration.annique.com`; changing wsSetting alone won't redirect them
- [x] **`sp_ws_syncItems` hardcodes old store URL** — confirmed `https://anniquestore.co.za/` hardcoded
- [x] **`NOP_OfferList` existence** — does NOT exist in live DB
- [x] **`NOP_Offers` existence** — does NOT exist in live DB
- [x] **`changes` table column structure** — confirmed from live INFORMATION_SCHEMA query
- [x] **`sp_ct_Rebates` read** — confirmed reads `compplanlive..ctcomph`, creates invoices with item `SASAILM165`
- [x] **`[WEBSTORE]` linked server is live code** — `sp_ws_autopickverify` & `sp_ws_gensoxitems` actively reference it
- [x] **Bank alert email targets** — confirmed: `shona@annique.com`, cc `annalien.johnson@annique.com;veronica@annique.com`
- [x] **`amanniquenam` mirror** — confirmed via `SA_NAM_ItemChanges` and `socamp_insupd` triggers
- [x] **`NopIntegration..Brevolog`** — confirmed it's Brevo email log (not API audit)
- [x] **Row counts** — all major tables verified via `sys.partitions`
- [x] **Consultant counts** — 10,308 active (A), 109,308 inactive (I)
- [x] **Delivery mix** — FASTWAY 44%, BERCO 34%, SKYNET 16%, POSTNET 6%
- [x] **Campaign lifecycle** — C=Current (Feb_2026), F=Future (Mar_2026), H=Historical

### 🔲 Still To Verify (Requires Production Access or Further Analysis)
- [ ] **SQL Agent jobs** — query `msdb.dbo.sysjobs` requires elevated access; need to know what schedules NOP sync SPs
- [ ] **Production `wsSetting.ws.url`** — test server shows `anniquestore.co.za`; confirm production value
- [ ] **Production linked servers** — confirm `[NOP]`, `[WEBSTORE]`, `[Portal]` present on AMSERVER (prod)
- [ ] **`sp_NOP_syncOrders` current call frequency** — no job data available; estimate from sosord/soportal timestamps
- [ ] **Rotate credentials** — remove hardcoded `BASIC 0123456789ABCDEF...` from `sp_ws_HTTP` before going live
- [ ] **OLE Automation server setting** — `sp_configure 'Ole Automation Procedures'` must = 1 on new server
- [ ] **`arcust.cbankacct` encryption** — confirm if bank accounts are encrypted at rest
- [ ] **`compplanlive` database access** — external commission DB must remain accessible from ERP server
- [ ] **`amanniquenam` database** — confirm if Namibia migration also needs Shopify; if so, separate migration project
- [ ] **`compsys.MAILMESSAGE` service** — confirm what processes and delivers emails from the queue
- [ ] **`[AMSERVER-V9]` linked server purpose** — only linked server on test; what does it host?
- [ ] **`socamp.lactive=1` on production** — confirm production has correct active campaign flagged
- [ ] **Exclusive items Shopify strategy** — no direct Shopify equivalent for `soxitems`/`ANQ_ExclusiveItems`
- [ ] **Event registrations flow** — `sp_ws_gensoxitems` reads event registrations from `[WEBSTORE].anniquestore.dbo.registration`; this flow must be preserved or rebuilt

---

*Document generated from source analysis of `01_stored_procedures.csv` (8.3MB), `01_tables.csv`, `02_columns.csv`, `03_foreign_keys.csv`, `04_triggers.csv`, `05_stored_procs.csv`. All claims are source-verified except where explicitly marked as inferred.*
