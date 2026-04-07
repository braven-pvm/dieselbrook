# Reporting Domain Deep Dive

*Ordered by: D-10 closure — 2026-03-15*
*Reporting deep dive triggered to classify all 52 NopReports entries*
*Status: Framework established; Annique confirmation of production report usage required to finalise migration decisions*

---

## 1. Purpose

This document executes the D-10 ordered action: classify all existing NopCommerce/NISource reporting assets to determine which reports should migrate to DBM, which remain in AM, and which are hybrid.

The objective is to prevent two failure modes:
- **Overbuilding**: migrating accounting and MLM reports that belong in AM and will never be used by an ecommerce operator through DBM
- **Under-scoping**: leaving consultant self-service or operational ecommerce reports out of DBM scope, creating a gap on go-live

---

## 2. Classification Framework

Four classification categories apply to every report in the NopReports estate:

| Category | Definition | Migration Decision |
|---|---|---|
| **AM-only** | Report reads exclusively from ERP/MLM data (commission, accounting, downline hierarchy, rebates, titles). Output is consumed by Annique internal staff via AM or back-office tooling. Ecommerce operators and consultants do not need this through DBM. | Stay in AM. Do not migrate to DBM. |
| **Ecommerce** | Report reads from order, product, or storefront data. Output is relevant to ecommerce operations or consumer-facing reporting. Has no meaningful MLM/commission dimension. | Migrate to DBM or native Shopify reporting. |
| **Hybrid** | Report reads from both ERP/MLM data and order/ecommerce data, or its output is consumed by both AM-side operators and DBM-side users. | Partial migration: DBM provides ecommerce-facing slice; AM retains ERP-facing slice. |
| **Consultant self-service** | Report is exposed to individual consultants through the storefront (My Account area). Typically reads consultant-specific statement, downline, or order history. | Migrate methodology to DBM consultant portal. Data may still come from `compplanLive` via DBM service layer. |

---

## 3. Evidence Base

### 3.1 Report infrastructure — what powers reporting

**FoxPro/NISource layer:**

- `NISource/reports.prg` — dynamic report metadata lookup, parameter hydration, and report rendering dispatch
- `NISource/apiprocess.prg` — routes HTTP report requests to `reports.prg`

**SQL stored procedures (report-backing):**

From `amanniquelive` (confirmed on staging):

| Procedure | Function | Am-side or DBM-relevant? |
|---|---|---|
| `sp_ct_statement` | Generates consultant commission/comp plan statement | AM-only: MLM commission data |
| `sp_ct_statementALL` | Bulk statement generation | AM-only: MLM commission data |
| `sp_ct_downlinebuild` | Builds downline hierarchy snapshot | AM-only: MLM hierarchy |
| `sp_ct_groupbuild` | Group structure computation | AM-only: MLM hierarchy |
| `sp_ct_titlebuild` | MLM title computation | AM-only: MLM hierarchy |
| `sp_ct_Rebates` | Rebate computation | AM-only: MLM commission |
| `sp_ct_updatedisc` | Discount update for comp plan | AM-only: MLM |
| `sp_alert_orderstats` | Daily order statistics by type (portal, typed, first kit) | Hybrid: operational + ecommerce |
| `sp_alert_rateofsale` | Rate-of-sale tracking | Ecommerce/operational |
| `sp_alert_invoiceTracking` | Invoice status tracking | AM-side: fulfilment monitoring |
| `sp_alert_mkMTDSMS` | Marketing MTD SMS stats | AM-side: marketing ops |
| `sp_alert_MTDSMS` | Monthly SMS statistics | AM-side: marketing ops |
| `sp_report_sales` | Sales report | Hybrid: depends on consumer |
| `sp_Camp_CatSummary` | Campaign category summary | Campaign admin — DBM admin console |
| `sp_Camp_BrandByMonth` | Brand-by-month campaign view | Campaign admin — DBM admin console |
| `sp_Camp_SkuByMonthVert` | SKU-by-month vertical | Campaign admin — DBM admin console |
| `sp_Camp_Full` | Full campaign view | Campaign admin — DBM admin console |
| `sp_Camp_GetCamp` | Get campaign detail | Campaign admin — DBM admin console |
| `sp_bi_updatesalesmainf` | BI sales main fact table update | AM-only: BI/data warehouse layer |

**NopIntegration support views (from `06_views.csv`):**

| View | Likely category |
|---|---|
| `BDMReport` | Hybrid: BDM-level (Business Development Manager) consultant report |
| `MJM CR Income Report` | AM-only: MLM income/credit report |
| `MJM Credit Report` | AM-only: MLM credit statement |
| `MJM Income Report` | AM-only: MLM income |
| `MJM New Beate Report` | AM-only: special consultant programme report |
| `MJM Reregistration Open Sales OrderReport` | Hybrid: re-registration linked to orders |
| `Mo_High_Level_Report_Bucket` | Operational: high-level ops summary |
| `Mo_OpsHighLevelReport` / `_v1` | Operational: ops dashboard — hybrid |
| `RMA_Report` | AM-only: returns/RMA tracking |
| `SL_vw_Berco_freight_Report` | AM-only: freight-side report (logistics partner) |
| `vm_mk_creditreport` | AM-only: marketing credit report |
| `vm_mk_DownlineReport` | AM-only: downline marketing view |
| `Xen_Void_Report` | AM-only: voided order/transaction |

**Plugin components (NopCommerce):**

- `Controllers/AdminAnniqueReportController.cs` — admin report management UI
- `Services/AnniqueReport/` — report services
- `Factories/AnniqueReport/` — report factories
- Report ACL/menu management and parameter rendering

---

## 4. Report Category Classification

### Category A: AM-Only (Remain in AccountMate — Do Not Migrate)

These reports read exclusively from MLM commission, accounting, or ERP inventory data. They are consumed by Annique internal operations staff via AM or dedicated back-office tooling. DBM does not need to replicate these.

| Report / Source | Data driven from | Reason |
|---|---|---|
| Consultant commission statement (`sp_ct_statement`) | `compplanLive.CTstatement`, `CTstatementh` | Pure MLM compensation; AM is SoT |
| Bulk statement (`sp_ct_statementALL`) | `compplanLive` | MLM; AM is SoT |
| Downline build / hierarchy (`sp_ct_downlinebuild`, `sp_ct_groupbuild`) | `CTdownlineh`, `CTcomp`, `CTcomph`, `CTconsv` | MLM hierarchy computation |
| Title build / qualification (`sp_ct_titlebuild`) | `CTRunData`, MLM qualification tables | MLM titles; AM is SoT |
| Rebates (`sp_ct_Rebates`) | `compplanLive` rebate structures | MLM compensation |
| Discount update (`sp_ct_updatedisc`, `sp_ct_updateportaldisc`) | `compplanLive` discount parameters | MLM/comp plan; not ecommerce |
| Invoice tracking (`sp_alert_invoiceTracking`) | `arinvc`, fulfilment tables | AM-side fulfilment monitoring |
| Marketing SMS stats (`sp_alert_mkMTDSMS`, `sp_alert_MTDSMS`) | AM-managed messaging campaign records | AM marketing ops, not DBM |
| RMA Report | Returns/RMA tables | AM fulfilment, not ecommerce portal |
| Freight report (`SL_vw_Berco_freight_Report`) | Logistics partner data | AM ops |
| Marketing credit report (`vm_mk_creditreport`) | Marketing credit tables | AM marketing ops |
| Downline marketing view (`vm_mk_DownlineReport`) | `compplanLive` | MLM hierarchy, AM-only |
| Void report (`Xen_Void_Report`) | Void/cancelled order records | AM accounting |
| BI sales update (`sp_bi_updatesalesmainf`) | ERP BI fact tables | AM BI / data warehouse layer |
| MJM income/credit reports | MLM income/credit tables | AM/MLM stakeholder reports |
| TREPORT / treportt / ttreport tables | 8369 / 10678 / 191541 rows — AM-side report state tables | Retain in AM; DBM does not read |

---

### Category B: Ecommerce (Migrate to DBM or Shopify Analytics)

These reports are relevant to ecommerce operations — product performance, order throughput, conversion, pick-pack stats. They can be served from DBM middleware data or Shopify's native analytics.

| Report / Source | Data driven from | Migration path |
|---|---|---|
| Order status / My Orders (storefront) | Shopify order data + DBM fulfilment state | Shopify native order history + DBM-linked status |
| Rate of sale (`sp_alert_rateofsale`) | Order lines × time | DBM can replicate from Shopify order data |
| Product sales report (`sp_report_sales`) | Order lines + SKU | DBM analytics or Shopify reporting |
| Campaign category / brand / SKU summary (`sp_Camp_CatSummary`, `sp_Camp_BrandByMonth`, `sp_Camp_SkuByMonthVert`, `sp_Camp_Full`) | Campaign + order data | DBM admin console reporting; campaign admin surface |
| Storefront product visibility reports | Campaign + product metafield state | DBM admin console |

---

### Category C: Hybrid (Partial Migration)

These reports combine ERP and ecommerce data. DBM provides the ecommerce-facing slice; AM retains or continues serving the ERP/MLM-facing slice.

| Report / Source | DBM slice | AM slice |
|---|---|---|
| Order stats (`sp_alert_orderstats`) | Web order counts, portal order throughput by day | Typed (walk-in/call-centre) order stats — stays in AM |
| BDM Report (`BDMReport` table/view) | BDM trading activity relevant to ecommerce | BDM downline/upline context — AM |
| Re-registration order report (`MJM Reregistration...`) | Re-registration orders from ecommerce channel | Re-registration commission impact — AM |
| High-level ops report (`Mo_OpsHighLevelReport`) | Web orders, conversion summary | Non-web order volumes — AM |
| Sales report with consultant context | Ecommerce order lines with consultant pricing applied | Commission attribution, comp plan impact — AM |

---

### Category D: Consultant Self-Service (Migrate to DBM Consultant Portal)

These reports are currently exposed through the NopCommerce My Account area and the `AdminAnniqueReportController`. Individual consultants access their own data. These must migrate to the DBM consultant portal.

| Report type | Current surface | DBM migration target |
|---|---|---|
| My order history | NopCommerce My Account → Orders | Shopify order history (native) + DBM fulfilment status overlay |
| My consultant statement | NopCommerce My Account → Reports (drives `sp_ct_statement`) | DBM consultant portal, reads `compplanLive` via `ConsultantReportingDataService` |
| My downline summary | NopCommerce My Account → Reports (drives `sp_ct_downlinebuild`) | DBM consultant portal, reads `compplanLive` via service |
| Sponsoring/recruitment report | NopCommerce My Account → Reports | DBM consultant portal |
| My campaign/exclusive item history | NopCommerce My Account → Campaign items | DBM portal, driven from DBM campaign and order data |

**Note:** The data for consultant self-service reports (statement, downline) comes from `compplanLive`, which remains the SoT for MLM data. DBM's `ConsultantReportingDataService` is the intermediary: it reads from `compplanLive` via the SQL access gateway and returns results to the DBM portal. DBM does not own this data — it mediates read-only access.

---

## 5. NopReports Table — Status and Action Required

The `NopIntegration..NopReports` table contains 52 report definitions with associated:
- Report name and system name
- Parameter metadata
- ACL and menu settings
- Public vs admin report flags

**Current state:** The actual row-level records from `NopReports` have not been exported or catalogued in the current analysis estate. The `02_columns.csv` in `docs/DB Structure/` reflects the `amanniquelive` and related databases, not `NopIntegration`.

### Action required

Annique IT must provide a dump of the 52 `NopReports` records, specifically:

```sql
SELECT Id, Name, SystemName, IsPublished, DisplayOrder, IsPublicReport
FROM NopIntegration..NopReports
ORDER BY DisplayOrder
```

For each record, Annique Operations must confirm:
- Is this report actively used in production today? (Yes / No / Unknown)
- Who uses it? (Consultant self-service / Operations staff / Annique management / IT only)
- How frequently? (Daily / Monthly / Campaign-cycle / Never)

This confirmation directly determines phase-1 reporting scope.

---

## 6. Phase-1 Reporting Scope Recommendation

Based on the classification above and the programme constraint (go-live end July 2026), the recommended phase-1 reporting scope is:

### Include in phase 1

| Scope item | Rationale |
|---|---|
| Shopify native order history for consultants and consumers | Shopify provides this natively; zero DBM build cost |
| DBM fulfilment status overlay on order history | Required for consultant trust in delivery status |
| Consultant statement read-only (via `ConsultantReportingDataService`) | Consultants need to see their commission statement in the portal |
| Downline summary read-only | Consultants need basic downline visibility to self-manage their network |
| Campaign and order throughput report in DBM admin console | Operations need visibility into ecommerce channel performance |
| Basic web order stats (replacement for web-channel slice of `sp_alert_orderstats`) | Operations need daily order volume visibility |

### Defer to later phase

| Scope item | Rationale |
|---|---|
| Full consultant statement with drill-down | Phase-1 parity: summary is sufficient for day one |
| Deep downline hierarchy navigation | MLM hierarchy navigation is a later-phase feature |
| Marketing analytics (MTD, rate-of-sale) | DBM admin analytics sufficient; dedicated BI comes later |
| BDM/management reporting from DBM | Management reports stay in AM in phase 1 |
| Admin report metadata management in DBM admin console | Low priority; can be managed via direct data access in phase 1 |

### Remain in AM permanently (no DBM migration)

All category A reports listed in section 4. DBM does not replicate AM accounting, commission, or full MLM reporting.

---

## 7. Risks

| Risk | Severity | Mitigation |
|---|---|---|
| Annique cannot confirm which NopReports records are actively used | High | Fallback: treat all 52 as potentially active; classify by metadata and SQL backing; confirm with usage triage session |
| Consultant self-service reports depend on compplanLive reads at runtime | Medium | DBM SQL access gateway must be confirmed to have read access to `compplanLive`; see A-08 |
| Dynamic metadata-driven report execution in `reports.prg` is opaque | Medium | Source code access (recommended in D-24 AM interface doc) helps; otherwise reverse-engineer from network capture and test data |
| Phase-1 scope creep from stakeholders expecting all 52 reports in DBM | High | Use this classification to negotiate: AM-only reports stay in AM; consultant portal provides what consultants need |

---

## 8. Decision and Dependency References

| Reference | Status | Notes |
|---|---|---|
| D-10 Operationally used reports | Closed 2026-03-15 — ordered this deep dive | `docs/analysis/02_open_decisions_register.md` |
| X-DEP-04 Confirmation of used reports | Partially resolved | Annique confirmation of NopReports row usage required |
| A-02 Preserve ERP/MLM truth in phase 1 | Active | AM-only reports are consistent with this assumption |
| A-08 Private SQL connectivity | Active | `ConsultantReportingDataService` requires `compplanLive` access |
| D-09 Admin console | Closed 2026-03-15 | Campaign and order throughput reports live in DBM admin console |
