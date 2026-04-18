# =============================================================================
#  Dieselbrook - Annique AM Server Discovery Script
# =============================================================================
#
#  Purpose: Capture a complete inventory of the AM server so that Dieselbrook
#           can plan the AM Azure migration and the DBM integration without
#           needing admin / SSH access to the production server.
#
#  This script is READ-ONLY. It:
#    - Does NOT modify any files, databases, registry keys, or services
#    - Does NOT change any configuration
#    - Does NOT install anything
#    - Does NOT send any data over the network - it writes a single text file
#      to C:\Temp which the operator can review and forward to Dieselbrook
#
#  What it captures:
#    1. Operating system, hardware, disk layout, domain membership
#    2. Network configuration (IP, gateway, DNS, listening/established ports)
#    3. Local admins, RDP users, current login sessions
#    4. Running services and scheduled tasks
#    5. Installed software (from the Windows uninstall registry)
#    6. AccountMate install location, version, licence files, config
#    7. Top-level folder structure on every drive
#    8. IIS sites, applications, and bindings (if IIS is present)
#    9. SQL Server version, databases, file sizes, linked servers, logins,
#       roles, SQL Agent jobs, backup history, schema counts, row counts
#       of key AM tables, pricing procedures metadata and source
#   10. Common application config files that typically contain connection
#       string metadata (NOT passwords - see note below)
#   11. ODBC DSN inventory
#
#  About sensitive data: the configuration files in section 10 may contain
#  plaintext SQL passwords. Those passwords are already on disk in cleartext;
#  this script simply copies what is there. Review the output file before
#  sending and redact anything you are uncomfortable sharing.
#
#  How to run:
#    1. Copy this .ps1 file to the AM server (via RDP copy-paste, email
#       attachment, USB, or any method)
#    2. Open PowerShell as Administrator (right-click -> Run as administrator)
#    3. cd to the folder containing this file
#    4. Run:   powershell -ExecutionPolicy Bypass -File .\annique-am-discovery.ps1
#    5. When it finishes, the output file path is shown - send that file
#       back to Dieselbrook (developer@pvm.co.za)
#
#  Expected run time: 2-5 minutes
#  Expected output size: 200-500 KB text file
# =============================================================================

$ErrorActionPreference = 'Continue'
$OutputDir = 'C:\Temp'
$Timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$OutputFile = "$OutputDir\Annique_AM_Discovery_$Timestamp.txt"

if (-not (Test-Path $OutputDir)) {
    New-Item -Path $OutputDir -ItemType Directory -Force | Out-Null
}

# Write helper functions
function Write-Line($text) { $text | Out-File -FilePath $OutputFile -Append -Encoding UTF8 }
function Write-Section($title) {
    Write-Line ''
    Write-Line ''
    Write-Line ('=' * 78)
    Write-Line $title.ToUpper()
    Write-Line ('=' * 78)
    Write-Host "-> $title" -ForegroundColor Cyan
}
function Write-Heading($text) {
    Write-Line ''
    Write-Line "--- $text ---"
}
function Write-Block($label, $block) {
    Write-Heading $label
    try {
        $result = & $block 2>&1 | Out-String
        if ($null -eq $result -or ($result -replace '\s','') -eq '') {
            Write-Line '(no output)'
        } else {
            Write-Line $result.TrimEnd()
        }
    } catch {
        Write-Line "ERROR: $($_.Exception.Message)"
    }
}

# =============================================================================
# HEADER
# =============================================================================
Write-Line 'DIESELBROOK - ANNIQUE AM SERVER DISCOVERY REPORT'
Write-Line ('=' * 78)
Write-Line "Generated      : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"
Write-Line "Host           : $env:COMPUTERNAME"
Write-Line "Running as     : $env:USERDOMAIN\$env:USERNAME"
Write-Line "Script version : 1.1 (2026-04-18)"
Write-Line ''
Write-Line 'This is a READ-ONLY discovery script. No changes are made to the system.'
Write-Line 'Please review the output file before sending; it may contain plaintext'
Write-Line 'passwords from application config files already present on the server.'

Write-Host ''
Write-Host 'Dieselbrook AM Server Discovery' -ForegroundColor Green
Write-Host '================================' -ForegroundColor Green
Write-Host "Output file: $OutputFile"
Write-Host ''

# =============================================================================
# 1. HOST PROFILE
# =============================================================================
Write-Section '1. Host profile'

Write-Block 'Operating System' {
    Get-WmiObject Win32_OperatingSystem |
        Select-Object Caption, Version, BuildNumber, OSArchitecture, InstallDate, LastBootUpTime, FreePhysicalMemory, TotalVirtualMemorySize |
        Format-List
}

Write-Block 'Computer and Memory' {
    Get-WmiObject Win32_ComputerSystem |
        Select-Object Manufacturer, Model, TotalPhysicalMemory, NumberOfProcessors, NumberOfLogicalProcessors, Domain, DnsHostName, DomainRole |
        Format-List
}

Write-Block 'CPU' {
    Get-WmiObject Win32_Processor |
        Select-Object Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, L2CacheSize, SocketDesignation |
        Format-List
}

Write-Block 'Disks (local drives)' {
    Get-WmiObject Win32_LogicalDisk -Filter 'DriveType=3' |
        Select-Object DeviceID, VolumeName, FileSystem,
            @{n='SizeGB';e={[math]::Round($_.Size/1GB,1)}},
            @{n='FreeGB';e={[math]::Round($_.FreeSpace/1GB,1)}},
            @{n='UsedPct';e={[math]::Round(100-($_.FreeSpace/$_.Size*100),1)}} |
        Format-Table -AutoSize
}

Write-Block 'PowerShell version' {
    $PSVersionTable | Format-List
}

# =============================================================================
# 2. NETWORKING
# =============================================================================
Write-Section '2. Networking'

Write-Block 'IP configuration' {
    Get-WmiObject Win32_NetworkAdapterConfiguration -Filter 'IPEnabled=True' |
        Select-Object Description, MACAddress, IPAddress, IPSubnet, DefaultIPGateway, DNSServerSearchOrder, DHCPEnabled, DNSDomainSuffixSearchOrder |
        Format-List
}

Write-Block 'IPv4 routing table' { route print -4 }
Write-Block 'DNS cache (recent)' { ipconfig /displaydns 2>&1 | Select-Object -First 80 }
Write-Block 'Listening TCP ports' { netstat -ano -p TCP | Select-String 'LISTENING' }
Write-Block 'Active outbound connections' { netstat -ano -p TCP | Select-String 'ESTABLISHED' }
Write-Block 'Listening UDP ports (top 20)' { netstat -ano -p UDP | Select-Object -First 20 }
Write-Block 'Windows Firewall profile state' { netsh advfirewall show allprofiles state }

# =============================================================================
# 3. USERS AND SESSIONS
# =============================================================================
Write-Section '3. Users and sessions'

Write-Block 'Local Administrators group' { net localgroup Administrators }
Write-Block 'Remote Desktop Users group' { net localgroup 'Remote Desktop Users' }
Write-Block 'Local user accounts' {
    Get-WmiObject Win32_UserAccount -Filter 'LocalAccount=True' |
        Select-Object Name, FullName, Disabled, Lockout, PasswordChangeable, PasswordExpires |
        Format-Table -AutoSize
}
Write-Block 'Current sessions (query user)' { query user 2>&1 }
Write-Block 'Current sessions (query session)' { query session 2>&1 }

# =============================================================================
# 4. SERVICES AND SCHEDULED TASKS
# =============================================================================
Write-Section '4. Services and scheduled tasks'

Write-Block 'Running services' {
    Get-WmiObject Win32_Service |
        Where-Object { $_.State -eq 'Running' } |
        Select-Object Name, DisplayName, StartMode, StartName, PathName |
        Sort-Object Name |
        Format-Table -AutoSize -Wrap
}

Write-Block 'Services set to Auto start (regardless of current state)' {
    Get-WmiObject Win32_Service |
        Where-Object { $_.StartMode -eq 'Auto' } |
        Select-Object Name, DisplayName, State, StartName |
        Sort-Object Name |
        Format-Table -AutoSize -Wrap
}

Write-Block 'Scheduled tasks (summary)' {
    schtasks /query /fo LIST /v 2>&1 |
        Select-String 'TaskName:|Task To Run:|Run As User:|Last Run Time:|Next Run Time:|Schedule Type:|Status:' |
        Out-String
}

# =============================================================================
# 5. INSTALLED SOFTWARE
# =============================================================================
Write-Section '5. Installed software'

Write-Block 'Installed programs (Windows uninstall registry)' {
    $keys = @(
        'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall',
        'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall'
    )
    Get-ChildItem $keys -ErrorAction SilentlyContinue |
        ForEach-Object { Get-ItemProperty $_.PSPath -ErrorAction SilentlyContinue } |
        Where-Object { $_.DisplayName } |
        Select-Object DisplayName, DisplayVersion, Publisher, InstallDate |
        Sort-Object Publisher, DisplayName |
        Format-Table -AutoSize -Wrap
}

# =============================================================================
# 6. ACCOUNTMATE INSTALLATION
# =============================================================================
Write-Section '6. AccountMate installation'

$amCandidatePaths = @(
    'C:\amsql', 'D:\amsql', 'E:\amsql',
    'C:\AMW', 'D:\AMW', 'E:\AMW',
    'C:\AM', 'D:\AM', 'E:\AM',
    'C:\AccountMate', 'D:\AccountMate', 'E:\AccountMate',
    'C:\amsql9', 'C:\amsql93'
)
$amFound = $amCandidatePaths | Where-Object { Test-Path $_ }

Write-Heading 'AM install paths detected'
if ($amFound) {
    $amFound | ForEach-Object { Write-Line "  $_" }
} else {
    Write-Line '(No AM install path found at standard locations. If AM is installed elsewhere, please tell Dieselbrook and re-run this script with the path added to $amCandidatePaths.)'
}

foreach ($amPath in $amFound) {
    Write-Heading "Folder contents: $amPath"
    Get-ChildItem $amPath -ErrorAction SilentlyContinue |
        Select-Object Mode, LastWriteTime, Length, Name |
        Sort-Object Name |
        Format-Table -AutoSize |
        Out-String | ForEach-Object { Write-Line $_ }

    foreach ($f in @('AMSETUP.AM', 'AMSETUP-test.AM', 'config.fpw', 'amsql.exe.config')) {
        $fp = Join-Path $amPath $f
        if (Test-Path $fp) {
            Write-Heading "$f contents"
            Get-Content $fp -ErrorAction SilentlyContinue | ForEach-Object { Write-Line $_ }
        }
    }

    Write-Heading 'Licence (.lic) files'
    Get-ChildItem "$amPath\*.lic" -ErrorAction SilentlyContinue |
        Select-Object Name, Length, LastWriteTime |
        Format-Table -AutoSize |
        Out-String | ForEach-Object { Write-Line $_ }
}

# =============================================================================
# 7. TOP-LEVEL FOLDERS ON ALL DRIVES
# =============================================================================
Write-Section '7. Drive content survey (top-level folders)'

$drives = Get-WmiObject Win32_LogicalDisk -Filter 'DriveType=3' | Select-Object -ExpandProperty DeviceID
foreach ($drive in $drives) {
    Write-Heading "${drive}\"
    Get-ChildItem "$drive\" -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.PSIsContainer } |
        Select-Object Name, LastWriteTime |
        Sort-Object Name |
        Format-Table -AutoSize |
        Out-String | ForEach-Object { Write-Line $_ }
}

# =============================================================================
# 8. IIS CONFIGURATION
# =============================================================================
Write-Section '8. IIS configuration'

$iisConfigPath = 'C:\Windows\System32\inetsrv\config\applicationHost.config'
if (Test-Path $iisConfigPath) {
    Write-Heading 'Sites, applications, and bindings'
    try {
        $cfg = [xml](Get-Content $iisConfigPath)
        $sites = $cfg.configuration.'system.applicationHost'.sites.site
        foreach ($site in $sites) {
            Write-Line ''
            Write-Line "SITE: $($site.name) (id=$($site.id))"
            foreach ($app in $site.application) {
                Write-Line "  APP: $($app.path)  ->  $($app.virtualDirectory.physicalPath)"
            }
            foreach ($b in $site.bindings.binding) {
                Write-Line "  BINDING: $($b.protocol) $($b.bindingInformation)"
            }
        }

        Write-Heading 'Application pools'
        $pools = $cfg.configuration.'system.applicationHost'.applicationPools.add
        foreach ($p in $pools) {
            Write-Line "  POOL: $($p.name)  runtime=$($p.managedRuntimeVersion)  mode=$($p.managedPipelineMode)  identity=$($p.processModel.identityType)"
        }
    } catch {
        Write-Line "Could not parse applicationHost.config: $($_.Exception.Message)"
    }
} else {
    Write-Line 'IIS not installed (applicationHost.config not found)'
}

# =============================================================================
# 9. SQL SERVER INVENTORY
# =============================================================================
Write-Section '9. SQL Server inventory'

# Locate sqlcmd
$sqlcmd = $null
$sqlcmdSearchPaths = @(
    'C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\*\Tools\Binn\SQLCMD.EXE',
    'C:\Program Files\Microsoft SQL Server\*\Tools\Binn\SQLCMD.EXE',
    'C:\Program Files (x86)\Microsoft SQL Server\*\Tools\Binn\SQLCMD.EXE'
)
foreach ($pattern in $sqlcmdSearchPaths) {
    $found = Get-ChildItem $pattern -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) { $sqlcmd = $found.FullName; break }
}
if (-not $sqlcmd) { $sqlcmd = 'sqlcmd' }
Write-Line "Using sqlcmd: $sqlcmd"

# Build the SQL script
$sqlFile = "$OutputDir\_dbm_discovery_$Timestamp.sql"
$sqlOutFile = "$OutputDir\_dbm_discovery_$Timestamp.out"

$sqlScript = @'
SET NOCOUNT ON;

PRINT '=== SERVER VERSION ===';
SELECT CAST(@@SERVERNAME AS VARCHAR(200)) AS info
UNION ALL SELECT CAST(@@VERSION AS VARCHAR(500))
UNION ALL SELECT 'Edition: ' + CAST(SERVERPROPERTY('Edition') AS VARCHAR(200))
UNION ALL SELECT 'ProductVersion: ' + CAST(SERVERPROPERTY('ProductVersion') AS VARCHAR(200))
UNION ALL SELECT 'ProductLevel: ' + CAST(SERVERPROPERTY('ProductLevel') AS VARCHAR(200))
UNION ALL SELECT 'MachineName: ' + CAST(SERVERPROPERTY('MachineName') AS VARCHAR(200))
UNION ALL SELECT 'InstanceName: ' + COALESCE(CAST(SERVERPROPERTY('InstanceName') AS VARCHAR(200)), 'DEFAULT')
UNION ALL SELECT 'Collation: ' + CAST(SERVERPROPERTY('Collation') AS VARCHAR(200));
GO

PRINT '';
PRINT '=== CONFIG (memory, parallelism, xp_cmdshell) ===';
SELECT name, value, value_in_use
FROM sys.configurations
WHERE name IN ('max server memory (MB)','min server memory (MB)','max degree of parallelism','cost threshold for parallelism','xp_cmdshell','clr enabled')
ORDER BY name;
GO

PRINT '';
PRINT '=== DATABASES ===';
SELECT name, database_id, state_desc, recovery_model_desc, compatibility_level, create_date
FROM sys.databases
ORDER BY name;
GO

PRINT '';
PRINT '=== DATABASE FILES (size and location) ===';
SELECT DB_NAME(database_id) AS DatabaseName, type_desc, name AS LogicalName, physical_name AS PhysicalFile,
       CAST(size * 8.0 / 1024 AS DECIMAL(10,1)) AS SizeMB
FROM sys.master_files
WHERE database_id > 4
ORDER BY DB_NAME(database_id), type_desc;
GO

PRINT '';
PRINT '=== LINKED SERVERS ===';
SELECT name, product, provider, data_source, catalog, is_remote_login_enabled, is_rpc_out_enabled, modify_date
FROM sys.servers
WHERE server_id != 0;
GO

PRINT '';
PRINT '=== LINKED SERVER LOGIN MAPPINGS ===';
SELECT s.name AS LinkedServer, l.remote_name AS RemoteLogin, l.uses_self_credential, sp.name AS LocalLogin
FROM sys.linked_logins l
JOIN sys.servers s ON l.server_id = s.server_id
LEFT JOIN sys.server_principals sp ON l.local_principal_id = sp.principal_id
WHERE s.server_id != 0;
GO

PRINT '';
PRINT '=== LOGINS (non-system) ===';
SELECT name, type_desc, is_disabled, default_database_name, create_date, modify_date
FROM sys.server_principals
WHERE type IN ('S','U','G')
  AND name NOT LIKE '##%'
  AND name NOT LIKE 'NT SERVICE%'
  AND name NOT LIKE 'NT AUTHORITY%'
ORDER BY type_desc, name;
GO

PRINT '';
PRINT '=== PRIVILEGED ROLE MEMBERSHIPS ===';
SELECT sp.name AS LoginName, r.name AS RoleName
FROM sys.server_role_members rm
JOIN sys.server_principals sp ON rm.member_principal_id = sp.principal_id
JOIN sys.server_principals r  ON rm.role_principal_id   = r.principal_id
ORDER BY r.name, sp.name;
GO

PRINT '';
PRINT '=== SQL AGENT JOBS ===';
SELECT j.name, j.enabled, j.date_created, j.date_modified, j.owner_sid,
       COALESCE((SELECT MAX(h.run_date) FROM msdb.dbo.sysjobhistory h WHERE h.job_id = j.job_id), 0) AS LastRunYYYYMMDD
FROM msdb.dbo.sysjobs j
ORDER BY name;
GO

PRINT '';
PRINT '=== TOP 25 RECENT BACKUPS ===';
SELECT TOP 25 b.database_name, b.backup_finish_date,
       CASE b.type WHEN 'D' THEN 'Full' WHEN 'I' THEN 'Diff' WHEN 'L' THEN 'Log' ELSE b.type END AS BkType,
       CAST(b.backup_size/1024.0/1024.0 AS INT) AS SizeMB,
       m.physical_device_name
FROM msdb.dbo.backupset b
JOIN msdb.dbo.backupmediafamily m ON b.media_set_id = m.media_set_id
ORDER BY b.backup_finish_date DESC;
GO

PRINT '';
PRINT '=== DATABASE BACKUP SUMMARY (latest full per DB) ===';
SELECT database_name, MAX(backup_finish_date) AS LastFullBackup, COUNT(*) AS TotalBackups
FROM msdb.dbo.backupset
WHERE type = 'D'
GROUP BY database_name
ORDER BY database_name;
GO

-- AMANNIQUELIVE-SPECIFIC SECTIONS (only if DB exists)
IF DB_ID('amanniquelive') IS NOT NULL
BEGIN
    USE amanniquelive;
    PRINT '';
    PRINT '=== AMANNIQUELIVE: SCHEMA COUNTS ===';
    SELECT 'Tables' AS ObjectType, COUNT(*) AS Cnt FROM sys.tables
    UNION ALL SELECT 'Procedures', COUNT(*) FROM sys.procedures
    UNION ALL SELECT 'Views', COUNT(*) FROM sys.views
    UNION ALL SELECT 'Functions', COUNT(*) FROM sys.objects WHERE type IN ('FN','IF','TF');
END
GO

IF DB_ID('amanniquelive') IS NOT NULL
BEGIN
    USE amanniquelive;
    PRINT '';
    PRINT '=== AMANNIQUELIVE: TOP 40 TABLES BY ROW COUNT ===';
    SELECT TOP 40 t.name AS TableName, SUM(p.rows) AS Rws,
           CAST(SUM(a.total_pages) * 8.0 / 1024 AS DECIMAL(10,1)) AS SizeMB
    FROM sys.tables t
    JOIN sys.indexes i ON t.object_id = i.object_id
    JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
    JOIN sys.allocation_units a ON p.partition_id = a.container_id
    WHERE i.index_id IN (0,1)
    GROUP BY t.name
    ORDER BY SUM(p.rows) DESC;
END
GO

IF DB_ID('amanniquelive') IS NOT NULL
BEGIN
    USE amanniquelive;
    PRINT '';
    PRINT '=== AMANNIQUELIVE: KEY TABLES (create/modify dates) ===';
    SELECT name, OBJECT_SCHEMA_NAME(object_id) AS SchemaName, create_date, modify_date
    FROM sys.tables
    WHERE name IN ('icitem','arcust','SOPortal','soxitems','Campaign','CampDetail','CampSku','arinvc','iccprc','mlmcust','iciwhs_daily','gltrsn','wsSetting')
    ORDER BY name;
END
GO

IF DB_ID('amanniquelive') IS NOT NULL
BEGIN
    USE amanniquelive;
    PRINT '';
    PRINT '=== AMANNIQUELIVE: TOP 30 RECENTLY MODIFIED PROCEDURES ===';
    SELECT TOP 30 name, create_date, modify_date
    FROM sys.procedures
    ORDER BY modify_date DESC;
END
GO

IF DB_ID('amanniquelive') IS NOT NULL
BEGIN
    USE amanniquelive;
    PRINT '';
    PRINT '=== AMANNIQUELIVE: PRICING AND WEB-SERVICE PROCEDURES ===';
    SELECT name, create_date, modify_date
    FROM sys.procedures
    WHERE name LIKE 'sp_camp%' OR name LIKE 'sp_ws%' OR name LIKE 'vsp_ic_%' OR name LIKE 'sp_NOP%' OR name LIKE '%price%'
    ORDER BY name;
END
GO

IF DB_ID('amanniquelive') IS NOT NULL
BEGIN
    USE amanniquelive;
    IF OBJECT_ID('dbo.wsSetting') IS NOT NULL
    BEGIN
        PRINT '';
        PRINT '=== AMANNIQUELIVE: wsSetting TABLE ===';
        SELECT Name, LEFT(CAST(Value AS VARCHAR(500)), 500) AS ValuePreview
        FROM dbo.wsSetting
        ORDER BY Name;
    END
END
GO

IF DB_ID('amanniquelive') IS NOT NULL
BEGIN
    USE amanniquelive;
    IF OBJECT_ID('dbo.SOPortal') IS NOT NULL
    BEGIN
        PRINT '';
        PRINT '=== AMANNIQUELIVE: SOPortal ROW COUNT AND RECENT ORDERS ===';
        SELECT COUNT(*) AS TotalRows, MIN(dcreate) AS EarliestOrder, MAX(dcreate) AS LatestOrder FROM dbo.SOPortal;
        SELECT TOP 10 csono, dcreate, dprinted, cStatus FROM dbo.SOPortal ORDER BY dcreate DESC;
    END
END
GO

IF DB_ID('amanniquelive') IS NOT NULL
BEGIN
    USE amanniquelive;
    PRINT '';
    PRINT '=== AMANNIQUELIVE: PRICING PROCEDURE SOURCE (sp_camp_getSPprice) ===';
    IF OBJECT_ID('dbo.sp_camp_getSPprice') IS NOT NULL
        SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.sp_camp_getSPprice')) AS ProcedureSource;
    ELSE PRINT '(sp_camp_getSPprice not found)';

    PRINT '';
    PRINT '=== AMANNIQUELIVE: CONTRACT PRICING SOURCE (vsp_ic_getcontractprice) ===';
    IF OBJECT_ID('dbo.vsp_ic_getcontractprice') IS NOT NULL
        SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.vsp_ic_getcontractprice')) AS ProcedureSource;
    ELSE PRINT '(vsp_ic_getcontractprice not found)';
END
GO

-- =========================================================================
-- INTEGRATION GRAPH: how production AM talks to the rest of the estate
-- =========================================================================

PRINT '';
PRINT '=== SQL AGENT JOB STEPS (the actual commands that run) ===';
-- Often reveals integration paths: CmdExec commands, PowerShell scripts, xp_cmdshell, linked-server queries
SELECT j.name AS JobName, s.step_id, s.step_name, s.subsystem, s.database_name, LEFT(s.command, 2000) AS Command
FROM msdb.dbo.sysjobs j
JOIN msdb.dbo.sysjobsteps s ON j.job_id = s.job_id
ORDER BY j.name, s.step_id;
GO

PRINT '';
PRINT '=== SQL AGENT ALERTS ===';
SELECT name, event_source, message_id, severity, enabled, has_notification, performance_condition
FROM msdb.dbo.sysalerts
ORDER BY name;
GO

PRINT '';
PRINT '=== SQL AGENT OPERATORS (who gets notified) ===';
SELECT name, enabled, email_address, pager_address, netsend_address
FROM msdb.dbo.sysoperators
ORDER BY name;
GO

PRINT '';
PRINT '=== DATABASE MAIL PROFILES ===';
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'msdb')
BEGIN
    IF OBJECT_ID('msdb.dbo.sysmail_profile') IS NOT NULL
        SELECT profile_id, name, description, last_mod_datetime FROM msdb.dbo.sysmail_profile ORDER BY name;
END
GO

PRINT '';
PRINT '=== DATABASE MAIL ACCOUNTS (SMTP servers and from-addresses) ===';
IF OBJECT_ID('msdb.dbo.sysmail_account') IS NOT NULL
    SELECT a.name AS AccountName, a.description, a.email_address AS FromAddress, a.replyto_address, s.servername AS SmtpServer, s.port, s.username, s.enable_ssl
    FROM msdb.dbo.sysmail_account a
    LEFT JOIN msdb.dbo.sysmail_server s ON a.account_id = s.account_id
    ORDER BY a.name;
GO

PRINT '';
PRINT '=== SQL CLR ASSEMBLIES (custom .NET loaded into SQL) ===';
IF DB_ID('amanniquelive') IS NOT NULL
BEGIN
    USE amanniquelive;
    SELECT name, clr_name, permission_set_desc, create_date, modify_date, is_user_defined
    FROM sys.assemblies
    WHERE is_user_defined = 1
    ORDER BY name;
END
GO

PRINT '';
PRINT '=== DDL AND DML TRIGGERS IN AMANNIQUELIVE ===';
IF DB_ID('amanniquelive') IS NOT NULL
BEGIN
    USE amanniquelive;
    -- DML triggers (attached to tables)
    SELECT
        OBJECT_SCHEMA_NAME(parent_id) AS SchemaName,
        OBJECT_NAME(parent_id) AS TableName,
        name AS TriggerName,
        is_disabled,
        is_instead_of_trigger,
        create_date,
        modify_date
    FROM sys.triggers
    WHERE is_ms_shipped = 0 AND parent_class = 1
    ORDER BY OBJECT_NAME(parent_id), name;

    PRINT '';
    PRINT '=== DATABASE-LEVEL DDL TRIGGERS IN AMANNIQUELIVE ===';
    SELECT name, create_date, modify_date, is_disabled
    FROM sys.triggers
    WHERE is_ms_shipped = 0 AND parent_class = 0
    ORDER BY name;
END
GO

PRINT '';
PRINT '=== SERVER-LEVEL DDL TRIGGERS ===';
SELECT name, create_date, modify_date, is_disabled, parent_class_desc
FROM sys.server_triggers
WHERE is_ms_shipped = 0
ORDER BY name;
GO

PRINT '';
PRINT '=== PROCEDURES / FUNCTIONS / TRIGGERS THAT CALL EXTERNAL SYSTEMS ===';
-- Finds every programmable object in amanniquelive whose body references
-- external integration (HTTP calls, OLE automation, xp_cmdshell, linked servers, etc)
IF DB_ID('amanniquelive') IS NOT NULL
BEGIN
    USE amanniquelive;
    SELECT DISTINCT
        o.name AS ObjectName,
        o.type_desc AS ObjectType,
        o.create_date,
        o.modify_date,
        CASE WHEN CHARINDEX('sp_ws_HTTP', m.definition) > 0 THEN 'Y' ELSE '' END AS CallsHTTP,
        CASE WHEN CHARINDEX('sp_OACreate', m.definition) > 0 OR CHARINDEX('sp_OAMethod', m.definition) > 0 THEN 'Y' ELSE '' END AS OLEAutomation,
        CASE WHEN CHARINDEX('xp_cmdshell', m.definition) > 0 THEN 'Y' ELSE '' END AS CmdShell,
        CASE WHEN CHARINDEX('OPENROWSET', m.definition) > 0 THEN 'Y' ELSE '' END AS OpenRowset,
        CASE WHEN CHARINDEX('OPENQUERY', m.definition) > 0 THEN 'Y' ELSE '' END AS OpenQuery,
        CASE WHEN CHARINDEX('http://', m.definition) > 0 OR CHARINDEX('https://', m.definition) > 0 THEN 'Y' ELSE '' END AS LiteralURL,
        CASE WHEN CHARINDEX('[AMSERVER-V9]', m.definition) > 0 THEN 'Y' ELSE '' END AS UsesAMServerV9,
        CASE WHEN CHARINDEX('[WEBSTORE]', m.definition) > 0 THEN 'Y' ELSE '' END AS UsesWebstore,
        CASE WHEN CHARINDEX('sp_send_dbmail', m.definition) > 0 THEN 'Y' ELSE '' END AS SendsDBMail,
        CASE WHEN CHARINDEX('BULK INSERT', m.definition) > 0 THEN 'Y' ELSE '' END AS BulkInsert
    FROM sys.sql_modules m
    JOIN sys.objects o ON m.object_id = o.object_id
    WHERE o.is_ms_shipped = 0
      AND (
            CHARINDEX('sp_ws_HTTP', m.definition) > 0
         OR CHARINDEX('sp_OACreate', m.definition) > 0
         OR CHARINDEX('sp_OAMethod', m.definition) > 0
         OR CHARINDEX('xp_cmdshell', m.definition) > 0
         OR CHARINDEX('http://', m.definition) > 0
         OR CHARINDEX('https://', m.definition) > 0
         OR CHARINDEX('OPENROWSET', m.definition) > 0
         OR CHARINDEX('OPENQUERY', m.definition) > 0
         OR CHARINDEX('[AMSERVER-V9]', m.definition) > 0
         OR CHARINDEX('[WEBSTORE]', m.definition) > 0
         OR CHARINDEX('sp_send_dbmail', m.definition) > 0
         OR CHARINDEX('BULK INSERT', m.definition) > 0
      )
    ORDER BY o.name;
END
GO

PRINT '';
PRINT '=== sp_ws_HTTP SOURCE (the HTTP helper many procs call) ===';
IF DB_ID('amanniquelive') IS NOT NULL
BEGIN
    USE amanniquelive;
    IF OBJECT_ID('dbo.sp_ws_HTTP') IS NOT NULL
        SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.sp_ws_HTTP')) AS sp_ws_HTTP_Source;
    ELSE PRINT '(sp_ws_HTTP not found)';
END
GO

PRINT '';
PRINT '=== SERVICE BROKER STATE PER DATABASE ===';
SELECT name, is_broker_enabled, service_broker_guid, log_reuse_wait_desc
FROM sys.databases
WHERE database_id > 4
ORDER BY name;
GO

PRINT '';
PRINT '=== SERVICE BROKER QUEUES / SERVICES / ROUTES (amanniquelive) ===';
IF DB_ID('amanniquelive') IS NOT NULL
BEGIN
    USE amanniquelive;
    SELECT 'Queue' AS ObjType, name, activation_procedure AS Detail FROM sys.service_queues WHERE is_ms_shipped = 0
    UNION ALL SELECT 'Service', name, CAST(broker_instance_identifier AS VARCHAR(50)) FROM sys.services WHERE is_ms_shipped = 0
    UNION ALL SELECT 'Route', name, address FROM sys.routes WHERE is_ms_shipped = 0;
END
GO

PRINT '';
PRINT '=== CURRENTLY CONNECTED SQL SESSIONS (non-system) ===';
SELECT TOP 100
    session_id,
    login_name,
    host_name,
    program_name,
    client_interface_name,
    login_time,
    last_request_start_time,
    CAST(host_process_id AS VARCHAR(20)) AS ClientPID
FROM sys.dm_exec_sessions
WHERE is_user_process = 1
ORDER BY login_time DESC;
GO

PRINT '';
PRINT '=== DISTINCT LOGIN / HOST / PROGRAM COMBINATIONS CONNECTING TO THIS SQL (with counts) ===';
-- This is the single most valuable view for understanding which external apps/machines
-- are actually consuming this SQL instance. Look for: unknown host_name values, unexpected
-- program_name values (custom apps), and consumer IPs that shouldn't be hitting prod.
SELECT login_name, host_name, program_name, client_interface_name,
       DB_NAME(database_id) AS DBName, COUNT(*) AS Conns
FROM sys.dm_exec_sessions
WHERE is_user_process = 1
GROUP BY login_name, host_name, program_name, client_interface_name, database_id
ORDER BY Conns DESC;
GO

PRINT '';
PRINT '=== REMOTE IP ADDRESSES CURRENTLY CONNECTED (via sys.dm_exec_connections) ===';
-- Maps each session to the actual remote IP address that opened it - critical for
-- spotting inbound integration traffic and identifying machines by IP when the
-- host_name is generic or spoofed.
SELECT c.client_net_address AS RemoteIP,
       COUNT(*) AS Conns,
       MIN(c.connect_time) AS FirstConnect,
       MAX(s.login_name) AS SampleLogin,
       MAX(s.host_name) AS SampleHostName,
       MAX(s.program_name) AS SampleProgram
FROM sys.dm_exec_connections c
JOIN sys.dm_exec_sessions s ON c.session_id = s.session_id
WHERE s.is_user_process = 1 AND c.client_net_address IS NOT NULL
GROUP BY c.client_net_address
ORDER BY Conns DESC;
GO

PRINT '';
PRINT '=== RECENT QUERIES REFERENCING LINKED SERVERS (plan cache) ===';
SELECT TOP 30
    qs.execution_count,
    qs.last_execution_time,
    qs.creation_time,
    LEFT(st.text, 500) AS QueryTextPreview
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
WHERE st.text LIKE '%AMSERVER-V9%' OR st.text LIKE '%WEBSTORE%' OR st.text LIKE '%OPENQUERY%' OR st.text LIKE '%OPENROWSET%'
ORDER BY qs.last_execution_time DESC;
GO

PRINT '';
PRINT '=== CURRENT DATABASE CONNECTIONS COUNT PER DATABASE ===';
SELECT DB_NAME(database_id) AS DatabaseName, COUNT(*) AS ConnectionCount
FROM sys.dm_exec_sessions
WHERE is_user_process = 1 AND database_id > 0
GROUP BY database_id
ORDER BY ConnectionCount DESC;
GO

-- NopIntegration database exists on production AM SQL per Nopintegration.ini configuration.
-- ANQ_SyncCustomerTOAM references [AMSERVER-V9].NopIntegration.dbo.NopFieldMapping.
-- If this box hosts NopIntegration, enumerate its schema.
IF DB_ID('NopIntegration') IS NOT NULL
BEGIN
    USE NopIntegration;
    PRINT '';
    PRINT '=== NopIntegration DB SCHEMA COUNTS ===';
    SELECT 'Tables' AS ObjType, COUNT(*) AS Cnt FROM sys.tables WHERE is_ms_shipped = 0
    UNION ALL SELECT 'Procedures', COUNT(*) FROM sys.procedures WHERE is_ms_shipped = 0
    UNION ALL SELECT 'Views', COUNT(*) FROM sys.views WHERE is_ms_shipped = 0
    UNION ALL SELECT 'Functions', COUNT(*) FROM sys.objects WHERE type IN ('FN','IF','TF') AND is_ms_shipped = 0
    UNION ALL SELECT 'Triggers', COUNT(*) FROM sys.triggers WHERE is_ms_shipped = 0;

    PRINT '';
    PRINT '=== NopIntegration: TOP 30 TABLES BY ROW COUNT ===';
    SELECT TOP 30 t.name AS TableName, SUM(p.rows) AS Rws, CAST(SUM(a.total_pages)*8.0/1024 AS DECIMAL(10,1)) AS SizeMB
    FROM sys.tables t
    JOIN sys.indexes i ON t.object_id = i.object_id
    JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
    JOIN sys.allocation_units a ON p.partition_id = a.container_id
    WHERE i.index_id IN (0,1) AND t.is_ms_shipped = 0
    GROUP BY t.name
    ORDER BY SUM(p.rows) DESC;

    PRINT '';
    PRINT '=== NopIntegration: ALL TABLES (names + modify dates) ===';
    SELECT name, OBJECT_SCHEMA_NAME(object_id) AS SchemaName, create_date, modify_date
    FROM sys.tables WHERE is_ms_shipped = 0 ORDER BY name;

    PRINT '';
    PRINT '=== NopIntegration: ALL PROCEDURES ===';
    SELECT name, create_date, modify_date FROM sys.procedures WHERE is_ms_shipped = 0 ORDER BY name;

    PRINT '';
    PRINT '=== NopIntegration: TRIGGERS ===';
    SELECT OBJECT_NAME(parent_id) AS TableName, name AS TriggerName, is_disabled, create_date, modify_date
    FROM sys.triggers WHERE is_ms_shipped = 0 AND parent_class = 1
    ORDER BY OBJECT_NAME(parent_id), name;

    PRINT '';
    PRINT '=== NopIntegration: NopFieldMapping contents (the field-name translation table) ===';
    IF OBJECT_ID('dbo.NopFieldMapping') IS NOT NULL
        SELECT TOP 200 * FROM dbo.NopFieldMapping;
    ELSE PRINT '(NopFieldMapping table not found)';
END
GO
'@

Set-Content -Path $sqlFile -Value $sqlScript -Encoding ASCII

Write-Heading 'SQL discovery output'
try {
    # -y 0 = unlimited column display width (needed for long proc source output)
    # Note: -y and -W are mutually exclusive in newer sqlcmd, so we use only -y
    $sqlStderrFile = "$OutputDir\_dbm_discovery_stderr_$Timestamp.txt"
    & $sqlcmd -S localhost -E -i $sqlFile -y 0 -o $sqlOutFile 2>$sqlStderrFile
    $sqlExit = $LASTEXITCODE
    Write-Line "sqlcmd exit code: $sqlExit"
    if (Test-Path $sqlOutFile) {
        Write-Line '--- sqlcmd stdout ---'
        Get-Content $sqlOutFile | ForEach-Object { Write-Line $_ }
    } else {
        Write-Line '(sqlcmd produced no output file)'
    }
    if (Test-Path $sqlStderrFile) {
        $stderrContent = Get-Content $sqlStderrFile -ErrorAction SilentlyContinue
        if ($stderrContent) {
            Write-Line '--- sqlcmd stderr ---'
            $stderrContent | ForEach-Object { Write-Line $_ }
        }
        Remove-Item $sqlStderrFile -Force -ErrorAction SilentlyContinue
    }
} catch {
    Write-Line "sqlcmd failed to launch: $($_.Exception.Message)"
    Write-Line 'If this machine is not a SQL Server, or if the account running this script'
    Write-Line 'is not a SQL sysadmin, this section may be empty. That is expected.'
}
Remove-Item $sqlFile -Force -ErrorAction SilentlyContinue
Remove-Item $sqlOutFile -Force -ErrorAction SilentlyContinue

# =============================================================================
# 10. APPLICATION CONFIG FILES (connection string audit)
# =============================================================================
Write-Section '10. Application config files'

$configFiles = @(
    'C:\amsql\AMSETUP.AM',
    'C:\amsql\AMSETUP-test.AM',
    'C:\amsql\config.fpw',
    'C:\CompPLan\Compplan.ini',
    'C:\CompPLan\Mlm.ini',
    'C:\Email2SMS\config.fpw',
    'C:\Email2SMS\email2sms.ini',
    'C:\Eft\config.fpw',
    'C:\NopIntegration\deploy\Nopintegration.ini',
    'C:\Backoffice\deploy\Backoffice.ini',
    'C:\WebConnectionProjects\Webstore\deploy\Webstore.ini',
    'E:\Projects\BackofficeAPI\deploy\BackofficeAPI.ini',
    'C:\Windows\System32\drivers\etc\hosts'
)
foreach ($cfg in $configFiles) {
    if (Test-Path $cfg) {
        Write-Heading "File: $cfg"
        Get-Content $cfg -ErrorAction SilentlyContinue | ForEach-Object { Write-Line $_ }
    }
}

# =============================================================================
# 11. ODBC DSN INVENTORY
# =============================================================================
Write-Section '11. ODBC DSNs'

Write-Block 'System DSNs (64-bit)' {
    Get-ChildItem 'HKLM:\SOFTWARE\ODBC\ODBC.INI' -ErrorAction SilentlyContinue |
        ForEach-Object {
            $props = Get-ItemProperty $_.PSPath -ErrorAction SilentlyContinue
            [PSCustomObject]@{
                DSN      = $_.PSChildName
                Server   = $props.Server
                Database = $props.Database
                Driver   = $props.Driver
            }
        } | Format-Table -AutoSize
}

Write-Block 'System DSNs (32-bit / WOW6432)' {
    Get-ChildItem 'HKLM:\SOFTWARE\WOW6432Node\ODBC\ODBC.INI' -ErrorAction SilentlyContinue |
        ForEach-Object {
            $props = Get-ItemProperty $_.PSPath -ErrorAction SilentlyContinue
            [PSCustomObject]@{
                DSN      = $_.PSChildName
                Server   = $props.Server
                Database = $props.Database
                Driver   = $props.Driver
            }
        } | Format-Table -AutoSize
}

# =============================================================================
# 12. HOSTS FILE (DNS overrides — critical for linked-server hostname resolution)
# =============================================================================
Write-Section '12. Hosts file (DNS overrides)'

$hostsPath = 'C:\Windows\System32\drivers\etc\hosts'
if (Test-Path $hostsPath) {
    Write-Heading 'Full hosts file'
    Get-Content $hostsPath | ForEach-Object { Write-Line $_ }

    Write-Heading 'Non-comment entries (the actual overrides)'
    Get-Content $hostsPath | Where-Object { $_ -match '^\s*[^#\s]' -and $_ -match '\S+\s+\S+' } | ForEach-Object { Write-Line $_ }

    Write-Heading 'Entries mentioning AMSERVER, annique, or v9'
    Get-Content $hostsPath | Where-Object { $_ -match '(?i)amserver|annique|v9' } | ForEach-Object { Write-Line $_ }
}

Write-Block 'Active DNS resolution: common Annique hostnames (from this box)' {
    $hosts = @(
        # AM / linked server hostnames
        'AMSERVER-v9', 'AMSERVER-V9', 'AMSERVER-v9.annique.local',
        'AMSERVER-TEST', 'AMSERVER-TEST.annique.local',
        'ITREPORT-SERVER', 'ITREPORT-SERVER.annique.local', 'ITREPORT-SERVER\ANREPORTS',
        'andc.annique.local',
        # Public SA hosts
        'away1.annique.com', 'away2.annique.com',
        'nopintegration.annique.com', 'annique.com', 'www.annique.com',
        'stage.annique.com', 'stage.annique.co.na',
        'anniquestore.co.za', 'stage.anniquestore.co.za', 'shopapi.annique.com',
        'quiz.annique.com', 'backoffice.annique.com', 'newregistration.annique.com',
        'anniqueshop.com',
        # Known internet IPs (reverse lookups for sanity)
        '196.3.178.122', '196.3.178.123',
        '20.87.212.38',
        '41.193.227.190',
        '172.19.16.100', '172.19.16.101', '172.19.16.16', '172.19.16.27', '172.19.16.63'
    )
    foreach ($h in $hosts) {
        $result = nslookup $h 2>&1 | Out-String
        "=== $h ==="
        $result.TrimEnd()
        ''
    }
}

Write-Block 'Reverse DNS of currently-ESTABLISHED outbound connections' {
    # Each remote IP we are currently talking to - try to get the hostname.
    # This surfaces integration endpoints we might not have thought of.
    try {
        $remoteIps = netstat -ano -p TCP 2>$null |
            Select-String 'ESTABLISHED' |
            ForEach-Object {
                $parts = ($_ -split '\s+') | Where-Object { $_ -ne '' }
                if ($parts.Count -ge 3) {
                    $remote = $parts[2]
                    # Strip port
                    if ($remote -match '^(.+):(\d+)$') { $matches[1] }
                }
            } |
            Where-Object { $_ -and $_ -notmatch '^(127\.|10\.|::1|0\.0\.0\.0)' -and $_ -notmatch '^(10\.)' } |
            Sort-Object -Unique
        foreach ($ip in $remoteIps) {
            $ptr = nslookup $ip 2>$null | Out-String
            $name = ''
            if ($ptr -match 'Name:\s*(\S+)') { $name = $matches[1] }
            "$ip -> $(if ($name) { $name } else { '(no PTR record)' })"
        }
    } catch {
        "ERROR: $($_.Exception.Message)"
    }
}

# =============================================================================
# 13. SMB SHARES
# =============================================================================
Write-Section '13. SMB shares'

Write-Block 'net share (all shares)' { net share 2>&1 }
Write-Block 'Share permissions (Win32_LogicalShareSecuritySetting)' {
    Get-WmiObject Win32_Share -ErrorAction SilentlyContinue |
        Where-Object { $_.Type -eq 0 } |
        Select-Object Name, Path, Description, AllowMaximum |
        Format-Table -AutoSize
}

# =============================================================================
# 14. WINDOWS FIREWALL RULES (inbound surface)
# =============================================================================
Write-Section '14. Windows Firewall rules'

Write-Block 'All firewall rules (verbose)' {
    # netsh works back to Vista / 2008, unlike Get-NetFirewallRule which is 2012+
    netsh advfirewall firewall show rule name=all verbose 2>&1
}

# =============================================================================
# 15. COM+ REGISTERED APPLICATIONS (NISource pattern, other VFP integrations)
# =============================================================================
Write-Section '15. COM+ registered applications'

Write-Block 'COM+ Applications' {
    try {
        $admin = New-Object -ComObject 'COMAdmin.COMAdminCatalog'
        $apps = $admin.GetCollection('Applications')
        $apps.Populate()
        foreach ($app in $apps) {
            $name = $app.Value('Name')
            $id = $app.Value('ID')
            $identity = $app.Value('Identity')
            $activation = $app.Value('Activation')
            Write-Host "  $name [$id] identity=$identity activation=$activation"
            "  Name: $name"
            "    ID       : $id"
            "    Identity : $identity"
            "    Activation: $activation"
            # Enumerate components inside the application
            try {
                $comps = $admin.GetCollection('Components', $id)
                $comps.Populate()
                foreach ($c in $comps) {
                    "      Component: $($c.Value('DisplayName')) (CLSID $($c.Value('CLSID')))  ProgId=$($c.Value('ProgID'))"
                }
            } catch {
                "      (could not enumerate components: $($_.Exception.Message))"
            }
        }
    } catch {
        "ERROR: $($_.Exception.Message)"
    }
}

# =============================================================================
# 16. WMI EVENT SUBSCRIPTIONS + LEGACY AT-JOBS (silent scheduled integrations)
# =============================================================================
Write-Section '16. WMI event subscriptions and legacy at-jobs'

Write-Block 'WMI permanent event filters (root\subscription)' {
    Get-WmiObject -Namespace 'root\subscription' -Class __EventFilter -ErrorAction SilentlyContinue |
        Select-Object Name, Query, QueryLanguage |
        Format-List
}
Write-Block 'WMI permanent event consumers (root\subscription)' {
    Get-WmiObject -Namespace 'root\subscription' -Class __EventConsumer -ErrorAction SilentlyContinue |
        Select-Object Name, CommandLineTemplate, ExecutablePath |
        Format-List
}
Write-Block 'WMI FilterToConsumerBindings (root\subscription)' {
    Get-WmiObject -Namespace 'root\subscription' -Class __FilterToConsumerBinding -ErrorAction SilentlyContinue |
        Select-Object Consumer, Filter |
        Format-List
}
Write-Block 'Legacy at-jobs (Win32_ScheduledJob)' {
    Get-WmiObject Win32_ScheduledJob -ErrorAction SilentlyContinue |
        Select-Object JobId, Command, RunRepeatedly, StartTime |
        Format-List
}

# =============================================================================
# 17. FULL PROCESS LIST WITH COMMAND LINES
# =============================================================================
Write-Section '17. Process list with command lines'

Write-Block 'All processes with executable path and command line' {
    Get-WmiObject Win32_Process -ErrorAction SilentlyContinue |
        Sort-Object Name |
        Select-Object @{n='PID';e={$_.ProcessId}}, Name, @{n='ParentPID';e={$_.ParentProcessId}}, @{n='CmdLine';e={$_.CommandLine}} |
        Format-Table -AutoSize -Wrap
}

# =============================================================================
# 18. NETSTAT SNAPSHOTS (sampled 3x over 30s — catches periodic connections)
# =============================================================================
Write-Section '18. Netstat snapshots (periodic sampling)'

for ($i = 1; $i -le 3; $i++) {
    Write-Heading "Snapshot $i (established connections) at $(Get-Date -Format 'HH:mm:ss')"
    try {
        $ns = netstat -ano -p TCP 2>&1 | Select-String 'ESTABLISHED' | Out-String
        if ($ns -and $ns.Trim() -ne '') {
            Write-Line $ns.TrimEnd()
        } else {
            Write-Line '(no established TCP connections observed)'
        }
    } catch {
        Write-Line "ERROR: $($_.Exception.Message)"
    }
    if ($i -lt 3) {
        Write-Host "  waiting 10s for next snapshot..." -ForegroundColor DarkGray
        Start-Sleep -Seconds 10
    }
}

# Integration-focused summary: group established connections by (process, remote IP)
# This reveals who is talking to what - the central question for understanding
# integration traffic. Inbound (someone connecting to our listening ports) and
# outbound (we're connecting to someone) are shown separately.
Write-Heading 'Connection summary: outbound (process -> remote IP) grouped'
try {
    $nsEst = netstat -ano -p TCP 2>$null | Select-String 'ESTABLISHED'
    # Parse to structured objects
    $connObjs = @()
    foreach ($line in $nsEst) {
        $parts = ($line -split '\s+') | Where-Object { $_ -ne '' }
        if ($parts.Count -ge 5) {
            $local = $parts[1]
            $remote = $parts[2]
            $procId = $parts[4]
            $localIP = ''; $localPort = 0
            $remoteIP = ''; $remotePort = 0
            if ($local -match '^(.+):(\d+)$') { $localIP = $matches[1]; $localPort = [int]$matches[2] }
            if ($remote -match '^(.+):(\d+)$') { $remoteIP = $matches[1]; $remotePort = [int]$matches[2] }
            $connObjs += [PSCustomObject]@{ ProcId = [int]$procId; LocalIP = $localIP; LocalPort = $localPort; RemoteIP = $remoteIP; RemotePort = $remotePort }
        }
    }
    # Get ProcId -> process name mapping once
    $procMap = @{}
    foreach ($p in (Get-WmiObject Win32_Process -ErrorAction SilentlyContinue)) { $procMap[[int]$p.ProcessId] = $p.Name }
    # Heuristic: if LocalPort is a well-known service port (<10000) and RemotePort is high, it's likely INBOUND
    # Otherwise (LocalPort high, RemotePort well-known), it's OUTBOUND.
    $outbound = $connObjs | Where-Object { $_.RemotePort -lt 10000 -and $_.LocalPort -gt 10000 } | Where-Object { $_.RemoteIP -notmatch '^(127\.|0\.0\.0\.0|::1)$' }
    $inbound = $connObjs | Where-Object { $_.LocalPort -lt 10000 -and $_.RemotePort -gt 10000 } | Where-Object { $_.RemoteIP -notmatch '^(127\.|0\.0\.0\.0|::1)$' }

    'OUTBOUND (we connected OUT) grouped by process + remote endpoint:'
    $outbound | Group-Object @{e={$procMap[$_.ProcId]}}, RemoteIP, RemotePort |
        Sort-Object Count -Descending |
        Select-Object @{n='Count';e={$_.Count}}, @{n='Process';e={$procMap[$_.Group[0].ProcId]}}, @{n='Remote';e={"$($_.Group[0].RemoteIP):$($_.Group[0].RemotePort)"}} |
        Format-Table -AutoSize | Out-String | ForEach-Object { Write-Line $_ }

    ''
    'INBOUND (someone connected TO us) grouped by local port + remote IP:'
    $inbound | Group-Object @{e={$_.LocalPort}}, RemoteIP |
        Sort-Object Count -Descending |
        Select-Object @{n='Count';e={$_.Count}}, @{n='LocalPort';e={$_.Group[0].LocalPort}}, @{n='RemoteIP';e={$_.Group[0].RemoteIP}}, @{n='Process';e={$procMap[$_.Group[0].ProcId]}} |
        Format-Table -AutoSize | Out-String | ForEach-Object { Write-Line $_ }
} catch {
    Write-Line "ERROR: $($_.Exception.Message)"
}

# =============================================================================
# 19. IIS LOG VOLUME SUMMARY (is IIS actually serving traffic?)
# =============================================================================
Write-Section '19. IIS log volume summary'

$iisLogPath = 'C:\inetpub\logs\LogFiles'
if (Test-Path $iisLogPath) {
    Write-Block 'IIS log sites and their most recent log files' {
        Get-ChildItem $iisLogPath -ErrorAction SilentlyContinue |
            Where-Object { $_.PSIsContainer } |
            ForEach-Object {
                $site = $_.Name
                $latest = Get-ChildItem $_.FullName -Filter '*.log' -ErrorAction SilentlyContinue |
                          Sort-Object LastWriteTime -Descending |
                          Select-Object -First 3
                foreach ($log in $latest) {
                    [PSCustomObject]@{
                        Site = $site
                        LogFile = $log.Name
                        SizeKB = [math]::Round($log.Length / 1KB, 1)
                        LastWrite = $log.LastWriteTime
                    }
                }
            } |
            Format-Table -AutoSize
    }

    Write-Block 'IIS log total disk usage per site' {
        Get-ChildItem $iisLogPath -ErrorAction SilentlyContinue |
            Where-Object { $_.PSIsContainer } |
            ForEach-Object {
                $site = $_.Name
                $total = (Get-ChildItem $_.FullName -Recurse -ErrorAction SilentlyContinue |
                          Measure-Object -Property Length -Sum).Sum
                [PSCustomObject]@{
                    Site = $site
                    TotalMB = [math]::Round($total / 1MB, 1)
                }
            } |
            Format-Table -AutoSize
    }

    Write-Block 'Last 10 lines of the most recent IIS log (any site)' {
        $latest = Get-ChildItem $iisLogPath -Recurse -Filter '*.log' -ErrorAction SilentlyContinue |
                  Sort-Object LastWriteTime -Descending |
                  Select-Object -First 1
        if ($latest) {
            "File: $($latest.FullName)"
            "Last write: $($latest.LastWriteTime)"
            ''
            Get-Content $latest.FullName -ErrorAction SilentlyContinue | Select-Object -Last 10
        } else {
            '(no IIS log files found)'
        }
    }

    # Per-site URL + source-IP analysis for the most recent log
    # This is THE most valuable IIS analysis - tells us WHICH endpoints are being
    # called and BY WHOM. Critical for identifying integration callers.
    Get-ChildItem $iisLogPath -ErrorAction SilentlyContinue | Where-Object { $_.PSIsContainer } | ForEach-Object {
        $siteFolder = $_
        $mostRecent = Get-ChildItem $siteFolder.FullName -Filter '*.log' -ErrorAction SilentlyContinue |
                      Sort-Object LastWriteTime -Descending |
                      Select-Object -First 1
        if ($mostRecent -and $mostRecent.Length -gt 0) {
            Write-Heading "Site $($siteFolder.Name) - URL endpoint frequency (most recent log: $($mostRecent.Name))"
            try {
                # IIS W3C log format: typical fields are date time s-ip cs-method cs-uri-stem cs-uri-query s-port cs-username c-ip ...
                # Field index for cs-uri-stem varies; we find it from the #Fields header.
                $logLines = Get-Content $mostRecent.FullName -ErrorAction SilentlyContinue
                $fieldsLine = $logLines | Where-Object { $_ -match '^#Fields:' } | Select-Object -Last 1
                if (-not $fieldsLine) {
                    Write-Line '(no #Fields header in log; skipping structured analysis)'
                } else {
                    $fields = ($fieldsLine -replace '^#Fields:\s*', '') -split '\s+'
                    $uriIdx = [array]::IndexOf($fields, 'cs-uri-stem')
                    $queryIdx = [array]::IndexOf($fields, 'cs-uri-query')
                    $cipIdx = [array]::IndexOf($fields, 'c-ip')
                    $methodIdx = [array]::IndexOf($fields, 'cs-method')
                    $statusIdx = [array]::IndexOf($fields, 'sc-status')
                    $dataLines = $logLines | Where-Object { $_ -notmatch '^#' -and $_.Trim() -ne '' }

                    # URL frequency
                    "Top URL paths (by hit count):"
                    $dataLines | ForEach-Object {
                        $p = $_ -split '\s+'
                        if ($p.Count -gt $uriIdx -and $uriIdx -ge 0) { $p[$uriIdx] }
                    } | Group-Object | Sort-Object Count -Descending | Select-Object -First 20 |
                        Select-Object Count, @{n='URL';e={$_.Name}} | Format-Table -AutoSize | Out-String | ForEach-Object { $_ }

                    ''
                    "Top source IPs (by hit count):"
                    $dataLines | ForEach-Object {
                        $p = $_ -split '\s+'
                        if ($p.Count -gt $cipIdx -and $cipIdx -ge 0) { $p[$cipIdx] }
                    } | Group-Object | Sort-Object Count -Descending | Select-Object -First 20 |
                        Select-Object Count, @{n='SourceIP';e={$_.Name}} | Format-Table -AutoSize | Out-String | ForEach-Object { $_ }

                    ''
                    "Status code distribution:"
                    $dataLines | ForEach-Object {
                        $p = $_ -split '\s+'
                        if ($p.Count -gt $statusIdx -and $statusIdx -ge 0) { $p[$statusIdx] }
                    } | Group-Object | Sort-Object Count -Descending |
                        Select-Object Count, @{n='Status';e={$_.Name}} | Format-Table -AutoSize | Out-String | ForEach-Object { $_ }

                    ''
                    "HTTP method distribution:"
                    $dataLines | ForEach-Object {
                        $p = $_ -split '\s+'
                        if ($p.Count -gt $methodIdx -and $methodIdx -ge 0) { $p[$methodIdx] }
                    } | Group-Object | Sort-Object Count -Descending |
                        Select-Object Count, @{n='Method';e={$_.Name}} | Format-Table -AutoSize | Out-String | ForEach-Object { $_ }
                }
            } catch {
                "ERROR analysing log: $($_.Exception.Message)"
            }
        }
    }
} else {
    Write-Line 'IIS log directory not found — IIS probably not installed or logging disabled'
}

# =============================================================================
# 20. FILE WATCHERS AND CONFIGURED INTEGRATION FOLDERS
# =============================================================================
Write-Section '20. Integration file-drop folders'

$integrationPaths = @(
    'C:\Eft', 'C:\Eft\in', 'C:\Eft\out', 'C:\Eft\processed',
    'C:\Imports', 'C:\Exports', 'C:\Inbox', 'C:\Outbox',
    'C:\FTP', 'C:\Files',
    'D:\Eft', 'D:\Imports', 'D:\Exports',
    'E:\Eft', 'E:\Imports', 'E:\Exports',
    'C:\AMW\export', 'C:\AMW\import',
    'C:\amsql\export', 'C:\amsql\import',
    'C:\AnniqueFiles', 'C:\AnniqueData'
)
foreach ($p in $integrationPaths) {
    if (Test-Path $p) {
        Write-Heading "Folder: $p"
        Get-ChildItem $p -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 20 |
            Select-Object Mode, LastWriteTime, Length, Name |
            Format-Table -AutoSize |
            Out-String | ForEach-Object { Write-Line $_ }
    }
}

# =============================================================================
# 21. INTEGRATION RUNTIME SCAN
# =============================================================================
# Find running processes that look like Annique integrations - daemons, sync
# apps, or web-handler processes. Matches common patterns seen on AZ-ANNIQUE-WEB
# and AMSERVER-TEST. Captures full command line + start time + working directory.
Write-Section '21. Integration runtime scan (running processes)'

Write-Block 'Processes matching integration patterns (name, path, or command line)' {
    $integrationRegex = '(?i)(nopintegration|backoffice|anqimagesync|brevocontacts?sync|anq[a-z]+sync|imagesync|amsync|annique|accountmate|wconnect|webconnection|runasyncrequest)'
    Get-WmiObject Win32_Process -ErrorAction SilentlyContinue |
        Where-Object {
            ($_.Name -match $integrationRegex) -or
            ($_.ExecutablePath -and $_.ExecutablePath -match $integrationRegex) -or
            ($_.CommandLine -and $_.CommandLine -match $integrationRegex) -or
            ($_.ExecutablePath -and $_.ExecutablePath -match '(?i)\\Apps\\|\\NopIntegration\\|\\Backoffice\\|\\Email2SMS\\|\\CompPLan\\|\\Eft\\')
        } |
        Select-Object @{n='PID';e={$_.ProcessId}},
                      @{n='Name';e={$_.Name}},
                      @{n='Exe';e={$_.ExecutablePath}},
                      @{n='CmdLine';e={if ($_.CommandLine -and $_.CommandLine.Length -gt 250) { $_.CommandLine.Substring(0,250) + '...' } else { $_.CommandLine }}},
                      @{n='Parent';e={$_.ParentProcessId}} |
        Format-Table -AutoSize -Wrap
}

Write-Block 'Known integration deployment folders - enumerate contents' {
    $deployFolders = @(
        'C:\NopIntegration', 'C:\NopIntegration\deploy', 'C:\NopIntegration\web',
        'C:\Backoffice', 'C:\Backoffice\deploy', 'C:\Backoffice\web',
        'C:\Apps', 'C:\Apps\BrevoContactSync', 'C:\Apps\ImageSync',
        'C:\Email2SMS', 'C:\Eft', 'C:\CompPLan',
        'D:\Apps', 'E:\Projects',
        'C:\WebConnectionProjects',
        'C:\AnqIntegrationAPI', 'C:\inetpub\wwwroot\AnqIntegrationAPI'
    )
    foreach ($folder in $deployFolders) {
        if (Test-Path $folder) {
            "=== $folder ==="
            Get-ChildItem $folder -ErrorAction SilentlyContinue |
                Sort-Object LastWriteTime -Descending |
                Select-Object -First 25 |
                Select-Object Mode, LastWriteTime, Length, Name |
                Format-Table -AutoSize | Out-String | ForEach-Object { $_ }
            ''
        }
    }
}

Write-Block 'Config / connection-string files inside integration folders' {
    # Find .ini, .json, .config files inside the integration folder tree
    # and report their path + size + last modified. Do NOT read contents here -
    # section 10 already reads specific known files; this is for discovery of
    # unknown config files.
    $searchRoots = @('C:\NopIntegration','C:\Backoffice','C:\Apps','C:\CompPLan','C:\Email2SMS','C:\Eft','E:\Projects')
    $allCfg = @()
    foreach ($root in $searchRoots) {
        if (Test-Path $root) {
            $allCfg += Get-ChildItem $root -Recurse -Include '*.ini','*.json','*.config','*.fpw','appsettings.*' -ErrorAction SilentlyContinue |
                Where-Object { -not $_.PSIsContainer }
        }
    }
    $allCfg | Sort-Object FullName | Select-Object -First 100 |
        Select-Object @{n='Path';e={$_.FullName}}, Length, LastWriteTime |
        Format-Table -AutoSize
}

# =============================================================================
# 22. INTEGRATION LOG TAILS (what syncs are happening RIGHT NOW)
# =============================================================================
# Find VFP/FoxPro and .NET integration logs in the usual folders; tail the most
# recent files. This is how we confirm which syncs are actually running live.
Write-Section '22. Integration log tails (live sync evidence)'

Write-Block 'Most recently-written *.log files in integration folders (top 20)' {
    $logRoots = @('C:\NopIntegration\deploy','C:\Backoffice\deploy','C:\Apps\BrevoContactSync','C:\Apps\ImageSync','C:\Email2SMS','C:\Eft','C:\CompPLan','E:\Projects')
    $allLogs = @()
    foreach ($root in $logRoots) {
        if (Test-Path $root) {
            $allLogs += Get-ChildItem $root -Recurse -Filter '*.log' -ErrorAction SilentlyContinue | Where-Object { -not $_.PSIsContainer }
        }
    }
    $allLogs | Sort-Object LastWriteTime -Descending | Select-Object -First 20 |
        Select-Object LastWriteTime, @{n='SizeKB';e={[math]::Round($_.Length/1KB,1)}}, FullName |
        Format-Table -AutoSize -Wrap
}

Write-Block 'Tail of the 5 most recent log files (last 15 lines each)' {
    $logRoots = @('C:\NopIntegration\deploy','C:\Backoffice\deploy','C:\Apps\BrevoContactSync','C:\Apps\ImageSync','C:\Email2SMS','C:\Eft','C:\CompPLan','E:\Projects')
    $allLogs = @()
    foreach ($root in $logRoots) {
        if (Test-Path $root) {
            $allLogs += Get-ChildItem $root -Recurse -Filter '*.log' -ErrorAction SilentlyContinue | Where-Object { -not $_.PSIsContainer -and $_.Length -gt 0 }
        }
    }
    $recent = $allLogs | Sort-Object LastWriteTime -Descending | Select-Object -First 5
    foreach ($log in $recent) {
        "=== $($log.FullName) (last modified $($log.LastWriteTime), $([math]::Round($log.Length/1KB,1)) KB) ==="
        Get-Content $log.FullName -ErrorAction SilentlyContinue | Select-Object -Last 15
        ''
    }
}

# =============================================================================
# END OF CONTENT — everything below is tamper-evidence
# =============================================================================
Write-Section 'End of report'
Write-Line "Completed: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"
Write-Line "Please send this file to Dieselbrook: developer@pvm.co.za"

# -----------------------------------------------------------------------------
# TAMPER-EVIDENCE SIGNATURE
# -----------------------------------------------------------------------------
# We use an explicit end-of-content marker that is INCLUDED in the hash scope.
# This avoids ambiguity about trailing whitespace / newlines.
#
# To verify:
#   (a) Read the file as raw bytes
#   (b) Find the line starting with '<<<DIESELBROOK-HASHED-CONTENT-ENDS-HERE>>>'
#       - the scope is: start of file through the end of that line INCLUDING
#         its line-ending bytes (CR and/or LF)
#   (c) Compute SHA-256 over exactly that byte range
#   (d) Compare to the "SHA-256" value in the signature block following the
#       end-of-content marker
# If the values match, the hashed range is byte-for-byte unchanged since
# generation.
# -----------------------------------------------------------------------------

function Get-Sha256OfFile($path) {
    $sha   = New-Object System.Security.Cryptography.SHA256Managed
    $bytes = [System.IO.File]::ReadAllBytes($path)
    $hash  = $sha.ComputeHash($bytes)
    return ([BitConverter]::ToString($hash)).Replace('-', '').ToLower()
}

# Write the end-of-content marker as part of the hashed range.
# Note: Out-File on PowerShell 2.0 / Windows defaults to CRLF line endings.
Write-Line ''
Write-Line '<<<DIESELBROOK-HASHED-CONTENT-ENDS-HERE>>>'

# Compute hash of the file as it now stands - this includes the end marker.
$contentHash = Get-Sha256OfFile $OutputFile

# Append the signature block AFTER the hash is computed.
# The signature block is NOT part of the hashed range.
Write-Line ''
Write-Line '===== DIESELBROOK TAMPER-EVIDENCE SIGNATURE ====='
Write-Line "Generated        : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"
Write-Line "Script version   : 1.0"
Write-Line "Host             : $env:COMPUTERNAME"
Write-Line "Generated by     : $env:USERDOMAIN\$env:USERNAME"
Write-Line "Algorithm        : SHA-256"
Write-Line "Scope            : all bytes from start of file through (and including) the line"
Write-Line "                   '<<<DIESELBROOK-HASHED-CONTENT-ENDS-HERE>>>' and its terminator"
Write-Line "SHA-256          : $contentHash"
Write-Line '================================================================='
Write-Line ''
Write-Line 'If any byte in the hashed range has been altered since generation,'
Write-Line 'recomputing SHA-256 over the same byte range will NOT match the value'
Write-Line 'above. The same hash is also written to a sidecar file (.sha256)'
Write-Line 'and printed to the PowerShell console at the end of the run.'

# Write the sidecar .sha256 file
$sidecarFile = "$OutputFile.sha256"
"$contentHash  $(Split-Path -Leaf $OutputFile)" | Out-File -FilePath $sidecarFile -Encoding ASCII

Write-Host ''
Write-Host '==============================================' -ForegroundColor Green
Write-Host '  Discovery complete.' -ForegroundColor Green
Write-Host "  Output file: $OutputFile" -ForegroundColor Yellow
Write-Host "  Hash file  : $sidecarFile" -ForegroundColor Yellow
Write-Host "  Size       : $([math]::Round((Get-Item $OutputFile).Length / 1KB, 1)) KB" -ForegroundColor Yellow
Write-Host ''
Write-Host '  TAMPER-EVIDENCE SHA-256:' -ForegroundColor Magenta
Write-Host "    $contentHash" -ForegroundColor White -BackgroundColor DarkMagenta
Write-Host ''
Write-Host '  Please send BOTH files (output + sidecar .sha256) to Dieselbrook.' -ForegroundColor Cyan
Write-Host '  If possible, also paste the SHA-256 value above into your email' -ForegroundColor Cyan
Write-Host '  body (separate channel) so we can cross-verify integrity.' -ForegroundColor Cyan
Write-Host ''
Write-Host '  Email: developer@pvm.co.za' -ForegroundColor Cyan
Write-Host '==============================================' -ForegroundColor Green
Write-Host ''
