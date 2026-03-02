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
| **All `sp_NOP_*` HTTP calls are "doorbell POSTs"** — empty body, no payload, response ignored | Verified from full source of sp_NOP_syncOrders, sp_NOP_syncItemAll, sp_NOP_syncItemAvailability, sp_NOP_syncItemChanges, sp_NOP_syncOrderStatus |
| **`nopintegration.annique.com` is a complete black box with its own DB credentials** — it reads `amanniquelive` directly; ERP only sends a wake-up ping | Deduced from doorbell POST pattern; all sync logic lives inside this service |
| **`sp_ws_UpdateImage` contains plaintext Namibia backoffice admin credentials** in the POST body | sp_ws_UpdateImage source: `WebLogin_txtPassword=4nnique4admin@` — CRITICAL security finding |
| **`backofficenam.annique.com`** is a previously undocumented integration endpoint for Namibia image sync | sp_ws_UpdateImage source: `https://backofficenam.annique.com/jsoncallbacks.ann` |

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

### 8.1 The "Doorbell POST" Pattern — How the Integration Service Actually Works

> **This is one of the most important architectural facts for the Shopify migration.**

The ERP stored procedures send **no data payload** to `nopintegration.annique.com`. The full call for `sp_NOP_syncOrders` is:

```sql
SET @cUrl = 'https://nopintegration.annique.com/syncorders.api'
EXEC sp_ws_HTTP @cURL = @curl, @cretvalue = @cretvalue OUTPUT, @cresponse = @cresponse OUTPUT
-- @cpostData defaults to '' — empty string
```

The `@cresponse` is captured but **never read, acted upon, or logged** by any calling SP. The procedure checks only whether `@cretvalue = '200'` (or ignores even that in some SPs).

**The HTTP POST is a doorbell — it carries no data.** It is a fire-and-forget wake-up signal.

#### How the integration service knows what to do

`nopintegration.annique.com` has its **own direct SQL connection to `amanniquelive`**. When it receives a POST to `/syncorders.api`, it:

```
ERP fires:  POST /syncorders.api  ← empty body

nopintegration service:
    1. Opens its own SQL connection to amanniquelive (own credentials, unknown)
    2. Reads whatever it needs directly from the ERP database:
           /syncorders.api        → reads SOPortal / NOP Orders
           /syncproducts.api?type=all       → reads icitem
           /syncproducts.api?type=changes   → reads changes table (cfieldname='cstatus')
           /syncproducts.api?type=availability → reads icqoh
           /syncorderstatus.api   → reads soportal WHERE cstatus='S'
    3. Determines what has changed (using its own state tracking / timestamps)
    4. Pushes the results to NopCommerce
    5. Returns HTTP 200 to the ERP (no payload in response either)

ERP sees 200 → SP ends. No record of what was processed.
```

The `?type=` query parameter is the only "instruction" the ERP gives — it selects which sync mode the service runs. The `?instancing=single` parameter likely tells the service to prevent overlapping concurrent runs.

#### Critical implications

| Implication | Impact |
|---|---|
| The service has its own DB credentials to `amanniquelive` | These credentials are unknown and unaudited from the ERP side |
| The service tracks sync state itself | It knows "what did I last sync" — probably via timestamp or row ID watermarks unknown to us |
| The ERP has zero visibility into what was processed | No record of which orders, products, or stock levels were actually sent |
| Schema changes break it silently | If a column is renamed, the ERP SP still receives HTTP 200, but nothing was synced |
| `nopintegration.annique.com` is a complete black box | No source code, no admin panel, no logs visible from the ERP side |
| Replacing it requires replicating its unknown internal logic | The new Shopify middleware must independently discover what the existing service does when each endpoint is hit |

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
- **`sp_ws_UpdateImage` exposes Namibia backoffice admin credentials in plaintext** — `WebLogin_txtUsername=Administrator` / `WebLogin_txtPassword=4nnique4admin@` are hardcoded in the stored procedure body. Any user with `VIEW DEFINITION` permission on `amanniquelive` (or access to the CSV exports) can read these credentials. This is a CRITICAL security finding independent of the Shopify migration and should be remediated immediately by: (1) rotating the Namibia backoffice admin password, (2) creating a restricted API user for this call, (3) moving credentials out of SP source into an encrypted config table.
- **`nopintegration.annique.com` holds unknown ERP database credentials** — the integration service must have a SQL login to `amanniquelive`. These credentials are invisible from within the ERP and have never been audited from this side. The scope of access (read-only? read-write? which tables?) is unknown.

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

> ⚠️ **CRITICAL CONSTRAINT — The Black Box Problem:**
> The compatible-path approach above assumes the new middleware can faithfully replicate what `nopintegration.annique.com` does internally when each endpoint receives a POST. **This is not currently known.** The existing service has its own DB credentials, its own state tracking, and its own sync logic — none of which is visible from the ERP. Before the new middleware can be designed in detail, the following must be established:
>
> 1. **Who owns / built `nopintegration.annique.com`?** Is there source code? A vendor who can hand it over?
> 2. **What SQL login does it use?** What tables does it read, and how does it track sync state (watermark columns? timestamp comparisons? a dedicated state table)?
> 3. **What does it write to NopCommerce?** For each endpoint, what is the exact data shape pushed to NopCommerce — field mappings, transformations, exclusions?
> 4. **Does it have internal scheduling?** Are there sync jobs inside the service that fire independently of the ERP POST calls?
>
> Without answering these, the middleware spec is incomplete and the migration carries **HIGH risk of silent data divergence** between the ERP and Shopify after go-live.

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
- [ ] **`nopintegration.annique.com` — obtain source code or owner contact** — black box; must be reverse-engineered or handed over before middleware can be specced (see RISK-13)
- [ ] **`nopintegration.annique.com` SQL login identity** — what SQL user does it connect as, and what permissions does it hold on `amanniquelive`?
- [ ] **`nopintegration.annique.com` state tracking** — does it use timestamp watermarks, a dedicated sync-state table, or row ID cursoring to determine "what's new"?
- [ ] **`backofficenam.annique.com` admin credentials** — rotate `Administrator` password immediately; create restricted API user (see RISK-14, confirmed security finding)
- [ ] **`shopapi.annique.com` — obtain source code or owner contact** — separate black box; affects staff/consultant sync to webstore

---

## 12. Endpoint Catalogue (Source-Verified)

All HTTP endpoints discovered in stored procedure source code. Where the endpoint is not to an external system, the internal stored procedure or table is listed.

### 12.1 `nopintegration.annique.com` — NopCommerce Integration Middleware

The current NopCommerce integration middleware. Called exclusively via `sp_ws_HTTP` using OLE Automation POST. All calls are fire-and-forget with no response logging.

| Endpoint | Called By SP | Direction | Purpose | Last Modified | Notes |
|---|---|---|---|---|---|
| `POST /syncorders.api` | `sp_NOP_syncOrders` | NOP → AM | Trigger the middleware to pull new NopCommerce orders into AccountMate (arinvc, aritrs) | 2026-01-09 | Most recently maintained SP — actively used |
| `POST /syncproducts.api?type=all&instancing=single` | `sp_NOP_syncItemAll` | AM → NOP | Full product catalog push (icitem → NOP Product table) | 2024-09-03 | Heavy operation — manual trigger only |
| `POST /syncproducts.api?type=availability` | `sp_NOP_syncItemAvailability` | AM → NOP | Push current stock levels (icqoh → NOP Product.StockQuantity) | 2024-09-03 | Stock sync |
| `POST /syncproducts.api?type=changes` | `sp_NOP_syncItemChanges` | AM → NOP | Delta product sync — reads `changes` table WHERE cfieldname='cstatus' | 2024-09-03 | Uses `changes` table as delta queue |
| `POST /syncorderstatus.api?instancing=single` | `sp_NOP_syncOrderStatus` | AM → NOP | Push fulfillment/shipping status change back to NopCommerce order | 2024-10-28 | Polls soportal for cStatus='S' |
| `POST /sendsms.api` | `sp_SendAdminSMS` | AM → NOP | Send admin SMS notification (JSON body with message + phone) | 2025-05-07 | Only SP with a real POST body |

**Auth:** All calls use hardcoded `Authentication: BASIC 0123456789ABCDEF0123456789ABCDEF` header. Note: header name is misspelled — should be `Authorization`.

---

### 12.2 `shopapi.annique.com` — Staff/Consultant Sync API

Separate endpoint for staff synchronisation. Distinct from nopintegration.

| Endpoint | Called By SP | Direction | Purpose | Last Modified |
|---|---|---|---|---|
| `POST /syncstaff.api` | `sp_NOP_syncStaff` | AM → ShopAPI | Sync consultant/staff accounts from arcust to webstore customer list | 2023-03-20 |

---

### 12.3 `anniquestore.co.za` — Old Webstore (sp_ws_* family)

The older webstore integration, predating NopCommerce. Some procedures still actively call it; `wsSetting.ws.url` points here on the test server.

| Endpoint | Called By SP | Direction | Purpose | Last Modified | Notes |
|---|---|---|---|---|---|
| `POST syncavailability.sync` | `sp_ws_syncAvailability` | AM → WS | Push single-item stock availability to old webstore | 2020-06-18 | Uses wsSetting URL |
| `POST syncavailability.sync?citemno=all&instancing=single` | `sp_ws_syncAvailabilityAll` | AM → WS | Full availability push for all items | 2020-06-18 | Uses wsSetting URL |
| `POST syncimage.sync?citemno={item}` | `sp_ws_SyncImages` | AM → WS | Per-item image sync to old webstore | 2020-06-18 | Uses wsSetting URL |
| `POST syncitems.sync` | `sp_ws_syncItems` | AM → WS | Item master sync | 2025-03-26 | **HARDCODES** `https://anniquestore.co.za/` — ignores wsSetting |
| `POST syncitems.sync?citemno=all` | `sp_ws_syncItemsAll` | AM → WS | Full item catalog sync | 2023-03-01 | Uses wsSetting URL |

**Queue source:** `icWsUpdate` (7.2M rows, 22 pending) feeds `sp_ws_getupdates` which batches items for these sync calls. `sp_ws_syncAvailability` marks `dUpdated=GETDATE()` on HTTP 200.

---

### 12.4 Direct DB Linked Server Access (no HTTP)

| Operation | SP / Trigger | Target | Tables | Purpose |
|---|---|---|---|---|
| Read product ID by SKU | `sp_NOP_syncSoxitems` | `[NOP].annique.dbo.Product` | `Product` | Look up NOP product ID for exclusive item mapping |
| Read customer ID by username | `sp_NOP_syncSoxitems` | `[NOP].annique.dbo.Customer` | `Customer` | Look up NOP customer ID (username = arcust.ccustno) |
| Write exclusive item | `sp_NOP_syncSoxitems` | `[NOP].annique.dbo.ANQ_ExclusiveItems` | `ANQ_ExclusiveItems` | Grant consultant access to exclusive product |
| Sync product images | `sp_ws_UpdateImageNOP` | `[NOP].annique.dbo.ANQ_SyncImage` | (procedure call) | Push updated images into NopCommerce |
| Validate portal order | `sp_ws_autopickverify` | `[WEBSTORE].anniquestore.dbo` | `orderitem`, `ws_sostrs` | Cross-check portal order against old webstore DB before autopick |
| Generate exclusive items from events | `sp_ws_gensoxitems` | `[WEBSTORE].anniquestore.dbo` | `registration`, `EventItems` | Create soxitems entries for event registrants |
| Read commission history | `sp_ct_Rebates` | `compplanlive..ctcomph` | `ctcomph` | Monthly commission source (external MLM database) |
| Mirror product changes | `SA_NAM_ItemChanges` trigger | `amanniquenam.dbo.icitem` | `icitem` | Automatic field-by-field Namibia ERP mirror |
| Mirror campaign changes | `socamp_insupd` trigger | `amanniquenam..socamp` | `socamp` | Sync campaign master to Namibia ERP |
| Email alert on bank change | `customer` trigger | `compsys.[dbo].[MAILMESSAGE]` | `MAILMESSAGE` | Alert Annique staff on arcust bank account change |

---

### 12.5 `backofficenam.annique.com` — Namibia Backoffice (Newly Discovered)

Found in `sp_ws_UpdateImage` — **not previously documented**. This is a separate endpoint for the Namibia backoffice system, called when a product image is updated.

| Endpoint | Called By SP | Direction | Purpose | Notes |
|---|---|---|---|---|
| `POST /jsoncallbacks.ann?method=updateimagefromam&citemno={itemno}` | `sp_ws_UpdateImage` | AM → NAM Backoffice | Push updated product image to Namibia backoffice system | Separate from `amanniquenam` SQL mirror |

**Auth:** This endpoint uses **form-POST credentials** in the request body (not a header):
```
WebLogin_txtUsername=Administrator
WebLogin_txtPassword=4nnique4admin@
```
This is a **critical security issue** — admin credentials for the Namibia backoffice are stored in plaintext in the stored procedure source. Anyone with `VIEW DEFINITION` permission on the ERP database can read them. See Section 9.2.

---

### 12.6 External Carrier APIs

| Carrier | Integration Point | Tables | Notes |
|---|---|---|---|
| **Fastway** | `sp_Fastway_AddManualConsignment` | `fw_consignment`, `fw_items`, `fw_labels`, `fw_hubs` | Fastway REST API for consignment creation. 44% of orders. |
| **Berco** | `be_waybill` table | `be_waybill`, `somanifest` | 34% of orders. Waybill generation method unclear without SP source. |
| **Skynet** | `SkyTrack` (inferred) | `SkyTrack` (table present) | 16% of orders. Tracking integration. |
| **Postnet** | `sp_ws_UPdatePostnetStores` | Unknown | 6% of orders. Update Postnet store list for collection points. |

---

## 13. High-Risk Issues & Showstoppers

Issues that could block, derail, or significantly increase the cost of the Shopify migration. Each item includes a **discovery action** — what must be confirmed before a proposal can be safely scoped.

### 🔴 SHOWSTOPPER / MUST RESOLVE BEFORE PROPOSAL

---

#### RISK-01: All `sp_NOP_*` endpoints are hardcoded — no single config change possible

**Location:** Source of `sp_NOP_syncOrders`, `sp_NOP_syncItemAll`, `sp_NOP_syncItemChanges`, `sp_NOP_syncItemAvailability`, `sp_NOP_syncOrderStatus`, `sp_NOP_syncStaff`

**Detail:** Every procedure reads `wsSetting.ws.url` only as a kill-switch (returns if NULL), then immediately overwrites the variable with `https://nopintegration.annique.com/...`. There is no configuration-based redirect possible. Cutover to Shopify requires **code changes** to each SP or deployment of a URL-compatible middleware that responds on the same paths.

**Discovery needed:** None — fully confirmed from source. The compatible-path middleware approach (Section 10.3) avoids touching these SPs during transition.

**Effort if not mitigated:** 6 SP rewrites + regression testing of all integration paths.

---

#### RISK-02: `sp_ws_HTTP` uses OLE Automation (`sp_OACreate`) — SQL Server feature that must be enabled

**Location:** `sp_ws_HTTP`, called by all integration SPs

**Detail:** SQL Server's OLE Automation Procedures must be enabled (`sp_configure 'Ole Automation Procedures', 1`). This is disabled by default and is a known attack surface. If migrating to a new SQL Server or Azure SQL, this feature may be unavailable entirely — **Azure SQL does not support OLE Automation**.

**Discovery needed:** `EXEC sp_configure 'Ole Automation Procedures'` on production. Confirm if any future server migration is planned (Azure SQL would break all HTTP calls from within the DB).

**Effort if blocked:** All 12 integration SPs must be refactored to remove the in-database HTTP calls — significant rework or replacement by middleware that pulls instead of being pushed.

---

#### RISK-03: Namibia ERP (`amanniquenam`) receives ALL product and campaign changes via triggers — any bypass breaks it

**Location:** `SA_NAM_ItemChanges` trigger on `icitem`, `socamp_insupd` trigger on `socamp`

**Detail:** Every field-level change to `icitem` is automatically mirrored to `amanniquenam.dbo.icitem`. Campaign changes on `socamp` mirror to `amanniquenam..socamp`. This is entirely invisible unless triggers are inspected. If the Shopify implementation manages products by calling Shopify API directly (bypassing the ERP), the Namibia database will go out of sync silently.

**Discovery needed:** Confirm whether `amanniquenam` on the **production server** is an active operational database used by Namibia staff, or a legacy remnant. If active, any product or campaign change must still flow through the ERP trigger chain.

**Effort if active:** All product management in the Shopify admin must write back through AccountMate — Namibia cannot be a separate migration track.

---

#### RISK-04: `[WEBSTORE]` linked server is still live production code (not dead)

**Location:** `sp_ws_autopickverify`, `sp_ws_gensoxitems`

**Detail:** `sp_ws_autopickverify` validates portal orders against `[WEBSTORE].anniquestore.dbo.orderitem` before allowing AutoPick to process them. `sp_ws_gensoxitems` generates exclusive items for consultants based on event registrations read from `[WEBSTORE].anniquestore.dbo.registration`. If the old webstore is decommissioned without updating these procedures, **AutoPick will fail or skip validation** and **event-based exclusive items will stop generating**.

**Discovery needed:** Confirm whether `anniquestore.co.za` is still running and whether the `[WEBSTORE]` linked server is present in production. Confirm if event registrations are still used.

**Effort if active:** `sp_ws_autopickverify` must be updated to remove the old webstore check or replace it with a Shopify order lookup. `sp_ws_gensoxitems` must be updated to use a new event registration source.

---

#### RISK-05: MLM commission calculation is entirely inside AccountMate SQL — no API surface

**Location:** `sp_ct_Rebates`, `sp_ct_downlinebuild`, `compplanlive..ctcomph`

**Detail:** The monthly rebate cycle reads from `compplanlive` (a separate database on the same SQL Server instance), performs complex multi-level commission calculations with a cursor-per-consultant loop, and writes credits/debits directly to `arinvc`, `aritrs`, `arcapp`. There is no API, no webhook, and no external system interface. This logic **cannot be replicated in Shopify** natively and must remain in AccountMate SQL Server indefinitely.

**Discovery needed:** Confirm that `compplanlive` access is not restricted by the same user/role constraints as the `DieselBrook` login. Confirm who triggers `sp_ct_Rebates` (SQL Agent job? Manual?).

**Effort:** No Shopify replacement required — stays in AccountMate. However, every Shopify order that is a billable MLM transaction must still create an `arinvc` record in AccountMate, or commissions will not be calculated.

---

#### RISK-06: Consultant exclusive items (`soxitems`) have no Shopify native equivalent

**Location:** `soxitems` table, `sp_NOP_syncSoxitems`, `[NOP].annique.dbo.ANQ_ExclusiveItems`

**Detail:** Annique uses per-consultant exclusive product access granted via `soxitems → ANQ_ExclusiveItems` in NopCommerce. Shopify has no built-in equivalent. Options (Shopify B2B, customer metafields, private app) each have significant limitations or cost implications. This affects approximately 10,308 active consultants who may have exclusive SKU allocations.

**Discovery needed:** Understand the business rules — how many consultants have exclusive items at any time, what triggers an exclusive item grant, and how consultants are notified. Understand if `soxitems` records are per-consultant or per-event batch.

**Effort:** HIGH — requires custom Shopify app or Shopify Plus B2B feature. This is non-trivial.

---

### 🟠 HIGH RISK — Requires Direct Code or Config Discovery

---

#### RISK-07: No API audit logging — sync failures are silent

**Location:** Architecture-wide — `sp_ws_HTTP` logs nothing on failure

**Detail:** When `sp_ws_HTTP` receives a non-200 response, it returns the status code as `@cretvalue` to the calling SP — but none of the calling SPs log failures to a table, send an alert, or retry. The only observable evidence of sync failure is accumulating `dUpdated IS NULL` rows in `icWsUpdate`. Currently 22 such rows exist.

**Discovery needed:** Confirm whether the NOP integration middleware (`nopintegration.annique.com`) has its own logging. Assess whether Annique staff are aware of the 22 stuck sync items.

**Effort for Shopify:** The new middleware must implement proper error logging and alerting — treat this as a baseline requirement for the new architecture.

---

#### RISK-08: SQL Agent job schedule is unknown — all sync timing is a black box

**Location:** `msdb.dbo.sysjobs` — access denied with current credentials

**Detail:** All sync procedures (`sp_NOP_syncOrders`, `sp_NOP_syncItemAll`, etc.) are believed to be run on a schedule via SQL Server Agent. Without job definitions, the actual sync frequency is unknown. If any jobs run every few minutes, the new middleware must match that frequency to avoid customer-visible stock discrepancies.

**Discovery needed:** Requires elevated SQL Server access or Windows credentials to query `msdb`. Must be obtained from Annique's DBA or system admin.

---

#### RISK-09: `arcust.cbankacct` and payment data — potential unencrypted PII

**Location:** `arcust` table, `customer` trigger

**Detail:** The `customer` trigger detects bank account changes and fires email alerts — indicating that bank account numbers are stored in `arcust`. It is unknown whether these are encrypted at rest. South African POPIA compliance requires that financial personal information be appropriately protected.

**Discovery needed:** `SELECT TOP 1 cbankacct FROM arcust WHERE cbankacct IS NOT NULL` — if the value is a legible account number, it is plaintext. This is a POPIA risk regardless of migration.

**Effort:** If plaintext — immediate risk requiring column-level encryption or vault migration, independent of Shopify scope.

---

#### RISK-10: AutoPick batching logic ties to portal and old webstore

**Location:** `AutoPick` SP, `sp_ws_autopickverify`

**Detail:** AutoPick only triggers if: `@rows = 0` (nothing pending) OR `(@mins >= 15 AND @rows >= 10)` — i.e., it batches orders in 15-minute or 10-order windows. It calls `sp_ws_autopickverify` which validates against the old `[WEBSTORE]` DB. If the portal is replaced by Shopify webhooks and the old webstore is retired, the autopick trigger logic and validation step both need updating.

**Discovery needed:** How do Shopify orders enter `soportal`? Confirm whether the new flow will be: Shopify webhook → middleware → soportal → AutoPick. The AutoPick logic itself can stay unchanged if `soportal` remains the staging table.

---

#### RISK-11: Campaign specials and pricing complexity — no direct Shopify mapping

**Location:** `Campaign`, `CampDetail`, `CampSku`, `sosppr` (260K rows), `socamp`

**Detail:** Annique's campaign system is the core of their monthly specials model. Each month has a campaign with discount rates, MLM rates, product categories, and SKU-level pricing locked in `sosppr`. NopCommerce uses `sp_ws_getactive` to pull the active campaign. Shopify's native discount system (percentage off, fixed amount, buy X get Y) does not natively represent this tiered campaign model where:
- Every consultant gets 20% off (campaign rate)
- MLM upline earns 21% (MLM rate)
- Only active-status consultants get campaign pricing
- Campaign pricing is attached to `socamp.ccampno`, not to individual Shopify discount codes

**Discovery needed:** How does Annique expect consultants to experience campaign pricing on Shopify? Does every consultant have their own login with automatic pricing applied, or is there a discount code mechanism?

**Effort:** MEDIUM–HIGH. Likely requires Shopify Price Lists (B2B) or a custom pricing app. The campaign data itself is clean and structured — the challenge is the Shopify representation.

---

#### RISK-12: Data residency — test server timestamp shows data is ~1 month old; production data state unknown

**Location:** Test server `AMSERVER-TEST`

**Detail:** All queries were run on the test server, which has data dated to approximately 2026-02-02. The production server (`AMSERVER`) is a separate machine. Production may differ in: linked server configurations, `wsSetting.ws.url`, active SQL Agent jobs, campaign state, and database versions.

**Discovery needed:** Direct access to production server, or a current production restore on the test server, before final pre-migration scoping.

---

#### RISK-13: `nopintegration.annique.com` is a complete black box — its internal logic is unknown and must be reverse-engineered before Shopify middleware can be built

**Location:** `nopintegration.annique.com` — the integration middleware

**Detail:** The ERP stored procedures send **no data** to this service — they fire an empty POST as a wake-up signal ("doorbell pattern"). The service has its own direct SQL connection to `amanniquelive` and independently decides what to read, what has changed, and what to push to NopCommerce. From the ERP side:

- **No source code** is available or known
- **No admin interface** is visible
- **No logs** are accessible
- **Its DB credentials** (which SQL login it uses to read `amanniquelive`) are unknown
- **Its state-tracking mechanism** (how it knows what was last synced) is unknown
- **Its error handling** (what happens when NopCommerce rejects a sync) is unknown
- **Its exact table-reading behaviour** (which queries it runs per endpoint) is unknown

The six endpoints we know about (`/syncorders.api`, `/syncproducts.api?type=all/changes/availability`, `/syncorderstatus.api`, `/sendsms.api`) represent the full extent of what we can see — there may be additional internal endpoints or scheduled tasks within the service that are not triggered by the ERP at all.

**Discovery needed:** This is the single biggest blocker for the Shopify middleware design. Must obtain: (1) source code or documentation from whoever built/hosts the service, (2) confirmation of who manages it (internal dev, third-party vendor, or Annique IT), (3) identity of the SQL login it uses and its permissions, (4) any internal scheduling or job runner it contains.

**Effort if source unavailable:** The new Shopify middleware must be built by observing the service's effects rather than its code — i.e., intercept what it writes to NopCommerce by comparing DB state before/after each endpoint is triggered. This is feasible but significantly extends the discovery phase. Estimated additional 2–4 weeks of reverse-engineering before middleware spec can be finalised.

**Risk if ignored:** Building the Shopify middleware without understanding the existing service means the new system will almost certainly miss edge cases, batch logic, error recovery paths, or data transformations that the current service handles silently.

---

#### RISK-14: `sp_ws_UpdateImage` exposes Namibia backoffice admin credentials in plaintext in the database

**Location:** `sp_ws_UpdateImage` stored procedure source

**Detail:** The procedure calls `https://backofficenam.annique.com/jsoncallbacks.ann?method=updateimagefromam` and authenticates by posting `WebLogin_txtUsername=Administrator&WebLogin_txtPassword=4nnique4admin@` as a form body. These credentials are hardcoded in the stored procedure and visible to any user with `VIEW DEFINITION` permission on `amanniquelive` — which includes the `DieselBrook` login used for this analysis.

**Discovery needed:** None — fully confirmed from source code.

**Immediate action required:** (1) Rotate the Namibia backoffice Administrator password now. (2) Create a restricted API-only user for this integration. (3) Move credentials out of the SP into an encrypted config table or SQL Server credential store. This is a POPIA and general security risk that exists **independently** of the Shopify migration and should not wait for it.

---

## 14. Process Integrity — What Must Stay Intact

This section maps existing business processes to their risk under the Shopify migration and what must be done to preserve them.

### 14.1 Mission-Critical Process Map

| Process | Current Mechanism | Shopify Impact | Preservation Action |
|---|---|---|---|
| **Monthly rebates / MLM commissions** | `sp_ct_Rebates` reads `compplanlive`, writes to `arinvc` | None — stays in AccountMate | Every Shopify order MUST create an `arinvc` record |
| **Downline hierarchy rebuild** | `sp_ct_downlinebuild` + recursive CTEs monthly | None — stays in AccountMate | No change needed |
| **Campaign pricing** | `socamp` → `sosppr` → consultant discount | Must be replicated in Shopify for all 10K active consultants | Use Shopify B2B price lists or custom pricing app per campaign period |
| **Exclusive item access** | `soxitems` → `ANQ_ExclusiveItems` (NOP) | NopCommerce gone — needs Shopify equivalent | Custom app, Shopify B2B lists, or customer metafields |
| **Order AutoPick & fulfillment** | `SOPortal` → AutoPick → Fastway/Berco/Skynet/Postnet | AutoPick stays in AccountMate — Shopify orders must populate `soportal` | Middleware writes Shopify orders into `soportal` |
| **Namibia product/campaign sync** | `SA_NAM_ItemChanges` + `socamp_insupd` triggers | Invisible — only breaks if trigger chain bypassed | All product & campaign writes must go through AccountMate ERP |
| **Inventory sync** | `iciwhs_sync` trigger → `icWsUpdate` queue | Queue source stays; queue consumer changes | Middleware polls `icWsUpdate` and pushes to Shopify Inventory API |
| **Image sync** | `imageUpdate` trigger → `iciimgUpdateNOP` → `sp_ws_UpdateImageNOP` | Queue stays; consumer changes | Middleware polls `iciimgUpdateNOP`, pushes to Shopify Product Image API |
| **Bank account change alerts** | `customer` trigger → `compsys.MAILMESSAGE` | No change — ERP side only | No change needed |
| **Order status → customer portal** | `sp_NOP_syncOrderStatus` → NOP order status | NOP gone — replace with Shopify Fulfillment API | Middleware marks Shopify fulfillment on soportal.cStatus='S' |
| **Carrier waybill generation** | Fastway REST API, Berco waybill, Skynet, Postnet | Stays in AccountMate — not Shopify's job | No change — all carrier integrations remain in ERP |
| **Monthly statement generation** | `sp_statement` + arstmt | Stays in AccountMate | No change — consultant statements from ERP |
| **GL posting** | All transactions → `gltrsn` (118M rows) | AccountMate is GL of record | Every Shopify order must post an AR invoice via middleware |
| **Staff/consultant sync** | `sp_NOP_syncStaff` → `shopapi.annique.com` | shopapi stays — update to include Shopify customer sync | Extend middleware to also create/update Shopify customers |

### 14.2 What Changes Fundamentally

| Current | Proposed Shopify Replacement |
|---|---|
| NopCommerce storefront | Shopify storefront (product pages, cart, checkout) |
| NopCommerce customer accounts | Shopify customer accounts (with MLM metafields) |
| NopCommerce order intake | Shopify order webhooks → middleware → AccountMate |
| `[NOP].annique.dbo` direct DB writes | Shopify API calls via middleware |
| NopCommerce `ANQ_ExclusiveItems` | Shopify B2B / customer tags / custom access control |
| NopCommerce product/image display | Shopify product catalog (Liquid themes or Hydrogen) |
| NopCommerce campaign/offer pages | Shopify custom sections + metaobjects for campaign content |
| `nopintegration.annique.com` middleware | New middleware (Node.js/Python) with compatible API paths |
| SOPortal as primary customer-facing portal | Shopify as customer-facing store; SOPortal becomes internal staging |

---

## 15. Proposed Shopify Architecture & Middleware

### 15.1 Architecture Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                        SHOPIFY (Cloud)                           │
│  ┌────────────────┐  ┌───────────────┐  ┌──────────────────┐   │
│  │  Storefront    │  │  Customer      │  │  Orders &        │   │
│  │  (Liquid/H2)   │  │  Accounts      │  │  Fulfillments    │   │
│  │  Products      │  │  + MLM meta    │  │  + Tracking      │   │
│  │  Campaign pages│  │  fields        │  │                  │   │
│  └───────┬────────┘  └───────┬───────┘  └────────┬─────────┘   │
│          │ Shopify APIs      │                    │             │
└──────────┼───────────────────┼────────────────────┼─────────────┘
           │                   │                    │
           ▼                   ▼                    ▼
┌──────────────────────────────────────────────────────────────────┐
│              MIDDLEWARE (new — replaces nopintegration)           │
│              e.g. Node.js / .NET / Python on VPS or Azure        │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Shopify Webhook Receivers                                │   │
│  │  orders/paid → create arinvc + sosord in AccountMate     │   │
│  │  orders/fulfilled → update soportal.cStatus              │   │
│  │  customers/create → sync to arcust                        │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Scheduled Pollers (replacing SQL Agent jobs)            │   │
│  │  Every 5 min : poll icWsUpdate → Shopify Inventory API   │   │
│  │  Every 5 min : poll iciimgUpdateNOP → Shopify Image API  │   │
│  │  Every 5 min : poll soportal cStatus='S' → Shopify FulfAPI│  │
│  │  On demand   : campaign change → Shopify price list update│  │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Legacy-Compatible API Paths (transition period)          │   │
│  │  POST /syncorders.api         (ERP SPs unchanged)        │   │
│  │  POST /syncproducts.api       (ERP SPs unchanged)        │   │
│  │  POST /syncorderstatus.api    (ERP SPs unchanged)        │   │
│  │  POST /sendsms.api            (ERP SPs unchanged)        │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  API Audit Log (new — currently missing)                  │   │
│  │  Every request/response logged with timestamp + status   │   │
│  │  Alerting on repeated failures                           │   │
│  └──────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
           │                   │
           ▼                   ▼
┌──────────────────────────────────────────────────────────────────┐
│              ACCOUNTMATE SQL SERVER (unchanged)                   │
│  amanniquelive: GL, AR, inventory, MLM, fulfillment, campaigns   │
│  amanniquenam:  Namibia mirror (auto via triggers — untouched)    │
│  compplanlive:  MLM commission engine (untouched)                 │
│  compsys:       Email alerts (untouched)                          │
└──────────────────────────────────────────────────────────────────┘
```

### 15.2 Shopify Feature Map

| Business Requirement | Shopify Feature | Plan Required | Custom Dev? |
|---|---|---|---|
| Product catalogue + search | Shopify native | Basic | No |
| Campaign specials (monthly discount) | Shopify **Price Lists** (B2B) or Discount API | Plus (B2B) or Advanced | Low — campaign sync |
| Consultant login (10K accounts) | Shopify **B2B** company accounts or standard customer accounts | Plus (B2B) recommended | Medium |
| Exclusive product access per consultant | Shopify **B2B** catalogs, or custom app with checkout validation | Plus (B2B) | High — custom app |
| Monthly campaign pages | Shopify **Metaobjects** + theme sections | Any | Low — theme dev |
| Stock availability (real-time) | Shopify Inventory API (via middleware poller) | Any | Medium — middleware |
| Order → AccountMate flow | Shopify Order webhook → middleware → AM | Any | High — core middleware |
| Fulfillment status → Shopify | Middleware polls soportal → Shopify Fulfillment API | Any | Medium — middleware |
| Consultant MLM rank display | Shopify Customer **Metafields** | Any | Low — sync only |
| Commission statement access | Custom page reading from middleware/ERP API | Any | Medium |
| Carrier tracking (Fastway etc.) | Shopify **Shipping** + tracking number on fulfillment | Any | Low |
| SMS notifications | Reuse `sp_SendAdminSMS` → update endpoint to new middleware | Any | Low |
| Image management | Shopify Product Image API via middleware | Any | Low — image poller |

### 15.3 Proposed Platform Stack

| Component | Proposed Platform | Rationale |
|---|---|---|
| **Storefront** | Shopify (Liquid or Hydrogen + Oxygen) | Purpose-built e-commerce, hosted, PCI compliant |
| **Middleware** | Node.js (Express) or .NET 8 on a VPS or Azure App Service | Low latency DB access to SQL Server; familiar to ERP developers |
| **Message queue** | Azure Service Bus or Redis Queue | Decouple Shopify webhook processing from AccountMate writes |
| **API logging** | Seq or Azure Application Insights | Currently zero visibility on sync failures |
| **Campaign management UI** | Shopify admin + custom metaobjects | No custom CMS needed for campaign pages |
| **MLM portal** | Custom Shopify app or Liquid account page with metafield data | Reuse existing arcust/MLM data via middleware API |
| **B2B / Exclusive access** | Shopify Plus B2B (if budget allows) or custom checkout validation app | Handles the soxitems exclusive-items problem |

### 15.4 Estimated Effort Summary

| Stream | Complexity | Effort | Dependency |
|---|---|---|---|
| Middleware core (webhook receiver + AccountMate writer) | HIGH | 6–10 weeks | SQL Server access, AccountMate SP knowledge |
| Shopify product + inventory sync (icWsUpdate poller) | MEDIUM | 3–4 weeks | Middleware core |
| Shopify order status → fulfillment sync | MEDIUM | 2–3 weeks | Middleware core |
| Campaign pricing on Shopify (B2B price lists or discount app) | HIGH | 4–6 weeks | Campaign data model confirmed |
| Consultant exclusive items (soxitems on Shopify) | HIGH | 4–8 weeks | Business rules confirmed, Shopify Plus decision |
| Customer migration (10K active arcust → Shopify) | MEDIUM | 2–3 weeks | Data cleanse of arcust |
| Product migration (~800 active icitem → Shopify) | LOW | 1–2 weeks | Images available via iciimg |
| Shopify theme (campaign pages, consultant portal) | MEDIUM | 4–6 weeks | UX design complete |
| Middleware API logging + monitoring | LOW | 1–2 weeks | Middleware core |
| Testing + UAT | HIGH | 4–6 weeks | All streams |
| **Total (parallel streams)** | | **~20–24 weeks** | Team of 3–4 developers |

> These are indicative estimates only. A more accurate scope requires: production server access, SQL Agent job definitions, confirmed Namibia operational status, and a decision on Shopify Plus vs. standard plan.

### 15.5 Decision Gates Before Proposal Can Be Finalised

The following decisions directly change scope, cost, and feasibility:

| Gate | Question | Impact if YES | Impact if NO |
|---|---|---|---|
| **G1** | Is `amanniquenam` (Namibia ERP) actively used? | All product writes must go through AccountMate — no direct Shopify product management | Can simplify product management flow |
| **G2** | Is Shopify Plus (B2B) in budget? | Exclusive items solved natively; consultant pricing via price lists | Custom app required for both — adds 6–10 weeks |
| **G3** | Will `anniquestore.co.za` (old webstore) stay live during transition? | `[WEBSTORE]` linked server stays; AutoPick works unchanged | AutoPick and event-exclusive-item SPs must be updated before old store retires |
| **G4** | Are SQL Agent job schedules accessible? | Exact sync frequency known — middleware can match | Middleware must design for worst-case (near real-time) |
| **G5** | Is the middleware server on the same LAN as AccountMate SQL Server? | Low-latency direct SQL connection viable | VPN tunnel or Azure Hybrid Connection required |
| **G6** | Will Namibia get a separate Shopify store? | Separate project scoped independently | Namibia uses same Shopify store — need multi-region variant logic |

---

*Document generated from source analysis of `01_stored_procedures.csv` (8.3MB), `01_tables.csv`, `02_columns.csv`, `03_foreign_keys.csv`, `04_triggers.csv`, `05_stored_procs.csv`. All claims are source-verified except where explicitly marked as inferred.*
