# Pricing Risk Notes

- Custom Nop plugin pricing behavior is concentrated in overridden services, discount allocation, cart logic, checkout filters, special offers, and exclusive-item/product visibility rules; it should be treated as a high-risk replacement zone, not a simple discount migration.
- Newly received NISource/campaign module is directly relevant to campaign pricing administration: it calls sp_Camp_CatSummary, sp_Camp_SkuByMonthvert, sp_Camp_BrandByMonth, sp_Camp_GetCamp, and sp_Camp_GetSponTypes, and appears to manage Campaign/CampDetail/CampSku data plus stage/Namibia publishing actions.
- Current snapshot of NISource/campaign looks partial because callback wiring and CampData class definitions are not yet visible elsewhere in the repo; treat it as relevant but incomplete until the rest of the module is confirmed.
- Campaign module gap analysis confirms the delivered campaign zip is not standalone: missing _layoutpageVue.wcs, shared scripts/helpers, callback routing glue, and CampData definitions.
- Current working assumption: NISource contains much of the important middleware source, but not a complete self-sufficient snapshot of the whole nopintegration web application, especially on the browser-facing/admin shell side.

## Post-archaeology findings (2026-03-09)

- AM pricing current-state is centered on Campaign/CampDetail + icitem fallback. Legacy socamp/sosppr are NOT the live pricing architecture — they are lineage only.
- sp_camp_getSPprice is the confirmed effective-price oracle for consultant pricing parity tests. Logic: returns active CampDetail.nPrice if a campaign row is valid for the order date, else icitem.nprice.
- sp_ct_updatedisc resolves consultant arcust.ndiscrate to 20 in all observed branches — the model is effectively a flat 20% consultant discount with campaign overrides.
- CampDetail supports delta sync via dLastUpdate. Time-window boundary sweeps (every 5-15 min) are ALSO mandatory because dFrom/dTo transitions can activate/expire prices without a new write.
- Preferred architecture: precomputed effective consultant prices stored as Shopify product metafields; deterministic Shopify Function applies fixed discount delta (retail minus synced consultant price) at checkout. Do NOT make the Function a full pricing engine.
- Shopify Function availability constraint: custom apps containing Functions = Shopify Plus only. Public App Store apps = any plan. App distribution model is the key variable — not B2B.
- Shopify B2B native tooling is NOT the recommended path. The decision gate (D-06) is about plan tier and app distribution model only.
- Open items from pricing archaeology: OI-8 (public-app Functions viability on Advanced), OI-9 (VAT-inclusive or exclusive sync), OI-10 (negative-price voucher rows mapping), OI-11 (parity harness: middleware vs sp_camp_getSPprice).
