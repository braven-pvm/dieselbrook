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

Until Azure subscription access is granted, **DBM infrastructure provisioning cannot begin**.

---

## Part 1 — Infrastructure Map (What We Know)

### Confirmed servers and services

```
ANNIQUE INFRASTRUCTURE — CONFIRMED AS OF 2026-04-08

ON-PREMISES (Irene, Pretoria)
  ├── AMSERVER-v9 (172.19.16.100)         PRODUCTION AM SQL Server
  │   └── Port 1433: NOT exposed externally
  │   └── All AM databases live here (amanniquelive, etc.)
  │   └── Physical server — RAID Web Console 2 on desktop
  │
  ├── AMSERVER-v9 clone (172.19.16.101)   TEST/STAGING AM SQL Server
  │   └── Port 62111: NOT tested internally (VPN needed)
  │   └── VPN + RDP access provided by Marcel ✅
  │   └── AccountMate 9.3 confirmed running (login dialog visible)
  │
  ├── FortiGate firewall                   NETWORK BOUNDARY
  │   └── Public IP: 41.193.227.190
  │   └── SSL-VPN gateway: away1.annique.com:10443 — PORT OPEN ✅
  │   └── VPN credentials provided: Dieselbrook / Diesel@2026#7 ✅
  │   └── Also: 41.193.227.190:443 open (admin interface?)

SEPARATE HOSTING (NOT Azure) — 129.232.215.87
  ├── shopapi.annique.com                  NISource VFP PRODUCTION SERVER
  │   └── IIS/Web Connection (Microsoft-IIS/10.0, ASP.NET)
  │   └── LIVE — responds to API calls
  │   └── /otpGenerate.api → HTTP 400 (endpoint exists, bad request)
  │   └── /SyncCancelOrders.api → HTTP 200 ✅ (TC-05 answered)
  │   └── Ports: 80 OPEN, 443 OPEN, 3389 CLOSED, 1433 CLOSED
  │   └── NOT Azure — separate hosting provider
  │   └── Location unknown — Annique IT has not identified the host

STAGING SQL (196.3.178.122)                UNKNOWN HOST
  └── Port 62111: OPEN (this is the staging AM SQL instance)
  └── All other ports: CLOSED
  └── This is the existing staging AM connection from our earlier analysis
  └── NOT the same IP as any Azure server we know about

AZURE (20.87.212.38)                       AZURE — SOUTH AFRICA NORTH
  ├── nopintegration.annique.com → 20.87.212.38
  ├── CONFIRMED: This is the NopCommerce production web server
  │   └── Port 80: OPEN — IIS default page (bare IP, not hosted domain)
  │   └── Port 443: OPEN — TLS cert: CN=annique.com (Let's Encrypt, exp June 2026)
  │   └── Port 1433: CLOSED — SQL Server NOT exposed
  │   └── Port 3389: CLOSED — RDP NOT accessible ❌
  │   └── Redirects annique.com/www.annique.com requests to HTTPS
  │
  └── annique.com (Cloudflare-proxied, IPv6 address)
      └── The live Annique website — hosted behind Cloudflare CDN

LIVE WEBSITE (annique.com)
  └── Cloudflare proxy → likely the Azure VM (20.87.212.38)
  └── NopCommerce confirmed (HTML: "nopCommerce" generator meta tag)
  └── stage.annique.com → ALSO NopCommerce staging store (Cloudflare)

UNKNOWN / NOT PROVIDED
  ├── Azure SQL Server for NopCommerce (not the on-prem AM — a separate DB)
  ├── Azure SQL Server for AnqIntegrationApi / Brevo
  ├── Azure App Service or IIS site for AnqIntegrationApi (annique.com/api-backend/)
  ├── Azure subscription details (subscription ID, tenant ID)
  └── shopapi.annique.com hosting provider details
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

| # | Access | Provided by | Status | Verdict |
|---|---|---|---|---|
| 1 | FortiClient VPN credentials | Marcel Truter | `away1.annique.com:10443` · port is OPEN | **✅ Valid — gateway reachable. Needs FortiClient installed to test auth.** |
| 2 | RDP to test AM server | Marcel Truter | `172.19.16.101` · `annique\Dieselbrook` | **⚠️ Valid but VPN-only.** Must connect VPN first. Then test. |
| 3 | AccountMate 9.3 login | Via email screenshot | `DieselB` user shown in AM login dialog | **⚠️ Partial.** Shows we have AM application access on the test server. No AM admin (sa) access. |
| 4 | RDP to "main Azure server" | Marcel Truter | `20.87.212.38` · `azuread\DieselBrook` | **❌ Wrong server. Port 3389 CLOSED.** `20.87.212.38` is the NopCommerce web server. RDP is blocked. Even if it weren't, this is the wrong access model entirely. |
| 5 | Azure subscription access | Requested by Deon | Not provided | **❌ Not provided. Blocking everything.** |
| 6 | NopCommerce admin access | Not requested | Not provided | **❌ Not provided.** |
| 7 | AnqIntegrationApi production config | Not requested | Not provided | **❌ Not provided.** |
| 8 | shopapi.annique.com server access | Not requested | Not provided | **❌ Not provided.** |
| 9 | Source control repository | Does not exist | N/A | **❌ No source control exists anywhere.** |
| 10 | Shopify store access | Not yet requested | Not provided | **❌ Not provided.** |

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

| Priority | Item | Who at Annique | Status | Days blocking |
|---|---|---|---|---|
| 🔴 1 | Azure subscription Owner access | Marcel / Melinda | **NOT PROVIDED** | Every day |
| 🔴 2 | Azure Entra ID app registration access | Marcel / Melinda | **NOT PROVIDED** | Every day |
| 🟠 3 | NopCommerce admin access (stage + prod) | Marcel | **NOT PROVIDED** | Blocking integration |
| 🟠 4 | AnqIntegrationApi live config (`appsettings.json`) | Marcel | **NOT PROVIDED** | Blocking architecture |
| 🟠 5 | `shopapi.annique.com` server access or hosting details | Marcel / unknown | **NOT PROVIDED** | Blocking comms spec |
| 🟠 6 | Live NISource source from `shopapi.annique.com` | Marcel | **NOT PROVIDED** | Blocking OTP spec |
| 🟡 7 | Shopify Plus store collaborator access | Adele / management | **NOT PROVIDED** | ~3 weeks |
| 🟡 8 | Production AM SQL read-only login | Marcel | **NOT PROVIDED** | AM migration spec |
| 🟡 9 | NopCommerce SQL Server name/IP | Marcel | **NOT PROVIDED** | Data migration spec |
| ✅ — | FortiClient VPN credentials | Marcel | **PROVIDED** | — |
| ✅ — | RDP to test AM server (172.19.16.101) | Marcel | **PROVIDED** | — |
| ❌ — | "Azure server RDP" (20.87.212.38) | Marcel | **WRONG SERVER, PORT CLOSED** | — |

---

## Reference

- Infrastructure spec: `docs/spec/03_infrastructure.md`
- AM migration guide: `docs/analysis/27_azure_setup_and_am_migration.md`
- Annique action plan: `docs/analysis/28_annique_requirements_and_actions.md`
- Linear ANN-20 (Technical Confirmations Pending): tracks TC-04, TC-05
- Linear ANN-24 (AM Migration): tracks AM migration work
