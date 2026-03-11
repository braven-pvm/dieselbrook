# Current-State Consultant Domain

## Scope

This document explains how consultants behave today across:

- AccountMate
- the FoxPro middleware
- the NopCommerce custom plugin layer
- consultant-only storefront features
- downstream commission and reporting dependencies

## Short Answer

No, consultants are not just users in NopCommerce.

They are currently represented across three layers:

1. ERP consultant/customer records in AccountMate
2. NopCommerce customer accounts with consultant role and plugin-managed metadata
3. custom consultant features layered into storefront, checkout, registration, awards, reports, and exclusive-item flows

## 1. ERP-Centric Consultant Identity

The most important consultant record still lives in AccountMate.

Confirmed current signals:

- `arcust.ccustno` acts as the consultant number / ERP identity
- `arcust.cStatus` controls active vs inactive status
- `arcust.cSponsor` and related hierarchy fields feed sponsor/upline logic
- `arcust.dStarter` represents activation or starter-date state
- monthly and daily MLM processes depend on AccountMate-side hierarchy and invoice data

Consultant lifecycle and commission logic are therefore ERP-native, not storefront-native.

## 2. FoxPro Consultant Sync From AM to NopCommerce

The current middleware implementation lives in `NISource/syncconsultant.prg`.

### What it does

For a given AccountMate consultant:

1. loads the consultant from `arcust`
2. tries to find the matching Nop customer by `username = ccustno`
3. creates the Nop customer if missing
4. updates billing and shipping addresses from AM address sources
5. assigns customer roles
6. updates consultant profile metadata in the Nop plugin tables
7. loads starter-kit allocations for new consultants

### Important implementation facts

| Behaviour | Current implementation |
|---|---|
| consultant identity in NOP | username = AM `ccustno` |
| initial password for new consultant | generated from consultant number plus static suffix |
| active flag | NOP `active` tracks AM `cStatus = A` |
| roles added | `Registered` and `Annique Consultant` |
| profile sync | `ANQ_UserProfileAdditionalInfo` updated from AM fields |
| starter kits | `NopIntegration..ANQ_loadstarter` called for new consultants |

### Additional profile fields synced from AM

The FoxPro sync writes consultant metadata such as:

- title
- nationality
- ID number
- language
- ethnicity
- bank name
- account holder
- account number
- account type
- activation date
- WhatsApp number

This is the first strong signal that the consultant model is wider than a normal ecommerce user record.

## 3. NopCommerce Consultant Role And Plugin Behavior

The main storefront customization lives in:

- `Plugins/Annique.Plugins.Nop.Customization`

The plugin has an explicit `ConsultantRoleId` setting and checks that role throughout checkout, profile, registration, reports, awards, and navigation logic.

### Consultant-only customizations confirmed in source

| Area | Current behaviour |
|---|---|
| profile UI | consultant users get a custom account/info flow |
| checkout | consultant users get consultant-specific gift, starter-kit, exclusive-item, and special-offer handling |
| billing/shipping edits | consultant address changes are tracked in `ANQ_CustomerChanges` |
| activation | first paid qualifying order updates consultant activation date in plugin data |
| awards | awards are redeemed and marked taken during consultant checkout/order flow |
| exclusive items | consultant access to exclusive category/products is explicitly enforced |
| registration | custom consultant registration route and workflow exist |
| reports | consultant-facing report route exists under customer area |
| bookings/events | bookings track both customer and consultant identity |

## 4. Consultant Profile Completeness Gate

The plugin service `UserProfileAdditionalInfoService.ValidateUserProfileAsync` can block checkout when:

- customer email is missing
- consultant profile record is missing
- consultant `ProfileUpdated` is false

Meaning:

- consultant checkout eligibility depends on consultant-specific profile completeness state, not just a valid login

## 5. Activation-Date Logic

The plugin method `UpdateActivationDateOnFirstOrderAsync` shows:

- consultant activation date is set only if currently null
- it uses the first paid order in `Processing` or `Complete` status
- it writes a detailed order note when activation date is updated

This is not just ERP data mirroring. It is active consultant lifecycle management happening inside the storefront system.

## 6. Consultant Checkout Extensions

The custom checkout flow distinguishes consultant vs non-consultant users.

For consultant users, the plugin can surface:

- blank gifts
- starter-kit exclusive items
- special offers

before final checkout progression.

This means consultant entitlements are woven into cart and checkout UX, not only into pricing.

## 7. Exclusive Items Model

The custom plugin creates and uses `ANQ_ExclusiveItems`.

Confirmed fields include:

- `ProductID`
- `CustomerID`
- `RegistrationID`
- `nQtyLimit`
- `nQtyPurchased`
- `dFrom`, `dTo`
- `IActive`
- `IStarter`
- `IForce`

Confirmed behaviours include:

- consultant access to exclusive category/products is filtered by allocation
- some exclusive items are force-add or starter-kit driven
- exclusive allocations are decremented or consumed on order placement
- exclusive allocations can be restored on order cancellation

This is a real entitlement subsystem, not a cosmetic category restriction.

## 8. Awards Model

The plugin creates and uses `ANQ_Award` and `ANQ_AwardShoppingCartItem`.

Confirmed award fields include:

- `CustomerId`
- `AwardType`
- `Description`
- `MaxValue`
- `ExpiryDate`
- `OrderId`
- `dCreated`
- `dTaken`

Awards are processed during consultant checkout completion and during admin-side paid-order handling.

## 9. Consultant Registration Model

The plugin provides a dedicated route:

- `consultant-register`

and persists consultant registration data into:

- `ANQ_NewRegistrations`
- `ANQ_RegistrationPageSettings`

Confirmed registration fields include:

- sponsor
- proposed consultant number
- name/company/title
- language
- phone/email
- country and geo coordinates
- acceptance flags
- referral / heard-about / interests
- status and activation link

This shows that the storefront also supports new-consultant onboarding, not just existing-consultant login.

## 10. Consultant Reports And My Account Surface

The custom route provider exposes:

- `customer/myaccount`
- `customer/reports`

for consultant-focused account and report experiences.

That means the consultant domain includes self-service reporting, not only shopping and profile data.

## 11. Events / Bookings Linkage

The plugin creates and uses `ANQ_Booking` with fields including:

- `CustomerID`
- `ConsultantCustomerID`
- `OrderID`
- `cSono`
- `cInvno`

Meaning:

- bookings can explicitly reference both the attendee/customer and the consultant relationship
- AM order and invoice references flow into consultant-adjacent features

## 12. Commission And Hierarchy Are Still ERP-Side

The storefront plugin adds consultant-facing behavior, but commission logic is still fundamentally in AccountMate.

Confirmed ERP-side processes:

- `sp_ct_Rebates` calculates multi-level rebates
- `sp_ct_downlinebuild` rebuilds hierarchy / downline structures
- tables such as `genreb`, `CTDownline`, `CTDownlineh`, and related views support commission and hierarchy logic

This is the critical architectural boundary:

- NopCommerce expresses consultant-facing UX and entitlements
- AccountMate remains the commission and hierarchy engine

## Current-State Conclusion

The consultant domain currently spans:

1. ERP identity and lifecycle
2. middleware sync into storefront accounts
3. storefront plugin metadata and UX rules
4. entitlement logic for gifts, awards, and exclusive items
5. consultant registration and self-service reporting
6. ERP-side hierarchy and commission processing

Any Dieselbrook replacement that collapses this into “Shopify users with tags” will miss critical business behavior.