# Annique ERP Integration Technical Summary

## Executive Overview

The Annique ERP system integrates with NopCommerce e-commerce platform through a multi-layered architecture combining:
1. **API-based syncing** via HTTPS calls to `nopintegration.annique.com`
2. **Direct database writes** to the linked NopCommerce database at `[NOP].annique.dbo`
3. **Local staging tables** (wsSetting, changes, ANQ_ExclusiveItems)
4. **MLM/Rebate system** with complex consultant hierarchy and commission calculations
5. **Order fulfillment** through portal integration (soportal, sosord tables)

---

## Part 1: NopCommerce Integration

### 1.1 Sync Procedures Overview

#### `sp_NOP_syncOrders`
**Purpose**: Syncs new orders from NopCommerce web store into AccountMate ERP

**Data Flow**:
- **Direction**: NopCommerce → AccountMate
- **Mechanism**: API-based (HTTP POST)
- **Endpoint**: `https://nopintegration.annique.com/syncorders.api`
- **Sync Trigger**: Manual execution or scheduled job via SQL Agent
- **Method**: Calls `sp_ws_HTTP` to make HTTPS request to integration service

**Key Details**:
- Fetches wsSetting table for base URL configuration
- Uses HTTP basic authentication (hardcoded in sp_ws_HTTP)
- Integration service parses NOP orders and writes to AccountMate database
- No direct database connection used (pure API call)

**Related Tables**:
- Source: [NOP].annique.dbo.Orders, [NOP].annique.dbo.OrderItems
- Destination: arinvc (AR Invoices), aritrs (AR Invoice Line Items)

---

#### `sp_NOP_syncItemAvailability`
**Purpose**: Syncs product inventory levels from AccountMate to NopCommerce

**Data Flow**:
- **Direction**: AccountMate → NopCommerce
- **Mechanism**: API-based (HTTP GET with query parameters)
- **Endpoint**: `https://nopintegration.annique.com/syncproducts.api?type=availability`
- **Sync Trigger**: Manual or scheduled
- **Key Business Rule**: Synchronizes stock quantities to control web availability

**Related Tables**:
- Source: icqoh (Item Quantity On Hand)
- Destination: [NOP].annique.dbo.Product.StockQuantity

---

#### `sp_NOP_syncItemChanges`
**Purpose**: Syncs product changes (price, description, etc.) based on change tracking

**Data Flow**:
- **Direction**: AccountMate → NopCommerce
- **Mechanism**: API-based HTTP GET
- **Endpoint**: `https://nopintegration.annique.com/syncproducts.api?type=changes`
- **Change Detection**: Uses `changes` table to track modified fields
- **Sync Trigger**: Manual or scheduled

**Business Rules**:
- Monitors for field changes in icitem (Item Master)
- Tracks which fields changed (cfieldname='cstatus', etc.)
- Only syncs products with status 'I' (Inactive) or changed status

**Related Tables**:
- Source: icitem (Item Master), changes (Change Tracking Log)
- Destination: [NOP].annique.dbo.Product

---

#### `sp_NOP_syncItemAll`
**Purpose**: Full product catalog refresh to NopCommerce

**Data Flow**:
- **Direction**: AccountMate → NopCommerce (bulk)
- **Mechanism**: API-based HTTP GET
- **Endpoint**: `https://nopintegration.annique.com/syncproducts.api?type=all&instancing=single`
- **Sync Trigger**: Manual execution (heavy operation)
- **Scope**: Complete product catalog including all properties

**Related Tables**:
- Source: icitem, icuom, icprcd
- Destination: [NOP].annique.dbo.Product (complete refresh)

---

#### `sp_NOP_syncOrderStatus`
**Purpose**: Syncs fulfillment/shipment status back to NopCommerce orders

**Data Flow**:
- **Direction**: AccountMate → NopCommerce
- **Mechanism**: API-based HTTP GET
- **Endpoint**: `https://nopintegration.annique.com/syncorderstatus.api?instancing=single`
- **Sync Trigger**: Manual or scheduled (typically after fulfillment)

**Business Rules**:
- Pulls fulfillment status from soportal table
- Updates order status in NOP based on shipment state
- Critical for customer visibility of order progress

**Related Tables**:
- Source: soportal (Sales Order Portal), sosord (Sales Orders), soship (Shipments)
- Destination: [NOP].annique.dbo.Orders.OrderStatus

---

#### `sp_NOP_syncSoxitems`
**Purpose**: Syncs exclusive items/consultant-exclusive products to NopCommerce

**Data Flow**:
- **Direction**: AccountMate ↔ NopCommerce (bidirectional)
- **Mechanism**: Direct database writes to linked NOP server
- **Trigger**: Can be called manually or via triggers when soxitems updated
- **Key Feature**: Handles consultant-exclusive product registration

**Implementation Details**:
```sql
-- Reads from:
- soxitems table (id, ccustno, citemno, nqtylimit, nQtypurchased, dfrom, dto, lactive, etc.)

-- Writes directly to:
- [NOP].annique.dbo.ANQ_ExclusiveItems (product exclusivity tracking)
- [NOP].annique.dbo.Product (marks as exclusive)
- [NOP].annique.dbo.Customer (consultant profile)

-- Key Logic:
1. Check if exclusive item exists in NOP
2. If NOT EXISTS: INSERT new exclusive item (requires valid Product and Customer IDs)
3. If EXISTS: UPDATE exclusivity details (quantities, dates, active status)
4. Handles iStarter flag (set to 1 if triggered from TRIGGER, 0 otherwise)
5. Sets iforce flag to control availability window
```

**Business Rules**:
- Only adds exclusive item if both Product and Customer exist in NOP
- Updates quantity limits and date ranges
- Deletes exclusive item if product no longer valid (ProductID=0)
- Consultant identified by username (ccustno -> Customer.UserName)

**Trigger Behavior**:
- When called with @cUser='TRIGGER', marks as starter (@iStarter=1)
- Manual calls (@cUser not 'TRIGGER') keep iStarter based on existing value

**Related Tables**:
- Source: soxitems (SOX Items - Consultant Exclusive)
- Destination: [NOP].annique.dbo.ANQ_ExclusiveItems, [NOP].annique.dbo.Product

---

#### `sp_NOP_syncStaff`
**Purpose**: Syncs consultant/staff profiles to shop API

**Data Flow**:
- **Direction**: AccountMate → Shop API
- **Mechanism**: API-based HTTP GET
- **Endpoint**: `https://shopapi.annique.com/syncstaff.api`
- **Note**: Different endpoint than main nopintegration.annique.com

**Related Tables**:
- Source: arcust (AR Customers/Consultants), artsal (AR Salesperson)
- Destination: [Shop API].Staff/Consultants

---

### 1.2 Supporting Procedures

#### `NOP_UpdateExclusiveItemCode`
**Purpose**: Updates product SKU for exclusive items without impacting old SKUs

**Logic**:
- Updates SoxItems table with new product code
- Only applies to items not yet purchased (nQtyPurchased=0)
- Only applies to future-dated items (dto >= GETDATE())
- Important for campaign refreshes

#### `sp_ws_UpdateImageNOP`
**Purpose**: Syncs product images to NopCommerce

**Logic**:
- Iterates through iciimgUpdateNOP table (image update queue)
- For each pending image (dUpdated IS NULL):
  - Calls external procedure: `nop.annique.dbo.ANQ_SyncImage`
  - Marks image as updated (SET dUpdated=GETDATE())

---

### 1.3 Discount and Offer Insertion

#### `sp_NOP_DiscountINS`
**Purpose**: Inserts new discount rules into NOP_Discount tracking table

**Data Structure**:
- ANQ_ID: Maps to Annique source
- DiscountTypeId: Type of discount (coupon, category, product)
- UsePercentage: Flag for percentage vs. fixed amount
- DiscountPercentage/Amount: Discount value
- StartDateUtc/EndDateUtc: Validity window
- IsActive: Controls discount activation

#### `sp_NOP_OfferINS`
**Purpose**: Inserts discount offers (rules) into NOP_Offers table

**Links**: Connects DiscountID to specific offer rules with:
- MinQty/MaxQty: Quantity thresholds
- MinValue/MaxValue: Price thresholds
- RuleType: How offer applies

#### `sp_NOP_OfferListINS`
**Purpose**: Inserts items included in an offer into NOP_OfferList

**Mapping**:
- OfferID → Offer record
- ProductID → NOP Product ID
- citemno → Annique SKU
- ListType: 'I' (Include) or 'E' (Exclude)

---

### 1.4 Configuration & Settings

#### `wsSetting` Table
**Purpose**: Centralized configuration for web service integration

**Key Settings**:
| Setting | Example Value | Purpose |
|---------|---------------|---------|
| ws.url | https://nopintegration.annique.com/ | Base URL for API calls |
| ws.auth | BASIC 0123456789ABCDEF | Authentication header |
| ws.timeout | 30 | Request timeout in seconds |

**Usage in Procedures**:
```sql
SELECT @cUrl=LTRIM(RTRIM(Value)) FROM wsSetting WHERE Name='ws.url'
IF @cUrl IS NULL RETURN -- Disable sync if not configured
```

**Critical Note**: If wsSetting.ws.url is NULL, ALL API-based syncs are disabled as safety measure.

---

### 1.5 Change Tracking Mechanism

#### `changes` Table
**Purpose**: Tracks field-level changes in key master tables

**Structure**:
```
ctablename: Source table (e.g., 'ICITEM')
cprimarykey: Record identifier (e.g., item number)
cfieldname: Field changed (e.g., 'cstatus', 'nprice')
coldvalue: Previous value
cnewvalue: New value
dchanged: Timestamp of change
ccreatedby: User who made change
```

**Usage**:
- `sp_NOP_syncItemChanges` filters: `WHERE cfieldname='cstatus' AND cnewvalue='I'`
- Allows selective sync of only changed items
- Prevents unnecessary full catalog syncs
- Supports change history audit trail

---

### 1.6 NopCommerce Database Structure

**Linked Server Reference**: `[NOP].annique.dbo`

**Key Tables in NOP**:
| Table | Purpose |
|-------|---------|
| Customer | Consultant/customer profiles (username=ccustno) |
| Product | Product catalog (sku=citemno) |
| ANQ_ExclusiveItems | Consultant-exclusive products |
| Orders | Web orders from store |
| OrderItems | Line items in orders |
| ANQ_Booking | Event/campaign bookings |
| Discount, Offer, OfferList | Discount management |
| ANQ_Lookups | Reference data (Bank names, etc.) |

**Data Relationships**:
```
Customer (id, username=ccustno)
    ├── ANQ_ExclusiveItems (CustomerID, ProductID)
    │   └── Product (id, sku=citemno)
    └── Orders (CustomerId)
        └── OrderItems (OrderId)
```

---

## Part 2: MLM & Rebate System

### 2.1 Commission Calculation Flow

#### `sp_ct_Rebates`
**Purpose**: Generates monthly rebate invoices for consultant commissions

**Monthly Cycle**:
1. **Run Date**: Typically first week of following month (parameter: @drundate)
2. **Calculation Date**: Last day of previous month
3. **Data Source**: `compplanlive..ctcomph` (Commission Plan History)
4. **Output**: New invoice records in arinvc/aritrs

**Commission Calculation Formula**:

```
Base Rebate = SUM(ctcomph.namount) WHERE year/month/ccustno
Tax = Base Rebate × nVatRate / 100  (default: 15% VAT)
Total Rebate = Base Rebate + Tax

Downline Rebate = SUM(ctcomph.namount) WHERE ccontrib <> ccustno
  (Rebates earned from downline, credited to sponsor/upline)
```

**Invoice Creation**:

1. **Rebate Invoice (Type R)**:
   - cinvno: M + random 9-char suffix (e.g., M123ABC456)
   - ctype: 'R' (Rebate/Return)
   - ccustno: Consultant receiving rebate
   - nsalesamt: -(Base Rebate) [negative for credit]
   - ntaxamt1: -(Tax Amount)
   - nbalance: -(Base Rebate + Tax) [negative balance = credit]
   - lmlm: 1 (marked as MLM)
   - lautoreb: 0 (not auto-generated)

2. **Credit Memos** (for downline earnings):
   - Type: 'R' (Rebate)
   - ccustno: Upline consultant
   - ccontrib: Downline consultant contributing the sale
   - cmlmlink: Links sponsor-to-contributor relationship
   - lautoreb: 1 (auto-rebate)

3. **Debit Memos** (to debit contributor):
   - Type: (blank)
   - ccustno: Downline consultant
   - Records their payment obligation to sponsor

**Key Database Operations**:
```sql
INSERT arinvc (cuid, cinvno, ctype='R', ccustno, lmlm=1, ...)
INSERT aritrs (cuid, cinvno, citemno='SASAILM165', ...)
INSERT arcapp (cc ustno, cinvno, npaidamt=-(namount+ntaxamt1), ...)
```

**Monthly Cycle Trigger**:
- Marked complete: `UPDATE compplanlive..ctcomph SET cCompStatus='P'`
- Prevents duplicate rebate generation

**Related Tables**:
- Source: compplanlive..ctcomph (Commission Plan History - external database)
- Source: arcust (Customer details for address/company info)
- Destination: arinvc (AR Invoices)
- Destination: aritrs (AR Invoice Detail)
- Destination: arcapp (AR Applications/Payments)

---

#### `sp_ct_downlinebuild`
**Purpose**: Builds monthly consultant hierarchy snapshot for MLM calculations

**Process**:
1. **Input**: Year and Month
2. **Truncate**: Clears CTDownline (daily working table)
3. **Insert**: Rebuilds hierarchy from fn_get_downlineCT function
4. **Filter**: Only includes registered consultants (dRegister < statement date)
5. **Month-End**: Archives to CTDownlineh (history table) on last day of month

**Hierarchy Structure**:
```
ccustno: Consultant ID
ilevel: Generation level (1=Direct, 2=Downline, etc.)
csponsor: Direct sponsor
gen: Generation number
iTitle: Rank at this level
ipaTitle: Alternative title
cstatus: Active/Inactive status
```

**Related Tables**:
- Source: fn_get_downlineCT (recursive function building tree)
- Source: CTCons (Consultant status table)
- Source: CTDownlineh (previous month's hierarchy)
- Destination: CTDownline (current working table)
- Destination: CTDownlineh (monthly archive)

---

#### `vsp_mlm_getnewdocno` & `vsp_mlm_getnewmlminvc`
**Purpose**: Generate unique sequential document numbers for MLM transactions

**Document Number Generation**:

**MLM Receipt No (cmlmrcpt)**:
- Stored in: arsyst.cmlmrcpt
- Format: 10-char alphanumeric (space-padded on left)
- Algorithm: Increments right-to-left with overflow
- Sequence: 0-9, then A-Z, then restart at 0 with left digit increment
- Example: `0000000001` → `0000000009` → `000000000A` → `000000000Z` → `0000000010`

**MLM Invoice No (cmlmcinvno)**:
- Stored in: arsyst.cmlmcinvno
- Same algorithm as cmlmrcpt
- Used for invoice numbering in rebate system

**Transaction Protection**:
- Both wrapped in: `BEGIN TRANSACTION` / `COMMIT TRANSACTION`
- Prevents number collisions in concurrent execution
- Updates arsyst immediately after generation

**Related Tables**:
- Read/Write: arsyst (System Control - cmlmrcpt, cmlmcinvno fields)
- Check: arinvc (to verify number not already used)

---

#### `vsp_rpt_rbtmlm`
**Purpose**: Generate MLM rebate report for specific date and sponsor

**Parameters**:
- @dgenerate: Report date (NULL = today)
- @cSponsor: Filter by sponsor (empty = all sponsors)

**Output**: Temporary table `rbtmlm` with columns:
```
ccustno, csponsor (1st upline), csponsor0 (ultimate sponsor)
cstatus1 (status at level 1), cLevel, cstatus
ltl (Long Tail indicator)
nSalesamt, ngsalesamt, nsalesdisc, ngsalesdisc
nrebate, nrebateamt, ngrebateamt, ngrebateamt
arcust.ccompany, cphone1, cphone2
```

**Filtering**:
- Excludes records where cLevel = 'Sponsor' (reports on actual rebates, not sponsor commissions)
- Filters by creation date and optional sponsor
- Orders by ltl (Long Tail) descending

**Related Tables**:
- Source: RebateDtl (Rebate Detail - external/denormalized table)
- Source: arcust (Customer details)
- Destination: rbtmlm (temporary report table)

---

### 2.2 Campaign System

#### `Camp_NewMonth`
**Purpose**: Transitions campaign status at month boundary

**Logic**:
1. Find current active campaign: `cStatus='C'` AND `dto=@date` (ends today)
2. Mark current campaign as Historical: `cStatus='H'`
3. Find next campaign: `dfrom > @dto` (starts after current)
4. Mark next campaign as Current: `cStatus='C'`
5. Sync to NAM system: `EXEC sp_Camp_SynctoNam`

**Status Codes**:
- `C` = Current/Active (live in shop)
- `H` = Historical/Archived (completed)
- `D` = Draft (not yet started)
- `P` = Pending approval

**Related Tables**:
- Source/Dest: Campaign (campaign master)
- Destination: NAM integration (partner system)

**Campaign Fields**:
```
Campaign.ID
Campaign.cStatus (C/H/D/P)
Campaign.dfrom (start date)
Campaign.dto (end date)
Campaign.cStaffID (consultant/staff)
Campaign.nActualSales (current sales)
Campaign.nActualMLM (current MLM amount)
```

---

### 2.3 Rebate-Related Columns in ARINVC

When `lmlm=1` (MLM rebate invoice):

| Column | Purpose | Value |
|--------|---------|-------|
| ctype | Invoice type | 'R' = Rebate/Return |
| lmlm | MLM flag | 1 = MLM Transaction |
| lautoreb | Auto-rebate flag | 1 = Auto-generated, 0 = Manual |
| cmlmlink | MLM link ID | Unique ID linking sponsor-contributor |
| cmlmcinvno | MLM invoice number | From vsp_mlm_getnewmlminvc |
| nsalesamt | Sales amount (negative for credits) | -(Base Rebate) |
| ntaxamt1 | Tax | -(Tax Amount) |
| nbalance | Outstanding balance | -(Base + Tax) for credits |
| lfinchg | Finance charge flag | 0 = No finance charges |
| lapplytax | Apply tax flag | 1 = Tax applicable |

---

## Part 3: Order Fulfillment & Portal

### 3.1 Order Processing Pipeline

#### `AutoPick`
**Purpose**: Identifies orders ready for picking and batches them for warehouse processing

**Logic**:
1. Query soportal for printed but not-yet-picked orders
2. Check timing:
   - If `dprinted` is NULL: Return empty (order not printed)
   - If `dprinted` < 15 minutes AND < 10 orders: Return empty (wait for batch)
   - Otherwise: Proceed with batch
3. Mark orders with cStatus='P' (Picking)
4. Return batch to calling application for warehouse pick list generation

**Related Tables**:
- Source: soportal (Sales Order Portal - web order interface)
- Source: sosord (Sales Orders)
- Fields:
  - soportal.dcreate: Order creation time
  - soportal.dprinted: When order was sent to print
  - soportal.cstatus: ' ' (pending) → 'P' (picking) → 'S' (shipped)

**Business Logic**:
```
If NO printed orders: return nothing
If printed < 15 minutes AND count < 10: return nothing (insufficient batch size)
Else: batch process all pending orders
```

---

#### `Over_App`
**Purpose**: Apply overpayments and small balance write-offs to customer accounts

**Use Case**: Handles rounding differences and small payment discrepancies

**Process**:
1. Find accounts with:
   - `arcash.npaidamt > arcash.nappamt` (unmatched payment)
   - Difference between -5 and 5 (small variance)
   - `arcash.lvoid=0` AND `arcapp.lvoid=0` (not void)
   - `arcash.crfndno=''` (no refund issued)

2. Create Journal Entry (Type 'D' = Debit):
   - arjrnl: Journal header
   - arjtrs: Journal transaction detail
   - arinvc: Creates document 'J' + invoice number
   - aritrs: Line item with special code 'JOURNAL'
   - arcapp: Application record linking payment

3. Update:
   - `arcash.nappamt += @nBalance` (mark as applied)
   - Closes small discrepancy

**Related Tables**:
- Source: arcash (AR Cash Receipts)
- Source: arcapp (AR Applications)
- Source: arinvc (AR Invoices)
- Destination: arjrnl, arjtrs (Journal entries)
- Destination: arinvc (Journal invoices - type 'D')
- Destination: aritrs (Journal detail lines)

**Chart of Accounts**:
- Debit: `662000-2600` (Discount/Variance account)
- Credit: `200100-2600` (AR Control account)

---

#### `Return_App`
**Purpose**: Apply credit memos and returns to customer accounts

**Use Case**: Processes return authorizations, credit notes, and negative balances

**Process**:
1. Find invoices with:
   - `ctype IN ('R', 'N', 'C')` (Return, Note, Credit)
   - `nbalance < 0` (negative balance = credit)
   - `nbalance BETWEEN -5 and 5` (small variance)
   - `lvoid = 0` (not void)

2. Create Journal Entry (Type 'C' = Credit):
   - Similar to Over_App but for return/credit scenarios
   - Creates offsetting journal entry
   - Updates original invoice: `nbalance = 0, nftotpaid += @nBalance`

3. Complete the transaction:
   - Links credit memo to original invoice
   - Clears outstanding balance
   - Records in journal for audit trail

**Related Tables**:
- Source: arinvc (with ctype='R'/'N'/'C')
- Source: aritrs (invoice detail)
- Destination: arjrnl, arjtrs (Journal entries - Type 'C')
- Destination: arinvc (creates journal entry)
- Destination: aritrs (journal detail)

---

### 3.2 Portal Integration

**soportal Table**:
```
csono: Sales order number
cbcompany: Bill-to company
dcreate: Creation timestamp
dprinted: When order was printed to warehouse
cstatus: Order status (' '=pending, 'P'=picking, 'S'=shipped)
```

**Status Flow**:
```
WEB ORDER → ' ' (pending) → 'P' (AutoPick) → 'S' (shipped) → NOP (sp_NOP_syncOrderStatus)
```

---

## Part 4: Integration Architecture

### 4.1 Integration Method: API vs Direct DB

**API-Based Syncs** (Recommended for data security):
- `sp_NOP_syncOrders`
- `sp_NOP_syncItemChanges`
- `sp_NOP_syncItemAvailability`
- `sp_NOP_syncOrderStatus`
- `sp_NOP_syncItemAll`
- `sp_NOP_syncStaff`

**Mechanism**:
- HTTP GET/POST to `https://nopintegration.annique.com/*`
- Calls `sp_ws_HTTP` stored procedure
- Integration service parses request and updates databases
- Returns JSON response
- Note: No dedicated API call audit logging table identified. NopIntegration..Brevolog is a Brevo email marketing campaign log, not an API audit trail

**Direct Database Writes** (Direct linked server):
- `sp_NOP_syncSoxitems` - Writes directly to `[NOP].annique.dbo.ANQ_ExclusiveItems`
- `sp_ws_UpdateImageNOP` - Calls external procedure in NOP database

**Hybrid Approach**:
- Primary: API for main data flows (orders, products)
- Secondary: Direct DB for exclusive items and images
- Benefits: Decouples systems, allows independent scaling, audit trail via API logs

---

### 4.2 Linked Server Configuration

**Linked Server Name**: `NOP`

**Connection String**:
```
Server=[NOP_SERVER]\[INSTANCE]
Database=annique
Trusted_Connection=yes
```

**Usage Pattern**:
```sql
SELECT * FROM [NOP].annique.dbo.Product
INSERT INTO [NOP].annique.dbo.ANQ_ExclusiveItems (...)
UPDATE [NOP].annique.dbo.Customer SET ...
```

**Tables Accessed**:
- `[NOP].annique.dbo.Customer`
- `[NOP].annique.dbo.Product`
- `[NOP].annique.dbo.ANQ_ExclusiveItems`
- `[NOP].annique.dbo.ANQ_Booking`
- `[NOP].annique.dbo.Orders`
- `[NOP].annique.dbo.OrderItems`
- `[NOP].annique.dbo.Discount`
- `[NOP].annique.dbo.ANQ_Lookups`

---

### 4.3 Web Service HTTP Call Implementation (sp_ws_HTTP)

**Authentication**:
```sql
DECLARE @authHeader NVARCHAR(64)
SET @authHeader = 'BASIC 0123456789ABCDEF0123456789ABCDEF'
-- Hardcoded Base64-encoded credentials
```

**Request Types**:
- **GET**: For read operations and syncs
  ```
  https://nopintegration.annique.com/syncproducts.api?type=changes
  ```
- **POST**: For data submission
  ```
  Content-Type: application/json
  @cpostData: JSON payload with product/order data
  ```

**Response Handling**:
```sql
@cretvalue: Return status code (32-char)
@cresponse: Response body (4000-char max)
-- Typically JSON with success/error details
```

**Error Handling**:
- Returns @return_value (0=success, non-zero=error)
- No dedicated API error logging table identified (Brevolog is Brevo email campaign tracking, not API audit)
- Procedures check wsSetting for NULL before executing

---

### 4.4 Configuration Management

**Control Table: wsSetting**

```sql
SELECT * FROM wsSetting
```

**Key Settings**:

| Name | Value | Purpose |
|------|-------|---------|
| ws.url | https://nopintegration.annique.com/ | Base URL for API calls |
| ws.timeout | 30 | HTTP timeout in seconds |
| ws.retries | 3 | Number of retry attempts |
| ws.auth | BASIC (hardcoded) | Authentication method |

**Safety Mechanism**:
```sql
SELECT @cUrl=LTRIM(RTRIM(Value)) FROM wsSetting WHERE Name='ws.url'
IF @cUrl IS NULL RETURN  -- Disable all syncs if URL not configured
```

**Implications**:
- Can disable all NOP syncs by setting ws.url to NULL
- Centralized configuration allows runtime adjustments
- No code changes needed for URL/timeout modifications

---

### 4.5 Change Detection & Audit

**Changes Table Structure**:
```
ctablename: Source table (ICITEM, ARCUST, etc.)
cprimarykey: Record ID (item number, customer, etc.)
cfieldname: Column name that changed
coldvalue: Previous value
cnewvalue: New value
dchanged: Timestamp
ccreatedby: User making change
```

**Usage in Integration**:
```sql
-- Only sync items with status changed to Inactive
WHERE cfieldname='cstatus' AND cnewvalue='I'

-- Full audit trail of all product changes
SELECT * FROM changes WHERE ctablename='ICITEM'
```

**Benefits**:
- Selective syncing (only changed items)
- Audit trail for compliance
- Change history for troubleshooting
- Prevents unnecessary full catalog syncs

---

## Part 5: Data Mapping Reference

### NopCommerce to AccountMate Mapping

| NOP Table | NOP Column | AccountMate Table | AccountMate Column | Purpose |
|-----------|-----------|-------------------|-------------------|---------|
| Customer | id | arcust | cCustno | Consultant ID mapping |
| Customer | username | arcust | cCustno (string) | Username = Customer #  |
| Product | id | icitem | id (custom) | Product ID |
| Product | sku | icitem | citemno | SKU mapping |
| Product | name | icitem | cdescript | Product name |
| Product | StockQuantity | icqoh | nqtyoh | On-hand inventory |
| Orders | id | arinvc | coriginvno | Original order# |
| OrderItems | ProductId | aritrs | citemno | Line item SKU |
| OrderItems | Quantity | aritrs | nshipqty | Quantity shipped |
| ANQ_ExclusiveItems | id | soxitems | id | Exclusive item link |
| ANQ_ExclusiveItems | CustomerID | soxitems | ccustno | Consultant ID |
| ANQ_ExclusiveItems | ProductID | soxitems | citemno | Item number |

---

### MLM/Rebate Mapping

| Field | Table | Purpose |
|-------|-------|---------|
| lmlm | arinvc | MLM flag (1=MLM, 0=Normal) |
| lautoreb | arinvc | Auto-rebate (1=auto, 0=manual) |
| cmlmlink | arinvc | Link ID for sponsor-contributor |
| cmlmcinvno | arinvc | MLM invoice number |
| cmlmrcpt | arsyst | MLM receipt counter |
| ccustno | arcust | Consultant ID |
| csponsor | arcust | Direct sponsor |
| ilevel | ctdownline | Generation level |
| cstatus | arcust | Consultant status (A/I) |

---

## Part 6: Key Business Rules Summary

### NOP Sync Rules
1. **Orders**: Only synced if order exists in both NOP and accounting database
2. **Items**: Exclusive items only added if Product and Customer exist in NOP
3. **Changes**: Only changed fields trigger sync (not full record every time)
4. **Status**: Order status synced back from AccountMate to NOP after fulfillment
5. **Availability**: Stock levels updated in real-time for web accuracy

### MLM Rules
1. **Monthly Cycle**: Rebates generated once per month, typically first week
2. **Downline Earnings**: Automatically credited to sponsor's account
3. **Hierarchy**: Built monthly (CTDownline) based on registration date
4. **Consultant Status**: Only active (cstatus='A') consultants receive rebates
5. **Tax**: Rebates subject to VAT (default 15%)
6. **Document Numbering**: Sequential, collision-safe using transaction protection

### Order Fulfillment Rules
1. **AutoPick**: Batching requires either 15+ minutes or 10+ orders
2. **Over_App**: Applies small variances (-5 to +5) as write-offs
3. **Return_App**: Processes returns only if balance <0 and <5 variance
4. **Portal**: soportal.cstatus controls warehouse picking workflow
5. **Archive**: Old orders archived, affecting batch picking logic

---

## Part 7: Trigger & View Support

### Key Triggers
- `tr_soxitems_insert`: Calls `sp_NOP_syncSoxitems` when exclusive item added
- `tr_soxitems_update`: Calls `sp_NOP_syncSoxitems` when exclusive item changed
- `tr_icitem_update`: Updates `changes` table when item changed
- `tr_arinvc_insert`: Creates change record for new invoice
- Various inventory triggers: Update `changes` table on icqoh modifications

### Related Views
- `vsp_rpt_rbtmlm`: MLM rebate report view
- Views filtering MLM-related transactions
- Views for inventory availability reporting
- Order status views for portal integration

---

## Part 8: Potential Issues & Considerations

### API Integration Risks
1. **Service Availability**: If `nopintegration.annique.com` is down, syncs fail silently
   - Mitigation: No API audit log table identified; consider adding one for monitoring
   - Check: wsSetting.ws.url is configured

2. **Authentication**: Hardcoded credentials in sp_ws_HTTP
   - Mitigation: Move to configuration table
   - Store credentials securely

3. **Request Timeout**: 30-second default may be too short for large syncs
   - Check: Timeout in wsSetting
   - Monitor: No API timeout logging identified; consider adding error tracking

### Data Integrity Risks
1. **Exclusive Items**: Requires valid Product and Customer in NOP
   - Risk: Items silently not synced if customer/product missing
   - Mitigation: Validate before inserting into soxitems

2. **MLM Invoices**: Complex multi-step insertion across 3 tables
   - Risk: Partial insert if error occurs mid-transaction
   - Mitigation: Wrapped in transaction but verify reconciliation

3. **Document Numbers**: Sequential generation with transaction lock
   - Risk: Gaps if process killed during update
   - Mitigation: Check arsyst for orphaned sequences

### Performance Considerations
1. **Full Item Sync** (`sp_NOP_syncItemAll`): May timeout with large catalog
   - Recommendation: Run during off-peak hours
   - Consider: Parallel processing of category subsets

2. **Rebate Generation** (`sp_ct_Rebates`): Complex cursor iteration
   - Recommendation: Run during batch window
   - Monitor: Execution time, locking

3. **Downline Building** (`sp_ct_downlinebuild`): Recursive function
   - Recommendation: Index CTDownline tables
   - Monitor: Level depth if organization grows

---

## Part 9: Maintenance & Monitoring

### Health Check Queries

```sql
-- Check last sync execution
-- Note: NopIntegration..Brevolog is Brevo email campaign logs, NOT API audit
-- No dedicated API call logging table identified in current schema
SELECT TOP 10 * FROM NopIntegration..Brevolog ORDER BY ddate DESC  -- Email campaign actions only

-- Verify configuration
SELECT * FROM wsSetting WHERE Name LIKE 'ws.%'

-- Check pending changes
SELECT COUNT(*) FROM changes WHERE dchanged > DATEADD(day, -1, GETDATE())

-- Verify exclusive items sync
SELECT COUNT(*) FROM soxitems WHERE lupdatetows=1

-- Check order portal status
SELECT cstatus, COUNT(*) FROM soportal GROUP BY cstatus
```

### Regular Maintenance Tasks
1. **Weekly**: Check for API sync failures (no dedicated audit log exists; monitor via wsSetting and changes table)
2. **Monthly**: Verify rebate generation (cCompStatus='P')
3. **Monthly**: Archive old portal records (dprinted > 90 days ago)
4. **Quarterly**: Audit exclusive items in both systems
5. **Quarterly**: Review and clean up orphaned document numbers

---

## Conclusion

The Annique integration architecture combines multiple approaches:
- **API-based syncing** for orders, items, and status
- **Direct database writes** for exclusive items and images
- **Complex MLM business logic** with monthly rebate cycles
- **Portal-based order fulfillment** with intelligent batching

Key architectural strengths:
1. Decoupled systems via API reduce tight coupling
2. Configuration-driven wsSetting allows runtime changes
3. Change tracking prevents unnecessary full syncs
4. Transaction protection ensures data consistency in MLM
5. Change tracking (changes table) supports compliance. Note: Brevolog is Brevo email campaign logging, not API audit. No dedicated API audit trail exists — this is a gap to address in migration.

Areas requiring careful monitoring:
1. API endpoint availability and performance
2. Credential management in sp_ws_HTTP
3. Data synchronization validation (especially exclusive items)
4. Document number sequence integrity
5. Rebate generation accuracy and timing
