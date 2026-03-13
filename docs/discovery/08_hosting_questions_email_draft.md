# Hosting Verification Email Draft

## Subject

Questions to Confirm Current Hosting and Network Topology for AccountMate / Middleware

## Draft Email

Hi team,

We are at the point where we need to confirm the current hosting and network topology around AccountMate, the existing middleware, and NopCommerce before we lock down the target Dieselbrook integration architecture.

To make this as easy as possible, we have pre-filled the items already known from the discovery work. Please complete only the `Answer / Confirmation` column and attach any supporting screenshot, diagram, or note where available.

From the source code and discovery work, we have already confirmed the following:

- the current middleware connects directly to live AccountMate SQL at `172.19.16.100`
- the current middleware also connects directly to the NopCommerce SQL environment at `20.87.212.38,63000`
- the live AccountMate SQL target is on a private IP, which means it sits behind private routing of some kind
- the public-facing middleware / NopCommerce tier appears to be hosted on Microsoft infrastructure, likely Azure, but we do not yet have infrastructure-level confirmation

Please complete the table below.

## Hosting Verification Table

| Area | Question | What we currently know | Answer / Confirmation | Evidence / Screenshot |
|---|---|---|---|---|
| AccountMate SQL host | What is the actual host or VM name for the live AccountMate SQL Server currently reached at `172.19.16.100`? | IP only is known from source and discovery. |  |  |
| AccountMate SQL host | Is the live AccountMate SQL Server on-premises, in Azure, hosted by a third party, or elsewhere? | Current status unknown. |  |  |
| AccountMate SQL host | If cloud-hosted, which provider and service model is used? | Microsoft-hosted public infrastructure is suspected for the public tier, but not confirmed for AccountMate. |  |  |
| AccountMate SQL host | Is `172.19.16.100` the SQL Server itself, or an internal address that fronts another host or instance? | Current status unknown. |  |  |
| Middleware host | What server or VM currently hosts `nopintegration.annique.com`? | Public hostname is known; exact host or VM name is not. |  |  |
| Middleware host | Is the middleware on the same machine as AccountMate, on a separate machine in the same private network, or in a separate hosted environment? | Middleware has direct SQL access to live AccountMate, but topology is not confirmed. |  |  |
| Middleware host | Is the middleware hosted in Azure, on-premises, or elsewhere? | Public tier appears Microsoft-hosted, likely Azure, but not confirmed. |  |  |
| Middleware host | What is the hostname and private IP of the middleware server? | Current status unknown. |  |  |
| Connectivity | How does the middleware reach `172.19.16.100`? | Private connectivity exists because the middleware connects to a private IP. |  |  |
| Connectivity | Is this same LAN, site-to-site VPN, private peering, hosted private network, or another mechanism? | Current status unknown. |  |  |
| Connectivity | Are there firewall allow-lists, NAT, NSGs, or other network rules specifically enabling this connection? | Current status unknown. |  |  |
| Connectivity | Is the connection low-latency and stable enough for direct SQL use during business hours? | Current design assumes yes, but this is not formally confirmed. |  |  |
| SQL instance layout | Does the live AccountMate SQL Server also host any integration databases, such as a production equivalent of `NopIntegration`? | In staging, `AccountMate` and `NopIntegration` share the same SQL endpoint. Live is not confirmed. |  |  |
| SQL instance layout | Are AccountMate and any integration databases on the same SQL Server instance in production? | Confirmed in staging only; unconfirmed in live. |  |  |
| SQL instance layout | If not, which SQL instances or servers are involved in production? | Current status unknown. |  |  |

## Evidence That Would Help Most

Any of the following would be enough to help us verify the environment quickly:

- VM or server name for the live AccountMate SQL host
- VM or server name for the middleware host
- private IP and subnet details for both hosts
- a simple network diagram showing how the middleware reaches AccountMate SQL
- confirmation of whether the environment is on-prem, Azure, or another hosted platform
- confirmation of any VPN, VNet, firewall, or private routing path used
- a screenshot from the hosting portal or infrastructure inventory showing the relevant server(s)

## Why We Need This

These answers directly affect:

- where the new Dieselbrook middleware can be hosted
- whether direct SQL access to AccountMate is viable from a cloud-hosted solution
- whether we need VPN/private connectivity as part of the project scope
- whether we should design for LAN-adjacent deployment rather than public cloud deployment

Thanks,

[Your Name]

## Minimal Reply Template

If they still do not want to fill in the full table, ask them to complete only this:

| Item | Answer |
|---|---|
| Live AccountMate SQL host name |  |
| Live AccountMate hosting type: on-prem / Azure / other |  |
| Live AccountMate private IP |  |
| Middleware host name |  |
| Middleware hosting type: on-prem / Azure / other |  |
| Middleware private IP |  |
| Relationship between middleware and AccountMate: same server / same LAN / VPN / other |  |
| Production SQL instances involved |  |
| Any firewall or private network requirements |  |
| Any diagram or screenshot attached: yes / no |  |

