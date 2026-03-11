# Nop Customization Domain Classification

## Purpose

This document breaks the custom NopCommerce plugin into migration planning buckets:

1. replace in Shopify and Dieselbrook
2. candidate to retire
3. preserve as a business concept but re-home differently

The purpose is to stop treating the custom plugin as one large indivisible block.

## Bucket 1: Replace In Shopify And Dieselbrook

These domains are core commerce behavior and must be reimplemented one way or another.

| Domain | Why it must be replaced | Evidence in plugin |
|---|---|---|
| Order processing and reporting | core transactional flow; affects all order behavior | `Services/Orders/`, `Controllers/CustomOrderController.cs`, `Services/OverriddenServices/OverriddenOrderReportService.cs`, order events |
| Pricing and discount engine | custom role- and rule-driven pricing is central to the business model | `Services/OverriddenServices/OverriddenPriceCalculationService.cs`, `Services/OverriddenServices/OverrideDiscountService.cs`, `Services/DiscountAllocation/` |
| Shopping cart behavior | cart logic carries gifts, awards, exclusives, and checkout rules | `Services/OverriddenServices/AnniqueShoppingCartService.cs`, `ActionFilters/UpdateCartActionFilter.cs`, cart events |
| Product catalog behavior | product visibility and composition are customized beyond standard catalog rendering | `Services/OverriddenServices/CustomProductService.cs`, `Controllers/OverriddenProductController.cs`, catalog factories |
| Checkout flow | checkout is heavily customized for payment, gifts, shipping, and consultant logic | `Controllers/CustomCheckoutController.cs`, `ActionFilters/CheckoutActionFilter.cs`, `ActionFilters/PaymentInfoActionFilter.cs` |
| Shipping and delivery rules | shipping rules, pickup collection, and address validation directly affect fulfillment | `Services/ShippingRule/`, `Services/PickUpCollection/`, `Services/ShippingAddressValidation/` |
| Customer registration and auth behavior | login, registration, session, and identity behavior are customized | `Services/OverriddenServices/OverriddenCustomerRegistrationService.cs`, `Services/OverriddenServices/OverriddenCookieAuthenticationService.cs`, register filters |
| Address management | address capture and validation have custom business rules | `Services/OverriddenServices/OverriddenAddressService.cs`, billing and address action filters |

## Bucket 2: Candidate To Retire

These domains may be low-value or overly specific legacy programs and should be challenged before reimplementation.

| Domain | Why retirement is plausible | Evidence in plugin |
|---|---|---|
| Consultant awards system | recognition and reward logic appears tightly coupled to legacy shopping flow and may not justify first-release rebuild | `Services/ConsultantAwards/`, `Domain/ConsultantAwards/`, `Controllers/AwardsController.cs` |
| Gift card allocation and gift marketplace | niche promotional mechanics that may be simplified or folded into other promotion tooling | `Services/GiftCardAllocation/`, `Services/CheckoutGIfts/`, `Domain/AnniqueGifts/` |
| Trip promotion engine | appears campaign-specific and not foundational to baseline commerce replacement | trip-related settings and `Components/TripPromotionViewComponent.cs` |
| Some legacy gift/promotional widgets | highly specific merchandising features may not survive first-release simplification | gift components and related offer widgets |

## Bucket 3: Preserve As Concept But Re-Home Differently

These are real business capabilities, but they should not be rebuilt as a like-for-like Nop plugin.

| Domain | Why it should be preserved | Evidence in plugin | Recommended re-home |
|---|---|---|---|
| Consultant onboarding and validation | core business process for field-force growth | `Services/ConsultantRegistrations/`, `Controllers/ConsultantRegistrationController.cs`, registration domain models | Dieselbrook middleware plus Shopify app admin surfaces |
| OTP and multi-factor behavior | security and identity hardening may still be needed | `Services/OTP/`, `Domain/Otp.cs`, OTP components and settings | identity provider or dedicated auth adapter |
| Extended consultant profiles | consultant identity extends beyond standard customer fields | `Services/UserProfile/`, `Domain/UserProfileAdditionalInfo.cs`, profile components | Shopify metafields or middleware profile service |
| Business reporting | reports remain operationally important but should not stay embedded in storefront plugin logic | `Services/AnniqueReport/`, `Controllers/PublicAnniqueReportController.cs`, report domain models | BI/reporting service backed by middleware APIs |
| Events and bookings | may remain strategically valuable but belong in a fit-for-purpose subsystem | `Services/AnniqueEvents/`, `Domain/AnniqueEvents/`, `Controllers/AnniqueEventsController.cs` | dedicated event platform or specialized module |
| AI chatbot | potentially valuable, but should be a standalone channel/service instead of a plugin-owned concern | `Services/ChatbotAnnalie/`, `Controllers/ChatbotController.cs`, chatbot models | external AI/chat service integrated with Shopify and middleware |
| Category and manufacturer integration | integration mappings still matter but should live in middleware | `Services/CategoryIntegration/`, `Services/ManufacturerIntegration/`, admin controllers | middleware reference-data sync |
| Dynamic promotions and offers | offer rules matter, but should move to explicit pricing/promotion services | `Services/SpecialOffers/`, `Domain/SpecialOffers/`, offer components | Shopify plus Dieselbrook pricing/promo engine |
| Staff and consultant checkout rules | operationally important but should be expressed as role/policy rules, not Nop filters | `Services/StaffCustomerCheckout/`, checkout filters | policy-driven checkout and pricing services |
| Pickup collection | real delivery option, but better expressed through fulfillment integrations | `Services/PickUpCollection/`, pickup controllers/components | fulfillment or BOPIS-style integration |
| Activity auditing | important for operations and compliance | `Services/NewActivityLogs/`, page/order/login events | centralized event and audit stream |
| URL routing and slug behavior | SEO and redirects matter, but should be rehomed in storefront routing strategy | `Infrastructure/AnniqueSlugRouteTransformer.cs`, `Infrastructure/RouteProvider.cs` | Shopify routing and redirect layer |

## Cross-Cutting Plugin Concerns

### Overridden core services

The plugin replaces core Nop services for product, pricing, discounts, auth, cart, addresses, and order reporting.

That means business logic is not isolated in one feature area. It is woven into the platform service graph.

### Action-filter interception

Checkout, cart, payment, address, admin order, customer info, and registration behavior are intercepted via action filters.

That means request-level behavior must be extracted as explicit policies and workflow rules during migration.

### Event-driven side effects

Order, login, logout, page rendering, and cart events trigger downstream logic.

That means a future state must support business events, not only synchronous controller actions.

### Settings and feature flags

The plugin exposes a large feature-flag surface covering OTP, pickup, chatbot, shipping, registration, promotions, and third-party integrations.

That means Dieselbrook should separate permanent business rules from optional, toggle-driven features.

## Migration Guidance

1. Rebuild bucket 1 features as first-class Shopify plus middleware capabilities.
2. Force a business decision on every bucket 2 feature before implementation starts.
3. Re-home bucket 3 features into purpose-built services rather than reproducing Nop plugin architecture.
4. Treat overridden services, action filters, and event consumers as high-risk logic concentration points during detailed mapping.