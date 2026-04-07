# 02 — Technical Architecture
## Systems, Stacks, Infrastructure, and Data Landscape

**Last updated:** 2026-04-07

---

## 1. System Map

```
┌─────────────────────────────────────────────────────────────────────┐
│                         AZURE CLOUD TIER                            │
│                                                                     │
│  ┌─────────────────┐   ┌──────────────────┐   ┌─────────────────┐  │
│  │  Shopify Plus   │   │  AnniqueAPI       │   │  DBM (target)   │  │
│  │  (new, hosted   │   │  (AnqIntegration  │   │  Dieselbrook    │  │
│  │   by Shopify)   │   │  ApiSource)       │   │  Middleware     │  │
│  │                 │   │  .NET 9 / JWT /   │   │  .NET 9 / EF    │  │
│  │  Shopify CDN    │   │  EF Core / SQL    │   │  Core / SQL     │  │
│  └────────┬────────┘   └────────┬──────────┘   └───────┬─────────┘  │
│           │                    │                        │            │
│           └────────────────────┴────────────────────────┘            │
│                                │                                     │
│                    Azure Virtual Network                             │
│                    (existing private routing)                        │
└────────────────────────────────┼────────────────────────────────────┘
                                 │  Private route (VPN/ExpressRoute)
┌────────────────────────────────┼────────────────────────────────────┐
│                         ON-PREMISES TIER                            │
│                                │                                     │
│  ┌─────────────────────────────┴──────────────────────────────────┐  │
│  │                    AMSERVER-v9 (SQL Server)                    │  │
│  │                                                                 │  │
│  │  amanniquelive  │  compplanLive  │  compsys  │  NopIntegration │  │
│  │  amanniquenam   │  compplanNam   │  Annique_ │  BackOffice(Nam)│  │
│  │                                   Reports                      │  │
│  └─────────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  ┌──────────────────┐   ┌─────────────────────────────────────────┐  │
│  │  AccountMate     │   │  NopIntegration web app                 │  │
│  │  Application     │   │  (shopapi.annique.com / NISource VFP)   │  │
│  │  (ERP client)    │   │  VFP Web Connection server              │  │
│  └──────────────────┘   └─────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. System Profiles

### 2.1 AccountMate (AM) — ERP

| Property | Value |
|---|---|
| Type | Proprietary ERP system (Larnett Corp) |
| Database engine | SQL Server (on-prem, AMSERVER-v9) |
| Access method | Standard SQL Server — tables + stored procedures |
| ODBC driver (in VFP source) | `DRIVER={SQL Server}` |
| Primary database | `amanniquelive` |
| Linked databases | `compplanLive`, `compsys`, `NopIntegration`, `Annique_Reports`, `BackOffice` |
| Named instance | `AMSERVER-v9` (confirmed from `exec [AMSERVER-v9].amanniquelive.dbo.sp_WS_rpt_invoice`) |
| Live endpoint | `172.19.16.100:1433` (private LAN) |
| Staging endpoint | `196.3.178.122:62111` (external, non-standard port) |
| Credentials (live/staging) | `sa` / `AnniQu3S@` (HARDCODED IN LEGACY SOURCE — must be replaced in DBM) |
| SQL Server version (installer present) | SQL Server 2025 Express (`database/sqlmedia/Express_ENU/`, v17.0.1000.7) |
| Modification constraint | **None.** AccountMate must not be modified. |

### 2.2 NopCommerce (legacy storefront — being replaced)

| Property | Value |
|---|---|
| Type | Open-source ASP.NET e-commerce platform |
| Source | `NopCommerce - Annique/` folder in workspace |
| Version | Annique-customised NopCommerce fork |
| Custom plugin | `Annique.Plugins.Nop.Customization` |
| Payment plugin | `Annique.Plugins.Payments.AdumoOnline` (PayU via Adumo) |
| Database | NopCommerce SQL Server DB (`AnniqueNOP.bak`, 7.5 GB, backed up 06/03/2026) |
| NopCommerce Azure VM | `20.87.212.38:63000` |
| Status | Being replaced by Shopify Plus. Read-only reference. |

### 2.3 AnniqueAPI — Existing .NET 9 Integration API

| Property | Value |
|---|---|
| Source | `AnqIntegrationApiSource/` |
| Project | `AnqIntegrationApi.csproj` — `net9.0` |
| Solution | `AnqIntegrationApi.sln` |
| Framework | ASP.NET Core 9 / EF Core 9.0.7 / SQL Server |
| Auth | JWT (see `JwtService.cs`, `AuthController.cs`) |
| Logging | Serilog |
| DB connections | NopDb, BrevoDb, AccountMateDb, Settings, Outbox |
| Key functions | Brevo CRM sync, WhatsApp opt-in, product reviews, sync controller, JWT test |
| Deployment | Azure VM (same VNet as NopCommerce) |
| Role in programme | **Architectural pattern reference for DBM.** DBM should follow these patterns. |
| Subproject | `BrevoApiHelpers/` — Brevo CRM/email/WhatsApp helper library |

**AnniqueAPI is NOT DBM.** It is a separate service that continues to run alongside DBM. DBM is new build.

### 2.4 NISource — Legacy VFP Middleware (being replaced)

| Property | Value |
|---|---|
| Location | `NISource/` |
| Language | Visual FoxPro (VFP 9) via `wconnect` (West Wind Web Connection) |
| Entry point | `nopintegrationmain.prg` |
| Key source files | `syncproducts.prg`, `syncorders.prg`, `syncconsultant.prg`, `syncorderstatus.prg`, `apiprocess.prg`, `appprocess.prg`, `brevoprocess.prg`, `ssoclass.prg`, `amdata.prg` |
| AM access | Via `wwSQL` class — `oSql.ExecuteStoredProcedure()` calls |
| Connection pattern | Hardcoded connection strings in `.prg` files (IPs + `sa` credentials) |
| External URL | `shopapi.annique.com` (TC-04 — ownership/source still unconfirmed) |
| Campaign module | `NISource/campaign/` (4 files — partial, not runnable standalone) |
| Status | **Legacy being replaced by DBM.** Read source for behaviour reference only. |

### 2.5 Shopify Plus (target storefront — being built by Dieselbrook)

| Property | Value |
|---|---|
| Platform | Shopify Plus (enterprise Shopify tier) |
| Required for | Shopify Functions (custom checkout pricing logic) |
| Custom app | To be built by architect — webhooks + Shopify Functions |
| Payment | PayU via Adumo (Shopify Plus natively supports custom payment gateways) |
| Consultant pricing | Shopify Function applies fixed discount at checkout using middleware-synced metafields |
| Status | Not yet provisioned (Dieselbrook's task) |

### 2.6 DBM — Dieselbrook Middleware (new build — to be built by architect)

| Property | Value |
|---|---|
| Target stack | .NET 9 / ASP.NET Core / EF Core 9.0.7 / SQL Server |
| Pattern reference | `AnqIntegrationApiSource/` |
| Deployment | Azure (same VNet as existing AnniqueAPI) |
| Auth | JWT (same pattern as AnniqueAPI) |
| Admin console | Node.js web app (separate project) |
| Status | **Not yet started. Starting with PRD → Architecture → Build.** |

---

## 3. Database Inventory

### 3.1 amanniquelive (primary AM database)

Core AccountMate tables confirmed in EF Core mapping (`AccountMateDbContext.cs`):

| Table | Domain | Purpose |
|---|---|---|
| `Arcust` | Consultants/Customers | Customer/consultant master (cCustNo = key) |
| `Arcadr` | Addresses | Delivery addresses |
| `Arcapp` | Accounts payable | - |
| `Arcash` | Cashbook | Cash receipts |
| `Arinvc` | Invoices | AR invoice header (downstream of order) |
| `Aritrk` | Invoice tracking | - |
| `Icitem` | Products | Item master (SKU, description, price `nprice`) |
| `Icikit` | Kits | Product kit definitions |
| `Sosord` | Orders | Sales order header (cPono = NopCommerce OrderID) |
| `Soxitem` | Order line items | Order line items |
| `Sostr` / `Sostrs` | Order transactions | Order status/tracking detail |
| `Soskit` | Kit items in orders | Kit line items in orders |
| `Sosptr` | Order pointers | Order pointer/reference |
| `Soship` | Shipments | Shipment records |

Pricing tables (from stored procedure archaeology):
- `Campaign` — Campaign master (77 rows on staging)
- `CampDetail` — Campaign line items with `nPrice` and `dLastUpdate` (18571 rows on staging)
- `CampSku` — Campaign SKU mapping
- `wsSetting` — Integration settings (2 rows on staging)

### 3.2 compplanLive (MLM/commissions database)

| Table | Row count (staging) | Purpose |
|---|---|---|
| `CTcomph` | 8,623,539 | Commission period headers |
| `CTcomp` | 11,574 | Commission computation records |
| `CTconsv` | 1,779,211 | Consultant view/summary |
| `CTRunData` | 3,702,636 | Commission run data |
| `CTstatement` | 52,511 | Statement records |
| `CTstatementh` | 4,481,557 | Statement history |
| `deactivations` | 35,702 | Consultant deactivations |
| `CTdownline` | 0 | Active downline (empty on staging) |
| `CTdownlineh` | 1,491,507 | Downline history |

Key function: `CompPlanLive.dbo.fn_Get_DownlineHist` (referenced in `apiprocess.prg`)

### 3.3 NopIntegration (integration support database)

| Object | Row count (staging) | Purpose |
|---|---|---|
| `BrevoLog` | 124,726 | Brevo send log |
| `NopReports` | 52 | Report definitions |
| `ApiClients` | 8 | API client registry / JWT issuance |
| `NopSettings` | 19 | Runtime integration settings |
| `NopSSO` | 10,260 | SSO session tokens |
| `wwrequestlog` | 1,640,737 | VFP web request log (legacy) |

### 3.4 compsys (system support)

| Object | Purpose |
|---|---|
| `MAILMESSAGE` | Outbound email queue (752,963 rows on staging) |
| `sp_Sendmail` | Email dispatch stored procedure |

### 3.5 NopCommerce database (AnniqueNOP.bak)
- 7.5 GB backup, dated 06/03/2026
- Located at `database/AnniqueNOP.bak`
- Tables mapped in `AnqIntegrationApiSource/DbContexts/NopDbContext.cs`
- Contains Annique-custom tables: `AnqAward`, `AnqBooking`, `AnqEvent`, `AnqGift`, `AnqCategoryIntegration`, `AnqManufacturerIntegration`, `AnqOffer`, `AnqOfferList`, `AnqNewRegistration`, `AnqCustomerChange`, `AnqDiscountAppliedToCustomer`, `AnqExclusiveItem`, `AnqDiscountUsage`, `AnqLookup`

---

## 4. API Endpoints (confirmed)

### 4.1 shopapi.annique.com (NISource/VFP — production)

From SQL Agent job schedules (confirmed in production):

| Endpoint | Schedule | Purpose | Risk |
|---|---|---|---|
| `POST /syncstaff.api` | 1st of month, 09:00 | Sync consultant records from `arcust` | Medium |
| `POST /NotifyVouchers.api?instancing=single` | Every 4h, 08:00–midnight | Voucher notifications | Low |
| `POST /otpGenerate.api` | On-demand | **OTP/login gate for live consultants** | **HIGH** |

TC-04 is open: the full source code and ownership of `shopapi.annique.com` are unconfirmed. It may be the `NISource` VFP server, or it may be a separate service.

### 4.2 AnniqueAPI endpoints (AnqIntegrationApiSource)

| Controller | Key routes | Purpose |
|---|---|---|
| `AuthController` | `POST /auth/token` | JWT issuance |
| `SyncController` | `POST /sync/*` | Trigger sync operations |
| `BrevoController` | `POST /brevo/*` | Brevo CRM operations |
| `WhatsappOptInController` | `POST /whatsapp-optin` | WhatsApp marketing opt-in |
| `ProductReviewsController` | `GET/POST /product-reviews` | NopCommerce review sync |
| `MeController` | `GET /me` | Current user info |
| `HealthController` | `GET /health` | Health check |
| `OutboxDebugController` | `GET /outbox-debug` | Outbox inspection |
| `UploadController` | `POST /upload` | File upload |

---

## 5. Infrastructure Topology

### Confirmed topology (from Annique-supplied diagram, 2026-03-11)

```
Azure Cloud Tier:
├── NopCommerce application
├── NopCommerce SQL Server (20.87.212.38:63000)
├── AnniqueAPI (AnqIntegrationApiSource)
└── DBM (TARGET — deploys here in same VNet)

On-Premises Tier:                  
├── AMSERVER-v9 (SQL Server)
│   ├── amanniquelive
│   ├── compplanLive
│   ├── compsys
│   ├── NopIntegration
│   ├── amanniquenam
│   ├── compplanNam
│   ├── Annique_Reports
│   ├── BackOffice
│   └── BackOfficeNam
├── AccountMate application
├── Data warehouse
└── NISource (VFP Web Connection — shopapi.annique.com)
```

Private routing exists between Azure VNet and on-prem AMSERVER-v9. This routing is already in use by AnniqueAPI. DBM will use the same path.

---

## 6. AccountMate Access Pattern

The VFP source (`NISource`) accesses AccountMate using:
- Class: `wwSQL` (West Wind Web Connection SQL object)
- Connection string pattern: `DRIVER={SQL Server};SERVER=<IP>;UID=sa;PWD=<pwd>;database=amanniquelive`
- Method: `oSql.ExecuteStoredProcedure()` for procedure calls, direct table reads for queries
- Four-part naming for linked server calls: `[AMSERVER-v9].amanniquelive.dbo.<procedure>`

The .NET 9 API (`AnqIntegrationApiSource`) accesses AccountMate using:
- EF Core `AccountMateDbContext` — direct table mapping, no ORM migrations
- Standard SQL Server connection string (environment-injected)
- Custom factory: `ClientDbContextFactory` / `ApiClientProvider`

**DBM should follow the EF Core pattern from AnqIntegrationApiSource.** Do not use the VFP ODBC pattern.

---

## 7. Security Notes

- **Hardcoded `sa` credentials in NISource** VFP files must never be reproduced in DBM
- DBM must use environment-based secrets (Azure Key Vault or environment variables)
- The `sa` account should be retired in favor of a least-privilege service account when AM is migrated to Azure
- JWT auth: use the same Bearer token pattern as AnniqueAPI
- API client registration: use the `ApiClients` table pattern from `NopIntegration` DB

---

## 8. AccountMate Pricing Logic (confirmed)

This is the single most complex domain. Facts confirmed from stored procedure archaeology:

| Fact | Source |
|---|---|
| Pricing oracle | `sp_camp_getSPprice` stored procedure |
| Formula | Effective price = `CampDetail.nPrice` if active campaign row exists, else `icitem.nprice` (20% discount floor) |
| Campaign discount | `sp_ct_updatedisc` → flat 20% consultant discount in all branches |
| Delta sync trigger | `CampDetail.dLastUpdate` + `Campaign.dLastUpdate` |
| Boundary enforcement | Mandatory sweep every 5–15 min to catch `dFrom`/`dTo` transitions without edit events |
| Shopify implementation | DBM precomputes effective prices → Shopify product metafields → Shopify Function applies at checkout |

See `docs/analysis/20_pricing_engine_deep_dive.md` and `docs/analysis/21_pricing_access_supplement.md` for full details.

---

*Continue reading: [03_workspace_guide.md](03_workspace_guide.md)*
