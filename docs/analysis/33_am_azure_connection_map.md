# AM ↔ AZ-ANNIQUE-WEB Connection Map
## Dieselbrook — Internal Reference

**Date:** 2026-04-18
**Access used:** Subscription Owner → Azure Run-Command on AZ-ANNIQUE-WEB (no prod-side access required)
**Status:** First complete integration-traffic map between production AM and the Azure VM

> ⚠️ CONFIDENTIAL — Dieselbrook internal. Contains production IPs, credentials, and integration endpoints.

---

## 1. What this document answers

Doc 30 established that production AM exists at `196.3.178.122:62111` and the Azure VM at `20.87.212.38`. Doc 32 established what runs on the Azure VM. This doc maps **every observed byte of traffic between those two estates**, using only the Azure VM side — no access to the AM physical machines was needed.

The core question: **how does the Azure VM actually integrate with production AM — physically, logically, and at the network layer — and what gaps remain when we don't have the AM-side admin access yet?**

---

## 2. The nodes

### 2.1 Azure VM side

- **`AZ-ANNIQUE-WEB`** — Azure VM, public IP `20.87.212.38`, private IP `10.0.0.4`, Azure subscription `355e7b0b-ead0-4fb3-96ba-914900c4f2c4`, single NIC.
- Processes that matter for AM integration:
  - **`sqlservr.exe`** (SQL Server 2019) on TCP 63000
  - **`nopintegration.exe`** (VFP Web Connection COM server, 5 instances)
  - **`backoffice.exe`** (VFP Web Connection COM server)
  - **`w3wp.exe`** (IIS workers for Annique, NopIntegration, Backoffice, API pools)
  - **`AnqImageSync.Console.exe`** (.NET 10 scheduled task, every 10 min)
  - **`BrevoContactsSync.exe`** (.NET scheduled task, daily)

### 2.2 Production AM side (as seen from Azure VM)

Two IPs in the same /24 network block — evidence strongly suggests they are two interfaces on one physical box or a tightly co-located pair:

- **`196.3.178.122:62111`** → `away2.annique.com` → **production AM SQL Server** (`sa`/`AnniQu3S@`, databases `amanniquelive`, `NopIntegration`, `compplanLive`, `compsys`, etc. — the main AM SQL instance)
- **`196.3.178.123`** → **the AM integration runner** — a neighbour IP that originates HTTPS POSTs and also has an OLEDB SQL session to our Azure VM right now. Has not been identified by DNS (no reverse lookup on the Azure box).

### 2.3 Annique LAN side

- **`41.193.227.190`** — Annique office public IP (FortiGate NAT egress). The `172.19.16.0/24` LAN NATs through here. From Azure VM's perspective, the whole Annique office looks like one IP.

### 2.4 ITREPORT-SERVER side

- **`ITREPORT-SERVER\ANREPORTS`** (`172.19.16.27` from doc 30). Appears as a `host_name` in our Azure SQL session list — someone on ITREPORT-SERVER has SSMS open against the Azure SQL right now. Traffic arrives via the Annique FortiGate NAT (`41.193.227.190`).

---

## 3. The observed traffic (snapshot from discovery session)

### 3.1 Outbound: Azure VM → Production AM

Live TCP connections from `10.0.0.4` to `196.3.178.122:62111`:

| Local process | Count | Purpose |
|---|---|---|
| `nopintegration.exe` | 3 | VFP Web Connection COM servers — query AM SQL during `.api` request handling |
| `backoffice.exe` | 1 | VFP BackOffice app — queries AM SQL |
| `w3wp.exe` | 1 | IIS worker (NopCommerce or API pool) — probably an ADO.NET/EF connection |
| `sqlservr.exe` | 1+ | SQL Server 2019 on 63000 — this is the **[AMSERVER-V9] linked server** outbound connection from SQL Agent jobs + procs |

**All 6+ connections use the same public-internet path** to `196.3.178.122:62111`. Firewalled only by the destination side (prod AM SQL is directly internet-exposed on port 62111 with `sa` auth). No VPN, no private endpoint, no peering.

Each sync job (there are 21 of them, cadence 5 min to monthly) touches this path whenever it fires. ImageSync runs every 10 min as a .NET process and opens its own connection — it wasn't mid-run during capture, but it uses the same path (per its `appsettings.json`).

### 3.2 Outbound: Azure VM SQL → HTTPS endpoints (OLE Automation)

The `ANQ_HTTP` proc (identical to `sp_ws_HTTP` on AMSERVER-TEST) uses `sp_OACreate 'MSXML2.ServerXMLHTTP'` for outbound HTTP. It is called from 9 procs in the Annique DB targeting Aramex, FastWay, SkyNet, PayU, Meta Capi, and Brevo. These go to various public endpoints; Cloudflare edge IPs are observed in netstat suggesting some pass through Cloudflare.

**Key point:** these HTTP calls originate from the `sqlservr.exe` process — meaning the SQL Server OS process is the HTTP client. Requires `Ole Automation Procedures = 1` on the instance (confirmed enabled).

### 3.3 Inbound: Production AM → Azure VM (port 63000, SQL)

**One currently open connection** from `196.3.178.123:52533` → Azure VM `sqlservr:63000`. The `sys.dm_exec_sessions` view lists this session with:

- `login_name` = `sa`
- `host_name` = **`AMSERVER-V9`**
- `program_name` = `Microsoft SQL Server OLEDB`
- `client_interface_name` = `OLEDB`
- `database_id` = `master`

**Interpretation:** a machine whose Windows hostname is literally `AMSERVER-V9` is connecting directly via SQL OLEDB to the Azure VM's SQL Server. This is likely the **production AM application server** itself, using its native hostname (doc 30 noted `172.19.16.100` is `AMSERVER-v9` on the Annique LAN). Its public egress comes out on **`196.3.178.123`** — so the AM application server doesn't sit at Annique office; it sits on the same network block as the AM SQL box (196.3.178.x).

This is the first evidence that **the AM application server is internet-reachable from that network, and it opens SQL sessions to the Azure VM on port 63000 with `sa`**. Purpose of the session: unknown without AM-side access, but `program_name = OLEDB` suggests it's a raw custom app, not SSMS.

### 3.4 Inbound: Production AM → Azure VM (port 443 / HTTPS)

From today's IIS log for the NopIntegration site (`W3SVC4`):

- **`196.3.178.123`** — **171 POST requests today** — User-Agent `Mozilla/4.0 (compatible; Win32; WinHttp.WinHttpRequest.5)` — this is Windows' **WinHTTP COM object**, invoked by VFP/classic Windows apps (almost certainly the same `AMSERVER-V9` machine, calling our `.api` endpoints via its own VFP app).

Endpoints called from `196.3.178.123`:

| Endpoint | Hits | Meaning |
|---|---|---|
| `/syncproducts.api?type=availability` | 56+ | Pull product availability deltas to NopCommerce |
| `/syncproducts.api?type=changes` | 28+ | Pull full product changes |
| `/syncorders.api` | 49 | Push NopCommerce orders back to AM (or vice versa) |
| `/SyncCancelOrders.api?instancing=single` | 9 | Order cancellations |

All run on a **5-minute cadence** — consistent with the `Synchlog20260418.log` file inside `C:\NopIntegration\deploy` that records `Sync Orders Start`, `Sync Products Start`, etc. as scheduled by something on the AM side.

### 3.5 Inbound: Other callers hitting NopIntegration `.api`

Same IIS log shows additional endpoints hit by consumer mobile IPs and courier webhooks:

| Endpoint | Hits | Source pattern |
|---|---|---|
| `/NopReport.api` | 156 | Consumer mobile & desktop browsers — this is the **consultant-facing reporting API** |
| `/SkyWebHook.api` | 9 | `102.33.100.20` (SkyNet courier origin) — **incoming courier webhooks** |
| `/updatereferral.api` | 2 | SA broadband IPs — referral program |
| `/api/api/ValidateNewRegistration/<id>` | 3 | MVC-style registration validation |
| `/syncdatatobrevo.br` | 1 | Brevo batch sync |

### 3.6 Inbound: Annique office (`41.193.227.190`) → Azure VM

**7 active SQL connections to port 63000**, each from an ephemeral port on the office public IP. Session metadata shows these are SSMS sessions (Microsoft SQL Server Management Studio) run by `AnqAdmin`, `AmAdmin`, and an `sa` login — all on `host_name` values from the Annique LAN (`ITREPORT-SERVER` appears; others reported as `AZ-ANNIQUE-WEB` because the session is reporting the target box's name for internal SSMS clients).

So staff on the LAN are **directly querying the Azure VM's production SQL** from SSMS at any given moment. No DB service account, no proxy — just `sa`.

### 3.7 Outbound: Azure VM → ancillary services

Additional outbound TCP captured (not AM-related but flags the broader perimeter):

- Cloudflare edge IPs (`162.158.*`, `172.68.*`, `172.69.*`, `172.71.*`) — `System` (HTTP.sys) — this is how `www.annique.com` gets proxied through Cloudflare into this VM inbound, and how outbound calls to Cloudflare-proxied APIs (Brevo? PayU? Meta?) go through.
- New Relic (`162.247.243.*`) — metrics upload
- Fluent Bit — log shipping
- Datto RMM (`3.33.180.249`, `54.194.42.15`) — remote management
- Azure Wire Server (`168.63.129.16`) — Azure metadata/platform
- ESET cloud (`91.228.165.148:8883`) — AV telemetry

### 3.8 The complete observed connection matrix

```
                  ┌─────────────────────────────────────────────────────────┐
                  │             PRODUCTION AM NETWORK (196.3.178.0/24)      │
                  │                                                         │
                  │  196.3.178.122:62111  ── AM SQL (amanniquelive,        │
                  │                           NopIntegration, compplanLive)│
                  │                                                         │
                  │  196.3.178.123        ── AM App Server                 │
                  │    hostname: AMSERVER-V9                               │
                  │    - runs VFP/WinHTTP scheduled syncs                  │
                  │    - has SQL OLEDB connection to Azure SQL             │
                  └────────┬──────────────────────────────────┬─────────────┘
                           │                                  │
                           │ 6+ TCP to .122:62111             │ 1 TCP SQL + 171/day HTTPS
                           │ (ALL sync procs,                 │ from .123 to Azure VM
                           │  linked server queries,          │
                           │  VFP direct ODBC)                │
                           │                                  │
                ◄──────────┘                                  ▼
                OUTBOUND               INBOUND TO AZURE ──────────────────────┐
                                                                              ▼
                                                        ┌─────────────────────────────────┐
                                                        │      AZ-ANNIQUE-WEB             │
                                                        │      20.87.212.38 / 10.0.0.4    │
                                                        │                                 │
                                                        │  sqlservr on 63000              │
                                                        │    - 101 local NopCommerce      │
                                                        │    - 7 inbound Annique office   │
                                                        │    - 1 inbound 196.3.178.123    │
                                                        │    - ? inbound Cloudflare via HTTPS
                                                        │                                 │
                                                        │  IIS on 443/80                  │
                                                        │    - Annique site (NopCommerce) │
                                                        │    - NopIntegration site        │
                                                        │        /syncorders.api          │
                                                        │        /syncproducts.api        │
                                                        │        /SyncCancelOrders.api    │
                                                        │        /NopReport.api           │
                                                        │        /SkyWebHook.api          │
                                                        │        /updatereferral.api      │
                                                        │    - Backoffice site            │
                                                        │    - Quiz, /API, Staging        │
                                                        └──┬─────────────────────────────┘
                                                           │
                                                           │ Inbound SSMS×7
                                                           │ Inbound courier webhooks etc.
                                                           │
                              ┌────────────────────────────┴──────────────────────────┐
                              │  ANNIQUE OFFICE                                        │
                              │  41.193.227.190 (FortiGate NAT egress)                │
                              │  Behind it: 172.19.16.0/24 LAN                         │
                              │     172.19.16.16  andc.annique.local (DC/DNS)         │
                              │     172.19.16.27  ITREPORT-SERVER\ANREPORTS           │
                              │     172.19.16.100 AMSERVER-v9 (AM thick client)       │
                              │                    — note this hostname also seen     │
                              │                    as 196.3.178.123 externally        │
                              │     172.19.16.101 AMSERVER-TEST (we have admin)       │
                              └───────────────────────────────────────────────────────┘
```

---

## 4. What this tells us about the physical/logical topology

### 4.1 Physical

- **AM SQL and the AM App server live together on the same hosted /24 block** (`196.3.178.122` and `.123`) — almost certainly the same physical rack or same VM host. This is NOT inside Annique's office LAN; it's a hosted/colocation site.
- **The production AM server is NOT actually on 172.19.16.100.** That IP on the Annique LAN hosts something named `AMSERVER-v9` but the real production activity is originating from `196.3.178.123` — which identifies *itself* as `AMSERVER-V9` to remote SQL connections. Likely interpretation: **the Annique office box at 172.19.16.100 is a replica or just an RDP gateway into the real machine at 196.3.178.123**, with domain-join hostname aliasing making both report the same name.

  This would explain why Marcel says "the production server is at the office" (true for the thick-client interface) while the actual server doing work is at a hosting provider on `196.3.178.123`.

- **The Azure VM hosts everything else.** Nothing routes back to Annique office LAN for production traffic — traffic flows Azure ↔ 196.3.178.0/24 ↔ consumers directly.

### 4.2 Logical

Five distinct integration patterns are in play:

**Pattern 1: VFP direct ODBC from Azure to AM SQL**
- `nopintegration.exe` + `backoffice.exe` open ODBC connections to `196.3.178.122:62111` as `sa` while handling `.api` requests
- No mediation — raw SQL queries over the internet using `sa`/`AnniQu3S@`
- All credentials in `C:\NopIntegration\deploy\Nopintegration.ini` in cleartext

**Pattern 2: SQL linked server from Azure SQL to AM SQL**
- 16+ procs in the `Annique` database use `[AMSERVER-V9]` to query AM
- SQL Agent jobs invoke these procs on schedules (hourly to every 5 min)
- `sa` → `sa` credential mapping stored in `sys.linked_logins`
- This is what `sqlservr.exe`'s outbound connections to `.122:62111` are

**Pattern 3: .NET app direct to AM SQL**
- `AnqImageSync.Console.exe` runs every 10 min as AmAdmin, opens a fresh connection to AM SQL each run
- Credentials in `appsettings.json` as plaintext

**Pattern 4: AM-side WinHTTP to Azure IIS**
- `196.3.178.123` originates ~1/minute POST requests to `nopintegration.annique.com/*.api`
- User-Agent `Mozilla/4.0 (compatible; Win32; WinHttp.WinHttpRequest.5)` confirms it's Windows COM HTTP, almost certainly VFP `sp_OACreate('MSXML2.ServerXMLHTTP')` pattern running on the AM side
- Mirrors exactly the `ANQ_HTTP` / `sp_ws_HTTP` pattern we extracted from the Azure SQL — same code style on both sides of the wire

**Pattern 5: AM-side direct SQL OLEDB to Azure SQL**
- One OLEDB session from `196.3.178.123` to `20.87.212.38:63000`, logged in as `sa`, database `master`
- Purpose unknown without proc/app source, but active now
- NSG rule `SQL (port 63000, source *)` allows this from anywhere — no source restriction

### 4.3 Network path

- **No VPN, no private link, no peering** — all AM↔Azure traffic traverses the public internet.
- Both ends accept inbound SQL traffic on non-default ports (62111 and 63000) with `sa` credentials.
- Cloudflare sits in front of `nopintegration.annique.com` (observed in VM's outbound — Cloudflare edges) but does NOT sit in front of either SQL endpoint.
- The only perimeter control is the Azure NSG, which as documented in doc 32 allows:
  - SQL 63000 from `*` (wide open)
  - RDP from 26-IP whitelist
  - HTTP/HTTPS from `*`
  - SFTP 2222/2223 from ~15-IP whitelist

---

## 5. Gaps now filled from the Azure side (without AM admin access)

Before this session we didn't know:
- ✅ **Where `nopintegration.annique.com` resolved to** → confirmed `20.87.212.38` = this Azure VM
- ✅ **What `.api` endpoints the VFP Web Connection app exposes** → captured from IIS log (at least 9 distinct endpoints observed today)
- ✅ **Who actually calls those endpoints** → `196.3.178.123` via WinHTTP every 5 min, plus mobile consumers for `/NopReport.api`, plus SkyNet for `/SkyWebHook.api`
- ✅ **Call frequency per endpoint** → 171 sync calls/day from AM side today
- ✅ **What connection strings `nopintegration.exe` uses** → 4 separate SQL connections (AM NopIntegration DB, AM amanniquelive DB, local Annique DB, old webstore DB)
- ✅ **Which process IDs hold AM SQL connections right now** → VFP × 4, IIS × 1, SQL Server outbound × 1+
- ✅ **Whether the AM side ever connects TO us** → yes, both SQL (OLEDB) and HTTPS (WinHTTP)
- ✅ **The `196.3.178.123` mystery IP** → now identified as the real production AM application server (same box that identifies as `AMSERVER-V9` on the LAN)
- ✅ **That `AMSERVER-v9` the hostname is NOT `AMSERVER-V9` the linked server target** → two different things; linked server definition points at `away2.annique.com,62111` (the SQL IP), while the box *named* `AMSERVER-V9` is on `.123` running SQL clients
- ✅ **That there is a `NopIntegration` database on the production AM SQL** → confirmed (appears in `Nopintegration.ini` as one of 4 connections + in `ANQ_SyncCustomerTOAM` as `[AMSERVER-V9].NopIntegration.dbo.NopFieldMapping`)

---

## 6. Gaps that still require AM-side access

Even with everything above, these questions require running something on the AM side:

1. **What exact app on `196.3.178.123` makes the WinHTTP calls?** VFP? .NET? A scheduled Windows task? A service?
2. **What credentials does it use to connect to our SQL on 63000?** We see `sa`, but is there a service account?
3. **What's in the `NopIntegration` database on AM SQL?** The field-mapping table (`NopFieldMapping`) was referenced; what else is there? Queues? State tables? Logs?
4. **What SQL Agent jobs exist on the production AM SQL?** We saw these patterns on AMSERVER-TEST but the live prod could differ.
5. **What triggers on AM tables feed the integration?** `sp_NOP_syncOrders` was in the test box; does prod have others?
6. **Exact cadence + source of the 5-minute sync on 196.3.178.123** — is it a Windows scheduled task? A service? The AM application's own background thread?
7. **What files land on the Rebex SFTP server (ports 2222/2223)?** Drop folder contents not inspected yet.
8. **Physical host of `196.3.178.122`/`.123`** — hosting provider and location (the CIDR range belongs to which ISP?). Affects migration DR planning.

These are all trivially answered by running our discovery script (`scripts/annique-am-discovery.ps1`) on the AM machines — which is why we built it.

---

## 7. Implications for DBM staging

Now that we know exactly how the connection works, the staging environment requirements are fully specified:

### 7.1 What staging must reproduce

- A **local replica of AM SQL** on a host reachable by both:
  - The staging NopCommerce/VFP VM (simulating Azure VM outbound)
  - A simulated "AM app server" that makes WinHTTP calls inbound
- A **fake or staging version of `196.3.178.123`** — a box that:
  - Hosts scheduled tasks mimicking the 5-minute sync cycle
  - Can call a staging `nopintegration.annique.com` equivalent
  - Can open OLEDB connections to the staging SQL
- **DNS overrides or dedicated hostnames** so procs hardcoded to `nopintegration.annique.com` / `away2.annique.com` route to staging versions instead of production
- **Cloudflare is optional** — production runs through it but staging can bypass

### 7.2 What DBM must replace

To eliminate the plaintext-`sa`, internet-exposed-SQL, hardcoded-URL patterns:

1. Move production AM SQL to a **private endpoint** (close NSG 63000 from `*`)
2. Replace **VFP-to-SQL direct ODBC** with DBM's typed API — `nopintegration.exe` and `backoffice.exe` call DBM instead of `sa`ing straight into AM SQL
3. Replace the **`[AMSERVER-V9]` linked server procs** with DBM service calls — no cross-server SQL joins
4. Replace the **`.NET AnqImageSync`** pattern with DBM's product sync domain
5. Replace the **WinHTTP → `.api` endpoints** pattern with DBM as the receiver (DBM's `/webhooks/*` endpoints), with proper auth
6. Replace **`sa` everywhere** with per-service Managed Identity + Key Vault credentials

The `/NopReport.api` consultant mobile endpoint stays (it's a customer-facing API with its own lifecycle), but should move onto the DBM-authenticated path.

### 7.3 What can't be changed yet (phase-1 carve-out)

- The AM thick-client workflow — staff use the Windows AM app, that stays on AM servers
- The AM SQL schema — DBM reads/writes via stored procs, doesn't change the base tables
- The courier webhook endpoints (`/SkyWebHook.api`) — these are where couriers post to; we don't control their configuration, so the URLs have to keep working. DBM becomes the handler.

---

## 8. Evidence base (all captured via Azure Run-Command as SYSTEM)

| Finding | Source |
|---|---|
| Active TCP connections | `Get-NetTCPConnection` on Azure VM |
| SQL session inventory | `sys.dm_exec_sessions` on local SQL (port 63000) |
| IIS logs for NopIntegration | `C:\inetpub\logs\LogFiles\W3SVC4\u_ex260418_x.log` |
| VFP Integration config | `C:\NopIntegration\deploy\Nopintegration.ini` |
| Integration log files | `C:\NopIntegration\deploy\Synchlog*.log`, `SynchOrderStatuslog*.log` |
| DNS resolutions | `Resolve-DnsName` on Azure VM |
| Linked server definitions | `sys.servers`, `sys.linked_logins` |
| NSG rules | `az network nsg rule list` |
| Process→connection mapping | `Get-NetTCPConnection` joined with `Get-Process -Id` |
