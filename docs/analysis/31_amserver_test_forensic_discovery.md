# AMSERVER-TEST — Complete Forensic Discovery
## Dieselbrook — Internal Reference

**Date:** 2026-04-17
**Access method:** NordVPN (dedicated IP) → FortiClient SSL-VPN → RDP + SSH to `172.19.16.101`
**Access level:** Local Administrator + SQL Server sysadmin
**Status:** First direct forensic session on an Annique AM server

> ⚠️ **CONFIDENTIAL — DIESELBROOK INTERNAL ONLY**
> This document contains production credentials, server internals, and source code extracts
> obtained during authorised discovery. Do not share. Rotate credentials before DBM go-live.

---

## 1. Executive Summary

This is the first session where Dieselbrook has had direct SSH and full admin access to an Annique AccountMate server. The findings here substantially revise our understanding of the Annique estate:

**Top findings:**

1. **AMSERVER-TEST is not just a test AM server** — it's a full developer workstation running AM, plus live operational services (Email2SMS is polling inboxes every 5 minutes), plus IIS hosting two Web Connection applications, plus a mail server, plus an FTP server.
2. **Pricing oracle source extracted** — we now have the full source of `sp_camp_getSPprice` (campaign pricing) and `vsp_ic_getcontractprice` (contract pricing). This confirms the two-tier pricing architecture.
3. **The "unknown SQL server" 172.19.16.27 from doc 30 is identified** — it's `ITREPORT-SERVER\ANREPORTS`, a reporting server. One of Annique's config files still points at it for the live compensation plan database.
4. **New public endpoint discovered: `nopintegration.annique.com`** — this URL is hardcoded in `sp_NOP_syncOrders` and is not the same as `shopapi.annique.com` or the Azure VM-hosted API.
5. **Another domain: `anniquestore.co.za`** — stored as `ws.url` in `wsSetting` table.
6. **Dieselbrook SQL logins were pre-created on 2026-02-25** — 2 months before we got RDP access. Someone at Annique prepared the box for us then cleaned up 5 databases the day before.
7. **Very old operating environment** — 2009-era Xeon X5550 hardware, Windows Server 2008 R2 (out of support since Jan 2020), SQL Server 2014 (out of support since Jul 2024), .NET Framework < 4.5 (can't reach GitHub over TLS 1.2). Last reboot: **2024-02-01** (uptime 2+ years).
8. **Stale RDP sessions from 2024** still alive — wilbert idle 630 days, rodney 39 days, melinda 10 days — users just close their RDP windows rather than signing out.
9. **Most of `amanniquelive` is real data** — 380 GB used out of 609 GB allocated, not 609 GB of live data as the file sizes suggested.

---

## 2. Host Profile

### 2.1 Hardware

| Attribute | Value |
|---|---|
| Manufacturer / Model | Intel Corporation S5520HC (server-class, ~2009-2010 Nehalem era) |
| CPU | 2× Intel Xeon X5550 (4 cores each, 2.67 GHz, HT) = **8 cores / 16 logical processors** |
| RAM | 90 GB physical (94,426,398,720 bytes reported) |
| Storage RAID | Hardware RAID (RAID Web Console 2 installed) |
| Storage layout | C: 141 GB (82% used) / D: 571 GB (36% used, "AMDATA") / E: 913 GB (74% used) |
| NIC | Intel 82575EB Gigabit, single connection on 172.19.16.101 |

> **Critical:** C: drive is 82% full (25 GB free). Large drives D: and E: hold data.

### 2.2 Operating System

| Attribute | Value |
|---|---|
| OS | **Windows Server 2008 R2 Enterprise** (build 7601 SP1) |
| OS install date | 2018-09-17 |
| **OS extended support ended** | **2020-01-14** (over 6 years out of support) |
| .NET Framework | **< 4.5** (no TLS 1.2 support — cannot reach GitHub without workaround) |
| PowerShell | **2.0** (missing modern cmdlets like `Invoke-WebRequest`, `Add-WindowsCapability`, `New-NetFirewallRule`) |
| Windows Firewall | ON (Domain, Private, Public profiles) |
| Last boot | **2024-02-01 14:06** — uptime **2+ years**, no reboots |

### 2.3 Domain / Directory

| Attribute | Value |
|---|---|
| AD domain | `annique.local` |
| Domain controller | `andc.annique.local` (172.19.16.16) |
| This machine FQDN | `AMSERVER-TEST.annique.local` (172.19.16.101) |

### 2.4 Local Administrators (ANNIQUE domain accounts)

- `ANNIQUE\Administrator`
- `ANNIQUE\Dieselbrook` ← ours (added 2026-04-17)
- `ANNIQUE\ITSUPPORT`
- `ANNIQUE\Melinda`
- `ANNIQUE\rodney` ← Rod the developer
- **`ANNIQUE\Sean`** (new name, not previously known)
- **`ANNIQUE\Wilbert`** (new name, not previously known)
- Local `Administrator` account (enabled)
- Local `Guest` (disabled)

### 2.5 Current / Stale RDP Sessions

Users leave their RDP windows "disconnected" without signing out — sessions remain alive for years with all their programs still running:

| User | Session state | Idle time | Logged in |
|---|---|---|---|
| `wilbert` | Disconnected | **630+ days** | 2024-03-01 |
| `rodney` | Disconnected | 39+ days | 2024-02-01 (same day as last boot) |
| `melinda` | Disconnected | 10+ days | 2024-02-02 |
| `sean` | Disconnected | 9+ days | 2024-02-29 |
| `itsupport` | Disconnected | Today | 2026-04-17 14:01 |
| `dieselbrook` | Disconnected | Today | 2026-04-17 14:18 |

The 3 SSMS.exe instances, VFP9 IDE, and 6 Chrome processes we saw are from these ghosted sessions — running in Rodney's and others' detached sessions since 2024.

---

## 3. Installed Software (notable items)

### Server-role / operational software

- **AccountMate 9.3 for SQL** (multiple installations — different company databases)
- **hMailServer 5.6.8** — full SMTP/POP3/IMAP mail server (ports 25, 110, 143, 587)
- **Xlight FTP Server 3.9.3.2** — FTP server
- **IIS 10.0 Express** + **Microsoft IIS** (W3SVC, WAS services running)
- **Crystal Reports 10** + **SAP Crystal Reports runtime engine for .NET Framework**
- **RAID Web Console 2** (MegaRAID management)
- **ESET Server Security** (ekrn.exe running, 416 MB)
- **Malwarebytes 5.3.4** (double AV with ESET)
- **West Wind Web Connection 7** (`C:\wconnect7\`)
- **DAEMON Tools Lite** (virtual CD/DVD)

### Developer software (this is a dev workstation too)

- **Microsoft Visual FoxPro 9.0 Professional** — full IDE
- **Visual Studio Community 2019** — full IDE
- **.NET Core 3.1 / 5.0 SDKs** (with ASP.NET Core shared frameworks)
- **.NET Framework 4.8** (tool runtimes — but base framework is older)
- **Microsoft SQL Server 2014 Management Studio**
- **Microsoft Azure SDK** (Storage Emulator, Compute Emulator, Authoring Tools v2.9.6)
- **Entity Framework 6.2**
- **Google Chrome**
- **Greenshot** (screenshot tool)
- **Microsoft Web Deploy 4.0**, **IIS URL Rewrite 2**

---

## 4. AccountMate Installation

### 4.1 Install location and binaries

`C:\amsql\` contains AccountMate 9.3 for SQL:

- `amsql.exe` (6.5 MB main executable, dated **2015-07-10**)
- Module executables: `amsqladm.exe` (admin), `amsqlap.exe` (AP), `amsqlar.exe` (AR), `amsqlbr.exe` (bank recon), `amsqlcf.exe` (config), `amsqlcm.exe`, `amsqlco.exe`, `amsqldc.exe`, `amsqlgl.exe` (GL), `amsqlic.exe`, `amsqlmi.exe`, `amsqlpo.exe` (PO), `amsqlpr.exe`, `amsqlra.exe`, `amsqlrp.exe`, `amsqlso.exe` (SO), `amsqlts.exe`
- `amsql.zip` — 1.9 GB compressed (probably a backup of the entire install)
- `common.7z` — 9 MB compressed

All module binaries dated **2015-07-10** — AM 9.3 original release, never patched at binary level.

### 4.2 AccountMate Configuration

| File | Contents |
|---|---|
| `AMSETUP.AM` | `DBSERVER=AMSERVER-TEST` — this machine is its own DB server |
| `AMSETUP-test.AM` | Alternative setup (13 bytes) |
| `config.fpw` | `TITLE = AccountMate 9.3 for SQL`, standard FoxPro settings |
| `amsql.exe.config` | Minimal .NET config — only `loadFromRemoteSources=true` |

### 4.3 AM Licences

Two licence files confirm two company installations:

- `amms9_ANNI619265.lic` (2017-03-29) — **matches the production support number ANNI619265 we have on record**
- `amms9_ANHB760221.lic` (2017-03-29) — unknown company (ANHB = Annique Holdings?)

### 4.4 Backup copies within `C:\amsql\`

Multiple timestamped backup folders show careful manual version control over the years:
- `common` (current, 2021-12-13)
- `common - bu270722` (2022-07-27)
- `common - Copy` (2020-09-02)
- `Common_test` (2020-09-02)
- `common-AM903std` (2018 — AM 9.03 standard config)
- `common_BU04201716` (2018 — backup April 2017)
- `common_bu201611` (2019 — backup Nov 2016)
- `common_bu201704` (2018 — backup April 2017)

### 4.5 Sample/Demo Databases

- `sample.dbc/.dct/.dcx` (2018-11-09) — FoxPro database container
- `sample999.dbc/.dct/.dcx` (2017-03-31) — another demo

---

## 5. Operational Services Running on This Box

### 5.1 Email-to-SMS Service (ACTIVE)

`C:\Email2SMS\email2sms.exe` — FoxPro application.

**Scheduled Task:** runs **every 5 minutes, as `ANNIQUE\rodney`**. Last run 2026-04-17 17:21, next run 17:26.

Activity:
- `sms.log` last modified 2025-05-09 (service wrote to log ~11 months ago — may have stopped producing output)
- `pop.prg` / `pop.FXP` — POP3 client that fetches email and forwards as SMS
- `smsclass.prg` — SMS sender class

**Implication:** This box is polling an email inbox every 5 minutes for SMS relay. Unknown what inbox, what SMS provider, or whether anything is actually flowing. Needs investigation.

### 5.2 EFT Processing

`C:\Eft\`:
- `arproc.PRG` (2019-09-18) — AR processor
- `NED954.csv` (2025-05-06) — Nedbank EFT file, recent
- `NED1000000005.ach` (2021-04-16) — NACHA format EFT file

**Implication:** This box processes EFT (Electronic Funds Transfer) files for Nedbank. Last active May 2025.

### 5.3 Compensation Plan (MLM)

`C:\CompPLan\`:
- `compruns.exe` (2023-05-26, 2.3 MB) — the main compensation calculation engine
- `compruns - Copy.exe` (2021-08-03) — older copy
- `comprunmanual.exe` (2018-07-31) — manual run tool
- `compplanLIVE.log` (2025-05-26, 25 KB) — active logs for Live environment
- `compplanNAM.log` (2024-02-26, 3 KB) — Namibia logs
- **`Compplan.ini`** connects to `AMSERVER-TEST` with `sa`/`go` (weak password!) at database `compplanlive`
- **`Mlm.ini`** connects to `ITREPORT-SERVER\ANREPORTS` with `sa`/`AnniQu3S@` at database `compplan`
- Uses .NET interop: `Newtonsoft.Json.dll`, `Renci.SshNet.dll`, `wwDotNetBridge.dll`

**Note:** The `compplanlive` database is no longer on AMSERVER-TEST (removed 2026-02-24). The Compplan.ini is pointing at a database that doesn't exist on this server anymore — so CompPlan can't run here until reconfigured. The Mlm.ini pointing at ITREPORT-SERVER is more current.

### 5.4 Mail Server (hMailServer)

Running on ports 25 (SMTP), 110 (POP3), 143 (IMAP), 587 (SMTP submission). Configuration not yet inspected — needs Admin Tools check.

### 5.5 FTP Server (Xlight)

Running (version 3.9.3.2). Endpoints not yet inspected.

---

## 6. IIS Web Applications

### 6.1 IIS Site Configuration (from `applicationHost.config`)

Single site: **Default Web Site**

Bindings:
- HTTP: `172.19.16.101:8088` (bound to specific IP, NOT port 80)
- HTTPS: `*:8443`
- net.tcp:808, net.pipe:*, net.msmq:localhost (WCF bindings)

Applications:
- `/` → `C:\inetpub\wwwroot` (default iisstart page, no real content)
- `/crystalreportviewers12` → `C:\Program Files (x86)\Business Objects\Common\4.0\crystalreportviewers12` (Crystal Reports viewer)
- `/wconnect` → `C:\wconnect7\web` (West Wind Web Connection demo)
- **`/backofficeapi` → `E:\Projects\BackofficeAPI\web`** ← **BackOffice API**
- **`/Webstore` → `C:\WebConnectionProjects\Webstore\Web`** ← **Webstore integration**

App pools: ASP.NET v4.0, ASP.NET v4.0 Classic, Classic .NET AppPool, DefaultAppPool, **`West Wind Web Connection`**

### 6.2 BackofficeAPI project (`E:\Projects\BackofficeAPI\`)

Complete West Wind Web Connection project:
- `data/` — project data
- `deploy/` — compiled .exe (last deployed 2023-01-06)
- `web/` — IIS public root
- `WebConnectionWebServer/` — WC server config
- `build.ps1`, `Build.bat` — build scripts (dated 2020-03)
- `.webconnection`, `.gitignore` — git markers (no .git folder visible)
- `Install-IIS-Features.ps1` — IIS setup script

### 6.3 Webstore project (`C:\WebConnectionProjects\Webstore\`)

Another complete Web Connection project. Last active development: **2022-10-31**.

Deploy folder:
- `webstore.exe` (753 KB, 2022-09-16) — compiled FoxPro executable
- `Webstore.PJX/.PJT` — FoxPro project file
- `Webstore.ini`, `Webstore.VBR`, `Webstore.tlb` — COM interface metadata
- Source .prg files: `WebstoreMain.prg`, `StoreProcess.prg`, `Launch.prg`, `bld_Webstore.prg`, `Webstore_ServerConfig.prg`
- .NET interop: `renci.sshnet.dll`, `wwdotnetbridge.dll`

Web folder contains **multiple dated snapshots** (developer habit):
- `index.ws`, `index.ws09092022`, `index.ws16092022`, `index.ws101022` — manual dated backups
- `BeautySpa/` — sub-project (2022-10-11)
- Standard web assets: `views/`, `js/`, `css/`, `img/`, `assets/`, `icons/`
- `web.config`, `web.aspnet.config`, `web.dotnetcore.config` — IIS configs

**Not clear yet which of these corresponds to `shopapi.annique.com` in production.** The `shopapi.annique.com` server runs at `129.232.215.87` (per doc 30), a different IP. The Webstore project here may be a newer version or a separate integration.

### 6.4 E:\Projects (source repository)

| Project | State |
|---|---|
| `Anniquestore` | Minimal stub (deploy/, web/) — placeholder |
| `Backoffice` | Minimal stub (deploy/, web/) — placeholder |
| **`BackofficeAPI`** | **Full project** — linked to IIS |
| `Compplan` | Empty (only data/) — placeholder |
| **`ShopApi`** | **EMPTY** — 0 files. Created 2022-07-26 as placeholder. |
| `vfptools` | Tool folder |
| `WebConnection-7.20.exe` | Installer archive |

**⚠️ The `ShopApi` folder is empty.** The source for `shopapi.annique.com` is NOT on this box — at least not in this expected location.

---

## 7. SQL Server Configuration

### 7.1 Server Identity

| Attribute | Value |
|---|---|
| `@@SERVERNAME` | `AMSERVER-TEST` |
| Version | SQL Server 2014 SP3 (12.0.6024.0) |
| Edition | Standard Edition (64-bit) |
| Host | `AMSERVER-TEST.annique.local` (172.19.16.101) |
| Clustered | No |
| Port | 1433 (default) |
| Listening | all interfaces (0.0.0.0:1433 AND [::]:1433) — internet-addressable via firewall rules |
| **SQL extended support ended** | **2024-07-09** (out of support) |

### 7.2 Memory / Parallelism Configuration

| Setting | Value | Note |
|---|---|---|
| `max server memory (MB)` | 2,147,483,647 | **Default (unlimited)** — SQL can use all 90GB |
| `min server memory (MB)` | 0 | Default |
| `max degree of parallelism` | 0 | Default |
| `cost threshold for parallelism` | 5 | Default |
| `clr enabled` | 0 | CLR disabled |
| **`xp_cmdshell`** | **1** | **ENABLED — security risk!** |

### 7.3 Linked Servers

Single linked server: **`AMSERVER-V9`**
- Provider: SQLNCLI11 (SQL Server Native Client 11)
- Data source: `AMSERVER-v9` (hostname — not yet resolved; may be in hosts file or DNS)
- `is_remote_login_enabled = 1`, `is_rpc_out_enabled = 1`
- Login mapping: local `sa` → remote `sa` (with stored credential)

This is the same `[AMSERVER-V9]` linked server alias pattern we see in all the VFP source code. On the production Azure VM it resolves to `away2.annique.com:62111`. On this test box, the alias is defined but what it actually resolves to here is unknown — could be this machine itself, or could be an unused/broken pointer.

### 7.4 SQL Logins (non-system)

| Login | Type | Created | Notes |
|---|---|---|---|
| `amlogin` | SQL | 2019-12-12 | AM application login |
| **`DieselBroek`** | SQL | **2026-02-25 12:10** | Pre-created for DBM access (with typo: Dutch "Broek") |
| **`DieselBrook`** | SQL | **2026-02-25 13:59** | Corrected spelling, default DB `AmAnniqueLive` |
| `sa` | SQL | 2003-04-08 | Standard |
| `ANNIQUE\Dieselbrook` | Windows | 2026-04-17 16:49 | Our login, added today via single-user mode |
| `ANNIQUE\rodney` | Windows | 2019-12-12 | Rod's login |

**Key finding:** Dieselbrook SQL logins were **pre-created on 2026-02-25** — 7 weeks before we got RDP access (2026-04-17). Same day, the 5 databases (`compplanLive`, `compplanNam`, `CompSys`, `backofficeapi`, `sample999`) were removed. Something coordinated happened 2026-02-24/25 that prepared this box for Dieselbrook access.

### 7.5 Privileged Roles (sysadmin members)

- `ANNIQUE\Dieselbrook` — ours
- `ANNIQUE\rodney` — Rod (since 2019)
- `sa`
- NT Service accounts (expected: MSSQLSERVER, SQLSERVERAGENT, SQLWriter, Winmgmt)

Notably absent: `DieselBrook`/`DieselBroek` SQL logins are NOT sysadmins — they have restricted access (intended for DBM application use with least-privilege).

### 7.6 SQL Agent Jobs

Only one custom job: `syspolicy_purge_history` (SQL default maintenance job).

**There are NO custom SQL Agent jobs running on this box.** The daily backups come from Windows Server Backup (see §9.1), not SQL Agent.

---

## 8. Database Inventory

### 8.1 Current Databases

| Database | Compat Level | Recovery | Allocated | Actual Used | State |
|---|---|---|---|---|---|
| `amanniquelive` | **100** (SQL 2008) | SIMPLE | 609 GB data + 15 GB log | **380 GB data + 12 MB log** | ONLINE (main AM DB) |
| `amwsys` | 100 | SIMPLE | 352 MB data + 150 MB log | 298 MB + 3 MB | ONLINE (AM system DB — licences, companies, users) |
| `wh` | 120 | FULL | 17 GB data + 17 GB log | 17 GB + 1.3 GB | ONLINE (single-table warehouse DB, 22M rows) |
| `rod` | 120 | FULL | 16 GB data + 17 GB log | **2.8 MB + 529 MB** | ONLINE (Rod's working DB — has ONE table `wd`, empty) |

Oversized file allocations — especially `rod` (16 GB for 2.8 MB of data, 99.98% empty).

**`amanniquelive` is ~380 GB of real data**, not 609 GB — the 229 GB difference is pre-allocated empty space in `am_user_data.ndf`.

### 8.2 Removed Databases (historical — backup records only)

Six databases were **removed on 2026-02-24** (all last backed up same day, 21:00:32):

| Database | Last backup date | Purpose |
|---|---|---|
| `compplanLive` | 2026-02-24 | MLM compensation plan — moved to ITREPORT-SERVER |
| `compplanNam` | 2026-02-24 | Namibia compensation plan |
| `CompSys` | 2026-02-24 | Communications system |
| `backofficeapi` | 2026-02-24 | BackOffice API working DB |
| `sample999` | 2026-02-24 | AccountMate demo |
| `compplanLiveTest` | 2024-05-02 | Older test DB |

**The same day those DBs were removed, Dieselbrook SQL logins were created.** This strongly suggests the box was prepared for DBM access — they cleaned up non-essential DBs and set up our logins ahead of giving us RDP.

### 8.3 amanniquelive — schema scale

| Object type | Count |
|---|---|
| Tables | **914** |
| Procedures | **968** |
| Views | **1,743** |
| Functions | 75 |

This is a full production-scale AM database, not a trimmed test copy.

### 8.4 Top 10 tables by row count

| Table | Rows | Size | Purpose |
|---|---|---|---|
| `iciwhs_daily` | 130,000,000 | 101 GB | IC warehouse daily history |
| `gltrsn` | 118,000,000 | 39 GB | GL transactions |
| `aritrsh` | 21,600,000 | 16.7 GB | AR transaction history |
| `genreb` | 19,900,000 | 2.4 GB | Rebate data |
| `ictrsnh` | 17,500,000 | 4.9 GB | IC transaction history |
| `iciwhs_monthly` | 15,700,000 | 12.3 GB | IC warehouse monthly |
| `sosptrh` | 11,900,000 | 6.4 GB | SO transaction header history |
| `sostrsh` | 11,900,000 | 9.4 GB | SO transactions history |
| `arsktrh` | 8,420,000 | 1.9 GB | AR stock transaction history |
| `arinvch` | 5,830,000 | 11.6 GB | Invoice history |

Other notable:
- `mlmcust` — 3 million MLM customers (consultants including inactive)
- `SOPortal` — 646,237 rows (half a million orders tracked through portal status)
- `Campaign` — 77 (monthly campaigns since ~2022 or earlier)
- `CampDetail` — 18,371 (campaign price details)
- `CampSku` — 14,197 (campaign SKUs)

### 8.5 Recently modified stored procedures (top 15)

| Procedure | Last modified | Purpose |
|---|---|---|
| `sp_NOP_syncOrders` | **2026-01-09** | NopCommerce order sync |
| `sp_Camp_GetCamp` | 2025-12-15 | Campaign lookup |
| `sp_TradeGroupBuild` | 2025-12-01 | **Trade groups (new concept, created Nov 2025)** |
| `sp_ws_reactivate` | 2025-11-24 | Web service — reactivate |
| `sp_ws_getAvailability_CodeNEW` | 2025-11-24 | Web service — availability |
| `sp_WS_GetAvailability` | 2025-10-31 | Web service — availability |
| `NOP_UpdateExclusiveItemCode` | 2025-09-29 | NopCommerce — exclusive items |
| `sp_ws_ordersLoadAllPaged` | 2025-08-29 | Web service — orders paging |
| `sp_Camp_GetItemLookups` | 2025-08-15 | Campaign item lookups |
| `sp_dl_icitem_INS` | 2025-08-14 | IC item insert |

**NopCommerce integration is still being actively developed** as of January 2026 (3 months ago). New concept "Trade Groups" introduced Nov 2025.

---

## 9. Pricing Oracle — Full Source Extracted

### 9.1 `sp_camp_getSPprice` — campaign price (promotional / public)

Full source:

```sql
/** ID: IC90025.01 Name: vsp_ic_getprice Owner: AM ScriptDate: 03/07/2014 **/
CREATE procedure [dbo].[sp_camp_getSPprice]
  @ccustno char(10),                       -- customer (NOT USED!)
  @citemno char(20),                        -- item number
  @dorder datetime,                          -- order date
  @cretuid CHAR(15) output,                 -- returned CampDetail.cuid
  @nretvalue numeric(16, 4) = 0 output,    -- ex-VAT price
  @nretvalinctx numeric(16,4) = 0 output    -- inc-VAT price
as
begin
  declare @lnretvalue numeric(16,4), @lnretvalinctx numeric(16,4),
          @dfrom datetime, @dto datetime, @cId INT

  -- Step 1: find the campaign active on the order date
  select @cId=ID, @dfrom=dfrom, @dto=dto
  from Campaign
  where CAST(@dorder AS DATE) BETWEEN dfrom and dto

  -- Step 2: join icitem → CampSku → CampDetail; take CampDetail price if exists else icitem standard price
  Select @cretuid = d.cuid,
         @nretvalue = ISNULL(d.nprice, i.nprice),
         @nretvalinctx = ISNULL(d.nrsp, i.nprcinctx)
  FROM icitem i
       LEFT OUTER JOIN CampSku s   on s.campaignid=@cID and i.citemno=s.citemno
       LEFT OUTER JOIN CampDetail d on d.campaignid=@cID and d.campskuid=s.id
                                     AND @dorder BETWEEN d.dfrom and d.dto
                                     AND d.linactive=0
  WHERE i.citemno=@citemno
end
```

**Key insights:**

1. `@ccustno` parameter is received but **never used** in the function body. Campaign pricing is **customer-agnostic** (same price for everyone, at item × date level).
2. Returns both ex-VAT (`nretvalue`) and inc-VAT (`nretvalinctx`) prices from `CampDetail.nprice/nrsp`, falling back to `icitem.nprice/nprcinctx`.
3. Returns a `cretuid` — this is `CampDetail.cuid`, a 15-char unique ID identifying which campaign row was used (for audit/tracing).
4. `d.linactive=0` filter — only active campaign entries considered.
5. Header comment says "Name: vsp_ic_getprice" — **the procedure was cloned from an older one** (version IC90025.01 suggests 90025 revision 01 of AM build 9.0).

### 9.2 `vsp_ic_getcontractprice` — contract price (customer-specific)

Full source:

```sql
/** ID: IC90019.01 Name: vsp_ic_getcontractprice Owner: AM ScriptDate: 03/07/2014 **/
create procedure vsp_ic_getcontractprice
  @ccustno char(10),                       -- CUSTOMER (used!)
  @citemno char(20),                        -- item number
  @cmeasure char(10),                        -- unit of measure
  @dorder datetime,                          -- order date
  @nretvalue numeric(16,4) = 0 output,
  @nretvalinctx numeric(16,4) = 0 output
as
begin
  declare @ldeffective datetime, @lnprice numeric(16,4), @lnprcinctx numeric(16,4),
          @lnnewprice numeric(16,4), @lnnprcinctx numeric(16,4)

  -- Look up contract price for this specific customer × item × measure
  select @ldeffective = deffective, @lnprice = nprice, @lnprcinctx = nprcinctx,
         @lnnewprice = nnewprice, @lnnprcinctx = nnprcinctx
  from iccprc
  where ccustno = @ccustno and citemno = @citemno and cmeasure = @cmeasure and lactive = 1

  -- Use future "new price" if effective date has arrived, else current price
  if @ldeffective is null or @ldeffective > @dorder
    select @nretvalue = @lnprice, @nretvalinctx = @lnprcinctx
  else
    select @nretvalue = @lnnewprice, @nretvalinctx = @lnnprcinctx
end
```

**Key insights:**

1. **Customer-specific pricing** — `iccprc` table (Item Customer Contract Price) stores per-customer-per-item-per-measure contract prices.
2. Supports **price changes with effective dates** — `deffective` gives the date at which `nnewprice` takes over from `nprice`.
3. Inactive contracts filtered out via `lactive = 1`.
4. This is **where consultant-tier/wholesale pricing lives** — per-customer overrides.

### 9.3 Combined pricing architecture

Orchestration likely works this way (not yet verified by inspecting calling code):

1. **Contract price** (`vsp_ic_getcontractprice`) — if the customer has a contract row in `iccprc`, that takes precedence.
2. **Campaign price** (`sp_camp_getSPprice`) — if no contract exists, apply active campaign pricing.
3. **Standard item price** (`icitem.nprice`) — if no campaign, use item master price.

Need to find the orchestrator procedure(s) to confirm this precedence.

### 9.4 Campaign tables — column details

**`Campaign`** (77 rows): `ID`, `iYear`, `iMonth`, `cMonthName`, `dFrom`, `dTo`, `nTarget`, `nGPTarget`, `cStatus`, `cCampno`, `nActualSales`, `nActualCost`, `LastUser`, `dLastUpdate`, `nDiscrate`, `nMlmrate`, `nActualDisc`, `nActualMLM`, `vatrate`

**`CampDetail`** (18,371 rows): `ID`, `CampaignID`, `CampSkuID`, `cItemno`, **`cSponCat`** (sponsor category), **`cSponType`** (sponsor type), `dFrom`, `dTo`, `nPrice`, `nDiscRate`, `nRsp`, `nForeCast`, `nQtyLimit`, `RRPoints`, `cOffer`, `cPageNo`, `nActualSales/Cost/units`, `cuid`, `lGWP` (gift with purchase), `nGQtyLimit`, `cGftType`, `nMinSales`, **`lInactive`**, `LastUser`, `dLastUpdate`, `nActualDisc`, `nActualMLM`

**Note:** `CampDetail` has `cSponCat` and `cSponType` columns for sponsor-specific pricing, but `sp_camp_getSPprice` does NOT filter by them. Either (a) there's a separate procedure for sponsor-aware campaign pricing, or (b) only one CampDetail row per CampSku is used and sponsor differentiation happens elsewhere. Needs further investigation.

**`SOPortal`** (646,237 rows): `csono` (char 10), `dcreate` (datetime), `dprinted` (datetime), `cStatus` (char 1). Recent orders have `cStatus = NULL` (not `' '`). This is a light indexing/queue table sitting alongside the main AM sales order tables.

---

## 10. Integration Endpoints Discovered

### 10.1 NopCommerce integration URL — NEW

`sp_NOP_syncOrders` hardcodes the URL:

```
https://nopintegration.annique.com/syncorders.api
```

**This is a new domain we had not confirmed before.** It's distinct from:
- `shopapi.annique.com` (NISource VFP server at 129.232.215.87)
- `annique.com` (main e-commerce site)
- The Azure VM (AZ-ANNIQUE-WEB at 20.87.212.38)

The procedure also references a `wsSetting` table key `ws.url` but overrides it with the hardcoded value — legacy code pattern. **We should investigate `nopintegration.annique.com` separately** — could be a CNAME pointing at AZ-ANNIQUE-WEB or somewhere else.

### 10.2 `wsSetting` table values

| Name | Value |
|---|---|
| `ws.url` | `https://anniquestore.co.za/` |
| `ws.warehouse` | `4400` |

**Another new domain — `anniquestore.co.za`** — different from `annique.com` and `nopintegration.annique.com`. This is the legacy "web service URL" in AM config, possibly unused now since procs hardcode their own URLs.

### 10.3 ITREPORT-SERVER (mystery solved)

Our earlier doc 30 flagged `172.19.16.27` as "Unknown SQL Server — not investigated". Now resolved:

- **`ITREPORT-SERVER.annique.local` → 172.19.16.27**
- Runs SQL Server instance `ANREPORTS`
- Hosts a `compplan` database (the production MLM compensation database?)
- Credentials from `Mlm.ini`: `sa` / `AnniQu3S@`

This is **a dedicated reporting server**. When the 5 databases were removed from AMSERVER-TEST on 2026-02-24, some of them (at least `compplan`) were likely migrated to ITREPORT-SERVER.

---

## 11. Network & Connectivity

### 11.1 Network configuration

- Single NIC: 172.19.16.101/24 on `annique.local` domain
- Default gateway: `172.19.16.252` (FortiGate, based on prior discovery)
- DNS: `172.19.16.16` (andc AD DC), `196.22.218.248` (external)
- No VPN or tunnel adapters — only the single LAN interface

### 11.2 Listening ports (outbound services)

| Port | Service | Process |
|---|---|---|
| 22 | OpenSSH (us) | sshd.exe |
| **25** | SMTP | hMailServer |
| **110** | POP3 | hMailServer |
| 135 | RPC | svchost |
| **143** | IMAP | hMailServer |
| 445 | SMB | System |
| **587** | SMTP submission | hMailServer |
| **1433** | SQL Server (default) | sqlservr |
| 1688 | KMS licensing | |
| 3389 | RDP | TermService |
| **8088** | HTTP (IIS, bound to 172.19.16.101 specifically) | System |
| **8443** | HTTPS (IIS) | System |
| Various 49xxx | Dynamic RPC | |

### 11.3 Active outbound connections observed

- `91.228.165.144:8883` — ESET cloud telemetry (MQTT-SSL)
- `172.217.170.138:443` — Google (from ghosted Chrome sessions)
- `172.19.16.63:135` + `:6010` — **two processes connected to `172.19.16.63`** — this machine is not yet identified. Likely another AD-integrated server (port 135 is RPC endpoint mapper, 6010 is often DCOM). To be investigated.

### 11.4 ODBC DSNs

Only driver defaults and sample databases (Crystal Reports Xtreme, Visual FoxPro). **No production DSNs defined.** All AM apps use embedded connection strings in code or INI files, not ODBC DSNs.

---

## 12. Backups

### 12.1 Daily backup mechanism

**Scheduled Task:** `\Microsoft\Windows\Backup\Microsoft-Windows-WindowsBackup`

```cmd
%windir%\System32\wbadmin.exe start backup -templateId:{7ebb4591-9c90-453e-b961-1607a5656f03} -quiet
```

- Runs daily at **21:00** as SYSTEM
- Uses **Windows Server Backup** (built-in Windows feature)
- Uses **VSS snapshots** — that's why `msdb.dbo.backupset.physical_device_name` shows VSS GUIDs (e.g., `{E254B973-C3C0-42B0-88C7-D0A520521761}`) instead of file paths
- Destination: not visible in sysjobs or in typical backup folders — likely configured in Windows Server Backup console (external volume, network share, etc.)

### 12.2 Backup history observed

All current DBs have daily backups through 2026-04-16 21:00 (yesterday):
- `amanniquelive` — 380 GB backup
- `amwsys` — 298 MB backup
- `wh` — 17 GB backup
- `rod` — 18 MB backup
- Plus system DBs (master, model, msdb) — small

### 12.3 Manual backup file present

`E:\SQLBACKUP\wh.bak` — 18 GB, dated 2026-03-06. This is a manual backup of the `wh` database, probably for export/migration.

---

## 13. Security Posture

### 13.1 Surface area

- Running as dev workstation with **admin interactive sessions active 2+ years**
- **Windows Firewall ON** (all profiles) — good baseline
- **ESET Server Security + Malwarebytes** — double AV
- **OS and SQL out of vendor support** — no security patches since 2020 (OS) / 2024 (SQL)
- **xp_cmdshell enabled in SQL** — OS command execution from SQL with sa access
- **Multiple plaintext `sa` passwords in config files** (Compplan.ini: `go`, Mlm.ini: `AnniQu3S@`)
- **Linked server uses stored `sa` credentials** between servers
- **Remote Desktop group has 7 members** (IT team + consultants + developers)

### 13.2 Stale / abandoned sessions

Four user sessions from 2024 still active on the box (wilbert 630 days, rodney 39, melinda 10, sean 9). Each holds memory, open SSMS/VFP instances, Chrome tabs, and remains a persistence surface. Someone with the right access could enumerate what Rodney was working on in Feb 2024 by just viewing his orphan session.

### 13.3 Access paths (remote)

- **RDP (port 3389)** — all members of `Remote Desktop Users` group
- **SMB (port 445)** — file shares and admin shares (C$ etc.)
- **WinRM (port 47001)**
- **SSH (port 22)** — newly added today, LAN-only firewall rule (172.19.16.0/24)
- **IIS (port 8088, 8443)** — Web Connection apps
- **SQL Server (port 1433)** — standard port, listening on all interfaces (firewall-gated)

### 13.4 Credentials observed during discovery

| System | Login | Password | Where found |
|---|---|---|---|
| SQL `AMSERVER-TEST` | `sa` | unknown (not `Difficult1` or `AnniQu3S@`) | Required single-user mode workaround |
| SQL (via CompPlan) | `sa` | `go` | `C:\CompPLan\Compplan.ini` — points at this server |
| SQL (via MLM) | `sa` | `AnniQu3S@` | `C:\CompPLan\Mlm.ini` — points at ITREPORT-SERVER |
| Windows | `ANNIQUE\Dieselbrook` | `Diesel@2026#7` | RDP cred we were given |

---

## 14. Mystery Machines (still to investigate)

| IP / hostname | What we know | What's still unknown |
|---|---|---|
| **172.19.16.63** | Two processes on this box talk to it over RPC and DCOM | Hostname, role |
| **172.19.16.16** | AD domain controller `andc.annique.local` | — |
| **172.19.16.27** | `ITREPORT-SERVER\ANREPORTS` — reporting SQL server | Full database inventory; is `compplanlive` here now? |
| **172.19.16.100** | AM application server `AMSERVER-v9` (prod thick client host) | Our access to production box |
| **172.19.16.252** | FortiGate (default gateway) | — |

The linked server `AMSERVER-V9` on this test box points at the hostname `AMSERVER-v9`. Needs resolution check on this box to confirm where it actually resolves (hosts file, DNS alias, or internet hostname).

---

## 15. Implications for DBM Project

### 15.1 Positive findings

1. **Pricing oracle source extracted** — DBM can now faithfully implement the campaign + contract pricing logic with known precedence rules. Previous work was based on analysis; now we have the code.
2. **Test database with real schema** — AMSERVER-TEST's `amanniquelive` has the full 914-table schema. DBM integration testing can run against this box without needing a separate test AM SQL.
3. **Campaign tables have VAT rate and sponsor category columns** — these need to be surfaced through DBM's pricing API.
4. **Orders are ingested via `SOPortal`** — confirmed schema and high row volume. DBM's order ingestion path is well-understood.
5. **NopCommerce sync procedure is recently active** — the production integration pattern is still being maintained (`sp_NOP_syncOrders` modified Jan 2026). DBM should not assume these procs are frozen.

### 15.2 New concerns / open items

1. **`nopintegration.annique.com`** — new public endpoint not previously mapped. Need to identify the server behind it. This may be hosted on AZ-ANNIQUE-WEB (we noted the production Azure VM runs NopCommerce at port 63000 SQL, IIS on 80/443) or on a different machine.
2. **`anniquestore.co.za`** — another domain; confirm if used.
3. **ITREPORT-SERVER (172.19.16.27) access** — we should consider asking for access to verify what's there; `compplanlive` database may have moved here.
4. **Sponsor-tier pricing orchestration** — the campaign procedure doesn't filter by `cSponCat/cSponType`, so the actual price-selection logic that considers consultant tier must live elsewhere (in AM application code, in a different SP, or via pre-computed joins). DBM needs to understand this to reproduce pricing correctly.
5. **Row volume for migration planning:**
   - `iciwhs_daily` at 130M rows / 100 GB is likely the biggest migration challenge
   - `gltrsn` at 118M / 39 GB second-biggest
   - Total database is **380 GB real data, 609 GB allocated** — lift-and-shift takes a real plan for transfer time

### 15.3 Security items to raise (not DBM scope, but worth flagging)

1. OS and SQL out of support — patches unavailable
2. `xp_cmdshell` enabled
3. Stale RDP sessions from 2024
4. Plaintext `sa` passwords in INI files
5. `sa` used for linked server auth
6. `rod` database over-allocated (16 GB allocated for 2.8 MB used — waste of premium SSD)

### 15.4 What this means for AM Azure migration (ANN-24)

Now that we have direct access to a real Annique AM SQL, the migration plan can be more concrete:

- Confirmed SQL Server 2014 Standard running on Windows 2008 R2
- Compat level 100 = easy lift to SQL 2022 (SQL 2022 supports 100–160)
- ~380 GB actual data volume on the production clone
- VSS-based backups mean `wbadmin`-style backup is the operational pattern; migration plan should use SQL-native backup (`BACKUP DATABASE ... WITH COMPRESSION`) rather than VSS for the actual move
- Likely multi-hour restore window for 380 GB on Azure Premium SSD

---

## 16. Comparison with Production (from doc 30)

| Attribute | Production (doc 30) | AMSERVER-TEST (this doc) |
|---|---|---|
| Hostname | `AMSERVER-v9` (linked server alias) | `AMSERVER-TEST` |
| IP | `196.3.178.122:62111` (internet-accessible via `away2.annique.com`) | `172.19.16.101:1433` (LAN only) |
| OS | Unknown — to confirm | Windows Server 2008 R2 SP1 |
| SQL version | Unknown — to confirm | SQL Server 2014 SP3 Standard |
| Licence | `ANNI619265` | `ANNI619265` same + `ANHB760221` |
| `amanniquelive` size | Unknown | **380 GB used** |
| Linked server | — | `AMSERVER-V9` → `AMSERVER-v9` hostname |
| Backups | Unknown | Windows Server Backup, daily 21:00 |

**Strong hypothesis:** production (`AMSERVER-v9` at `196.3.178.122:62111`) is likely the same or very similar hardware/OS/SQL configuration. Migration effort is probably comparable.

---

## 17. Evidence Base

| Artefact | Source |
|---|---|
| OS version | `Get-WmiObject Win32_OperatingSystem` |
| Hardware | `Get-WmiObject Win32_ComputerSystem`, `Win32_Processor`, `Win32_LogicalDisk` |
| Installed software | HKLM uninstall registry + `Win32_Product` |
| Running services | `Win32_Service` |
| Running processes | `Get-Process` top memory |
| User accounts | `net localgroup`, `Win32_UserAccount` |
| Sessions | `query session`, `query user` |
| Listening ports | `netstat -ano \| findstr LISTENING` |
| Outbound connections | `netstat -ano \| findstr ESTABLISHED` |
| IIS sites | `applicationHost.config` XML parse |
| Scheduled tasks | `schtasks /query /fo LIST /v` |
| SQL version / config | `sys.configurations`, `SERVERPROPERTY()` |
| SQL logins / roles | `sys.server_principals`, `sys.server_role_members` |
| Linked servers | `sys.servers`, `sys.linked_logins` |
| Database files / sizes | `sys.master_files`, `sys.database_files` |
| Backup history | `msdb.dbo.backupset`, `msdb.dbo.backupmediafamily` |
| Pricing oracle source | `OBJECT_DEFINITION(OBJECT_ID('dbo.sp_camp_getSPprice'))` |
| Contract pricing source | `OBJECT_DEFINITION(OBJECT_ID('dbo.vsp_ic_getcontractprice'))` |
| `wsSetting` URL | `SELECT * FROM amanniquelive.dbo.wsSetting` |
| `SOPortal` samples | `SELECT TOP 10 ... FROM amanniquelive.dbo.SOPortal ORDER BY dcreate DESC` |
| DNS resolution | `nslookup ITREPORT-SERVER` |
| Table row counts | `sys.partitions` + `sys.indexes` + `sys.allocation_units` |
| Firewall state | `netsh advfirewall show allprofiles state` |
| ODBC DSNs | `HKLM:\SOFTWARE\ODBC\ODBC.INI` registry |
