# Staging MLM Inventory

## Purpose

This document records what was directly confirmed on the staging SQL server for the consultant / MLM / middleware support databases.

## Server Confirmed

- SQL endpoint used: `196.3.178.122,62111`
- SQL Server name reported by `@@SERVERNAME`: `AMSERVER-V9`

## Key Databases Confirmed On Staging

The following databases were confirmed present on the staging SQL instance:

- `amanniquelive`
- `amanniquenam`
- `compplanLive`
- `compplanNam`
- `compsys`
- `NopIntegration`

Other confirmed databases also present on the same server include:

- `Annique_Reports`
- `BackOffice`
- `BackOfficeNam`
- `amannique`
- `amwsys`

## `compplanLive` Confirmed Objects

Confirmed base tables on staging:

- `CTcomph`
- `CTcomp`
- `CTconsv`
- `CTRunData`
- `CTstatement`
- `CTstatementh`
- `deactivations`
- `CTdownline`
- `CTdownlineh`

Selected row counts captured on staging:

| Table | Row count |
|---|---:|
| `CTcomph` | 8,623,539 |
| `CTcomp` | 11,574 |
| `CTconsv` | 1,779,211 |
| `CTRunData` | 3,702,636 |
| `CTstatement` | 52,511 |
| `CTstatementh` | 4,481,557 |
| `deactivations` | 35,702 |
| `CTdownline` | 0 |
| `CTdownlineh` | 1,491,507 |

Interpretation:

- `compplanLive` is confirmed populated and operational on staging
- `CTdownline` appears to be a working/current table and may be empty outside run windows
- `CTdownlineh` holds substantial historical hierarchy data

## `compsys` Confirmed Objects

Confirmed on staging:

- table: `MAILMESSAGE`
- procedure: `sp_Sendmail`

Selected row count:

| Table | Row count |
|---|---:|
| `MAILMESSAGE` | 752,963 |

Interpretation:

- `compsys` is confirmed populated and active as a mail/support database

## `NopIntegration` Confirmed Objects

Confirmed on staging:

- table: `BrevoLog`

Selected row count:

| Table | Row count |
|---|---:|
| `BrevoLog` | 124,726 |

Interpretation:

- `NopIntegration` is present on the same staging SQL instance
- the Brevo campaign log is populated and available for inspection

## What This Changes

These findings remove the earlier uncertainty about whether the consultant/MLM support databases were only referenced by code or actually available in accessible staging infrastructure.

They are confirmed available on staging.