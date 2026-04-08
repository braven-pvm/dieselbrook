# DBM Infrastructure Specification — Azure Resources
## Dieselbrook Middleware (DBM) — Annique Cosmetics Platform Modernisation

**Document type:** Infrastructure Specification  
**Programme:** Annique → Shopify Plus migration and Dieselbrook Middleware build  
**Version:** 0.1 — initial  
**Date:** 2026-04-07  
**Status:** Draft — actionable  
**Linear:** ANN-21  

---

## Decision Dependencies

| Decision ID | Topic | Status |
|---|---|---|
| All programme decisions | D-01 through D-10, TC-02/03/06/07/08 | ✅ All closed 2026-03-15 |

**This spec is fully actionable.** Infrastructure provisioning can begin immediately.

---

## 1. Prerequisites and Access Setup

Before any provisioning begins, the following access must be in place.

### 1.1 Azure access required

| Access | Who needs it | Purpose |
|---|---|---|
| Azure subscription — Owner role | Architect (Braven Lab) | Provision all resources, create managed identities, assign RBAC roles |
| Azure Active Directory (Entra ID) — Application Administrator | Architect | Register app, create service principals |
| Annique's existing Azure subscription | Confirm with Annique IT | DBM may deploy into the same subscription as the existing Annique Azure estate (NopCommerce, AnniqueAPI) or a new dedicated subscription. **Confirm before provisioning.** |
| GitHub repository — Admin | Architect | Create secrets, configure Actions runners, set environments |
| Shopify Partner account | Dieselbrook | Create development stores for staging and parity environments |
| Shopify Plus store — Collaborator access | Architect | Install custom app, register webhooks, deploy Shopify Functions |

### 1.2 Subscription decision

**Option A — Shared subscription (recommended for cost simplicity):** DBM resources deploy into the same Azure subscription as the existing Annique estate. Resource groups isolate environments. Simpler billing, no cross-subscription networking complexity.

**Option B — Dedicated subscription:** DBM gets its own subscription. Cleaner separation, separate billing, but requires subscription-level VNet peering to reach existing Annique resources and any shared VPN gateway.

**Default assumption in this spec:** Option A — shared subscription, DBM resources in new resource groups alongside the existing estate.

### 1.3 Azure CLI and tooling required

```powershell
# Required on the provisioning machine
az --version           # Azure CLI ≥ 2.58
az bicep --version     # Bicep CLI ≥ 0.25
git --version          # Git ≥ 2.40
gh --version           # GitHub CLI (for secret setup)
sqlcmd -?              # SQL Server command-line tools (for DB seed scripts)
```

Install if missing:
```powershell
# Azure CLI
winget install -e --id Microsoft.AzureCLI

# Bicep (installed via az)
az bicep install

# GitHub CLI
winget install -e --id GitHub.cli

# SQLCMD (part of SQL Server tools)
winget install -e --id Microsoft.SQLServerCommandLineUtilities
```

### 1.4 Required secrets to have ready before provisioning

Gather these before running any provisioning scripts. They will be loaded into Key Vault at provisioning time.

| Secret | Source | Notes |
|---|---|---|
| `am-sql-password` | Create a new strong password | Password for `dbm_svc` SQL login on AM SQL Server |
| `dbm-state-db-password` | Create a new strong password | Azure SQL admin password for DBM State DB (managed identity used at runtime; this is the AAD admin backup) |
| `shopify-admin-api-token` | Shopify Partner Dashboard | After app installation in each environment |
| `shopify-webhook-signing-secret` | Shopify Partner Dashboard | After app creation |
| `brevo-api-key` | Brevo account (new dedicated DBM instance) | New Brevo account — not the legacy account |
| `sms-provider-api-key` | SMS provider (TC-03 resolved — provider TBD) | |
| `meta-capi-token` | Meta Business Manager | For Meta Conversions API event forwarding |
| `vpn-shared-key` | Generate strong random string | Pre-shared key for FortiGate S2S IPsec tunnel (production only) — share with Marcel Truter |

---

## 2. Azure Region and Resource Organisation

### 2.1 Azure region

**Primary region:** `southafricanorth` (South Africa North — Johannesburg)

Rationale: Annique is a South African business. AccountMate is on-premises in South Africa. DBM must have low-latency connectivity to AM. South Africa North is the closest Azure region to the AM SQL estate.

**No secondary region in phase 1.** DBM is not geo-redundant in the initial deployment. If the business requires multi-region failover, that is a post-phase-1 topic. Azure Backup provides point-in-time recovery for the AM IaaS VM.

### 2.2 Resource groups

Three resource groups, one per environment. All in `southafricanorth`.

| Resource Group | Environment | Description |
|---|---|---|
| `rg-dbm-staging` | Staging | All staging resources — DBM Core, Admin Console, DBM State DB, Service Bus, Key Vault, AM VM, Bastion, networking |
| `rg-dbm-parity` | Parity | Parity harness resources — can be torn down between runs to save cost |
| `rg-dbm-prod` | Production | All production resources — same layout as staging, scaled up |

### 2.3 Resource naming convention

Pattern: `{type}-{service}-{environment}[-{disambiguator}]`

| Resource type | Abbreviation | Staging example | Production example |
|---|---|---|---|
| Resource group | `rg` | `rg-dbm-staging` | `rg-dbm-prod` |
| Virtual network | `vnet` | `vnet-dbm-staging` | `vnet-dbm-prod` |
| Subnet (general) | `snet` | `snet-app-staging` | `snet-app-prod` |
| Network security group | `nsg` | `nsg-am-staging` | `nsg-am-prod` |
| Azure Bastion | `bas` | `bas-dbm-staging` | `bas-dbm-prod` |
| Public IP (Bastion) | `pip` | `pip-bas-dbm-staging` | `pip-bas-dbm-prod` |
| App Service Plan | `asp` | `asp-dbm-staging` | `asp-dbm-prod` |
| App Service (DBM Core) | `app` | `app-dbm-core-staging` | `app-dbm-core-prod` |
| App Service (Admin Console) | `app` | `app-dbm-admin-staging` | `app-dbm-admin-prod` |
| Azure SQL Server | `sql` | `sql-dbm-staging` | `sql-dbm-prod` |
| Azure SQL Database | `sqldb` | `sqldb-dbm-state-staging` | `sqldb-dbm-state-prod` |
| Azure Service Bus NS | `sb` | `sb-dbm-staging` | `sb-dbm-prod` |
| Azure Key Vault | `kv` | `kv-dbm-staging` | `kv-dbm-prod` |
| Application Insights | `appi` | `appi-dbm-staging` | `appi-dbm-prod` |
| Log Analytics Workspace | `log` | `log-dbm-staging` | `log-dbm-prod` |
| Storage Account (backups) | `st` | `stdbmbackupstg` | `stdbmbackupprd` |
| Virtual Machine (AM) | `vm` | `vm-am-staging` | `vm-am-prod` |
| VM OS disk | `osdisk` | `osdisk-am-staging` | `osdisk-am-prod` |
| VM data disk | `datadisk` | `datadisk-am-staging` | `datadisk-am-prod` |
| Managed Identity | `id` | `id-dbm-core-staging` | `id-dbm-core-prod` |
| Recovery Services Vault | `rsv` | `rsv-dbm-staging` | `rsv-dbm-prod` |

> **Storage account names** must be globally unique, 3–24 characters, lowercase alphanumeric only. Add a short random suffix if the base name is taken: `stdbmbackupstg4f2`.

---

## 3. Networking

### 3.1 VNet design (per environment)

Each environment has its own isolated VNet. No VNet peering between staging and production.

**Staging VNet:**

| Parameter | Value |
|---|---|
| Name | `vnet-dbm-staging` |
| Address space | `10.1.0.0/16` |
| Region | `southafricanorth` |
| DNS servers | Azure default (168.63.129.16) |

**Production VNet:**

| Parameter | Value |
|---|---|
| Name | `vnet-dbm-prod` |
| Address space | `10.3.0.0/16` |
| Region | `southafricanorth` |

**Parity VNet:**

| Parameter | Value |
|---|---|
| Name | `vnet-dbm-parity` |
| Address space | `10.2.0.0/16` |

### 3.2 Subnets (shown for staging — apply same pattern to prod/parity)

| Subnet name | CIDR | Purpose | Service delegation |
|---|---|---|---|
| `snet-app-staging` | `10.1.1.0/24` | App Service VNet Integration (DBM Core + Admin Console) | `Microsoft.Web/serverFarms` |
| `snet-am-staging` | `10.1.2.0/24` | AccountMate IaaS VM (private, no public IP) | None |
| `snet-data-staging` | `10.1.3.0/24` | Azure SQL private endpoint | None |
| `AzureBastionSubnet` | `10.1.4.0/27` | Azure Bastion (must use this exact name) | None |
| `snet-servicebus-staging` | `10.1.5.0/24` | Service Bus private endpoint (production only; Standard tier uses public + IP filter) | None |

> `AzureBastionSubnet` is fixed — Azure Bastion requires this exact subnet name and a minimum /27.

### 3.3 Network Security Groups

#### NSG: `nsg-am-staging` (attached to `snet-am-staging`)

**Inbound rules:**

| Priority | Name | Source | Port | Protocol | Action |
|---|---|---|---|---|---|
| 100 | Allow-SQL-from-AppService | `snet-app-staging` (10.1.1.0/24) | 1433 | TCP | Allow |
| 110 | Allow-RDP-from-Bastion | `AzureBastionSubnet` (10.1.4.0/27) | 3389 | TCP | Allow |
| 120 | Allow-WinRM-from-Bastion | `AzureBastionSubnet` (10.1.4.0/27) | 5985-5986 | TCP | Allow |
| 4000 | Deny-All-Inbound | Any | Any | Any | Deny |

**Outbound rules:**

| Priority | Name | Destination | Port | Protocol | Action |
|---|---|---|---|---|---|
| 100 | Allow-Internet-for-SQL-Updates | Internet | 443 | TCP | Allow |
| 200 | Allow-AzureMonitor | AzureMonitor | 443 | TCP | Allow |
| 4000 | Deny-All-Outbound | Any | Any | Any | Deny |

> No public IP on the AM VM. The only inbound path is from the app subnet (SQL) and from Bastion (RDP for admin). All other inbound is denied.

#### NSG: `nsg-app-staging` (attached to `snet-app-staging`)

App Service VNet Integration subnet — App Service manages its own egress; NSG controls inbound to the subnet only. Typically no restrictions needed here since App Service VNet Integration is egress-only.

### 3.4 Azure Bastion

Azure Bastion provides browser-based RDP to the AM VM without exposing a public IP or opening port 3389 to the internet.

| Parameter | Value |
|---|---|
| Name | `bas-dbm-staging` |
| SKU | Basic (sufficient for admin access) |
| Public IP | `pip-bas-dbm-staging` (Standard SKU, static) |
| Subnet | `AzureBastionSubnet` (10.1.4.0/27) |

> **Cost note:** Azure Bastion Basic costs ~$140/month when running. For staging/parity, consider **Just-in-Time (JIT) VM access** via Microsoft Defender for Cloud as the lower-cost alternative. JIT opens RDP only for the duration of the session and closes it automatically. Use Bastion for production where security posture is more important.

**Provision command:**
```bash
az network bastion create \
  --name bas-dbm-staging \
  --resource-group rg-dbm-staging \
  --vnet-name vnet-dbm-staging \
  --location southafricanorth \
  --sku Basic \
  --public-ip-address pip-bas-dbm-staging
```

### 3.5 Private connectivity to on-premises AMSERVER-v9

> **✅ Confirmed 2026-04-08:** Annique office has a **FortiGate firewall** (IT contact: Marcel Truter). Production connectivity via **FortiGate S2S IPsec VPN** to Azure. No new hardware required. This resolves the production connectivity question — go-live is not blocked by AM migration.

For the production environment, DBM's App Service reaches `AMSERVER-v9` (`172.19.16.100:1433`) via a site-to-site IPsec VPN tunnel between `vnet-dbm-prod` and the Annique office FortiGate.

```
DBM Core App Service (Azure)
  → VNet Integration → snet-app (10.3.1.0/24)
  → Azure VPN Gateway (vgw-dbm-prod)
  ← IPsec S2S IKEv2 tunnel →
  Annique FortiGate (away1.annique.com)
  → Annique office LAN (172.19.x.x)
  → AMSERVER-v9 (172.19.16.100:1433)
```

DBM's connection string uses `172.19.16.100:1433` unchanged. No code changes are needed when the S2S tunnel is in place. The tunnel is transparent to the application.

**Azure-side resources required (production only):**

| Resource | Bicep module | SKU | Est. cost |
|---|---|---|---|
| Azure VPN Gateway | `vpn-gateway.bicep` | VpnGw1 (route-based IKEv2) | ~$140/month |
| Local Network Gateway | included in `vpn-gateway.bicep` | — | ~$0 |
| VPN Connection | included in `vpn-gateway.bicep` | IKEv2, pre-shared key | ~$0 |

**FortiGate-side configuration (Marcel Truter, Annique IT):**
- Create a new IPsec phase 1/2 tunnel pointing at the Azure VPN Gateway public IP
- Phase 1: IKEv2, AES-256, SHA-256, DH group 2 (Azure defaults)
- Phase 2: AES-256, SHA-256, PFS group 2
- Route `10.3.0.0/16` (DBM prod VNet) over the tunnel
- Fortinet publish an Azure-specific S2S guide: search "FortiGate Azure VPN Gateway IPsec"

> **For staging/parity:** Not applicable — these environments use a local Azure IaaS VM for AM. Connectivity is VNet-internal with no VPN required.

> **AM migration to Azure (ANN-24):** This is a parallel, non-blocking track. When AM eventually migrates to `vm-am-prod` inside the DBM prod VNet, the connection string changes from `172.19.16.100` to the VM's private IP (`10.3.2.x`) — one Key Vault secret update, App Service restart. The VPN tunnel can then be decommissioned.

---

## 4. AccountMate IaaS VM

### 4.1 VM Specification

One AM VM per environment (staging, parity, production).

| Parameter | Staging / Parity | Production |
|---|---|---|
| VM name | `vm-am-staging` / `vm-am-parity` | `vm-am-prod` |
| VM SKU | `Standard_D4s_v5` | `Standard_D8s_v5` |
| vCPU | 4 | 8 |
| RAM | 16 GB | 32 GB |
| OS | Windows Server 2022 Datacenter (BYOL or Azure Hybrid Benefit) | Same |
| SQL Server | SQL Server 2022 Developer (staging/parity — free) | SQL Server 2022 Standard |
| Licensing | Azure Hybrid Benefit if Annique has existing SA licenses | Same |
| OS disk | 128 GB Premium SSD P15 | 256 GB Premium SSD P20 |
| Data disk | 512 GB Premium SSD P20 (SQL data + logs) | 1 TB Premium SSD P30 |
| Availability | Single VM, no availability set | Single VM; Azure Backup for recovery |
| Public IP | None | None |
| Subnet | `snet-am-staging` | `snet-am-prod` |
| Private IP | Static: `10.1.2.4` (staging) | Static: `10.3.2.4` (prod) |
| NIC | `nic-am-staging` | `nic-am-prod` |

**Azure Hybrid Benefit:** If Annique has existing Windows Server or SQL Server Software Assurance licences, Azure Hybrid Benefit eliminates the OS/SQL licence charge — significant cost saving (~40–70%).

### 4.2 VM Bicep module (`infra/bicep/modules/am-vm.bicep`)

Key parameters the module exposes:

```bicep
@description('VM name')
param vmName string

@description('Admin username (not sa — Windows administrator only)')
param adminUsername string

@secure()
@description('Admin password — loaded from Key Vault reference')
param adminPassword string

@description('Subnet resource ID for the AM subnet')
param subnetResourceId string

@description('VM SKU')
param vmSize string = 'Standard_D4s_v5'

@description('SQL Server image — Developer or Standard')
@allowed(['sql2022-ws2022', 'sql2022-ws2022-standard'])
param sqlImageSku string = 'sql2022-ws2022'

@description('Data disk size in GB')
param dataDiskSizeGb int = 512

@description('Environment tag')
@allowed(['staging', 'parity', 'production'])
param environment string
```

### 4.3 SQL Server configuration (post-provisioning steps)

After the VM is provisioned, run `scripts/am/configure-sql-server.ps1`:

```powershell
# scripts/am/configure-sql-server.ps1
# Configures SQL Server 2022 on a freshly provisioned VM

param(
    [string]$SqlInstance = ".",
    [int]$MaxMemoryMB = 12288,    # Leave 4 GB for OS on D4s_v5 (16 GB total)
    [string]$SqlDataPath = "D:\SQLData",
    [string]$SqlLogPath  = "D:\SQLLogs",
    [string]$SqlBackupPath = "D:\SQLBackup",
    [string]$TempDbPath = "D:\TempDB"
)

Import-Module SqlServer

# Set max server memory
Invoke-Sqlcmd -ServerInstance $SqlInstance -Query "
    EXEC sp_configure 'show advanced options', 1; RECONFIGURE;
    EXEC sp_configure 'max server memory (MB)', $MaxMemoryMB; RECONFIGURE;"

# Enable SQL Server Auth (Mixed mode)
$reg = [Microsoft.Win32.Registry]::LocalMachine.OpenSubKey(
    'SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQLServer', $true)
$reg.SetValue('LoginMode', 2)  # 2 = Mixed mode

# Configure TempDB — 4 files, 512 MB each, on data disk
Invoke-Sqlcmd -ServerInstance $SqlInstance -Query "
    USE master;
    ALTER DATABASE tempdb MODIFY FILE (NAME='tempdev', FILENAME='$TempDbPath\tempdb.mdf', SIZE=512MB);
    ALTER DATABASE tempdb ADD FILE (NAME='tempdev2', FILENAME='$TempDbPath\tempdev2.ndf', SIZE=512MB);
    ALTER DATABASE tempdb ADD FILE (NAME='tempdev3', FILENAME='$TempDbPath\tempdev3.ndf', SIZE=512MB);
    ALTER DATABASE tempdb ADD FILE (NAME='tempdev4', FILENAME='$TempDbPath\tempdev4.ndf', SIZE=512MB);"

# Set default data/log paths
Invoke-Sqlcmd -ServerInstance $SqlInstance -Query "
    EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE',
        N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultData', REG_SZ, N'$SqlDataPath';
    EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE',
        N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultLog', REG_SZ, N'$SqlLogPath';"

# Enable backup compression by default
Invoke-Sqlcmd -ServerInstance $SqlInstance -Query "
    EXEC sp_configure 'backup compression default', 1; RECONFIGURE;"

# Enable Optimize for Ad Hoc Workloads
Invoke-Sqlcmd -ServerInstance $SqlInstance -Query "
    EXEC sp_configure 'optimize for ad hoc workloads', 1; RECONFIGURE;"

# Restart SQL Server service to apply changes
Restart-Service -Name MSSQLSERVER -Force
Write-Host "SQL Server configured. Restart complete."
```

### 4.4 AM database restore and `dbm_svc` provisioning

Full script: `scripts/am/seed-staging-am.ps1`

```powershell
# scripts/am/seed-staging-am.ps1
# Restores AM production backup to staging-am VM and provisions dbm_svc

param(
    [Parameter(Mandatory)]
    [string]$BackupBlobUrl,          # Azure Blob SAS URL for the .bak file

    [Parameter(Mandatory)]
    [string]$SqlInstance,            # e.g. "vm-am-staging.southafricanorth.cloudapp.azure.com"

    [Parameter(Mandatory)]
    [string]$SaPassword,             # Temporary sa password for restore (sa enabled just for restore, then disabled)

    [Parameter(Mandatory)]
    [string]$DbmSvcPassword          # Password for dbm_svc — retrieved from Key Vault before calling

)

$dataPath   = "D:\SQLData"
$backupPath = "D:\SQLBackup"

# 1. Download backup from Azure Blob to the VM (run on VM via Invoke-Command or RDP)
Write-Host "Downloading backup from Blob Storage..."
azcopy copy "$BackupBlobUrl" "$backupPath\amanniquelive.bak" --overwrite=true

# 2. Restore amanniquelive
Invoke-Sqlcmd -ServerInstance $SqlInstance -Username sa -Password $SaPassword -Query "
    RESTORE DATABASE amanniquelive
    FROM DISK = '$backupPath\amanniquelive.bak'
    WITH
        MOVE 'amanniquelive_data' TO '$dataPath\amanniquelive.mdf',
        MOVE 'amanniquelive_log'  TO '$dataPath\amanniquelive_log.ldf',
        REPLACE, RECOVERY, STATS = 10;"

# 3. Restore compplanLive
Invoke-Sqlcmd -ServerInstance $SqlInstance -Username sa -Password $SaPassword -Query "
    RESTORE DATABASE compplanLive
    FROM DISK = '$backupPath\compplanLive.bak'
    WITH
        MOVE 'compplanLive_data' TO '$dataPath\compplanLive.mdf',
        MOVE 'compplanLive_log'  TO '$dataPath\compplanLive_log.ldf',
        REPLACE, RECOVERY, STATS = 10;"

# 4. Restore compsys (read-only reference)
Invoke-Sqlcmd -ServerInstance $SqlInstance -Username sa -Password $SaPassword -Query "
    RESTORE DATABASE compsys
    FROM DISK = '$backupPath\compsys.bak'
    WITH
        MOVE 'compsys_data' TO '$dataPath\compsys.mdf',
        MOVE 'compsys_log'  TO '$dataPath\compsys_log.ldf',
        REPLACE, RECOVERY, STATS = 10;"

# 5. Set SQL Server compatibility levels
Invoke-Sqlcmd -ServerInstance $SqlInstance -Username sa -Password $SaPassword -Query "
    ALTER DATABASE amanniquelive  SET COMPATIBILITY_LEVEL = 150;
    ALTER DATABASE compplanLive   SET COMPATIBILITY_LEVEL = 150;
    ALTER DATABASE compsys        SET COMPATIBILITY_LEVEL = 150;"

# 6. Create dbm_svc login
Invoke-Sqlcmd -ServerInstance $SqlInstance -Username sa -Password $SaPassword -Query "
    IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'dbm_svc')
        CREATE LOGIN dbm_svc WITH PASSWORD = '$DbmSvcPassword', CHECK_POLICY = OFF;
    ELSE
        ALTER LOGIN dbm_svc WITH PASSWORD = '$DbmSvcPassword';"

# 7. Create dbm_svc user and grant permissions in amanniquelive
Invoke-Sqlcmd -ServerInstance $SqlInstance -Username sa -Password $SaPassword -Database amanniquelive -Query "
    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'dbm_svc')
        CREATE USER dbm_svc FOR LOGIN dbm_svc;

    -- Table reads
    GRANT SELECT ON dbo.icitem        TO dbm_svc;
    GRANT SELECT ON dbo.arcust        TO dbm_svc;
    GRANT SELECT ON dbo.SOPortal      TO dbm_svc;
    GRANT SELECT ON dbo.soxitems      TO dbm_svc;
    GRANT SELECT ON dbo.Campaign      TO dbm_svc;
    GRANT SELECT ON dbo.CampDetail    TO dbm_svc;
    GRANT SELECT ON dbo.CampSku       TO dbm_svc;
    GRANT SELECT ON dbo.arinvc        TO dbm_svc;
    GRANT SELECT ON dbo.soship        TO dbm_svc;
    GRANT SELECT ON dbo.sostrs        TO dbm_svc;
    GRANT SELECT ON dbo.sosord        TO dbm_svc;
    GRANT SELECT ON dbo.soxitem       TO dbm_svc;
    GRANT SELECT ON dbo.soskit        TO dbm_svc;
    GRANT SELECT ON dbo.iciimgUpdateNOP TO dbm_svc;

    -- Stored procedure executes
    GRANT EXECUTE ON dbo.sp_camp_getSPprice   TO dbm_svc;
    GRANT EXECUTE ON dbo.sp_ws_syncorder      TO dbm_svc;
    GRANT EXECUTE ON dbo.sp_ws_reactivate     TO dbm_svc;
    GRANT EXECUTE ON dbo.sp_ws_gensoxitems    TO dbm_svc;
    -- TC-05: add cancellation SP here when confirmed
"

# 8. Create dbm_svc user and grant permissions in compplanLive
Invoke-Sqlcmd -ServerInstance $SqlInstance -Username sa -Password $SaPassword -Database compplanLive -Query "
    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'dbm_svc')
        CREATE USER dbm_svc FOR LOGIN dbm_svc;

    GRANT SELECT ON dbo.CTstatement   TO dbm_svc;
    GRANT SELECT ON dbo.CTdownlineh   TO dbm_svc;
    GRANT SELECT ON dbo.CTstatementh  TO dbm_svc;
    -- Add further hierarchy/downline tables as domain specs confirm"

# 9. Grant read-only access to compsys (boundary confirmation only — no writes ever)
Invoke-Sqlcmd -ServerInstance $SqlInstance -Username sa -Password $SaPassword -Database compsys -Query "
    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'dbm_svc')
        CREATE USER dbm_svc FOR LOGIN dbm_svc;
    -- Read-only, explicitly no EXECUTE on sp_Sendmail
    GRANT SELECT ON dbo.MAILMESSAGE TO dbm_svc;"

# 10. Disable sa account (not needed after provisioning)
Invoke-Sqlcmd -ServerInstance $SqlInstance -Username sa -Password $SaPassword -Query "
    ALTER LOGIN sa DISABLE;"

Write-Host "AM seed complete. dbm_svc provisioned. sa disabled."
```

### 4.5 Backup policy for AM VM

Azure Backup is configured via the Recovery Services Vault.

| Parameter | Staging | Production |
|---|---|---|
| Vault name | `rsv-dbm-staging` | `rsv-dbm-prod` |
| Policy name | `policy-am-staging` | `policy-am-prod` |
| Backup frequency | Daily at 02:00 UTC | Daily at 02:00 UTC + weekly long-term |
| Instant restore | 2 days | 5 days |
| Daily retention | 30 days | 90 days |
| Weekly retention | 4 weeks | 52 weeks |
| Monthly retention | — | 12 months |

Additionally: manual SQL-native backups via `BACKUP DATABASE` to the data disk (`D:\SQLBackup`) are uploaded to Azure Blob Storage as the seed backup source. These are not Azure Backup — they are SQL Server native backups used for the seed procedure.

```powershell
# scripts/am/take-production-backup.ps1
# Run against production AM to create a fresh seed backup
# Upload result to Azure Blob: stdbmbackupstg/am-backups/

$backupPath = "D:\SQLBackup"
$timestamp  = Get-Date -Format "yyyyMMdd_HHmmss"
$databases  = @("amanniquelive", "compplanLive", "compsys")

foreach ($db in $databases) {
    $file = "$backupPath\${db}_${timestamp}.bak"
    Invoke-Sqlcmd -Query "
        BACKUP DATABASE [$db]
        TO DISK = '$file'
        WITH COMPRESSION, CHECKSUM, STATS = 10;"
    Write-Host "Backup of $db complete: $file"

    # Upload to Azure Blob
    azcopy copy $file "https://stdbmbackupstg.blob.core.windows.net/am-backups/${db}_${timestamp}.bak"
}
```

---

## 5. App Service Plan and App Services

### 5.1 App Service Plan

| Parameter | Staging | Production |
|---|---|---|
| Name | `asp-dbm-staging` | `asp-dbm-prod` |
| OS | Linux | Linux |
| SKU | P2v3 (2 vCPU, 8 GB RAM) | P3v3 (4 vCPU, 16 GB RAM) |
| Per-site scaling | Enabled | Enabled |
| Region | `southafricanorth` | `southafricanorth` |

> **Why Linux?** .NET 9 runs natively on Linux App Service at ~20% lower cost than Windows App Service. No Windows dependencies in DBM Core.

> **Why P2v3 for staging?** The plan must support VNet Integration, which requires at minimum P1v3. P2v3 gives adequate memory for the 25 background workers running simultaneously during sync windows.

### 5.2 DBM Core App Service

| Parameter | Staging | Production |
|---|---|---|
| Name | `app-dbm-core-staging` | `app-dbm-core-prod` |
| Runtime stack | .NET 9 | .NET 9 |
| OS | Linux | Linux |
| Deployment slots | `staging` (swap target), `production` (current) | Same |
| Always On | Enabled | Enabled |
| VNet Integration | `snet-app-staging` | `snet-app-prod` |
| Managed Identity | System-assigned | System-assigned |
| Health check path | `/health` | `/health` |

**Application settings (environment variables):**

| Key | Value | Source |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Staging` / `Production` | Hard-coded per slot |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | App Insights connection string | Key Vault reference |
| `AzureKeyVaultUri` | `https://kv-dbm-staging.vault.azure.net/` | Hard-coded |
| `AM__Host` | `10.1.2.4` (staging private IP) | Hard-coded per environment |
| `AM__Port` | `1433` | Hard-coded |
| `AM__Database` | `amanniquelive` | Hard-coded |
| `AM__Username` | `dbm_svc` | Hard-coded |
| `AM__Password` | `@Microsoft.KeyVault(VaultName=kv-dbm-staging;SecretName=am-sql-password)` | Key Vault reference |
| `AM__Enabled` | `true` | Hard-coded (false for local only) |
| `ServiceBus__Namespace` | `sb-dbm-staging.servicebus.windows.net` | Hard-coded |
| `Quartz__DataSource` | `@Microsoft.KeyVault(...)` for DBM State DB | Key Vault reference |
| `Shopify__StoreDomain` | `annique-staging.myshopify.com` | Hard-coded per environment |
| `Shopify__ApiVersion` | `2024-07` | Hard-coded |
| `Shopify__AccessToken` | `@Microsoft.KeyVault(VaultName=kv-dbm-staging;SecretName=shopify-admin-api-token)` | Key Vault reference |
| `Shopify__WebhookSecret` | `@Microsoft.KeyVault(VaultName=kv-dbm-staging;SecretName=shopify-webhook-signing-secret)` | Key Vault reference |

**Deployment slot swap process:**
1. Deploy new version to `staging` slot (swap slot — separate URL, separate settings)
2. Run smoke tests against `staging` slot
3. Perform slot swap (zero downtime — Azure swaps routing)
4. Production slot is now running the new version

### 5.3 Admin Console App Service (Node.js / Remix)

| Parameter | Staging | Production |
|---|---|---|
| Name | `app-dbm-admin-staging` | `app-dbm-admin-prod` |
| Runtime stack | Node.js 20 LTS | Node.js 20 LTS |
| OS | Linux | Linux |
| VNet Integration | `snet-app-staging` (same subnet as Core) | `snet-app-prod` |
| Managed Identity | System-assigned | System-assigned |

**Application settings:**

| Key | Value |
|---|---|
| `NODE_ENV` | `production` (always — Remix production mode) |
| `DBM_API_BASE_URL` | `https://app-dbm-core-staging.azurewebsites.net` (internal preferred if VNet-routable) |
| `SHOPIFY_API_KEY` | Key Vault reference |
| `SHOPIFY_API_SECRET` | Key Vault reference |
| `SESSION_SECRET` | Key Vault reference — random 32-byte secret for Remix session signing |
| `PORT` | `3000` |

---

## 6. Azure SQL Database — DBM State

### 6.1 Specification

| Parameter | Staging | Production |
|---|---|---|
| Logical server name | `sql-dbm-staging` | `sql-dbm-prod` |
| Database name | `sqldb-dbm-state-staging` | `sqldb-dbm-state-prod` |
| Compute tier | Serverless (auto-pause after 1h idle) | Provisioned |
| SKU | `GP_S_Gen5_2` (2 vCores, 0.5–2 autoscale) | `GP_Gen5_4` (4 vCores, 20.4 GB) |
| Max storage | 32 GB | 100 GB |
| Geo-redundant backup | Disabled (staging) | Enabled (production) |
| Backup retention | 7 days | 35 days |
| Connectivity | Private endpoint in `snet-data-staging` | Private endpoint in `snet-data-prod` |
| AAD admin | Architect's AAD account (setup) + DBM Managed Identity | Same |
| SQL auth | Enabled initially (for migrations); disable after first deploy | Enabled for emergency access only |
| TLS | TLS 1.2 minimum | TLS 1.2 minimum |
| Firewall | No public access — private endpoint only | Same |

> **Why serverless for staging?** Staging is not always active. Auto-pause after 1 hour of inactivity saves ~60% of compute cost. First connection after pause takes ~30 seconds to resume — acceptable for staging.

### 6.2 Connection string format

Used in DBM Core `appsettings.{Environment}.json` or App Service settings:

```
Server=sql-dbm-staging.database.windows.net,1433;
Database=sqldb-dbm-state-staging;
Authentication=Active Directory Managed Identity;
Encrypt=true;
TrustServerCertificate=false;
Connection Timeout=30;
```

The Managed Identity on `app-dbm-core-staging` is granted `db_owner` on `sqldb-dbm-state-staging`. No SQL username/password required for the App Service connection — Managed Identity handles authentication.

### 6.3 Database migrations

DBM State schema is managed via **EF Core migrations** (the only place EF Core is used in DBM — for DBM's own state database, not for AM). This keeps schema versioned in source control and deployable automatically.

```bash
# Run migration on deploy (in GitHub Actions)
dotnet ef database update \
  --project src/DBM.Core \
  --connection "$DBM_STATE_CONNECTION_STRING"
```

Migration is part of the deployment pipeline — runs automatically after App Service deploy, before slot swap.

---

## 7. Azure Service Bus

### 7.1 Specification

| Parameter | Staging | Parity | Production |
|---|---|---|---|
| Namespace name | `sb-dbm-staging` | `sb-dbm-parity` | `sb-dbm-prod` |
| Tier | Standard | Standard | **Premium** |
| Messaging units | — | — | 1 MU (expandable) |
| Geo-redundancy | — | — | Disabled (phase 1) |
| VNet integration | Not available on Standard | Same | Available on Premium |
| Private endpoint | No | No | Yes (`snet-servicebus-prod`) |

> **Why Premium for production?** Standard tier does not support VNet integration — message traffic crosses the public internet (encrypted but not fully isolated). For production, Premium keeps all message traffic within the Azure private network. Standard is acceptable for staging.

### 7.2 Queues and configuration

| Queue | Dead-letter | Lock duration | Max delivery count | Message TTL |
|---|---|---|---|---|
| `order-ingest` | `order-ingest/$DeadLetterQueue` | 5 minutes | 3 | 7 days |
| `fulfillment-sync` | `fulfillment-sync/$DeadLetterQueue` | 5 minutes | 3 | 7 days |
| `consultant-sync` | `consultant-sync/$DeadLetterQueue` | 10 minutes | 3 | 7 days |
| `pricing-sync` | `pricing-sync/$DeadLetterQueue` | 10 minutes | 3 | 7 days |
| `comms-dispatch` | `comms-dispatch/$DeadLetterQueue` | 5 minutes | 3 | 7 days |

**Max delivery count = 3:** After 3 failed attempts, the message moves to the dead-letter queue. DBM's `dead_letter_items` table is populated from the dead-letter queue by the `AuditLogService` consumer. The admin console shows these for operator review and replay.

### 7.3 Access policy

DBM Core uses Managed Identity for Service Bus access. No connection string in code.

```bash
# Assign roles to DBM Core Managed Identity
az role assignment create \
  --assignee <managed-identity-object-id> \
  --role "Azure Service Bus Data Owner" \
  --scope "/subscriptions/.../resourceGroups/rg-dbm-staging/providers/Microsoft.ServiceBus/namespaces/sb-dbm-staging"
```

---

## 8. Azure Key Vault

### 8.1 Specification

| Parameter | Staging | Production |
|---|---|---|
| Name | `kv-dbm-staging` | `kv-dbm-prod` |
| SKU | Standard | Standard |
| RBAC model | Azure RBAC (not legacy access policies) | Same |
| Soft delete | Enabled, 90 days | Enabled, 90 days |
| Purge protection | Disabled (staging — for easy reset) | **Enabled** (production — prevents accidental permanent deletion) |
| Firewall | Disabled (Standard tier — Key Vault SDK uses HTTPS) | Allow Azure services + private endpoint |
| Network | Default (public — VNet integration limits added if needed) | Private endpoint in `snet-data-prod` |

### 8.2 Secrets inventory

All secrets follow the naming pattern: `{service}-{purpose}` in kebab-case.

| Secret name | Content | Rotated |
|---|---|---|
| `am-sql-password` | `dbm_svc` SQL login password on AM SQL Server | Manual |
| `dbm-state-db-admin-password` | Azure SQL logical server admin password (backup access only) | Manual |
| `shopify-admin-api-token` | Shopify Admin API access token for the custom app | On Shopify token rotation |
| `shopify-webhook-signing-secret` | Shopify webhook HMAC signing secret | On Shopify token rotation |
| `brevo-api-key` | Brevo API key (new dedicated DBM Brevo account) | Annual |
| `sms-api-key` | SMS provider API key (provider TBD — TC-03) | Annual |
| `meta-capi-token` | Meta Conversions API token | Annual |
| `admin-console-session-secret` | 32-byte random secret for Remix session signing | Annual |

### 8.3 RBAC role assignments on Key Vault

| Principal | Role | Scope | Purpose |
|---|---|---|---|
| `id-dbm-core-staging` (Managed Identity) | `Key Vault Secrets User` | `kv-dbm-staging` | DBM Core reads secrets at runtime |
| `id-dbm-admin-staging` (Managed Identity) | `Key Vault Secrets User` | `kv-dbm-staging` | Admin Console reads session secret at runtime |
| Architect AAD account | `Key Vault Secrets Officer` | `kv-dbm-staging` | Create/update secrets during deployment |
| GitHub Actions Service Principal | `Key Vault Secrets User` | `kv-dbm-staging` | CI/CD reads connection strings for migration steps |

```bash
# Example: grant DBM Core Managed Identity read access to Key Vault secrets
MI_OBJECT_ID=$(az webapp identity show \
  --name app-dbm-core-staging \
  --resource-group rg-dbm-staging \
  --query principalId -o tsv)

az role assignment create \
  --assignee $MI_OBJECT_ID \
  --role "Key Vault Secrets User" \
  --scope "/subscriptions/.../resourceGroups/rg-dbm-staging/providers/Microsoft.KeyVault/vaults/kv-dbm-staging"
```

### 8.4 Key Vault references in App Service settings

App Service supports Key Vault references directly in application settings — no SDK call required in application code:

```
@Microsoft.KeyVault(VaultName=kv-dbm-staging;SecretName=am-sql-password)
```

The App Service's Managed Identity must have `Key Vault Secrets User` on the vault. References are resolved at runtime and rotated automatically when the secret version changes.

---

## 9. Application Insights and Log Analytics

### 9.1 Log Analytics Workspace

One workspace per environment. Application Insights is workspace-based (logs flow into Log Analytics).

| Parameter | Staging | Production |
|---|---|---|
| Name | `log-dbm-staging` | `log-dbm-prod` |
| SKU | PerGB2018 | PerGB2018 |
| Retention | 90 days | 365 days |
| Daily cap | 2 GB/day (prevents runaway costs during testing) | 5 GB/day |

### 9.2 Application Insights

| Parameter | Staging | Production |
|---|---|---|
| Name | `appi-dbm-staging` | `appi-dbm-prod` |
| Workspace | `log-dbm-staging` | `log-dbm-prod` |
| Sampling | 100% (development visibility more important than cost) | 100% initially |
| Retention | Inherits from workspace | Inherits from workspace |
| Availability tests | None | HTTP ping on `/health` every 5 minutes from 3 locations |

**Serilog sink configuration (in `appsettings.json`):**

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.ApplicationInsights"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "ApplicationInsights",
        "Args": {
          "connectionString": "#{APPLICATIONINSIGHTS_CONNECTION_STRING}#",
          "telemetryConverter": "Serilog.Sinks.ApplicationInsights.TelemetryConverters.TraceTelemetryConverter, Serilog.Sinks.ApplicationInsights"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"],
    "Properties": {
      "Application": "DBM.Core",
      "Environment": "#{ASPNETCORE_ENVIRONMENT}#"
    }
  }
}
```

### 9.3 Production alert rules

Configured in Azure Monitor (linked to Application Insights):

| Alert name | Condition | Severity | Action |
|---|---|---|---|
| `dbm-order-dead-letter` | Dead-letter queue depth > 0 (any order-class queue) | 1 (Critical) | Email + Teams webhook |
| `dbm-pricing-parity-fail` | Custom metric `parity_check_failed` count > 0 | 1 (Critical) | Email |
| `dbm-sync-stale` | Custom metric `sync_watermark_age_minutes` > 20 for pricing | 2 (High) | Email |
| `dbm-health-degraded` | Availability test failure for `/health` | 1 (Critical) | Email + Teams |
| `dbm-sql-unreachable` | Custom metric `am_sql_connection_failed` count > 3 in 5 min | 1 (Critical) | Email + Teams |
| `dbm-high-dead-letter` | Dead-letter queue depth > 10 (any queue) | 2 (High) | Email |
| `dbm-consecutive-sync-fail` | Custom metric `sync_job_consecutive_failures` > 3 | 2 (High) | Email |

Custom metrics are emitted by DBM services via Serilog + Application Insights and tracked in Azure Monitor.

---

## 10. Azure Blob Storage — Backups and Artifacts

### 10.1 Storage account specification

| Parameter | Value |
|---|---|
| Account name | `stdbmbackupstg` (staging) / `stdbmbackupprd` (production) |
| Kind | StorageV2 (general purpose v2) |
| Redundancy | LRS (Locally Redundant — South Africa North) |
| Access tier | Hot (containers set individually) |
| Public access | Disabled — access via SAS URL or Managed Identity |
| Soft delete (blobs) | Enabled, 30-day retention |

### 10.2 Containers

| Container | Access tier | Purpose | Lifecycle policy |
|---|---|---|---|
| `am-backups` | Cool | AM SQL `.bak` files used for staging/parity seed | Delete after 90 days |
| `parity-artifacts` | Cool | Parity run reports (JSON), golden snapshot metadata | Delete after 180 days |
| `infra-state` | Hot | Bicep deployment outputs (parameter values for reference) | Manual |

### 10.3 SAS URL generation for backup uploads

```bash
# Generate SAS URL for the seed backup upload (24-hour expiry)
az storage blob generate-sas \
  --account-name stdbmbackupstg \
  --container-name am-backups \
  --name amanniquelive_20260407.bak \
  --permissions rw \
  --expiry $(date -u -d "+24 hours" '+%Y-%m-%dT%H:%MZ') \
  --output tsv
```

---

## 11. Managed Identities and RBAC

### 11.1 Managed Identity assignments

| Identity name | Assigned to | Roles |
|---|---|---|
| `id-dbm-core-staging` (system-assigned on App Service) | `app-dbm-core-staging` | Key Vault Secrets User (kv-dbm-staging), Azure Service Bus Data Owner (sb-dbm-staging), Azure SQL (db_owner on sqldb-dbm-state-staging via AAD auth) |
| `id-dbm-admin-staging` (system-assigned) | `app-dbm-admin-staging` | Key Vault Secrets User (session secret only) |
| `id-dbm-core-prod` | `app-dbm-core-prod` | Same pattern — prod vault, prod bus, prod DB |
| `id-dbm-admin-prod` | `app-dbm-admin-prod` | Same |

> **System-assigned vs. user-assigned:** System-assigned is simpler and sufficient for single-app deployments. Use user-assigned only if the same identity needs to be shared across multiple resources.

### 11.2 GitHub Actions service principal

Required for CI/CD to deploy to App Service and read Key Vault secrets during migrations.

```bash
# Create service principal for GitHub Actions
az ad sp create-for-rbac \
  --name "sp-dbm-github-staging" \
  --role "Contributor" \
  --scopes "/subscriptions/.../resourceGroups/rg-dbm-staging" \
  --json-auth

# Also assign Key Vault Secrets User for migration steps
az role assignment create \
  --assignee <sp-object-id> \
  --role "Key Vault Secrets User" \
  --scope "/subscriptions/.../providers/Microsoft.KeyVault/vaults/kv-dbm-staging"
```

The `--json-auth` output is stored as a GitHub repository secret `AZURE_CREDENTIALS_STAGING` / `AZURE_CREDENTIALS_PROD`.

---

## 12. Infrastructure as Code (Bicep)

### 12.1 Repository layout

```
infra/
  bicep/
    main.bicep                     # Entry point — accepts environment param, calls all modules
    parameters/
      staging.bicepparam           # Staging-specific values
      parity.bicepparam            # Parity-specific values (subset of staging)
      production.bicepparam        # Production-specific values
    modules/
      vnet.bicep                   # VNet + all subnets
      nsg.bicep                    # Network security groups
      bastion.bicep                # Azure Bastion + public IP
      am-vm.bicep                  # AccountMate IaaS VM (OS disk + data disk)
      app-service-plan.bicep       # App Service Plan
      app-service-core.bicep       # DBM Core App Service + deployment slots
      app-service-admin.bicep      # Admin Console App Service
      sql-server.bicep             # Azure SQL logical server
      sql-database.bicep           # DBM State database
      service-bus.bicep            # Service Bus namespace + queues
      key-vault.bicep              # Key Vault + RBAC assignments
      log-analytics.bicep          # Log Analytics Workspace
      app-insights.bicep           # Application Insights
      storage.bicep                # Blob Storage account + containers
      recovery-vault.bicep         # Recovery Services Vault + backup policy
      vpn-gateway.bicep            # Azure VPN Gateway + Local Network Gateway + Connection (production only)
  scripts/
    am/
      configure-sql-server.ps1     # SQL Server post-provisioning config
      seed-staging-am.ps1          # Restore prod backup → staging-am
      reset-parity-am.ps1          # Restore golden snapshot → parity-am
      take-golden-snapshot.ps1     # Capture parity-am as new golden snapshot
      take-production-backup.ps1   # Take prod backup + upload to Blob
      verify-am-connection.ps1     # Validate dbm_svc can reach all required SPs
    env/
      provision.sh                 # az deployment group create wrapper
      teardown-parity.sh           # Destroy parity RG (cost saving)
```

### 12.2 Main Bicep entry point (key structure)

```bicep
// infra/bicep/main.bicep

@description('Target environment')
@allowed(['staging', 'parity', 'production'])
param environment string

@description('Azure region')
param location string = 'southafricanorth'

@description('Resource group name')
param resourceGroupName string

// Module: Networking
module networking 'modules/vnet.bicep' = {
  name: 'networking-${environment}'
  params: {
    vnetName: 'vnet-dbm-${environment}'
    location: location
    addressPrefix: environment == 'staging'    ? '10.1.0.0/16'
                 : environment == 'parity'     ? '10.2.0.0/16'
                 : /* production */              '10.3.0.0/16'
    environment: environment
  }
}

// Module: AM IaaS VM
module amVm 'modules/am-vm.bicep' = {
  name: 'am-vm-${environment}'
  params: {
    vmName: 'vm-am-${environment}'
    subnetResourceId: networking.outputs.amSubnetId
    vmSize: environment == 'production' ? 'Standard_D8s_v5' : 'Standard_D4s_v5'
    sqlImageSku: environment == 'production' ? 'sql2022-ws2022-standard' : 'sql2022-ws2022'
    dataDiskSizeGb: environment == 'production' ? 1024 : 512
    environment: environment
    adminPassword: kvRef.getSecret('am-vm-admin-password')
  }
  dependsOn: [networking]
}

// Module: VPN Gateway (production only — S2S IPsec to Annique FortiGate)
module vpnGateway 'modules/vpn-gateway.bicep' = if (environment == 'production') {
  name: 'vpn-gateway-${environment}'
  params: {
    vnetName: networking.outputs.vnetName
    gatewaySubnetId: networking.outputs.gatewaySubnetId
    localNetworkGatewayAddress: '<annique-office-public-ip>'  // from Marcel Truter
    localNetworkAddressPrefixes: ['172.19.0.0/16']           // Annique LAN range
    sharedKey: kvRef.getSecret('vpn-shared-key')
    environment: environment
  }
  dependsOn: [networking]
}

// ... further modules follow same pattern
```

### 12.3 Provision command

```bash
# Provision staging environment
az deployment group create \
  --resource-group rg-dbm-staging \
  --template-file infra/bicep/main.bicep \
  --parameters infra/bicep/parameters/staging.bicepparam \
  --name "dbm-staging-$(date +%Y%m%d-%H%M)"

# Provision production environment
az deployment group create \
  --resource-group rg-dbm-prod \
  --template-file infra/bicep/main.bicep \
  --parameters infra/bicep/parameters/production.bicepparam \
  --name "dbm-prod-$(date +%Y%m%d-%H%M)"
```

---

## 13. Deployment Runbook — Staging

Step-by-step procedure to provision a clean staging environment from scratch. Follow in order.

### Step 1 — Azure prerequisites

```bash
# Login and set subscription
az login
az account set --subscription "<subscription-id>"

# Create resource group
az group create --name rg-dbm-staging --location southafricanorth
```

### Step 2 — Deploy infrastructure via Bicep

```bash
# Review what will be created (what-if)
az deployment group create \
  --resource-group rg-dbm-staging \
  --template-file infra/bicep/main.bicep \
  --parameters infra/bicep/parameters/staging.bicepparam \
  --what-if

# Deploy
az deployment group create \
  --resource-group rg-dbm-staging \
  --template-file infra/bicep/main.bicep \
  --parameters infra/bicep/parameters/staging.bicepparam
```

**Expected duration:** ~15–20 minutes (VM provisioning is the slowest step).

### Step 3 — Load secrets into Key Vault

```bash
# Load each secret from your local environment or password manager
az keyvault secret set --vault-name kv-dbm-staging --name am-sql-password \
  --value "<generate a strong password>"

az keyvault secret set --vault-name kv-dbm-staging --name shopify-admin-api-token \
  --value "<from Shopify Partner Dashboard after app install>"

az keyvault secret set --vault-name kv-dbm-staging --name shopify-webhook-signing-secret \
  --value "<from Shopify Partner Dashboard>"

az keyvault secret set --vault-name kv-dbm-staging --name brevo-api-key \
  --value "<from Brevo dashboard>"

az keyvault secret set --vault-name kv-dbm-staging --name admin-console-session-secret \
  --value "$(openssl rand -hex 32)"
```

### Step 4 — Configure SQL Server on AM VM

```bash
# Connect to VM via Bastion (browser) or JIT RDP
# On the VM, run:
powershell -ExecutionPolicy Bypass -File \\path\to\configure-sql-server.ps1
```

### Step 5 — Seed AM databases

```bash
# Upload production backup to Blob Storage (run on production AM or locally)
powershell -ExecutionPolicy Bypass -File scripts/am/take-production-backup.ps1

# Restore to staging-am (run on staging VM or via remote PowerShell)
powershell -ExecutionPolicy Bypass -File scripts/am/seed-staging-am.ps1 \
  -BackupBlobUrl "<SAS URL from Blob Storage>" \
  -SqlInstance "10.1.2.4" \
  -SaPassword "<temporary sa password>" \
  -DbmSvcPassword "<am-sql-password from Key Vault>"
```

### Step 6 — Verify AM connectivity

```powershell
# scripts/am/verify-am-connection.ps1
# Run from the App Service environment (or locally via VPN if routing is in place)
# Confirms all required SPs are callable from dbm_svc

$cs = "Server=10.1.2.4,1433;Database=amanniquelive;User Id=dbm_svc;Password=<secret>"
$sps = @("sp_camp_getSPprice", "sp_ws_syncorder", "sp_ws_reactivate", "sp_ws_gensoxitems")

foreach ($sp in $sps) {
    $result = Invoke-Sqlcmd -ConnectionString $cs -Query "
        SELECT OBJECT_ID('dbo.$sp') AS sp_id, HAS_PERMS_BY_NAME('dbo.$sp', 'OBJECT', 'EXECUTE') AS can_exec"
    Write-Host "$sp : ID=$($result.sp_id) CanExec=$($result.can_exec)"
}
```

### Step 7 — Run DBM schema migration

```bash
# After deploying DBM Core App Service
dotnet ef database update \
  --project src/DBM.Core \
  --connection "$(az keyvault secret show --vault-name kv-dbm-staging --name dbm-state-db-connstr --query value -o tsv)"
```

### Step 8 — Configure GitHub Actions secrets

```bash
# Create GitHub secrets for staging deployment
gh secret set AZURE_CREDENTIALS_STAGING \
  --body "$(az ad sp create-for-rbac --name sp-dbm-github-staging \
    --role Contributor --scopes /subscriptions/.../resourceGroups/rg-dbm-staging \
    --json-auth)"

gh secret set AZURE_APP_NAME_STAGING      --body "app-dbm-core-staging"
gh secret set AZURE_APP_NAME_ADMIN_STAGING --body "app-dbm-admin-staging"
gh secret set AZURE_RG_STAGING             --body "rg-dbm-staging"
```

### Step 9 — Install Shopify app on staging store

```bash
# In the shopify-app/ directory
shopify app deploy --store annique-staging.myshopify.com

# Register webhooks (after app install)
# DBM Core registers its own webhooks on startup if SHOPIFY_AUTO_REGISTER_WEBHOOKS=true
```

### Step 10 — Validate end-to-end

1. Open `https://app-dbm-core-staging.azurewebsites.net/health` — expect `200 OK`
2. Open `https://app-dbm-core-staging.azurewebsites.net/health/ready` — expect `{ "status": "Healthy", "am_sql": "Healthy", "service_bus": "Healthy" }`
3. Submit a test Shopify order on `annique-staging.myshopify.com`
4. Confirm the webhook is received in Application Insights (trace query: `where operation_Name == "POST /webhooks/orders/create"`)
5. Confirm the order appears in `sqldb-dbm-state-staging` `idempotency_keys` table
6. Confirm the order appears in staging AM (`SELECT TOP 1 * FROM sosord ORDER BY dOrderDate DESC`)

---

## 14. Cost Model

Estimates in **South Africa North** pricing (USD/month). Actual costs depend on usage, reserved instances, and Azure Hybrid Benefit eligibility.

### Staging environment (always-on)

| Resource | SKU | Est. monthly cost |
|---|---|---|
| `vm-am-staging` — Standard_D4s_v5 + SQL Server 2022 Dev (free) | Windows Server 2022 | ~$220 |
| `asp-dbm-staging` — P2v3 Linux | 2 vCPU, 8 GB | ~$130 |
| App Services (2 × on same plan) | Included in plan | $0 |
| `sql-dbm-staging` — GP_S_Gen5_2 serverless | Autoscale, auto-pause | ~$30–60 |
| `sb-dbm-staging` — Standard | 10M ops/month base | ~$10 |
| `kv-dbm-staging` — Standard | Operations | ~$2 |
| `appi-dbm-staging` + `log-dbm-staging` | ~3 GB/month | ~$7 |
| `stdbmbackupstg` — LRS | 50 GB/month | ~$1 |
| `rsv-dbm-staging` — Backup | 1 VM + 1 TB | ~$15 |
| `bas-dbm-staging` — Basic (or JIT) | Bastion OR JIT (~$0) | ~$140 or ~$0 |
| **Subtotal (with Bastion)** | | **~$560/month** |
| **Subtotal (with JIT instead)** | | **~$420/month** |

### Production environment

| Resource | SKU | Est. monthly cost |
|---|---|---|
| `vm-am-prod` — Standard_D8s_v5 + SQL Server 2022 Standard | Windows Server 2022 | ~$650 |
| `asp-dbm-prod` — P3v3 Linux | 4 vCPU, 16 GB | ~$250 |
| `sql-dbm-prod` — GP_Gen5_4 | 4 vCores provisioned | ~$220 |
| `sb-dbm-prod` — Premium, 1 MU | 1 messaging unit | ~$670 |
| `kv-dbm-prod` — Standard | Operations | ~$2 |
| `appi-dbm-prod` + `log-dbm-prod` | ~10 GB/month | ~$23 |
| `stdbmbackupprd` — LRS | 100 GB/month | ~$2 |
| `rsv-dbm-prod` — Backup | 1 VM + long-term | ~$30 |
| `bas-dbm-prod` — Basic | | ~$140 |
| `vgw-dbm-prod` — VPN Gateway VpnGw1 | S2S IPsec to Annique FortiGate | ~$140 |
| **Subtotal** | | **~$2,127/month** |

> **Cost reduction levers:**
> - Azure Hybrid Benefit (Windows + SQL Server): up to 40–70% off VM cost
> - Reserved Instances (1-year): ~40% off compute
> - Service Bus Premium: consider whether Standard + IP filter is acceptable for production (saves ~$660/month)
> - Auto-shutdown parity VM when not in use: ~$200/month saving
> - Use JIT instead of Bastion for staging: ~$140/month saving

**With Azure Hybrid Benefit on VMs + 1-year Reserved Instances for production App Service Plan:**
Estimated production cost: ~$1,200–1,400/month

---

## 15. Security Checklist

Before go-live, verify all items below.

### Azure resource security
- [ ] No public IPs on AM VMs (staging and production)
- [ ] NSG on AM subnet: only SQL from app subnet, only RDP/WinRM from Bastion subnet
- [ ] Purge protection enabled on `kv-dbm-prod`
- [ ] Soft delete enabled on all Key Vaults
- [ ] Azure SQL: no public network access — private endpoint only (production)
- [ ] Service Bus: Premium tier in production with VNet integration
- [ ] Storage account: public blob access disabled
- [ ] All App Services: HTTPS-only, TLS 1.2 minimum
- [ ] Managed Identity used for all service-to-service auth (no API keys in environment variables)

### SQL Server security
- [ ] `sa` login disabled after seed procedure
- [ ] `dbm_svc` is the only SQL login used by DBM
- [ ] `dbm_svc` has no more than the specified grants (no `db_owner`, no `sysadmin`)
- [ ] SQL Server is not accessible from the internet (private subnet only)
- [ ] Windows Firewall on AM VM: blocks all inbound except via NSG (defence in depth)
- [ ] SQL Server audit log enabled

### Application security
- [ ] No credentials in source code or git history (`git log --all -S "password"` returns nothing relevant)
- [ ] All Key Vault references working in App Service settings (no plaintext secrets)
- [ ] Shopify webhook HMAC verification in place and tested (sends invalid HMAC → 401)
- [ ] DBM Data API JWT authentication working (admin console presents token, invalid token → 401)
- [ ] Health endpoint `/health/detail` requires JWT (not public)

### Operational
- [ ] Azure Backup policy applied to production AM VM
- [ ] Alert rules configured and tested (trigger a test dead-letter → confirm email received)
- [ ] Application Insights availability test passing for `/health`
- [ ] Log retention set correctly (staging 90 days, production 365 days)

---

## Evidence Base

| Artefact | Location |
|---|---|
| Architecture specification | `docs/spec/01_dbm_architecture.md` |
| PRD — platform NFRs | `docs/spec/00_prd.md §5` |
| Platform runtime cross-cut | `docs/analysis/16_platform_runtime_and_hosting_crosscut.md` |
| DBM–AM interface contract | `docs/analysis/24_dbm_am_interface_contract.md` |
| AM migration advisory | `docs/onboarding/04_session_state_april_2026.md §7` |
| Shopify Plus requirements | `docs/analysis/23_shopify_plus_requirements_analysis.md` |
