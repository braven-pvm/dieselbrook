# Hosting Topology Notes

- Live AM SQL target in source: 172.19.16.100 (private IP, private routing only).
- Live NOP/public middleware tier points at 20.87.212.38; RDAP owner returned MSFT, so Microsoft-hosted/Azure-like public tier is strongly evidenced.
- Confirmed: nopintegration has direct SQL access to live AM and live NOP.
- Confirmed in staging: AccountMate and NopIntegration DB share 196.3.178.122,62111.
- Unproven for live: same server, same SQL instance, same LAN, and exact AM hosting class (on-prem vs private cloud VM).
- Verification doc created: docs/07_hosting_certainty_matrix.md.
- New evidence from stakeholder topology diagram (2026-03-11): Azure cloud tier contains NopCommerce, Nop SQL, AnniqueAPI, and Nopintegration; private/on-prem tier contains AMSERVER-v9, AccountMate application, data warehouse, and IP-restricted SQL databases. Existing private routing connects Azure to on-prem. CONFIRMED: Dieselbrook middleware deploys in Azure and joins the existing routing path.
- A-09 (Azure middleware deployment assumption) is now CONFIRMED — no longer an assumption.
- X-DEP-06 (hosting topology dependency) is RESOLVED.
