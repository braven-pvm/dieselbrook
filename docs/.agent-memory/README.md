# Agent Memory — Dieselbrook Workspace

This directory contains portable agent memory files for the Annique → Shopify programme. These files are committed to the repo so they survive machine changes, re-clones, and agent restarts.

## Files

| File | Contents |
|---|---|
| `analysis-phase-notes.md` | Current synthesis phase status, doc inventory, sequencing rules |
| `environment-access-notes.md` | Staging SQL access facts, confirmed DB row counts, doc pointers |
| `hosting-topology-notes.md` | Confirmed Azure+on-prem split topology, confirmed/resolved assumptions |
| `notion-page-ids.md` | Notion page IDs for all maintained programme pages |
| `order-flow-notes.md` | Confirmed order lifecycle, idempotency facts, staging order estate |
| `pricing-risk-notes.md` | Pricing archaeology findings, recommended architecture, open items |

## Session Logs

Individual session summaries are stored under `sessions/`. Each file covers the key decisions, discoveries, and actions from one working session.

## Usage Rule

A new agent starting work on this repository should:
1. Read `docs/analysis/06_future_agent_onboarding.md` first
2. Then read all files in this directory
3. Then read `docs/analysis/02_open_decisions_register.md` for current open decisions
4. Then read the relevant domain pack(s) for the area of work
