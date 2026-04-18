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
function Capture-Block($label, $block) {
    Write-Heading $label
    try {
        $result = & $block 2>&1 | Out-String
        if ([string]::IsNullOrWhiteSpace($result)) {
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
Write-Line "Script version : 1.0 (2026-04-18)"
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

Capture-Block 'Operating System' {
    Get-WmiObject Win32_OperatingSystem |
        Select-Object Caption, Version, BuildNumber, OSArchitecture, InstallDate, LastBootUpTime, FreePhysicalMemory, TotalVirtualMemorySize |
        Format-List
}

Capture-Block 'Computer and Memory' {
    Get-WmiObject Win32_ComputerSystem |
        Select-Object Manufacturer, Model, TotalPhysicalMemory, NumberOfProcessors, NumberOfLogicalProcessors, Domain, DnsHostName, DomainRole |
        Format-List
}

Capture-Block 'CPU' {
    Get-WmiObject Win32_Processor |
        Select-Object Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, L2CacheSize, SocketDesignation |
        Format-List
}

Capture-Block 'Disks (local drives)' {
    Get-WmiObject Win32_LogicalDisk -Filter 'DriveType=3' |
        Select-Object DeviceID, VolumeName, FileSystem,
            @{n='SizeGB';e={[math]::Round($_.Size/1GB,1)}},
            @{n='FreeGB';e={[math]::Round($_.FreeSpace/1GB,1)}},
            @{n='UsedPct';e={[math]::Round(100-($_.FreeSpace/$_.Size*100),1)}} |
        Format-Table -AutoSize
}

Capture-Block 'PowerShell version' {
    $PSVersionTable | Format-List
}

# =============================================================================
# 2. NETWORKING
# =============================================================================
Write-Section '2. Networking'

Capture-Block 'IP configuration' {
    Get-WmiObject Win32_NetworkAdapterConfiguration -Filter 'IPEnabled=True' |
        Select-Object Description, MACAddress, IPAddress, IPSubnet, DefaultIPGateway, DNSServerSearchOrder, DHCPEnabled, DNSDomainSuffixSearchOrder |
        Format-List
}

Capture-Block 'IPv4 routing table' { route print -4 }
Capture-Block 'DNS cache (recent)' { ipconfig /displaydns 2>&1 | Select-Object -First 80 }
Capture-Block 'Listening TCP ports' { netstat -ano -p TCP | Select-String 'LISTENING' }
Capture-Block 'Active outbound connections' { netstat -ano -p TCP | Select-String 'ESTABLISHED' }
Capture-Block 'Listening UDP ports (top 20)' { netstat -ano -p UDP | Select-Object -First 20 }
Capture-Block 'Windows Firewall profile state' { netsh advfirewall show allprofiles state }

# =============================================================================
# 3. USERS AND SESSIONS
# =============================================================================
Write-Section '3. Users and sessions'

Capture-Block 'Local Administrators group' { net localgroup Administrators }
Capture-Block 'Remote Desktop Users group' { net localgroup 'Remote Desktop Users' }
Capture-Block 'Local user accounts' {
    Get-WmiObject Win32_UserAccount -Filter 'LocalAccount=True' |
        Select-Object Name, FullName, Disabled, Lockout, PasswordChangeable, PasswordExpires |
        Format-Table -AutoSize
}
Capture-Block 'Current sessions (query user)' { query user 2>&1 }
Capture-Block 'Current sessions (query session)' { query session 2>&1 }

# =============================================================================
# 4. SERVICES AND SCHEDULED TASKS
# =============================================================================
Write-Section '4. Services and scheduled tasks'

Capture-Block 'Running services' {
    Get-WmiObject Win32_Service |
        Where-Object { $_.State -eq 'Running' } |
        Select-Object Name, DisplayName, StartMode, StartName, PathName |
        Sort-Object Name |
        Format-Table -AutoSize -Wrap
}

Capture-Block 'Services set to Auto start (regardless of current state)' {
    Get-WmiObject Win32_Service |
        Where-Object { $_.StartMode -eq 'Auto' } |
        Select-Object Name, DisplayName, State, StartName |
        Sort-Object Name |
        Format-Table -AutoSize -Wrap
}

Capture-Block 'Scheduled tasks (summary)' {
    schtasks /query /fo LIST /v 2>&1 |
        Select-String 'TaskName:|Task To Run:|Run As User:|Last Run Time:|Next Run Time:|Schedule Type:|Status:' |
        Out-String
}

# =============================================================================
# 5. INSTALLED SOFTWARE
# =============================================================================
Write-Section '5. Installed software'

Capture-Block 'Installed programs (Windows uninstall registry)' {
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
'@

Set-Content -Path $sqlFile -Value $sqlScript -Encoding ASCII

Write-Heading 'SQL discovery output'
try {
    & $sqlcmd -S localhost -E -i $sqlFile -y 0 -W -o $sqlOutFile 2>&1 | Out-Null
    if (Test-Path $sqlOutFile) {
        Get-Content $sqlOutFile | ForEach-Object { Write-Line $_ }
    } else {
        Write-Line '(sqlcmd produced no output file)'
    }
} catch {
    Write-Line "sqlcmd failed: $($_.Exception.Message)"
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

Capture-Block 'System DSNs (64-bit)' {
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

Capture-Block 'System DSNs (32-bit / WOW6432)' {
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
# END
# =============================================================================
Write-Section 'End of report'
Write-Line "Completed: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"
Write-Line "Please send this file to Dieselbrook: developer@pvm.co.za"

Write-Host ''
Write-Host '==============================================' -ForegroundColor Green
Write-Host '  Discovery complete.' -ForegroundColor Green
Write-Host "  Output file: $OutputFile" -ForegroundColor Yellow
Write-Host "  Size: $([math]::Round((Get-Item $OutputFile).Length / 1KB, 1)) KB" -ForegroundColor Yellow
Write-Host ''
Write-Host '  Please review the file for anything you are uncomfortable sharing,' -ForegroundColor Cyan
Write-Host '  then send it to Dieselbrook (developer@pvm.co.za).' -ForegroundColor Cyan
Write-Host '==============================================' -ForegroundColor Green
Write-Host ''
