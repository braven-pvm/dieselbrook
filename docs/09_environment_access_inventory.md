# Environment Access Inventory

## Purpose

This document records what infrastructure, database, and file artifacts are currently accessible in the Dieselbrook workspace and how they are being used during discovery.

## Workspace Paths Available

Confirmed top-level workspace folders:

- `docs/`
- `database/`
- `middleware/`
- `NISource/`
- `NopCommerce - Annique/`
- `AnqIntegrationApiSource/`
- `tmp/`

## Local File Artifacts

### `docs/DB Structure/`

Local extracted structure files are available for the main documented SQL export, including:

- stored procedures
- tables
- columns
- triggers
- views
- indexes
- foreign keys

Important limitation:

- these local CSV extracts do not currently include full standalone table inventories for `compplanLive` or `compsys`
- they do contain many cross-database references to those databases from procedures, triggers, and views

### `database/`

Confirmed local artifacts:

- `database/AnniqueNOP.bak`
- `database/sqlmedia/`

Current limitation:

- no local backup or export for `compplanLive` or `compsys` has been identified in the workspace so far

## Confirmed SQL CLI Access

Confirmed available local SQL client:

- `sqlcmd`

This is sufficient for direct staging SQL inspection from the current environment.

## Confirmed Staging SQL Access

Direct CLI inspection confirmed access to the staging SQL server:

- endpoint: `196.3.178.122,62111`
- server name from `@@SERVERNAME`: `AMSERVER-V9`

### How access was verified

Example verification commands used:

```powershell
sqlcmd -S 196.3.178.122,62111 -U <user> -P <password> -Q "SELECT @@SERVERNAME"
sqlcmd -S 196.3.178.122,62111 -U <user> -P <password> -Q "SELECT name FROM sys.databases"
```

Credential handling note:

- legacy connection details are present in the source tree
- do not duplicate plaintext credentials into new documentation
- long-term delivery should move these into a proper secret store or secure environment configuration

## Key Databases Confirmed On Staging

Confirmed present on `AMSERVER-V9`:

- `amanniquelive`
- `amanniquenam`
- `compplanLive`
- `compplanNam`
- `compsys`
- `NopIntegration`
- `Annique_Reports`
- `BackOffice`
- `BackOfficeNam`

## Confirmed Database Roles

| Database | Role |
|---|---|
| `amanniquelive` | primary SA ERP data |
| `amanniquenam` | Namibia ERP mirror / sibling DB |
| `compplanLive` | MLM / consultant compensation and hierarchy data |
| `compsys` | mail queue and support-service database |
| `NopIntegration` | integration support database, including `BrevoLog` |

## Confirmed Staging Objects

### `compplanLive`

Confirmed present and populated:

- `CTcomph`
- `CTcomp`
- `CTconsv`
- `CTRunData`
- `CTstatement`
- `CTstatementh`
- `deactivations`
- `CTdownline`
- `CTdownlineh`

### `compsys`

Confirmed present:

- `MAILMESSAGE`
- `sp_Sendmail`

### `NopIntegration`

Confirmed present:

- `BrevoLog`
- `NopReports`
- `ApiClients`
- `NopSettings`
- `NopSSO`
- `wwrequestlog`

### `amanniquelive`

Confirmed present and populated for order operations:

- `sosord`
- `sostrs`
- `soskit`
- `soship`
- `arinvc`
- `arcash`
- `arcust`
- `SOPortal`
- `be_waybill`
- `changes`
- `soxitems`
- `Campaign`
- `CampDetail`
- `wsSetting`

## Access Conclusions

- we have direct staging SQL visibility into the consultant / MLM / support databases
- we do not currently have equivalent full local backup artifacts for those databases in the workspace
- for consultant and middleware replacement planning, staging SQL is now the authoritative source for validating `compplanLive`, `compsys`, and `NopIntegration`

## Recommended Usage Rule

Use sources in this order when validating legacy behavior:

1. direct staging SQL inspection
2. legacy source code in `NISource/`
3. local extracted structure files in `docs/DB Structure/`
4. older discovery markdown documents