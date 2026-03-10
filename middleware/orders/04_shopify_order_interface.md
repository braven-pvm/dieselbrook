# Shopify Order Interface

## Purpose

This document defines how the new Dieselbrook middleware should interface with Shopify for the order domain.

It covers:

- inbound Shopify events
- canonical translation into middleware order contracts
- outbound status updates from AccountMate back to Shopify
- key mapping rules between Shopify order objects and AccountMate order structures

## Recommended Shopify Inputs

The middleware should consume at least these Shopify signals:

| Shopify input | Why it matters |
|---|---|
| `orders/create` | initial order creation |
| `orders/updated` | late edits, payment updates, note changes |
| `orders/paid` or equivalent payment-ready signal | gate for AM write eligibility |
| `refunds/create` | returns and financial reversals |
| `fulfillments/create` / `fulfillments/update` | outbound visibility alignment |
| `customers/update` | customer/account resolution dependencies |

The exact webhook set may vary depending on Shopify app architecture, but the middleware must have a reliable trigger for when an order becomes safe to write into AM.

## Inbound Translation Pipeline

Recommended translation stages:

1. receive raw Shopify webhook
2. validate signature and store raw payload
3. enrich by reading additional Shopify data if the webhook is incomplete
4. build canonical order contract
5. resolve AM customer and item mappings
6. apply business rules
7. write into AM only when the order reaches a valid integration state

## Key Shopify to AM Field Mapping

| Shopify concept | Middleware canonical field | AM target |
|---|---|---|
| Order ID | `shopify_order_id` | `sosord.cPono` |
| Order name / number | `shopify_order_name` | candidate for `sosord.cSono` or secondary trace field |
| Customer ID | `shopify_customer_id` | used to resolve `sosord.cCustno` |
| Customer tags / segmentation | account resolution inputs | determines final AM customer mapping |
| Billing address | `billing_address` | `sosord.cb*` |
| Shipping address | `shipping_address` | `sosord.cs*` |
| Shipping line title/code | `shipping_method_code` | `sosord.cShipVia`, `sosord.cFrgtCode` |
| Currency | `currency_code` | `sosord.cCurrCode` |
| Order created time | `order_date_utc` | `sosord.dOrder` |
| Line item SKU | `resolved_am_itemno` | `sostrs.cItemNo` |
| Line item quantity | `quantity` | `sostrs.nOrdQty` |
| Line unit price | `price_excl_tax` / `price_incl_tax` | `sostrs.nPrice`, `sostrs.nPrcInCtx` |
| Line discount | `discount_amount` / `discount_rate` | `sostrs.nDiscAmt`, `sostrs.nDiscRate` |
| Tax lines | `tax_rate` / `tax_amount` | `sostrs.nTaxAmt*`, `sosord.nTaxAmt*` |
| Payment transactions | `payments[]` | `arcash` mapping or equivalent AM payment path |
| Fulfillment tracking | status sync payload | Shopify shipment/tracking update |

## Order Number Strategy

The legacy implementation uses `CustomOrderNumber` as the source for `cSono` and the NOP numeric order ID as `cPono`.

For Shopify, Dieselbrook should keep two identities:

- AM order number used operationally inside AccountMate
- external Shopify order identity stored for idempotency and traceability

Recommended approach:

- keep Shopify Order ID as the immutable external identity
- keep Shopify order name as a human-facing reference
- do not rely on one field to serve both support and idempotency purposes

## Shopify Status -> AM Write Eligibility

The middleware should define explicit gates such as:

| Shopify state | AM write decision |
|---|---|
| draft / incomplete | do not write |
| authorized but not settled | depends on business rule |
| paid | write when all mappings resolve |
| cancelled before import | do not write |
| edited after import | reconcile / operator workflow |

This needs final business confirmation, but it must be explicit.

## AM -> Shopify Status Mapping

Based on the current system, the middleware will need a state map roughly like this:

| AM signal | Shopify-visible result |
|---|---|
| `SOPortal` blank and no shipment | pending / processing note |
| `SOPortal = P` | picking / warehouse in progress |
| invoice created but no tracking | packed / waiting for courier logic |
| shipment with waybill | tracking added |
| shipped / delivered rules met | fulfillment marked sent or delivered |

## Tracking URL Logic

The current system derives tracking links from AM shipping method values. The new middleware should preserve that, but in a normalized mapping table rather than hardcoded branching.

Suggested mapping table:

| AM `cShipVia` | Carrier | Tracking URL template |
|---|---|---|
| `BERCO` | Aramex | `https://www.aramex.com/ar/en/track/track-results-new?ShipmentNumber={waybill}` |
| `FASTWAY` | Fastway | `https://www.fastway.co.za/our-services/track-your-parcel?l={waybill}` |
| `POSTNET` | Aramex | `https://www.aramex.com/ar/en/track/track-results-new?ShipmentNumber={waybill}` |
| `SKYNET` | Paxi | `https://parcel-tracking.paxiplatform.com/?id={waybill}` |
| `COLLECT` | none | no external tracking |

## Shopify-Specific Data Gaps to Plan For

The middleware will likely need supplementary Shopify data beyond the basic order payload for:

- customer segmentation / role resolution
- pickup-point selection details
- special attributes that currently influence shipping or award logic
- event/ticket side features
- manual edits after payment

These should be modeled explicitly as metadata or enriched reads, not left as implicit assumptions.

## Target Rule

Shopify is the source of customer-facing order intent.
AccountMate is the source of ERP execution and financial realization.
The middleware must be the system that translates between those two without leaking either model directly into the other.