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
15. [Reference: Discovery Workshop Notes (WS1)](#15-reference-discovery-workshop-notes-ws1)

---

## 1. Executive Summary

Annique operates a **heavily customised AccountMate v7.x ERP** on SQL Server 2014, with NopCommerce as the current customer webstore. The two systems communicate via a **three-tier middleware stack**: (1) `nopintegration.annique.com` — a **Visual FoxPro Web Connection** application (source code obtained: `NISource/`) — woken by empty HTTP POST "doorbell" calls from the ERP; (2) `annique.com/api-backend/` — a **ASP.NET 8 REST API** (`AnqIntegrationApiSource/`) that the VFP tier calls via JWT; and (3) a **massive NopCommerce plugin** (`Annique.Plugins.Nop.Customization`, 280+ custom files) that handles storefront behaviour.

**The key migration insight:** replacing NopCommerce with Shopify means building a new middleware service that replicates everything `nopintegration.annique.com` and `annique.com/api-backend/` do — a task now fully scopeable since all three source codebases have been obtained and analysed.

### Critical Immediate Actions (pre-migration)
| Priority | Item |
|---|---|
| 🔴 URGENT | Rotate SA credentials `sa`/`AnniQu3S@` (AccountMate) and `sa`/`Difficult1` (NopCommerce) — hardcoded **SA (system administrator)** credentials found in `NISource/syncproducts.prg` |
| 🔴 URGENT | Rotate Namibia backoffice admin password — plaintext credentials found hardcoded in `sp_ws_UpdateImage` |
| ✅ RESOLVED | ~~Obtain source code / owner of `nopintegration.annique.com`~~ — Source obtained: VFP Web Connection (`NISource/`). Full logic documented in Sections 3–4. |
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
┌────────┴───────────────────────────────────────────────────────────┐
│  nopintegration.annique.com                                        │
│  Visual FoxPro Web Connection (NISource/)                          │
│  · oNopSql → SQL Server 20.87.212.38,63000 UID=sa PWD=Difficult1  │
│  · oAMSql  → SQL Server 172.19.16.100     UID=sa PWD=AnniQu3S@    │
│  · oNop    → HTTPS annique.com/api-backend/ (JWT IntegrationUser)  │
│  syncorders.prg · syncproducts.prg · syncorderstatus.prg           │
│  syncconsultant.prg · syncstaff.prg                                │
├────────────────────────────────────────────────────────────────────┤
│  shopapi.annique.com (staff sync — separate service, source TBD)   │
└────────────────────────────────────────────────────────────────────┘
         ▲ triggered by                   ▲ also calls
┌────────┴──────────────────┐   ┌─────────┴───────────────────────┐
│  ACCOUNTMATE ERP          │   │  annique.com/api-backend/        │
│  sp_NOP_* stored procs    │   │  NopCommerce Web API plugin      │
│  (fire empty POSTs on     │   │  (Product CRUD, Customer CRUD,   │
│   schedule via SQL Agent) │   │   Auth, Category, Address)       │
└───────────────────────────┘   └─────────────────────────────────┘
```

### 2.2 Key Facts at a Glance

| Item | Value |
|---|---|
| SQL Server version | SQL Server 2014 SP3 (12.0.6024.0) |
| Main database | `amanniquelive` |
| Active consultants | 10,308 (of 119,616 total in `arcust`) |
| Active products | ~800 (of 10,676 in `icitem`) |
| Current webstore | NopCommerce (separate SQL Server, `[NOP]` linked server) |
| Integration middleware | `nopintegration.annique.com` — **Visual FoxPro Web Connection app** (source now obtained: `NISource/`) |
| NopCommerce API backend | `annique.com/api-backend/` — NopCommerce Web API plugin; NISource authenticates as `IntegrationUser` |
| New middleware (C# .NET 8) | `AnqIntegrationApiSource/` — JWT-authenticated API for Brevo, WhatsApp, registration validation |
| Integration method | Empty HTTP POSTs from SQL Server OLE Automation (doorbell pattern) |
| NISource DB access | `sa` / `AnniQu3S@` (AccountMate) · `sa` / `Difficult1` (NopCommerce) — **hardcoded in VFP source** |
| Sync queue | `icWsUpdate` — 7.2M rows, 22 pending (never purged) |
| Current campaign | Feb_2026 — 20% consultant discount, 21% MLM rate, 15% VAT |
| Delivery mix | Fastway 44%, Berco 34%, Skynet 16%, Postnet 6% |
| Namibia ERP | `amanniquenam` — auto-mirrored via triggers (operational status unknown) |
| Default consultant password | `{ccustno}Anq!` (e.g., `CONSU0001Anq!`) — set by `syncconsultant.prg` |
| Staff NopCommerce password | `cIdno` (national ID number) — set by `syncstaff.prg` |
| Order sync schedule | Every **10 min** (`sp_NOP_syncOrders`) |
| Availability sync schedule | Every **5 min** (`sp_NOP_syncItemAvailability`) |
| Item change sync schedule | Every **15 min** 7am–5pm (`sp_NOP_syncItemChanges`) |
| Full item sync schedule | **3× daily** — 12:15am, 7:00am, 3:15pm (`sp_NOP_syncItemAll`) |
| Order status sync schedule | Every **20 min** 6am–5pm (NopCommerce-side `ANQ_syncOrderStatus` SP) |
| Exclusive items sync | Every **30 min** (`sp_ws_syncsoxitems`) — runs on **both** `amanniquelive` and `amanniquenam` |
| Staff sync schedule | **1st of every month** 9:00am (`sp_NOP_syncStaff`) |
| NopCommerce → AM customer sync | Every **hour** 7am–5pm (`ANQ_CustomerChanges_UPDATE` + `ANQ_CustomerAddress_UPDATE`) — **bi-directional** |
| Gift sync schedule | Every **15 min** (`ANQ_SyncGift` on NopCommerce SQL Server) |
| PayU reconciliation | Every **hour** (`ANQ_Payu_GetPendingOrderTransactionStates` on NopCommerce) |
| Consultant discount display | Flat 20% — **only visible at checkout**, not shown on product pages |
| Active sellable SKUs (DRP) | 227 (vs ~800 `icitem` active rows — DRP covers stocked/planned only) |

### 2.3 Confirmed Architecture Facts

| Fact | Source |
|---|---|
| All `sp_NOP_*` HTTP calls send **no payload** — they are empty doorbell POSTs | Full SP source verified: `sp_NOP_syncOrders`, `sp_NOP_syncItemAll`, `sp_NOP_syncItemAvailability`, `sp_NOP_syncItemChanges`, `sp_NOP_syncOrderStatus` |
| `nopintegration.annique.com` is a **Visual FoxPro Web Connection application** | `NISource/nopintegrationmain.prg` + `.fxp` Web Connection libraries — NOT .NET or Node.js |
| NISource accesses AccountMate as `sa` with `AnniQu3S@` | `syncproducts.prg`: `SERVER=172.19.16.100;UID=sa;PWD=AnniQu3S@;database=amanniquelive` (prod) |
| NISource accesses NopCommerce as `sa` with `Difficult1` | `syncproducts.prg`: `SERVER=20.87.212.38,63000;UID=sa;PWD=Difficult1;database=annique` (prod) |
| NISource also calls `annique.com/api-backend/` (NopCommerce Web API) as `IntegrationUser` | `nopapi.prg` + `syncclass.prg`: authenticates via JWT, calls `Product_Create`, `Customer_Create`, etc. |
| `AnqIntegrationApiSource` is a newer C# .NET 8 API, separate from NISource | Handles: Brevo, WhatsApp, registration validation, JWT auth for API clients (multi-tenant) |
| `wsSetting.ws.url` is only a kill-switch — all `sp_NOP_*` hardcode their URLs | Verified in all 5 NOP sync SPs |
| `sp_ws_HTTP` always POSTs — no GET path exists | Source: `EXEC @ret = sp_OAMethod @token, 'open', NULL, 'POST'` |
| Auth header is misspelled: `Authentication` not `Authorization` | Source: `sp_OAMethod @token, 'setRequestHeader', NULL, 'Authentication'` |
| Idempotency: order already synced check via `sosord.cPono = NOP OrderID` | `syncorders.prg`: `loadbase("cPono='...' AND ccustno='...'")`  — prevents double-processing |
| Inactive consultants are auto-reactivated when they place an order | `syncorders.prg`: `EXEC sp_ws_reactivate @ccustno='...'` called if `arcust.cStatus <> 'A'` |
| Customer role in NopCommerce determines which AccountMate account gets charged | `syncorders.prg`: Consultant→`ccustno`, Staff→`STAFF1`, Bounty→`ASHOP1`, Client→`ASHOP2` |
| `NOP_OfferList` and `NOP_Offers` tables exist in the **NopCommerce DB** (not amanniquelive) | `Offers.cs` / `OfferList.cs` domain objects in `Annique.Plugins.Nop.Customization` confirmed |
| `[WEBSTORE]` linked server is live production code — not dead | `sp_ws_autopickverify` and `sp_ws_gensoxitems` actively use it |
| AutoPick batches in 15-minute or 10-order windows | Source: `IF @rows=0 OR (@mins<15 AND @rows<10)` |
| `sp_ws_UpdateImage` contains plaintext Namibia backoffice admin credentials | Source: `WebLogin_txtPassword=4nnique4admin@` in POST body — **critical security issue** |
| `NopIntegration..Brevolog` is a Brevo email marketing log, NOT an API audit log | Confirmed from SP source: INSERT targeting Brevo campaign tracking |
| `NOP_OfferList` / `NOP_Offers` do NOT exist in `amanniquelive` — they ARE in the NopCommerce DB as `AnqOffer`/`AnqOfferList` | `Offers.cs` / `OfferList.cs` in `Annique.Plugins.Nop.Customization`; actively used by `SpecialOffersService` |
| Customer sync is **bi-directional** — NopCommerce profile/address changes push BACK to AccountMate | NopCommerce SQL Agent job: `ANQ_CustomerChanges_UPDATE` + `ANQ_CustomerAddress_UPDATE` every hour |
| A previously undocumented endpoint exists: `POST /SyncCancelOrders.api` on `nopintegration.annique.com` | AccountMate SQL Agent job: runs every 15min between 8am–5:59pm — cancels orders in AM when cancelled in NOP |
| `shopapi.annique.com` handles **voucher notifications** in addition to staff sync | AccountMate SQL Agent job calls `POST https://shopapi.annique.com/NotifyVouchers.api?instancing=single` every 4 hours |
| `sp_ws_syncsoxitems` exclusive item sync runs on **both** `amanniquelive` and `amanniquenam` | SQL Agent job: "Sync Customer Exclusive Items" every 30 min — explicitly listed "amanniquelive and amanniquenam" |
| `ANQ_SyncAffiliate` is called via direct SQL from AccountMate as `EXEC Nop.Annique.[dbo].[ANQ_SyncAffiliate]` | SQL Agent job: "NOP - Sync Affiliates" runs every 4 hours 6am–4pm — this is a **linked-server direct call**, not HTTP |
| Consultant 20% discount is **only visible at checkout** — not displayed on product pages | WS1 Discovery notes: "flat 20% discount, visible only at checkout" |
| November peak is approximately **2× normal order and revenue volume** | WS1 Campaign notes confirmed |
| Lost cart tracking is stored in the **Brevo database** (`LostCart_Tick` SP runs hourly on Brevo DB) | NopCommerce side SQL Agent job |

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

The service (a VFP Web Connection process) does all the work itself:

```
ERP fires:  POST /syncorders.api  ← empty body, no data

nopintegration (VFP syncorders.prg):
    1. oNopSql → connects to NopCommerce SQL Server (sa/Difficult1)
    2. EXEC ANQ_UnprocessedOrders → finds orders with:
           OrderStatusID=20, ShippingStatusID=20,
           PaymentStatusID=30 (or OrderTotal=0),
           no Shipment record, ID>30
    3. For each order:
         a. Load Order, Customer, billing + shipping Address, OrderItems, OrderNotes
         b. Determine AccountMate account from customer role:
              Annique Consultant → arcust.ccustno (Username)
              AnniqueStaff / AnniqueExco → STAFF1
              Bounty → ASHOP1 (intercompany)
              Client / default → ASHOP2 / ASHOP1
         c. Parse payment: PayU (credit card / EFT / PayFlex), COD, Manual EFT, Gift Card
         d. oAMSql → connect to AccountMate (sa/AnniQu3S@)
         e. Auto-reactivate: EXEC sp_ws_reactivate if cStatus<>'A'
         f. Idempotency: skip if sosord WHERE cPono='{OrderID}' AND ccustno='{custno}' exists
         g. Create sosord (order header) + sostrs (order lines) + soskit (kit components)
         h. Create SOPortal record
         i. oNop → POST to api-backend/ to create NopCommerce Shipment record
    4. Returns HTTP 200

ERP sees 200 → SP exits. No record of what was synced.
```

The `?type=` query parameter is the **only instruction** the ERP gives to `/syncproducts.api`. `?instancing=single` prevents concurrent runs.

### 3.1a Customer Role → AccountMate Account Mapping

| NopCommerce Role | AccountMate `ccustno` | Use Case |
|---|---|---|
| `Annique Consultant` | `arcust.ccustno` (= NopCommerce `Username`) | Standard consultant order |
| `AnniqueStaff` | `STAFF1` | Internal staff purchase |
| `AnniqueExco` | `STAFF1` | Executive committee purchase |
| `Bounty` | `ASHOP1` | Inter-company / reseller |
| `Client` | `ASHOP2` | Retail customer |
| *(default — no match)* | `ASHOP1` | Guest / unclassified |

### 3.1b Order Shipping Method Determination

| Condition | `cShipVia` / `cFrgtCode` |
|---|---|
| Staff order (STAFF1) or collect flag | `COLLECT` |
| TEST01 customer | `BERCO` |
| ShippingAddress.CustomAttributes has StorePickupPoint → `"PN"` | `POSTNET` |
| ShippingAddress.CustomAttributes has StorePickupPoint → other code | `SKYNET` |
| All other | `COURIER` (specific carrier determined by warehouse) |

### 3.1c Order Payment Method Mapping

| NopCommerce Payment System Name | AccountMate Payment Method |
|---|---|
| `Atluz.PayUSouthAfrica` | `creditcard` (card number) / `eft` (bank) / `payflex` |
| `Payments.CashOnDelivery` | `account` |
| `Payments.Manual` | `eft` |
| *(OrderTotal = 0)* | `eft` (reference: "Free") |
| Gift card also applied | `giftcard` segment in payment collection |

### 3.1d Key sosord Field Mapping (Shopify must replicate)

| `sosord` field | Value / Source |
|---|---|
| `cSono` | `PADL(NOP.CustomOrderNumber, 10)` |
| `cCustno` | AccountMate account (per role map above) |
| `cPono` | NopCommerce `Order.ID` (idempotency key) |
| `cOrderby` | NopCommerce `CustomerID` (as string) |
| `cEnterby` | `"Web Order"` (consultants) or `"Shop Order"` (retail) |
| `cPaycode` | `"CWO"` (cash with order — hardcoded) |
| `cBankno` | `"ABS423"` (hardcoded bank reference) |
| `lSource` | `4` (webshop source code — hardcoded) |
| `lHold` | `1` if order number starts with `'Z'` or customer = `TEST01` |
| `nDiscRate` | `arcust.nDiscRate` (consultant's discount rate) |
| `nSalesamt` | `Order.OrderSubtotalExclTax` |
| `nTaxAmt1` | `Order.OrderTax` |
| `nDiscAmt` | `Order.OrderDiscount` |
| `nFrtAmt` | `Order.OrderShippingExclTax` |
| `nFrtTax1` | `Order.OrderShippingInclTax - Order.OrderShippingExclTax` |
| `nXchgRate` | `Order.CurrencyRate` |
| `dOrder` | `Order.CreatedOnUtc` (converted to local date) |


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

### 3.4 SQL Agent Job Schedule (AccountMate Side) — Now Confirmed

> Source: `Accountmate - SQL Jobs for Dieselbrook.xlsx` provided by Annique IT (resolves RISK-08 / D2).

| Job Name | Schedule | Command / SP | Database |
|---|---|---|---|
| NOP - Sync Orders | **Every 10 min** | `EXEC sp_NOP_syncOrders` | `amanniquelive` |
| NOP - Sync Availability | **Every 5 min** | `EXEC sp_NOP_syncItemAvailability` | `amanniquelive` |
| NOP - Sync Cancel Orders | **Every 15 min** (8am–6pm) | `POST https://nopintegration.annique.com/SyncCancelOrders.api` | `amanniquelive` |
| NOP - Sync Item Changes | **Every 15 min** (7am–5pm) | `EXEC sp_NOP_syncItemChanges` | `amanniquelive` |
| NOP - Update Invoiced Tickets | **Every 15 min** (7am–8pm) | `EXEC sp_ws_invoicedtickets` | `amanniquelive` |
| Sync Customer Exclusive Items | **Every 30 min** | `EXEC sp_ws_syncsoxitems` | `amanniquelive` **and** `amanniquenam` |
| NOP - Send Voucher Notification | **Every 4 hours** (8am–midnight) | `POST https://shopapi.annique.com/NotifyVouchers.api` | `NopIntegration` |
| NOP - Sync Affiliates | **Every 4 hours** (6am–4pm) | `EXEC Nop.Annique.[dbo].[ANQ_SyncAffiliate]` (linked server) | `amanniquelive` |
| NOP - Full Item Sync | **3× daily** (12:15am, 7:00am, 3:15pm) | `EXEC sp_NOP_syncItemAll` | `amanniquelive` |
| NOP - Sync Staff | **1st of month** 9:00am | `EXEC sp_NOP_syncStaff` | `amanniquelive` |

**Notes:**
- `NOP - Sync Cancel Orders` calls `nopintegration.annique.com/SyncCancelOrders.api` — a **previously undocumented endpoint** not found in any ERP stored procedure; the middleware must expose this path
- `NOP - Sync Affiliates` uses a **direct linked-server SQL call** (`EXEC Nop.Annique.[dbo].[ANQ_SyncAffiliate]`), not HTTP — this will stop working when `[NOP]` linked server is removed
- `Sync Customer Exclusive Items` runs on **both** `amanniquelive` and `amanniquenam` — exclusive item grants also propagate to Namibia
- `NOP - Update Invoiced Tickets` (`sp_ws_invoicedtickets`) had not been previously documented — needs source review

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
    │       NISource: EXEC sp_ws_getactiveNEW @ldate='...' → scans all active items
    │       For each item: calls oNop.Product_Create or Product_Update (via api-backend/)
    │       After all: EXEC ANQ_SyncPublished (marks non-campaign items as published)
    │
    ├── DELTA SYNC (scheduled): sp_NOP_syncItemChanges
    │       POST /syncproducts.api?type=changes (empty)
    │       NISource: EXEC sp_ws_getactiveNEW @ldate='...' @citemno='...'
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

**Product field mapping (icitem → NopCommerce Product via sp_ws_getactiveNEW):**

| NopCommerce Product field | Source in AccountMate | Notes |
|---|---|---|
| `Sku` | `icitem.citemno` | Primary key |
| `Name` | `icitem.cdescript` | |
| `ManufacturerPartNumber` | `icitem.cuid` | Internal UID |
| `Gtin` | `icitem.cbarcode1` | Barcode |
| `Price` | `ROUND(npprice × 1.15, 2)` | Excl-tax price + 15% VAT = displayed RSP |
| `OldPrice` | `nprcinctx` | Current RSP (may differ from campaign price) |
| `Weight` | `nweight` | |
| `AvailableStartDatetimeUtc` | `dfrom` | Campaign start |
| `AvailableEndDatetimeUtc` | `dto` (set to 23:59:59) | Campaign end |
| `Published` | `cstatus='A' AND lportal=1` | Unpublished if inactive or not portal-enabled |
| `VisibleIndividually` | `lfree=0` | Free gifts hidden from browse |
| `StockStatus` (custom field) | — | Added via `AlterProductSchema.sql` |
| `HasDiscountsApplied` | `nDiscRate > 0` | |

After new product created: `EXEC ANQ_SyncImage '{citemno}'` and `EXEC ANQ_SyncProductSEO {ProductID}` run automatically.

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

> **Source obtained and fully analysed.** Built with **West Wind Web Connection** (Visual FoxPro web framework). Source: `NISource/` folder (21 `.prg` files). Triggered by ERP doorbell POSTs; executes all sync logic internally via direct SQL, then calls `annique.com/api-backend/` for NopCommerce writes.

| Endpoint | Triggered By | Schedule | Direction | What It Does | Last SP Change |
|---|---|---|---|---|---|
| `POST /syncorders.api` | `sp_NOP_syncOrders` | **Every 10 min** | NOP → AM | Reads NOP Orders, creates `arinvc`/`aritrs` in AccountMate | 2026-01-09 |
| `POST /syncproducts.api?type=all` | `sp_NOP_syncItemAll` | **3× daily** (12:15am, 7am, 3:15pm) | AM → NOP | Full `icitem` push to NOP Product table | 2024-09-03 |
| `POST /syncproducts.api?type=availability` | `sp_NOP_syncItemAvailability` | **Every 5 min** | AM → NOP | Pushes `icqoh` to NOP Product.StockQuantity | 2024-09-03 |
| `POST /syncproducts.api?type=changes` | `sp_NOP_syncItemChanges` | **Every 15 min** (7am–5pm) | AM → NOP | Reads `changes` table (cfieldname='cstatus'), delta sync | 2024-09-03 |
| `POST /syncorderstatus.api?instancing=single` | `sp_NOP_syncOrderStatus` | Every 20 min (NOP-side job) | AM → NOP | Reads `soportal` cStatus='S', updates NOP order status | 2024-10-28 |
| `POST /SyncCancelOrders.api?instancing=single` | SQL Agent job (AM) | **Every 15 min** (8am–6pm) | NOP → AM | ⚠️ **Newly discovered** — cancels orders in AccountMate when cancelled in NopCommerce | — |
| `POST /sendsms.api` | `sp_SendAdminSMS` (AM) + NopCommerce plugin | On-demand | AM → NOP / NOP → SMS | SMS gateway: used for admin alerts (AM-side) **and** password reset OTP (NopCommerce plugin setting: `passwordresetapi`) | 2025-05-07 |
| `POST /api/api/ValidateNewRegistration/` | NopCommerce registration plugin | On new consultant registration | NOP → NOPINT | Validates new consultant registration details; NopCommerce setting: `registrationvalidationapiendpoint` | — |

All calls use: `Authentication: BASIC 0123456789ABCDEF0123456789ABCDEF` (hardcoded, header name typo'd).

### 5.2 `shopapi.annique.com` — Staff/Consultant Sync + Voucher Notifications

> **Source TBD.** Separate service from nopintegration. Now confirmed to handle at least two distinct functions. Owner still unconfirmed.

| Endpoint | Triggered By | Schedule | Direction | Purpose | Last SP Change |
|---|---|---|---|---|---|
| `POST /syncstaff.api` | `sp_NOP_syncStaff` | **1st of month**, 9am | AM → ShopAPI | Sync `arcust` consultant accounts to webstore | 2023-03-20 |
| `POST /NotifyVouchers.api?instancing=single` | SQL Agent job (NopIntegration DB) | **Every 4 hours** (8am–midnight) | NOP → ShopAPI | Send voucher availability notifications to consultants | — |
| `POST /otpGenerate.api` | NopCommerce plugin (OTP setting) | On login / OTP trigger | NOP → ShopAPI | Generate and dispatch OTP codes; NopCommerce setting: `otpapiurl` | — |

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

### 5.7 `annique.com/api-backend/` — NopCommerce Web API (AnqIntegrationApiSource)

> **Source obtained.** ASP.NET 8 REST API (`AnqIntegrationApiSource/`). Serves as the write-interface to NopCommerce for NISource (VFP). Also handles new registrations, Brevo/WhatsApp comms, and product reviews. Multi-tenant: `ApiClient` table in Settings DB stores per-environment `NopDbConnection` + `AccountMateDbConnection`.

**Auth:** JWT Bearer (issued by `AuthController`) + `X-Api-Key` header. NISource authenticates as user `IntegrationUser`.

| Controller / Endpoint group | Called By | Purpose |
|---|---|---|
| `AuthController` | NISource (`SyncClass.INIT`) | Issue JWT token for `IntegrationUser` |
| Product Create/Update/Delete | NISource `nopapi.prg` | Upsert NopCommerce `Product` records |
| Product Inventory (`AdjustInventory`, `ProductWarehouse`) | NISource | Update NopCommerce stock quantities |
| Product Picture upload | NISource | Upload/replace product images |
| Category Create/Update | NISource | Manage NopCommerce categories |
| Customer Create/Update/Delete | NISource `syncconsultant.prg` | Upsert NopCommerce `Customer` from `arcust` |
| Customer Role assign/remove | NISource | Set `Annique Consultant`, `AnniqueStaff`, etc. |
| Customer SetPassword | NISource | Set initial consultant password (`{ccustno}Anq!`) |
| Address Create/Update | NISource | Billing/shipping address management |
| Shipment Create/Ship/Deliver | NISource `syncorders.prg` | Create NopCommerce `Shipment` record (marks order as processed) |
| Order Note Create | NISource | Add admin notes to NOP orders |
| `ValidateNewRegistrationController` | NopCommerce plugin (`ConsultantNewRegistrationService`) | Validate new consultant registration; find sponsor via `ANQ_LocateRefSponsor`; send Brevo notification to sponsor |
| `BrevoController` / `BrevoTestController` | Internal / admin | Direct Brevo API integration |
| `ProductReviewsController` | NopCommerce theme | Product review management |
| `UploadController` | NopCommerce admin | Image/file upload |
| `WhatsappOptInController` | NopCommerce storefront | WhatsApp opt-in flow |
| `OutboxDebugController` | Admin monitoring | View/retry Brevo outbox queue |
| `MeController` | API client | Return current authenticated user info |

**Key shared DB entities (EF Core contexts):**
- AccountMate: `Arcust`, `Arcadr`, `Arinvc`, `Aritrs`, `Icitem`, `Icikit`, `Sosord`, `Sostr`, `Soskit`, `Soship`, `Sosptr`, `Soxitem`, `Arcash`, `Arcapp`, `Aritrk`
- NopCommerce custom: `AnqNewRegistration`, `AnqExclusiveItem`, `AnqOffer`, `AnqOfferList`, `AnqEvent`, `AnqEventItem`, `AnqGift`, `AnqGiftCardAdditionalInfo`, `AnqGiftsTaken`, `AnqAward`, `AnqAwardShoppingCartItem`, `AnqBooking`, `AnqCategoryIntegration`, `AnqManufacturerIntegration`, `AnqCustomerChange`, `AnqDiscountAppliedToCustomer`, `AnqDiscountUsage`, `AnqLookup`, `AnqUserProfileAdditionalInfo`

### 5.8 NopCommerce-Side SQL Agent Jobs

These jobs run **on the NopCommerce SQL Server** (`20.87.212.38,63000`, database `Annique`), independently of the ERP. They are a critical part of the sync architecture and must be replicated when NopCommerce is replaced.

| SQL Job | Schedule | SP | Notes |
|---|---|---|---|
| Sync Order Status | Every **20 min** (6am–5pm) | `EXEC ANQ_syncOrderStatus` | Pushes shipment status from AM → NOP |
| Sync - Gifts | Every **15 min** | `EXEC ANQ_SyncGift` | Syncs gift item availability/assignment |
| Sync Affiliates | Every **15 min** | `EXEC ANQ_SyncAffiliates` | Syncs affiliate data |
| Sync - Customers to AM | Every **hour** (7am–5pm) | `EXEC ANQ_CustomerChanges_UPDATE` + `EXEC ANQ_CustomerAddress_UPDATE` | **Bi-directional** — NopCommerce profile/address changes pushed BACK to AccountMate |
| Sync - Published | Every **hour** (7am–5pm) | `EXEC ANQ_SyncPublished` | Marks products as published/unpublished |
| Brevo - Update Transaction Attributes | Every **hour** | `EXEC ANQ_Brevo_PushOrderItemTransactionalAttributes` | Pushes order transaction data to Brevo for email flows |
| Check and mark PayU paid / not processed | Every **hour** | `EXEC ANQ_Payu_GetPendingOrderTransactionStates` | PayU payment reconciliation loop |
| Lost Cart Run | Every **hour** | `EXEC LostCart_Tick` | Abandoned cart tracking — runs in `Brevo` database |
| Build Full Text | Every **4 hours** (3am–12am) | `EXEC ANQ_BUILDFullText` | Rebuilds full-text search index |

**⚠️ Critical migration implication:** These jobs running on the NopCommerce SQL Server will stop working the moment NopCommerce DB is decommissioned. The new middleware must absorb all of them — especially the bi-directional customer sync and the PayU reconciliation loop.

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
| `nopintegration.annique.com` + `annique.com/api-backend/` middleware stack | New middleware (see Section 7) — fully scopeable now source is obtained | HIGH |
| `sp_NOP_syncOrders` (pull orders from NOP) | Shopify Order webhook → middleware → AM | HIGH |
| `sp_NOP_syncItemAll/Changes/Availability` | Middleware polls `icitem`/`changes`/`icqoh` → Shopify Product/Inventory API | MEDIUM |
| `sp_NOP_syncOrderStatus` | Middleware polls `soportal` cStatus='S' → Shopify Fulfillment API | MEDIUM |
| `sp_NOP_syncSoxitems` (direct NOP DB write) | Shopify B2B catalog / custom app / customer metafields | HIGH |
| `sp_ws_UpdateImageNOP` (direct NOP DB write) | Middleware polls `iciimgUpdateNOP` → Shopify Image API | LOW |
| `sp_NOP_syncStaff` / `syncconsultant.prg` | Extend middleware to create/update Shopify customers | LOW |
| `sp_SendAdminSMS` → `nopintegration` | Re-point to new middleware `/sendsms.api` | LOW |
| `[NOP]` linked server | Decommission after middleware live | — |
| `SOPortal` as web order intake | `soportal` becomes Shopify order staging table (fed by middleware) | MEDIUM |
| NopCommerce `SpecialOffersService` (`AnqOffer`/`AnqOfferList`) | Shopify Metaobjects + Discount API | MEDIUM |
| NopCommerce `ExclusiveItemsService` (`AnqExclusiveItem`) | Shopify B2B Catalogs (Plus) or custom checkout function | HIGH |
| NopCommerce `AwardService` (`AnqAward`) — consultant reward redemption | Custom Shopify app + metafields or Shopify Loyalty integration | HIGH |
| NopCommerce `EventService` (`AnqEvent`/`AnqEventItem`) — events with exclusive item grants | Custom Shopify app | HIGH |
| NopCommerce `GiftService` + `GiftCardAdditionalInfoService` | Custom Shopify gift card handling | MEDIUM |
| NopCommerce `BookingService` (`AnqBooking`) — click-and-collect bookings | Custom Shopify app or store location feature | MEDIUM |
| NopCommerce `OtpService` — OTP for new registration | Custom Shopify flow / Brevo OTP | LOW |
| NopCommerce `DiscountCustomerMappingService` — per-consultant monthly discount | Shopify B2B Price Lists (Plus) or Discount Function | HIGH |
| NopCommerce `ConsultantNewRegistrationService` + `ValidateNewRegistration` API | Custom Shopify registration app calling AnqIntegrationApi or new equivalent | MEDIUM |
| NopCommerce `AnniqueReportService` — template-based dynamic reports | Separate admin tool / Power BI | LOW |
| NopCommerce `ChatbotService` — AI chatbot | Shopify Chat app or custom integration | LOW |
| NopCommerce `AnniqueOrderTotalCalculationService` + `OverriddenPriceCalculationService` | Shopify Checkout Extensions / Functions | HIGH |
| Annique.Plugins.Payments.AdumoOnline (SA payment gateway) | Shopify Payment App (AdumoOnline / PayU SA) — requires payment partner | HIGH |

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
│     platform TBD — see G5 / Q14 / Q15 for hosting constraints   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  WEBHOOK RECEIVERS                                        │   │
│  │  orders/paid      → create arinvc+aritrs+sosord in AM    │   │
│  │  orders/fulfilled → update soportal.cStatus              │   │
│  │  customers/create → sync to arcust                        │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  SCHEDULED POLLERS (intervals from confirmed SQL Agent schedules)│ │
│  │  Every 5 min:  icWsUpdate pending rows → Shopify Inventory API │ │
│  │  Every 5 min:  iciimgUpdateNOP pending → Shopify Image API     │ │
│  │  Every 10 min: ANQ_UnprocessedOrders → Shopify Order write-back│ │
│  │  Every 15 min: SyncCancelOrders → cancel orders in AM          │ │
│  │  Every 15 min: changes (cstatus delta) → Shopify Product       │ │
│  │  Every 20 min: soportal cStatus='S' → Shopify Fulfillment API  │ │
│  │  Every hour:   NOP customer changes → AccountMate arcust       │ │
│  │  Every hour:   PayU reconciliation loop                         │ │
│  │  Every 4 hr:   Voucher notifications → shopapi.annique.com     │ │
│  │  3× daily:     full icitem push → Shopify Product              │ │
│  │  Monthly:      campaign change → Shopify price list update     │ │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  LEGACY-COMPATIBLE API PATHS (ERP SPs unchanged)          │   │
│  │  POST /syncorders.api          POST /syncproducts.api     │   │
│  │  POST /syncorderstatus.api     POST /sendsms.api          │   │
│  │  POST /SyncCancelOrders.api    (new — not in any known SP)│   │
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

> ✅ **Source constraint resolved.** All three source codebases have been obtained and analysed (`NISource/`, `AnqIntegrationApiSource/`, `Annique.Plugins.Nop.Customization`). The new middleware can now be specified with full confidence. The detailed sync logic, field mappings, state tracking, payment parsing, and carrier routing are all documented in Sections 3–4.

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
| **Middleware** | **TBD** — .NET 8 (ASP.NET Core) or Node.js (Express) | ⚠️ Platform decision blocked on G5 / Q14 / Q15 — hosting topology must be confirmed first; direct SQL access to `172.19.16.100` is a hard requirement |
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

#### ~~RISK-13~~ · ✅ RESOLVED — `nopintegration.annique.com` source obtained and analysed

The service is **West Wind Web Connection** (Visual FoxPro web framework). Source code: `NISource/` folder, 21 `.prg` files. All internal logic is now fully documented in Sections 3–4 of this document.

**Resolved findings:**
- **Framework:** West Wind Web Connection VFP web app — NOT .NET or Node.js
- **DB access:** SA credentials hardcoded in `syncproducts.prg` (see RISK-15 below)
- **State tracking:** No internal watermark — uses `ANQ_UnprocessedOrders` SP to find unprocessed orders; uses existence of NOP `Shipment` record as order-processed flag
- **Sync trigger:** Responds to ERP doorbell POSTs; does NOT have its own scheduled jobs independent of ERP
- **NopCommerce writes:** All NopCommerce writes go via `annique.com/api-backend/` (JWT auth as `IntegrationUser`) — NISource does not write directly to NopCommerce DB for standard objects
- **Key source files:** `syncorders.prg` · `syncproducts.prg` · `syncorderstatus.prg` · `syncconsultant.prg` · `syncstaff.prg` · `nopapi.prg` · `syncclass.prg`

The new middleware spec can now be written with full confidence. New RISK (RISK-15) identified during source review.

---

#### RISK-15 · SA (`system administrator`) credentials hardcoded in NISource source

`NISource/syncproducts.prg` lines 22–24 contain hardcoded SQL Server SA credentials:

```
AccountMate (live):  SERVER=172.19.16.100;UID=sa;PWD=AnniQu3S@;database=amanniquelive
AccountMate (stage): SERVER=196.3.178.122,62111;UID=sa;PWD=AnniQu3S@;database=amanniquelive
NopCommerce (live):  SERVER=20.87.212.38,63000;UID=sa;PWD=Difficult1;database=annique
NopCommerce (stage): SERVER=20.87.212.38,63000;UID=sa;PWD=Difficult1;database=staging
NopIntegration:      SERVER=196.3.178.122,62111;UID=sa;PWD=AnniQu3S@;database=NopIntegration
```

**SA = SQL Server System Administrator — full access to create/drop databases, add logins, read all data.**

This is **more severe than RISK-14** (Namibia backoffice admin credentials). Anyone who has ever had access to the `NISource/` git repository has full SA access to both the AccountMate and NopCommerce SQL Servers.

**Mitigation (independent of migration):**
1. **Rotate both SA passwords immediately** — `AnniQu3S@` (AccountMate) and `Difficult1` (NopCommerce)
2. Create a dedicated, least-privilege SQL login for the NISource service (read-only on NopCommerce, scoped write access on AccountMate)
3. Move credentials to environment variables or Windows Credential Manager — remove from source code
4. Audit `sys.logins` for unknown logins on both servers — SA breach may have gone unnoticed

---

### 🟠 High Risk — Requires Access or Config Discovery

---

#### RISK-07 · No API audit logging — sync failures are silent

`sp_ws_HTTP` swallows non-200 responses. No failure is logged to any table. The only visible evidence is accumulating `dUpdated IS NULL` rows in `icWsUpdate` (currently 22 stuck rows from `anniquestore.co.za`). The new middleware must implement logging and alerting as a baseline requirement.

---

#### ~~RISK-08~~ · ✅ RESOLVED — SQL Agent job schedules obtained

Full schedules provided by Annique IT team (`Accountmate -SQL Jobs for Dieselbrook.xlsx` + `NOP-SQL Jobs for Dieselbrook.xlsx`). All sync frequencies are now known — see Section 2.2 Key Facts and Section 3.2a below.

**Key schedule facts for middleware design:**
- Order sync: **every 10 min** — new middleware must match or tighten this
- Stock/availability: **every 5 min** — fastest current sync
- Order cancellation: **every 15 min** (8am–6pm) — a previously undocumented flow
- Full item sync: **3× per day** — not once daily as previously assumed
- Customer changes (NOP → AM): **every hour** — customer sync is bi-directional

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
| **D1** | ~~Identify who built/owns `nopintegration.annique.com` and obtain source code~~ | ✅ RESOLVED | Source obtained (`NISource/`) — VFP Web Connection, fully documented |
| **D2** | ~~Get production SQL Server access — query `msdb.dbo.sysjobs` for SQL Agent schedule~~ | ✅ RESOLVED | Full schedules provided by Annique IT team (`Accountmate -SQL Jobs for Dieselbrook.xlsx` + `NOP-SQL Jobs for Dieselbrook.xlsx`) — see Section 3.4 |
| **D3** | Confirm whether `amanniquenam` (Namibia ERP) is actively used by Namibia staff | Annique management | 🔴 Blocker |
| **D4** | Decide: Shopify Plus (B2B) or Shopify Advanced / standard? | Business stakeholder | 🔴 Blocker |
| **D5** | Identify source / owner of `shopapi.annique.com` | Annique IT | 🟠 High — partially resolved: now known to handle voucher notifications + staff sync |
| **D6** | ~~Confirm `[WEBSTORE]` linked server presence on production; confirm if `anniquestore.co.za` is still live~~ | ✅ **PARTIALLY RESOLVED** | Staging NopCommerce server (`20.87.212.38,63000`) has `[WEBSTORE]` linked server → `stage.anniquestore.co.za,61023` (SQLNCLI). Production `.bak` contains no linked servers (instance-level objects not in backup). `anniquestore.co.za` staging endpoint confirmed live. |
| **D7** | Obtain source of `SyncCancelOrders.api` and `sp_ws_invoicedtickets` | Annique IT | 🟠 High — both discovered via SQL Agent job list; no source reviewed yet |
| **D8** | ~~Confirm `NOP - Sync Affiliates` linked-server call (`Nop.Annique.[dbo].[ANQ_SyncAffiliate]`) — what does it do?~~ | ✅ **PARTIALLY RESOLVED** | SP is `ANQ_SyncAffiliates` (plural) — confirmed in production NopCommerce DB. It syncs NopCommerce affiliate/consultant linkage data. The AccountMate SQL Agent job description named it as `ANQ_SyncAffiliate` (singular) which does NOT exist — this may indicate the AM job calls a different path or the name was documented incorrectly. **Still need to confirm the exact AM→NOP mechanism in production.** |

### 9.2 Security Actions — Do Now, Not After Migration

| # | Action | How | Urgency |
|---|---|---|---|
| **S1** | Rotate Namibia backoffice Administrator password | Change via `backofficenam.annique.com` admin panel; update `sp_ws_UpdateImage` | 🔴 Immediate |
| **S2** | **Rotate NISource SA password `AnniQu3S@`** (AccountMate live + stage) | Change SA password on both SQL Server instances; update NISource env config | 🔴 Immediate |
| **S3** | **Rotate NISource SA password `Difficult1`** (NopCommerce live + stage) | Change SA password on NopCommerce SQL Server; update NISource env config | 🔴 Immediate |
| **S4** | Create dedicated least-privilege SQL login to replace SA in NISource | Scoped to specific tables: read on NopCommerce, write on sosord/arinvc in AccountMate | 🔴 Immediate |
| **S5** | Move NISource DB credentials to environment variables — remove from source code | Use West Wind Web Connection config / env vars; never commit credentials | 🔴 Immediate |
| **S6** | Create restricted API user for `sp_ws_UpdateImage` calls | Add scoped backoffice API account; remove admin use | 🟠 High |
| **S7** | Move `sp_ws_UpdateImage` credentials out of SP source | Use SQL Server Credential object or encrypted `wsSetting` row | 🟠 High |
| **S8** | Check `arcust.cbankacct` for plaintext bank accounts | `SELECT TOP 1 cbankacct FROM arcust WHERE cbankacct IS NOT NULL` | 🟠 High |
| **S9** | Audit `sys.logins` on AccountMate and NopCommerce SQL Servers | Check for unknown logins — SA breach may have created unauthorized accounts | 🟠 High |

### 9.3 Pre-Scoping Technical Questions

| # | Question | Status | Why It Matters |
|---|---|---|---|
| **Q1** | What tables does `nopintegration.annique.com` read from `amanniquelive`? | ✅ **Answered** — `sosord`, `arcust`, `arinvc`, `soportal`, `icitem`, `iciwhs`/`icqoh`, `changes` | Middleware data access requirements defined in Sections 3–4 |
| **Q2** | How does it track sync state — watermark timestamps, row ID, or dedicated table? | ✅ **Answered** — `ANQ_UnprocessedOrders` SP identifies new orders; `Shipment` record existence = processed | Delta-sync strategy for new middleware is now known |
| **Q3** | Does it have internal scheduled jobs that fire without ERP POSTs? | ✅ **Answered** — No; triggered exclusively by ERP doorbell POSTs (`sp_NOP_*`) | No orphaned jobs to worry about post-cutover |
| **Q4** | What is the exact field mapping from `icitem` to NopCommerce `Product`? | ✅ **Answered** — via `sp_ws_getactiveNEW` SP; full mapping in Section 4.2 | Shopify Product field mapping defined |
| **Q5** | How are campaign discounts currently presented to consultants in NopCommerce? | ❓ Open | Defines Shopify customer pricing approach |
| **Q6** | How many consultants have active exclusive items at any one time? | 19,574 `soxitems` rows known; NOP `AnqExclusiveItem` count unknown | Scopes exclusive item solution complexity |
| **Q7** | Are there event registrations through the webstore that generate `soxitems`? | ❓ Open | Determines if `sp_ws_gensoxitems` / `[WEBSTORE]` replacement is needed |
| **Q8** | Does Namibia need its own Shopify store, or share SA's? | ❓ Open | Determines if separate project scope applies |
| **Q9** | What is the target go-live date? Does it require parallel running of NopCommerce + Shopify? | ❓ Open | Determines if a feature-flag cutover approach is needed |
| **Q10** | What NopCommerce features do consultants actively use? (Awards, Events, Gifts, Bookings) | ❓ Open | Determines replacement scope — each is a custom plugin feature |

---

## 10. Pre-Migration Verification Status

### ✅ Confirmed via Live DB Queries

- `wsSetting.ws.url = https://anniquestore.co.za/` — NOT nopintegration
- All `sp_NOP_*` hardcode `nopintegration.annique.com` — wsSetting change alone won't redirect them
- `sp_ws_syncItems` hardcodes `https://anniquestore.co.za/` — ignores wsSetting even in the `sp_ws_*` family
- `NOP_OfferList` / `NOP_Offers` do NOT exist in `amanniquelive` — they exist in the NopCommerce DB as `AnqOffer`/`AnqOfferList` (active in custom plugin)
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

### ✅ Confirmed via Source Code Analysis (NISource / AnqIntegrationApiSource / NopCommerce Plugin)

- `nopintegration.annique.com` is **West Wind Web Connection** (Visual FoxPro) — source: `NISource/` (21 `.prg` files)
- Full AccountMate SQL Agent job schedule confirmed (see Section 3.4): orders every 10 min, availability every 5 min, full sync 3× daily, cancel orders every 15 min, staff sync 1st of month
- Full NopCommerce-side SQL Agent job schedule confirmed (see Section 5.8): customer sync to AM hourly, gift sync every 15 min, PayU reconciliation hourly, lost cart hourly
- `shopapi.annique.com` confirmed to handle: staff sync (monthly) + voucher notifications (every 4 hours)
- `SyncCancelOrders.api` is a **previously undocumented endpoint** on `nopintegration.annique.com` — runs every 15 min via SQL Agent
- NopCommerce → AccountMate customer sync is **bi-directional**: `ANQ_CustomerChanges_UPDATE` + `ANQ_CustomerAddress_UPDATE` push NOP profile changes back to AM every hour
- Exclusive items sync (`sp_ws_syncsoxitems`) runs on **both** `amanniquelive` and `amanniquenam` — 30-min cycle
- `NOP - Sync Affiliates` uses a **direct linked-server SQL call** to NopCommerce DB, not HTTP
- Consultant 20% discount only visible at checkout — not shown on product pages
- November peak volume is ~2× normal
- DRP covers 227 active SKUs (vs ~800 `icitem` active rows) — DRP = demand-planned/stocked items only
- Lost cart tracking lives in the **Brevo database** (SP: `LostCart_Tick`)
- `annique.com/api-backend/` is **ASP.NET 8** — source: `AnqIntegrationApiSource/`; handles NopCommerce writes + Brevo/WhatsApp + new registration validation
- NopCommerce custom plugin: `Annique.Plugins.Nop.Customization` (280+ C# files, 25+ custom services)
- NISource uses `sa`/`AnniQu3S@` for AccountMate (live: `172.19.16.100`, stage: `196.3.178.122,62111`) — **hardcoded in source**
- NISource uses `sa`/`Difficult1` for NopCommerce (`20.87.212.38,63000`) — **hardcoded in source**
- NISource authenticates to `annique.com/api-backend/` as `IntegrationUser` (JWT, via `syncclass.prg`)
- Order processed state tracked via `ANQ_UnprocessedOrders` SP + existence of NOP `Shipment` record
- NISource does NOT have independent scheduled jobs — triggered only by ERP doorbell POSTs
- Order→AM field mapping fully documented: `cPono` = NOP Order ID (idempotency key), `cBankno="ABS423"` (hardcoded), `lSource=4`
- Default consultant NopCommerce password: `{ccustno}Anq!` (e.g., `ANQ001234Anq!`)
- Staff NopCommerce password: `arcust.cIdno` (national ID number)
- Carrier routing: STAFF1 → `COLLECT`; StorePickupPoint `FirstName="PN"` → `POSTNET`; others → `SKYNET`; default → `COURIER`
- Payment parse: `Atluz.PayUSouthAfrica` → PayU (credit card / EFT / PayFlex); `Payments.CashOnDelivery` → account; `Payments.Manual` → EFT
- Auto-reactivation: inactive consultants reactivated via `EXEC sp_ws_reactivate` on order creation
- AnqIntegrationApi is multi-tenant: `ApiClient` table stores per-environment DB connection strings
- All 20 ANQ_* custom NopCommerce EF entities confirmed (see Section 5.7)
- `AnqOffer`/`AnqOfferList`, `AnqEvent`/`AnqEventItem`, `AnqAward`, `AnqBooking`, `AnqGift` — all confirmed active NopCommerce features requiring Shopify equivalents
- Brevo integration: outbox pattern with retry; template IDs 431, 588, 584, 59; logs to `logs/brevo-.log` (14-day rolling)
- `ANQ_CategoryIntegration` and `ANQ_ManufacturerIntegration` tables bridge AccountMate categories to NopCommerce categories/brands
- New consultant registration validates via `ANQ_LocateRefSponsor` SP (finds sponsor by postal code); sends Brevo notification to sponsor

### ✅ Confirmed via Production DB Backup (`AnniqueNOP.bak` — restored 2026-03-06)

**Production data volumes (as of backup date):**
- Orders: **128,722** in 2024; **118,554** in 2025 YTD (March) — approximately 10,000 orders/month
- Customers: **71,097** total (non-deleted); **29,285** active in 2025
- Published products: **135** SKUs on live storefront (vs 227 DRP-planned / ~800 `icitem` active in AM)
- Exclusive items: **21,682** rows in `ANQ_ExclusiveItems` — heavily used
- New registrations pipeline: **6,332** rows in `ANQ_NewRegistrations`

**Feature liveness confirmed from production:**
- **Awards**: ✅ **LIVE** — 843 awards issued; 3 types: `FS1` R800 (290 issued), `FS2` R1,600 (310 issued), `FS3` R2,300 (243 issued); ~60-day expiry; these are consultant first-set incentive vouchers
- **Bookings**: ✅ **LIVE** — 940 booking records in `ANQ_Booking`
- **Events**: ⚠️ **MINIMAL** — only 5 events configured total; not a primary feature
- **Gift promotions** (`ANQ_Gift`): ✅ **CONFIGURED, PERIODICALLY ACTIVE** — 12 records total; 0 active at backup date; used for campaign gifts (time-limited)
- **Exclusive items** (`ANQ_ExclusiveItems`): ✅ **HEAVILY USED** — 21,682 rows
- **Chatbot**: ❌ **DISABLED** — `ischatbotenable = False`; OpenAI API key is empty — was being built, not live
- **Stripe**: ❌ **DISABLED** — `istripenable = False`; PayU only
- **Email verification**: ❌ **DISABLED** — `isemailverification = False`
- **OTP**: ✅ **LIVE** — `isotp = True`; via `shopapi.annique.com/otpGenerate.api`
- **Pickup/Click-and-collect**: ✅ **LIVE** — `ispickupcollection = True`; enabled for consultant role IDs 10, 11
- **Meta Conversions API (CAPI)**: ✅ **HEAVILY USED** — `ANQ_MetaCapiQueue` has **81,228 queued events**; Facebook server-side event tracking is a core integration

**Payment methods (production active):**
- `Atluz.PayUSouthAfrica` — PayU (credit card, EFT, PayFlex)
- `Payments.CheckMoneyOrder` — EFT / manual bank transfer
- `Payments.CashOnDelivery` — account/COD
- `Payments.Manual` — manual payment

**Shipping (production active):**
- Rate calculation: `Shipping.FixedByWeightByTotal` only (standard NopCommerce fixed-rate table)
- Click-and-collect: `Pickup.PickupInStore`
- Address validation: Fastway suburb lookup API (`https://ecommerce.fastway.co.za/api/suburbfinder/`)
- **Carrier tracking SPs are POST-DISPATCH only** — Aramex, FastWay, SkyNet, PostNet, PEP are order-tracking integrations, NOT rate calculation

**Additional confirmed endpoints (from NopCommerce settings):**
- `POST https://shopapi.annique.com/otpGenerate.api` — OTP generation
- `POST https://nopintegration.annique.com/sendsms.api` — also used for password reset SMS (not just admin alerts)
- `POST https://nopintegration.annique.com/api/api/ValidateNewRegistration/` — consultant registration validation

**Trip/competition feature confirmed:**
- Mediterranean Cruise Competition (May 14 – June 30, 2025) — qualifying spend R600 minimum; message template managed in settings

**Production NopCommerce DB — custom object counts:**
- 73 custom stored procedures (`ANQ_*` namespace)
- 40 custom tables (`ANQ_*` namespace)
- 4 custom views (`ANQ_vw_*`)
- 8 lookup categories in `ANQ_Lookups` (BANK 22, REGSTATUS 9, ETHNICITY 6, TITLE 6, CALLTIME 4, ACCOUNTTYPE 2, LANGUAGE 2, NATIONALITY 2)

**Notable SP groups discovered (not previously documented):**
- `ANQ_MetaCapi_*` (4 SPs) — Meta CAPI event queue and processing
- `ANQ_Brevo_*` (3 SPs) + `ANQ_BrevoSync` — Brevo email sync
- `ANQ_AramexTrack`, `ANQ_FastWayTrack`, `ANQ_SkyNetTrack`, `ANQ_SyncPostnet`, `ANQ_SyncPep` — 5 carrier post-dispatch tracking integrations
- `ANQ_SyncEvents` — events sync from AccountMate
- `ANQ_HTTP` — generic HTTP helper SP used internally
- `ANQ_Payu_MarkOrderAsPaidViaApi` — PayU manual reconciliation helper
- `ANQ_UpdateACL`, `ANQ_UpdateActivationDate` — ACL and consultant activation management

**Production linked servers (staging instance — not from backup, backup is DB-only):**
- `AMSERVER-V9` → `away2.annique.com,62111` (AccountMate staging) — SQLNCLI
- `WEBSTORE` → `stage.anniquestore.co.za,61023` (old webstore staging) — SQLNCLI

### 🔲 Still Unknown — Requires Further Access

- Production `wsSetting.ws.url` value
- Production linked server configuration (`[NOP]`, `[WEBSTORE]`, `[Portal]`) — linked servers are instance-level, not in DB backup; staging has `AMSERVER-V9` → `away2.annique.com,62111` and `WEBSTORE` → `stage.anniquestore.co.za,61023`
- `shopapi.annique.com` source code and full owner — *function partially known: staff sync + voucher notifications + OTP*
- Whether `amanniquenam` is actively operational on production
- Whether `arcust.cbankacct` is plaintext or encrypted
- Whether `compplanlive` is accessible with current credentials
- Whether event registrations via `anniquestore.co.za` (stage) are still in active use by real users
- Shopify Plus vs. standard plan decision
- Namibia store scope (shared vs separate)
- Source code for `SyncCancelOrders.api` and `sp_ws_invoicedtickets`
- Exact AM→NOP mechanism for `NOP - Sync Affiliates` job (SP name mismatch: job says `ANQ_SyncAffiliate` singular, NOP DB only has `ANQ_SyncAffiliates` plural)
- Which SMS provider handles OTP and password reset delivery (via `shopapi.annique.com/otpGenerate.api` and `nopintegration.annique.com/sendsms.api`)
- Whether guest checkout or PayU-without-redirect are live or still WIP
- What the Skin Care Analysis tool integrates with (custom vs third-party)
- Whether Skin Care, Donation at checkout, Spin and Win are live — no corresponding custom tables found in production DB

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

## 15. Reference: Discovery Workshop Notes (WS1)

> Source: `WS1 - Campaigns.docx` and `WS1 - Item Master.docx` — Dieselbrook Discovery Notes, February 2026.

### 15.1 Pricing Waterfall (Confirmed)

| Layer | Source | Sync | Notes |
|---|---|---|---|
| Standard Retail Price | Item Master (`icitem.npprice × 1.15`) | Daily (3× daily full sync) | Base for all products |
| Monthly Promotion Price | `backoffice.annique.com` → `socamp`/`sosppr` | ~10 min | Price override only — **no new SKU created** |
| Flash Sale Price | `backoffice.annique.com` | ~10 min | Price override; reverts to monthly when flash stock exhausted, then to standard |
| Consultant Discount (20%) | Applied at checkout | Checkout only | Flat — no tiering by consultant level. **Not shown on product pages.** |
| Starter Kit (~50% off) | Item Master (bundle SKU priced at ~50% off) | Daily | New consultants only, first order only, max 1 of each of 4 available kits |

**Priority:** Flash Sale → Monthly Promotion → Standard (consultant discount applies on top at checkout)

**⚠️ Open item:** Where does the pricing revert trigger reside when flash sale stock exhausts — in NopCommerce (stock count) or backoffice.annique.com?

### 15.2 SKU Architecture (Confirmed)

- All products are **flat SKUs** — no variants natively. Product size/format variations are separate SKUs.
- Bundles, kits, gift items, and voucher items are each their own SKU in the Item Master.
- Starter kit compositions change infrequently (~1–2× per year).
- DRP (Demand Requirements Planning) tracks **227 active SKUs** (vs ~800 active `icitem` rows — DRP covers stocked/planned only).
- When a kit is sold, stock is deducted at the **component level** (not just bundle SKU) — confirmed from DRP breakdown.
- Kit composition is defined in `icikit` (8,510 rows).
- **Shopify opportunity:** Consolidate related flat SKUs into parent/variant structure — improves browsing and SEO, but adds integration complexity.

### 15.3 Content Management Split (Proposed)

| Managed in AccountMate (Item Master) | Managed in NopCommerce (stays in Shopify) |
|---|---|
| SKU, barcode, cost price | SEO metadata (meta titles, descriptions, URL slugs) |
| Weight, dimensions | Extended marketing descriptions / copy |
| Standard retail pricing | Cross-sell / upsell relationships |
| Product images (primary) | Product tags and filters |
| Product status (active/inactive) | Collection / category assignment |
| Inventory levels | Additional lifestyle images / video content |

**⚠️ Key implication:** SEO content, extended descriptions, and cross-sells live only in NopCommerce — not synced from AccountMate. These will need a separate migration strategy (export from NopCommerce, re-import to Shopify). **Do not assume icitem covers all product data.**

### 15.4 Voucher Framework (Gaps Documented)

| Gap | Impact |
|---|---|
| Single voucher per user (no stacking) | Cannot have multiple voucher types active simultaneously |
| No user segmentation | All voucher assignment is manual — no behaviour-based targeting |
| Visibility only at login pop-up and checkout | Customers unaware of vouchers during browsing |
| No stacking | Cannot combine vouchers (e.g., free shipping + discount) |

**Current state:** Vouchers are manually created in `backoffice.annique.com` and linked to individual accounts.
**Desired state (from WS1 notes):** Behavioural targeting, stacking, site-wide visibility.

### 15.5 Fulfillment Model (Open Question)

Consultants can have 50+ shipping addresses stored in NopCommerce. This implies consultants may receive products and redistribute to their own end customers — not all orders are direct-to-consumer. **This significantly affects the Shopify shipping architecture:**
- If consultants act as sub-distributors, their shipping address list must be preserved in Shopify.
- The fulfillment model (Annique ships direct to end customer vs ships to consultant) must be confirmed.

### 15.6 Key Open Items from WS1 Not Yet Answered

| # | Item | Workstream |
|---|---|---|
| 1 | Where does Starter Kit first-order restriction logic reside? (NopCommerce / AccountMate / both) | WS1 |
| 2 | How is flash sale inventory ring-fenced from regular stock? | WS1 + WS3 |
| 3 | Where does flash sale pricing revert trigger reside? | WS1 + WS3 |
| 4 | Confirm promotion conflict priority: Flash → Monthly → Standard | WS1 |
| 5 | Any campaign processes specific to November peak not covered? | WS1 |
| 6 | Confirm approval workflow for campaigns (maker/checker or creator has full publish authority?) | WS1 |
| 7 | How are gift/voucher SKUs triggered, fulfilled, and reconciled between NopCommerce and AccountMate? | WS1 + WS3 |
| 8 | Product variants: are any used, or all separate flat SKUs? | WS1 |
| 9 | Where does web-specific product content (SEO, descriptions, cross-sells) live? | WS1 + WS2 |
| 10 | Is the fulfilment model DTC (Annique → end customer) or consultant-redistributed? | WS1 + WS3 |
| 11 | QlikView reporting: scope of campaign performance reports to be mapped | WS2 |
| 12 | Complete the Offer Mechanics Matrix — 17 potential offer types; which are currently live? | WS1 |

---

*Document generated from: `01_stored_procedures.csv` (8.3MB), `01_tables.csv`, `02_columns.csv`, `03_foreign_keys.csv`, `04_triggers.csv`, `05_stored_procs.csv` + live SQL queries against `amanniquelive` on `Away2.annique.com,62222` (2026-03-02). All facts are source-verified except where explicitly marked as inferred.*
