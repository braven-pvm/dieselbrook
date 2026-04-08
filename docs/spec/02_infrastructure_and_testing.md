# Infrastructure, Environments & Testing Strategy
## Dieselbrook Middleware (DBM) — Annique Cosmetics Platform Modernisation

**Document type:** Infrastructure & Testing Specification  
**Programme:** Annique → Shopify Plus migration and Dieselbrook Middleware build  
**Version:** 0.1 — initial  
**Date:** 2026-04-07  
**Status:** Draft  
**Linear:** ANN-17  

---

## Decision Dependencies

| Decision ID | Topic | Status | Impact If Still Open |
|---|---|---|---|
| D-06 | Shopify plan tier | ✅ Closed 2026-03-15 | Was: Shopify Plus required for dev store parity and Functions testing |
| TC-02 | Go-live date and cutover model | ✅ Closed 2026-03-15 | Was: affected environment promotion timeline |
| All other decisions | — | ✅ All closed 2026-03-15 | — |

This specification is fully writeable. No open decisions block it.

---

## 1. Strategy Overview

### 1.1 The core testing problem

DBM replaces a production system that has been running for 15+ years. The risk is not that DBM fails to start — it is that DBM produces subtly wrong outputs that are not obviously wrong: an order written to AM with the wrong price, a consultant sync that drops an entitlement flag, a pricing sweep that misses a campaign boundary.

The only defence against this class of failure is **behavioural parity verification**: given the same inputs (same Shopify event, same AM database state), DBM must produce the same outputs as the legacy system — or a provably correct improvement over it.

This defines the testing strategy. Every environment, every test suite, every CI gate exists to answer the question:
> *"Does DBM behave correctly relative to the legacy system for the same inputs?"*

### 1.2 Key principles

1. **Environments are code.** Every environment is provisioned from IaC (Bicep). No snowflake configuration. A fresh environment can be provisioned in minutes from a single command.
2. **AM is reproducible.** The staging AM instance can be reset to a known snapshot at any time. Tests that depend on AM state are deterministic because they control the starting state.
3. **TDD is mandatory.** No service implementation is written without a test file first. The test defines the contract; the implementation satisfies it.
4. **Parity is a first-class test suite.** Pricing parity, order write-back parity, and sync correctness are automated and run on every significant AM snapshot.
5. **Production AM is approximated in Azure from day one.** We do not wait for a formal AM migration project. An Azure-hosted AM instance is provisioned as part of the staging environment and seeded from the existing production backup. DBM staging connects to this instance — it is the primary integration target.

---

## 2. Environment Architecture

### 2.1 Environment tiers

Four tiers, each with a distinct purpose and access model:

| Tier | Name | Purpose | AM instance | Shopify store |
|---|---|---|---|---|
| 1 | `local` | Developer workstation. Unit tests, manual smoke testing. | None (mocked via test doubles) | None (Shopify API mocked) |
| 2 | `staging` | Full integration. All services run. Used for feature testing, sprint reviews, and parity runs. | Azure IaaS VM — seeded from production backup (`staging-am`) | Shopify Plus development store (`annique-staging`) |
| 3 | `parity` | Dedicated environment for parity harness runs. Isolated from sprint activity. AM reset from snapshot before each run. | Azure IaaS VM — reset from golden snapshot before each parity run (`parity-am`) | Shopify Partner dev store (`annique-parity`) — reset between runs |
| 4 | `production` | Live environment post go-live. | Production AM (Azure IaaS VM after migration, or on-prem until then) | Live Shopify Plus store |

> **Why separate `parity` from `staging`?** Parity runs require a known AM baseline. A shared staging AM is mutated by ordinary feature testing and sprint work, making parity comparisons unreliable. The parity environment is only written to by automated parity runs — never manually.

### 2.2 Environment topology diagram

```
┌───────────────────────────────────────────────────────┐
│  DEVELOPER WORKSTATION  (local)                       │
│                                                       │
│  dotnet run / docker compose up                       │
│  ─ DBM Core (all services, reduced schedule)          │
│  ─ Azure SQL Local (Docker: mcr.microsoft.com/mssql)  │
│  ─ Azurite (Service Bus emulator)                     │
│  ─ No AM connection — test doubles inject AM responses│
└───────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────────┐
│  AZURE  ·  staging resource group  (dbm-staging-rg)               │
│                                                                   │
│  App Service — DBM Core (staging slot)                            │
│  App Service — Admin Console (staging slot)                       │
│  Azure SQL — DBM State DB (staging)                               │
│  Azure Service Bus (staging namespace)                            │
│  Azure Key Vault (staging vault)                                  │
│  Application Insights (staging workspace)                         │
│                                                                   │
│  Azure IaaS VM — staging-am                                       │
│  └─ Windows Server 2022, SQL Server 2022 Standard                 │
│  └─ amanniquelive (restored from production backup)               │
│  └─ compplanLive · compsys (restored from production backup)       │
│  └─ Private endpoint — only reachable from staging-rg VNet        │
│                                                                   │
│  Shopify dev store: annique-staging (Shopify Partner)             │
└───────────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────────┐
│  AZURE  ·  parity resource group  (dbm-parity-rg)                 │
│                                                                   │
│  App Service — DBM Core (parity slot) — reset between runs        │
│  Azure SQL — DBM State DB (parity) — reset between runs           │
│  Azure Service Bus (parity namespace)                             │
│  Azure Key Vault (parity vault)                                   │
│                                                                   │
│  Azure IaaS VM — parity-am                                        │
│  └─ Windows Server 2022, SQL Server 2022 Standard                 │
│  └─ Databases restored from golden snapshot before each run       │
│                                                                   │
│  Shopify dev store: annique-parity — reset between runs           │
└───────────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────────┐
│  AZURE  ·  production resource group  (dbm-prod-rg)               │
│                                                                   │
│  App Service — DBM Core (production)                              │
│  App Service — Admin Console (production)                         │
│  Azure SQL — DBM State DB (production)                            │
│  Azure Service Bus (production namespace)                         │
│  Azure Key Vault (production vault)                               │
│  Application Insights (production workspace)                      │
│                                                                   │
│  Azure IaaS VM — production-am                                    │
│  (or on-prem AMSERVER-v9 via private routing until migration)     │
│                                                                   │
│  Live Shopify Plus store                                          │
└───────────────────────────────────────────────────────────────────┘
```

### 2.3 VNet and private connectivity

Each Azure resource group (`staging`, `parity`, `production`) has its own VNet. The AM IaaS VM is placed in a dedicated subnet within that VNet with no public endpoint. DBM App Service uses VNet Integration to reach the AM subnet directly via private IP.

This mirrors the confirmed existing Annique topology where NopCommerce/AnniqueAPI reach `AMSERVER-v9` via private routing. DBM joins the same connectivity model.

---

## 3. AccountMate in Azure — IaaS Specification

### 3.1 Decision basis

AM cannot run on Azure SQL PaaS — confirmed by vendor. The correct Azure path is **IaaS: Windows Server 2022 VM + SQL Server 2022 Standard**.

For the purposes of this programme, we assume and provision Azure-hosted AM from the start — for both staging and production. This is not dependent on a formal AM cloud migration project. The staging VM can be provisioned and seeded immediately.

### 3.2 AM IaaS VM specification

| Parameter | Value |
|---|---|
| VM SKU | `Standard_D4s_v5` (4 vCPU, 16 GB RAM) — sized for staging/parity; upgrade for production |
| OS | Windows Server 2022 Datacenter |
| SQL Server | SQL Server 2022 Standard (or Developer edition for staging/parity) |
| Storage | 256 GB Premium SSD P15 OS disk + 512 GB Premium SSD P20 data disk |
| Networking | Private subnet, no public IP, NSG allows SQL (1433) from DBM subnet only |
| Backup | Azure Backup VM policy — daily backup, 30-day retention (staging); 90-day retention (production) |
| SQL compatibility | Set `COMPATIBILITY_LEVEL = 150` (SQL Server 2019) for initial deployment; validate AM function before upgrading |

### 3.3 Database seed and reset workflow

The AM VM must be seedable from a production backup at any time. The reset workflow:

```
scripts/am/
  seed-staging-am.ps1      # Restores production backup → staging-am
  reset-parity-am.ps1      # Restores golden snapshot → parity-am (used before each parity run)
  take-golden-snapshot.ps1 # Captures parity-am state as the new golden snapshot
  verify-am-connection.ps1 # Confirms DBM can reach AM and the required SPs are accessible
```

**Seed procedure (staging-am):**
1. Take a production SQL Server backup (`.bak` file) — coordinated with Annique IT.
2. Upload to Azure Blob Storage (`dbm-am-backups` container, access tier: cold).
3. Run `seed-staging-am.ps1` — restores backup to the staging-am SQL instance, renames databases if needed, applies the `dbm_svc` login and permission grants.
4. Run `verify-am-connection.ps1` — confirms all required SPs are callable and return expected structures.

**Reset procedure (parity-am):**
1. Restore the golden snapshot via `reset-parity-am.ps1`.
2. Drop and recreate the DBM State DB on the parity Azure SQL instance.
3. Flush the parity Service Bus queues.
4. Result: a clean parity environment with known AM baseline, ready for a parity run.

### 3.4 `dbm_svc` SQL login provisioning

The `seed-staging-am.ps1` script applies the least-privilege service account after each restore:

```sql
-- Applied by seed script after every restore
CREATE LOGIN dbm_svc WITH PASSWORD = '<from Key Vault>';
USE amanniquelive;
  CREATE USER dbm_svc FOR LOGIN dbm_svc;
  -- Read access on required tables/views
  GRANT SELECT ON dbo.icitem        TO dbm_svc;
  GRANT SELECT ON dbo.arcust        TO dbm_svc;
  GRANT SELECT ON dbo.SOPortal      TO dbm_svc;
  -- Execute on named SPs only
  GRANT EXECUTE ON dbo.sp_camp_getSPprice   TO dbm_svc;
  GRANT EXECUTE ON dbo.sp_ws_syncorder      TO dbm_svc;
  GRANT EXECUTE ON dbo.sp_ws_reactivate     TO dbm_svc;
  GRANT EXECUTE ON dbo.sp_ws_gensoxitems    TO dbm_svc;
  -- (TC-05 cancellation SP — add when confirmed)
USE compplanLive;
  CREATE USER dbm_svc FOR LOGIN dbm_svc;
  GRANT SELECT ON dbo.CTstatement   TO dbm_svc;
  GRANT SELECT ON dbo.CTdownlineh   TO dbm_svc;
  -- (add further objects as domain specs confirm requirements)
```

No `sa` credentials. No broad `db_owner`. The grant list is the exact boundary — nothing wider.

---

## 4. Infrastructure as Code

### 4.1 Repository layout

```
infra/
  bicep/
    main.bicep                    # Top-level deployment — calls modules
    parameters/
      staging.bicepparam
      parity.bicepparam
      production.bicepparam
    modules/
      app-service.bicep            # DBM Core App Service
      app-service-admin.bicep      # Admin Console App Service
      sql-dbm-state.bicep          # DBM State Azure SQL
      service-bus.bicep            # Azure Service Bus
      key-vault.bicep              # Key Vault + secret stubs
      app-insights.bicep           # Application Insights
      vnet.bicep                   # VNet + subnets
      am-vm.bicep                  # AccountMate IaaS VM
  docker/
    docker-compose.yml             # Local development stack
    docker-compose.test.yml        # Integration test stack (real SQL, no AM)
  scripts/
    am/
      seed-staging-am.ps1
      reset-parity-am.ps1
      take-golden-snapshot.ps1
      verify-am-connection.ps1
    env/
      provision-staging.sh
      provision-parity.sh
      teardown-parity.sh           # Destroys parity env after a run to save cost
```

### 4.2 Local development stack (Docker Compose)

Provides a complete local DBM runtime without requiring Azure resources or AM connectivity.

```yaml
# docker-compose.yml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "DevLocal_1234"
      ACCEPT_EULA: "Y"
    ports: ["1433:1433"]
    volumes: ["./data/sql:/var/opt/mssql/data"]

  servicebus-emulator:
    image: mcr.microsoft.com/azure-messaging/servicebus-emulator:latest
    ports: ["5672:5672", "8080:8080"]

  dbm-core:
    build: ./src/DBM.Core
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__DbmState: "Server=sqlserver;Database=dbm_state_dev;User Id=sa;Password=DevLocal_1234;"
      ServiceBus__ConnectionString: "Endpoint=sb://localhost;..."
      AM__Enabled: "false"  # AM disabled in local; test doubles inject AM responses
    depends_on: [sqlserver, servicebus-emulator]
    ports: ["5000:8080"]

  admin-console:
    build: ./src/DBM.AdminConsole
    environment:
      NODE_ENV: development
      DBM_API_URL: "http://dbm-core:8080"
    ports: ["3000:3000"]
```

`AM__Enabled: false` — in local mode, the AM client is replaced by a configurable test double that serves pre-recorded AM responses from JSON fixture files. This allows full local development without an AM connection.

### 4.3 Environment promotion model

```
main branch
  │
  ├── PR merge → deploy to staging (auto)
  │     ├── Run unit tests
  │     ├── Run integration tests (against staging-am)
  │     └── Deploy if all green
  │
  ├── Manual trigger → run parity harness (against parity-am)
  │     ├── Reset parity environment
  │     ├── Run parity test suite
  │     └── Publish parity report — must be clean before go-live sign-off
  │
  └── Manual approval → deploy to production
        ├── Requires: parity harness clean
        ├── Requires: staging integration tests passing
        └── Blue/green slot swap (App Service deployment slots)
```

---

## 5. Testing Strategy

### 5.1 Test pyramid

```
                    ┌─────────────────────┐
                    │    E2E Tests        │  Playwright — admin console flows
                    │    (slow, few)      │  API client — webhook → AM round-trip
                    └─────────────────────┘
              ┌──────────────────────────────────────┐
              │        Parity Tests                  │  Parity harness — DBM vs oracle
              │   (scheduled, golden-snapshot-based) │  Per-domain comparison runs
              └──────────────────────────────────────┘
        ┌────────────────────────────────────────────────────┐
        │            Integration Tests                       │  Against staging-am or parity-am
        │     (medium, run on PR merge)                      │  Real SQL, real SPs, real ASB
        └────────────────────────────────────────────────────┘
  ┌─────────────────────────────────────────────────────────────────┐
  │                     Unit Tests                                  │  xUnit (.NET), Jest (Node.js)
  │              (fast, run on every commit)                        │  Test doubles for all external deps
  └─────────────────────────────────────────────────────────────────┘
```

### 5.2 TDD mandate

**TDD is not optional.** Every service, every domain function, every API handler follows this workflow:

```
1. Write a failing test that describes the required behaviour (RED)
2. Write the minimum implementation to make it pass (GREEN)
3. Refactor without breaking the test (REFACTOR)
```

**What this means in practice:**

- No service class file is created before its test file.
- The test class defines the interface contract. The implementation class satisfies it.
- A feature branch cannot be merged if it adds implementation code without corresponding test coverage.
- The CI pipeline enforces a minimum coverage threshold: **80% line coverage for domain services, 70% for platform services** (advisory initially, enforced at go-live).

**Test file conventions (.NET):**

```
src/
  DBM.Core/
    Services/
      OrderWritebackService.cs       # Implementation
tests/
  DBM.Core.Tests/
    Unit/
      Services/
        OrderWritebackServiceTests.cs  # Unit test
  DBM.Integration.Tests/
    Orders/
      OrderWritebackIntegrationTests.cs  # Integration test (hits staging-am)
  DBM.Parity.Tests/
    Orders/
      OrderWritebackParityTests.cs  # Parity harness test
```

### 5.3 Unit tests

**Framework:** xUnit (.NET), Jest (Node.js admin console)

**Scope:** A unit test must complete with no external dependencies — no SQL, no Service Bus, no HTTP. All external calls are replaced by test doubles (interfaces + fakes or stubs, not mocks).

**Required per service:**
- Happy path: correct input → correct output
- Idempotency: processing the same event twice produces the same result and no duplicate side effects
- Error paths: AM write failure → exception propagated correctly; retry logic triggered correctly
- Boundary conditions: empty collections, null fields, campaign boundary dates

**Test double strategy:**

```csharp
// Interface — used in production
public interface IAccountMateClient
{
    Task<SyncOrderResult> SyncOrderAsync(SyncOrderRequest req, CancellationToken ct);
}

// Test double — injected in unit tests
public class FakeAccountMateClient : IAccountMateClient
{
    public List<SyncOrderRequest> ReceivedRequests { get; } = new();
    public SyncOrderResult? NextResult { get; set; }

    public Task<SyncOrderResult> SyncOrderAsync(SyncOrderRequest req, CancellationToken ct)
    {
        ReceivedRequests.Add(req);
        return Task.FromResult(NextResult ?? new SyncOrderResult { Success = true });
    }
}
```

Fakes over mocks. Fakes implement the interface with real (simple) logic. Mocks are only used where a fake would be more complex than the system under test.

### 5.4 Integration tests

**Framework:** xUnit with `WebApplicationFactory<T>` (.NET), real SQL Server (staging-am or parity-am)

**Scope:** Integration tests exercise the complete service stack: the real `IAccountMateClient` implementation, the real Dapper queries, the real stored procedure calls, against a real SQL Server with live AM data (seeded from production backup).

**Run environment:** These tests do not run locally by default. They run in the CI pipeline against `staging-am` on PR merge. They can be run locally by pointing at `staging-am` with the correct connection string.

**Required per domain:**
- End-to-end: Shopify event payload → DBM processing → AM state diff (verify correct rows written)
- Idempotency: submit same event twice → AM state identical to single-submission outcome
- Retry/fault tolerance: simulate AM SP failure → verify retry kicks in → verify success on retry
- Watermark advance: after a sync run, watermark advances correctly

**Example: order write-back integration test**

```csharp
[Fact]
public async Task WriteBack_GivenValidConsultantOrder_WritesExpectedRowsToAM()
{
    // Arrange: restore known AM snapshot
    await _amFixture.RestoreSnapshotAsync("order_writeback_baseline");
    var order = ShopifyOrderFixtures.ValidConsultantOrder();

    // Act
    var result = await _orderWritebackService.ProcessAsync(order, CancellationToken.None);

    // Assert: verify the expected rows appeared in AM
    var arOrderRows = await _amFixture.QueryAsync(
        "SELECT * FROM soh WHERE cOrderNo = @no", new { no = result.AmOrderNo });
    arOrderRows.Should().HaveCount(1);
    arOrderRows[0].cCustNo.Should().Be(order.Customer.MetafieldConsultantNo);
    arOrderRows[0].nAmtOrd.Should().Be(order.TotalPrice);
}
```

### 5.5 Parity tests (harness)

See §6 for the full parity harness specification.

### 5.6 E2E tests

**Framework:** Playwright (admin console UI), custom API test client (webhook round-trip flows)

**Scope:** E2E tests exercise the complete system as a black box — from an external event (Shopify webhook, admin console action) through DBM and back out to Shopify or AM.

**Run environment:** Run against the `staging` environment on demand (not on every PR — too slow). Required to be clean before go-live.

**Critical E2E flows to cover:**
- Shopify order placed → webhook received → AM order written → fulfillment event returned → Shopify order updated
- Admin console: campaign management UI → creates/updates campaign → pricing sync fires → Shopify metafields updated
- Admin console: dead-letter queue view → operator clicks replay → item reprocessed successfully
- Pricing boundary: advance clock past campaign `dFrom` → pricing sweep fires → Shopify metafields reflect new campaign price

---

## 6. Parity Harness Specification

### 6.1 The parity problem

The legacy NISource (VFP) stack cannot be re-run in a controlled test environment. Its behaviour exists only in production, and its outputs are the current state of AccountMate and NopCommerce. We cannot compare DBM against a live re-run of NISource.

Instead, the parity strategy is:
1. **Capture** the authoritative legacy output as a "golden" state.
2. **Seed** DBM's test AM instance with a known baseline state.
3. **Run** DBM against that baseline.
4. **Compare** DBM's output against the golden expected state.

For operations that call AM SPs (not VFP logic), we can go further: **the SP itself is the contract**. DBM must call it with the correct parameters for any given input. The SP's output in a controlled AM state is deterministic and verifiable.

### 6.2 Parity harness architecture

```
                  ┌─────────────────────────────────────────┐
                  │  Golden Dataset (parity golden snapshot)│
                  │  ─ AM DB snapshot (production-derived)  │
                  │  ─ Shopify event fixtures (JSON files)  │
                  │  ─ Expected output definitions (JSON)   │
                  └───────────────┬─────────────────────────┘
                                  │ restore
                                  ▼
                  ┌─────────────────────────────────────────┐
                  │  parity-am (reset to golden snapshot)   │
                  └───────────────┬─────────────────────────┘
                                  │ DBM runs against it
                                  ▼
                  ┌─────────────────────────────────────────┐
                  │  DBM Core (parity environment)          │
                  │  processes each fixture event           │
                  └───────────────┬─────────────────────────┘
                                  │ produces
                                  ▼
                  ┌─────────────────────────────────────────┐
                  │  Parity Comparator                      │
                  │  ─ Queries AM for actual state          │
                  │  ─ Compares against expected definitions│
                  │  ─ Emits parity report (pass/fail/diff) │
                  └─────────────────────────────────────────┘
```

### 6.3 Per-domain parity strategy

#### Orders — write-back parity

**What we're testing:** Given a Shopify order, DBM writes the correct rows to AM's order tables.

**How golden data is built:**
1. In production, capture a set of representative Shopify orders (consultant orders, DTC orders, orders with campaign pricing, orders with exclusive items).
2. Record the AM table diff after NISource processes each order (before/after snapshot of `soh`, `sol`, `arcash` etc).
3. Store each diff as an "expected output" JSON file in the parity golden dataset.

**Parity test:**
1. Restore parity-am to the "before" snapshot for that order.
2. Submit the same Shopify order payload to DBM's webhook receiver.
3. Query AM for the actual resulting rows.
4. Assert actual == expected (field-by-field comparison, with allowlist for fields like timestamps that legitimately differ).

**Success condition:** DBM writes identical order rows to AM for each test order. Any divergence must be either a known improvement (documented) or a bug.

#### Pricing — oracle parity

**What we're testing:** DBM's precomputed effective prices match `sp_camp_getSPprice` for all active SKUs.

**Golden data:** Not required — we run the oracle directly on parity-am and compare to DBM's computation. This is a live comparison, not a recorded comparison.

**Parity test:**
1. On parity-am, run `sp_camp_getSPprice` for all active consultant × SKU combinations. Record results.
2. Run DBM's `CampaignPriceSyncService` against parity-am.
3. Query DBM State DB for the precomputed prices.
4. Assert DBM price == oracle price for every SKU.
5. Advance the clock past a campaign `dFrom` boundary.
6. Run the boundary sweep.
7. Re-run oracle on parity-am and re-compare — DBM prices must now reflect the new campaign.

**Success condition:** Zero price divergence between DBM precomputed prices and the oracle for all active SKUs at all tested campaign boundaries.

#### Product and inventory sync parity

**What we're testing:** DBM syncs the correct product and inventory data from AM to Shopify.

**Golden data:** The current live Shopify store state IS the golden output — it reflects what NISource `syncproducts.prg` has been maintaining. A snapshot of the live Shopify store's products is captured as the baseline.

**Parity test:**
1. Seed parity-am from production backup.
2. Run DBM `ProductSyncService` full sync.
3. Compare DBM-produced Shopify products against the baseline snapshot: variant titles, SKU codes, active/inactive flags, metafield values.
4. Document known intentional divergences (improvements over legacy).

#### Consultant sync parity

**What we're testing:** DBM projects the correct consultant state from AM into Shopify.

**Golden data:** Current live Shopify customer metafields for a sample of active consultants, captured from the live store.

**Parity test:**
1. Seed parity-am from production backup.
2. Run DBM `ConsultantSyncService` for the same sample set.
3. Compare DBM-produced customer metafields against the golden baseline: `consultant.number`, `consultant.status`, `consultant.tier`, `consultant.exclusive_access`.

### 6.4 Golden dataset management

```
tests/
  DBM.Parity.Tests/
    golden/
      orders/
        fixtures/
          order_consultant_campaign_01.json     # Shopify order payload
          order_dtc_no_campaign_01.json
          order_exclusive_item_01.json
        expected/
          order_consultant_campaign_01_am_diff.json  # Expected AM table diff
          order_dtc_no_campaign_01_am_diff.json
      products/
        baseline_shopify_snapshot.json          # Captured from live Shopify
      consultants/
        sample_consultant_metafields.json       # Captured from live Shopify
      snapshots/
        README.md                               # Documents snapshot naming and version
```

Golden fixtures are versioned in source control. When the legacy system is updated or a production backup refreshed, the golden fixtures are regenerated using:

```
scripts/am/take-golden-snapshot.ps1 --capture-shopify-baseline
```

This script captures the AM snapshot AND the corresponding live Shopify state, producing a consistent pair.

### 6.5 Parity report

Each parity run produces a machine-readable and human-readable report:

```json
{
  "run_id": "parity-2026-04-07T14:00:00Z",
  "am_snapshot": "parity-golden-2026-04-05",
  "domains": {
    "orders": {
      "fixtures_tested": 12,
      "passed": 12,
      "failed": 0,
      "divergences": []
    },
    "pricing": {
      "skus_tested": 135,
      "passed": 135,
      "failed": 0,
      "campaign_boundary_tested": true
    },
    "products": {
      "products_tested": 135,
      "passed": 133,
      "known_improvements": 2,
      "failed": 0
    }
  },
  "overall_status": "CLEAN"
}
```

A parity run must report `"overall_status": "CLEAN"` before any production deployment is approved.

---

## 7. CI/CD Pipeline

### 7.1 Pipeline overview

Three GitHub Actions workflows, each with specific environment gates:

```yaml
# .github/workflows/ci.yml  — runs on every push/PR
jobs:
  unit-tests:          # dotnet test --filter Category=Unit
  build:               # dotnet publish, npm run build
  lint:                # dotnet format --verify-no-changes, eslint

# .github/workflows/deploy-staging.yml  — runs on merge to main
jobs:
  integration-tests:   # dotnet test --filter Category=Integration (against staging-am)
  deploy-staging:      # needs: integration-tests
    # Blue/green slot swap

# .github/workflows/parity.yml  — runs on manual trigger or weekly schedule
jobs:
  reset-parity-env:    # scripts/env/reset-parity-env.sh
  parity-tests:        # dotnet test --filter Category=Parity (against parity-am)
  publish-report:      # Upload parity report to Azure Blob Storage

# .github/workflows/deploy-production.yml  — manual trigger, requires approvals
jobs:
  check-prerequisites:
    # Validates: parity report clean (< 7 days old), staging integration tests green
  deploy-production:   # needs: check-prerequisites, human-approval
    # Blue/green slot swap
```

### 7.2 Test categorisation (.NET)

Tests are categorised using xUnit traits so the pipeline can select them precisely:

```csharp
[Trait("Category", "Unit")]
public class OrderWritebackServiceTests { }

[Trait("Category", "Integration")]
[Trait("RequiresAm", "true")]
public class OrderWritebackIntegrationTests { }

[Trait("Category", "Parity")]
[Trait("RequiresParityAm", "true")]
public class OrderWritebackParityTests { }
```

### 7.3 Environment gates summary

| Gate | When enforced | Blocks |
|---|---|---|
| Unit tests pass | Every commit (PR and main) | PR merge |
| Build succeeds | Every commit | PR merge |
| Integration tests pass (staging-am) | Every merge to main | Staging deployment |
| Parity report clean (< 7 days) | Production deploy trigger | Production deployment |
| Human approval (two reviewers) | Production deploy trigger | Production deployment |

---

## 8. Shopify Environment Strategy

### 8.1 Store tiers

| Store | Type | Shopify plan | Purpose |
|---|---|---|---|
| `annique-local` | Partner development store | Free | Local Shopify API mocking not needed — Shopify APIs hit `annique-staging` from local via tunnel if needed |
| `annique-staging` | Partner development store | Shopify Plus dev store | Full integration testing — Shopify Functions deployed, webhooks pointed at staging DBM |
| `annique-parity` | Partner development store | Free (no Checkout needed for parity) | Reset between parity runs; Shopify state comparison target |
| Live store | Production | Shopify Plus | Production |

### 8.2 Shopify Function deployment

Shopify Functions (Rust) are compiled and deployed with the custom app using `shopify app deploy`. Each environment has its own custom app install targeting its own Shopify store.

Functions are deterministic — they read from product/customer metafields only. If DBM is correctly syncing the right metafield values, the Functions are automatically correct. Parity testing of pricing and exclusive-access behaviour flows through the metafield parity tests, not through Shopify Function unit tests (though the Function itself also has unit tests via `cargo test`).

---

## 9. POC and MVP Deployment Strategy

### 9.1 Phase gates

The environment model supports incremental deployment by domain:

| Phase | Deploys | Go-live action |
|---|---|---|
| **POC** | Product sync only | DBM syncs AM products to staging Shopify. Validates Dapper + SP pattern, sync watermark, outbox relay end-to-end. |
| **MVP** | Product sync + Order write-back + Pricing sync | First customer-facing value: orders and pricing working end-to-end in staging. |
| **Phase 1** | All unblocked domains (see PRD phase 1 list) | Parallel run: DBM running in staging, legacy NISource still handling production. |
| **Cutover** | Switch DNS/webhooks from NISource to DBM production | DBM handles all traffic. NISource off. |

Each phase has its own parity run requirement before promotion.

### 9.2 Parallel run model

Between Phase 1 deployment and cutover, both NISource and DBM process a mirrored stream of orders:
- Shopify orders flow to both NISource (via existing integration) and DBM staging.
- DBM's AM writes go to the staging-am instance (not production AM).
- Parity comparator compares DBM's staging AM writes against what NISource wrote to production AM.
- Divergences are investigated and resolved before cutover.

This requires a mechanism to replay production Shopify events to DBM staging — achieved via a mirroring webhook endpoint configured for the parallel run window.

---

## 10. Observability in Non-Production

All environments have Application Insights configured. The non-production workspaces are deliberately separated from production to prevent test noise from appearing in production dashboards or triggering production alerts.

| Workspace | Alerts enabled | Sampling rate |
|---|---|---|
| staging | Warning level only | 100% |
| parity | None (results are captured by test harness) | 100% |
| production | Full alert set (see architecture spec §8.3) | 100% initially, reduce at scale |

Parity test runs emit structured telemetry to the parity Application Insights workspace so runs can be compared over time.

---

## Evidence Base

| Artefact | Location |
|---|---|
| Architecture specification | `docs/spec/01_dbm_architecture.md` |
| PRD — phase 1 scope and NFRs | `docs/spec/00_prd.md` |
| Platform runtime cross-cut | `docs/analysis/16_platform_runtime_and_hosting_crosscut.md` |
| Auditability and idempotency cross-cut | `docs/analysis/18_auditability_idempotency_and_reconciliation_crosscut.md` |
| Data access and SQL contract | `docs/analysis/17_data_access_and_sql_contract_crosscut.md` |
| Pricing engine deep dive (parity strategy) | `docs/analysis/20_pricing_engine_deep_dive.md` |
| DBM–AM interface contract | `docs/analysis/24_dbm_am_interface_contract.md` |
| AM cloud migration advisory | `docs/onboarding/04_session_state_april_2026.md` §7 |
