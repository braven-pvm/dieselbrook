# AZ-ANNIQUE-WEB — Complete Forensic Discovery
## Dieselbrook — Internal Reference

**Date:** 2026-04-18
**Access method:** Subscription Owner → Azure Run-Command (SYSTEM-level PowerShell) + RDP as local admin `DieselBrook`
**Access level:** Full local Administrator + SQL Server sysadmin (via `sa`/`Difficult1`)
**Status:** First admin-level forensic pass on the Annique production Azure VM

> ⚠️ **CONFIDENTIAL — DIESELBROOK INTERNAL ONLY**
> Contains production credentials, infrastructure details, SQL integration source, and security-sensitive findings obtained during authorised discovery. Do not share. Rotate all credentials before DBM go-live.

---

## 1. Executive Summary

This is the first session with full admin + SQL-sysadmin access to AZ-ANNIQUE-WEB, the Azure VM hosting **the entire Annique online estate**. Earlier discovery (doc 30) gave us a partial picture via RDP at a restricted level; this session fills every remaining gap and captures the full integration graph.

### The one headline to take away

**`annique.com`, the production e-commerce store, runs on a single Azure VM (Standard_B16ms, one NIC, no redundancy)**, alongside staging, the Namibia store, the NopCommerce VFP integration API (`nopintegration.annique.com`), the BackOffice portal, the Quiz site, all integration services (Brevo, Meta, PayU, courier tracking, WhatsApp, image sync), Apache Solr, SFTP, the SQL Server instance holding 11 databases (901K orders), Rod's live dev sandbox, and the backup target. **Everything.**

### Top 10 findings

1. **Single-VM SPOF** — the entire Annique online business is one VM away from total outage. Nothing is redundant.
2. **SQL Server 2019 RTM (15.0.2000.5) — never patched**. Out of mainstream support since Jan 2025. Extended support until Jan 2030.
3. **SQL Server port 63000 is exposed to the entire internet** with `sa` / `Difficult1`. Any internet scan of `20.87.212.38:63000` can try to log in.
4. **The integration engine is entirely SQL-Agent-driven** — 21 SQL Agent jobs orchestrate everything between NopCommerce, AM, Brevo, Meta, PayU, and couriers. Cadence ranges from every 5 minutes to monthly.
5. **HTTP calls from SQL use `sp_OACreate 'MSXML2.ServerXMLHTTP'`** via a custom proc `ANQ_HTTP` (byte-for-byte identical to `sp_ws_HTTP` on AMSERVER-TEST — copy-pasted). `Ole Automation Procedures = ON`. Hardcoded default auth header.
6. **Rod actively develops on the production VM** — `Rod` and `RodWork` databases, multiple RDP sessions hanging for months. `RodWork` was created 2026-02-06, the same week Dieselbrook SQL logins were pre-created on AMSERVER-TEST.
7. **`nopintegration.annique.com` is hosted here** as an IIS site running a West Wind VFP Web Connection app — and its app pool runs as **LocalSystem**. This is the endpoint `sp_NOP_syncOrders` on production AM calls.
8. **`AnqImageSync.Console.exe` — a .NET 10 app — runs every 10 minutes** as AmAdmin, pulling images from production AM SQL (`away2.annique.com:62111`, `sa`/`AnniQu3S@`) and writing them into the NopCommerce SQL (`20.87.212.38:63000`, `sa`/`Difficult1`). New AM data crosses the internet hop every 10 minutes.
9. **26 IPs are whitelisted for RDP** (Annique office + personal IPs of staff + consultant IPs). Management of this NSG rule list is essentially unmanaged.
10. **Integration to real AM SQL happens three ways at once**: (a) SQL Agent jobs calling procs that use the `[AMSERVER-V9]` linked server; (b) VFP processes (`nopintegration.exe`, `backoffice.exe`) connecting directly via TCP to `196.3.178.122:62111`; (c) .NET scheduled tasks (ImageSync) connecting directly via TCP. None of these are routed or isolated — they talk to production AM directly from this box.

---

## 2. Host Profile

### 2.1 VM and OS

| Attribute | Value |
|---|---|
| VM name | `AZ-ANNIQUE-WEB` |
| Resource group | `Annique_Web_Server` |
| VM SKU | `Standard_B16ms` (burstable — 16 vCPU, 64 GB RAM) |
| Subscription | Azure CSP Subscription (`355e7b0b-ead0-4fb3-96ba-914900c4f2c4`) |
| Region | `southafricanorth` |
| OS | Microsoft Windows Server 2022 Datacenter Azure Edition (build 20348) |
| OS install | 2023-01-09 |
| Last boot | **2025-10-28 19:50** — uptime 171.6 days |
| Free RAM | 20.5 GB of 64 GB |
| Host | Hypervisor (virtual) |
| Logical processors | 16 |
| Domain | `WORKGROUP` — **NOT domain-joined** (Workgroup membership) |
| AAD state | **Azure AD joined** (confirmed via `dsregcmd /status`) to tenant `annique.com` (`2147b956-...`) |
| VM extensions | `AADLoginForWindows`, `VMAccessAgent`, `RunCommandWindows`, `SqlIaaSAgent` |
| Public IP | `20.87.212.38` (static) |
| Private IP | `10.0.0.4` (single NIC) |

### 2.2 Local admins (all **local** accounts, not AAD)

All user SIDs are under the machine SID `S-1-5-21-1207130743-2071083568-3428478065-*`:

| Account | SID suffix | Role |
|---|---|---|
| `MULTiAdministrator` | -500 | Built-in Administrator (creator account) |
| `AmAdmin` | -1000 | AM integration account (runs scheduled tasks) |
| `AnqAdmin` | -1002 | Annique admin (active user) |
| `nopAccelerate` | -1004 | NopCommerce accelerator |
| `Backoffice` | -1005 | BackOffice service account |
| `DieselBrook` | -1006 | Our account (added 2026-04-08, promoted to admin 2026-04-18) |

### 2.3 Active and stale RDP sessions (at time of discovery)

| User | State | Idle | Logged in |
|---|---|---|---|
| `multiadministrator` | Disconnected | **163 days** | 2025-11-05 10:01 |
| `anqadmin` | Disconnected | 18 min | 2025-10-29 08:23 |
| `amadmin` | Disconnected | 5 min | 2025-10-29 09:58 |
| `dieselbrook` | **Active** | 4 min | 2026-04-18 09:23 |

**Same pattern as AMSERVER-TEST** — users disconnect without signing out, so sessions from months ago are still alive with their programs (SSMS, VFP IDE, Chrome) running. `AmAdmin` and `AnqAdmin` sessions are used to run the automation as those users never logged out; changing their passwords would kill scheduled tasks.

### 2.4 Disk layout

Previously inventoried in doc 30. The only note here: both `C:` and `D:` are shared as `C$` / `D$` defaults only — no custom file shares. IPC$ and ADMIN$ are default too.

---

## 3. Network Perimeter

### 3.1 Azure NSG inbound rules

| Rule | Port | Sources | Risk |
|---|---|---|---|
| `RDP` (prio 300) | TCP 3389 | **26 IP whitelist** including Annique office `41.193.227.190`, `86.38.71.144` (our NordVPN), various UK/SA/Asia IPs | Manually maintained — bloat risk |
| **`SQL`** (prio 310) | **TCP 63000** | **`*` (internet)** | **CRITICAL — `sa`/`Difficult1` reachable from any internet host** |
| `AllowAnyCustom80Inbound` (prio 320) | 80 any-protocol | `*` | Expected — web store |
| `AllowAnyCustom443Inbound` (prio 330) | 443 any-protocol | `*` | Expected — web store |
| `2222Inbound` (prio 340) | TCP 2222 | 15-IP whitelist | Rebex SFTP server |
| `AllowCidrBlockCustom2223Inbound` (prio 350) | TCP 2223 | 14-IP whitelist (includes `196.3.178.123`) | Rebex SFTP server (second instance) |
| `SMTPOutbount` (prio 100 out) | TCP 587 | Outbound | Allows outbound SMTP |

Default rules at bottom: `AllowVnetInBound` (65000), `AllowAzureLoadBalancerInBound` (65001), `DenyAllInBound` (65500).

**RDP whitelist includes hardcoded IPs of consultants' home broadband** — unmaintainable and leaks if anyone's ISP changes their allocation.

### 3.2 VNet layout

Single VM on `snet-default` (`10.0.0.0/24`) in `vnet-annique` within `Annique_Web_Server` RG. No peerings to other VNets. No VPN Gateway. No Bastion.

### 3.3 Listening ports (on the VM)

| Port | Binding | Process | Role |
|---|---|---|---|
| 80 | `::` | System (IIS) | HTTP |
| 443 | `::` | System (IIS) | HTTPS |
| 135 | `::` | svchost | RPC endpoint mapper |
| 139 | `10.0.0.4` | System | NetBIOS |
| 445 | `::` | System | SMB |
| 1434 | `127.0.0.1` | sqlservr | SQL Browser (loopback only) |
| **2222** | `0.0.0.0` | `RebexTinySftpServer` | SFTP (primary) |
| **2223** | `0.0.0.0` | `RebexTinySftpServer` | SFTP (secondary) |
| 3389 | `::` | svchost | RDP |
| 5985 | `::` | System | WinRM |
| 6183, 6184, 9395, 62646, 62651, 62653 | various | Veeam services | Backup agent |
| 7983 | `127.0.0.1` | java | Solr stop port |
| **8983** | `::` | java | **Apache Solr** (product search) |
| 18003 | `127.0.0.1` | newrelic-infra | New Relic local endpoint |
| 47001 | `::` | System | WinRM |
| **63000** | `::` | sqlservr | **SQL Server (non-default port, internet-exposed per NSG)** |

### 3.4 Live outbound connections observed (during discovery)

Examples from a single netstat snapshot — this is constant background traffic:

- `nopintegration.exe` × 6 connections → `196.3.178.122:62111` (production AM SQL)
- `backoffice.exe` × 1 → `196.3.178.122:62111`
- `w3wp.exe` (IIS workers) × 3 → `196.3.178.122:62111`
- `sqlservr.exe` outbound to `196.3.178.123:64820` (note: this is `away2.annique.com + 1` — unusual, could be a secondary AM endpoint or misconfiguration)
- Multiple connections to Cloudflare edge IPs (162.158.*, 172.68.*, 172.69.*, 172.71.*) — likely Brevo API, PayU API, etc. via Cloudflare
- New Relic: `162.247.243.20:443` (newrelic-infra), `162.247.243.22:443` (fluent-bit)
- Datto RMM (formerly CentraStage/ConnectWise Automate): `3.33.180.249:443` (AEMAgent), `54.194.42.15:443` (CagService)
- ESET telemetry: `91.228.165.148:8883`
- Azure Guest Agent: `168.63.129.16` (Azure wire server)
- Chrome (from ghost sessions): `74.125.206.188:5228` (Google GCM)
- Windows Update: `2.20.13.164:443`, `197.234.242.*` and `197.234.243.*` (MS update CDN)
- Inbound RDP: `86.38.71.144:59461` → `:3389` (our NordVPN session)
- **Inbound SQL from `41.193.227.190` × 7 connections** — the Annique office public IP. Staff are querying this VM's SQL right now from the office.

### 3.5 Hosts file overrides

```
127.0.0.1    annique.com
127.0.0.1    www.annique.com
```

The VM resolves `annique.com` and `www.annique.com` to itself. Any IIS-hosted code that references `annique.com` (for self-callback or URL generation) stays local. No other overrides.

---

## 4. IIS Sites

Single IIS 10 server hosting seven bindings across five active sites:

| Site | Physical path | Bindings | Pool identity | Role |
|---|---|---|---|---|
| `Default Web Site` | `%SystemDrive%\inetpub\wwwroot` | `http://*:80` (catch-all) | `ApplicationPoolIdentity` | Default handler |
| `Annique` | `C:\inetpub\wwwroot\Annique` | `annique.com`, `www.annique.com` HTTP+HTTPS | `ApplicationPoolIdentity` | **Production NopCommerce 4.60 (.NET 7)** — the main store |
| `Staging` | `C:\inetpub\wwwroot\Staging` | `stage.annique.com`, `stage.annique.co.na` HTTP+HTTPS | `ApplicationPoolIdentity` | Staging NopCommerce + Namibia store |
| `NopIntegration` | `C:\NopIntegration\web` | `nopintegration.annique.com` HTTP+HTTPS | **`LocalSystem`** | **VFP Web Connection app — the endpoint `sp_NOP_syncOrders` calls** |
| `Backoffice` | `C:\Backoffice\web` | `backoffice.annique.com` HTTP+HTTPS | `LocalSystem` (inherits `NopIntegration` pool) | VFP BackOffice portal |
| `Quiz` | `C:\NopIntegration\web\LandingPages\Registration` | `quiz.annique.com` HTTP+HTTPS | `NetworkService` | Product quiz |
| `Registrations` | `c:\registrations\web` | `newregistration.annique.com` | `ApplicationPoolIdentity` | **STOPPED** |

Additional applications inside those sites:
- `/backoffice` (on Default Web Site) → `c:\backoffice\web` (pool: `NopIntegration`)
- `/NopBlockPublisher` → `C:\inetpub\wwwroot\NopBlockPublisher` (pool: `NopBlockPublisher`)
- `/API` → `C:\inetpub\wwwroot\AnqIntegrationAPI` (pool: `API` — no managed-runtime = .NET Core / 5+)

App pool identities worth flagging:
- **`NopIntegration` runs as `LocalSystem`** — the VFP Web Connection app has SYSTEM privileges on the VM. Any compromise of the VFP app = full OS compromise.
- **`WebConnection` app pool also `LocalSystem`** (legacy Web Connection demo pool)
- Rest use `ApplicationPoolIdentity` (sane default).

---

## 5. Running Applications (integration layer)

### 5.1 Scheduled tasks (OS-level integrations)

| Task | Frequency | Runs as | Action |
|---|---|---|---|
| **`Brevo Contact Sync`** | Daily 05:22 | `AmAdmin` | `C:\Apps\BrevoContactSync\BrevoContactsSync.exe` |
| **`Sync Images to Nop`** | **Every 10 min** | `AmAdmin` | `cmd /c C:\Apps\ImageSync\AnqImageSync.Console.exe >> log.txt 2>&1` |
| `win-acme renew` | Daily | `SYSTEM` | `C:\Software\win-acme.*\wacs.exe --renew` (Let's Encrypt) |
| `OneDrive Reporting Task-*` | Daily 03:00 (per user) | each local user | OneDrive telemetry (one task per user profile) |
| Misc MS Edge / Google updater tasks | various | SYSTEM / individual | browser auto-update |

### 5.2 `AnqImageSync.Console.exe` (the 10-minute loop)

Location: `C:\Apps\ImageSync\`

Runtime: **.NET 10** (per `AnqImageSync.Console.runtimeconfig.json`) — unusually modern for this estate.

`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SourceDb": "Server=away2.annique.com,62111;Database=AmAnniqueLive;User Id=sa;Password=AnniQu3S@;Encrypt=False;TrustServerCertificate=True;",
    "NopDb": "Server=20.87.212.38,63000;UID=sa;PWD=Difficult1;database=annique;Encrypt=False;TrustServerCertificate=True;"
  },
  "ImageSync": {
    "Mode": "PendingUpdates",
    "WebpQuality": 75,
    "OnlyItemNo": "",
    "BatchSize": 200,
    "MaxDegreeOfParallelism": 1,
    "OutputFolder": "",
    "WriteToDatabase": true
  }
}
```

**This single app is the live AM→NopCommerce image bridge.** Every 10 minutes it:
1. Connects to production AM SQL (`away2.annique.com:62111`) using `sa`/`AnniQu3S@`
2. Reads pending image updates from AM (almost certainly the `iciimg` table we saw referenced in the now-disabled `ANQ_SyncImage` proc)
3. Re-encodes as WebP @ 75% quality
4. Writes binary data to local NopCommerce SQL (`Picture`, `PictureBinary`, `Product_Picture_Mapping` tables)
5. Logs to `C:\Apps\ImageSync\log.txt`

Both connection strings use plaintext `sa` credentials with `Encrypt=False`. Replacing this pattern is DBM table stakes.

### 5.3 Other operational services

- **Apache Solr 8.11.0** at `C:\solr-8.11.0\` — product search index, listening on 8983 (Java)
- **Rebex TinySFTPServer** — two instances on ports 2222 and 2223 — file drop endpoints (contents of transfers not yet inspected)
- **Veeam Agent for Microsoft Windows 6.3.2** + **Veeam Service Provider Console Management Agent 9.1** — backup
- **Datto RMM 4.4.10748** — remote monitoring/management (successor to ConnectWise Automate CentraStage)
- **New Relic .NET Agent 10.19.2** + **New Relic Infrastructure Agent 1.48.1** + **New Relic CLI** — monitoring
- **ESET Server Security 12.0** — AV
- **Chrome Remote Desktop Host 147.0** — secondary remote access mechanism (installed March 2026)
- **Java 8 Update 441** — Solr dependency
- **Notepad++ 8.8.8** — developer tool

### 5.4 Solr cores present

Folders in `C:\solr-8.11.0\server\solr\`:
- `staging_annique460` (2025-02-22) — staging NopCommerce 4.60 index
- `staging_annique460_new` (2025-02-22) — second staging index
- `stag_annique460` (2025-02-26) — another staging variant
- Plus default: `configsets`, `filestore`, `userfiles`

No production index in the list — production NopCommerce may be pointing at one of the `staging*` indices or Solr may be used for staging/testing only.

---

## 6. SQL Server Deep Dive

### 6.1 Instance

| Attribute | Value |
|---|---|
| `@@SERVERNAME` | `AZ-ANNIQUE-WEB` |
| Version | Microsoft SQL Server 2019 (**RTM** — 15.0.2000.5) |
| Edition | Standard Edition (64-bit) |
| Collation | `SQL_Latin1_General_CP1_CI_AS` |
| Port | **63000** (non-default; NSG-exposed to the internet) |
| Mainstream support ended | **2025-01-07** (3 months before this discovery) |
| Extended support ends | 2030-01-08 |
| Memory config | `max = 28000 MB`, `min = 0 MB` |
| MaxDOP | 8 |
| Cost threshold for parallelism | 5 (default — usually recommended to raise to 50) |
| **`Ole Automation Procedures`** | **1 (enabled)** — required for `ANQ_HTTP` proc |
| **`xp_cmdshell`** | **1 (enabled)** — security risk |
| `clr enabled` | 0 |
| `Database Mail XPs` | 0 (DbMail not used) |
| `Agent XPs` | 1 |

### 6.2 Databases

| Database | Recovery | Compat | Created | Backup size | Role |
|---|---|---|---|---|---|
| **`Annique`** | FULL | 150 | 2023-03-09 | **8.3 GB** | **NopCommerce production** |
| `BackOffice` | SIMPLE | **120** | 2025-02-13 | 718 MB | BackOffice VFP working DB |
| `Brevo` | FULL | 150 | 2025-09-22 | 260 MB | Brevo integration data + segmentation |
| `NewRegistration` | FULL | 150 | 2025-03-19 | 3 MB | New registration flow |
| `Registrations` | FULL | **100** | 2025-09-18 | 346 MB | Older registration DB (SQL 2008 compat — odd) |
| `Quizdb` | FULL | 150 | 2025-07-15 | 4 MB | Quiz site |
| `Anq_Archive` | SIMPLE | 150 | 2025-10-30 | **6.2 GB** | Annique archive |
| `Arc` | FULL | 150 | 2025-11-03 | **13.2 GB** | Another archive (largest user DB) |
| `Staging` | SIMPLE | 150 | 2023-01-30 | 672 MB | Staging NopCommerce |
| `Rod` | FULL | 150 | 2024-05-30 | 17 MB | Rod's dev DB |
| **`RodWork`** | FULL | 150 | **2026-02-06** | **7.5 GB** | **Rod's new work-in-progress DB created 6 weeks before we got access** |

The `RodWork` creation date (2026-02-06) is the same week Dieselbrook SQL logins were pre-created on AMSERVER-TEST (2026-02-25). Something organisational was happening at Annique in early 2026 — possibly preparing to hand some work to us.

### 6.3 Linked servers

| Name | Provider | Data source | `sa`/`sa` mapping |
|---|---|---|---|
| `AMSERVER-V9` | SQLNCLI | `away2.annique.com,62111` | Yes |
| `WEBSTORE` | SQLNCLI | `stage.anniquestore.co.za,61023` | Yes |

Both use `SQLNCLI` (legacy SQL Native Client) not the modern MSOLEDBSQL. Both map local `sa` → remote `sa` with stored credentials. Fallback credential (empty local principal) also mapped.

### 6.4 Logins and privileged accounts

**6 sysadmin logins** (over-privileged — most should be least-privilege):

- `sa` — SQL login (2003 — this SID came with the instance; not rotated)
- **`reports`** — **SQL login**, sysadmin (2023-10-30). Unknown purpose. Reporting? Integration? Needs investigation.
- `AZ-ANNIQUE-WEB\Accelerate` — Windows login, sysadmin
- `AZ-ANNIQUE-WEB\AmAdmin` — Windows login, sysadmin
- `AZ-ANNIQUE-WEB\AnqAdmin` — Windows login, sysadmin
- `AZ-ANNIQUE-WEB\MULTiAdministrator` — Windows login, sysadmin

Plus NT services (expected): MSSQLSERVER, SQLSERVERAGENT, SQLWriter, Winmgmt.

No login for `DieselBrook` currently — we use `sa` for discovery.

### 6.5 SQL Agent jobs (the integration engine)

All 21 jobs inventoried with cadence and actions:

| Job | Schedule | Action | Purpose |
|---|---|---|---|
| `Sync - Customers to AM` | Hourly (07:00 start) | `EXEC ANQ_CustomerChanges_UPDATE; EXEC ANQ_CustomerAddress_UPDATE` | Push customer changes from NopCommerce→AM via `[AMSERVER-V9]` |
| `Sync - Gifts` | Every 15 min + daily 03:30 | `EXEC ANQ_SyncGift` | Gift products / GWP |
| `Sync - Published` | Hourly | `EXEC [dbo].[ANQ_SyncPublished]` | Publish/unpublish products based on AM stock + exclusive items |
| `Sync Affiliates` | Every 15 min | `EXEC ANQ_SyncAffiliates` | Affiliate (consultant) records |
| **`Sync Order Status`** | **Every 20 min (office hours only)** | `EXEC ANQ_syncOrderStatus` | Pull order status from AM via `OPENQUERY` |
| `MetaCapi Enqueue + Process` | Every 5 min | `EXEC Annique.dbo.ANQ_MetaCapi_ProcessQueue` | Meta (Facebook) Conversions API |
| `Segmentation - Rebuild & Sync (Hourly)` | Hourly | `EXEC dbo.sp_Segments_Rebuild; EXEC dbo.sp_Segments_PostToBrevo` | Customer segmentation → Brevo |
| `Brevo - Update Transaction Attributes` | Hourly | Brevo DB procs | Brevo attribute updates |
| `Check and mark any Payu Sales that are paid and not processed` | Hourly | PayU reconciliation procs | SA payment gateway reconciliation |
| `Cleanup New Registrations` | Every 5 min | Registrations procs | Housekeep new registrations |
| `Update Activation Date if null` | Hourly | `EXEC ANQ_UpdateActivationDate` | Consultant activation date fill |
| `Fix Gift Published` | Every 2 min (**DISABLED**) | `ANQ_*` | Legacy gift flag fix |
| `Update Special Offer Ribbon` | Every 15 min (**DISABLED**) | `EXEC ANQ_SpecialOfferRibbon` | Offer ribbon update |
| `Whatsapps for Sliding Scale and ADQ` | 24th + 28th of month | `EXEC CompPlan_WhatsAppSlidingScale; EXEC CompPlan_WhatsAppADQ` | WhatsApp MLM comms |
| `Archive Nop Logs etc` | Daily | Log archiving | Maintenance |
| `Build Full TEXT` | Daily 04:30 | `EXEC ANQ_BUILDFullText` | Rebuild NopCommerce full-text index |
| `Purge ExportImport Old Files` | Daily | PowerShell script | Deletes `C:\inetpub\wwwroot\Annique\wwwroot\files\exportimport\*` older than 14 days |
| `Clear 404 log` | Hourly | Log cleanup | Housekeep |
| `Loast Cart Run` [sic] | Hourly | Lost cart recovery | Abandoned cart processing |
| `Begin Month` | Monthly (1st) | Month-end procs | Month-close routines |
| `syspolicy_purge_history` | Daily | Default MS maintenance job | Maintenance |

**Notable PowerShell action** (inside the `Purge ExportImport Old Files` job):

```powershell
$folderPath = "C:\inetpub\wwwroot\Annique\wwwroot\files\exportimport"
$cutoff = (Get-Date).AddDays(-14)
Get-ChildItem -Path $folderPath -File -Recurse |
    Where-Object { $_.LastWriteTime -lt $cutoff } |
    Remove-Item -Force -ErrorAction SilentlyContinue
```

This is SQL Agent executing PowerShell that deletes NopCommerce file system content. Confirms SQL Agent has `SUBSYSTEM:PowerShell` privileges and file-system write access to the NopCommerce folder.

### 6.6 Database Mail

**Not configured** — no profiles, no accounts, `Database Mail XPs = 0`. All email is sent via the application layer (NopCommerce built-in SMTP, AnqIntegrationApi, or Brevo API) — not via SQL Database Mail. Good — one less attack surface.

### 6.7 CLR assemblies

**None** — no user-defined CLR assemblies on any database. Custom code all lives in T-SQL procs or external apps.

### 6.8 Service Broker

Enabled per-DB:

| Database | Broker enabled |
|---|---|
| `Annique` | No |
| `Anq_Archive` | **Yes** |
| `Arc` | No |
| `BackOffice` | No |
| `Brevo` | **Yes** |
| `NewRegistration` | **Yes** |
| `Quizdb` | No |
| `Registrations` | No |
| `Rod` | **Yes** |
| `RodWork` | No |
| `Staging` | No |

Broker is enabled on `Brevo` and `NewRegistration` — could be using queued messaging internally. `Anq_Archive` and `Rod` likely have it on by accident (default when restoring a DB). Not yet investigated for queue/service definitions.

### 6.9 Daily backups

Via **Windows Server Backup** (same pattern as AMSERVER-TEST — VSS snapshot-style, GUID physical_device_name, not file paths). Plus **Veeam Agent** installed — possibly running as secondary backup.

Schedule: daily at **00:31** (just past midnight). All 11 user DBs + 3 system DBs backed up.

### 6.10 NopCommerce (`Annique`) database profile

- 226 tables, 72 procs, 5 views, 11 functions, 5 triggers
- **62 custom `ANQ_*` stored procedures** — the whole Annique integration layer
- Top tables by rows:
  - `OrderItem` — 5.3M rows
  - `GenericAttribute` — 3.1M rows / 594 MB (NopCommerce's key-value store; heaviest table)
  - `Address` — 2.3M rows
  - `Order` — 901K rows (**900K orders total**)
  - `Customer` — 219K rows
  - `Affiliate` — 102K rows (consultant base)
  - `ANQ_MetaCapiQueue` — 340K / 509 MB (Meta Conversions API queue)
  - `ANQ_BrevoLog` — 292K rows (Brevo call log)
  - `ANQ_CustomerChanges` — 574K rows (change log driving AM sync)

### 6.11 Triggers on `Annique` (non-MS)

| Table | Trigger | Enabled | Purpose |
|---|---|---|---|
| `ANQ_NewRegistrations` | `trg_ANQ_NewRegistrations_ParseBrowser` | Yes | Parse User-Agent |
| `ANQ_NewRegistrations` | `TRG_ANQ_NewRegistrations_PreventDuplicateEmail` | **DISABLED** | Duplicate check — turned off |
| `ANQ_NewRegistrations` | `trg_ANQ_NewRegistrations_SetDefaults` | Yes | Default values |
| `Customer` | `trg_Customer_Deactivate` | Yes | Recently added (2026-01-16); fires on Customer updates |
| `Customer_CustomerRole_Mapping` | `ANQ_trgCustomer_CustomerRole_Mapping` | Yes | Role change sync |

### 6.12 Procedures that call external systems

**25 procs in `Annique` reach outside SQL**:

**Outbound HTTP via OLE Automation** (`sp_OACreate MSXML2.ServerXMLHTTP`):
- `ANQ_HTTP` — the shared HTTP helper (source identical to `sp_ws_HTTP` on AMSERVER-TEST)
- `ANQ_AramexTrack` — Aramex courier tracking
- `ANQ_FastWayTrack` — FastWay courier tracking
- `ANQ_MetaCapi_SendEventFromFbclid` — Meta Conversions API
- `ANQ_MetaCapi_SendEventFromFbclid_Deprec` — older Meta version
- `MetaCapi_SendEventFromFbclid` — another variant (three Meta procs!)
- `ANQ_Payu_GetPendingOrderTransactionStates` — PayU pending check
- `ANQ_Payu_MarkOrderAsPaidViaApi` — PayU mark-as-paid
- `ANQ_Brevo_PushOrderItemTransactionalAttributes` — Brevo transactional attributes

**Linked-server queries against production AM (`[AMSERVER-V9]`)**:
- `ANQ_Brevo_AllConsultants`
- `ANQ_Brevo_AllCustomers`
- `ANQ_Brevo_dataClientsCustomers`
- `ANQ_CleanUpInactives`
- `ANQ_CustomerAddress_UPDATE`
- `ANQ_CustomerChanges_UPDATE`
- `ANQ_GET_Invno` (scalar function)
- `ANQ_GetOfferItems`
- `ANQ_OrderStatus`
- `ANQ_SkyNetTrack`
- `ANQ_SyncCustomerTOAM` (reads `[AMSERVER-V9].NopIntegration.dbo.NopFieldMapping` — there's a `NopIntegration` DB on prod AM server with field-mapping config)
- `ANQ_SyncGift`
- `ANQ_SyncImage` (source has `RETURN;` at top — deprecated, replaced by `.NET AnqImageSync.Console.exe`)
- `ANQ_SyncPep` (PEP pickup points)
- `ANQ_SyncPostnet` (PostNet pickup points)
- `ANQ_SyncPublished` (**uses BOTH `[AMSERVER-V9]` AND `[WEBSTORE]`** — reads AM `CampDetail/CampSku/icItem/Soxitems` and old webstore `exclusiveitems`)
- `ANQ_TrackOrder`
- `ANQ_vw_Category` (view using OPENQUERY)
- `ANQ_vw_ProductSummary` (view using OPENQUERY)

**WEBSTORE linked-server queries**:
- `ANQ_SyncAffiliateOLD` (deprecated)
- `ANQ_SyncEvents`
- `ANQ_SyncPublished` (also uses WEBSTORE)

**`xp_cmdshell` usage:**
- `ANQ_syncOrderStatus` — the order-status sync proc uses shell commands (needs investigation of what it runs)

### 6.13 The `ANQ_HTTP` pattern — same as AMSERVER-TEST

```sql
CREATE PROCEDURE [dbo].[ANQ_HTTP]
  @cURL nVarChar(max), @cpostData nVarChar(max)='',
  @cContentType CHAR(50)='application/json',
  @cretvalue char(32) output, @cresponse nVarChar(4000) output
AS
BEGIN
  DECLARE @authHeader NVARCHAR(64) = 'BASIC 0123456789ABCDEF0123456789ABCDEF';
  DECLARE @token INT, @ret INT, ...

  EXEC @ret = sp_OACreate 'MSXML2.ServerXMLHTTP', @token OUT;
  EXEC @ret = sp_OAMethod @token, 'open', NULL, 'POST', @cUrl, 'false';
  EXEC @ret = sp_OAMethod @token, 'setRequestHeader', NULL, 'Authentication', @authHeader;
  EXEC @ret = sp_OAMethod @token, 'setRequestHeader', NULL, 'Content-type', @cContentType;
  EXEC @ret = sp_OAMethod @token, 'send', NULL, @cPostData;
  EXEC @ret = sp_OAGetProperty @token, 'status', @status OUT;
  EXEC @ret = sp_OAGetProperty @token, 'responseText', @responseText OUT;
  select @cretvalue = @status, @cresponse = @responseText
  EXEC @ret = sp_OADestroy @token;
END
```

Observations:
- **Hardcoded default auth header**: `BASIC 0123...`. Real calls must be overriding this via the calling proc. Worth dumping actual recent calls (plan cache) to see what URLs and auth are actually flowing.
- **POST method only** — no GET support.
- **`'false'` = synchronous blocking call** — the entire SQL Agent job step blocks on the HTTP response.
- **No SSL verification, no timeout** (the `setTimeouts` line is commented out).
- **Response truncated at 4000 chars** — long responses will be clipped.

---

## 7. `nopintegration.annique.com` — the finally-identified endpoint

From AMSERVER-TEST discovery we found `sp_NOP_syncOrders` hardcoded to `https://nopintegration.annique.com/syncorders.api`. We didn't know what server served that domain.

**Now confirmed**: `nopintegration.annique.com` is an **IIS site on AZ-ANNIQUE-WEB** pointing at `C:\NopIntegration\web` — a **West Wind Web Connection VFP app**. Its app pool runs as **LocalSystem**.

So the integration flow for `sp_NOP_syncOrders` (running on production AM SQL at `196.3.178.122:62111`) is:

1. AM SQL executes `ANQ_HTTP` / `sp_ws_HTTP` using `sp_OACreate MSXML2.ServerXMLHTTP`
2. Makes a POST to `https://nopintegration.annique.com/syncorders.api`
3. DNS resolves to this Azure VM's public IP (`20.87.212.38`)
4. Traffic routes through Cloudflare (based on observed Cloudflare edge connections)
5. Lands at IIS on this VM
6. `nopintegration.exe` (VFP, running as LocalSystem) handles the request
7. The handler connects to local SQL (`20.87.212.38:63000 Annique`) and/or back to AM SQL (`196.3.178.122:62111`) — both observed in netstat
8. Response goes back through the same path

That's the complete loop — a cross-hemisphere integration cycle initiated by a SQL trigger/job on production AM, bouncing off an Azure-hosted VFP web app.

---

## 8. Integration Graph Summary

```
                   ┌─────────────────────────────────────────────┐
                   │          AZ-ANNIQUE-WEB (SPOF)              │
                   │  20.87.212.38  /  10.0.0.4                 │
                   │                                             │
                   │  IIS: annique.com (NopCommerce 4.60)        │
                   │  IIS: stage.annique.com (staging)           │
                   │  IIS: stage.annique.co.na (Namibia)         │
                   │  IIS: nopintegration.annique.com (VFP)      │
                   │  IIS: backoffice.annique.com (VFP)          │
                   │  IIS: quiz.annique.com                      │
                   │  IIS: /API (AnqIntegrationAPI .NET 9)       │
                   │                                             │
                   │  SQL 2019 (port 63000, internet-exposed)    │
                   │    Annique (8 GB), Brevo, BackOffice,       │
                   │    Staging, Registrations, Quizdb,          │
                   │    Arc (13 GB), Anq_Archive (6 GB),         │
                   │    Rod, RodWork (7.5 GB)                    │
                   │                                             │
                   │  Scheduled tasks:                           │
                   │    AnqImageSync every 10 min                │
                   │    BrevoContactSync daily                   │
                   │                                             │
                   │  SQL Agent: 21 jobs (5 min–monthly)         │
                   │  Apache Solr (8983)                         │
                   │  Rebex SFTP (2222, 2223)                    │
                   │  Veeam + Windows Server Backup              │
                   └──────────────────┬──────────────────────────┘
                                      │
                    ┌─────────────────┼─────────────────┐
                    ▼                 ▼                 ▼
   ┌────────────────────────┐  ┌──────────────┐  ┌─────────────────┐
   │  Production AM SQL     │  │  Brevo API   │  │  Meta Capi      │
   │  196.3.178.122:62111   │  │  (HTTPS)     │  │  (HTTPS)        │
   │  away2.annique.com     │  └──────────────┘  └─────────────────┘
   │  sa / AnniQu3S@        │
   │                        │  ┌──────────────┐  ┌─────────────────┐
   │  Called via:           │  │  PayU API    │  │  Courier APIs   │
   │  - [AMSERVER-V9]       │  │  (HTTPS)     │  │  Aramex/FastWay/│
   │    linked server       │  └──────────────┘  │  SkyNet         │
   │  - TCP direct from     │                    └─────────────────┘
   │    nopintegration.exe  │  ┌──────────────┐
   │  - TCP direct from     │  │  Let's       │  ┌─────────────────┐
   │    backoffice.exe      │  │  Encrypt     │  │  WhatsApp via   │
   │  - TCP direct from     │  │  (win-acme)  │  │  Brevo template │
   │    AnqImageSync        │  └──────────────┘  └─────────────────┘
   └────────────────────────┘
                    ▲
                    │
   ┌────────────────────────┐          ┌─────────────────┐
   │  [WEBSTORE] linked     │          │  Cloudflare     │
   │  stage.anniquestore.   │          │  (CDN / proxy)  │
   │  co.za:61023           │          │  — observed in  │
   │  (legacy old webstore) │          │    outbound     │
   └────────────────────────┘          └─────────────────┘
                                       ┌─────────────────┐
                                       │  Monitoring:    │
                                       │  New Relic,     │
                                       │  Fluent Bit,    │
                                       │  Datto RMM,     │
                                       │  ESET cloud     │
                                       └─────────────────┘
```

---

## 9. Security Findings

Ordered by severity:

1. **SQL Server exposed to the internet** — port 63000 allowed from `*` in NSG; `sa`/`Difficult1` is the credential; `xp_cmdshell` and OLE Automation both enabled. Critical.
2. **SQL Server 2019 RTM, never patched** — 6 years of SQL Server CUs/security patches missed.
3. **`NopIntegration` IIS app pool runs as `LocalSystem`** — a compromise of the VFP web handler becomes full OS compromise.
4. **Plaintext `sa` credentials in multiple `appsettings.json` and `.ini` files** scattered across `C:\Apps\`, `C:\NopIntegration\`, `C:\Backoffice\`. No Key Vault or DPAPI.
5. **RDP whitelist is 26 IPs** — unmanaged list with staff home IPs. Any churn = breakage or drift.
6. **ghosted RDP sessions from 2025** — `multiadministrator` 163 days idle, `amadmin`/`anqadmin` hours to days. Each holds a cached token + running programs. Changing any of their passwords would kill their scheduled tasks.
7. **Hardcoded default basic-auth header** (`BASIC 0123...`) in `ANQ_HTTP` — if a proc forgets to override, that header is sent.
8. **The `TRG_ANQ_NewRegistrations_PreventDuplicateEmail` trigger is disabled** — probably intentional due to ingestion pattern, but worth confirming no duplicates are slipping through.
9. **Daily backups use GUID VSS devices** — means restore requires Windows Server Backup (not SQL-native restore) unless Veeam captures SQL-consistent images separately.
10. **Single NIC, no redundancy** — single point of failure for networking.
11. **`reports` SQL login is sysadmin** — needs investigation; unknown purpose.

---

## 10. Implications for DBM + Staging Plan

### 10.1 The staging question (the original reason for this session)

**AMSERVER-TEST is not a staging environment for the same reasons as doc 31.** But now we have the full picture of what a real staging environment needs to replicate:

- The **NopCommerce stack** (Annique DB + app) with its 62 custom `ANQ_*` procs
- All **21 SQL Agent jobs** with their exact cadences
- The **two scheduled tasks** (BrevoContactSync + AnqImageSync)
- The **VFP Web Connection apps** (`nopintegration.exe`, `backoffice.exe`) because they are called by production AM's `sp_NOP_syncOrders`
- **Both linked servers** (to clones of AM and webstore) — or mocked equivalents
- **Solr indices**
- **Let's Encrypt / win-acme** for TLS (or just proxied through the staging cert)

### 10.2 What DBM replaces vs. what stays

Based on this picture, DBM's place in the architecture is:

**DBM replaces:**
- The 21 SQL Agent jobs doing cross-system sync via linked server + OLE HTTP — these become scheduled DBM workers
- `ANQ_HTTP` + all procs calling it — become typed .NET HTTP clients with Key Vault auth, proper retry, proper TLS
- `AnqImageSync` (the .NET 10 loop) — folds into DBM's product sync
- `BrevoContactSync` — folds into DBM's comms service
- Direct TCP from `nopintegration.exe` / `backoffice.exe` to production AM SQL — moves behind DBM's typed interfaces

**DBM leaves alone (at least in phase 1):**
- NopCommerce itself (the Annique DB)
- The VFP web handlers on `nopintegration.annique.com` and `backoffice.annique.com` (they're customer-facing APIs called by other systems)
- Solr
- win-acme
- New Relic / Datto / ESET / Veeam
- Rod's dev workflow

**Where staging lives:**
The infrastructure spec (`docs/spec/03_infrastructure.md`) already plans this: a separate Azure resource group (`rg-dbm-staging`) with its own VNet, a parity AM VM restored from prod, DBM app services, and an optional new NopCommerce instance (or an existing staging one). This discovery confirms the scope of what has to be reproduced.

### 10.3 Credential rotation scope before go-live

Every one of these credentials must be rotated or replaced with Key Vault secrets before DBM-era go-live:

| Where found | Credential |
|---|---|
| `ImageSync/appsettings.json` | `sa`/`AnniQu3S@` (AM) and `sa`/`Difficult1` (NopCommerce) |
| Every NISource / BackOffice `.ini` file | same `sa`/`AnniQu3S@` pattern |
| `BrevoContactSync/appsettings.json` | Brevo API key |
| AnqIntegrationAPI `appsettings.json` | JWT signing key + Brevo API key + DB passwords |
| Linked server `sa`→`sa` mappings | stored credentials; migrate to DBM service account |
| `ANQ_HTTP` hardcoded BASIC auth header | overridden per-call; review real values via plan cache |
| NSG RDP whitelist | trim to minimum |
| NSG SQL rule | **close 63000 from internet immediately**; DBM should use VNet integration or private endpoint |

### 10.4 The VM-move question (for AM migration)

The AM migration to Azure (ANN-24) originally assumed we'd move production AM onto a new VM in a DBM-managed VNet. But **this VM already exists in Azure** and the integration endpoints (`nopintegration.annique.com`, `backoffice.annique.com`) are already here. A cleaner long-term architecture would:

1. Split AZ-ANNIQUE-WEB into multiple VMs (web / integration / SQL / Rod-dev) — reduce SPOF
2. Move production AM SQL into Azure near this VM — removes the cross-internet hop that `ANQ_HTTP`, ImageSync, and linked-server queries currently traverse
3. Isolate Rod's dev environment from production

But that's post-DBM-go-live scope.

---

## 11. Evidence Base

All findings in this document were captured via `az vm run-command invoke` against AZ-ANNIQUE-WEB on 2026-04-18, using PowerShell-as-SYSTEM execution authenticated through our subscription Owner role. No agent installed, no RDP required for the discovery itself (though DieselBrook RDP access was independently established).

| Claim | How captured |
|---|---|
| Scheduled tasks | `Get-ScheduledTask`, `Get-ScheduledTaskInfo` |
| Services | `Get-WmiObject Win32_Service` |
| IIS sites | `Get-Website`, `Get-ChildItem IIS:\AppPools`, `Get-WebApplication` |
| NSG rules | `az network nsg rule list` |
| SQL inventory | `sqlcmd -S localhost,63000 -U sa -P Difficult1` |
| Integration proc analysis | `CHARINDEX` queries against `sys.sql_modules.definition` |
| Network state | `Get-NetTCPConnection`, `netstat -ano` |
| Installed software | HKLM `Uninstall` registry keys |
| Hosts file | direct read of `C:\Windows\System32\drivers\etc\hosts` |
| COM+ apps | `COMAdmin.COMAdminCatalog` COM object |

---

## 12. What this discovery did NOT cover (deferred)

- **IIS logs** — content not sampled (sites definitely serving traffic — `w3wp.exe` was active during discovery)
- **NopCommerce plugin inventory** — only the top-level was observed; full plugin list not enumerated
- **`AnqIntegrationAPI` exact endpoints** — app pool confirmed, appsettings.json content partially sampled in prior sessions (doc 30) but new endpoints may have been added since
- **The SFTP server's authorised keys / accounts** — Rebex config not yet inspected
- **Rod's `RodWork` database contents** — created 2026-02-06, 7.5 GB, contents unknown
- **Plan cache sampling** — to identify the actual URLs being called by `ANQ_HTTP` (where our hardcoded auth header is being overridden to what real values)
- **`reports` SQL login** — sysadmin-level but purpose unknown
- **`xp_cmdshell` usage** — `ANQ_syncOrderStatus` uses it; the specific command not yet extracted
- **Veeam backup destination** — where the backups actually land (Azure Backup vault? storage account? off-VM?)
- **Datto RMM policies** — what the RMM agent is allowed to do remotely
- **New Relic account / data exported** — where Annique's perf data lives

These are natural follow-up investigations with the same access we have now.
