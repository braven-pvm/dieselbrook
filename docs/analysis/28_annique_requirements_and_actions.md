# Annique — Requirements & Action Plan
## What We Need and What Annique Needs to Do

**Document type:** Internal reference — Dieselbrook team  
**Date:** 2026-04-08  
**Status:** Living document — update as items are completed

---

## Part 1 — Everything We Need From Annique

### Access

| # | What | Who at Annique | Urgency | Blocks |
|---|---|---|---|---|
| A1 | **Azure subscription** — Owner or Contributor + User Access Administrator role on the Azure subscription currently hosting the NopCommerce site | Melinda Kotze / IT | 🔴 Now | All Azure infrastructure provisioning |
| A2 | **FortiClient VPN** — already provided ✅ | Marcel Truter | Done | — |
| A3 | **RDP to test AM server** (172.19.16.101 · annique\Dieselbrook) — already provided ✅ | Marcel Truter | Done | — |
| A4 | **Shopify Plus partner access** — confirm active Shopify Plus plan; invite Dieselbrook to Shopify Partner Dashboard as collaborator | Adele du Toit / management | 🔴 Now | Shopify store setup, staging, Shopify Functions deploy |
| A5 | **Azure DevOps / GitHub** — not required; Dieselbrook manages the repo | — | — | — |

### Data

| # | What | Who at Annique | Urgency | Blocks |
|---|---|---|---|---|
| D1 | **Production AM database backup** — `.bak` files for `amanniquelive`, `compplanLive`, `compsys`, `amanniquenam`. Coordinated with Marcel. Initial seed for `vm-am-staging`. | Marcel Truter | 🟠 Within 1 week | Staging environment AM seed |
| D2 | **AM server info** — run the diagnostic SQL queries (see ANN-24 for exact scripts) on AMSERVER-v9 or the test server: SQL Server version/edition, SQL Agent jobs list, Windows Server version, linked servers | Marcel Truter | 🟠 Within 1 week | `docs/spec/06_am_migration.md` (Track B) |
| D3 | **AM client machine count** — how many workstations run the AccountMate client? Do they have Active Directory/Windows domain? | Marcel Truter | 🟡 Before AM migration | Track B planning (ODBC update scope) |

### Decisions / Confirmations

| # | What | Who at Annique | Urgency | Blocks |
|---|---|---|---|---|
| C1 | **TC-04 — OTP / `shopapi.annique.com`** — confirm ownership, who built it, full source code access. The `/otpGenerate.api` endpoint must be confirmed before the OTP/registration domain can be specced. | Annique IT / management | 🟠 Before domain specs | ANN-15; OTP section of communications spec |
| C2 | **TC-05 — Order cancellation** — confirm `SyncCancelOrders.api` source and `sp_ws_invoicedtickets` status. Does the SP exist on production AM? | Marcel Truter / IT | 🟠 Before domain specs | ANN-16; order cancellation write-back spec |
| C3 | **AccountMate support contact** — confirm the reseller or direct AccountMate support contact for licence transfer. Support # is **ANNI619265**. | Melinda / management | 🟡 Before production | AM migration licence transfer |
| C4 | **Payment provider** — confirm AdumoOnline / PayU SA integration details for Shopify. Who manages the relationship? | Adele du Toit / management | 🟡 Before go-live | PayU reconciliation domain |
| C5 | **Production AM cutover window** — what is the acceptable downtime window? (Target: 30–60 minutes; after hours or weekend) | Melinda / management | 🟡 Before Track B | AM migration cutover scheduling |
| C6 | **Namibia store** — is `amanniquenam` (Namibia ERP) actively used? Does Namibia need a separate Shopify store? | Melinda / management | 🟡 Phase 2 scoping | Namibia scope |

### Approvals (required at go-live)

| # | What | Who at Annique | When |
|---|---|---|---|
| P1 | **Parallel run sign-off** — review and approve the parallel run comparator report (DBM vs NISource output over 4-week window). Zero unresolved divergences required. | Melinda Kotze + management | Pre-cutover |
| P2 | **Production deployment approval** — two human approvers required in the GitHub Actions `deploy-production.yml` workflow. Designated approvers must have GitHub accounts. | Melinda Kotze + one other (TBD) | Each production deploy |

---

## Part 2 — Annique Action Steps in Process Order

Steps are grouped by programme phase. Each step is owned by a specific person at Annique.

---

### NOW — Unblocking the build (week of 2026-04-08)

These are blocking active work right now.

**Step 1 — Azure subscription access (Melinda / IT)**  
Grant Dieselbrook `Owner` role (or `Contributor + User Access Administrator`) on the Azure subscription currently hosting the NopCommerce site.  
- Why: We cannot provision any Azure infrastructure (VNets, App Services, VMs, Key Vault, etc.) without this access.
- How: Azure Portal → Subscriptions → select subscription → Access control (IAM) → Add role assignment → Owner → assign to `mariusbloemhof@gmail.com` or a Dieselbrook service account.
- Fits into: Pre-Phase 1 (before any Azure Bicep deploy)

**Step 2 — Shopify Plus confirmation (Adele / management)**  
Confirm the Shopify Plus plan is active (or confirm when it will be activated). Invite Dieselbrook as a Shopify Partner collaborator.  
- Why: Shopify Functions (Rust, for pricing and exclusive items) require Shopify Plus. The staging development store also needs to be on Shopify Plus.
- How: Shopify Admin → Settings → Plan → confirm Plus. For Partner access: Shopify Partner Dashboard → Stores → invite collaborator.
- Fits into: Pre-Phase 1 (needed before any Shopify app or Function work)

**Step 3 — Production AM backup (Marcel)**  
Take a SQL Server backup of all 4 databases and upload to Azure Blob Storage:
- `amanniquelive`, `compplanLive`, `compsys`, `amanniquenam`
- Storage account: `stdbmstaging` · container: `am-backups`
- File naming: `{database}_{YYYYMMDD}.bak` (e.g. `amanniquelive_20260408.bak`)

Alternatively, Dieselbrook can VPN in and run the backup commands directly (via VPN + RDP already provided) — just confirm it is acceptable to do so.  
- Why: Required to seed `vm-am-staging` with production data for integration testing.
- Fits into: Phase 3 of the setup flow (AM VM seed, before integration tests can run)

---

### SHORTLY — Before staging environment goes live (~1–2 weeks)

**Step 4 — Run AM diagnostic queries on test server (Marcel)**  
Connect to the test AM server (172.19.16.101, which we already have VPN + RDP access to) and run the diagnostic SQL queries from ANN-24. Dieselbrook can do this directly if Marcel confirms it is acceptable.

If Marcel runs them himself, copy/paste the output from SSMS and send to Deon / Marius. The queries are:

```sql
-- 1. SQL Server version + edition
SELECT @@VERSION;
SELECT SERVERPROPERTY('Edition'), SERVERPROPERTY('ProductVersion');

-- 2. Databases
SELECT name, compatibility_level, recovery_model_desc FROM sys.databases ORDER BY name;

-- 3. SQL Agent jobs
SELECT name, enabled, description FROM msdb.dbo.sysjobs ORDER BY name;

-- 4. Linked servers
SELECT name, product, data_source FROM sys.servers WHERE is_linked = 1;

-- 5. Logins
SELECT name, type_desc, is_disabled FROM sys.server_principals
WHERE type IN ('S','U') ORDER BY name;

-- 6. Windows Server version
-- Run in PowerShell or Command Prompt: winver
-- Or: [System.Environment]::OSVersion.VersionString
```

- Why: Needed to confirm SQL Server edition (for Azure licence planning) and SQL Agent jobs (must be recreated after migration).
- Fits into: ANN-24 Track B assessment

**Step 5 — Resolve TC-04 — OTP / shopapi.annique.com (IT + management)**  
Identify who owns `shopapi.annique.com` and obtain or confirm:
- The source code or hosting details for the OTP endpoint (`/otpGenerate.api`)
- Whether this will continue running alongside DBM or be replaced

- Why: DBM cannot spec or build the OTP/registration domain until this is confirmed. TC-04 is blocking ANN-15.
- Fits into: Before domain spec for Communications is written (ANN-10)

**Step 6 — Resolve TC-05 — Order cancellation (Marcel / IT)**  
On the production or test AM server, confirm:
1. Does stored procedure `sp_ws_invoicedtickets` exist?
2. Is `SyncCancelOrders.api` currently handling order cancellations in production?

```sql
-- Check on AMSERVER-v9 / test server:
SELECT name FROM sys.procedures WHERE name LIKE '%cancel%' OR name LIKE '%invoiced%';
```

- Why: TC-05 is blocking the order cancellation write-back spec and parity fixtures.
- Fits into: Before Order Write-Back domain spec (ANN-8) is finalised

---

### BEFORE PRODUCTION DEPLOYMENT — Critical path items

**Step 7 — FortiGate S2S IPsec VPN setup (Marcel)**  
Configure a site-to-site IPsec tunnel from the Annique FortiGate to the Azure VPN Gateway (`vgw-dbm-prod`) in the DBM production VNet.

Dieselbrook provides:
- Azure VPN Gateway public IP (output from Bicep production deploy)
- Pre-shared key (`vpn-shared-key` — generated by Dieselbrook, shared securely)
- Azure VNet address space: `10.3.0.0/16`

Marcel configures on the FortiGate:
- IPsec Phase 1: IKEv2, AES-256, SHA-256, DH Group 2
- IPsec Phase 2: AES-256, SHA-256, PFS Group 2
- Route `10.3.0.0/16` over the tunnel
- Fortinet publish an Azure-specific guide — search "FortiGate Azure VPN Gateway IPsec site to site"

Verification: Dieselbrook runs `verify-am-connection.ps1` from the production App Service against `172.19.16.100:1433` — all SPs must return successfully.

- Why: Production DBM must reach AMSERVER-v9 (`172.19.16.100`) via private routing. The S2S VPN is the confirmed connectivity path. Without this, production cannot be verified.
- Fits into: Phase 7 of the infrastructure setup (after Bicep production deploy)

**Step 8 — Contact AccountMate support re licence (Melinda / management)**  
Call AccountMate support (or the Annique AccountMate reseller) to initiate the licence transfer discussion.

Support number: **ANNI619265**  
Script for the call: *"We are planning to migrate our AccountMate 9.3 server from an on-premises Windows Server to an Azure IaaS virtual machine running Windows Server 2022. We need to understand the licence transfer process. Our current server is AMSERVER-v9 and our support number is ANNI619265."*

- Why: Licence transfers can take 1–3 business days. Starting the conversation early avoids a last-minute blocker.
- Fits into: Track B planning; should start 2–4 weeks before planned AM migration cutover
- Does NOT block go-live — this is for Track B (AM migration), not Track A (DBM go-live)

---

### PRE-GO-LIVE SIGN-OFF

**Step 9 — Designate two GitHub production approvers (Melinda + one other)**  
Two people at Annique (or Dieselbrook, per agreement) need GitHub accounts added as required reviewers on the `production` GitHub Actions environment. They will receive an email notification and must click Approve before any production deployment can proceed.

- Why: The `deploy-production.yml` workflow requires two human approvals as a safety gate.
- Who: Melinda Kotze + one other (Adele du Toit or Deon Kretzschmar)
- Fits into: Before the first production deployment

**Step 10 — Parallel run sign-off (Melinda + management)**  
Review the parallel run comparator report produced by Dieselbrook. This report shows DBM vs NISource output over a 4-week window running against the same Shopify events.

What Annique reviews:
- Any documented divergences (DBM output differs from NISource output)
- Confirmation that divergences are either confirmed as improvements or have been investigated and resolved
- Sign-off that the parallel run is clean and production cutover can proceed

- Why: This is the primary business assurance mechanism. No production go-live without this sign-off.
- Fits into: End of Phase 1 / pre-cutover

---

### TRACK B — AM MIGRATION (post go-live, no fixed deadline)

These steps happen after DBM is live in production. They are not urgent — schedule at Annique's convenience.

**Step 11 — Confirm AM migration window (Melinda / management)**  
Agree on the acceptable downtime window for the AM server migration. Target: 30–60 minutes. Options: Friday evening, Saturday morning, or any after-hours window when no AM transactions are expected.

**Step 12 — Final production AM backup for migration (Marcel)**  
On the day before migration cutover, take a fresh final backup of all 4 AM databases and upload to Azure Blob (same process as Step 3 above). This is the backup that gets restored to `vm-am-prod`.

**Step 13 — ODBC update on AM client machines (Marcel)**  
After the AM VM is running in Azure and verified, update the ODBC System DSN on all AM client workstations:
- Old server: `172.19.16.100` (AMSERVER-v9 on-prem)
- New server: `10.3.2.x` (vm-am-prod private IP in Azure VNet, confirmed after provisioning)

If Active Directory is in use: Dieselbrook provides a Group Policy ADM template that pushes the ODBC update centrally.  
If no Active Directory: Marcel visits each workstation and updates manually (Control Panel → Administrative Tools → ODBC Data Sources (64-bit) → System DSN → AccountMate → Edit → Server).

**Step 14 — AccountMate licence activation on Azure VM (Marcel / Annique management)**  
On cutover day, after the AM databases are restored and verified on `vm-am-prod`:
1. Activate the AccountMate licence on the Azure VM (via AccountMate support — ANNI619265)
2. Deactivate the on-prem AMSERVER-v9 licence
3. Open AM on a client machine → connect → verify data loads correctly
4. Run a test transaction

**Step 15 — Decommission AMSERVER-v9 (Marcel)**  
After 1 week of stable AM operation on Azure with no issues: power down AMSERVER-v9. Keep it offline (do not destroy) for a further 2 weeks as a cold fallback. After 3 weeks of stable operation: decommission.

---

## Summary Table — Owner and Timing

| Step | Action | Owner at Annique | Timing |
|---|---|---|---|
| 1 | Grant Azure Owner access | Melinda / IT | **Now — blocking** |
| 2 | Confirm Shopify Plus + Partner access | Adele / management | **Now — blocking** |
| 3 | Provide production AM backup (.bak) | Marcel Truter | Within 1 week |
| 4 | Run AM diagnostic queries on test server | Marcel Truter | Within 1 week |
| 5 | Resolve TC-04 (shopapi.annique.com OTP) | IT + management | Before comms domain spec |
| 6 | Resolve TC-05 (cancellation SP) | Marcel / IT | Before order spec finalised |
| 7 | Configure FortiGate S2S IPsec VPN to Azure | Marcel Truter | Before production deploy |
| 8 | Contact AccountMate support (ANNI619265) | Melinda / management | 2–4 weeks before AM migration |
| 9 | Add GitHub production approvers | Melinda + 1 other | Before first production deploy |
| 10 | Parallel run sign-off | Melinda + management | Pre-cutover |
| 11 | Confirm AM migration window | Melinda / management | Post go-live, Track B |
| 12 | Final AM backup for migration | Marcel Truter | Day before AM cutover |
| 13 | ODBC update on AM client machines | Marcel Truter | AM cutover day |
| 14 | AccountMate licence activation on Azure | Marcel + management | AM cutover day |
| 15 | Decommission AMSERVER-v9 | Marcel Truter | 3 weeks post AM cutover |

---

## Key Contacts

| Role | Name | Contact |
|---|---|---|
| Business Analyst / Programme coordinator | Melinda Kotze | Melinda@Annique.com · +27 12 345 9800 |
| Marketing Manager | Adele du Toit | Adele.DuToit@annique.com · +27 82 820 3915 |
| IT Systems Administrator | Marcel Truter | ITSUPPORT@annique.com · +27 12 345 9800 |

---

## Reference Documents

| Document | Location |
|---|---|
| Infrastructure specification (Azure resources) | `docs/spec/03_infrastructure.md` |
| Environment architecture | `docs/spec/04_environments.md` |
| Azure setup + AM migration guide | `docs/analysis/27_azure_setup_and_am_migration.md` |
| AccountMate migration issue | Linear ANN-24 |
| Technical confirmations (TC-04, TC-05) | Linear ANN-15, ANN-16 |
