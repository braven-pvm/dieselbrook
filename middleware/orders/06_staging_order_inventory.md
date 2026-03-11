# Staging Order Inventory

## Purpose

This document records the order-side databases and objects that are confirmed directly on staging and that matter to Dieselbrook's order replacement work.

It is meant to ground the order workstream in verified data, not inferred documentation alone.

## Confirmed Staging Databases In Scope

| Database | Role |
|---|---|
| `amanniquelive` | primary SA ERP order, customer, invoice, fulfillment, and campaign data |
| `NopIntegration` | middleware support database for reports, SSO, settings, logging, and marketing support tables |

## Confirmed `amanniquelive` Order Objects

| Object | Row count | Role in current order estate |
|---|---:|---|
| `sosord` | 63,842 | sales order header |
| `soship` | 63,377 | shipment header / fulfillment state |
| `soskit` | 5,909,681 | order line / kit detail |
| `sostrs` | 333,607 | sales order transaction detail |
| `arinvc` | 308,340 | invoice header and downstream finance anchor |
| `arcash` | 89,321 | cash receipt / payment posting support |
| `arcust` | 120,320 | customer / consultant master used during order import |
| `SOPortal` | 653,961 | downstream operational order portal and status surface |
| `be_waybill` | 339,774 | shipment tracking / waybill support |
| `changes` | 533,788 | change tracking / sync support |
| `soxitems` | 21,086 | exclusive-item / restricted SKU support |
| `Campaign` | 77 | campaign header data |
| `CampDetail` | 18,571 | campaign detail / pricing support |
| `wsSetting` | 2 | web sync settings / configuration support |

## Confirmed `NopIntegration` Support Objects

| Object | Row count | Why it matters |
|---|---:|---|
| `BrevoLog` | 124,726 | marketing and customer communication audit trail |
| `NopReports` | 52 | report metadata used by middleware/report endpoints |
| `ApiClients` | 8 | API client / access registration support |
| `NopSettings` | 19 | middleware setting surface |
| `NopSSO` | 10,260 | single-sign-on support data |
| `wwrequestlog` | 1,640,737 | West Wind request logging footprint |

## What This Confirms

- order replacement is not limited to `sosord` and `soskit`
- invoicing, payment posting, portal status, and tracking side effects are part of the real operational flow
- `NopIntegration` is not just a marketing side database; it also contains request logging, SSO support, settings, and report metadata tied to legacy middleware behavior

## Immediate Implications For Dieselbrook

- order parity testing must include `SOPortal`, `arinvc`, `arcash`, and `be_waybill`, not only order creation
- exclusive-item and campaign behavior must be validated against `soxitems`, `Campaign`, and `CampDetail`
- replacement of legacy middleware endpoints should account for the current `NopIntegration` support footprint, especially report metadata and request logging expectations

## Recommended Usage Rule

Use this inventory together with the order flow and business-rules documents when deciding what must be preserved in phase 1 versus moved behind cleaner middleware boundaries.