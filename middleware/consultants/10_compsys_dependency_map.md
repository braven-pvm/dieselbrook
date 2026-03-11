# Compsys Dependency Map

## Purpose

This document traces what currently touches `compsys`, especially `compsys.dbo.mailmessage`, and what that means for Dieselbrook scope.

## Bottom Line

`compsys` is not the MLM ledger database.

It is a same-instance support database used for queued email and alert delivery.

Because `NISource` uses it directly for consultant-related communications, the `compsys` integration surface is in scope for Dieselbrook replacement where FoxPro currently owns the behavior.

As of 2026-03-11, `compsys` is also confirmed directly on staging rather than only inferred from code and trigger references.

## What `compsys` Appears To Do

The strongest current evidence indicates:

- `compsys.dbo.mailmessage` is an email queue table
- `Compsys.[dbo].[sp_Sendmail]` is a mail dispatch procedure or dispatch trigger point
- ERP triggers and stored procedures use it for operational alerts
- FoxPro code also uses it for middleware-owned communications such as consultant welcome mail

## Staging Confirmation

Direct staging SQL inspection confirmed:

- SQL Server identity: `AMSERVER-V9`
- database present: `compsys`
- table present: `MAILMESSAGE`
- procedure present: `sp_Sendmail`
- `MAILMESSAGE` row count on staging: 752,963

This confirms that `compsys` is a real and populated same-instance support database on the staging server.

## Confirmed Touchpoints

### 1. FoxPro mail queue model

`NISource/amdata.prg` defines a `MailMessage` class with:

- `cfilename = "compsys.dbo.mailmessage"`

This is a direct application-code dependency on `compsys`.

### 2. Consultant welcome mail

`NISource/syncconsultant.prg` creates `MailMessage` and queues onboarding email content.

This means at least some consultant communications are currently routed through `compsys`, not only through Brevo or storefront email systems.

### 3. ERP-triggered operational alerts

Database trigger extracts show direct inserts into:

- `compsys.[dbo].[MAILMESSAGE]`

The discovery notes already tie one confirmed use case to:

- bank account change alerts

So `compsys` is part of the current ERP-side operational alerting path.

### 4. Mail dispatch procedure references

Stored procedure and trigger extracts show repeated execution of:

- `Compsys.[dbo].[sp_Sendmail]`

This suggests `compsys` is not only a passive queue table; it likely also contains the dispatch procedure or mail delivery orchestration.

## Systems That Touch `compsys`

| System / layer | Touch type | Evidence | What it does |
|---|---|---|---|
| FoxPro middleware base classes | direct write model | `NISource/amdata.prg` | generic queued email model via `MailMessage` |
| FoxPro consultant sync | direct queue usage | `NISource/syncconsultant.prg` | consultant welcome / onboarding email queueing |
| ERP triggers | insert + execute | trigger extracts | operational alerts such as bank-change notifications |
| ERP stored procedures | insert + execute | SP extracts | portal or operations alert queueing and dispatch |
| NopCommerce custom plugin | no direct evidence found | no direct `compsys` references found | not a direct caller |

## Scope Consequence For Dieselbrook

When replacing `NISource`, Dieselbrook must decide per communication flow whether to:

- continue queueing into `compsys`
- invoke `sp_Sendmail` where still required
- or replace middleware-owned communication flows with Dieselbrook-owned infrastructure

## Recommended Boundary

### Keep ERP-side in phase 1

- pure ERP alerts raised by SQL triggers and procedures
- bank-account-change alerting or similar finance-side notifications

### Replace in Dieselbrook phase 1

- consultant welcome or onboarding email flows currently owned by FoxPro
- any middleware-owned operational notifications currently using `MailMessage`
- any application-level dependency on writing directly to `compsys.dbo.mailmessage`

## Recommended Target Design

For middleware-owned communication flows:

- create a Dieselbrook-owned communication adapter
- avoid hard-coding new application behavior directly to `compsys` unless required for parity
- isolate any retained `compsys` dependency behind a clear messaging gateway interface

For ERP-owned operational alerts:

- leave the SQL-triggered `compsys` behavior intact in phase 1 unless explicitly replacing the ERP alerting model

## Practical Interpretation

`compplanlive` and `compsys` should be treated differently:

- `compplanlive` is a business-data and MLM-logic dependency
- `compsys` is a support-service and communication dependency

Both matter for `NISource` replacement, but they do not carry the same architectural weight.