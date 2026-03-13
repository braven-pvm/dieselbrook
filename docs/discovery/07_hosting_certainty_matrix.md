# Hosting Certainty Matrix

This document separates what is confirmed, what is strongly evidenced, and what is still unknown about the current Annique hosting and network topology.

## Current Bottom Line

- Confirmed: `nopintegration.annique.com` has direct SQL access to live AccountMate at `172.19.16.100` and direct SQL access to NopCommerce at `20.87.212.38,63000`.
- Confirmed: the live AccountMate target is a private IP, so it is behind private routing and is not directly internet-exposed.
- Strongly evidenced: the public NopCommerce and middleware tier is hosted on Microsoft infrastructure, which is consistent with Azure hosting.
- Strongly evidenced: a stakeholder-supplied topology diagram places NopCommerce, Nop SQL Server, `AnniqueAPI` middleware, and `Nopintegration` middleware in an Azure cloud tier, with AccountMate and related data remaining on a private/on-premises side.
- Not confirmed: whether live AccountMate (`172.19.16.100`) is on-premises, an Azure VM on a private network, or another privately hosted server.
- Not confirmed: whether `nopintegration.annique.com` is on the same machine, same SQL instance, or same LAN as live AccountMate.

## New Evidence Added On 2026-03-11

- A stakeholder-supplied topology diagram shows an `Azure Cloud Server` containing:
	- NopCommerce
	- Nop SQL Server
	- `AnniqueAPI Middleware` for consultant verification and Brevo communication
	- `Nopintegration Middleware` for AccountMate sync, reports, and communication
- The same diagram shows an `On-Premises Server` side containing:
	- `AMSERVER-v9`
	- AccountMate application
	- data warehouse server
	- `Remote SQL Server (IP Restricted)`
	- `AccountMate DB`
	- `Related DB`
- The diagram also explicitly separates the two middleware responsibilities:
	- `AnniqueAPI` = consultant verification and Brevo communication
	- `Nopintegration` = AccountMate sync, reports, and communication

This is useful architectural evidence, but it is still a stakeholder diagram rather than direct infrastructure proof such as Azure portal access, VM inventory, firewall rules, or VPN configuration.

## Status Legend

- Confirmed: directly proven from source, live SQL, or public DNS/IP evidence.
- Strongly evidenced: multiple signals point the same way, but we do not have direct infrastructure proof.
- Unknown: plausible, but not proven.

## Certainty Matrix

| Topic | Current status | Evidence | What we can safely say now | What would verify it conclusively |
|---|---|---|---|---|
| `nopintegration.annique.com` can connect directly to live AccountMate | Confirmed | `NISource/syncproducts.prg` contains `SERVER=172.19.16.100;...database=amanniquelive`; discovery notes also show `oAMSql -> SQL Server 172.19.16.100` | The middleware has direct SQL connectivity into live AccountMate | Successful live login test from the middleware host, or server-side config export |
| `nopintegration.annique.com` can connect directly to live NopCommerce SQL | Confirmed | `NISource/syncproducts.prg` contains `SERVER=20.87.212.38,63000;...database=annique`; discovery notes show `oNopSql -> SQL Server 20.87.212.38,63000` | The middleware talks straight to the NopCommerce SQL instance as well | Successful live login test from the middleware host, or server-side config export |
| Live AccountMate SQL host `172.19.16.100` is private-only | Confirmed | `172.19.16.100` is an RFC1918 private IP; it is not a public-routable address | Live AM is behind internal/private networking of some kind | Hostname, network diagram, or routing/VPN documentation |
| Current production topology is split into a cloud commerce tier and a private ERP/data tier | Strongly evidenced | Source confirms direct SQL connectivity to both sides; the stakeholder topology diagram places Nop/Nop SQL/middleware in Azure and AccountMate/data systems on the private side | The current design should be treated as a split architecture, not a single-host deployment | Azure resource list, server inventory, firewall/VPN configuration, or infrastructure owner confirmation |
| Public middleware/Nop tier is on Microsoft infrastructure | Strongly evidenced | `nopintegration.annique.com` resolves to `20.87.212.38`; RDAP ownership for `20.87.212.38` returned `MSFT` | The public node is very likely running on Microsoft-hosted infrastructure | Azure subscription/resource proof, VM metadata, or hosting invoice |
| Public middleware/Nop tier is specifically Azure | Strongly evidenced | Microsoft-owned public IP, Azure-aligned target architecture assumptions, stakeholder topology diagram showing an `Azure Cloud Server`, and no conflicting hosting evidence surfaced | Azure is the leading hypothesis for the public tier and now has documentary support | Portal screenshot, VM hostname metadata, Azure VM/NSG records |
| Live AccountMate SQL is on-premises | Unknown | Private IP fits on-prem, but private IPs also exist in cloud VNets and hosted private environments | On-prem is plausible, not proven | SQL host inventory, VPN/firewall map, Windows host details, or site IT confirmation |
| Live AccountMate SQL is an Azure VM/private Azure SQL host | Unknown | Private IP is compatible with an Azure VM in a VNet, but AccountMate relies on SQL Server features and surrounding logic that do not imply Azure SQL PaaS | A private Azure VM is plausible; Azure SQL PaaS is unlikely for the full current stack | SQL host metadata, Azure portal records, VM name, subnet/VNet proof |
| Live AccountMate SQL is AWS-hosted | Unknown | No AWS ownership, DNS, or source evidence has surfaced | AWS remains possible in theory, but currently unsupported by evidence | AWS account/resource proof or host metadata |
| Live middleware app and live AccountMate are on the same Windows server | Unknown | We have separate endpoints in source (`nopintegration.annique.com` vs `172.19.16.100`) but no host-level mapping | Same machine is not proven and should not be assumed | IIS host name, Windows hostname, or middleware server inventory |
| Live middleware app and live AccountMate use the same SQL Server instance | Unknown | For live, we only know middleware connects to `172.19.16.100`; we do not have a separate live `NopIntegration` DB connection string like staging | Same instance is possible but unproven for live | `SELECT @@SERVERNAME`, SQL instance inventory, or live connection string dump |
| Staging AccountMate and `NopIntegration` DB share the same SQL Server instance | Confirmed | Source shows `AccountMate (stage): SERVER=196.3.178.122,62111` and `NopIntegration: SERVER=196.3.178.122,62111` | In staging, both databases sit on the same SQL endpoint | SQL instance inventory on staging |
| Live middleware and live AccountMate are on the same LAN/private network | Strongly evidenced | Middleware reaches private AM SQL directly; no public SQL endpoint for live AM is exposed in source | Some private routing path exists between middleware and AM | Network diagram, VPN details, traceroute from middleware host, or firewall rules |
| Low-latency direct SQL access to AccountMate is a real design constraint | Confirmed | Discovery gate G5 explicitly treats LAN/private connectivity as a blocker for middleware platform decisions | Replacement middleware must be placed where it can reliably reach AM SQL | Ping/latency test and agreed production topology |

## What We Can Confirm Today

1. The current middleware is not a thin HTTP adapter. It opens direct SQL connections to both sides.
2. Live AccountMate is not directly exposed on a public IP. It sits behind private routing.
3. The public-facing middleware/Nop tier is very likely Microsoft-hosted and likely Azure-hosted.
4. The overall architecture is now strongly evidenced as a split model: cloud commerce/integration tier plus private ERP/data tier.
5. The exact hosting class of live AccountMate is still an infrastructure question, not a code question.

## Highest-Value Verification Checks

If Annique or infrastructure support can provide only a small amount of additional evidence, these checks will close the biggest gaps fastest:

1. From the live middleware host, capture hostname, private IP, and route to `172.19.16.100`.
2. From the live SQL host, capture `@@SERVERNAME`, Windows hostname, and network/subnet details.
3. Confirm whether a site-to-site VPN, private VNet peering, or on-prem LAN connection exists between the middleware host and AccountMate SQL.
4. Confirm whether `172.19.16.100` is physically in an Annique office/server room, a hosted VM, or a cloud VM.
5. Export the live middleware connection/config values, especially any non-source overrides for SQL targets.

## Impact Assessment

### What this changes

- It materially strengthens the working assumption that Dieselbrook should design for a split deployment topology rather than a single co-hosted server.
- It increases confidence that the public commerce and middleware side belongs in Azure or at least Azure-compatible Microsoft-hosted infrastructure.
- It reinforces that consultant-facing services and Brevo-related flows may sit on a different middleware boundary from the AccountMate synchronization layer.
- It supports treating private SQL connectivity into the ERP/data tier as a hard delivery constraint, not a speculative requirement.

### What this does not change

- It does not prove the exact hosting model of AccountMate.
- It does not prove the exact network mechanism between the cloud tier and the private ERP/data tier.
- It does not prove whether the Azure side is one VM, many VMs, App Services, or another resource layout.
- It does not reduce the need for direct infrastructure verification before final production deployment design.

### Practical impact on Dieselbrook delivery

- Low impact on the core architecture recommendation: the previously recommended middleware-first design still stands.
- Medium positive impact on environment planning: Azure-hosted middleware now looks like the right default assumption unless infrastructure evidence contradicts it.
- High impact on connectivity planning: private SQL reachability to AccountMate remains a gating dependency and should stay on the critical path.
- Medium impact on service decomposition: keep the option to separate commerce-facing middleware concerns from ERP-sync/reporting concerns.

## Recommended Working Assumption

Until infrastructure proves otherwise, the safest planning assumption is:

- public NopCommerce and middleware tier: likely Azure-hosted or Microsoft-hosted public VM infrastructure
- live AccountMate SQL: privately reachable SQL Server with unknown physical hosting model
- delivery constraint: any new Dieselbrook middleware must have stable private connectivity to AccountMate SQL before build decisions are finalized

That assumption is conservative and aligns with all currently verified evidence without over-claiming same-server, same-instance, or on-prem status.