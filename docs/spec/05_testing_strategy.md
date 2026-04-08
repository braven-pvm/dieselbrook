# Testing Strategy
## Dieselbrook Middleware (DBM) — Annique Cosmetics Platform Modernisation

**Document type:** Testing Strategy Specification  
**Programme:** Annique → Shopify Plus migration and Dieselbrook Middleware build  
**Version:** 0.1 — initial  
**Date:** 2026-04-07  
**Status:** Draft  
**Linear:** ANN-23  
**Parent specs:** `docs/spec/03_infrastructure.md`, `docs/spec/04_environments.md`  
**Notion:** https://www.notion.so/dieselbrook/annique-programme

---

## Decision Dependencies

| Decision ID | Topic | Status | Impact If Still Open |
|---|---|---|---|
| D-06 | Shopify plan tier | ✅ Closed 2026-03-15 | Was: Shopify Plus required for dev store parity |
| TC-02 | Go-live date | ✅ Closed 2026-03-15 | Was: affected test phase timeline |
| TC-04 | OTP delivery model | ⚠️ Open | Blocks OTP section of communications integration tests |
| TC-05 | Order cancellation SP | ⚠️ Open | Blocks cancellation write-back integration test |

TC-04 and TC-05 are open but do not block the bulk of this specification. Affected sections are marked.

---

## 1. The Testing Problem

DBM replaces a production system that has been running for 15+ years. The system under test is not greenfield — it is a replacement. The risk is not that DBM fails catastrophically. The risk is that DBM produces **subtly wrong outputs** that are not obviously wrong:

- An order written to AM with a price that is off by R0.01 due to a rounding difference
- A consultant sync that drops an entitlement flag for one edge-case tier
- A pricing sweep that misses a campaign boundary by 60 seconds
- A product sync that marks an active SKU as inactive because a NULL field is handled differently

These failures are silent. They pass cursory inspection. They surface as data quality issues weeks after go-live.

The only defence is **behavioural parity verification**: given the same inputs (same Shopify event, same AM database state), DBM must produce the same outputs as the legacy system — or a provably correct, documented improvement.

This is the central question every test must answer:

> *"For these inputs, does DBM produce the same result as the legacy system, or a documented improvement?"*

---

## 2. Testing Principles

### 2.1 TDD is mandatory

No implementation file is created before its test file. The workflow is:

```
1. Write a failing test that describes the required behaviour  (RED)
2. Write the minimum implementation to make it pass            (GREEN)
3. Refactor without breaking the test                          (REFACTOR)
```

**Enforcement:**

- No PR merges if implementation code has no corresponding test coverage.
- CI pipeline enforces minimum coverage thresholds: **80% line coverage for domain services, 70% for platform services** (advisory until go-live, hard-enforced at go-live).
- Coverage reporting via `dotnet-coverage` published to GitHub Actions on every CI run.

### 2.2 Test doubles, not mocks

For unit tests, external dependencies (AM, Shopify API, Service Bus) are replaced by **fakes** — interface implementations with simple real logic — not mocks. Mocks that assert on call counts or parameter matching are brittle and test implementation rather than behaviour.

- `FakeAccountMateClient` implements `IAccountMateClient` with configurable response fixtures.
- `FakeShopifyApiClient` implements `IShopifyApiClient` and records outbound calls for assertion.
- `FakeOutboxRepository` implements `IOutboxRepository` with an in-memory list.

Mocks are only used when a fake would be more complex than the system under test.

### 2.3 Integration tests hit real infrastructure

Integration tests use real SQL Server (against `staging-am`), real Service Bus, and real DBM State DB. There is no mocking at the integration test level. The only test double that persists into integration testing is the Shopify API client (to avoid polluting the staging Shopify store with test data). Shopify API calls at the integration test level are recorded and replayed via fixture files.

### 2.4 Parity is a first-class test suite

Parity tests are not an afterthought or a one-time validation. They are a suite that:

- Has its own test project (`DBM.Parity.Tests`)
- Runs on a weekly schedule and on manual trigger
- Must be clean before any production deployment
- Grows as new domain specs are completed

### 2.5 Coverage thresholds are per-layer

| Layer | Threshold | Rationale |
|---|---|---|
| Domain services | 80% line | Core business logic; highest correctness risk |
| Platform services (outbox, retry, watermark) | 70% line | Infrastructure concerns; lower business complexity |
| API controllers / webhook handlers | 60% line | Thin; integration tests provide better coverage than unit tests |
| Admin Console (Node.js) | 70% statement | Business logic paths in the admin console |

---

## 3. Test Pyramid

```
                        ┌───────────────────────┐
                        │    E2E Tests           │
                        │    Playwright (UI)     │  ~ 20 tests
                        │    API round-trips     │  Slow, run on-demand
                        └───────────────────────┘

              ┌─────────────────────────────────────────┐
              │           Parity Tests                  │
              │   Golden snapshot × domain comparison   │  ~ 50 scenarios
              │   Scheduled weekly + manual trigger     │  Medium, 20–40 min
              └─────────────────────────────────────────┘

        ┌─────────────────────────────────────────────────────┐
        │             Integration Tests                       │
        │   Real SQL (staging-am), real SPs, real ASB         │  ~ 150 tests
        │   Run on every merge to main                        │  Medium, 5–15 min
        └─────────────────────────────────────────────────────┘

  ┌─────────────────────────────────────────────────────────────────┐
  │                     Unit Tests                                  │
  │   xUnit (.NET) + Jest (Node.js)                                 │  ~ 500 tests
  │   Test doubles for all external deps                            │  Fast, < 2 min
  │   Run on every commit (PR and main)                             │
  └─────────────────────────────────────────────────────────────────┘
```

---

## 4. Unit Tests

### 4.1 Framework and tooling

| Component | Framework | Runner | Assertions |
|---|---|---|---|
| DBM Core (.NET) | xUnit 2.x | `dotnet test` | FluentAssertions |
| Admin Console (Node.js) | Jest | `npm test` | Jest matchers |
| Shopify Functions (Rust) | `cargo test` | `cargo test` | Standard Rust assertions |

### 4.2 Test file structure

```
src/
  DBM.Core/
    Services/
      OrderWritebackService.cs
      PricingSyncService.cs
      ProductSyncService.cs
      ConsultantSyncService.cs
      OutboxRelayService.cs
tests/
  DBM.Core.Tests/
    Unit/
      Services/
        OrderWritebackServiceTests.cs
        PricingSyncServiceTests.cs
        ProductSyncServiceTests.cs
        ConsultantSyncServiceTests.cs
        OutboxRelayServiceTests.cs
      Domain/
        PricingCalculationTests.cs
        CampaignBoundaryTests.cs
        ConsultantEntitlementTests.cs
      Platform/
        IdempotencyLedgerTests.cs
        RetryPolicyTests.cs
        WatermarkAdvanceTests.cs
      Webhooks/
        OrderWebhookHandlerTests.cs
        CustomerWebhookHandlerTests.cs
    fixtures/
      am/
        sp_camp_getSPprice/
        sp_ws_syncorder/
        icitem/
        arcust/
      shopify/
        orders/
        customers/
        products/
```

### 4.3 Test doubles

```csharp
// IAccountMateClient — production interface
public interface IAccountMateClient
{
    Task<IReadOnlyList<AmProduct>> GetActiveProductsAsync(CancellationToken ct);
    Task<IReadOnlyList<AmCustomer>> GetCustomerDeltaAsync(DateTimeOffset since, CancellationToken ct);
    Task<SyncOrderResult> SyncOrderAsync(SyncOrderRequest req, CancellationToken ct);
    Task<decimal> GetSpPriceAsync(string custNo, string itemNo, CancellationToken ct);
    Task<IReadOnlyList<CampaignPriceResult>> GetAllCampaignPricesAsync(CancellationToken ct);
}

// Fake — used in all unit tests
public class FakeAccountMateClient : IAccountMateClient
{
    public List<SyncOrderRequest> SyncedOrders { get; } = new();
    public List<(string CustNo, string ItemNo)> PriceQueries { get; } = new();

    // Configurable responses
    public IReadOnlyList<AmProduct> Products { get; set; } = Array.Empty<AmProduct>();
    public IReadOnlyList<AmCustomer> CustomerDelta { get; set; } = Array.Empty<AmCustomer>();
    public SyncOrderResult DefaultSyncResult { get; set; } = new() { Success = true };
    public Dictionary<(string, string), decimal> PriceMap { get; } = new();

    public Task<IReadOnlyList<AmProduct>> GetActiveProductsAsync(CancellationToken ct)
        => Task.FromResult(Products);

    public Task<IReadOnlyList<AmCustomer>> GetCustomerDeltaAsync(DateTimeOffset since, CancellationToken ct)
        => Task.FromResult(CustomerDelta);

    public Task<SyncOrderResult> SyncOrderAsync(SyncOrderRequest req, CancellationToken ct)
    {
        SyncedOrders.Add(req);
        return Task.FromResult(DefaultSyncResult);
    }

    public Task<decimal> GetSpPriceAsync(string custNo, string itemNo, CancellationToken ct)
    {
        PriceQueries.Add((custNo, itemNo));
        return Task.FromResult(PriceMap.TryGetValue((custNo, itemNo), out var p) ? p : 0m);
    }

    public Task<IReadOnlyList<CampaignPriceResult>> GetAllCampaignPricesAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<CampaignPriceResult>>(Array.Empty<CampaignPriceResult>());
}
```

```csharp
// IShopifyApiClient — production interface
public interface IShopifyApiClient
{
    Task<ShopifyProduct> UpsertProductAsync(UpsertProductRequest req, CancellationToken ct);
    Task UpdateCustomerMetafieldsAsync(string customerId, IReadOnlyDictionary<string, string> metafields, CancellationToken ct);
    Task<ShopifyOrder> GetOrderAsync(string orderId, CancellationToken ct);
}

// Fake
public class FakeShopifyApiClient : IShopifyApiClient
{
    public List<UpsertProductRequest> UpsertedProducts { get; } = new();
    public List<(string CustomerId, IReadOnlyDictionary<string, string> Metafields)> UpdatedMetafields { get; } = new();

    public Task<ShopifyProduct> UpsertProductAsync(UpsertProductRequest req, CancellationToken ct)
    {
        UpsertedProducts.Add(req);
        return Task.FromResult(new ShopifyProduct { Id = "fake-" + req.Sku });
    }

    public Task UpdateCustomerMetafieldsAsync(string customerId,
        IReadOnlyDictionary<string, string> metafields, CancellationToken ct)
    {
        UpdatedMetafields.Add((customerId, metafields));
        return Task.CompletedTask;
    }

    public Task<ShopifyOrder> GetOrderAsync(string orderId, CancellationToken ct)
        => Task.FromResult(new ShopifyOrder { Id = orderId });
}
```

### 4.4 Required unit tests per service

Every service must have unit tests covering:

| Test type | Description |
|---|---|
| Happy path | Correct input → correct output, correct side effects |
| Idempotency | Processing the same event twice produces the same result; no duplicate AM writes |
| Error propagation | AM failure throws the correct exception type; retry is triggered |
| Watermark advance | After successful processing, watermark moves forward correctly |
| Boundary conditions | Empty collections, null fields, campaign boundary dates (midnight transition) |
| Concurrency | Two parallel requests for the same order don't produce duplicate AM writes (idempotency key) |

### 4.5 Example: order write-back unit test

```csharp
[Trait("Category", "Unit")]
public class OrderWritebackServiceTests
{
    private readonly FakeAccountMateClient _am = new();
    private readonly FakeOutboxRepository _outbox = new();
    private readonly OrderWritebackService _sut;

    public OrderWritebackServiceTests()
    {
        _sut = new OrderWritebackService(_am, _outbox, NullLogger<OrderWritebackService>.Instance);
    }

    [Fact]
    public async Task ProcessAsync_GivenValidConsultantOrder_CallsSyncOrderWithCorrectParameters()
    {
        var order = ShopifyOrderFixtures.ValidConsultantOrder(consultantNo: "C001", total: 350.00m);

        await _sut.ProcessAsync(order, CancellationToken.None);

        _am.SyncedOrders.Should().HaveCount(1);
        var req = _am.SyncedOrders[0];
        req.CustomerNo.Should().Be("C001");
        req.OrderTotal.Should().Be(350.00m);
        req.OrderLines.Should().HaveCount(order.LineItems.Count);
    }

    [Fact]
    public async Task ProcessAsync_WhenCalledTwiceWithSameOrder_CallsAmOnlyOnce()
    {
        var order = ShopifyOrderFixtures.ValidConsultantOrder();
        _am.DefaultSyncResult = new SyncOrderResult { Success = true };

        await _sut.ProcessAsync(order, CancellationToken.None);
        await _sut.ProcessAsync(order, CancellationToken.None);

        // Idempotency: second call must detect duplicate via idempotency key
        _am.SyncedOrders.Should().HaveCount(1);
    }

    [Fact]
    public async Task ProcessAsync_WhenAmThrows_PropagatesExceptionForRetry()
    {
        var order = ShopifyOrderFixtures.ValidConsultantOrder();
        _am.DefaultSyncResult = null;
        _am.ShouldThrow = new AmTransientException("Connection refused");

        await _sut.Invoking(s => s.ProcessAsync(order, CancellationToken.None))
            .Should().ThrowAsync<AmTransientException>();
    }

    [Fact]
    public async Task ProcessAsync_GivenCampaignPricedOrder_UsesCampaignLinePrice()
    {
        var order = ShopifyOrderFixtures.ConsultantOrderWithCampaignLine(
            sku: "SKU-001", campaignPrice: 249.00m, regularPrice: 299.00m);

        await _sut.ProcessAsync(order, CancellationToken.None);

        var line = _am.SyncedOrders[0].OrderLines.Single();
        line.UnitPrice.Should().Be(249.00m); // Campaign price, not regular
    }
}
```

### 4.6 Example: pricing boundary unit test

```csharp
[Trait("Category", "Unit")]
public class CampaignBoundaryTests
{
    [Theory]
    [InlineData("2026-04-01T00:00:00", "2026-04-07T23:59:59", "2026-04-07T12:00:00", true)]
    [InlineData("2026-04-01T00:00:00", "2026-04-07T23:59:59", "2026-04-08T00:00:00", false)]
    [InlineData("2026-04-01T00:00:00", "2026-04-07T23:59:59", "2026-03-31T23:59:59", false)]
    public void IsActive_ReturnsTrueOnlyWithinCampaignWindow(
        string dFrom, string dTo, string checkAt, bool expectedActive)
    {
        var campaign = new Campaign
        {
            DFrom = DateTimeOffset.Parse(dFrom),
            DTo   = DateTimeOffset.Parse(dTo)
        };

        campaign.IsActiveAt(DateTimeOffset.Parse(checkAt)).Should().Be(expectedActive);
    }

    [Fact]
    public void BoundarySweep_WhenClockCrossesDFrom_MarksNewCampaignActive()
    {
        var sweepService = new PricingBoundarySweepService(
            new FakeCampaignRepository(),
            new FakeShopifyApiClient(),
            NullLogger<PricingBoundarySweepService>.Instance);

        // Campaign starts in 1 millisecond
        var campaign = CampaignFixtures.StartsAt(DateTimeOffset.UtcNow.AddMilliseconds(1));

        // Advance clock past dFrom
        TestClock.AdvanceTo(campaign.DFrom.AddSeconds(1));

        sweepService.RunSweepAsync(CancellationToken.None).Wait();

        // Shopify metafields should reflect new campaign price
        _shopify.UpdatedMetafields.Should().ContainKey("campaign.active_price");
    }
}
```

---

## 5. Integration Tests

### 5.1 Framework and setup

**Framework:** xUnit with `WebApplicationFactory<T>` for API endpoint tests; direct service instantiation for service-layer tests.

**External dependencies in integration tests:**

| Dependency | Approach |
|---|---|
| AM SQL Server | Real connection to `staging-am` via connection string in CI secrets |
| DBM State DB | Real Azure SQL (staging), reset per test class via fixture |
| Service Bus | Real Azure Service Bus (staging namespace) |
| Shopify API | Fixture-based replay — real API calls are recorded against staging store during test authoring, replayed in CI |

### 5.2 Test categorisation

All tests are categorised with xUnit traits. The CI pipeline selects test categories using `--filter`:

```csharp
// Unit tests — no external deps
[Trait("Category", "Unit")]
public class OrderWritebackServiceTests { }

// Integration tests — require staging-am and staging Azure resources
[Trait("Category", "Integration")]
[Trait("RequiresAm", "staging")]
public class OrderWritebackIntegrationTests { }

// Parity tests — require parity-am (golden snapshot already restored)
[Trait("Category", "Parity")]
[Trait("RequiresAm", "parity")]
public class OrderWritebackParityTests { }

// E2E tests — require full staging environment + staging Shopify store
[Trait("Category", "E2E")]
public class OrderRoundTripE2ETests { }
```

**CI filter commands:**

```bash
# Unit only (fast, every commit)
dotnet test --filter Category=Unit

# Integration only (on PR merge)
dotnet test --filter Category=Integration

# Parity only (parity workflow)
dotnet test --filter Category=Parity
```

### 5.3 Database fixture strategy

Integration tests that mutate AM state require a known baseline. Each integration test class that touches AM state uses `AmSnapshotFixture` to:

1. Before the test class: restore a named AM snapshot to the staging-am SQL instance.
2. After the test class: the staging-am state is left as-is (not rolled back) — integration tests accept cumulative state.

```csharp
// Shared collection fixture
[CollectionDefinition("Integration-OrderWriteback")]
public class IntegrationCollection : ICollectionFixture<AmSnapshotFixture<OrderWritebackBaseline>> { }

// Snapshot fixture
public class AmSnapshotFixture<TBaseline> : IAsyncLifetime where TBaseline : IAmBaseline, new()
{
    public IAccountMateClient AmClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var connStr = TestConfiguration.AmConnectionString;
        await new TBaseline().RestoreAsync(connStr);
        AmClient = new SqlAccountMateClient(connStr);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
```

### 5.4 Required integration tests per domain

| Domain | Test | AM interaction | Expected result |
|---|---|---|---|
| Orders | Write-back happy path | Call `sp_ws_syncorder` | Correct rows in `soh`/`sol` |
| Orders | Idempotency (duplicate webhook) | Two calls to `sp_ws_syncorder` with same order | AM state identical to single-call result |
| Orders | Retry on transient failure | Simulate SP timeout, retry | Success on retry; one AM write |
| Pricing | Full campaign price fetch | Call `sp_camp_getSPprice` for sample SKUs | Prices match oracle for known input set |
| Pricing | Boundary sweep | Advance clock past `dFrom`; run sweep | DBM prices updated to new campaign |
| Products | Full sync | Read `icitem` + `iciimgUpdateNOP` | Shopify products reflect AM product catalogue |
| Products | Delta sync (watermark) | Update a product in staging-am; run delta | Only changed products synced; watermark advances |
| Consultants | Delta sync | Update a consultant in staging-am; run delta | Customer metafields updated in Shopify |
| Outbox relay | At-least-once delivery | Commit outbox entry; relay processes it | Shopify API called; entry marked complete |
| Outbox relay | Dead-letter on exhaustion | Force all retries to fail | Entry moved to DLQ; no further retries |

### 5.5 Example: order write-back integration test

```csharp
[Collection("Integration-OrderWriteback")]
[Trait("Category", "Integration")]
[Trait("RequiresAm", "staging")]
public class OrderWritebackIntegrationTests
{
    private readonly AmSnapshotFixture<OrderWritebackBaseline> _amFixture;
    private readonly OrderWritebackService _sut;

    public OrderWritebackIntegrationTests(AmSnapshotFixture<OrderWritebackBaseline> amFixture)
    {
        _amFixture = amFixture;
        _sut = new OrderWritebackService(
            _amFixture.AmClient,
            new SqlOutboxRepository(TestConfiguration.DbmStateConnectionString),
            NullLogger<OrderWritebackService>.Instance);
    }

    [Fact]
    public async Task WriteBack_GivenValidConsultantOrder_WritesExpectedRowsToAM()
    {
        var order = ShopifyOrderFixtures.ValidConsultantOrder(
            consultantNo: "C001",
            skuAmounts: new[] { ("SKU-001", 2, 299.00m), ("SKU-002", 1, 149.00m) });

        var result = await _sut.ProcessAsync(order, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.AmOrderNo.Should().NotBeNullOrEmpty();

        // Query AM to verify the rows were written
        var sosordRows = await _amFixture.AmClient.QueryAsync<dynamic>(
            "SELECT * FROM dbo.sosord WHERE cOrderNo = @no",
            new { no = result.AmOrderNo });

        sosordRows.Should().HaveCount(1);
        ((string)sosordRows[0].cCustNo).Should().Be("C001");
    }

    [Fact]
    public async Task WriteBack_WhenSubmittedTwice_WritesOnlyOneOrderToAM()
    {
        var order = ShopifyOrderFixtures.ValidConsultantOrder(orderId: "shopify-order-99999");

        await _sut.ProcessAsync(order, CancellationToken.None);
        var secondResult = await _sut.ProcessAsync(order, CancellationToken.None);

        secondResult.Success.Should().BeTrue();
        secondResult.WasDuplicate.Should().BeTrue();

        var amOrders = await _amFixture.AmClient.QueryAsync<dynamic>(
            "SELECT COUNT(*) cnt FROM dbo.sosord WHERE cExtOrderNo = @id",
            new { id = "shopify-order-99999" });

        ((int)amOrders[0].cnt).Should().Be(1); // Only one AM order, not two
    }
}
```

### 5.6 Example: pricing integration test

```csharp
[Trait("Category", "Integration")]
[Trait("RequiresAm", "staging")]
public class PricingSyncIntegrationTests : IClassFixture<StagingAmFixture>
{
    [Fact]
    public async Task GetAllCampaignPrices_ReturnsNonEmptyForActiveConsultants()
    {
        var client = new SqlAccountMateClient(TestConfiguration.AmConnectionString);

        var prices = await client.GetAllCampaignPricesAsync(CancellationToken.None);

        prices.Should().NotBeEmpty("staging-am must have at least one active campaign");
        prices.All(p => p.Price >= 0).Should().BeTrue();
        prices.All(p => !string.IsNullOrEmpty(p.ItemNo)).Should().BeTrue();
    }

    [Fact]
    public async Task PriceSyncService_AfterFullSync_AllActivePricesStoredInStateDb()
    {
        var service = new CampaignPriceSyncService(
            new SqlAccountMateClient(TestConfiguration.AmConnectionString),
            new SqlPriceRepository(TestConfiguration.DbmStateConnectionString),
            NullLogger<CampaignPriceSyncService>.Instance);

        await service.RunFullSyncAsync(CancellationToken.None);

        var storedCount = await new SqlPriceRepository(TestConfiguration.DbmStateConnectionString)
            .CountAsync();

        storedCount.Should().BeGreaterThan(0);
    }
}
```

---

## 6. Parity Tests

### 6.1 Architecture

Parity tests compare DBM's output against the authoritative legacy output for the same inputs. See `docs/spec/04_environments.md` §5 for the full parity run lifecycle.

```
Golden Dataset (parity golden snapshot)
  ├── AM DB snapshot (production-derived) → restored to parity-am before run
  ├── Shopify event fixtures (JSON) → submitted to DBM webhook endpoint
  └── Expected output definitions (JSON) → compared against actual AM state

                    ↓ restore

parity-am (golden snapshot)

                    ↓ DBM processes each fixture event

DBM Core (parity environment)

                    ↓ produces

Parity Comparator
  ├── Queries parity-am for actual state
  ├── Compares actual vs expected (field-by-field, with allowlist)
  └── Emits parity report (pass/fail/diff per domain)
```

### 6.2 Parity test project structure

```
tests/
  DBM.Parity.Tests/
    Orders/
      OrderWritebackParityTests.cs
    Pricing/
      PricingOracleParityTests.cs
      PricingBoundaryParityTests.cs
    Products/
      ProductSyncParityTests.cs
    Consultants/
      ConsultantSyncParityTests.cs
    golden/
      orders/
        fixtures/                            # Input: Shopify order payloads
          order_consultant_campaign_01.json
          order_consultant_campaign_02.json
          order_dtc_no_campaign_01.json
          order_exclusive_item_01.json
          order_multiline_mixed_01.json
          order_zero_price_sample_01.json
          order_backorder_01.json
          order_cancelled_01.json
          order_partial_fulfillment_01.json
          order_high_value_01.json
          order_kit_item_01.json
          order_consultant_reactivation_01.json
        expected/                            # Expected: AM table diffs
          order_consultant_campaign_01_am_diff.json
          ...
      products/
        baseline_shopify_snapshot.json       # Current live Shopify product state
      consultants/
        sample_consultant_metafields.json    # Current live Shopify customer metafields
      README.md                             # Documents golden dataset version
```

### 6.3 Parity test: order write-back

```csharp
[Trait("Category", "Parity")]
[Trait("RequiresAm", "parity")]
public class OrderWritebackParityTests : IClassFixture<ParityAmFixture>
{
    private static readonly string FixtureDir =
        Path.Combine(AppContext.BaseDirectory, "golden", "orders");

    [Theory]
    [MemberData(nameof(GetOrderFixtures))]
    public async Task WriteBack_ProducesExpectedAmState(string fixtureName)
    {
        // Arrange: load Shopify order fixture and expected AM diff
        var order = JsonFixture.Load<ShopifyOrder>(
            Path.Combine(FixtureDir, "fixtures", fixtureName + ".json"));
        var expected = JsonFixture.Load<AmTableDiff>(
            Path.Combine(FixtureDir, "expected", fixtureName + "_am_diff.json"));

        // Restore the "before" snapshot specific to this fixture
        await _parityFixture.RestoreNamedSnapshotAsync("order_writeback_baseline");

        // Act: submit order to DBM
        var result = await _orderService.ProcessAsync(order, CancellationToken.None);

        // Assert: query parity-am and compare to expected diff
        var actualDiff = await _parityFixture.CaptureTableDiffAsync(
            result.AmOrderNo, tables: new[] { "sosord", "sol", "arcash" });

        ParityAssert.DiffsEqual(expected, actualDiff, allowlist: ParityAllowlist.Timestamps);
    }

    public static IEnumerable<object[]> GetOrderFixtures() =>
        Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "golden", "orders", "fixtures"))
            .Select(f => new object[] { Path.GetFileNameWithoutExtension(f) });
}
```

### 6.4 Parity test: pricing oracle

The pricing oracle parity test is a live comparison — it does not use pre-recorded expected output. Instead, it calls `sp_camp_getSPprice` directly and compares the result to DBM's precomputed price.

```csharp
[Trait("Category", "Parity")]
[Trait("RequiresAm", "parity")]
public class PricingOracleParityTests : IClassFixture<ParityAmFixture>
{
    [Fact]
    public async Task AllActiveSkuPrices_MatchOracleOutput()
    {
        // 1. Get oracle prices by calling sp_camp_getSPprice for all active SKUs
        var oraclePrices = await _amClient.GetAllCampaignPricesAsync(CancellationToken.None);

        // 2. Run DBM's pricing sync service against parity-am
        await _pricingSyncService.RunFullSyncAsync(CancellationToken.None);

        // 3. Get DBM's stored prices
        var dbmPrices = await _priceRepository.GetAllAsync();

        // 4. Compare — must match for every SKU
        var divergences = new List<string>();
        foreach (var oracle in oraclePrices)
        {
            var dbm = dbmPrices.SingleOrDefault(p =>
                p.ItemNo == oracle.ItemNo && p.ConsultantNo == oracle.ConsultantNo);

            if (dbm == null)
            {
                divergences.Add($"Missing DBM price: {oracle.ItemNo}/{oracle.ConsultantNo}");
                continue;
            }
            if (dbm.Price != oracle.Price)
            {
                divergences.Add($"Price mismatch for {oracle.ItemNo}/{oracle.ConsultantNo}: " +
                                $"oracle={oracle.Price} dbm={dbm.Price}");
            }
        }

        divergences.Should().BeEmpty("all DBM prices must match the oracle");
    }

    [Fact]
    public async Task AfterCampaignBoundary_PricesReflectNewCampaign()
    {
        // Advance parity test clock past the next campaign dFrom
        var nextBoundary = await _campaignRepository.GetNextBoundaryAsync();
        TestClock.AdvanceTo(nextBoundary.DFrom.AddSeconds(1));

        // Run boundary sweep
        await _boundarySweepService.RunSweepAsync(CancellationToken.None);

        // Run oracle with advanced clock
        var oracleAfter = await _amClient.GetAllCampaignPricesAsync(CancellationToken.None);
        var dbmAfter = await _priceRepository.GetAllAsync();

        // All prices must still match oracle after boundary
        ParityAssert.PricesMatch(oracleAfter, dbmAfter);
    }
}
```

### 6.5 Parity test: product sync

```csharp
[Trait("Category", "Parity")]
[Trait("RequiresAm", "parity")]
public class ProductSyncParityTests : IClassFixture<ParityAmFixture>
{
    [Fact]
    public async Task AfterFullSync_ShopifyProductsMatchProductionBaseline()
    {
        // Arrange: load the golden baseline snapshot of the live Shopify product catalogue
        var baseline = JsonFixture.Load<ShopifyProductSnapshot>(
            "golden/products/baseline_shopify_snapshot.json");

        // Act: run DBM full product sync against parity-am
        await _productSyncService.RunFullSyncAsync(CancellationToken.None);

        // Assert: compare DBM-produced Shopify products against baseline
        var actual = await _parityShopifyClient.GetAllProductsAsync();
        var result = ParityComparator.CompareProducts(baseline.Products, actual,
            allowlistFields: new[] { "updatedAt", "createdAt" });

        result.KnownImprovements.Should().OnlyContain(i => i.IsDocumented,
            "all intentional divergences must be documented");
        result.UnexpectedDivergences.Should().BeEmpty();
    }
}
```

### 6.6 Parity report schema

Each parity run produces a structured JSON report written to `PARITY_REPORT_OUTPUT`:

```json
{
  "runId": "parity-2026-04-07T02-00-00Z",
  "timestamp": "2026-04-07T02:34:12Z",
  "goldenSnapshotDate": "2026-04-07",
  "gitSha": "abc123def456",
  "domains": {
    "orders": {
      "fixturesTested": 12,
      "passed": 12,
      "failed": 0,
      "divergences": []
    },
    "pricing": {
      "skusTested": 135,
      "passed": 135,
      "failed": 0,
      "campaignBoundaryTested": true,
      "divergences": []
    },
    "products": {
      "productsTested": 135,
      "passed": 133,
      "failed": 0,
      "knownImprovements": [
        {
          "productId": "SKU-042",
          "field": "status",
          "legacy": "active",
          "dbm": "draft",
          "reason": "PRD §4.2 — DBM publishes discontinued SKUs as draft instead of active-but-out-of-stock"
        }
      ]
    },
    "consultants": {
      "consultantsTested": 25,
      "passed": 25,
      "failed": 0,
      "divergences": []
    }
  },
  "overallStatus": "CLEAN"
}
```

`overallStatus` is `"CLEAN"` only if: no `failed > 0` in any domain AND no undocumented divergences.

### 6.7 Production deployment gate check

```python
# scripts/ci/check-parity-report.py
# Reads the latest parity report from Azure Blob and validates:
# 1. Report is less than max-age-days old
# 2. overallStatus == "CLEAN"
# Exits non-zero if either check fails.

import json, sys, urllib.request
from datetime import datetime, timezone, timedelta

def main():
    # ... download latest report from Azure Blob ...
    report = json.loads(download_latest_report())

    report_ts = datetime.fromisoformat(report["timestamp"])
    age_days = (datetime.now(timezone.utc) - report_ts).days
    if age_days > MAX_AGE_DAYS:
        print(f"FAIL: Parity report is {age_days} days old (max {MAX_AGE_DAYS})")
        sys.exit(1)

    if report["overallStatus"] != "CLEAN":
        print(f"FAIL: Parity report status is {report['overallStatus']}")
        for domain, result in report["domains"].items():
            if result.get("failed", 0) > 0:
                print(f"  {domain}: {result['failed']} failed")
        sys.exit(1)

    print(f"PASS: Parity report clean ({age_days} days old)")
```

---

## 7. E2E Tests

### 7.1 Framework and scope

**UI E2E:** Playwright — tests the Admin Console in a real browser against the staging environment.

**API E2E:** Custom HTTP test client — tests the full webhook → processing → AM write-back → Shopify update round-trip against staging.

E2E tests are slow (minutes per test) and require the full staging environment to be healthy. They are:

- Not run on every PR
- Run on-demand (before go-live, before major releases)
- Required to be fully clean before production cutover is approved

### 7.2 Critical E2E flows

| Flow | Description | Pass condition |
|---|---|---|
| Order round-trip | Shopify order created in staging store → webhook received by DBM → AM write-back succeeds → fulfillment event synced back → Shopify order marked fulfilled | AM has the expected order rows; Shopify order status updated |
| Pricing sweep | Campaign boundary approached in staging → DBM boundary sweep fires → Shopify product metafields updated to new campaign prices | Shopify metafields match `sp_camp_getSPprice` oracle for new campaign |
| Admin console — campaign management | Operator creates a new campaign via Admin Console → pricing sync fires → Shopify metafields updated | New campaign visible in Shopify; prices correct |
| Admin console — DLQ replay | Operator views dead-letter queue → clicks replay on a failed message → message reprocessed successfully | Item reprocessed; DLQ entry removed; no duplicate AM write |
| Consultant onboarding | New consultant registered in AM → delta sync runs → Shopify customer created with correct metafields | Customer visible in Shopify; metafields match AM source |
| Product catalogue sync | Product data updated in AM → delta sync runs → Shopify product updated | Shopify product reflects AM changes within sync cycle |

### 7.3 Playwright test structure

```
tests/
  DBM.E2E.Tests/
    AdminConsole/
      CampaignManagement.spec.ts
      DeadLetterQueue.spec.ts
      ConsultantView.spec.ts
    playwright.config.ts
```

```typescript
// tests/DBM.E2E.Tests/AdminConsole/CampaignManagement.spec.ts
import { test, expect } from '@playwright/test';

test('operator can create a campaign and see it reflected in pricing', async ({ page }) => {
  await page.goto(process.env.ADMIN_CONSOLE_URL!);

  // Log in
  await page.fill('[data-testid="email"]', process.env.ADMIN_USER!);
  await page.fill('[data-testid="password"]', process.env.ADMIN_PASSWORD!);
  await page.click('[data-testid="login-btn"]');

  // Navigate to campaigns
  await page.click('[data-testid="nav-campaigns"]');
  await page.click('[data-testid="new-campaign-btn"]');

  // Fill campaign details
  await page.fill('[data-testid="campaign-name"]', 'E2E Test Campaign');
  await page.fill('[data-testid="campaign-from"]', '2026-05-01');
  await page.fill('[data-testid="campaign-to"]', '2026-05-31');
  await page.click('[data-testid="save-campaign-btn"]');

  // Verify campaign appears in list
  await expect(page.locator('[data-testid="campaign-list"]'))
    .toContainText('E2E Test Campaign');
});
```

---

## 8. Test Data Management

### 8.1 Fixture naming conventions

All test fixture files follow a consistent naming convention:

```
{domain}_{scenario}_{variant}_{sequence}.json

Examples:
  order_consultant_campaign_01.json    → consultant order with campaign pricing, first variant
  order_dtc_no_campaign_01.json        → DTC order without campaign pricing
  price_response_sku_ABC_tier_gold.json → pricing SP response for SKU ABC, gold tier
  customer_consultant_active_01.json   → active consultant customer record
```

### 8.2 AM diff format

Expected AM diffs (parity golden dataset) use a standardised format:

```json
{
  "fixture": "order_consultant_campaign_01",
  "description": "Consultant order with campaign pricing — R349 line total",
  "baseline_snapshot": "order_writeback_baseline",
  "tables": {
    "sosord": {
      "inserted": [
        {
          "cOrderNo": "{{result.AmOrderNo}}",
          "cCustNo": "C001",
          "nAmtOrd": 349.00,
          "cStatus": "O"
        }
      ],
      "updated": [],
      "deleted": []
    },
    "sol": {
      "inserted": [
        {
          "cOrderNo": "{{result.AmOrderNo}}",
          "cItemNo": "SKU-001",
          "nQtyOrd": 1,
          "nUnitPrice": 349.00
        }
      ]
    }
  },
  "allowlist": ["dtStamp", "dtMod", "nSeq"],
  "known_improvements": []
}
```

Placeholders (`{{result.AmOrderNo}}`) are resolved at comparison time using the actual SP output.

### 8.3 Golden dataset update process

When a new golden snapshot is needed (see `docs/spec/04_environments.md` §5.4):

1. Capture new AM snapshot from production via `take-golden-snapshot.ps1`.
2. Re-run existing parity tests against the new snapshot. Most should still pass.
3. For any test that fails: investigate whether the difference is:
   - A **legitimate AM data change** (e.g. new campaign structure) → update the expected diff JSON.
   - A **DBM regression** → fix DBM, do not update the expected diff.
4. Capture updated Shopify baseline via `take-golden-snapshot.ps1 --capture-shopify-baseline`.
5. Update `GOLDEN_SNAPSHOT_DATE` in `.github/workflows/parity.yml`.
6. PR the updated golden dataset with a comment explaining what changed and why.

Golden dataset changes require the same review process as code changes — they represent the behavioural contract.

---

## 9. Coverage and Quality Gates

### 9.1 Coverage configuration

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutputFormat>cobertura</CoverletOutputFormat>
  <CoverletOutput>./TestResults/coverage.cobertura.xml</CoverletOutput>
  <Exclude>[*.Tests]*,[*]*.Migrations.*,[*]*.Fixtures.*</Exclude>
</PropertyGroup>
```

### 9.2 CI coverage reporting

```yaml
# In ci.yml — after unit tests job
- name: Generate coverage report
  run: |
    dotnet tool install -g dotnet-reportgenerator-globaltool
    reportgenerator \
      -reports:"**/*.cobertura.xml" \
      -targetdir:coverage-report \
      -reporttypes:HtmlInline_AzurePipelines,Cobertura
- uses: actions/upload-artifact@v4
  with:
    name: coverage-report
    path: coverage-report/

- name: Check coverage thresholds
  run: |
    python scripts/ci/check-coverage.py \
      --cobertura coverage-report/Cobertura.xml \
      --domain-threshold 80 \
      --platform-threshold 70
```

### 9.3 Quality gates summary

| Gate | Threshold | Enforced when |
|---|---|---|
| Unit tests pass | 100% pass rate | Every PR and main |
| Build succeeds | 0 errors | Every PR and main |
| Lint / format | 0 violations | Every PR and main |
| Domain service coverage | ≥ 80% line | Advisory now; hard gate at go-live |
| Platform service coverage | ≥ 70% line | Advisory now; hard gate at go-live |
| Integration tests pass | 100% pass rate | On merge to main |
| Parity report clean | 0 failed, 0 undocumented divergences | Before any production deployment |
| E2E tests clean | 100% pass rate | Before production cutover |
| Human approval | 2 approvers | Before production deployment |

---

## 10. Domain-Specific Testing Notes

### 10.1 Orders — write-back

**Key scenarios requiring parity fixtures:**

| Scenario | Why it's a risk |
|---|---|
| Consultant order with campaign pricing | Campaign price vs regular price selection logic |
| DTC order without campaign | Different SP call path |
| Order with exclusive items | Entitlement check before AM write |
| Kit/bundle item | Multi-line expansion logic |
| Zero-price sample item | Edge case: `nUnitPrice = 0` handling |
| Order with backorder flag | `cStatus` field behaviour |
| High-value order (> R10,000) | No rounding issues at high totals |
| Order cancellation | ⚠️ TC-05 open — add after SP confirmed |

### 10.2 Pricing — campaign oracle

**Key scenarios:**

| Scenario | Why it's a risk |
|---|---|
| Multiple overlapping campaigns for same SKU | Priority resolution logic |
| Campaign with zero consultant price | Edge: `nSPprice = 0` handling |
| Campaign boundary at midnight SAST | Timezone handling — AM stores in SAST, DBM uses UTC |
| SKU not in any campaign | Should return regular price, not null |
| Consultant tier affects price | Gold vs Silver vs Bronze tier price differences |

**Timezone note:** AM stores campaign dates in SAST (UTC+2). DBM uses UTC internally. The boundary sweep must convert correctly. This is the single highest-risk timezone edge case. Unit tests must cover the 22:00 UTC / 00:00 SAST transition explicitly.

### 10.3 Products — sync parity

**Known intentional divergences to document:**

- Discontinued SKUs: legacy keeps active with zero stock; DBM sets status to `draft`.
- Product descriptions: DBM may normalise HTML encoding differences.
- Variant ordering: DBM may order variants differently; comparison is by variant identifier, not position.

### 10.4 Consultants — sync parity

**Key scenarios:**

| Scenario | Why it's a risk |
|---|---|
| Active consultant, standard tier | Baseline case |
| Consultant with exclusive item access | `exclusive_access` metafield |
| Consultant with deactivated account | Status must reflect correctly in Shopify |
| Consultant with downline relationship | `downline_count` metafield |
| New consultant (first sync) | Customer must be created, not updated |

### 10.5 Communications (non-OTP)

Email dispatch integration tests require a staging email sink (not a real mailbox). Use [MailHog](https://github.com/mailhog/MailHog) or a staging Postmark/SendGrid account with test mode.

**OTP testing:** Blocked on TC-04. Add after OTP delivery model is confirmed.

---

## Evidence Base

| Artefact | Location |
|---|---|
| Infrastructure specification | `docs/spec/03_infrastructure.md` |
| Environment architecture | `docs/spec/04_environments.md` |
| Architecture specification | `docs/spec/01_dbm_architecture.md` |
| PRD — phase 1 scope and NFRs | `docs/spec/00_prd.md` |
| Data access and SQL contract | `docs/analysis/17_data_access_and_sql_contract_crosscut.md` |
| Auditability and idempotency cross-cut | `docs/analysis/18_auditability_idempotency_and_reconciliation_crosscut.md` |
| Pricing engine deep dive | `docs/analysis/20_pricing_engine_deep_dive.md` |
| DBM–AM interface contract | `docs/analysis/24_dbm_am_interface_contract.md` |
