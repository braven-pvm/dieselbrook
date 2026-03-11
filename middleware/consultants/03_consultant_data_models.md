# Consultant Data Models

## Purpose

This document separates:

- current ERP consultant structures
- current Nop plugin consultant structures
- proposed middleware consultant model
- the minimum Shopify-facing data needed for consultant behavior

## 1. Current ERP Consultant Structures

## `arcust`

This is the most important consultant/customer master source.

Key consultant-relevant fields evidenced in discovery and source:

| Field | Meaning |
|---|---|
| `ccustno` | consultant/customer number |
| `cStatus` | active/inactive consultant state |
| `cSponsor` | sponsor / upline reference |
| `dStarter` | activation / starter date |
| `cEmail`, `cPhone*` | contact details |
| `cFname`, `cLname`, `cCompany` | identity details |
| `cIdNo` | national or identity number |
| `cLanguage`, `cRace`, `cTitle` | profile fields |
| bank/account fields | payout / consultant profile data |

## hierarchy and commission structures

ERP-side consultant hierarchy and commission logic also depends on:

- `CTDownline`
- `CTDownlineh`
- `genreb`
- related views and procedures such as `sp_ct_Rebates` and `sp_ct_downlinebuild`

## 2. Current Nop Plugin Tables

Confirmed plugin schema mappings include:

| Entity | Table |
|---|---|
| `UserProfileAdditionalInfo` | `ANQ_UserProfileAdditionalInfo` |
| `ExclusiveItems` | `ANQ_ExclusiveItems` |
| `Award` | `ANQ_Award` |
| `AwardShoppingCartItem` | `ANQ_AwardShoppingCartItem` |
| `Booking` | `ANQ_Booking` |
| `CustomerChanges` | `ANQ_CustomerChanges` |
| `NewRegistrations` | `ANQ_NewRegistrations` |
| `RegistrationPageSettings` | `ANQ_RegistrationPageSettings` |
| `Report` | `ANQ_Report` |
| `ReportParameter` | `ANQ_ReportParameter` |
| `ReportParameterValue` | `ANQ_ReportParameterValue` |
| `Otp` | `ANQ_Otp` |

## `ANQ_UserProfileAdditionalInfo`

Confirmed fields:

- `CustomerId`
- `Title`
- `Nationality`
- `IdNumber`
- `Language`
- `Ethnicity`
- `BankName`
- `AccountHolder`
- `AccountNumber`
- `AccountType`
- `ActivationDate`
- `Accept`
- `ProfileUpdated`
- `WhatsappNumber`
- `BrevoID`

Purpose:

- consultant-only profile extension on top of Nop customer

## `ANQ_ExclusiveItems`

Confirmed fields:

- `ProductID`
- `CustomerID`
- `RegistrationID`
- `nQtyLimit`
- `nQtyPurchased`
- `dFrom`, `dTo`
- `IActive`
- `IStarter`
- `IForce`

Purpose:

- consultant-specific product entitlement and starter allocation model

## `ANQ_Award`

Confirmed fields:

- `CustomerId`
- `AwardType`
- `Description`
- `MaxValue`
- `ExpiryDate`
- `OrderId`
- `dCreated`
- `dTaken`

Purpose:

- consultant award entitlement and redemption tracking

## `ANQ_Booking`

Confirmed fields:

- `EventID`
- `CustomerID`
- `ConsultantCustomerID`
- `Name`
- `Status`
- `DateBooked`
- `Attended`
- `OrderID`
- `cSono`
- `cInvno`
- `IsPrimaryRegistrant`
- `dLastUpd`
- `IEmail`

Purpose:

- consultant-adjacent event/booking model with ERP order/invoice linkage

## `ANQ_NewRegistrations`

Confirmed fields include:

- sponsor
- proposed consultant number
- personal/contact details
- acceptance flags
- referral and interest fields
- status and activation link

Purpose:

- consultant registration intake pipeline

## 3. Proposed Middleware Consultant Model

Dieselbrook should create a canonical consultant record independent of both Shopify and raw ERP table structure.

## `consultant_master`

| Field | Purpose |
|---|---|
| `id` | middleware consultant id |
| `am_ccustno` | ERP consultant number |
| `shopify_customer_id` | linked Shopify customer |
| `status` | active / inactive / pending / reactivated |
| `sponsor_am_ccustno` | direct sponsor reference |
| `activation_date` | effective activation date |
| `email` | canonical contact email |
| `phone` | primary phone |
| `whatsapp_number` | consultant WhatsApp |
| `language` | language preference |
| `last_synced_at` | sync timestamp |

## `consultant_profile`

| Field | Purpose |
|---|---|
| `consultant_id` | parent consultant |
| `title` | title |
| `nationality` | nationality |
| `id_number` | identity number |
| `ethnicity` | ethnicity |
| `bank_name` | banking info |
| `account_holder` | banking info |
| `account_number` | banking info |
| `account_type` | banking info |
| `accepted_terms` | consultant agreement state |
| `profile_completed` | profile gate state |

## `consultant_entitlement`

| Field | Purpose |
|---|---|
| `consultant_id` | parent consultant |
| `type` | starter / exclusive / award / gift / special_offer |
| `external_ref` | source entitlement reference |
| `product_ref` | linked product if applicable |
| `quantity_limit` | entitlement cap |
| `quantity_used` | usage count |
| `valid_from` | start date |
| `valid_to` | end date |
| `force_apply` | force-add / force-show semantics |
| `active` | current active state |

## `consultant_registration`

| Field | Purpose |
|---|---|
| `id` | registration id |
| `submitted_at` | intake timestamp |
| `sponsor_ref` | sponsor info |
| `candidate_email` | applicant email |
| `candidate_phone` | applicant phone |
| `status` | submitted / validated / approved / rejected / converted |
| `validation_payload` | third-party validation result |
| `am_ccustno` | created consultant number when applicable |
| `shopify_customer_id` | linked storefront customer when applicable |

## 4. Shopify-Facing Consultant Data

The storefront likely needs only a subset of the full consultant model.

Recommended Shopify-facing data:

| Data | Why it matters |
|---|---|
| `consultant.is_consultant` | storefront eligibility gate |
| `consultant.am_ccustno` | support/debug trace |
| `consultant.status` | active/inactive storefront behavior |
| `consultant.activation_date` | diagnostics and milestone logic |
| `consultant.profile_completed` | checkout gating |
| `consultant.sponsor_ref` | optional display / support use |
| consultant entitlement summary | exclusive items / starter kits / awards |

## Important Boundary

Shopify should not become the primary storage system for:

- MLM hierarchy
- rebate ledger history
- deep consultant payout/bank processes
- historical downline snapshots

Those belong in ERP or middleware-controlled stores.