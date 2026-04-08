# Access Audit & Requirements — Full Analysis
## Dieselbrook — Internal Reference

**Document type:** Internal — Dieselbrook eyes only  
**Date:** 2026-04-08  
**Status:** Current  
**Context:** CEO-authorised programme. Access is being delayed, deflected, or incorrectly provided by IT and programme contacts. This document records exactly what has been provided, what is wrong with it, and what must be obtained before work can proceed.

---

## Executive Summary

The CEO of Annique has authorised this programme and committed to providing all necessary access. As of 2026-04-08, the following is true:

- **We have been given access to the wrong things.** Marcel's "Azure RDP access" is to the NopCommerce web server, not a management server, and RDP is closed on it. It is useless.
- **The single most important item — Azure subscription access — has not been provided** despite being requested multiple times since before Easter.
- **No source code version control exists** for any of Annique's three systems. Code was delivered as raw file exports, likely copied off a developer's local machine.
- **The staging AM SQL instance (`196.3.178.122:62111`) is exposed on a non-standard port** with no VPN requirement — a security issue that needs flagging.
- **`shopapi.annique.com` is confirmed live** and is the NISource VFP Web Connection server — it responds to API calls. This is the high-risk OTP server.
- **`SyncCancelOrders.api` exists and returns HTTP 200** — TC-05 is partially answered.
- **UPDATE 2026-04-08 (post-VPN session):** VPN connected successfully. RDP to test server `172.19.16.101` works. However, the account provided (`annique\Dieselbrook`) has **no administrator privileges** — `New-NetFirewallRule` and `netsh advfirewall` both fail with access denied. We cannot configure SQL Server firewall rules, install SSH, or run any privileged operations. **Standard user access to one test server is not the same as the administrative access required for this programme.**

---

## UPDATE — VPN Session Findings (2026-04-08)

### What VPN access confirmed

Connecting FortiClient SSL-VPN to `away1.annique.com:10443` with credentials `Dieselbrook / Diesel@2026#7` assigns VPN IP `10.212.134.200` and routes `172.19.16.0/24` through the tunnel.

**Hosts confirmed reachable via VPN:**

| Host | Ports open | Identity |
|---|---|---|
| `172.19.16.100` | 1433 (SQL), 3389 (RDP) | **Production AMSERVER-v9** |
| `172.19.16.101` | 3389 (RDP) | **Test/clone AM server** (Marcel's provided server) |
| `172.19.16.16` | 80, 445 (SMB), 3389 (RDP) | Unknown Windows server |
| `172.19.16.27` | 1433 (SQL), 3389 (RDP) | **Unknown SQL Server** — not AM, purpose unknown |
| `172.19.16.63` | 445 (SMB) | Unknown Windows machine |
| `172.19.16.12/15/18/19/24/25` | 80 | Canon printers/copiers |

**Not accessible via VPN:**
- `20.87.212.38:3389` — Azure NopCommerce server RDP still closed
- `196.3.178.122:62111` — staging AM SQL — not routed through this VPN

**New discovery:** `172.19.16.27` has a SQL Server (port 1433) and RDP that we did not know about. This could be the NopCommerce SQL Server or another internal system. Requires investigation.

### The privilege problem

RDP to `172.19.16.101` (test server) works with `annique\Dieselbrook` / `Diesel@2026#7`.

Once logged in, attempting to open SQL Server firewall rules failed:

```
New-NetFirewallRule : The term 'New-NetFirewallRule' is not recognized...
  + CategoryInfo : ObjectNotFound
  + FullyQualifiedErrorId : CommandNotFoundError
```

```
netsh advfirewall firewall add rule ... 
→ Access denied
```

**Root cause:** The `Dieselbrook` account is a **standard (non-administrator) domain user**. It has:
- RDP access to the test server ✅
- Desktop and basic application access ✅
- No ability to modify firewall rules ❌
- No ability to install software ❌
- No ability to configure SQL Server ❌
- No ability to run `sqlcmd` or SSMS with elevated privileges ❌
- No ability to create SQL backups via PowerShell ❌

**Additional finding:** `New-NetFirewallRule` not being recognised suggests this may be Windows Server 2008 R2 or 2012 (pre-NetSecurity module). This is relevant for the migration — older OS will need upgrade steps.

### What we can do with the current access

With a standard user RDP session on `172.19.16.101` we can:
- See the desktop and installed applications
- Open AccountMate 9.3 (user `DieselB` — confirmed from earlier screenshot)
- Browse the filesystem (non-system folders)
- Check what software is installed (`winver`, Programs list)

We **cannot** do anything technically useful for the programme without elevated access.

Until Azure subscription access is granted, **DBM infrastructure provisioning cannot begin**.

---

## Part 1 — Infrastructure Map (What We Know)

### Confirmed servers and services

```
ANNIQUE INFRASTRUCTURE — CONFIRMED AS OF 2026-04-08 (post-VPN session)

ON-PREMISES LAN: 172.19.16.0/24 (Irene, Pretoria)

  ├── 172.19.16.100   PRODUCTION AMSERVER-v9
  │   └── SQL Server (1433): TCP reachable via VPN ✅ — Windows Firewall blocks TDS connections
  │   └── RDP (3389): TCP reachable via VPN ✅ — no credentials provided for prod
  │   └── Physical server (RAID Web Console 2 on desktop)
  │   └── IIS running on port 80 (default IIS page — probably AM web services)
  │
  ├── 172.19.16.101   TEST / CLONE AM SERVER
  │   └── RDP (3389): ✅ ACCESSIBLE — annique\Dieselbrook / Diesel@2026#7
  │   └── AccountMate 9.3 confirmed running
  │   └── SQL Server port NOT confirmed open (TCP not tested internally yet)
  │   └── Account is STANDARD USER ONLY — no admin privileges ❌
  │   └── Cannot run firewall rules, install software, run elevated commands
  │
  ├── 172.19.16.16    UNKNOWN WINDOWS SERVER
  │   └── SMB (445), HTTP (80), RDP (3389) all open
  │   └── HTTP returns 403 Forbidden — IIS with access control
  │   └── Purpose unknown — not investigated
  │
  ├── 172.19.16.27    UNKNOWN SQL SERVER ⚠️ NEW DISCOVERY
  │   └── SQL Server (1433): TCP open via VPN
  │   └── RDP (3389): TCP open via VPN
  │   └── Not the AM server — different IP
  │   └── Could be: NopCommerce SQL, AnqIntegrationApi DB, or other internal system
  │   └── No credentials to access — requires investigation
  │
  ├── 172.19.16.63    UNKNOWN WINDOWS MACHINE
  │   └── SMB (445) only — likely a workstation or NAS
  │
  ├── 172.19.16.12/15/18/19/24/25   CANON PRINTERS/COPIERS
  │   └── HTTP admin interfaces on port 80 (Canon HTTP Server)
  │   └── Not relevant to programme
  │
  ├── FortiGate firewall              NETWORK BOUNDARY
  │   └── Public IP: 41.193.227.190
  │   └── SSL-VPN: away1.annique.com:10443 ✅ CONFIRMED WORKING
  │   └── VPN credentials: Dieselbrook / Diesel@2026#7 ✅
  │   └── Assigns VPN IP 10.212.134.200, routes 172.19.16.0/24 via VPN
  │   └── Port 443 also open on 41.193.227.190 (FortiGate admin interface?)

SEPARATE HOSTING (NOT Azure) — 129.232.215.87
  ├── shopapi.annique.com                  NISource VFP PRODUCTION SERVER
  │   └── IIS/Web Connection (Microsoft-IIS/10.0, ASP.NET)
  │   └── LIVE — responds to API calls
  │   └── /otpGenerate.api → HTTP 400 (endpoint exists, bad request)
  │   └── /SyncCancelOrders.api → HTTP 200 ✅ (TC-05 answered)
  │   └── Ports: 80 OPEN, 443 OPEN, 3389 CLOSED, 1433 CLOSED
  │   └── NOT Azure — separate hosting provider
  │   └── Location unknown — Annique IT has not identified the host

STAGING SQL (196.3.178.122)                UNKNOWN HOST — NOT ON ANNIQUE LAN
  └── Port 62111: OPEN from internet (no VPN needed — security risk)
  └── NOT reachable via FortiClient VPN (not on 172.19.16.0/24)
  └── Unknown what machine/hosting this is — separate IP range entirely
  └── Could be a legacy external staging server — Annique IT has not explained it

AZURE (20.87.212.38)                       AZURE SOUTH AFRICA NORTH
  ├── nopintegration.annique.com → 20.87.212.38 (DNS confirmed)
  ├── This is the NopCommerce production web server
  │   └── Port 80: OPEN — bare IIS default page (no domain routing on IP)
  │   └── Port 443: OPEN — TLS cert CN=annique.com (Let's Encrypt, expires June 2026)
  │   └── Port 1433: CLOSED — SQL Server not on this VM or not exposed
  │   └── Port 3389: CLOSED — RDP blocked by NSG ❌
  │   └── Marcel whitelisted our IP for RDP but port remains closed — wrong server or wrong rule
  └── annique.com / www.annique.com → Cloudflare CDN → this VM
      └── NopCommerce confirmed (meta generator tag in HTML)
      └── stage.annique.com → NopCommerce staging (also Cloudflare)

AZURE — UNKNOWN
  ├── NopCommerce SQL Server — location unknown (port 1433 closed on 20.87.212.38)
  ├── AnqIntegrationApi host — could be same VM or separate App Service
  ├── Azure subscription ID / tenant ID — not provided
  └── No Azure Portal access granted — cannot see any of this

EXTERNAL HOSTING (NOT Azure, NOT on-prem)
  └── shopapi.annique.com (129.232.215.87) — NISource VFP Web Connection server
      └── Hosting provider unknown — Annique IT has not identified it
      └── Live production server — handles OTP, SyncCancelOrders, other APIs
      └── No access provided
```

---

## Part 2 — Source Code Audit

### What we received and what it reveals

#### NISource (VFP — the legacy middleware)

**How we received it:** Raw `.prg` files in a folder. No git history, no repository, no version control of any kind.

**What the source reveals:**
- **Deployed on `shopapi.annique.com` (129.232.215.87)** — a separate, non-Azure server
- **IIS site number `w3svc/21`** — this is a specific IIS site on that server
- **Two development paths in the code:** `c:\WebConnectionProjects\NopIntegration\Web\` and `e:\development\nopintegration\web\` — suggesting the developer has/had it running locally on at least two machines
- **No CI/CD of any kind** — deployed by manually copying files to the server
- **Framework: Web Connection (VFP)** — a commercial FoxPro web framework. Confirmed by the test page content on `shopapi.annique.com`.
- **Live API endpoints confirmed:** `SyncCancelOrders.api` returns HTTP 200, `otpGenerate.api` returns HTTP 400 (exists but needs POST body), `syncorders.api` on `nopintegration.annique.com` also returns HTTP 200
- **Hardcoded credentials in source** (pre-redacted in the copy we received — but they were there)
- **No git, no SVN, no TFS markers** anywhere in the source tree

**Implication:** There is no "repository" to give us access to. The source lives on a developer's machine and gets copied to the server. There is no version control. **We have what exists.**

**What we don't have:**
- The `appsettings`/config file that contains the live connection strings (these are on the server, not in source)
- The developer who wrote this (unknown — not Marcel, not Melinda)
- Any documentation of the VFP Web Connection deployment

---

#### AnqIntegrationApiSource (.NET 9 — the existing API)

**How we received it:** A Visual Studio solution folder. No `.git` directory. The `.vs/` metadata reveals it was developed locally at `C:\Development\VS\AnqIntegrationApi\` on a Windows machine.

**What the source reveals:**
- `appsettings.json` has **REDACTED** for all connection strings — the live values are not in the source we received
- Developed in Visual Studio 2022 (v17, from the `.vs/` metadata)
- **No CI/CD** — no pipelines, no GitHub Actions, no deployment scripts
- **An `appsettings.development.json` file exists but was gitignored** — it was never included in what they gave us. This file contains the real database connection strings.
- The app serves as a backend API that NopCommerce calls (`annique.com/api-backend/`)
- References to `melinda@annique.com` hardcoded as a debug/CC email recipient
- The app forces emails to `melinda@annique.com` when in development mode — this is a developer hack left in the code

**Implication:** We have the code structure but not the running configuration. The real connection strings (to NopCommerce SQL, to Brevo DB, to AccountMate) are on the server, not in source control.

**What we don't have:**
- `appsettings.development.json` (real connection strings)
- The production `appsettings.json` deployed on the Azure server
- Knowledge of where the API is hosted (IIS site on `20.87.212.38`? Azure App Service? Separate VM?)

---

#### NopCommerce — Annique Customisation

**How we received it:** A Visual Studio solution with custom plugins. Has a `.gitignore` (standard Visual Studio template) but **no `.git` directory** — it was never actually put in git.

**What the source reveals:**
- The `Deploying.Readme.txt` says: "1. Open solution in Visual Studio. 2. Rebuild. 3. Publish Nop.Web from Visual Studio." — this is entirely manual, no automation
- Custom plugins: `Annique.Plugins.Nop.Customization` and `Annique.Plugins.Payments.AdumoOnline` (PayU/AdumoOnline payment gateway — this is the live payment provider)
- `web.config` confirms deployed on IIS with AspNetCoreModuleV2 — the Azure VM at `20.87.212.38` (confirmed by `nopintegration.annique.com` DNS pointing there)
- **No version control. No CI/CD. No deployment automation.**

**Critical finding in `web.config`:** The comment says *"When deploying on Azure, make sure that 'dotnet' is installed and the path to it is registered in the PATH environment variable."* This is a manual, error-prone process. Annique's NopCommerce deployment is entirely manual — someone RDPs to the Azure VM and copies files.

**What we don't have:**
- The database connection string (stored in `appsettings.json` on the server)
- The NopCommerce admin credentials
- Access to the NopCommerce admin panel

---

### Source control conclusion

**None of Annique's three systems has any source control.** No git, no SVN, no TFS, no Azure DevOps. Code is maintained on developer machines and deployed manually by copying files to servers. The `.gitignore` in NopCommerce is there because Visual Studio adds it by default when creating a project — it was never actually committed to git.

**This is a significant risk for the programme:** we cannot establish a reliable baseline of "what is currently running in production" versus "what we received." The code on the servers may differ from what was sent to us.

---

## Part 3 — Access Inventory: What Has Been Provided

*Last updated: 2026-04-08 after active VPN + RDP session*

| # | Access | Provided by | Verified | Verdict |
|---|---|---|---|---|
| 1 | FortiClient VPN · `away1.annique.com:10443` | Marcel | ✅ **CONFIRMED WORKING** | VPN connects. Assigns IP `10.212.134.200`. Routes `172.19.16.0/24`. |
| 2 | RDP to test AM server `172.19.16.101` · `annique\Dieselbrook` | Marcel | ✅ **CONFIRMED WORKING** | RDP connects. Desktop accessible. |
| 3 | Administrator access on `172.19.16.101` | Marcel | ❌ **NOT PROVIDED** | Account is standard user. Firewall rules fail. Cannot install software. Cannot run elevated commands. Cannot take backups. Useless for technical work. |
| 4 | SQL Server access on `172.19.16.101` (port 1433) | Marcel | ❌ **NOT PROVIDED** | Windows Firewall blocks SQL connections from VPN. Requires admin to fix. |
| 5 | AccountMate 9.3 app login (`DieselB`) | Via screenshot | ⚠️ **PARTIAL** | AM application opens. No SQL-level access. No AM admin. |
| 6 | RDP to Azure server `20.87.212.38` · `azuread\DieselBrook` | Marcel | ❌ **WRONG SERVER + PORT CLOSED** | This is the NopCommerce web server. Port 3389 is blocked. Marcel added firewall rule to wrong machine. |
| 7 | Azure subscription / Portal access | Deon requested | ❌ **NOT PROVIDED** | Not provided. Blocking all infrastructure work. |
| 8 | Azure Entra ID / app registration | Not yet requested | ❌ **NOT PROVIDED** | Not provided. |
| 9 | NopCommerce admin panel (`stage.annique.com/admin`) | Not requested | ❌ **NOT PROVIDED** | Not provided. |
| 10 | AnqIntegrationApi live config (`appsettings.json`) | Not requested | ❌ **NOT PROVIDED** | Real connection strings not in source we received. |
| 11 | `shopapi.annique.com` server access | Not requested | ❌ **NOT PROVIDED** | Hosting provider unknown. No credentials. |
| 12 | Source control repository (any system) | Does not exist | ❌ **DOES NOT EXIST** | No git, SVN, or TFS on any system. Code on developer machines only. |
| 13 | Shopify Plus store access | Not yet requested | ❌ **NOT PROVIDED** | Not provided. |
| 14 | `172.19.16.27` SQL Server access | Not requested | ❌ **NOT PROVIDED** | Newly discovered server with SQL. Purpose unknown. |

---

## Part 4 — The Full List of What We Need

### CRITICAL — Blocking programme start

---

**NEED 1: Azure Subscription — Owner Access**

**What:** `Owner` role assignment on the Azure subscription that currently hosts NopCommerce (the subscription containing the `20.87.212.38` VM).

**Why:** Without this, we cannot:
- Provision any Azure infrastructure (VNets, VMs, App Services, Key Vault, Service Bus, SQL)
- See what resources currently exist (VMs, SQL servers, storage, networking)
- Create resource groups for the DBM staging/parity/production environments
- Assign Managed Identity roles
- Configure VPN Gateway for production AM connectivity
- Deploy anything

This is not optional. It is not negotiable. Every single piece of infrastructure work depends on this access. We have been asking for this since before Easter. The meeting Melinda requested to "hear exactly what access we need" should have resulted in this being actioned within the hour.

**Severity:** 🔴 **Programme cannot start without this.**

**How to provide (Marcel — 3 minutes):**
1. Go to `portal.azure.com` — log in with Annique Azure account
2. Search "Subscriptions" in the top search bar
3. Click on the subscription name (likely "Annique" or similar)
4. In the left menu, click **"Access control (IAM)"**
5. Click **"+ Add"** → **"Add role assignment"**
6. In the "Role" tab: search for and select **"Owner"**
7. Click "Next" → in "Members" tab: click **"+ Select members"**
8. Search for: `mariusbloemhof@gmail.com` → select it → click "Select"
9. Click "Review + assign" → "Review + assign" again
10. Done. Marius will receive an email confirmation.

If Marcel cannot find the subscription or is unsure: send a screenshot of `portal.azure.com` → "Subscriptions" page to Deon and we will guide step by step.

---

**NEED 1b: Local Administrator on the Test AM Server (172.19.16.101) — and SQL Firewall Open**

**What:** The `Dieselbrook` account needs to be added to the **Local Administrators** group on `172.19.16.101`. Additionally, Marcel must open Windows Firewall on that server to allow SQL Server (TCP 1433) from our VPN IP (`10.212.134.200`).

**Why:** We have RDP access to the test server. We cannot do anything useful with it. We cannot:
- Configure the SQL Server firewall to allow remote connections
- Install OpenSSH for shell access
- Run `sqlcmd` with elevated privileges
- Create database backups
- Check SQL Agent jobs
- Run any of the diagnostic queries that confirm the server is suitable for staging

Every single diagnostic and setup task on this server requires local admin. Marcel gave us a user account, not an administrator account. These are not the same thing.

**Severity:** 🔴 **Blocking all server-side diagnostic and setup work.**

**How to provide (Marcel — 2 minutes, while logged in as his own admin account):**

Step 1 — Add Dieselbrook to Local Administrators:
```
# Open PowerShell as Administrator (right-click → Run as Administrator)
Add-LocalGroupMember -Group "Administrators" -Member "annique\Dieselbrook"
```

Or via GUI:
1. Right-click "This PC" → Manage → Local Users and Groups → Groups
2. Double-click "Administrators"
3. Click "Add" → type `annique\Dieselbrook` → OK

Step 2 — Open SQL Server firewall for our VPN IP:
```
netsh advfirewall firewall add rule name="SQL Server - Dieselbrook" dir=in action=allow protocol=TCP localport=1433 remoteip=10.212.134.200
netsh advfirewall firewall add rule name="SQL Browser - Dieselbrook" dir=in action=allow protocol=UDP localport=1434 remoteip=10.212.134.200
```

That is all. Two PowerShell commands. Takes under 2 minutes.

---

**NEED 2: Azure Active Directory / Entra ID — Application Registration Access**

**What:** At minimum, `Application Developer` role in Azure AD / Entra ID to register DBM as an application and create service principals. Ideally, `Application Administrator`.

**Why:** DBM uses Managed Identity for all service-to-service authentication. Setting this up requires the ability to create and configure app registrations and service principals in the Entra ID tenant.

**Severity:** 🔴 **Blocking. Required before any App Service or Managed Identity work.**

**How to provide:**
1. Azure Portal → **"Microsoft Entra ID"** (search in top bar)
2. Left menu: **"Roles and administrators"**
3. Search for "Application Developer" → click it
4. Click **"Add assignments"** → search `mariusbloemhof@gmail.com` → assign

---

### HIGH PRIORITY — Blocking integration work

---

**NEED 3: NopCommerce Admin Access**

**What:** Admin login to the live NopCommerce store at `stage.annique.com` (staging) and `annique.com` (production — read-only access acceptable initially).

**Why:** We must be able to:
- Read the live product catalogue (what NopCommerce currently serves)
- Read the live customer/consultant data
- Understand current pricing and category structures
- Verify what the NISource → NopCommerce sync currently produces
- Set up webhooks for DBM to receive Shopify events (future)

**Severity:** 🟠 **Blocking domain specs and integration testing.**

**How to provide:**
1. Go to `stage.annique.com/admin` or `annique.com/admin`
2. Create a new admin account for `mariusbloemhof@gmail.com` (do not share the master admin password)
3. Or: Settings → Admin Users → Add new admin user → `marius@dieselbrook.co.za` / set a temporary password

---

**NEED 4: AnqIntegrationApi Production Configuration**

**What:** The live `appsettings.json` deployed on the production server for AnqIntegrationApi (the .NET 9 API running behind `annique.com/api-backend/`).

**Why:** This file contains the production database connection strings (NopCommerce SQL, Brevo DB, AccountMate DB). We need these to:
- Understand the current routing from AnqIntegrationApi to AccountMate
- Understand which SQL Server hosts the NopCommerce database (we know it's NOT on `20.87.212.38` because port 1433 is closed there)
- Confirm whether AnqIntegrationApi is hosted on the same Azure VM or a separate one

**Severity:** 🟠 **Blocking architecture confirmation.**

**How to provide:**
- Option A (preferred): Marcel RDPs to the server where AnqIntegrationApi runs → navigates to the app folder → opens `appsettings.json` → copies and sends to Deon (remove any passwords — we only need the server names/connection string structure)
- Option B: Marcel shares screen during a Teams/Zoom call and we walk through it together

---

**NEED 5: Access to `shopapi.annique.com` Server**

**What:** Either RDP access to the server at `129.232.215.87` (the NISource VFP server), or at minimum: confirmation of who hosts it, what OS/Windows version it runs, and whether Annique has access to it or if it's managed by an external provider.

**Why:**
- `shopapi.annique.com` hosts the OTP endpoint (`/otpGenerate.api`) and potentially other NISource functions
- We cannot replace these services without understanding what they do and how they connect to AccountMate
- TC-04 (OTP ownership) remains open and is blocking the communications domain spec
- **`SyncCancelOrders.api` is confirmed live on this server** (HTTP 200) — so order cancellation currently goes through this server, not through `nopintegration.annique.com`

**Severity:** 🟠 **Blocking communications domain spec and order cancellation spec.**

**How to provide:**
- Confirm: who is the hosting provider for `129.232.215.87`? (Afrihost? Hetzner? Other?)
- Confirm: does Marcel/IT have RDP or console access to this server?
- If yes: provide RDP credentials same pattern as the other servers
- If no: tell us who to contact

---

**NEED 6: Source Code for NISource Running on `shopapi.annique.com`**

**What:** The current **live** version of the NISource/Web Connection code running on `shopapi.annique.com`. Not the copy we already have, because that copy may be different from what is actually deployed.

**Why:** The code we received may be an old developer copy. The live server may have changes — especially for the OTP endpoint, which `shopapi.annique.com` specifically handles. The OTP logic is the highest-risk component to replace.

**Severity:** 🟠 **Blocking OTP/communications domain spec.**

**How to provide:**
- Marcel RDPs to `shopapi.annique.com` (or whoever manages it does)
- Navigates to `E:\development\nopintegration\web\` (or wherever the live files are)
- Zips the folder and shares it with Deon

---

### MEDIUM PRIORITY — Required before go-live

---

**NEED 7: Shopify Store Access**

**What:** Access to the live Shopify Plus store at `annique.com` as a store collaborator (Staff member with Orders, Products, Customers, Settings permissions). Also: access to the Shopify Partner Dashboard to set up the DBM custom app.

**Why:** DBM integrates with Shopify. We need to install the custom app, register webhooks, deploy Shopify Functions, and set up the staging development store.

**Severity:** 🟡 **Required before Shopify integration work begins (~2–3 weeks from now).**

**How to provide:**
1. Shopify Admin → Settings → Users and permissions → Add staff
2. Email: `mariusbloemhof@gmail.com`
3. Permissions: All of Orders, Products, Customers, Analytics, Settings (not Financials)

For Shopify Partner Dashboard access — Deon handles this as part of the Shopify Plus plan setup.

---

**NEED 8: AccountMate SQL Direct Access (Production — Read-Only)**

**What:** The ability to connect directly to `AMSERVER-v9` (172.19.16.100:1433) from the Annique LAN — once VPN is connected — with a **read-only SQL login** to run diagnostic queries.

**Why:** We need to verify the exact SQL Server version, edition, all SQL Agent jobs, linked servers, and database sizes **on the production server** (not just the test clone) before designing the migration.

**Severity:** 🟡 **Required for AM migration spec. Not blocking current DBM build.**

**How to provide:**
- Marcel creates a read-only SQL login on AMSERVER-v9:
  ```sql
  CREATE LOGIN dieselbrook_ro WITH PASSWORD = '<agreed_password>';
  -- Grant VIEW SERVER STATE for diagnostics
  GRANT VIEW SERVER STATE TO dieselbrook_ro;
  -- On each database:
  USE amanniquelive; CREATE USER dieselbrook_ro FOR LOGIN dieselbrook_ro;
  ALTER ROLE db_datareader ADD MEMBER dieselbrook_ro;
  ```
- Share the password with Deon directly

---

**NEED 9: NopCommerce SQL Server Details**

**What:** The name/IP and port of the SQL Server that hosts the NopCommerce database. We know it's NOT on `20.87.212.38` (port 1433 closed). It could be:
- A separate Azure VM
- An Azure SQL Database (PaaS)
- The same `196.3.178.122` machine that has AM staging

**Why:** AnqIntegrationApi connects to this database. When NopCommerce is decommissioned, we need to understand what data lives in it and what DBM needs to migrate or replace.

**Severity:** 🟡 **Required for data migration planning.**

**How to provide:** Marcel checks `appsettings.json` on the NopCommerce server → tells us the `Server=` value in the NopDb connection string.

---

## Part 5 — The Specific Problem with the Access Provided So Far

### The RDP "Azure server" situation — spelled out plainly

Marcel gave us:
```
RDP: 20.87.212.38
Username: azuread\DieselBrook
```

What `20.87.212.38` is: **the NopCommerce web server.** `nopintegration.annique.com` resolves to this IP. It runs IIS on port 80 and 443. The TLS certificate is for `annique.com`. Port 3389 is **closed** — RDP is not enabled or is blocked by the NSG.

Even if Marcel opens port 3389 on this server:
- This gives us access to the Windows Server running the NopCommerce website
- We cannot provision DBM infrastructure from inside a Windows VM via RDP
- This is not "Azure access" — it is "access to one VM that is inside Azure"
- The Azure management plane (Resource Manager, IAM, VNet, Key Vault, etc.) is accessed via `portal.azure.com` or Azure CLI — not via RDP to a specific VM

Marcel's static IP firewall rule is correct thinking but wrong target. He has whitelisted our IP for RDP on the NopCommerce web server. We do not need RDP to that server. We need IAM access on the subscription.

### The pattern — what is happening

The email chain shows a clear pattern:

| Date | What happened |
|---|---|
| 07 Apr | Deon emails asking for AccountMate + Azure access |
| 07 Apr | Adele responds asking for a website project plan — **ignores the access request** |
| 07 Apr | Melinda asks "what do you need on Azure?" — should have been obvious from the email |
| 07 Apr | Deon clarifies: "access to the hosting account" |
| 07 Apr | Marcel provides VPN + test server RDP — correct, but only half of what was asked |
| 08 Apr | Melinda asks for a meeting to "hear exactly what access you need on Azure" |
| 08 Apr | Deon follows up again on Azure access |
| 08 Apr | Marcel provides RDP to `20.87.212.38` — wrong server, port closed |

The Azure subscription access has been deflected three times. Whether this is deliberate gatekeeping or genuine technical confusion is unclear, but the effect is the same: **the programme cannot start**.

---

## Part 6 — Communication to Send

The following should be sent from Deon to Melinda, CC: Marcel, CC: Adele, CC: CEO:

> **Subject: Azure Access — Clear Instructions (Action Required Today)**
>
> Hi Melinda, Marcel,
>
> Dankie vir die VPN en AccountMate toegang — dit is ontvang en werk.
>
> Die Azure toegang wat tans verskaf is, is ongelukkig **verkeerd**. Ons het RDP-toegang ontvang na `20.87.212.38` (die NopCommerce webserver), maar:
> 1. **RDP (poort 3389) is gesluit** op daai server — ons kan nie inlog nie
> 2. **Selfs al was dit oop, is dit die verkeerde soort toegang** — ons het nie RDP na 'n spesifieke server nodig nie, ons het toegang tot die Azure **subscription** (die "Azure rekening") nodig
>
> **Wat ons presies nodig het** (dit neem Marcel 3 minute):
>
> 1. Gaan na **portal.azure.com** → teken in
> 2. Soek **"Subscriptions"** in die soekbalk bo-aan
> 3. Klik op die subscription naam
> 4. Links-kant menu: klik **"Access control (IAM)"**
> 5. Klik **"+ Add"** → **"Add role assignment"**
> 6. Kies rol: **"Owner"**
> 7. Klik "Next" → "Select members" → soek `mariusbloemhof@gmail.com` → "Select"
> 8. "Review + assign" → "Review + assign"
>
> Dit is dit. Hierdie stap is **kritiek** — sonder dit kan ons nie 'n enkele reël infrastruktuur opstel nie, en die hele projek is geblokkeer.
>
> Die Konsultant het gesê dit sal 3 minute neem. Kan dit **vandag gedoen word**?
>
> Groete,  
> Deon

---

## Part 7 — The Source Control Problem

Annique has no version control on any of their three systems. This is a risk we need to manage going forward, not fix retroactively.

**What this means for the programme:**
- We cannot establish with certainty that the source code we received matches what is running in production
- Any hotfix applied directly to a production server is invisible to us
- When NISource is eventually decommissioned, there is no repository to archive

**Our recommendation:** As part of the DBM programme, we will set up a GitHub repository for the Annique codebase. The deliverable from this programme (DBM) will be properly version-controlled in `github.com/braven-pvm/dieselbrook`. For the legacy systems (NISource, NopCommerce, AnqIntegrationApi), we recommend:
- Create a GitHub organisation for Annique
- Archive the source as-received into private repositories
- This gives Annique an audit trail going forward even if the code is never actively developed again

This is a recommendation, not a requirement for DBM go-live.

---

## Summary — Priority Table

*Last updated: 2026-04-08 after active VPN + RDP session*

| Priority | Item | Who | Status |
|---|---|---|---|
| 🔴 1 | **Azure subscription Owner access** | Marcel / Melinda | ❌ NOT PROVIDED — blocks everything |
| 🔴 2 | **Local Administrator on `172.19.16.101`** | Marcel | ❌ NOT PROVIDED — account is standard user only |
| 🔴 3 | **SQL Server firewall open on `172.19.16.101`** | Marcel | ❌ NOT PROVIDED — 2 netsh commands, needs admin |
| 🟠 4 | Azure Entra ID app registration access | Marcel / Melinda | ❌ NOT PROVIDED |
| 🟠 5 | NopCommerce admin panel access | Marcel | ❌ NOT PROVIDED |
| 🟠 6 | AnqIntegrationApi live `appsettings.json` | Marcel | ❌ NOT PROVIDED |
| 🟠 7 | `shopapi.annique.com` server access + hosting details | Marcel | ❌ NOT PROVIDED |
| 🟠 8 | Live NISource source from `shopapi.annique.com` | Marcel | ❌ NOT PROVIDED |
| 🟠 9 | Identity + purpose of `172.19.16.27` SQL Server | Marcel | ❌ NOT PROVIDED — newly discovered |
| 🟡 10 | Shopify Plus store collaborator access | Adele | ❌ NOT PROVIDED — needed in ~3 weeks |
| 🟡 11 | Production AM SQL read-only login | Marcel | ❌ NOT PROVIDED |
| 🟡 12 | NopCommerce SQL Server name/IP | Marcel | ❌ NOT PROVIDED |
| ✅ | FortiClient VPN (`away1.annique.com:10443`) | Marcel | ✅ WORKS — confirmed 2026-04-08 |
| ⚠️ | RDP to `172.19.16.101` (`annique\Dieselbrook`) | Marcel | ⚠️ WORKS but **standard user only — useless without admin** |
| ❌ | RDP to Azure server `20.87.212.38` (`azuread\DieselBrook`) | Marcel | ❌ WRONG SERVER — NopCommerce web VM, port 3389 closed |

---

## Reference

- Infrastructure spec: `docs/spec/03_infrastructure.md`
- AM migration guide: `docs/analysis/27_azure_setup_and_am_migration.md`
- Annique action plan: `docs/analysis/28_annique_requirements_and_actions.md`
- Linear ANN-20 (Technical Confirmations Pending): tracks TC-04, TC-05
- Linear ANN-24 (AM Migration): tracks AM migration work
