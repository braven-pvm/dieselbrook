# Environment Architecture & Setup Procedures
## Dieselbrook Middleware (DBM) — Annique Cosmetics Platform Modernisation

**Document type:** Environment Architecture Specification  
**Programme:** Annique → Shopify Plus migration and Dieselbrook Middleware build  
**Version:** 0.1 — initial  
**Date:** 2026-04-07  
**Status:** Draft  
**Linear:** ANN-22  
**Parent spec:** `docs/spec/03_infrastructure.md` (Azure resource specs)  
**Notion:** https://www.notion.so/dieselbrook/annique-programme

---

## Decision Dependencies

| Decision ID | Topic | Status | Impact If Still Open |
|---|---|---|---|
| D-06 | Shopify plan tier | ✅ Closed 2026-03-15 | Was: Shopify Plus required for dev store parity and Functions testing |
| TC-02 | Go-live date and cutover model | ✅ Closed 2026-03-15 | Was: affected environment promotion timeline |
| All other decisions | — | ✅ All closed 2026-03-15 | — |

This specification is fully writeable. No open decisions block it.

---

## 1. Scope

This document covers:

- The four-tier environment model (local, staging, parity, production)
- Local development environment: Docker Compose stack, AM test doubles, local secrets
- Staging environment: AM seed workflow with full scripts, reset procedures, Shopify store setup
- Parity environment: golden snapshot management, pre-run reset, parity run lifecycle, cost control
- Production environment: promotion gates, AM connectivity, cutover prerequisites
- Environment promotion model: branch-to-environment mapping, deployment pipeline, blue/green
- Shopify store configuration per tier
- Parallel run architecture for the pre-cutover window
- POC/MVP phase gate definitions

Azure resource provisioning (VNet, VM specs, App Service, Service Bus, Key Vault, SQL) is in `docs/spec/03_infrastructure.md`. This document assumes those resources exist and focuses on the *workflow* and *setup procedures* for using them.

---

## 2. Environment Tier Overview

Four tiers, each with a distinct purpose, access model, and lifecycle:

| Tier | Name | Purpose | AM instance | Shopify store | Lifecycle |
|---|---|---|---|---|---|
| 1 | `local` | Developer workstation. Unit tests, fast inner loop. | None (test doubles) | None (APIs mocked or tunnelled) | Always running |
| 2 | `staging` | Full integration. All services, sprint reviews, feature testing, parity candidate runs. | Azure IaaS VM — seeded from production backup | Shopify Plus dev store (`annique-staging`) | Always running |
| 3 | `parity` | Dedicated parity harness execution. Isolated. AM reset from golden snapshot before every run. | Azure IaaS VM — reset from golden snapshot | Shopify Partner dev store (`annique-parity`) — reset between runs | Provisioned on demand; VM auto-shuts down between runs |
| 4 | `production` | Live environment post go-live. | Azure IaaS VM (post-migration) or on-prem AMSERVER-v9 (pre-migration via private routing) | Live Shopify Plus store | Always running |

### Why four tiers?

- **local vs staging:** Local has no AM connectivity. It enables fast development without Azure cost or AM availability dependency. Integration correctness is confirmed in staging.
- **staging vs parity:** Staging AM is mutated by sprint work (feature testing, exploratory testing, manual data corrections). A shared staging AM cannot serve as a reliable parity baseline. Parity requires a controlled, locked, snapshot-restored baseline. Separating the environments enforces this invariant.
- **parity vs production:** Production is customer-facing. Parity runs must never touch production AM or the live Shopify store.

---

## 3. Local Development Environment

### 3.1 Purpose

The local environment provides a complete DBM Core runtime on a developer workstation with no Azure dependency and no AM connection. It supports:

- Development and unit-testing of all services
- Manual smoke-testing of API endpoints and webhook handling
- Admin console development
- Integration test execution (when pointed at `staging-am` with the correct connection string override)

### 3.2 Prerequisites

| Requirement | Version | Notes |
|---|---|---|
| Docker Desktop | 4.x+ | WSL 2 backend on Windows |
| .NET SDK | 9.0+ | `dotnet --version` |
| Node.js | 20 LTS | For Admin Console |
| Shopify CLI | 3.x | For local Function development |
| Azure CLI | 2.60+ | For Key Vault access when needed |
| SQLCMD | 16+ | For local SQL operations |

### 3.3 Docker Compose stack

The full local stack is defined in `infra/docker/docker-compose.yml`. Start with `docker compose up` from the repository root.

```yaml
# infra/docker/docker-compose.yml
version: "3.9"

services:

  # ─── SQL Server (DBM State DB) ────────────────────────────────────────────
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: dbm-sqlserver
    environment:
      SA_PASSWORD: "DevLocal_Str0ng!"
      ACCEPT_EULA: "Y"
      MSSQL_COLLATION: "Latin1_General_CI_AS"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql/data
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost",
             "-U", "sa", "-P", "DevLocal_Str0ng!", "-Q", "SELECT 1"]
      interval: 10s
      retries: 10
      start_period: 30s

  # ─── Azure Service Bus emulator ──────────────────────────────────────────
  servicebus-emulator:
    image: mcr.microsoft.com/azure-messaging/servicebus-emulator:latest
    container_name: dbm-servicebus
    ports:
      - "5672:5672"    # AMQP
      - "8080:8080"    # Management
    environment:
      SQL_SERVER: sqlserver
    depends_on:
      sqlserver:
        condition: service_healthy

  # ─── DBM Core ─────────────────────────────────────────────────────────────
  dbm-core:
    build:
      context: ./src/DBM.Core
      dockerfile: Dockerfile
    container_name: dbm-core
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:8080
      ConnectionStrings__DbmState: >-
        Server=sqlserver,1433;
        Database=dbm_state_dev;
        User Id=sa;
        Password=DevLocal_Str0ng!;
        TrustServerCertificate=True;
      ServiceBus__ConnectionString: "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=placeholder;"
      ServiceBus__Queues__OrderIngest: "order-ingest"
      ServiceBus__Queues__FulfillmentSync: "fulfillment-sync"
      ServiceBus__Queues__ConsultantSync: "consultant-sync"
      ServiceBus__Queues__PricingSync: "pricing-sync"
      ServiceBus__Queues__CommsDispatch: "comms-dispatch"
      # AM disabled in local — test doubles inject AM responses
      AM__Enabled: "false"
      AM__TestDoubles__FixturePath: "/app/test-fixtures/am"
      # Shopify — point at annique-staging with tunnel, or use fixtures
      Shopify__StoreDomain: "annique-staging.myshopify.com"
      Shopify__UseFixtures: "true"
      Shopify__FixturePath: "/app/test-fixtures/shopify"
    volumes:
      - ./tests/DBM.Core.Tests/fixtures:/app/test-fixtures:ro
    depends_on:
      sqlserver:
        condition: service_healthy
      servicebus-emulator:
        condition: service_started
    ports:
      - "5000:8080"

  # ─── Admin Console ─────────────────────────────────────────────────────────
  admin-console:
    build:
      context: ./src/DBM.AdminConsole
      dockerfile: Dockerfile
    container_name: dbm-admin
    environment:
      NODE_ENV: development
      DBM_API_URL: "http://dbm-core:8080"
      NEXTAUTH_URL: "http://localhost:3000"
      NEXTAUTH_SECRET: "dev-only-secret-not-used-in-prod"
    depends_on:
      - dbm-core
    ports:
      - "3000:3000"

volumes:
  sqlserver-data:
```

### 3.4 Integration test stack

A separate Docker Compose override adds the real SQL Server and wires connection strings for integration tests that run locally (against staging-am, not local SQL):

```yaml
# infra/docker/docker-compose.test.yml
# Usage: docker compose -f docker-compose.yml -f docker-compose.test.yml up
services:
  dbm-core:
    environment:
      # Override: point at staging-am for integration tests
      AM__Enabled: "true"
      AM__ConnectionString: "${STAGING_AM_CONNECTION_STRING}"
      # Staging-am connection string must be set in local .env
```

Developers can run integration tests locally against `staging-am` by placing the connection string in a `.env.local` file (gitignored). Integration tests are also run automatically in CI on every merge to `main`.

### 3.5 AM test doubles

In local mode (`AM__Enabled: false`), the `IAccountMateClient` interface is satisfied by `LocalAmTestDouble`, which reads pre-recorded fixture files from `tests/fixtures/am/`.

**Test double fixture format:**

```
tests/
  fixtures/
    am/
      sp_camp_getSPprice/
        consultant_123_sku_ABC.json   # Fixture response for a specific input
      sp_ws_syncorder/
        valid_order_001.json          # Expected AM response for order sync
      icitem/
        active_skus_snapshot.json     # Snapshot of product table for local dev
```

The `LocalAmTestDouble` matches incoming requests to fixture files by hashing the request parameters. If no fixture matches, it returns a configurable default (success) response and logs a warning. This means:

- Common development paths work without fixtures
- Tests requiring specific AM responses define their fixtures explicitly
- The fixture set grows naturally as developers encounter new scenarios

### 3.6 Local secrets

No secrets are committed to source control. Local development uses a `.env.local` file (gitignored) for any non-default overrides. For most local development, the Docker Compose default values are sufficient.

```bash
# .env.local — gitignored, copy from .env.local.template
STAGING_AM_CONNECTION_STRING=Server=<staging-am-private-ip>,1433;Database=amanniquelive;User Id=dbm_svc;Password=<from-key-vault>;TrustServerCertificate=True;
SHOPIFY_STAGING_ACCESS_TOKEN=<from-staging-app-install>
```

The template `.env.local.template` is committed and documents the required variables without values.

### 3.7 Running locally

```bash
# Start full local stack
docker compose -f infra/docker/docker-compose.yml up

# Run unit tests (no external deps required)
dotnet test tests/DBM.Core.Tests --filter Category=Unit

# Run integration tests (requires STAGING_AM_CONNECTION_STRING in .env.local)
dotnet test tests/DBM.Integration.Tests --filter Category=Integration

# Start Admin Console only (if DBM Core already running)
docker compose up admin-console

# Run EF Core migrations against local SQL
dotnet ef database update --project src/DBM.Core --connection "Server=localhost,1433;..."
```

---

## 4. Staging Environment

### 4.1 Purpose and access model

Staging is the primary integration environment. It is always running. All sprint work is tested here after PR merge.

| Who has access | What access |
|---|---|
| DBM Core App Service (system identity) | Key Vault, Service Bus, Azure SQL — via Managed Identity |
| Developers | Azure Portal read, Key Vault Secrets Officer, Kudu console (read), Application Insights |
| GitHub Actions (service principal) | App Service deployment, Key Vault Secrets User |
| No direct public access | DBM Core endpoints are accessible from the App Service URL; AM VM has no public IP |

### 4.2 Resource summary

Full resource specs in `docs/spec/03_infrastructure.md`. Summary for orientation:

| Resource | Name | Notes |
|---|---|---|
| Resource group | `rg-dbm-staging` | `southafricanorth` |
| VNet | `vnet-dbm-staging` | `10.1.0.0/16` |
| App Service Plan | `asp-dbm-staging` | P2v3 Linux |
| DBM Core App Service | `app-dbm-core-staging` | .NET 9, VNet Integration |
| Admin Console App Service | `app-dbm-admin-staging` | Node.js 20 LTS |
| Azure SQL (DBM State) | `sql-dbm-staging` | Serverless GP_S_Gen5_2 |
| Service Bus | `sbns-dbm-staging` | Standard tier |
| Key Vault | `kv-dbm-staging` | RBAC model |
| Application Insights | `ai-dbm-staging` | 90-day retention |
| AM IaaS VM | `vm-am-staging` | Standard_D4s_v5, private subnet |
| Blob Storage | `stdbmstaging` | am-backups, parity-artifacts containers |

### 4.3 AM seed procedure

The staging AM VM is seeded from a production backup. This is performed:

- On initial environment creation
- When a fresh production backup is needed (quarterly or after major data events)
- After accidental data corruption in staging

**Prerequisites:**

1. A `.bak` file of the production AM databases has been taken by Annique IT, with the following databases included:
   - `amanniquelive` — main transactional database
   - `compplanLive` — compensation plan database
   - `compsys` — communications/system database (optional; DBM only needs read access)
2. The backup has been uploaded to Azure Blob Storage:
   - Container: `am-backups` in `stdbmstaging`
   - Naming convention: `amanniquelive_YYYYMMDD.bak`, `compplanLive_YYYYMMDD.bak`, `compsys_YYYYMMDD.bak`

**Seed script:**

```powershell
# scripts/am/seed-staging-am.ps1
# Restores production backup to the staging-am SQL instance and provisions dbm_svc.
# Run from the staging-am VM (via Azure Bastion or JIT RDP + PowerShell remoting).
#
# Parameters:
#   -BackupDate      Date string matching backup file names (e.g. "20260407")
#   -StorageAccount  Azure Storage account name (e.g. "stdbmstaging")
#   -Container       Blob container name (default: "am-backups")
#   -DbmSvcPassword  Password for the dbm_svc SQL login (retrieved from Key Vault)

param(
    [Parameter(Mandatory)] [string] $BackupDate,
    [Parameter(Mandatory)] [string] $StorageAccount,
    [string] $Container = "am-backups",
    [Parameter(Mandatory)] [string] $DbmSvcPassword
)

$ErrorActionPreference = "Stop"
$SqlInstance = "localhost"
$BackupDir   = "D:\SQLBackups"

# ── 1. Download backups from Azure Blob ─────────────────────────────────────
Write-Host "Downloading backups from Azure Blob Storage..."
$dbs = @("amanniquelive", "compplanLive", "compsys")
foreach ($db in $dbs) {
    $blob = "${db}_${BackupDate}.bak"
    az storage blob download `
        --account-name $StorageAccount `
        --container-name $Container `
        --name $blob `
        --file "$BackupDir\$blob" `
        --auth-mode login
}

# ── 2. Restore databases ─────────────────────────────────────────────────────
Write-Host "Restoring databases..."
$sqlcmd = "sqlcmd -S $SqlInstance -E"

foreach ($db in $dbs) {
    $bak = "$BackupDir\${db}_${BackupDate}.bak"

    # Get logical file names from the backup
    $fileList = Invoke-Expression "$sqlcmd -Q `"RESTORE FILELISTONLY FROM DISK='$bak'`"" `
        | ConvertFrom-Csv -Delimiter "`t" -Header LogicalName,PhysicalName,Type

    $dataFile = ($fileList | Where-Object Type -eq 'D')[0].LogicalName
    $logFile  = ($fileList | Where-Object Type -eq 'L')[0].LogicalName

    Invoke-Expression "$sqlcmd -Q `"
        RESTORE DATABASE [$db]
        FROM DISK = '$bak'
        WITH REPLACE,
             MOVE '$dataFile' TO 'D:\SQLData\${db}.mdf',
             MOVE '$logFile'  TO 'D:\SQLData\${db}_log.ldf',
             STATS = 10;
    `""
    Write-Host "Restored $db"
}

# ── 3. Set compatibility level ───────────────────────────────────────────────
Write-Host "Setting compatibility level..."
foreach ($db in $dbs) {
    Invoke-Expression "$sqlcmd -Q `"ALTER DATABASE [$db] SET COMPATIBILITY_LEVEL = 150;`""
}

# ── 4. Provision dbm_svc login and grants ────────────────────────────────────
Write-Host "Provisioning dbm_svc..."
Invoke-Expression "$sqlcmd -Q `"
    IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'dbm_svc')
        CREATE LOGIN dbm_svc WITH PASSWORD = '$DbmSvcPassword', CHECK_POLICY = OFF;
    ELSE
        ALTER LOGIN dbm_svc WITH PASSWORD = '$DbmSvcPassword';
`""

# amanniquelive — read and controlled write via SPs
Invoke-Expression "$sqlcmd -d amanniquelive -Q `"
    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'dbm_svc')
        CREATE USER dbm_svc FOR LOGIN dbm_svc;

    -- Product / inventory reads
    GRANT SELECT ON dbo.icitem             TO dbm_svc;
    GRANT SELECT ON dbo.iciimgUpdateNOP    TO dbm_svc;

    -- Customer / consultant reads
    GRANT SELECT ON dbo.arcust            TO dbm_svc;

    -- Order management reads
    GRANT SELECT ON dbo.SOPortal          TO dbm_svc;
    GRANT SELECT ON dbo.soxitems          TO dbm_svc;
    GRANT SELECT ON dbo.arinvc            TO dbm_svc;
    GRANT SELECT ON dbo.soship            TO dbm_svc;
    GRANT SELECT ON dbo.sostrs            TO dbm_svc;
    GRANT SELECT ON dbo.sosord            TO dbm_svc;
    GRANT SELECT ON dbo.soxitem           TO dbm_svc;
    GRANT SELECT ON dbo.soskit            TO dbm_svc;

    -- Campaign reads
    GRANT SELECT ON dbo.Campaign          TO dbm_svc;
    GRANT SELECT ON dbo.CampDetail        TO dbm_svc;
    GRANT SELECT ON dbo.CampSku           TO dbm_svc;

    -- Controlled write-back via stored procedures only
    GRANT EXECUTE ON dbo.sp_camp_getSPprice   TO dbm_svc;
    GRANT EXECUTE ON dbo.sp_ws_syncorder      TO dbm_svc;
    GRANT EXECUTE ON dbo.sp_ws_reactivate     TO dbm_svc;
    GRANT EXECUTE ON dbo.sp_ws_gensoxitems    TO dbm_svc;
    -- TC-05: cancellation SP — add when confirmed by Annique IT
`""

# compplanLive — read-only
Invoke-Expression "$sqlcmd -d compplanLive -Q `"
    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'dbm_svc')
        CREATE USER dbm_svc FOR LOGIN dbm_svc;

    GRANT SELECT ON dbo.CTstatement        TO dbm_svc;
    GRANT SELECT ON dbo.CTdownlineh        TO dbm_svc;
    GRANT SELECT ON dbo.CTstatementh       TO dbm_svc;
`""

# compsys — read-only boundary only
Invoke-Expression "$sqlcmd -d compsys -Q `"
    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'dbm_svc')
        CREATE USER dbm_svc FOR LOGIN dbm_svc;

    GRANT SELECT ON dbo.MAILMESSAGE        TO dbm_svc;
`""

# ── 5. Disable sa ────────────────────────────────────────────────────────────
Write-Host "Disabling sa login..."
Invoke-Expression "$sqlcmd -Q `"ALTER LOGIN sa DISABLE;`""

# ── 6. Verify ────────────────────────────────────────────────────────────────
Write-Host "Seed complete. Run verify-am-connection.ps1 to confirm connectivity."
```

### 4.4 AM connectivity verification

After seeding, run `verify-am-connection.ps1` from DBM Core's CI or from the staging VM to confirm connectivity and grant scope:

```powershell
# scripts/am/verify-am-connection.ps1
# Verifies that the DBM service account can connect and call all required SPs.
# Run from a machine with network access to the AM VM private IP.

param(
    [Parameter(Mandatory)] [string] $AmHost,
    [string] $Port = "1433",
    [Parameter(Mandatory)] [string] $DbmSvcPassword
)

$conn = "Server=$AmHost,$Port;User Id=dbm_svc;Password=$DbmSvcPassword;TrustServerCertificate=True;"

$checks = @(
    @{ Db = "amanniquelive"; Query = "SELECT TOP 1 cItemNo FROM dbo.icitem";        Label = "icitem read" },
    @{ Db = "amanniquelive"; Query = "SELECT TOP 1 cCustNo FROM dbo.arcust";        Label = "arcust read" },
    @{ Db = "amanniquelive"; Query = "EXEC dbo.sp_camp_getSPprice @cCustNo='TEST', @cItemNo='TEST'"; Label = "sp_camp_getSPprice" },
    @{ Db = "amanniquelive"; Query = "EXEC dbo.sp_ws_gensoxitems @cOrderNo='TEST'"; Label = "sp_ws_gensoxitems" },
    @{ Db = "compplanLive";  Query = "SELECT TOP 1 1 FROM dbo.CTstatement";         Label = "CTstatement read" },
    @{ Db = "compsys";       Query = "SELECT TOP 1 1 FROM dbo.MAILMESSAGE";         Label = "MAILMESSAGE read" }
)

$allPassed = $true
foreach ($check in $checks) {
    try {
        $sql = New-Object System.Data.SqlClient.SqlConnection($conn + "Initial Catalog=$($check.Db);")
        $sql.Open()
        $cmd = $sql.CreateCommand()
        $cmd.CommandText = $check.Query
        $cmd.ExecuteNonQuery() | Out-Null
        $sql.Close()
        Write-Host "[PASS] $($check.Label)"
    } catch {
        Write-Host "[FAIL] $($check.Label): $_"
        $allPassed = $false
    }
}

if ($allPassed) {
    Write-Host "`nAll checks passed. AM connectivity verified."
    exit 0
} else {
    Write-Host "`nSome checks failed. Review permissions."
    exit 1
}
```

### 4.5 Staging reset

Staging AM is not regularly reset — it accumulates sprint test data. A reset is only needed after data corruption or when a fresh production-derived baseline is required.

To reset staging AM:

1. Take a new production backup (coordinated with Annique IT).
2. Upload to Azure Blob under the new date.
3. Run `seed-staging-am.ps1` with the new backup date.
4. Run `verify-am-connection.ps1`.
5. Re-run EF Core migrations: `dotnet ef database update --project src/DBM.Core`.
6. Trigger a full resync of staging Shopify store via Admin Console → "Force full sync".

### 4.6 Staging Shopify store configuration

| Setting | Value |
|---|---|
| Store type | Shopify Partner development store |
| Plan | Shopify Plus development store (free for Partner accounts) |
| Store domain | `annique-staging.myshopify.com` (or agreed subdomain) |
| Custom app install | DBM custom app — staging version |
| Webhooks | Pointed at `https://app-dbm-core-staging.azurewebsites.net/webhooks/shopify` |
| Shopify Functions | Deployed via `shopify app deploy --config staging` |
| Product catalogue | Seeded from live store product export (initial setup); DBM sync keeps it current |

**Webhook topics registered (staging):**

```
orders/create
orders/updated
orders/fulfilled
orders/cancelled
customers/create
customers/update
app/uninstalled
```

---

## 5. Parity Environment

### 5.1 Purpose and access model

The parity environment exists exclusively for executing the parity harness. It is:

- **Isolated:** No sprint activity or manual testing occurs in this environment.
- **Reset before every run:** Both the AM database and DBM State DB are restored from golden snapshots before each parity run.
- **Cost-managed:** The AM VM auto-shuts down between runs. The parity App Service Plan scales to zero when not in use.

| Who has access | What access |
|---|---|
| GitHub Actions (parity workflow) | Full deploy, reset scripts, test execution |
| Architects / senior developers | Azure Portal read, parity report blob access |
| No manual login to AM VM | Parity AM is not for exploratory testing — use staging for that |

### 5.2 Parity run lifecycle

A complete parity run follows this sequence:

```
1. TRIGGER
   ├── Manual trigger via GitHub Actions UI
   └── Weekly schedule (Sunday 02:00 SAST)

2. RESET ENVIRONMENT
   ├── Start parity-am VM (if stopped)
   ├── Restore golden snapshot → parity-am (reset-parity-am.ps1)
   ├── Drop and recreate DBM State DB on parity Azure SQL
   └── Flush all parity Service Bus queues

3. DEPLOY PARITY BUILD
   └── Deploy current main branch to parity App Service

4. RUN PARITY HARNESS
   ├── dotnet test DBM.Parity.Tests --filter Category=Parity
   ├── -- Pricing parity: oracle comparison for all active SKU × consultant
   ├── -- Order write-back parity: 12+ representative order fixtures
   ├── -- Product sync parity: full product catalogue comparison
   └── -- Consultant sync parity: sample consultant metafield comparison

5. PUBLISH RESULTS
   ├── Upload parity report JSON → Azure Blob (parity-artifacts container)
   ├── Upload HTML report → Azure Blob (public URL for stakeholder review)
   └── Post summary to GitHub Actions run summary

6. STOP VM
   └── Shut down parity-am VM (cost control — saves ~$200/month)

7. GATE CHECK
   └── Production deployment workflow reads latest parity report
       and requires overall_status = "CLEAN" (< 7 days old)
```

### 5.3 AM reset script (pre-parity-run)

```powershell
# scripts/am/reset-parity-am.ps1
# Restores the parity-am SQL instance to the golden snapshot.
# Run as part of the parity GitHub Actions workflow — not manually.
#
# The golden snapshot is a SQL Server backup taken after the last
# approved golden dataset capture. It lives in Azure Blob Storage
# alongside the parity fixture files.

param(
    [Parameter(Mandatory)] [string] $StorageAccount,
    [string] $Container = "parity-artifacts",
    [Parameter(Mandatory)] [string] $GoldenSnapshotDate,
    [Parameter(Mandatory)] [string] $DbmSvcPassword
)

$ErrorActionPreference = "Stop"
$SqlInstance = "localhost"
$BackupDir   = "D:\SQLBackups\Parity"

Write-Host "Downloading golden snapshot ($GoldenSnapshotDate)..."
$dbs = @("amanniquelive", "compplanLive", "compsys")
foreach ($db in $dbs) {
    $blob = "golden/${GoldenSnapshotDate}/${db}.bak"
    az storage blob download `
        --account-name $StorageAccount `
        --container-name $Container `
        --name $blob `
        --file "$BackupDir\${db}_golden.bak" `
        --auth-mode login
}

Write-Host "Restoring golden snapshot..."
foreach ($db in $dbs) {
    $bak = "$BackupDir\${db}_golden.bak"
    sqlcmd -S $SqlInstance -E -Q "
        RESTORE DATABASE [$db]
        FROM DISK = '$bak'
        WITH REPLACE,
             MOVE '${db}' TO 'D:\SQLData\${db}.mdf',
             MOVE '${db}_log' TO 'D:\SQLData\${db}_log.ldf',
             STATS = 10;
    "
}

Write-Host "Re-provisioning dbm_svc..."
# Apply identical grants as seed-staging-am.ps1 §4 above
# (same grant script — sourced from shared include)
. "$PSScriptRoot\Grant-DbmSvcPermissions.ps1" -DbmSvcPassword $DbmSvcPassword

Write-Host "Parity-am reset complete. Golden snapshot: $GoldenSnapshotDate"
```

### 5.4 Golden snapshot management

Golden snapshots represent the canonical AM state against which DBM output is compared. They are captured deliberately — not automatically from every production backup.

**When to capture a new golden snapshot:**

- Initial programme setup (first capture from production)
- After a confirmed production data event (e.g., campaign structure change, new consultant tier rule)
- When AM's stored procedure logic changes (must revalidate parity expectations)
- Quarterly refresh cycle (every 3 months)

**Capture procedure:**

```powershell
# scripts/am/take-golden-snapshot.ps1
# Captures the current parity-am state as the new golden snapshot.
# ONLY run when the parity golden dataset has been formally approved.
# This overwrites the previous golden snapshot.

param(
    [Parameter(Mandatory)] [string] $StorageAccount,
    [string] $Container = "parity-artifacts",
    [Parameter(Mandatory)] [string] $SnapshotDate   # e.g. "2026-04-07"
)

$ErrorActionPreference = "Stop"
$BackupDir = "D:\SQLBackups\Golden"
$dbs = @("amanniquelive", "compplanLive", "compsys")

foreach ($db in $dbs) {
    $bak = "$BackupDir\${db}_${SnapshotDate}.bak"
    sqlcmd -S localhost -E -Q "
        BACKUP DATABASE [$db]
        TO DISK = '$bak'
        WITH COMPRESSION, STATS = 10;
    "
    az storage blob upload `
        --account-name $StorageAccount `
        --container-name $Container `
        --name "golden/$SnapshotDate/${db}.bak" `
        --file $bak `
        --auth-mode login
    Write-Host "Uploaded golden snapshot: $db @ $SnapshotDate"
}

Write-Host "Golden snapshot captured. Update GOLDEN_SNAPSHOT_DATE in parity workflow."
```

After capturing a new golden snapshot, update `GOLDEN_SNAPSHOT_DATE` in `.github/workflows/parity.yml` and commit to `main`.

**Golden snapshot storage layout:**

```
parity-artifacts/             (Azure Blob container)
  golden/
    2026-04-07/
      amanniquelive.bak
      compplanLive.bak
      compsys.bak
  reports/
    parity-2026-04-07T02-00-00Z.json
    parity-2026-04-07T02-00-00Z.html
  fixtures/
    orders/
      order_consultant_campaign_01.json
      order_dtc_no_campaign_01.json
      order_exclusive_item_01.json
    expected/
      order_consultant_campaign_01_am_diff.json
      order_dtc_no_campaign_01_am_diff.json
      order_exclusive_item_01_am_diff.json
```

### 5.5 Parity Shopify store reset

Before each parity run, the parity Shopify store is reset to a known state. The reset procedure:

```bash
# scripts/env/reset-parity-shopify.sh
# Resets the parity Shopify store to the golden baseline state.
# Requires SHOPIFY_PARITY_ACCESS_TOKEN in environment.

# 1. Delete all products in parity store
shopify api graphql --store annique-parity --query ./scripts/shopify/delete-all-products.graphql

# 2. Delete all customers in parity store
shopify api graphql --store annique-parity --query ./scripts/shopify/delete-all-customers.graphql

# 3. Import baseline product catalogue from golden fixture
shopify api graphql --store annique-parity --variables ./tests/parity/golden/products/baseline_shopify_snapshot.json \
  --query ./scripts/shopify/import-products.graphql

# 4. Import baseline customer set from golden fixture
shopify api graphql --store annique-parity --variables ./tests/parity/golden/consultants/sample_consultants.json \
  --query ./scripts/shopify/import-customers.graphql

echo "Parity Shopify store reset complete."
```

### 5.6 Cost management

The parity environment costs approximately $200/month when the AM VM is always running. With auto-shutdown between runs, the effective cost is significantly lower.

**Auto-shutdown configuration:**

The parity-am VM is configured with Azure VM auto-shutdown at 04:00 SAST (02:00 UTC). The parity workflow starts the VM at the beginning of each run and the auto-shutdown schedule handles post-run cleanup automatically.

**VM start command in parity workflow:**

```yaml
# In .github/workflows/parity.yml
- name: Start parity AM VM
  run: |
    az vm start \
      --resource-group rg-dbm-parity \
      --name vm-am-parity \
      --no-wait
    # Poll until running
    az vm wait \
      --resource-group rg-dbm-parity \
      --name vm-am-parity \
      --custom "instanceView.statuses[?code=='PowerState/running']"
```

---

## 6. Production Environment

### 6.1 Purpose and constraints

Production is the live customer-facing environment. Constraints:

- **No direct developer access to production AM.** All access via JIT VM Access requests, approved by the Azure subscription owner.
- **No manual deployments.** All production deployments via the `deploy-production.yml` workflow with two human approvals.
- **No schema changes to AM ever.** DBM only writes via the defined set of stored procedures. No DDL statements are ever executed against production AM.
- **Blue/green deployments only.** App Service deployment slots are used for all production deployments. The slot swap is the atomic go-live action.

### 6.2 Production AM connectivity

At go-live, one of the following connectivity models will be in use (confirmed with Annique IT before production deployment):

| Option | Description | Requirements |
|---|---|---|
| **A — VNet Peering** | Peer the DBM production VNet (`vnet-dbm-prod`) to Annique's existing Azure VNet. | Annique must have an existing Azure VNet containing AMSERVER-v9 or the Azure-migrated AM VM. |
| **B — Azure Hybrid Connection** | Use Azure App Service Hybrid Connections to reach on-prem AMSERVER-v9. | Requires the Hybrid Connection Manager agent installed on the on-prem network. No VPN needed. |
| **C — Site-to-Site VPN** | New Azure VPN Gateway connecting the DBM production VNet to Annique's on-prem network. | BGP routing, static IP on Annique firewall, ~$140/month VPN gateway cost. |
| **D — AM migrated to Azure** | AMSERVER-v9 migrated to an Azure IaaS VM in the DBM production VNet. | AM stays in the DBM VNet — simplest connectivity. Preferred long-term path. |

**Preferred path:** Option D (AM in Azure). Staging already validates this model. The staging AM VM is the proof-of-concept for the production deployment.

**Action required:** Annique IT must confirm the current on-premises topology and which connectivity option is in use. This must be resolved before the production deployment runbook in `docs/spec/03_infrastructure.md` §13 can be completed.

### 6.3 Production promotion gates

No code reaches production without passing all of these:

| Gate | Who validates | How |
|---|---|---|
| Unit tests pass | GitHub Actions | `ci.yml` — every PR |
| Integration tests pass (staging-am) | GitHub Actions | `deploy-staging.yml` — on merge to main |
| Parity report clean (< 7 days) | GitHub Actions | `deploy-production.yml` pre-flight check |
| Staging smoke test | Architect | Manual confirmation that staging is healthy |
| Two human approvals | Two designated approvers | GitHub Actions environment protection rules |
| Slot swap verified | Deployer | Health check at `/health/ready` on new slot before swap |

---

## 7. Environment Promotion Model

### 7.1 Branch-to-environment mapping

```
Developer workstation
  │
  ├── feature/* branches
  │     └── Run locally via Docker Compose
  │         Unit tests run on commit (pre-commit hook)
  │
  └── Pull Request to main
        ├── Unit tests (ci.yml — triggered on PR open/push)
        ├── Build check (ci.yml)
        └── Code review

main branch
  │
  ├── On merge → Auto-deploy to staging
  │     ├── Integration tests run against staging-am
  │     ├── Deploy to staging slot (blue/green)
  │     └── Slot swap on success
  │
  ├── Weekly / manual trigger → Parity run
  │     ├── Reset parity environment
  │     ├── Run parity harness
  │     └── Publish parity report to Azure Blob
  │
  └── Manual trigger + approvals → Production deploy
        ├── Pre-flight: parity report clean, staging green
        ├── Two human approvals (GitHub Environment protection)
        ├── Deploy to production staging slot
        └── Slot swap + monitor
```

### 7.2 GitHub Actions workflows

Four workflow files in `.github/workflows/`:

**`ci.yml` — every PR and commit to main:**

```yaml
name: CI
on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.0.x' }
      - run: dotnet test tests/DBM.Core.Tests --filter Category=Unit
               --logger "trx;LogFileName=unit-results.trx"
      - uses: actions/upload-artifact@v4
        with:
          name: unit-test-results
          path: "**/*.trx"

  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.0.x' }
      - run: dotnet publish src/DBM.Core -c Release -o ./publish/dbm-core
      - run: npm ci && npm run build
        working-directory: src/DBM.AdminConsole

  lint:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: dotnet format --verify-no-changes src/DBM.Core
      - run: npm run lint
        working-directory: src/DBM.AdminConsole
```

**`deploy-staging.yml` — on merge to main:**

```yaml
name: Deploy to Staging
on:
  push:
    branches: [main]

jobs:
  integration-tests:
    runs-on: ubuntu-latest
    environment: staging
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.0.x' }
      - uses: azure/login@v2
        with: { creds: '${{ secrets.AZURE_STAGING_CREDENTIALS }}' }
      - run: dotnet test tests/DBM.Integration.Tests --filter Category=Integration
        env:
          AM_CONNECTION_STRING: ${{ secrets.STAGING_AM_CONNECTION_STRING }}
          DBMSTATE_CONNECTION_STRING: ${{ secrets.STAGING_DBMSTATE_CONNECTION_STRING }}

  deploy-staging:
    needs: integration-tests
    runs-on: ubuntu-latest
    environment: staging
    steps:
      - uses: actions/checkout@v4
      - uses: azure/login@v2
        with: { creds: '${{ secrets.AZURE_STAGING_CREDENTIALS }}' }
      - run: dotnet publish src/DBM.Core -c Release -o ./publish/dbm-core
      - uses: azure/webapps-deploy@v3
        with:
          app-name: app-dbm-core-staging
          slot-name: staging
          package: ./publish/dbm-core
      # Verify health on staging slot before swap
      - run: |
          sleep 30
          curl -f https://app-dbm-core-staging-staging.azurewebsites.net/health/ready
      # Swap staging slot → production
      - run: az webapp deployment slot swap
               --resource-group rg-dbm-staging
               --name app-dbm-core-staging
               --slot staging --target-slot production
```

**`parity.yml` — manual trigger or weekly:**

```yaml
name: Parity Run
on:
  workflow_dispatch:
    inputs:
      golden_snapshot_date:
        description: 'Golden snapshot date (YYYY-MM-DD)'
        required: false
        default: '2026-04-07'
  schedule:
    - cron: '0 0 * * 0'   # Sunday 02:00 SAST = Sunday 00:00 UTC

env:
  GOLDEN_SNAPSHOT_DATE: '2026-04-07'   # Update after each new golden snapshot

jobs:
  reset-parity-env:
    runs-on: ubuntu-latest
    environment: parity
    steps:
      - uses: actions/checkout@v4
      - uses: azure/login@v2
        with: { creds: '${{ secrets.AZURE_PARITY_CREDENTIALS }}' }
      - name: Start parity AM VM
        run: |
          az vm start --resource-group rg-dbm-parity --name vm-am-parity
          az vm wait --resource-group rg-dbm-parity --name vm-am-parity \
            --custom "instanceView.statuses[?code=='PowerState/running']"
      - name: Reset parity-am from golden snapshot
        run: |
          az vm run-command invoke \
            --resource-group rg-dbm-parity \
            --name vm-am-parity \
            --command-id RunPowerShellScript \
            --scripts @scripts/am/reset-parity-am.ps1 \
            --parameters "StorageAccount=stdbmparity" \
                         "GoldenSnapshotDate=${{ env.GOLDEN_SNAPSHOT_DATE }}" \
                         "DbmSvcPassword=${{ secrets.PARITY_DBMSVC_PASSWORD }}"
      - name: Reset parity DBM State DB
        run: |
          sqlcmd -S ${{ secrets.PARITY_SQL_SERVER }} \
                 -U dbm_svc -P ${{ secrets.PARITY_DBMSTATE_PASSWORD }} \
                 -Q "DROP DATABASE IF EXISTS dbm_state_parity; CREATE DATABASE dbm_state_parity;"
          dotnet ef database update --project src/DBM.Core \
            --connection "${{ secrets.PARITY_DBMSTATE_CONNECTION_STRING }}"
      - name: Reset parity Shopify store
        run: bash scripts/env/reset-parity-shopify.sh
        env:
          SHOPIFY_PARITY_ACCESS_TOKEN: ${{ secrets.SHOPIFY_PARITY_ACCESS_TOKEN }}

  parity-tests:
    needs: reset-parity-env
    runs-on: ubuntu-latest
    environment: parity
    steps:
      - uses: actions/checkout@v4
      - run: dotnet test tests/DBM.Parity.Tests --filter Category=Parity
               --logger "trx;LogFileName=parity-results.trx"
        env:
          AM_CONNECTION_STRING: ${{ secrets.PARITY_AM_CONNECTION_STRING }}
          DBMSTATE_CONNECTION_STRING: ${{ secrets.PARITY_DBMSTATE_CONNECTION_STRING }}
          SHOPIFY_PARITY_ACCESS_TOKEN: ${{ secrets.SHOPIFY_PARITY_ACCESS_TOKEN }}
          PARITY_REPORT_OUTPUT: ./parity-report.json

  publish-report:
    needs: parity-tests
    runs-on: ubuntu-latest
    environment: parity
    steps:
      - uses: azure/login@v2
        with: { creds: '${{ secrets.AZURE_PARITY_CREDENTIALS }}' }
      - run: |
          TIMESTAMP=$(date -u +%Y-%m-%dT%H-%M-%SZ)
          az storage blob upload \
            --account-name stdbmparity \
            --container-name parity-artifacts \
            --name "reports/parity-${TIMESTAMP}.json" \
            --file ./parity-report.json \
            --auth-mode login
```

**`deploy-production.yml` — manual + approvals:**

```yaml
name: Deploy to Production
on:
  workflow_dispatch:

jobs:
  check-prerequisites:
    runs-on: ubuntu-latest
    steps:
      - uses: azure/login@v2
        with: { creds: '${{ secrets.AZURE_PROD_CREDENTIALS }}' }
      - name: Verify parity report clean (< 7 days)
        run: |
          python scripts/ci/check-parity-report.py \
            --storage-account stdbmparity \
            --max-age-days 7

  deploy-production:
    needs: check-prerequisites
    runs-on: ubuntu-latest
    environment: production   # Has two required human approvers configured
    steps:
      - uses: actions/checkout@v4
      - uses: azure/login@v2
        with: { creds: '${{ secrets.AZURE_PROD_CREDENTIALS }}' }
      - run: dotnet publish src/DBM.Core -c Release -o ./publish/dbm-core
      - uses: azure/webapps-deploy@v3
        with:
          app-name: app-dbm-core-prod
          slot-name: staging
          package: ./publish/dbm-core
      - run: |
          sleep 60
          curl -f https://app-dbm-core-prod-staging.azurewebsites.net/health/ready
      - run: az webapp deployment slot swap
               --resource-group rg-dbm-prod
               --name app-dbm-core-prod
               --slot staging --target-slot production
```

---

## 8. Shopify Store Configuration

### 8.1 Store tier summary

| Store | Shopify plan | Purpose | Custom app | Functions deployed |
|---|---|---|---|---|
| `annique-staging.myshopify.com` | Plus dev store | Integration testing, sprint reviews | DBM staging app | Yes (`shopify app deploy --config staging`) |
| `annique-parity.myshopify.com` | Partner dev store (free) | Parity runs — reset between runs | DBM parity app | Yes (deployed once; not changed between runs) |
| Live store | Shopify Plus | Production | DBM production app | Yes (`shopify app deploy --config production`) |

### 8.2 Custom app per environment

DBM uses a Shopify custom app (private app) installed in each store. Each environment has its own app install with its own API credentials. The `shopify.app.toml` supports multiple environments:

```toml
# shopify.app.toml
name = "Dieselbrook Middleware"
client_id = "{{ DBM_SHOPIFY_CLIENT_ID }}"

[build]
  automatically_update_urls_on_dev = false

[[webhooks.subscriptions]]
  topics = ["orders/create", "orders/updated", "orders/fulfilled",
            "orders/cancelled", "customers/create", "customers/update",
            "app/uninstalled"]

[environments.staging]
  store = "annique-staging.myshopify.com"

[environments.parity]
  store = "annique-parity.myshopify.com"

[environments.production]
  store = "annique.myshopify.com"
```

**Deploy commands:**

```bash
# Deploy to staging
shopify app deploy --config staging

# Deploy to parity (done once after each golden snapshot capture)
shopify app deploy --config parity

# Deploy to production (part of deploy-production.yml workflow)
shopify app deploy --config production
```

### 8.3 Webhook endpoint configuration

DBM Core exposes webhook endpoints at:

```
POST /webhooks/shopify/{topic}
```

Webhook delivery requires HMAC signature verification. The `X-Shopify-Hmac-Sha256` header is validated against the app's webhook secret for the relevant environment.

| Environment | Webhook base URL |
|---|---|
| staging | `https://app-dbm-core-staging.azurewebsites.net/webhooks/shopify` |
| parity | `https://app-dbm-core-parity.azurewebsites.net/webhooks/shopify` |
| production | `https://app-dbm-core-prod.azurewebsites.net/webhooks/shopify` |

Webhooks are registered automatically by the `shopify app deploy` command for topics listed in `shopify.app.toml`.

### 8.4 Shopify Functions deployment

Shopify Functions (Rust, in `src/DBM.ShopifyApp/functions/`) are compiled and deployed as part of `shopify app deploy`. Functions are deterministic: they read from product/customer metafields only.

Functions installed in staging and parity stores are identical to production. No environment-specific Function configuration exists — correctness is guaranteed by DBM syncing the correct metafield values into the target store.

---

## 9. Parallel Run Model

### 9.1 Purpose

Between Phase 1 deployment (DBM handling mirrored traffic) and production cutover, both NISource (legacy) and DBM run in parallel:

- NISource continues to write to production AM.
- DBM processes a mirrored copy of the same Shopify events, writing to **staging-am** (not production AM).
- A comparator compares DBM's staging-am writes against what NISource wrote to production AM.
- Divergences are investigated and resolved before cutover.

### 9.2 Parallel run architecture

```
Shopify Production
  │
  ├── Existing webhooks → NISource (continues normal operation)
  │     └── Writes to production AM (AMSERVER-v9)
  │
  └── Mirror webhooks → DBM staging
        └── Writes to staging-am
              │
              └── Parallel Run Comparator
                    ├── Queries production AM (read-only)
                    ├── Queries staging-am
                    └── Emits divergence report
```

### 9.3 Mirroring webhook setup

Shopify Plus supports multiple webhook subscriptions for the same topic. During the parallel run window, a second webhook subscription is added for each topic, pointing at the DBM staging endpoint.

```bash
# Add mirror webhooks (run once at parallel run start)
shopify api post --store annique.myshopify.com \
  --path /admin/api/2024-01/webhooks.json \
  --body '{
    "webhook": {
      "topic": "orders/create",
      "address": "https://app-dbm-core-staging.azurewebsites.net/webhooks/shopify/orders/create",
      "format": "json"
    }
  }'
# Repeat for each topic
```

DBM staging will process these events and write to staging-am. Staging-am is seeded from a recent production backup so the baseline is representative.

### 9.4 Parallel run comparator

The comparator is a scheduled Azure Function (or daily GitHub Actions run) that:

1. Queries a sample of orders from production AM that NISource has processed in the last 24 hours.
2. Queries staging-am for the same order numbers.
3. Compares the AM row structure field-by-field.
4. Emits a divergence report to the parity-artifacts blob container.
5. Flags divergences for investigation.

**Known allowlisted divergences (expected to differ):**

- Timestamps (`dtStamp`, `dtMod`) — these legitimately differ
- Auto-increment IDs that are sequence-dependent
- Any field explicitly documented as an intentional improvement

**Success condition for cutover approval:**

Zero unresolved non-allowlisted divergences over a rolling 7-day window.

---

## 10. POC / MVP Phase Gates

### 10.1 Phase overview

The environment model supports incremental domain deployment. Each phase has specific promotion requirements.

| Phase | Domains live | Go-live gate | Environment gate |
|---|---|---|---|
| **POC** | Product sync only | Parity run clean for product sync domain | Staging smoke test passes |
| **MVP** | Products + Order write-back + Pricing sync | Parity run clean for all three domains | 2 weeks parallel run, zero order divergences |
| **Phase 1** | All Phase 1 domains (PRD §3.1) | Parity run clean for all domains; all integration tests green | 4 weeks parallel run, zero unresolved divergences |
| **Cutover** | Switch live traffic from NISource to DBM | Parallel run clean 7-day window; manual approvals; rollback plan documented | Production health checks passing |

### 10.2 POC checklist

POC is the first proof that the end-to-end stack works. Requirements:

- [ ] Staging AM VM provisioned and seeded from production backup
- [ ] DBM Core deployed to staging App Service
- [ ] Staging Shopify store configured with custom app and DBM installed
- [ ] `ProductSyncService` runs a full sync: AM `icitem` → Shopify products
- [ ] Sync watermark persisted to DBM State DB
- [ ] Outbox relay confirms all Shopify API writes committed
- [ ] Delta sync runs after manual AM product update
- [ ] Integration test: `ProductSyncIntegrationTests` all green
- [ ] Parity test: product sync parity run — staging Shopify state matches production Shopify baseline

### 10.3 MVP checklist

MVP is the first customer-valuable capability. Requirements:

- [ ] All POC requirements met
- [ ] `OrderWritebackService` processes a real Shopify order from staging store
- [ ] AM write-back confirmed via `verify-am-connection.ps1` + manual AM query
- [ ] `CampaignPriceSyncService` precomputes prices for all active SKUs
- [ ] Pricing parity run clean: DBM prices match `sp_camp_getSPprice` oracle for all SKUs
- [ ] Order write-back parity run clean: 12 representative order fixtures pass
- [ ] Parallel run configured (mirror webhooks active)
- [ ] 2-week parallel run period with zero order AM divergences
- [ ] E2E test: order webhook → AM write-back → fulfillment event → Shopify order updated

### 10.4 Phase 1 checklist

Phase 1 completes all PRD phase 1 domains (excluding OTP, pending TC-04):

- [ ] All MVP requirements met
- [ ] Consultant sync: `ConsultantSyncService` delta sync operational
- [ ] Exclusive items: exclusive-item entitlement flags synced to Shopify customer metafields
- [ ] Admin console: campaign management UI operational
- [ ] Admin console: dead-letter queue replay operational
- [ ] Communications: email dispatch (non-OTP) operational
- [ ] All domain integration tests green
- [ ] Full parity run clean across all domains
- [ ] 4-week parallel run period with zero unresolved divergences
- [ ] Parallel run comparator report approved by Annique stakeholders

### 10.5 Cutover

Cutover is the atomic switch from NISource to DBM as the system-of-record integration layer.

**Cutover prerequisites:**

1. Phase 1 checklist complete.
2. Parallel run comparator: 7-day clean window (zero unresolved non-allowlisted divergences).
3. Rollback plan documented and approved:
   - Rollback procedure: re-point Shopify webhooks to NISource, verify NISource processes correctly.
   - Rollback window: 48 hours after cutover.
4. Annique IT sign-off: NISource is ready to be decommissioned (not removed until DBM stable).
5. Production AM connectivity confirmed (see §6.2).
6. Production deployment pipeline tested against staging — no open deployment issues.
7. Two human approvals in `deploy-production.yml` workflow.

**Cutover sequence:**

```
1. Pre-cutover window (T-24h)
   └── Last parallel run comparator check — must be clean
   └── Take production AM backup

2. Go-live (T=0)
   ├── Deploy DBM to production (via deploy-production.yml)
   ├── Verify health: GET /health/ready → 200
   ├── Register production Shopify webhooks (shopify app deploy --config production)
   └── Remove mirror webhooks (or set NISource to read-only mode)

3. Post-cutover monitoring (T+0 to T+48h)
   ├── Watch Application Insights for errors
   ├── Watch Service Bus DLQ counts
   ├── Spot-check AM order writes (verify-am-connection.ps1)
   └── Rollback trigger: any P1 incident → re-enable mirror → investigate
```

---

## 11. Environment Variables and Secrets

### 11.1 Secret inventory per environment

All secrets are stored in the environment's Azure Key Vault (see `docs/spec/03_infrastructure.md` §8 for Key Vault details). App Service reads secrets via Key Vault references.

| Secret name | Description | Environments |
|---|---|---|
| `am-sql-password` | Password for `dbm_svc` SQL login on AM instance | staging, parity, prod |
| `dbm-state-db-password` | DBM State DB connection password | staging, parity, prod |
| `shopify-api-secret` | Shopify custom app API secret | staging, parity, prod |
| `shopify-webhook-secret` | HMAC secret for webhook verification | staging, parity, prod |
| `shopify-access-token` | Access token for Shopify API calls | staging, parity, prod |
| `servicebus-connection-string` | Service Bus connection string (fallback; prefer Managed Identity) | staging, parity, prod |
| `github-actions-client-secret` | Service principal secret for GitHub Actions | N/A — in GitHub Secrets |
| `parity-dbmsvc-password` | Parity-specific `dbm_svc` password | parity only |

### 11.2 GitHub Actions secret inventory

Secrets stored in GitHub repository secrets (not in Azure Key Vault):

| GitHub Secret | Description |
|---|---|
| `AZURE_STAGING_CREDENTIALS` | Service principal JSON for staging deployments |
| `AZURE_PARITY_CREDENTIALS` | Service principal JSON for parity operations |
| `AZURE_PROD_CREDENTIALS` | Service principal JSON for production deployments |
| `STAGING_AM_CONNECTION_STRING` | Full connection string for staging-am (for integration tests) |
| `STAGING_DBMSTATE_CONNECTION_STRING` | DBM State DB connection for integration tests |
| `PARITY_AM_CONNECTION_STRING` | Connection string for parity-am |
| `PARITY_DBMSTATE_CONNECTION_STRING` | DBM State DB connection for parity runs |
| `PARITY_DBMSVC_PASSWORD` | Password passed to reset-parity-am.ps1 |
| `SHOPIFY_STAGING_ACCESS_TOKEN` | Shopify API access token for staging store |
| `SHOPIFY_PARITY_ACCESS_TOKEN` | Shopify API access token for parity store |

---

## 12. Observability per Environment

All environments have Application Insights configured. Non-production workspaces are isolated from production to prevent noise and false alerts.

| Workspace | Alerts | Sampling | Log retention | Daily cap |
|---|---|---|---|---|
| `ai-dbm-local` | None | N/A (local logs to console) | N/A | N/A |
| `ai-dbm-staging` | Warning-level only | 100% | 90 days | 2 GB |
| `ai-dbm-parity` | None (captured by test harness) | 100% | 30 days | 500 MB |
| `ai-dbm-prod` | Full alert set (see arch spec §8.3) | 100% (reduce at scale) | 365 days | 5 GB |

Parity test runs emit structured telemetry to `ai-dbm-parity` so run-over-run trends can be tracked:

```json
{
  "customDimensions": {
    "runId": "parity-2026-04-07T02-00-00Z",
    "domain": "pricing",
    "snapshotDate": "2026-04-07",
    "skusTested": 135,
    "passed": 135,
    "failed": 0
  },
  "name": "ParityRunCompleted"
}
```

---

## Evidence Base

| Artefact | Location |
|---|---|
| Infrastructure specification (Azure resources) | `docs/spec/03_infrastructure.md` |
| Architecture specification | `docs/spec/01_dbm_architecture.md` |
| Testing strategy | `docs/spec/05_testing_strategy.md` |
| PRD — phase 1 scope and NFRs | `docs/spec/00_prd.md` |
| Platform runtime cross-cut | `docs/analysis/16_platform_runtime_and_hosting_crosscut.md` |
| Data access and SQL contract | `docs/analysis/17_data_access_and_sql_contract_crosscut.md` |
| Auditability and idempotency cross-cut | `docs/analysis/18_auditability_idempotency_and_reconciliation_crosscut.md` |
| DBM–AM interface contract | `docs/analysis/24_dbm_am_interface_contract.md` |
| Pricing engine deep dive | `docs/analysis/20_pricing_engine_deep_dive.md` |
| Programme state and session notes | `docs/onboarding/04_session_state_april_2026.md` |
