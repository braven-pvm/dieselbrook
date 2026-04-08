# Infrastructure Discovery — Complete Findings
## Dieselbrook — Internal Reference

**Date:** 2026-04-08  
**Method:** Direct access via FortiClient VPN + RDP to AZ-ANNIQUE-WEB (20.87.212.38)  
**Status:** Complete — all systems mapped

> ⚠️ **CONFIDENTIAL — DIESELBROOK INTERNAL ONLY**  
> This document contains production credentials obtained during authorised discovery.  
> Do not share. Rotate all credentials before DBM go-live.

---

## 1. Complete Infrastructure Map

```
┌─────────────────────────────────────────────────────────────────────────┐
│  AZURE VM: AZ-ANNIQUE-WEB                                               │
│  20.87.212.38 (public) / 10.0.0.4 (private)                            │
│  Subscription: 355e7b0b-ead0-4fb3-96ba-914900c4f2c4                    │
│  Resource Group: Annique_Web_Server                                      │
│  Size: Standard_B16ms (16 vCPU, 64 GB RAM)                             │
│  OS: Windows Server 2022 Datacenter Azure Edition                       │
│  Subnet: 10.0.0.0/24 (single VM, no other VMs in VNet)                 │
│                                                                         │
│  SQL Server 2019 (MSSQL15) — port 63000 — sa/Difficult1               │
│  Databases:                                                             │
│  ├── Annique        (17 GB) — NopCommerce production                   │
│  ├── BackOffice     (994 MB) — AM BackOffice web portal                │
│  ├── Brevo          — Brevo/email sync staging                         │
│  ├── Rod            — Rod's working DB                                 │
│  ├── RodWork        — Rod's work in progress (created Feb 2026)        │
│  ├── NewRegistration — Consultant registration                         │
│  ├── Registrations  — Registration data                                │
│  ├── Quizdb         — Product quiz                                     │
│  ├── Arc            — Archive (created Nov 2025)                       │
│  ├── Anq_Archive    — Annique archive (created Oct 2025)               │
│  └── Staging        — Staging NopCommerce                              │
│                                                                         │
│  Linked servers:                                                        │
│  ├── [AMSERVER-V9]  → away2.annique.com:62111 (production AM SQL)      │
│  └── [WEBSTORE]     → stage.anniquestore.co.za:61023 (old store)       │
│                                                                         │
│  IIS Applications (C:\inetpub\wwwroot\):                               │
│  ├── Annique/           NopCommerce 4.60 (.NET 7, last deploy Nov 2023) │
│  ├── AnqIntegrationAPI/ AnqIntegrationApi (.NET 9, last deploy Mar 2026│
│  ├── NopBlockPublisher/ Content publishing tool                        │
│  ├── Quiz/              Product quiz app                                │
│  ├── Registrations/     Consultant registration web app                │
│  └── Staging/           Staging NopCommerce                            │
│                                                                         │
│  VFP Applications:                                                      │
│  ├── C:\NopIntegration\  NISource (nopintegration.exe ×5, COM mode)   │
│  └── C:\Backoffice\      BackOffice (backoffice.exe ×2)                │
│                                                                         │
│  Other services:                                                        │
│  ├── C:\Apps\BrevoContactSync\  .NET 9 Brevo sync (dotnet.exe)        │
│  ├── C:\Apps\ImageSync\         Image sync app                         │
│  ├── C:\solr-8.11.0\            Apache Solr 8.11 (port 8983)          │
│  └── C:\Backoffice\             BackOffice VFP web portal              │
│                                                                         │
│  Remote access (4 methods):                                             │
│  ├── RDP port 3389                                                      │
│  ├── CentraStage/ConnectWise Automate RMM (CagService)                │
│  ├── UltraVNC (via CentraStage)                                        │
│  └── Chrome Remote Desktop (remoting_host.exe ×2)                     │
│                                                                         │
│  Monitoring/backup:                                                     │
│  ├── New Relic infrastructure agent                                     │
│  ├── Veeam Endpoint Backup + Management Agent                          │
│  ├── Fluent Bit (log shipping)                                         │
│  └── ESET NOD32 antivirus                                              │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│  PRODUCTION AM SQL SERVER                                               │
│  196.3.178.122:62111 (away2.annique.com)                               │
│  INTERNET EXPOSED — accessible without VPN                             │
│  Credentials: sa / AnniQu3S@                                           │
│                                                                         │
│  Databases (confirmed):                                                 │
│  ├── amanniquelive / AmAnniqueLive  — production AM transactions       │
│  ├── NopIntegration                 — NISource integration tables      │
│  ├── compplanLive                   — compensation/MLM                 │
│  └── compsys                        — communications/system            │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│  HETZNER SERVER (JNB3 South Africa)                                    │
│  129.232.251.215 (dedi475.jnb3.host-h.net)                            │
│  Also: anniqueshop.com → redirects to www.annique.com                  │
│                                                                         │
│  Apache web server (port 80/443)                                       │
│  MariaDB 10.11 (port 3306 — INTERNET EXPOSED)                         │
│  SSH (port 22), FTP (port 21)                                          │
│                                                                         │
│  Database: anniqhtkms_db2                                              │
│  Credentials: anniqhtkms_2 / ke9RDwqIgl7ry9TP6QL8                    │
│  Contents: WordPress + WooCommerce (196 wp_ tables)                    │
│  ├── wp_users:                31,691 rows                              │
│  ├── wp_woocommerce_order_items: 82,084 rows                          │
│  ├── vw_affiliates:           15,112 rows (consultant referrals)       │
│  └── uap_* tables:            Affiliate/referral program               │
│                                                                         │
│  This appears to be a consultant-facing affiliate/referral store       │
│  or a secondary storefront — NOT the main annique.com NopCommerce     │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│  ON-PREMISES (Annique office, Irene, Pretoria)                         │
│  LAN: 172.19.16.0/24                                                   │
│  FortiGate firewall: 41.193.227.190 (away1.annique.com:10443)          │
│                                                                         │
│  172.19.16.100  — AccountMate application server (AMSERVER-v9)        │
│    Staff run AM BackOffice thick client here                            │
│    SQL data is NOT here — it's at 196.3.178.122                        │
│    RDP accessible via VPN (no credentials provided for prod)           │
│                                                                         │
│  172.19.16.101  — Clone/test AM server                                 │
│    RDP: annique\Dieselbrook / Diesel@2026#7                            │
│    Standard user only — no admin access                                │
│    SQL on non-standard port (not yet confirmed via SQL connection)     │
│                                                                         │
│  172.19.16.27   — Unknown SQL Server                                   │
│    SQL port 1433 + RDP open                                            │
│    Purpose unknown — not investigated                                  │
│                                                                         │
│  172.19.16.16   — Unknown Windows server                               │
│    SMB + HTTP (403) + RDP                                              │
│                                                                         │
│  172.19.16.12/15/18/19/24/25 — Canon printers                         │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│  SHOPAPI.ANNIQUE.COM                                                    │
│  129.232.215.87 (SEPARATE — NOT same as Hetzner server)                │
│  IIS 10.0 / Web Connection (VFP)                                       │
│  This is the NISource VFP server for shopapi endpoints:                │
│  ├── /otpGenerate.api   — returns HTTP 400 (needs POST body)           │
│  └── /SyncCancelOrders.api — returns HTTP 200 (live)                  │
│  Hosting provider unknown                                               │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Complete Credential Inventory

> ⚠️ ALL of these are `sa` with plaintext passwords. All are in cleartext config files on the production server. All must be replaced before DBM go-live.

| System | Server | Port | Username | Password | Exposure |
|---|---|---|---|---|---|
| NopCommerce DB | `20.87.212.38` | 63000 | `sa` | `Difficult1` | Internet |
| Brevo DB | `20.87.212.38` | 63000 | `sa` | `Difficult1` | Internet |
| BackOffice DB | `20.87.212.38` | 63000 | `sa` | `Difficult1` | Internet |
| AM `amanniquelive` | `196.3.178.122` | 62111 | `sa` | `AnniQu3S@` | Internet |
| AM `NopIntegration` | `196.3.178.122` | 62111 | `sa` | `AnniQu3S@` | Internet |
| Old store SQL | `stage.anniquestore.co.za` | 61023 | `sa` | `Difficult1` | Internet |
| MariaDB (Hetzner) | `129.232.251.215` | 3306 | `anniqhtkms_2` | `ke9RDwqIgl7ry9TP6QL8` | Internet |
| NISource email (Outlook) | `smtp-mail.outlook.com` | 587 | `anniquealerts@annique.com` | `D!fficult1!2022` | N/A |
| BackOffice email (Mimecast) | `za-smtp-outbound-1.mimecast.co.za` | — | `anniquealerts@annique.com` | `P@ssw0rd!1` | N/A |
| Brevo API | `api.brevo.com` | — | — | `xkeysib-...` (see local only — not committed) | N/A |
| AnqIntegrationApi JWT | — | — | — | `egQ+...` (see local only — not committed) | N/A |
| VPN (Dieselbrook) | `away1.annique.com` | 10443 | `Dieselbrook` | `Diesel@2026#7` | Internet |
| RDP test AM server | `172.19.16.101` | 3389 | `annique\Dieselbrook` | `Diesel@2026#7` | VPN only |

---

## 3. Key Technical Discoveries

### 3.1 Development = Production

The `appsettings.development.json` for AnqIntegrationApi points at **the same production servers and databases** as `appsettings.json`. There is no development environment separation. Development testing writes to production databases. The only difference is `ForceToEmail: melinda@annique.com` (dev emails go to Melinda, not real recipients) and `BackgroundWorkerEnabled: false`.

### 3.2 [AMSERVER-V9] is a linked server alias, not a machine

The string `[AMSERVER-V9]` that appears throughout the VFP source code is a **SQL Server linked server** defined on `AZ-ANNIQUE-WEB`. It points to `away2.annique.com:62111`. The physical AM application server at `172.19.16.100` runs the thick client but has no SQL Server — data lives at `196.3.178.122`.

### 3.3 AM SQL Server is internet-accessible on non-standard port

`196.3.178.122:62111` is accessible from anywhere on the internet with credentials `sa`/`AnniQu3S@`. This is not behind a VPN. This is the production AccountMate database — `amanniquelive` with 15+ years of business data.

### 3.4 Rod is the developer

`AnniqueRod.mdf` (the NopCommerce database file name), `rod.log` (BackOffice activity log updating daily), `Rod` and `RodWork` databases on the Azure SQL Server — Rod is the developer/contractor who built NopCommerce customisation, BackOffice, and AnqIntegrationApi. He has active sessions on the production server. He develops directly on the production Azure VM.

### 3.5 WhatsApp integration is live

AnqIntegrationApi has WhatsApp templates (template ID 65) and a WhatsApp sender number `27648909237`. This is a live Brevo WhatsApp channel. DBM's communications spec must account for WhatsApp alongside email.

### 3.6 Brevo template IDs confirmed

From `appsettings.json` — these are the exact production Brevo template IDs:
- `431` — Sponsor info
- `588` — Sponsor new registration email
- `584` — Sponsor re-registration email
- `65` (WhatsApp) / `59` (dev) — New registration WhatsApp
- `774` — New template added to production (not in dev config) — unknown purpose

### 3.7 NopCommerce 4.60 on .NET 7, deployed November 2023

`C:\inetpub\wwwroot\Annique\` DLLs all dated `01/18/2023` with a last `Plugins` update `03/05/2026`. Exact version confirmed via `appsettings.json`:

```json
"WebOptimizer": {
  "CacheDirectory": "C:\\inetpub\\wwwroot\\nopCommerce_4.60.0\\wwwroot\\bundles"
}
```

Connection string:
```json
"ConnectionString": "Data Source=AZ-ANNIQUE-WEB;Initial Catalog=Annique;...User ID=sa;Password=Difficult1;Trust Server Certificate=True;"
```

This is NopCommerce **4.60** on .NET 7. The site runs but hasn't been significantly updated since initial deployment. Database `Annique` (17 GB) on the local Azure SQL Server.

**Backup directory — `__BACKUP__\Staging\`** (found on Azure VM) contains `Annique.Customization` SQL scripts from Sep 2023:
- `AlterExclusiveItemsSchema.sql` — custom schema for exclusive/consultant-only products
- `AlterProductSchema.sql` — custom product schema extensions
- `AlterSpGetFilterPickUpStores.sql` / `DropSpGetFilterPickUpStore.sql` / `DropSpGetPickUpStoreById.sql` — pick-up store stored procs
- `AlterUserProfileSchema.sql` — user profile extensions
- `CreateSpGetPickUpStoreById.sql` — new pick-up store SP

These scripts document the custom schema additions to the `Annique` (NopCommerce) database. DBM does not interact with this database directly, but the exclusive items schema is relevant to the product catalogue domain.

### 3.8 AnqIntegrationApi is .NET 9 — the reference implementation

`C:\inetpub\wwwroot\AnqIntegrationAPI\` — last deployed `03/25/2026` — 2 weeks ago. Uses:
- EF Core (Microsoft.EntityFrameworkCore.SqlServer)
- Serilog
- JWT Bearer auth (same signing key in dev and prod — security issue)
- Swashbuckle/Swagger
- Azure.Identity (Managed Identity ready)

This is the pattern DBM follows — but DBM uses Dapper for AM access, not EF Core.

### 3.9 The Brevo DB and sync apps

There is a dedicated `Brevo` database on the Azure SQL Server (created Sep 2025) and two apps that sync to it:
- `AnqIntegrationApi` — syncs Brevo events every 10 minutes
- `BrevoContactSync` (C:\Apps\) — separate .NET 9 console app syncing contacts, last built Mar 2026

Both use the same Brevo API key. DBM's communications service must integrate with this existing Brevo setup — same API key, same template IDs.

---

## 4. Implications for DBM

### What changes from earlier assumptions

| Assumption | Corrected fact |
|---|---|
| AM SQL is at `172.19.16.100` (on-prem) | AM SQL is at `196.3.178.122:62111` — internet-accessible |
| FortiGate S2S VPN required for production AM | **Not required** — DBM can connect to `196.3.178.122:62111` directly |
| `shopapi.annique.com` is unknown | Confirmed as NISource VFP server on `129.232.215.87` |
| NopCommerce on Azure VM is the live store | NopCommerce is legacy — WordPress/WooCommerce on Hetzner is parallel/newer |
| AnqIntegrationApi has REDACTED config | Full config confirmed — EF Core, `annique` + `brevo` + `AmAnniqueLive` databases |

### DBM production connectivity

DBM can connect to AM SQL directly:
```
Server=196.3.178.122,62111;Database=amanniquelive;User Id=dbm_svc;Password=<from-keyvault>
```

No VPN Gateway required. The `vpn-gateway.bicep` module in `docs/spec/03_infrastructure.md` remains available for future use (AM application server access, migration work) but is not on the critical path for go-live.

### Security actions required before go-live

1. **Create `dbm_svc` login** on `196.3.178.122:62111` with least-privilege grants (per `docs/spec/03_infrastructure.md §4.4`)
2. **Store in Key Vault** — never in `appsettings.json`
3. **Do NOT use `sa`** — disable or rotate `sa` on all systems as part of the programme (out of DBM scope but flagged)
4. **Rotate JWT signing key** — same key used in dev and prod; generate new key for DBM

### Brevo integration

DBM's comms service uses the **same Brevo API key** and same template IDs as the existing system. The transition must be coordinated — DBM cannot run in parallel with AnqIntegrationApi sending the same emails. The cutover for comms must be atomic.

Template IDs DBM must support:
- `431` — Sponsor info email
- `588` — Sponsor new registration
- `584` — Sponsor re-registration  
- `65` — New registration WhatsApp
- `774` — Unknown (needs investigation)

---

## 5. What Still Needs Investigation

| Item | How | Priority |
|---|---|---|
| What is `172.19.16.27` SQL Server? | RDP to .27 (need credentials from Marcel) | Medium |
| What is the `Rod` database (24 MB, May 2024)? | Run `SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES` on Azure SQL | Low |
| Is the WordPress/WooCommerce on Hetzner the current live `annique.com` or a separate store? | Check Cloudflare origin settings (need Azure/DNS access) | High |
| What does `C:\inetpub\wwwroot\Registrations\` do? | `dir` the path | Medium |
| What is Brevo template `774`? | Check Brevo account | Low |
| Who manages `shopapi.annique.com` hosting? | Ask Marcel | High — TC-04 |
| Full database list on `196.3.178.122:62111` | Connect via SSMS from test server | High |

---

## Evidence Base

| Artefact | Source |
|---|---|
| Azure VM metadata | `curl http://169.254.169.254/metadata/instance` from AZ-ANNIQUE-WEB |
| Process list | `tasklist` from AZ-ANNIQUE-WEB |
| Port inventory | `netstat -an` from AZ-ANNIQUE-WEB |
| NISource config | `C:\NopIntegration\deploy\Nopintegration.ini` |
| BackOffice config | `C:\Backoffice\deploy\Backoffice.ini` |
| AnqIntegrationApi config | `C:\inetpub\wwwroot\AnqIntegrationAPI\appsettings.json` |
| BrevoContactSync config | `C:\Apps\BrevoContactSync\appsettings.json` |
| SQL linked servers | SSMS query on AZ-ANNIQUE-WEB SQL Server |
| Azure SQL database list | Direct connection `20.87.212.38:63000` sa/Difficult1 |
| MariaDB tables | Direct connection `129.232.251.215:3306` anniqhtkms_2 |
