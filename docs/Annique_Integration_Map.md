**ANNIQUE**

Database Integration Map

AccountMate ERP ↔ NopCommerce

*Prepared for Shopify Migration Planning*

March 2026

**CONFIDENTIAL**

# Table of Contents

# Executive Summary

The Annique database is a SQL Server-based AccountMate ERP system that integrates with a NopCommerce e-commerce store through a hybrid architecture combining API-based HTTP calls and direct linked server database writes.

This document maps every integration point between the two systems to support the planned migration from NopCommerce to Shopify. It covers the 12 NopCommerce sync procedures, the MLM/rebate commission engine, the order fulfilment pipeline, and the 3PL warehouse integration.

## Database Scale

|  |  |
| --- | --- |
| **Metric** | **Count** |
| Total tables | 913 |
| Total rows (all tables) | ~400 million |
| Stored procedures | 968 |
| Views | 1,743 |
| Triggers | 154 |
| NOP sync procedures | 12 |
| MLM/rebate procedures | 6 |

## Integration Architecture

The integration uses two mechanisms:

1. **API-based syncing** via HTTPS calls to nopintegration.annique.com (9 procedures). Uses sp\_ws\_HTTP for HTTP GET/POST with Basic authentication. Configuration stored in wsSetting table. Responses logged in Brevolog table.
2. **Direct database writes** via linked server [NOP].annique.dbo (3 procedures). Used for exclusive items, images, and real-time triggers. Linked server connection: [NOP] pointing to the NopCommerce SQL Server database.

A separate Shop API endpoint exists at shopapi.annique.com for staff/consultant sync.

# NopCommerce Sync Procedures

The following 12 procedures handle all data synchronisation between AccountMate and NopCommerce. Nine use the API-based approach; three write directly to the NOP database via linked server.

|  |  |  |  |  |
| --- | --- | --- | --- | --- |
| **Procedure** | **Direction** | **Priority** | **Mechanism** | **Description** |
| **sp\_NOP\_syncOrders** | NOP → AccountMate | **CRITICAL** | API (HTTP POST) | Pulls web orders from NopCommerce into AccountMate sales order module. Creates invoices in arinvc and line items in aritrs. Most recently modified (Jan 2026). |
| **sp\_NOP\_syncItemAll** | AccountMate → NOP | HIGH | API (HTTP GET) | Full product catalog push to NopCommerce. Reads icitem (product master), icuom (units), icprcd (pricing). Manual trigger for bulk refresh. |
| **sp\_NOP\_syncItemChanges** | AccountMate → NOP | HIGH | API (HTTP GET) | Incremental product sync triggered by changes table. Only pushes items with modified fields (price, description, status). Prevents unnecessary full catalog syncs. |
| **sp\_NOP\_syncItemAvailability** | AccountMate → NOP | HIGH | API (HTTP GET) | Pushes real-time stock levels from icqoh (quantity on hand) to NopCommerce Product.StockQuantity. Critical for web store accuracy. |
| **sp\_NOP\_syncOrderStatus** | AccountMate → NOP | HIGH | API (HTTP GET) | Pushes fulfilment/shipping status from soportal back to NopCommerce. Updates order status (pending → picking → shipped) for customer visibility. |
| **sp\_NOP\_syncSoxitems** | AccountMate ↔ NOP | MEDIUM | Direct DB (linked server) | Syncs consultant-exclusive products. Writes directly to [NOP].annique.dbo.ANQ\_ExclusiveItems. Triggered by insert/update triggers on soxitems table. |
| **sp\_ws\_UpdateImageNOP** | AccountMate → NOP | MEDIUM | Direct DB | Syncs product images via iciimgUpdateNOP staging table. Calls nop.annique.dbo.ANQ\_SyncImage for each pending image, then marks as updated. |
| **sp\_NOP\_DiscountINS** | AccountMate → NOP | MEDIUM | Local staging | Inserts discount rules into NOP\_Discount table. Manages percentage vs fixed amount discounts with date-based validity windows. |
| **sp\_NOP\_OfferINS** | AccountMate → NOP | MEDIUM | Local staging | Inserts promotional offer rules into NOP\_Offers. Links discounts to quantity/value thresholds. |
| **sp\_NOP\_OfferListINS** | AccountMate → NOP | MEDIUM | Local staging | Manages which products are included/excluded from offers via NOP\_OfferList (ListType I=Include, E=Exclude). |
| **NOP\_UpdateExclusiveItemCode** | AccountMate → NOP | MEDIUM | Direct | Updates exclusive product SKUs in SoxItems. Only applies to unpurchased, future-dated items. Created Sept 2025. |
| **sp\_NOP\_syncStaff** | AccountMate → Shop API | MEDIUM | API (HTTP GET) | Syncs consultant/staff profiles to separate Shop API at shopapi.annique.com (different endpoint from main NOP integration). |

# Detailed Sync Flows

## Order Sync (NopCommerce → AccountMate)

**sp\_NOP\_syncOrders is the most critical integration point. It pulls orders placed on the NopCommerce web store into AccountMate for fulfilment and accounting.**

Endpoint: https://nopintegration.annique.com/syncorders.api

Mechanism: HTTP POST via sp\_ws\_HTTP with Basic authentication.

Source tables: [NOP].annique.dbo.Orders, [NOP].annique.dbo.OrderItems

Destination tables: arinvc (AR invoices), aritrs (AR invoice line items)

The integration service receives the API call, reads pending NOP orders, and creates corresponding AccountMate invoice records. This is the only inbound sync (NOP to AccountMate); all other procedures push data outbound.

## Product Catalog Sync (AccountMate → NopCommerce)

Three procedures handle product sync at different granularities:

* **sp\_NOP\_syncItemAll:** Full catalog refresh. Reads from icitem (product master), icuom (units of measure), icprcd (pricing). Heavy operation, run manually.
* **sp\_NOP\_syncItemChanges:** Incremental sync. Uses the changes table (531K rows) to detect field-level modifications. Only syncs items where cfieldname=cstatus and the value changed. Efficient for daily operations.
* **sp\_NOP\_syncItemAvailability:** Stock level sync. Pushes icqoh (quantity on hand) to NopCommerce Product.StockQuantity. Critical for preventing overselling on the web store.

## Exclusive Items Sync (Bidirectional)

sp\_NOP\_syncSoxitems is unique in that it uses direct database writes via the linked server rather than the API. It manages consultant-exclusive products that are only available to specific Annique consultants.

It writes directly to [NOP].annique.dbo.ANQ\_ExclusiveItems, checking that both the Product and Customer exist in NOP before inserting. Triggered automatically by insert/update triggers on the soxitems table (tr\_soxitems\_insert, tr\_soxitems\_update).

Key fields: CustomerID (mapped from ccustno via Customer.UserName), ProductID (mapped from citemno via Product.Sku), quantity limits, date ranges, and active status.

## Image Sync

sp\_ws\_UpdateImageNOP processes images queued in the iciimgUpdateNOP staging table (7,744 rows). For each image where dUpdated IS NULL, it calls nop.annique.dbo.ANQ\_SyncImage and marks the record as processed.

## Discounts and Offers

Three procedures manage promotional content: sp\_NOP\_DiscountINS (discount rules with percentage/fixed amounts and date windows), sp\_NOP\_OfferINS (offer rules with quantity/value thresholds), and sp\_NOP\_OfferListINS (product inclusion/exclusion lists per offer). These write to local NOP\_Discount, NOP\_Offers, and NOP\_OfferList staging tables.

## Order Status Feedback

sp\_NOP\_syncOrderStatus pushes fulfilment status from AccountMate back to NopCommerce. It reads from soportal (Sales Order Portal) and updates the NOP order status. The status flow is: pending → picking (via AutoPick) → shipped → synced back to NOP for customer visibility.

# Configuration and Control

## wsSetting Table

All API-based sync procedures read their configuration from the wsSetting table. This is the central control point for the integration.

|  |  |  |
| --- | --- | --- |
| **Setting** | **Value** | **Purpose** |
| ws.url | https://nopintegration.annique.com/ | Base URL for all API calls |
| ws.timeout | 30 | HTTP timeout in seconds |
| ws.retries | 3 | Retry attempts on failure |
| ws.auth | BASIC (hardcoded in sp\_ws\_HTTP) | Authentication method |

*Safety mechanism: If ws.url is NULL, all API syncs are disabled. This allows emergency cutover without code changes.*

## Change Detection (changes table)

The changes table (531,698 rows) provides field-level change tracking for selective synchronisation. Structure:

|  |  |
| --- | --- |
| **Column** | **Purpose** |
| ctablename | Source table (e.g., ICITEM, ARCUST) |
| cprimarykey | Record identifier (item number, customer number) |
| cfieldname | Column name that changed (e.g., cstatus, nprice) |
| coldvalue | Previous value |
| cnewvalue | New value |
| dchanged | Timestamp of change |
| ccreatedby | User who made the change |

## Linked Server

The NOP linked server connects directly to the NopCommerce database. Referenced as [NOP].annique.dbo in stored procedures. Used by sp\_NOP\_syncSoxitems, sp\_ws\_UpdateImageNOP, and exclusive item triggers.

Key NOP tables accessed: Customer, Product, ANQ\_ExclusiveItems, ANQ\_Booking, Orders, OrderItems, Discount, ANQ\_Lookups.

## Brevo Email Campaign Logging

NopIntegration..Brevolog is a Brevo (formerly Sendinblue) email marketing campaign log. It tracks consultant email list actions (CampaignID, ddate, ccustno, cEmail, Action) — for example, when a consultant is added to or removed from a mailing list. This is NOT an API audit trail.

*Important gap: No dedicated API call audit logging table was identified in the current architecture. The sp\_ws\_HTTP procedure makes HTTP calls but does not appear to log request/response data. This should be addressed in the Shopify migration by implementing proper API call logging.*

# MLM / Rebate Commission System

The MLM system is deeply integrated into the AccountMate AR module. It manages consultant hierarchies, monthly commission calculations, and rebate invoice generation.

## Commission Procedures

|  |  |  |
| --- | --- | --- |
| **Procedure** | **Priority** | **Description** |
| **sp\_ct\_Rebates** | **CRITICAL** | Monthly commission invoice generation. Reads from compplanlive..ctcomph, creates rebate invoices (type R) in arinvc/aritrs. Calculates base rebate + 15% VAT. Creates credit memos for upline sponsors and debit memos for contributing downline consultants. Marks processed with cCompStatus=P to prevent duplicates. |
| **sp\_ct\_downlinebuild** | HIGH | Builds monthly consultant hierarchy snapshot. Truncates CTDownline, rebuilds from fn\_get\_downlineCT recursive function. Archives to CTDownlineh on last day of month. Tracks generation level, sponsor, rank (iTitle), and active status. |
| **Camp\_NewMonth** | HIGH | Campaign month-end transition. Marks current campaign as Historical (H), activates next campaign as Current (C). Syncs to NAM partner system via sp\_Camp\_SynctoNam. |
| **vsp\_mlm\_getnewdocno** | MEDIUM | Sequential document number generation for MLM transactions. Uses arsyst.cmlmrcpt counter with transaction protection to prevent collision. |
| **vsp\_mlm\_getnewmlminvc** | MEDIUM | Sequential MLM invoice number generation. Same algorithm as getnewdocno, uses arsyst.cmlmcinvno counter. |
| **vsp\_rpt\_rbtmlm** | LOW | MLM rebate reporting. Generates rbtmlm temp table with consultant sales, rebates, sponsor hierarchy, and contact details for commission statements. |

## Commission Calculation Formula

sp\_ct\_Rebates runs monthly (typically first week of the following month) and generates three types of records:

1. **Rebate Invoice (Type R):** Credit to the consultant. Base rebate = SUM of ctcomph.namount for the month. Tax = Base × 15% VAT. Total = Base + Tax. Recorded as negative balance in arinvc (credit to consultant).
2. **Credit Memo (for downline earnings):** When a downline consultant generates sales, the upline sponsor receives a credit. Linked via cmlmlink field. lautoreb=1 (auto-generated).
3. **Debit Memo (contributor obligation):** The contributing downline consultant receives a debit record. This balances the credit given to the sponsor.

Source data comes from compplanlive..ctcomph (external database). Once processed, records are marked cCompStatus=P to prevent duplicate generation.

## Consultant Hierarchy

sp\_ct\_downlinebuild rebuilds the consultant tree monthly using the recursive function fn\_get\_downlineCT. The hierarchy is stored in CTDownline (working table) and archived monthly in CTDownlineh. Key fields: ccustno (consultant), ilevel (generation depth), csponsor (direct sponsor), iTitle (rank).

Related tables: mlmcust (3M rows, MLM customer master), CustSPNLVL (145K rows, customer-sponsor-level mapping), Level1/Level2 (tier membership), MoDownlinerV1 (1M rows, monthly downline snapshots).

*Note: Commission distribution depth (Level 1, 2, 3+) is inferred from Level1/Level2 table structure. The exact formula is in sp\_ct\_Rebates (13,126 chars). Verify with DBA before migration.*

## Campaign System

Camp\_NewMonth handles month-end campaign transitions. Status codes: C=Current/Active, H=Historical, D=Draft, P=Pending. On transition, the current campaign is archived (H) and the next campaign is activated (C), then synced to the NAM partner system.

# Order Fulfilment Pipeline

## Order Flow

Web orders follow this path through the system:

1. Customer places order on NopCommerce web store
2. sp\_NOP\_syncOrders pulls order into AccountMate (arinvc + aritrs)
3. Order appears in soportal (Sales Order Portal) with cstatus = pending
4. AutoPick batches orders (waits for 15 min or 10+ orders), sets cstatus = P (picking)
5. Warehouse processes pick list, ships order, sets cstatus = S (shipped)
6. sp\_NOP\_syncOrderStatus pushes shipped status back to NopCommerce
7. Customer sees updated order status on web store

## 3PL Warehouse Integration

The fw\_\* tables manage third-party logistics integration:

|  |  |  |
| --- | --- | --- |
| **Table** | **Rows** | **Purpose** |
| fw\_consignment | 674,738 | Shipments sent to 3PL warehouse |
| fw\_items | 681,759 | Items held in 3PL inventory |
| fw\_Hubs | 23,629 | Warehouse hub/location definitions |
| fw\_labels | 26,241 | Shipping labels generated |
| fw\_manifest | 6,861 | Shipment manifests |
| be\_waybill | 334,253 | Waybill/tracking numbers |
| SkyTrack | 138,096 | Carrier tracking (Sky Express) |

## Payment Reconciliation

Over\_App: Automatically writes off small payment variances (between -5 and +5) by creating journal entries. Debits account 662000-2600 (Discount/Variance) and credits 200100-2600 (AR Control).

Return\_App: Processes credit memos and returns with small balances. Creates offsetting journal entries for return types R (Return), N (Note), and C (Credit).

# Data Field Mapping: NopCommerce ↔ AccountMate

|  |  |  |  |  |
| --- | --- | --- | --- | --- |
| **NOP Table** | **NOP Column** | **AM Table** | **AM Column** | **Purpose** |
| Customer | id | arcust | cCustno | Consultant ID |
| Customer | username | arcust | cCustno | Username = Cust # |
| Product | id | icitem | id (custom) | Product ID |
| Product | sku | icitem | citemno | SKU mapping |
| Product | name | icitem | cdescript | Product name |
| Product | StockQty | icqoh | nqtyoh | Inventory level |
| Orders | id | arinvc | coriginvno | Order number |
| OrderItems | ProductId | aritrs | citemno | Line item SKU |
| OrderItems | Quantity | aritrs | nshipqty | Qty shipped |
| ANQ\_Exclusive | CustomerID | soxitems | ccustno | Consultant |
| ANQ\_Exclusive | ProductID | soxitems | citemno | Item number |

# Shopify Migration Implications

Migrating from NopCommerce to Shopify requires replacing all 12 sync procedures with Shopify API equivalents while preserving the AccountMate ERP as the backend system of record.

## What Must Be Replaced

* **Order sync:** Replace sp\_NOP\_syncOrders with Shopify Order webhooks pushing into AccountMate. This is the most critical replacement.
* **Product sync:** Replace sp\_NOP\_syncItemAll/Changes/Availability with Shopify Product API and Inventory API calls. The changes table mechanism can be reused to detect what to sync.
* **Order status:** Replace sp\_NOP\_syncOrderStatus with Shopify Fulfillment API updates.
* **Exclusive items:** Replace direct DB writes with Shopify customer metafields and product access controls (Shopify B2B or custom app with customer tags).
* **Images:** Replace sp\_ws\_UpdateImageNOP with Shopify Product Image API.
* **Discounts/Offers:** Replace NOP\_Discount/Offer tables with Shopify Discount API and automatic discounts.

## What Stays the Same

* AccountMate ERP remains the system of record for GL, AR, AP, inventory, and MLM
* MLM/rebate engine (sp\_ct\_Rebates, sp\_ct\_downlinebuild) stays in SQL Server
* 3PL warehouse integration (fw\_\* tables) continues as-is
* Campaign system (Camp\_NewMonth) stays in AccountMate
* Change detection (changes table) can be reused to trigger Shopify syncs

## Recommended Shopify Architecture

Build a middleware integration layer (custom Shopify app or integration service) that replaces the current nopintegration.annique.com endpoint. This middleware would:

1. Listen for Shopify webhooks (orders, customer updates)
2. Push data into AccountMate via the existing stored procedure layer
3. Poll AccountMate for changes (using changes table) and push to Shopify APIs
4. Handle consultant-exclusive products via Shopify customer tags or metafields
5. Manage MLM consultant identification for pricing tiers

The existing wsSetting table can be repurposed to point to the new middleware, allowing a clean cutover by simply updating the ws.url value.

# Verification Notes

This document was generated from automated analysis of 913 tables, 968 stored procedures, 1,743 views, and 154 triggers. The following items were verified against source code:

## Confirmed

* AutoPick batching rules (15 min / 10 orders) — verified in SP code
* GL account numbers (662000-2600, 200100-2600) — verified in Over\_App/Return\_App
* NOP linked server [NOP].annique.dbo — 21 references confirmed in code
* wsSetting configuration and NULL safety mechanism — 15 references confirmed
* sp\_ws\_HTTP uses MSXML2.ServerXMLHTTP with Basic auth — verified in SP code
* Order status flow (pending → P → S) — verified in AutoPick SP

## Assumptions to Verify

* API endpoints (nopintegration.annique.com) are hardcoded in SPs but may be overridden via wsSetting at runtime
* Changes table column structure inferred from SP usage, not verified from DDL. Run: EXEC sp\_help 'changes'
* MLM commission depth (multi-level) inferred from Level1/Level2 tables. Exact formula in sp\_ct\_Rebates needs full review
* Active consultant filter (cstatus='A') for rebates assumed but not confirmed in sp\_ct\_Rebates

## Corrected Errors

* **Brevolog: Originally described as API audit trail. Corrected: it is a Brevo email marketing campaign log (NopIntegration..Brevolog). No API audit trail exists — this is an architecture gap.**

## Recommended Pre-Migration Verification

1. Run EXEC sp\_help 'changes' to confirm change tracking table schema
2. Review full sp\_ct\_Rebates code (13K chars) for exact commission distribution formula
3. Get DBA to run SQL Agent Jobs query (requires msdb permissions) to identify scheduled sync jobs
4. Confirm nopintegration.annique.com and shopapi.annique.com are active production endpoints
5. Implement API call logging before Shopify migration to close the audit trail gap