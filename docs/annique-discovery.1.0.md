# Annique ERP → Shopify Migration — Architecture & Discovery Reference

**Prepared:** March 2, 2026
**Last DB Verified:** Live queries against `amanniquelive` on `Away2.annique.com,62222`
**Source:** Static CSV analysis (913 tables, 968 procs, 154 triggers, 1,743 views) + live SQL verification
**Purpose:** Ground-truth document to drive Shopify migration scoping, discovery, and design

> ⚠️ **TEST SERVER NOTE:** All live queries ran against `AMSERVER-TEST` — not production. Row counts and schema are production-equivalent, but linked servers (`[NOP]`, `[WEBSTORE]`, `[Portal]`) are absent on test. Production `wsSetting.ws.url` and SQL Agent jobs are unknown.

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Current System Overview](#2-current-system-overview)
3. [Integration Architecture — How It Actually Works](#3-integration-architecture--how-it-actually-works)
4. [Data Flows (End-to-End)](#4-data-flows-end-to-end)
5. [All External Endpoints & Integrations](#5-all-external-endpoints--integrations)
6. [What Must Be Replaced vs. What Stays](#6-what-must-be-replaced-vs-what-stays)
7. [Proposed Shopify Architecture](#7-proposed-shopify-architecture)
8. [Risks & Showstoppers](#8-risks--showstoppers)
9. [Discovery Actions Required](#9-discovery-actions-required)
10. [Pre-Migration Verification Status](#10-pre-migration-verification-status)
11. [Reference: Database Snapshot](#11-reference-database-snapshot)
12. [Reference: Table & Key Data Reference](#12-reference-table--key-data-reference)
13. [Reference: Trigger Architecture](#13-reference-trigger-architecture)
14. [Reference: MLM & Campaign Systems](#14-reference-mlm--campaign-systems)

---

## 1. Executive Summary

Annique operates a **heavily customised AccountMate v7.x ERP** on SQL Server 2014, with NopCommerce as the current customer webstore. The two systems communicate via a **middleware service at `nopintegration.annique.com`** — a black box that the ERP wakes up by firing empty HTTP POSTs ("doorbell" calls). The middleware then reads the ERP database directly using its own SQL credentials and pushes changes to NopCommerce.

**The key migration insight:** replacing NopCommerce with Shopify means building a new middleware service that replicates everything `nopintegration.annique.com` does internally — a task that currently cannot be scoped because that service's source code, logic, and DB credentials are unknown.

### Critical Immediate Actions (pre-migration)
| Priority | Item |
|---|---|
| 🔴 URGENT | Rotate Namibia backoffice admin password — plaintext credentials found hardcoded in `sp_ws_UpdateImage` |
| 🔴 BLOCKER | Obtain source code / owner of `nopintegration.annique.com` — migration cannot be scoped without it |
| 🟠 HIGH | Confirm whether `amanniquenam` (Namibia ERP) is actively used — changes migration scope significantly |
| 🟠 HIGH | Get production server access to read SQL Agent job schedules |
| 🟠 HIGH | Decide on Shopify Plus (B2B) vs standard — drives exclusive items and campaign pricing approach |

---

## 2. Current System Overview

### 2.1 System Topology

```
┌─────────────────────────────────────────────────────────────────┐
│              ANNIQUE SQL SERVER (AMSERVER / AMSERVER-TEST)      │
│                                                                 │
│  Databases on same instance:                                    │
│  ┌──────────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │  amanniquelive   │  │ amanniquenam │  │  compplanlive    │  │
│  │  Main SA ERP     │  │ Namibia ERP  │  │  MLM Commission  │  │
│  │  (AccountMate)   │  │ (auto-mirror │  │  ctcomph (source │  │
│  │                  │  │  via trigger)│  │  of rebate data) │  │
│  └────────┬─────────┘  └──────────────┘  └──────────────────┘  │
│           │                                                     │
│  ┌──────────────────┐  ┌──────────────┐                        │
│  │  NopIntegration  │  │   compsys    │                        │
│  │  (Brevolog only  │  │  MAILMESSAGE │                        │
│  │  — email log)    │  │  email queue │                        │
│  └──────────────────┘  └──────────────┘                        │
└──────────────────────────────────────────────────────────────┬──┘
    Linked Server [NOP] ──────────────────────────────────────┐│
    Linked Server [WEBSTORE] ─────────────────────────────┐   ││
    Linked Server [Portal] ────────────────────────────┐  │   ││
                                                       ▼  ▼   ▼▼
┌───────────────────┐  ┌──────────────────────┐  ┌────────────────────┐
│  NopCommerce DB   │  │  anniquestore.co.za  │  │  Accountmate_      │
│ [NOP].annique.dbo │  │  (old webstore DB)   │  │  Webstore (Portal) │
│ Product, Customer │  │  registration,       │  │  consultant disc.  │
│ ANQ_ExclusiveItems│  │  EventItems,orderitem│  │  sync              │
└───────────────────┘  └──────────────────────┘  └────────────────────┘
         ▲                       ▲
         │  HTTP (doorbell POST) │  HTTP (doorbell POST + direct SQL)
         │                       │
┌────────┴───────────────────────────────────────────────────────┐
│  nopintegration.annique.com  │  shopapi.annique.com            │
│  (Black box — own SQL creds) │  (staff sync — black box)       │
│  Triggered by ERP POST calls │                                 │
│  Reads ERP DB independently  │                                 │
└────────────────────────────────────────────────────────────────┘
         ▲ triggered by
┌────────┴───────────────────────────────────────────────────────┐
│         ACCOUNTMATE ERP — sp_NOP_* stored procedures           │
│         (fire empty POSTs to nopintegration on schedule)       │
└────────────────────────────────────────────────────────────────┘
```

### 2.2 Key Facts at a Glance

| Item | Value |
|---|---|
| SQL Server version | SQL Server 2014 SP3 (12.0.6024.0) |
| Main database | `amanniquelive` |
| Active consultants | 10,308 (of 119,616 total in `arcust`) |
| Active products | ~800 (of 10,676 in `icitem`) |
| Current webstore | NopCommerce (separate SQL Server, `[NOP]` linked server) |
| Integration middleware | `nopintegration.annique.com` — **black box, source unknown** |
| Integration method | Empty HTTP POSTs from SQL Server OLE Automation |
| Sync queue | `icWsUpdate` — 7.2M rows, 22 pending (never purged) |
| Current campaign | Feb_2026 — 20% consultant discount, 21% MLM rate, 15% VAT |
| Delivery mix | Fastway 44%, Berco 34%, Skynet 16%, Postnet 6% |
| Namibia ERP | `amanniquenam` — auto-mirrored via triggers (operational status unknown) |

### 2.3 Confirmed Architecture Facts

| Fact | Source |
|---|---|
| All `sp_NOP_*` HTTP calls send **no payload** — they are empty doorbell POSTs | Full SP source verified: `sp_NOP_syncOrders`, `sp_NOP_syncItemAll`, `sp_NOP_syncItemAvailability`, `sp_NOP_syncItemChanges`, `sp_NOP_syncOrderStatus` |
| `nopintegration.annique.com` has its own SQL credentials to `amanniquelive` | Deduced from doorbell pattern — service must read DB independently |
| `wsSetting.ws.url` is only a kill-switch — all `sp_NOP_*` hardcode their URLs | Verified in all 5 NOP sync SPs |
| `sp_ws_HTTP` always POSTs — no GET path exists | Source: `EXEC @ret = sp_OAMethod @token, 'open', NULL, 'POST'` |
| Auth header is misspelled: `Authentication` not `Authorization` | Source: `sp_OAMethod @token, 'setRequestHeader', NULL, 'Authentication'` |
| `sp_ws_UpdateImage` contains plaintext Namibia backoffice admin credentials | Source: `WebLogin_txtPassword=4nnique4admin@` in POST body — **critical security issue** |
| `NopIntegration..Brevolog` is a Brevo email marketing log, NOT an API audit log | Confirmed from SP source: INSERT targeting Brevo campaign tracking |
| `NOP_OfferList` and `NOP_Offers` tables do NOT exist in the live database | Confirmed: these tables are absent from `amanniquelive` |
| `[WEBSTORE]` linked server is live production code — not dead | `sp_ws_autopickverify` and `sp_ws_gensoxitems` actively use it |
| AutoPick batches in 15-minute or 10-order windows | Source: `IF @rows=0 OR (@mins<15 AND @rows<10)` |

---

## 3. Integration Architecture — How It Actually Works

### 3.1 The "Doorbell POST" Pattern

> This is the most important fact for the Shopify middleware design.

The ERP does **not** send data to `nopintegration.annique.com`. Every sync stored procedure fires an empty POST as a wake-up signal. `sp_NOP_syncOrders` in full:

```sql
SELECT @cUrl = Value FROM wsSetting WHERE Name = 'ws.url'  -- kill-switch only
IF @cUrl IS NULL RETURN
SET @cUrl = 'https://nopintegration.annique.com/syncorders.api'  -- immediately overrides
EXEC sp_ws_HTTP @cURL = @curl, @cretvalue = @cretvalue OUTPUT, @cresponse = @cresponse OUTPUT
-- @cpostData not passed — defaults to ''  (empty string)
-- @cresponse is captured but never read or logged
```

The service does all the work itself:

```
ERP fires:  POST /syncorders.api  ← empty body, no data

nopintegration service:
    1. Uses its own SQL connection to amanniquelive (credentials unknown)
    2. Reads whatever it needs:
         /syncorders.api              → reads SOPortal / NOP Orders
         /syncproducts.api?type=all   → reads icitem
         /syncproducts.api?type=changes → reads changes table
         /syncproducts.api?type=availability → reads icqoh
         /syncorderstatus.api         → reads soportal WHERE cstatus='S'
    3. Determines what has changed using its own state tracking (unknown method)
    4. Pushes to NopCommerce
    5. Returns HTTP 200

ERP sees 200 → SP exits. No record of what was synced.
```

The `?type=` query parameter is the **only instruction** the ERP gives. `?instancing=single` prevents concurrent runs.

### 3.2 The sp_ws_HTTP Transport Layer

All ERP-side HTTP calls go through `sp_ws_HTTP` — a single wrapper using Windows OLE Automation:

```sql
SET @authHeader = 'BASIC 0123456789ABCDEF0123456789ABCDEF'  -- hardcoded, never rotated
EXEC sp_OACreate 'MSXML2.ServerXMLHTTP', @token OUT         -- requires OLE Automation enabled
EXEC sp_OAMethod @token, 'open', NULL, 'POST', @url, 'false' -- always POST, never GET
EXEC sp_OAMethod @token, 'setRequestHeader', NULL, 'Authentication', @authHeader  -- typo: should be Authorization
EXEC sp_OAMethod @token, 'send', NULL, @postData            -- empty for all sp_NOP_* calls
```

**Known issues:**

| Issue | Impact |
|---|---|
| Always POST — no GET | Any GET-only API will fail |
| `Authentication` header typo | Presumably compensated server-side |
| Hardcoded Base64 credentials | Rotation requires a code deployment |
| OLE Automation required | Not available on Azure SQL — blocks cloud migration of ERP |
| No failure logging | Non-200 responses are silently swallowed |
| No retry logic | Failed syncs accumulate as unprocessed queue rows |

### 3.3 Cross-Database Dependencies

The ERP is **not a standalone database**. Changes flow automatically across multiple databases:

| Database | On Same Server | How Reached | Purpose |
|---|---|---|---|
| `amanniquelive` | ✅ | Direct | Main SA ERP |
| `amanniquenam` | ✅ | Same-instance reference | Namibia ERP — auto-mirror via icitem/socamp triggers |
| `compplanlive` | ✅ | Same-instance reference | MLM commission history (`ctcomph`) |
| `NopIntegration` | ✅ | Same-instance reference | Brevo email log only (`Brevolog`) |
| `compsys` | ✅ | Same-instance reference | Email alert queue (`MAILMESSAGE`) |
| `[NOP].annique.dbo` | ❌ | Linked server | NopCommerce database (absent on test) |
| `[WEBSTORE].anniquestore.dbo` | ❌ | Linked server | Old webstore DB — still active code (absent on test) |
| `[Portal].Accountmate_Webstore.dbo` | ❌ | Linked server | Consultant discount sync (absent on test) |

**Namibia auto-mirror (critical):** Every field-level change to `icitem` fires `SA_NAM_ItemChanges` trigger → mirrors to `amanniquenam.dbo.icitem`. Same for campaign changes via `socamp_insupd`. **If Shopify product management bypasses the ERP trigger chain, Namibia goes out of sync silently.**

---

## 4. Data Flows (End-to-End)

### 4.1 Order Flow: Customer Places Order

```
Customer on NopCommerce storefront
    │ places order
    ▼
[NOP].annique.dbo.Orders / OrderItems
    │
    │ Scheduled: sp_NOP_syncOrders → POST /syncorders.api (empty)
    │ nopintegration service reads NOP Orders, writes to AccountMate
    ▼
arinvc (AR Invoice Header) + aritrs (AR Invoice Line Items)
    │ trigger fires
    ▼
soportal (cstatus = ' '  — pending)
    │
    │ AutoPick SP (scheduled, every N mins)
    │ Waits until: 15 min elapsed OR 10 orders queued
    ▼
soportal (cstatus = 'P' — picking)
    │ Warehouse picks and ships. soship record created.
    ▼
soportal (cstatus = 'S' — shipped)
    │
    │ Scheduled: sp_NOP_syncOrderStatus → POST /syncorderstatus.api (empty)
    │ nopintegration polls soportal cstatus='S', updates NOP order status
    ▼
Customer sees "Shipped" on NopCommerce
```

**3PL variant (Fastway — 44% of orders):**
```
sosord / soxitems → soconsign
    → fw_consignment → fw_items → fw_labels
    → sp_Fastway_AddManualConsignment → Fastway REST API
```

### 4.2 Product Catalog Sync Flow

```
icitem (Item Master — 10,700 items, ~800 active)
    │
    ├── FULL SYNC (manual): sp_NOP_syncItemAll
    │       POST /syncproducts.api?type=all&instancing=single (empty)
    │       nopintegration reads full icitem, pushes to NOP Product table
    │
    ├── DELTA SYNC (scheduled): sp_NOP_syncItemChanges
    │       POST /syncproducts.api?type=changes (empty)
    │       nopintegration reads `changes` WHERE cfieldname='cstatus'
    │
    ├── STOCK SYNC (scheduled): sp_NOP_syncItemAvailability
    │       POST /syncproducts.api?type=availability (empty)
    │       nopintegration reads icqoh → pushes NOP Product.StockQuantity
    │
    └── IMAGE SYNC: sp_ws_UpdateImageNOP
            Reads iciimgUpdateNOP queue (7,744 pending)
            Writes to [NOP].annique.dbo.ANQ_SyncImage (direct linked server — no HTTP)
```

**Trigger-driven queue (stock changes):**
```
iciwhs row changes (warehouse 4400 only)
    → iciwhs_sync trigger fires
    → INSERT INTO icWsUpdate (ctype='iciwhs', ...)  ← 7.2M rows, 22 pending
    → sp_ws_syncAvailability reads queue, POSTs to anniquestore.co.za
```

### 4.3 Consultant Exclusive Items Flow

```
soxitems INSERT or UPDATE
    → tr_soxitems_insert / tr_soxitems_update fires
    → sp_NOP_syncSoxitems

sp_NOP_syncSoxitems:
    SELECT ProductID FROM [NOP].annique.dbo.Product WHERE sku = @citemno
    SELECT CustomerID FROM [NOP].annique.dbo.Customer WHERE UserName = @ccustno
    ├── Both found + not in ANQ_ExclusiveItems → INSERT ANQ_ExclusiveItems
    ├── Both found + exists → UPDATE ANQ_ExclusiveItems
    └── Product not found (ProductID=0) → DELETE ANQ_ExclusiveItems
```

Note: Consultant username in NopCommerce = `arcust.ccustno`. This mapping must be preserved in Shopify.

### 4.4 Namibia Product Mirror

```
icitem UPDATE (any field, any source)
    → SA_NAM_ItemChanges trigger fires
    → Field-by-field comparison
    → UPDATE amanniquenam.dbo.icitem SET [changed_field] = [new_value]

socamp INSERT or UPDATE
    → socamp_insupd trigger fires
    → Mirror to amanniquenam..socamp
```

This is **invisible** and automatic. Bypassing AccountMate for product management (e.g. direct Shopify Product API) breaks Namibia silently.

### 4.5 Monthly MLM Commission Cycle

```
Month-end (manual trigger or SQL Agent job — schedule unknown):
    sp_ct_Rebates reads compplanlive..ctcomph WHERE cCompStatus <> 'P'
    │
    For each consultant (cursor):
        Base Rebate = SUM(namount) for month
        VAT = Base × 15%
        Rebate Invoice → INSERT arinvc (ctype='R', lmlm=1) + aritrs + arcapp
        Upline Credit → INSERT arinvc (ctype='R', lautoreb=1, cmlmlink=...)
        Downline Debit → INSERT arinvc (ctype='', lautoreb=1)
    │
    Mark processed: UPDATE compplanlive..ctcomph SET cCompStatus='P'

Document numbering:
    vsp_mlm_getnewdocno → arsyst.cmlmrcpt (10-char counter, transaction-safe)
```

**Every Shopify order that counts toward MLM commissions must create an `arinvc` record in AccountMate — otherwise commissions are not calculated.**

---

## 5. All External Endpoints & Integrations

### 5.1 `nopintegration.annique.com` — Primary Integration Middleware

> **Black box.** Source code unknown. Called by ERP via empty doorbell POSTs. Reads `amanniquelive` using its own SQL credentials.

| Endpoint | Triggered By | Direction | What It Does (inferred) | Last SP Change |
|---|---|---|---|---|
| `POST /syncorders.api` | `sp_NOP_syncOrders` | NOP → AM | Reads NOP Orders, creates `arinvc`/`aritrs` in AccountMate | 2026-01-09 |
| `POST /syncproducts.api?type=all` | `sp_NOP_syncItemAll` | AM → NOP | Full `icitem` push to NOP Product table | 2024-09-03 |
| `POST /syncproducts.api?type=availability` | `sp_NOP_syncItemAvailability` | AM → NOP | Pushes `icqoh` to NOP Product.StockQuantity | 2024-09-03 |
| `POST /syncproducts.api?type=changes` | `sp_NOP_syncItemChanges` | AM → NOP | Reads `changes` table (cfieldname='cstatus'), delta sync | 2024-09-03 |
| `POST /syncorderstatus.api?instancing=single` | `sp_NOP_syncOrderStatus` | AM → NOP | Reads `soportal` cStatus='S', updates NOP order status | 2024-10-28 |
| `POST /sendsms.api` | `sp_SendAdminSMS` | AM → NOP | **Only call with a body**: `{'username':'...','message:':'...'}` | 2025-05-07 |

All calls use: `Authentication: BASIC 0123456789ABCDEF0123456789ABCDEF` (hardcoded, header name typo'd).

### 5.2 `shopapi.annique.com` — Staff/Consultant Sync

> **Black box.** Separate service from nopintegration. Source code unknown.

| Endpoint | Triggered By | Direction | Purpose | Last SP Change |
|---|---|---|---|---|
| `POST /syncstaff.api` | `sp_NOP_syncStaff` | AM → ShopAPI | Sync `arcust` consultant accounts to webstore | 2023-03-20 |

### 5.3 `backofficenam.annique.com` — Namibia Backoffice ⚠️ SECURITY ISSUE

> **Newly discovered** — not previously documented.

| Endpoint | Triggered By | Direction | Purpose |
|---|---|---|---|
| `POST /jsoncallbacks.ann?method=updateimagefromam&citemno={item}` | `sp_ws_UpdateImage` | AM → NAM | Push product image update to Namibia backoffice |

**⚠️ Critical:** Authenticates by posting `WebLogin_txtUsername=Administrator&WebLogin_txtPassword=4nnique4admin@` in the **request body**. These plaintext admin credentials are readable by anyone with `VIEW DEFINITION` on the ERP database. **Rotate immediately — independent of migration.**

### 5.4 `anniquestore.co.za` — Old Webstore (sp_ws_* family)

These procedures pre-date NopCommerce and some are still active. `wsSetting.ws.url` currently points here.

| Endpoint | Triggered By | Notes |
|---|---|---|
| `POST syncavailability.sync` | `sp_ws_syncAvailability` | Uses wsSetting URL |
| `POST syncavailability.sync?citemno=all` | `sp_ws_syncAvailabilityAll` | Uses wsSetting URL |
| `POST syncimage.sync?citemno={item}` | `sp_ws_SyncImages` | Uses wsSetting URL |
| `POST syncitems.sync` | `sp_ws_syncItems` | **Hardcodes** `https://anniquestore.co.za/` — ignores wsSetting |
| `POST syncitems.sync?citemno=all` | `sp_ws_syncItemsAll` | Uses wsSetting URL |

### 5.5 Direct Database Connections (No HTTP)

| What | From | To | Purpose |
|---|---|---|---|
| Exclusive item grant | `sp_NOP_syncSoxitems` (via trigger) | `[NOP].annique.dbo.ANQ_ExclusiveItems` | Write consultant exclusive access |
| Product ID lookup | `sp_NOP_syncSoxitems` | `[NOP].annique.dbo.Product` | Find NOP product by SKU |
| Customer ID lookup | `sp_NOP_syncSoxitems` | `[NOP].annique.dbo.Customer` | Find NOP customer by username |
| Image sync call | `sp_ws_UpdateImageNOP` | `[NOP].annique.dbo.ANQ_SyncImage` | Trigger image sync in NOP |
| Order validation | `sp_ws_autopickverify` | `[WEBSTORE].anniquestore.dbo.orderitem` | Validate portal order before AutoPick |
| Event exclusive items | `sp_ws_gensoxitems` | `[WEBSTORE].anniquestore.dbo.registration` | Create soxitems from event registrations |
| Commission source | `sp_ct_Rebates` | `compplanlive..ctcomph` | Monthly MLM rebate calculation |
| Namibia product mirror | `SA_NAM_ItemChanges` trigger | `amanniquenam.dbo.icitem` | Auto-mirror all product field changes |
| Namibia campaign mirror | `socamp_insupd` trigger | `amanniquenam..socamp` | Auto-mirror campaign changes |
| Bank change alert | `customer` trigger | `compsys.MAILMESSAGE` | Email staff on bank account change |

### 5.6 Carrier APIs

| Carrier | Orders | Integration | Status |
|---|---|---|---|
| Fastway | 44% | `sp_Fastway_AddManualConsignment` → Fastway REST API | Active |
| Berco | 34% | `be_waybill` table + `somanifest` | Active (SP detail needed) |
| Skynet | 16% | `SkyTrack` table (inferred) | Active |
| Postnet | 6% | `sp_ws_UPdatePostnetStores` | Active |

---

## 6. What Must Be Replaced vs. What Stays

### 6.1 Stays in AccountMate — No Shopify Involvement

| Component | Why |
|---|---|
| All GL, AR, AP, IC, PR, PO modules | AccountMate is the financial system of record |
| `sp_ct_Rebates` + MLM commission engine | Cursor-per-consultant SQL logic — no API surface, cannot move |
| `sp_ct_downlinebuild` + downline hierarchy | Stays in SQL Server |
| `Campaign` / `CampDetail` / `sosppr` system | Source of campaign pricing (77 campaigns, 260K price rows) |
| All 154 triggers (including NAM mirror) | Unchanged — trigger chain must not be bypassed |
| `changes` table + `icWsUpdate` queue | Reused as delta-sync source for new middleware |
| AutoPick / Over_App / Return_App SPs | Stays in AccountMate — operates on `soportal` |
| `compplanlive..ctcomph` | External MLM commission DB stays |
| `fw_*` 3PL tables / carrier APIs | Stays in AccountMate |
| `compsys.MAILMESSAGE` email alerts | Stays in AccountMate |
| `amanniquenam` Namibia ERP mirror | Automatic via triggers — untouched |

### 6.2 Must Be Replaced or Re-pointed

| Current | Replacement | Complexity |
|---|---|---|
| NopCommerce storefront | Shopify storefront | — |
| NopCommerce customer accounts | Shopify customers + MLM metafields | MEDIUM |
| `nopintegration.annique.com` middleware | New middleware (see Section 7) | HIGH — source unknown |
| `sp_NOP_syncOrders` (pull orders from NOP) | Shopify Order webhook → middleware → AM | HIGH |
| `sp_NOP_syncItemAll/Changes/Availability` | Middleware polls `icitem`/`changes`/`icqoh` → Shopify Product/Inventory API | MEDIUM |
| `sp_NOP_syncOrderStatus` | Middleware polls `soportal` cStatus='S' → Shopify Fulfillment API | MEDIUM |
| `sp_NOP_syncSoxitems` (direct NOP DB write) | Shopify B2B catalog / custom app / customer metafields | HIGH |
| `sp_ws_UpdateImageNOP` (direct NOP DB write) | Middleware polls `iciimgUpdateNOP` → Shopify Image API | LOW |
| `sp_NOP_syncStaff` → `shopapi.annique.com` | Extend middleware to create/update Shopify customers | LOW |
| `sp_SendAdminSMS` → `nopintegration` | Re-point to new middleware `/sendsms.api` | LOW |
| `[NOP]` linked server | Decommission after middleware live | — |
| `SOPortal` as web order intake | `soportal` becomes Shopify order staging table (fed by middleware) | MEDIUM |
| NopCommerce campaign/offer pages | Shopify Metaobjects + theme sections | MEDIUM |
| `NOP_Discount` + missing `NOP_Offers` tables | Shopify Discount API | MEDIUM |

### 6.3 Must Be Reworked (Not Replaced)

| Component | What Changes |
|---|---|
| `sp_ws_autopickverify` | Remove or replace `[WEBSTORE]` linked server order validation — old webstore retiring |
| `sp_ws_gensoxitems` | Replace `[WEBSTORE]` event registration source with new equivalent |
| `wsSetting` table | `ws.url` should point to new middleware (only affects `sp_ws_*` family — `sp_NOP_*` ignore it) |
| `sp_ws_UpdateImage` | Remove plaintext credentials; create restricted API user; encrypt config |
| `sp_ws_HTTP` credentials | Rotate hardcoded Base64 auth string |

---

## 7. Proposed Shopify Architecture

### 7.1 Architecture Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                     SHOPIFY (Cloud)                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │   Storefront    │  │   Customers     │  │    Orders &     │ │
│  │  Products       │  │   + MLM meta    │  │    Fulfillments │ │
│  │  Campaign pages │  │   + B2B catalog │  │    + Tracking   │ │
│  └────────┬────────┘  └────────┬────────┘  └────────┬────────┘ │
└───────────┼────────────────────┼────────────────────┼───────────┘
            │   Shopify APIs / Webhooks                │
            ▼                   ▼                      ▼
┌──────────────────────────────────────────────────────────────────┐
│              NEW MIDDLEWARE (replaces nopintegration)            │
│         e.g. Node.js / .NET 8  —  VPS or Azure App Service      │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  WEBHOOK RECEIVERS                                        │   │
│  │  orders/paid      → create arinvc+aritrs+sosord in AM    │   │
│  │  orders/fulfilled → update soportal.cStatus              │   │
│  │  customers/create → sync to arcust                        │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  SCHEDULED POLLERS                                        │   │
│  │  ~5 min: icWsUpdate pending rows → Shopify Inventory API  │   │
│  │  ~5 min: iciimgUpdateNOP pending → Shopify Image API      │   │
│  │  ~5 min: soportal cStatus='S'  → Shopify Fulfillment API  │   │
│  │  ~5 min: changes table (cstatus delta) → Shopify Product  │   │
│  │  Monthly: campaign change → Shopify price list update     │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  LEGACY-COMPATIBLE API PATHS (ERP SPs unchanged)          │   │
│  │  POST /syncorders.api          POST /syncproducts.api     │   │
│  │  POST /syncorderstatus.api     POST /sendsms.api          │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  AUDIT LOG (currently missing entirely)                   │   │
│  │  Every sync request/response logged with timestamp        │   │
│  │  Alerting on repeated failures                           │   │
│  └──────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
                    │  Direct SQL connection
                    ▼
┌──────────────────────────────────────────────────────────────────┐
│          ACCOUNTMATE SQL SERVER (unchanged)                      │
│  amanniquelive · amanniquenam · compplanlive · compsys           │
└──────────────────────────────────────────────────────────────────┘
```

**Key transition enabler:** The ERP's `sp_NOP_*` procedures hardcode their endpoint paths (`/syncorders.api`, etc.). Hosting the new middleware on a domain with the same paths — and only updating the hardcoded domain in each SP — means the ERP requires minimal changes during cutover.

> ⚠️ **The Black Box Constraint:** This design assumes the new middleware can replicate what `nopintegration.annique.com` does when each endpoint is triggered. **That is not currently knowable.** Before this architecture can be built, the service's source code, SQL login, and state-tracking method must be obtained. See Section 8 (RISK-13) and Section 9.

### 7.2 Shopify Feature Decisions

| Requirement | Shopify Approach | Shopify Plan | Custom Dev |
|---|---|---|---|
| Product catalogue + search | Native | Any | None |
| Campaign discounts (20% all consultants) | **B2B Price Lists** or Discount API | Plus (B2B) or Advanced | Low |
| Per-consultant login (10K accounts) | **B2B Company accounts** or standard customers | Plus recommended | Medium |
| Exclusive SKU access per consultant | **B2B Catalogs** (Plus) or custom checkout validation app | Plus (B2B) | High if no Plus |
| Monthly campaign pages | Metaobjects + theme sections | Any | Low |
| Stock availability (from `icqoh`) | Inventory API via middleware poller | Any | Medium |
| Order → AccountMate AR invoice | Order webhook → middleware → `arinvc`/`aritrs` | Any | High |
| Fulfillment status → customer | Middleware polls `soportal` → Fulfillment API | Any | Medium |
| MLM rank / consultant data display | Customer Metafields | Any | Low |
| Image management | Product Image API via `iciimgUpdateNOP` poller | Any | Low |
| SMS notifications | Re-point `sp_SendAdminSMS` to new middleware | Any | Low |

### 7.3 Proposed Platform Stack

| Component | Recommendation | Rationale |
|---|---|---|
| **Storefront** | Shopify (Liquid or Hydrogen + Oxygen) | Hosted, PCI compliant, purpose-built |
| **Middleware** | Node.js (Express) or .NET 8 | Low-latency SQL Server access; ERP-adjacent skillset |
| **Message queue** | Azure Service Bus or Redis | Decouple Shopify webhooks from ERP writes |
| **API logging** | Seq or Azure Application Insights | Currently zero visibility on sync failures |
| **Campaign pages** | Shopify Metaobjects + theme | No separate CMS needed |
| **B2B / Exclusive access** | Shopify Plus B2B (preferred) | Solves exclusive items + consultant pricing natively |

### 7.4 Indicative Effort Estimates

> These are rough order-of-magnitude estimates only. Accurate scoping requires production access, SQL Agent jobs, Namibia status, and Shopify plan decision.

| Stream | Complexity | Estimate |
|---|---|---|
| Middleware core (webhook receiver + AM writer) | HIGH | 6–10 weeks |
| Product + inventory sync (icWsUpdate poller) | MEDIUM | 3–4 weeks |
| Order status → Shopify fulfillment sync | MEDIUM | 2–3 weeks |
| Campaign pricing on Shopify | HIGH | 4–6 weeks |
| Exclusive items (soxitems) on Shopify | HIGH | 4–8 weeks |
| Customer migration (10K active arcust) | MEDIUM | 2–3 weeks |
| Product migration (~800 active icitem) | LOW | 1–2 weeks |
| Shopify theme + campaign pages | MEDIUM | 4–6 weeks |
| Audit logging + monitoring | LOW | 1–2 weeks |
| Testing + UAT | HIGH | 4–6 weeks |
| **Total (parallel team of 3–4)** | | **~20–24 weeks** |

### 7.5 Decision Gates — Must Be Resolved Before Proposal Is Finalised

| Gate | Question | Impact if YES | Impact if NO |
|---|---|---|---|
| **G1** | Is `amanniquenam` (Namibia ERP) actively used? | All product changes must flow through AccountMate trigger chain | Can simplify product management |
| **G2** | Shopify Plus (B2B) in budget? | Exclusive items + pricing solved natively | Custom app required — adds 6–10 weeks |
| **G3** | Will `anniquestore.co.za` stay live during transition? | `[WEBSTORE]` linked server stays; AutoPick unchanged | AutoPick + event exclusive item SPs must be updated before old store retires |
| **G4** | Can production SQL Agent jobs be accessed? | Exact sync frequency known — middleware can match | Must design for near real-time (worst case) |
| **G5** | Is middleware server on same LAN as AccountMate SQL Server? | Direct SQL connection, low latency | VPN tunnel or Azure Hybrid Connection required |
| **G6** | Does Namibia need its own Shopify store? | Separate scoped project | Share SA Shopify store — needs multi-region variant logic |

---

## 8. Risks & Showstoppers

### 🔴 Showstoppers — Must Resolve Before Proposal

---

#### RISK-01 · Hardcoded URLs — no single config cutover possible

Every `sp_NOP_*` procedure hardcodes `https://nopintegration.annique.com/...` immediately after the wsSetting kill-switch check. Changing `wsSetting.ws.url` does nothing.

**Mitigation:** Deploy new middleware with compatible paths (`/syncorders.api`, etc.) and update the hardcoded domain in each of the 6 SPs. No ERP logic changes needed.

---

#### RISK-02 · OLE Automation on SQL Server — blocks Azure SQL migration

`sp_ws_HTTP` uses `sp_OACreate` (OLE Automation) to make HTTP calls from within SQL Server. This is **not available on Azure SQL**. If a future plan includes moving the ERP to Azure SQL, all in-DB HTTP calls break entirely.

**Discovery needed:** `EXEC sp_configure 'Ole Automation Procedures'` on production. Confirm if a server migration is planned.

---

#### RISK-03 · Namibia ERP auto-mirror — bypass silently breaks it

`SA_NAM_ItemChanges` (on `icitem`) and `socamp_insupd` (on `socamp`) automatically mirror changes to `amanniquenam`. Any product or campaign update that bypasses AccountMate (e.g. direct Shopify Product API without ERP write-back) will silently desync Namibia.

**Discovery needed:** Confirm if `amanniquenam` is operational on production and actively used by Namibia staff.

---

#### RISK-04 · `[WEBSTORE]` linked server is live production code — not dead

`sp_ws_autopickverify` validates portal orders against `[WEBSTORE].anniquestore.dbo.orderitem` before AutoPick runs. `sp_ws_gensoxitems` pulls event registrations from the same server to create exclusive items. If `anniquestore.co.za` is decommissioned first, **AutoPick breaks** and **event-based exclusive items stop generating**.

**Discovery needed:** Confirm if `anniquestore.co.za` is still running and the `[WEBSTORE]` linked server exists on production.

---

#### RISK-05 · MLM commission engine has no API surface — stays in SQL forever

`sp_ct_Rebates` cursor-loops over 10K consultants, reads `compplanlive..ctcomph`, posts AR invoices. This cannot move to Shopify. **Every billable Shopify order must create an `arinvc` record in AccountMate** or MLM commission calculations will be wrong.

**Discovery needed:** Confirm who triggers `sp_ct_Rebates` (SQL Agent? Manual?). Confirm `compplanlive` access with current credentials.

---

#### RISK-06 · Exclusive items (`soxitems`) have no Shopify native equivalent

19,574 rows of per-consultant exclusive product access currently flow to `[NOP].annique.dbo.ANQ_ExclusiveItems`. Shopify has no direct equivalent.

**Options:**
1. **Shopify B2B Catalogs** (requires Shopify Plus) — cleanest solution
2. **Customer Metafields** — store `exclusive_skus` JSON, enforce via checkout function (custom app)
3. **Custom Shopify App** — access control at checkout

**Discovery needed:** How many consultants have exclusive items at any time? What business event triggers a grant? Shopify Plus decision (Gate G2).

---

#### RISK-13 · `nopintegration.annique.com` is a complete black box

The ERP sends no data to this service — it reads `amanniquelive` via its own SQL connection and determines what to sync independently. From the ERP side: no source code, no admin interface, no logs, no known SQL login, no known state-tracking method.

This is the **single biggest design blocker**. Without knowing what it does internally, the new middleware spec is guesswork.

**What is needed:**
1. Who built/owns `nopintegration.annique.com`? Internal dev? Third-party vendor?
2. Source code or repository
3. What SQL login it uses and what it reads
4. How it tracks sync state (watermark timestamps? dedicated state table?)
5. Whether it has scheduled jobs that run independently of ERP POST calls

**If source is unobtainable:** The service's effects must be observed by snapshotting the NopCommerce DB before and after each endpoint trigger. Adds 2–4 weeks of reverse-engineering to discovery.

**Risk if ignored:** New middleware will miss edge cases, batch logic, and error recovery that the existing service handles silently.

---

### 🟠 High Risk — Requires Access or Config Discovery

---

#### RISK-07 · No API audit logging — sync failures are silent

`sp_ws_HTTP` swallows non-200 responses. No failure is logged to any table. The only visible evidence is accumulating `dUpdated IS NULL` rows in `icWsUpdate` (currently 22 stuck rows from `anniquestore.co.za`). The new middleware must implement logging and alerting as a baseline requirement.

---

#### RISK-08 · SQL Agent job schedule is unknown

All sync SPs are assumed to run on SQL Agent schedules — but access to `msdb.dbo.sysjobs` is denied with the current login. Sync frequency is unknown. Middleware must be designed to match it or customer-visible stock/order discrepancies will occur.

**Discovery needed:** `msdb` access via DBA or Windows credentials.

---

#### RISK-09 · `arcust.cbankacct` — potential unencrypted PII (POPIA)

The `customer` trigger fires email alerts on bank account changes — meaning bank accounts are stored in `arcust`. Whether they are encrypted at rest is unknown. POPIA requires financial personal information be protected.

**Discovery needed:** `SELECT TOP 1 cbankacct FROM arcust WHERE cbankacct IS NOT NULL` — if legible, it is plaintext. Remediation is independent of Shopify scope.

---

#### RISK-10 · AutoPick validation tied to old webstore DB

AutoPick calls `sp_ws_autopickverify` which reads `[WEBSTORE].anniquestore.dbo.orderitem`. If the old webstore is decommissioned before this SP is updated, AutoPick will fail or skip validation silently.

**Discovery needed:** Confirm `[WEBSTORE]` linked server on production. Confirm old webstore decommission timeline.

---

#### RISK-11 · Campaign pricing has no direct Shopify mapping

Monthly campaign specials work as: `socamp` → `sosppr` (260K pricing rows) → consultant 20% discount rate. The current structure (status-based consultant eligibility, campaign-period-locked prices, MLM-rate-layered) has no native Shopify equivalent. Requires B2B Price Lists or a custom pricing app.

**Discovery needed:** How should consultants experience campaign pricing on Shopify — automatic price by account, or discount code?

---

#### RISK-12 · Production data state is unknown — test server only

All analysis was done on `AMSERVER-TEST`. Production (`AMSERVER`) may have different linked server configs, `wsSetting.ws.url`, SQL Agent jobs, and campaign state.

**Discovery needed:** Production server access or a current production restore.

---

#### RISK-14 · Plaintext Namibia backoffice admin credentials — act now

`sp_ws_UpdateImage` POSTs `WebLogin_txtUsername=Administrator&WebLogin_txtPassword=4nnique4admin@` to `https://backofficenam.annique.com/`. Readable by anyone with `VIEW DEFINITION` on `amanniquelive`. **Rotate immediately — does not require Shopify migration to remediate.**

---

## 9. Discovery Actions Required

### 9.1 Critical Path — Blockers for Migration Proposal

These must be resolved before a migration can be properly scoped or priced.

| # | Action | Who | Urgency |
|---|---|---|---|
| **D1** | Identify who built/owns `nopintegration.annique.com` and obtain source code | Annique IT / CTO | 🔴 Blocker |
| **D2** | Get production SQL Server access — query `msdb.dbo.sysjobs` for SQL Agent schedule | Annique DBA | 🔴 Blocker |
| **D3** | Confirm whether `amanniquenam` (Namibia ERP) is actively used by Namibia staff | Annique management | 🔴 Blocker |
| **D4** | Decide: Shopify Plus (B2B) or Shopify Advanced / standard? | Business stakeholder | 🔴 Blocker |
| **D5** | Identify who built/owns `shopapi.annique.com` | Annique IT | 🟠 High |
| **D6** | Confirm `[WEBSTORE]` linked server presence on production; confirm if `anniquestore.co.za` is still live | Annique IT / DBA | 🟠 High |

### 9.2 Security Actions — Do Now, Not After Migration

| # | Action | How | Urgency |
|---|---|---|---|
| **S1** | Rotate Namibia backoffice Administrator password | Change via `backofficenam.annique.com` admin panel; update `sp_ws_UpdateImage` | 🔴 Immediate |
| **S2** | Create restricted API user for `sp_ws_UpdateImage` calls | Add scoped backoffice API account; remove admin use | 🟠 High |
| **S3** | Move `sp_ws_UpdateImage` credentials out of SP source | Use SQL Server Credential object or encrypted `wsSetting` row | 🟠 High |
| **S4** | Check `arcust.cbankacct` for plaintext bank accounts | `SELECT TOP 1 cbankacct FROM arcust WHERE cbankacct IS NOT NULL` | 🟠 High |
| **S5** | Audit the SQL login used by `nopintegration.annique.com` | Check `sys.syslogins`; confirm read-only vs read-write access scope | 🟠 High |

### 9.3 Pre-Scoping Technical Questions

| # | Question | Why It Matters |
|---|---|---|
| **Q1** | What tables does `nopintegration.annique.com` read from `amanniquelive`? | Defines middleware data access requirements |
| **Q2** | How does it track sync state — watermark timestamps, row ID, or dedicated table? | Defines delta-sync strategy for new middleware |
| **Q3** | Does it have internal scheduled jobs that fire without ERP POSTs? | Determines if orphaned jobs will keep running after cutover |
| **Q4** | What is the exact field mapping from `icitem` to NopCommerce `Product`? | Defines Shopify Product field mapping |
| **Q5** | How are campaign discounts currently presented to consultants in NopCommerce? | Defines Shopify customer pricing approach |
| **Q6** | How many consultants have active exclusive items at any one time? | Scopes exclusive item solution complexity |
| **Q7** | Are there event registrations through the webstore that generate `soxitems`? | Determines if `sp_ws_gensoxitems` / `[WEBSTORE]` replacement is needed |
| **Q8** | Does Namibia need its own Shopify store, or share SA's? | Determines if separate project scope applies |
| **Q9** | What is the target go-live date? Does it require parallel running of NopCommerce + Shopify? | Determines if a feature-flag cutover approach is needed |

---

## 10. Pre-Migration Verification Status

### ✅ Confirmed via Live DB Queries

- `wsSetting.ws.url = https://anniquestore.co.za/` — NOT nopintegration
- All `sp_NOP_*` hardcode `nopintegration.annique.com` — wsSetting change alone won't redirect them
- `sp_ws_syncItems` hardcodes `https://anniquestore.co.za/` — ignores wsSetting even in the `sp_ws_*` family
- `NOP_OfferList` and `NOP_Offers` tables do NOT exist in the live database
- `changes` table column structure confirmed
- `sp_ct_Rebates` reads `compplanlive..ctcomph`, creates invoices with item `SASAILM165`
- `[WEBSTORE]` linked server code is NOT dead — `sp_ws_autopickverify` + `sp_ws_gensoxitems` use it
- Bank change alert emails go to: `shona@annique.com` cc `annalien.johnson@annique.com;veronica@annique.com`
- `SA_NAM_ItemChanges` and `socamp_insupd` triggers confirmed — Namibia mirror is real
- `NopIntegration..Brevolog` is Brevo email log, not API audit log
- All major row counts verified via `sys.partitions` (no full table scans)
- 10,308 active consultants (A), 109,308 inactive (I)
- Delivery split: Fastway 44%, Berco 34%, Skynet 16%, Postnet 6%
- Current campaign: Feb_2026 — 20% discount, 21% MLM rate
- `sp_ws_UpdateImage` contains plaintext Namibia backoffice admin credentials
- `backofficenam.annique.com` endpoint confirmed — previously undocumented
- All `sp_NOP_*` calls confirmed as empty doorbell POSTs (no data payload)
- `sp_ws_HTTP` source confirmed — always POST, no GET, header name typo, OLE Automation

### 🔲 Still Unknown — Requires Further Access

- SQL Agent job definitions and sync schedules (`msdb` access denied)
- Production `wsSetting.ws.url` value
- Production linked server configuration (`[NOP]`, `[WEBSTORE]`, `[Portal]`)
- `nopintegration.annique.com` source code, SQL login, and internal state-tracking logic
- `shopapi.annique.com` source code and owner
- Whether `amanniquenam` is actively operational on production
- Whether `arcust.cbankacct` is plaintext or encrypted
- Whether `compplanlive` is accessible with current credentials
- What `[AMSERVER-V9]` linked server hosts
- Whether event registrations via `anniquestore.co.za` are still in use
- Shopify Plus vs. standard plan decision
- Namibia store scope (shared vs separate)

---

## 11. Reference: Database Snapshot

### Environment (Test Server — Verified 2026-03-02)

| Item | Value |
|---|---|
| Server name | `AMSERVER-TEST` (staging — NOT production) |
| SQL Server version | SQL Server 2014 SP3 (12.0.6024.0) |
| Connection | `Away2.annique.com,62222` |
| Database | `amanniquelive` |
| Server time at query | `2026-03-02 17:53:37` |

### Live Row Counts (via `sys.partitions`)

| Table | Live Rows | Notes |
|---|---|---|
| `aritrsh` | **21,607,444** | All-time invoice line history |
| `icWsUpdate` | **7,227,023** | Sync queue — 22 pending (never purged) |
| `mlmcust` | **2,999,503** | MLM commission aggregation |
| `sosordh` | **1,624,995** | Historical order archive |
| `aritrs` | **769,633** | Current invoice line items |
| `fw_items` | **681,759** | Fastway parcel items |
| `fw_consignment` | **674,738** | Fastway consignments |
| `SOPortal` | **646,237** | Portal orders (cStatus blank on test) |
| `changes` | **531,698** | Consultant change audit (cstatus 167K, csponsor 147K, ndiscrate 53K) |
| `arinvc` | **275,883** | Invoice headers |
| `sosppr` | **260,147** | Campaign product pricing (all campaigns) |
| `arcust` | **119,616** | Consultants: 10,308 Active, 109,308 Inactive |
| `iciwhs` | **101,970** | Warehouse inventory |
| `sosord` | **56,063** | Current sales orders |
| `MLMCurr` | **49,788** | Current downline hierarchy |
| `iciimgUpdate` | **21,091** | Image sync queue (anniquestore.co.za) |
| `soxitems` | **19,574** | Exclusive items (2 pending, 19,572 processed) |
| `CampDetail` | **18,371** | Campaign product detail lines |
| `MLMHist` | **11,466** | Historical downline snapshots |
| `icitem` | **10,676** | Products: ~800 Active |
| `icikit` | **8,510** | Kit component definitions |
| `iciimgUpdateNOP` | **7,744** | NOP image sync queue |
| `Campaign` | **77** | Campaign records |
| `NOP_Discount` | **2** | NOP discount rules |

### wsSetting Table (Live)

| Name | Value |
|---|---|
| `ws.url` | `https://anniquestore.co.za/` (old webstore — NOT nopintegration) |
| `ws.warehouse` | `4400` |

### Current Campaign (at query time)

| Field | Value |
|---|---|
| Campaign | `Feb_2026` (ID: 90) |
| Period | 2026-02-02 → 2026-03-01 |
| Status | `C` (Current) |
| Consultant discount | 20% |
| MLM upline rate | 21% |
| VAT rate | 15% |
| Next campaign | `Mar_2026` (status `F` = Future) |

---

## 12. Reference: Table & Key Data Reference

### Core Transaction Tables

| Table | Live Rows | Purpose | Shopify Mapping |
|---|---|---|---|
| `arcust` | 119,616 | Consultant/Customer master | → Shopify Customer + metafields |
| `arinvc` | 275,883 | Invoice headers | → Shopify Order (must be created per Shopify order) |
| `aritrs` | 769,633 | Invoice line items | → Shopify Order.LineItems |
| `aritrsh` | 21,607,444 | Invoice history archive | Historical reference only |
| `icitem` | 10,676 | Product master (~800 active) | → Shopify Product |
| `iciwhs` | 101,970 | Warehouse inventory (04400) | → Shopify Inventory |
| `icqoh` | — | Quantity on hand | → Shopify Inventory.available |
| `sosord` | 56,063 | Sales order headers | AM fulfillment staging |
| `SOPortal` | 646,237 | Web portal orders | Replaced by Shopify webhook → middleware → soportal |
| `sosppr` | 260,147 | Campaign product pricing | → Shopify B2B Price Lists / metafields |
| `soxitems` | 19,574 | Per-consultant exclusive items | → Shopify B2B catalog / custom app |
| `soship` | — | Shipments | → Shopify Fulfillment |
| `changes` | 531,698 | Change audit log | Reuse as delta-sync source |
| `wsSetting` | 2 rows | API config | Update `ws.url` to new middleware |
| `icWsUpdate` | 7,227,023 | Sync queue (22 pending) | Reuse — middleware polls this |
| `iciimgUpdateNOP` | 7,744 | NOP image sync queue | Reuse — middleware polls this |
| `NOP_Discount` | 2 | Discount rules | → Shopify Discount API |
| `Campaign` | 77 | Campaign records | Campaign sync to Shopify pricing |
| `CampDetail` | 18,371 | Campaign product detail | Campaign product data |
| `fw_consignment` | 674,738 | Fastway consignments | Stays in AM |
| `fw_items` | 681,759 | Fastway parcel items | Stays in AM |

### MLM Tables (Stay in AccountMate)

| Table | Rows | Purpose |
|---|---|---|
| `mlmcust` | 2,999,503 | Consultant MLM aggregation |
| `MLMCurr` | 49,788 | Current downline hierarchy |
| `MLMHist` | 11,466 | Historical hierarchy snapshots |
| `rbtmlm` | — | Rebate tier master |
| `rbtmlmMONTHLY` | — | Monthly rebate aggregation |
| `rebatedtl` | 44,300 | Rebate detail lines |
| `CustSPNLVL` | 145,000 | Customer-sponsor-level mapping |

### icWsUpdate Queue Breakdown

| cType | Total Rows | Pending | Source |
|---|---|---|---|
| `iciwhs` | 7,214,520 | **22** | `iciwhs_sync` trigger (warehouse 4400 only) |
| `campdetail` | 6,958 | 0 | Campaign pricing changes |
| `icitem` | 5,350 | 0 | `itemsync` trigger |
| `campsku` | 195 | 0 | Campaign SKU changes |

The 22 pending `iciwhs` rows represent failed syncs to `anniquestore.co.za` (HTTP non-200). The queue never purges — 7.2M rows accumulated over its lifetime.

### AR Balance Formula

```
arinvc.nbalance =
    (nfsalesamt - nfdiscamt)                           -- net sales after discount
    + nffrtamt + nffinamt                              -- freight + finance charges
    + nftaxamt1 + nftaxamt2 + nftaxamt3               -- up to 3 tax types (VAT etc.)
    + nfadjamt                                         -- adjustments
    - (nftotpaid + nftotdisc + nftotadj + nftotdebt)  -- all payments applied

arcust.nbalance = SUM(arinvc.nbalance) for all open invoices
```

---

## 13. Reference: Trigger Architecture

### Integration-Relevant Triggers

| Trigger | Table | What It Does | Migration Impact |
|---|---|---|---|
| `SA_NAM_ItemChanges` | `icitem` | Mirrors every field change to `amanniquenam.dbo.icitem` | **CRITICAL** — bypass breaks Namibia silently |
| `socamp_insupd` | `socamp` | Mirrors campaign changes to `amanniquenam..socamp` | **CRITICAL** — same risk |
| `itemsync` | `icitem` | Product changes → `icWsUpdate` ('icitem' type) | Queue reused by new middleware |
| `iciwhs_sync` | `iciwhs` | Stock changes (warehouse 4400) → `icWsUpdate` ('iciwhs' type) | Queue reused by new middleware |
| `imageUpdate` | `iciimg` | Image changes → `iciimgUpdate` AND `iciimgUpdateNOP` | Queues reused by new middleware |
| `sosppr_insupd_sync` | `sosppr` | Campaign price changes → `icWsUpdate` ('sosppr' type) | Queue reused by new middleware |
| `tr_soxitems_insert/update` | `soxitems` | Calls `sp_NOP_syncSoxitems` → writes to `[NOP]` directly | Trigger chain must be updated for Shopify |
| `customer` | `arcust` | Bank change → `compsys.MAILMESSAGE` email alert | No change needed |
| `customeradr` / `arcadr_upd` | `arcadr` | Address change audit | No change needed |
| `arinvc_insert/update` | `arinvc` | Real-time AR balance maintenance | Must fire on every Shopify order write |

### Full Trigger Categories (154 triggers total)

| Domain | Count | Key Triggers |
|---|---|---|
| AR Balance | ~12 | `arcash_insert/update`, `arcapp_ins_upd/insert`, `arinvc_insert/update` |
| GL Posting | ~8 | `apdist_insert_update`, `apdist_delete/insert/audit_trail` |
| AP Payments | ~6 | `apcapp_delete`, `apvchk_insert`, `apvend_update/delete` |
| Item Master | ~8 | `itemsync`, `SA_NAM_ItemChanges`, `icitem_upd/update`, `newitem` |
| Infotrac Audit | ~10 | `trig_it_log_arcust_*`, `trig_it_log_icitem_*`, `trig_it_arinvc`, `trig_it_aritrs` |
| Inventory | ~4 | `iciwhs_sync`, `iciwhs_checkstockalert`, `wsLowStock_checkstockalert` |
| Sales Orders | ~3 | `sosord_insert/update/delete` |
| Campaign | ~5 | `socamp_insupd`, `sosppr_insupd_sync`, `socmpf_insupd`, `socmpq_insupd` |
| Customer | ~4 | `customer`, `customeradr`, `arcadr_upd` |
| Images | ~2 | `imageUpdate`, `iciimgUpdateNOP` feeds |
| PO / Receiving | ~8 | `popord_insert/update`, `porecg_insert`, `poctrs_insert` |
| Exclusive Items | ~2 | `update_webstore`, `tr_soxitems_insert/update` |
| Other | ~82 | Budget, costing, history stamping, misc |

---

## 14. Reference: MLM & Campaign Systems

### Campaign Lifecycle

```
Campaign.cStatus:
    D (Draft) → P (Pending) → C (Current) → H (Historical)

Camp_NewMonth (runs at month end):
    1. Find cStatus='C' AND dto=today → SET cStatus='H'
    2. Find next campaign (dfrom > @dto) → SET cStatus='C'
    3. EXEC sp_Camp_SynctoNam  (sync to Namibia system)

Structure:
    Campaign (77 records)
        └── CampCat (451 categories)
                └── CampSku (14,200 SKUs)
                        ├── CampDetail (18,400 lines) [CASCADE DELETE]
                        ├── CampKit (2,100) [CASCADE DELETE]
                        └── CampComp (4,600) [CASCADE DELETE]

CampChanges (892K rows) = full audit trail of all campaign modifications
```

### Monthly MLM Commission Cycle

```
Input:  compplanlive..ctcomph WHERE cCompStatus <> 'P'

sp_ct_Rebates (cursor per consultant):
    Base = SUM(namount) for month
    VAT = Base × 15%
    Creates:
        arinvc (ctype='R', lmlm=1)         — rebate invoice
        aritrs (citemno='SASAILM165')       — line item
        arcapp (npaidamt = -(amount+VAT))   — payment applied
        arinvc (ctype='R', lautoreb=1)      — upline credit
        arinvc (ctype='',  lautoreb=1)      — downline debit

Marks: UPDATE compplanlive..ctcomph SET cCompStatus='P'

Document numbering via arsyst counters (transaction-safe):
    arsyst.cmlmrcpt   → vsp_mlm_getnewdocno
    arsyst.cmlmcinvno → vsp_mlm_getnewmlminvc
```

### Consultant Hierarchy

```
arcust.csponsor = upline FK (self-referencing)

sp_ct_downlinebuild (runs monthly):
    TRUNCATE CTDownline
    INSERT FROM fn_get_downlineCT (recursive CTE)
        Fields: ccustno, ilevel, csponsor, gen, iTitle, cstatus
    If last day of month: INSERT CTDownlineh (archive)

Key related tables:
    mlmcust (2.9M)         — consultant sales aggregation
    rbtmlm / rbtmlmMONTHLY — rebate tier calculation
    rebatedtl / rebatedtlMONTHLY — rebate detail
    CustSPNLVL (145K)      — customer-sponsor-level mapping
    Level1 / Level2        — tier membership
    MoDownlinerV1 (1M)     — monthly downline snapshots
```

---

*Document generated from: `01_stored_procedures.csv` (8.3MB), `01_tables.csv`, `02_columns.csv`, `03_foreign_keys.csv`, `04_triggers.csv`, `05_stored_procs.csv` + live SQL queries against `amanniquelive` on `Away2.annique.com,62222` (2026-03-02). All facts are source-verified except where explicitly marked as inferred.*
